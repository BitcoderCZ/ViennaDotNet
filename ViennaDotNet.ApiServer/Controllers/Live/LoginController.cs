using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using ViennaDotNet.ApiServer.Models;
using ViennaDotNet.ApiServer.Utils;
using ViennaDotNet.Common.Utils;

namespace ViennaDotNet.ApiServer.Controllers.Live;

[Route("ppsecure")]
public partial class LoginController : ViennaControllerBase
{
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    private static Config config => Program.config;

    private readonly LiveDbContext _dbContext;

    private static readonly (string, string)[] namespaces =
    [
        ("S", "http://www.w3.org/2003/05/soap-envelope"),
        ("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"),
        ("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"),
        ("wsp", "http://schemas.xmlsoap.org/ws/2004/09/policy"),
        ("wst", "http://schemas.xmlsoap.org/ws/2005/02/trust"),
        ("wssc", "http://schemas.xmlsoap.org/ws/2005/02/sc"),
        ("wsa", "http://www.w3.org/2005/08/addressing"),
        ("ps", "http://schemas.microsoft.com/Passport/SoapServices/PPCRL"),
        ("psf", "http://schemas.microsoft.com/Passport/SoapServices/SOAPFault"),
        ("e", "http://www.w3.org/2001/04/xmlenc#"),
        ("ds", "http://www.w3.org/2000/09/xmldsig#"),
        ("ns1", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"),
    ];

    public LoginController(LiveDbContext context)
    {
        _dbContext = context;
    }

    [HttpGet("InlineConnect.srf")]
    public IActionResult GetLoginPage()
    {
        return Content("""
            <html>
            <head>
                <title>Login</title>
                <script src="/js/jquery-3.6.3.min.js"></script>
            </head>
            <body>
                <h1>Login</h1>
                <p>Please enter the username and password that you want to use and press the button below to generate a user token.</p>
                <form id="loginForm">
                    <input id="username" type="text" /><br />
                    <input id="password" type="password" /><br />
                    <input type="submit" />
                </form>
                <script>
                    $(document).ready(() => {
                    $('#loginForm').submit(() => {
                        $.post('./login', { username: $('#username').val(), password: $('#password').val() }).then(response => {
                        external.Property('DAToken', `<EncryptedData xmlns=\"http://www.w3.org/2001/04/xmlenc#\" Id=\"BinaryDAToken0\"><CipherData><CipherValue>${response.token}</CipherValue></CipherData></EncryptedData>`)
                        external.Property('DAStartTime', response.tokenIssuedAt)
                        external.Property('DAExpires', response.tokenExpires)
                        external.Property('DASessionKey', response.sessionKey)
                        external.Property('FirstName', response.username)
                        external.Property('LastName', response.username)
                        external.Property('SigninName', response.username)
                        external.Property('Username', response.username)
                        external.Property('CID', response.userId)
                        external.Property('PUID', response.userId)
                        external.FinalNext()
                        })

                        return false
                    })
                    })
                </script>
            </body>
            </html>
            """, "text/html");
    }

    private sealed record LoginResponse(
        string UserId,
        string Username,
        string Token,
        string TokenIssuedAt,
        string TokenExpires,
        string SessionKey
    );

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password, CancellationToken cancellationToken)
    {
        username = username.Trim();

        Log.Debug($"Login attempt: Username: {username}, Password: {password}");

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 16)
        {
            return BadRequest("Username must be 3-16 characters long");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4 || password.Length > 32)
        {
            return BadRequest("Password must be 4-32 characters long");
        }

        var account = _dbContext.Accounts
            .FirstOrDefault(account => account.Username == username);

        if (account is not null)
        {
            byte[] passwordHash = HashPassword(password, account.PasswordSalt);

            if (!passwordHash.AsSpan().SequenceEqual(account.PasswordHash))
            {
                return Unauthorized("Incorrect password");
            }
        }
        else
        {
            string userId = GenerateUserId(username);

            byte[] passwordSalt = new byte[16];
            _rng.GetBytes(passwordSalt);

            byte[] paswordHash = HashPassword(password, passwordSalt);

            account = new Models.Account()
            {
                Id = userId,
                Username = username,
                PasswordSalt = passwordSalt,
                PasswordHash = paswordHash,
            };

            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync(cancellationToken);

            Log.Information($"Account created: {username} ({userId})");
        }

        var tokenValidity = ValidityDatePair.Create(config.Login.UserTokenValidityMinutes);
        var token = new Tokens.Live.UserToken(
            account.Id,
            account.Username,
            Convert.ToBase64String(account.PasswordSalt),
            Convert.ToBase64String(account.PasswordHash)
        );
        string tokenString = JwtUtils.Sign(token, config.Login.UserTokenSecretBytes, tokenValidity);

        return Json(new LoginResponse(
            account.Id,
            account.Username,
            tokenString,
            tokenValidity.IssuedStr,
            tokenValidity.ExpiresStr,
            config.Login.UserTokenSessionKey
        ));
    }

    [HttpPost("deviceaddcredential.srf")]
    public IActionResult DeviceAddCredential()
        => Content("""
            <DeviceAddResponse Success="true"><success>true</success><puid>0</puid></DeviceAddResponse>
            """);

    [HttpPost("/RST2.srf")]
    public async Task<IActionResult> RST2()
    {
        var cancellationToken = Request.HttpContext.RequestAborted; // can't get from param, because if I do request body is empty

        var request = new XmlDocument();
        string rq;
        try
        {
            rq = await Request.Body.ReadAsString(cancellationToken);
            request.LoadXml(rq);
        }
        catch
        {
            return BadRequest();
        }

        var nsmgr = new XmlNamespaceManager(request.NameTable);
        foreach (var (prefix, uri) in namespaces)
        {
            nsmgr.AddNamespace(prefix, uri);
        }

        if (request.SelectSingleNode("/S:Envelope/S:Body/wst:RequestSecurityToken", nsmgr) is not null)
        {
            // device token request
            string? username = request.SelectSingleNode("/S:Envelope/S:Header/wsse:Security/wsse:UsernameToken/wsse:Username/text()", nsmgr)?.Value;
            string? password = request.SelectSingleNode("/S:Envelope/S:Header/wsse:Security/wsse:UsernameToken/wsse:Password/text()", nsmgr)?.Value;

            string? requestType = request.SelectSingleNode("/S:Envelope/S:Body/wst:RequestSecurityToken/wst:RequestType/text()", nsmgr)?.Value;
            string? requestAppliesTo = request.SelectSingleNode("/S:Envelope/S:Body/wst:RequestSecurityToken/wsp:AppliesTo/wsa:EndpointReference/wsa:Address/text()", nsmgr)?.Value;

            if (requestType is not "http://schemas.xmlsoap.org/ws/2005/02/trust/Issue" || requestAppliesTo is not "http://Passport.NET/tb")
            {
                return BadRequest();
            }

            var headerValidity = ValidityDatePair.Create(config.Login.SoapHeaderValidityMinutes);

            var deviceTokenValidity = ValidityDatePair.Create(config.Login.DeviceTokenValidityMinutes);
            var deviceToken = new Tokens.Live.DeviceToken();
            string deviceTokenString = JwtUtils.Sign(deviceToken, config.Login.DeviceTokenSecretBytes, deviceTokenValidity);

            var response = new XmlDocument();

            var envelope = CreateElement(response, "S", "Envelope");
            envelope.SetAttribute("xmlns:wsse", nsmgr.LookupNamespace("wsse"));
            envelope.SetAttribute("xmlns:wsu", nsmgr.LookupNamespace("wsu"));
            envelope.SetAttribute("xmlns:wsp", nsmgr.LookupNamespace("wsp"));
            envelope.SetAttribute("xmlns:wst", nsmgr.LookupNamespace("wst"));
            envelope.SetAttribute("xmlns:wssc", nsmgr.LookupNamespace("wssc"));
            envelope.SetAttribute("xmlns:wsa", nsmgr.LookupNamespace("wsa"));
            envelope.SetAttribute("xmlns:ps", nsmgr.LookupNamespace("ps"));
            envelope.SetAttribute("xmlns:psf", nsmgr.LookupNamespace("psf"));
            envelope.SetAttribute("xmlns:e", nsmgr.LookupNamespace("e"));
            envelope.SetAttribute("xmlns:ds", nsmgr.LookupNamespace("ds"));

            var header = CreateElement(response, "S", "Header");
            {
                var security = CreateElement(response, "wsse", "Security");
                var timestamp = CreateElement(response, "wsu", "Timestamp");
                timestamp.SetAttribute("wsu:Id", "Timestamp");
                {
                    var created = CreateElement(response, "wsu", "Created");
                    created.InnerText = headerValidity.IssuedStr;
                    timestamp.AppendChild(created);
                    var expires = CreateElement(response, "wsu", "Expires");
                    expires.InnerText = headerValidity.ExpiresStr;
                    timestamp.AppendChild(expires);
                }

                security.AppendChild(timestamp);
                header.AppendChild(security);

                var pp = CreateElement(response, "psf", "pp");
                header.AppendChild(pp);
            }

            envelope.AppendChild(header);

            var body = CreateElement(response, "S", "Body");
            {
                var requestSecurityTokenResponse = CreateElement(response, "wst", "RequestSecurityTokenResponse");
                {
                    var tokenType = CreateElement(response, "wst", "TokenType");
                    tokenType.InnerText = "urn:passport:legacy";
                    requestSecurityTokenResponse.AppendChild(tokenType);

                    var appliesTo = CreateElement(response, "wsp", "AppliesTo");
                    {
                        var endpointReference = CreateElement(response, "wsa", "EndpointReference");
                        {
                            var address = CreateElement(response, "wsa", "Address");
                            address.InnerText = "http://Passport.NET/tb";
                            endpointReference.AppendChild(address);
                        }

                        appliesTo.AppendChild(endpointReference);
                    }

                    requestSecurityTokenResponse.AppendChild(appliesTo);

                    var lifetime = CreateElement(response, "wst", "Lifetime");
                    {
                        var created = CreateElement(response, "wsu", "Created");
                        created.InnerText = deviceTokenValidity.IssuedStr;
                        lifetime.AppendChild(created);

                        var expires = CreateElement(response, "wsu", "Expires");
                        expires.InnerText = deviceTokenValidity.ExpiresStr;
                        lifetime.AppendChild(expires);
                    }

                    requestSecurityTokenResponse.AppendChild(lifetime);

                    var requestedSecurityToken = CreateElement(response, "wst", "RequestedSecurityToken");
                    {
                        /*var encryptedData = CreateElement(response, "e", "EncryptedData");
                        encryptedData.SetAttribute("Id", "BinaryDAToken0");
                        {
                            var cipherData = CreateElement(response, "e", "CipherData");
                            {
                                var cipherValue = CreateElement(response, "e", "CipherValue");
                                cipherValue.InnerText = deviceTokenString;
                                cipherData.AppendChild(cipherValue);
                            }

                            encryptedData.AppendChild(cipherData);
                        }*/
                        var encryptedData = response.CreateElement("EncryptedData");
                        encryptedData.SetAttribute("Id", "BinaryDAToken0");
                        {
                            var cipherData = response.CreateElement("CipherData");
                            {
                                var cipherValue = response.CreateElement("CipherValue");
                                cipherValue.InnerText = deviceTokenString;
                                cipherData.AppendChild(cipherValue);
                            }

                            encryptedData.AppendChild(cipherData);
                        }

                        requestedSecurityToken.AppendChild(encryptedData);
                    }

                    requestSecurityTokenResponse.AppendChild(requestedSecurityToken);

                    var requestedProofToken = CreateElement(response, "wst", "RequestedProofToken");
                    {
                        var binarySecret = CreateElement(response, "wst", "BinarySecret");
                        binarySecret.InnerText = "0000";
                        requestedProofToken.AppendChild(binarySecret);
                    }

                    requestSecurityTokenResponse.AppendChild(requestedProofToken);
                }

                body.AppendChild(requestSecurityTokenResponse);
            }

            envelope.AppendChild(body);

            response.AppendChild(envelope);

            return Content("""
                <?xml version="1.0" encoding="UTF-8"?>

                """ + response.OuterXml);
        }
        else if (request.SelectSingleNode("/S:Envelope/S:Body/ps:RequestMultipleSecurityTokens", nsmgr) is not null)
        {
            // user token request (user token + device token -> next user token + next session key + xbox token)

            string? userTokenString = request.SelectSingleNode("/S:Envelope/S:Header/wsse:Security/e:EncryptedData[@Id='BinaryDAToken0']/e:CipherData/e:CipherValue", nsmgr)?.InnerText;
            string? deviceDATokenString = request.SelectSingleNode("/S:Envelope/S:Header/wsse:Security/wsse:BinarySecurityToken[@Id='DeviceDAToken']", nsmgr)?.InnerText;

            string? deviceDATokenXMLStringEncoded = null;
            if (!string.IsNullOrEmpty(deviceDATokenString))
            {
                var match = GetDeviceDATokenStringRegex().Match(deviceDATokenString);
                if (match.Success && match.Groups.Count > 1)
                {
                    deviceDATokenXMLStringEncoded = match.Groups[1].Value;
                }
            }

            string? deviceDATokenXMLString = HttpUtility.UrlDecode(deviceDATokenXMLStringEncoded);

            string deviceTokenString = string.Empty;
            if (deviceDATokenXMLString is not null)
            {
                var deviceTokenXml = new XmlDocument();
                deviceTokenXml.LoadXml(deviceDATokenXMLString);
                if (deviceTokenXml is not null)
                {
                    deviceTokenString = deviceTokenXml.SelectSingleNode("/EncryptedData/CipherData/CipherValue")?.InnerText ?? string.Empty;
                }
            }

            double requestCount = EvaluateNumber(request, "count(/S:Envelope/S:Body/ps:RequestMultipleSecurityTokens/*)", nsmgr);

            string? requestType1 = request.SelectSingleNode("/S:Envelope/S:Body/ps:RequestMultipleSecurityTokens/wst:RequestSecurityToken[1]/wst:RequestType/text()", nsmgr)?.InnerText;
            string? appliesTo1 = request.SelectSingleNode("/S:Envelope/S:Body/ps:RequestMultipleSecurityTokens/wst:RequestSecurityToken[1]/wsp:AppliesTo/wsa:EndpointReference/wsa:Address/text()", nsmgr)?.InnerText;
            string? requestType2 = request.SelectSingleNode("/S:Envelope/S:Body/ps:RequestMultipleSecurityTokens/wst:RequestSecurityToken[2]/wst:RequestType/text()", nsmgr)?.InnerText;
            string? appliesTo2 = request.SelectSingleNode("/S:Envelope/S:Body/ps:RequestMultipleSecurityTokens/wst:RequestSecurityToken[2]/wsp:AppliesTo/wsa:EndpointReference/wsa:Address/text()", nsmgr)?.InnerText;

            if (requestCount is not 2 || requestType1 is not "http://schemas.xmlsoap.org/ws/2005/02/trust/Issue" || appliesTo1 is not "http://Passport.NET/tb" || requestType2 is not "http://schemas.xmlsoap.org/ws/2005/02/trust/Issue" || appliesTo2 is not "cobrandid=90023&scope=service%3A%3Auser.auth.xboxlive.com%3A%3Ambi_ssl" || userTokenString is null)
            {
                return BadRequest();
            }

            var userToken = JwtUtils.Verify<Tokens.Live.UserToken>(userTokenString, config.Login.UserTokenSecretBytes, allowExpired: true);
            var deviceToken = JwtUtils.Verify<Tokens.Live.DeviceToken>(deviceTokenString, config.Login.DeviceTokenSecretBytes, allowExpired: true);

            if (userToken is null || userToken.Expired is true)
            {
                throw new NotImplementedException();
            }
            else
            {
                var headerValidity = ValidityDatePair.Create(config.Login.SoapHeaderValidityMinutes);
                string nonce = GenerateNonce();

                var nextUserTokenValidity = ValidityDatePair.Create(config.Login.UserTokenValidityMinutes);
                var nextUserToken = userToken.Data;
                string nextUserTokenString = JwtUtils.Sign(nextUserToken, config.Login.UserTokenSecretBytes, nextUserTokenValidity);

                var xboxTokenValidity = ValidityDatePair.Create(config.Login.XboxTokenValidityMinutes);
                var xboxToken = new Tokens.Shared.XboxTicketToken(userToken.Data.UserId, userToken.Data.Username);
                string xboxTokenString = JwtUtils.Sign(xboxToken, config.Login.XboxTokenSecretBytes, xboxTokenValidity);

                string nextSessionKey = config.Login.UserTokenSessionKey;

                var tokenDocument = new XmlDocument();

                var requestSecurityTokenResponseCollection = CreateElement(tokenDocument, "wst", "RequestSecurityTokenResponseCollection");
                {
                    var encryptedData = tokenDocument.CreateElement("EncryptedData");
                    encryptedData.SetAttribute("xmlns", "http://www.w3.org/2001/04/xmlenc#");
                    encryptedData.SetAttribute("Id", "BinaryDAToken0");
                    {
                        var cipherData = tokenDocument.CreateElement("CipherData");
                        {
                            var cipherValue = tokenDocument.CreateElement("CipherValue");
                            cipherValue.InnerText = nextUserTokenString;
                            cipherData.AppendChild(cipherValue);
                        }

                        encryptedData.AppendChild(cipherData);
                    }

                    var binarySecret = CreateElement(tokenDocument, "wst", "BinarySecret");
                    binarySecret.InnerText = nextSessionKey;

                    AddTokenResponse("urn:passport:legacy", "http://Passport.NET/tb",
                         nextUserTokenValidity.IssuedStr, nextUserTokenValidity.ExpiresStr,
                         encryptedData, binarySecret);

                    var binarySecurityToken = CreateElement(tokenDocument, "wsse", "BinarySecurityToken");
                    binarySecurityToken.SetAttribute("Id", "Compact1");
                    binarySecurityToken.InnerText = xboxTokenString;

                    AddTokenResponse("urn:passport:compact", "cobrandid=90023&scope=service%3A%3Auser.auth.xboxlive.com%3A%3Ambi_ssl", xboxTokenValidity.IssuedStr, xboxTokenValidity.ExpiresStr, binarySecurityToken, null);

                    void AddTokenResponse(string tokenType, string address, string issued, string expires, XmlElement securityToken, XmlElement? proofToken)
                    {
                        var requestSecurityTokenResponse = CreateElement(tokenDocument, "wst", "RequestSecurityTokenResponse");
                        {
                            var tokenTypeEle = CreateElement(tokenDocument, "wst", "TokenType");
                            tokenTypeEle.InnerText = tokenType;
                            requestSecurityTokenResponse.AppendChild(tokenTypeEle);

                            var appliesTo = CreateElement(tokenDocument, "wsp", "AppliesTo");
                            {
                                var endpointReference = CreateElement(tokenDocument, "wsa", "EndpointReference");
                                {
                                    var addressEle = CreateElement(tokenDocument, "wsa", "Address");
                                    addressEle.InnerText = address;
                                    endpointReference.AppendChild(addressEle);
                                }

                                appliesTo.AppendChild(endpointReference);
                            }

                            requestSecurityTokenResponse.AppendChild(appliesTo);

                            var lifetime = CreateElement(tokenDocument, "wst", "Lifetime");
                            {
                                var createdEle = CreateElement(tokenDocument, "wsu", "Created");
                                createdEle.InnerText = issued;
                                lifetime.AppendChild(createdEle);

                                var expiresEle = CreateElement(tokenDocument, "wsu", "Expires");
                                expiresEle.InnerText = expires;
                                lifetime.AppendChild(expiresEle);
                            }

                            requestSecurityTokenResponse.AppendChild(lifetime);

                            var requestedSecurityToken = CreateElement(tokenDocument, "wst", "RequestedSecurityToken");
                            requestedSecurityToken.AppendChild(securityToken);

                            requestSecurityTokenResponse.AppendChild(requestedSecurityToken);

                            if (proofToken is not null)
                            {
                                var requestedProofToken = CreateElement(tokenDocument, "wst", "RequestedProofToken");
                                requestedProofToken.AppendChild(proofToken);

                                requestSecurityTokenResponse.AppendChild(requestedProofToken);
                            }
                        }

                        requestSecurityTokenResponseCollection.AppendChild(requestSecurityTokenResponse);
                    }
                }

                tokenDocument.AppendChild(requestSecurityTokenResponseCollection);
                string tokenDocumentString = tokenDocument.OuterXml;

                string tokenDocumentCipherText = DoAESEncryption(config.Login.UserTokenSessionKeyBytes, nonce, tokenDocumentString);

                var response = new XmlDocument();
                var envelope = CreateElement(response, "S", "Envelope");
                {
                    var header = CreateElement(response, "S", "Header");
                    {
                        var security = CreateElement(response, "wsse", "Security");
                        {
                            var timestamp = CreateElement(response, "wsu", "Timestamp");
                            {
                                var created = CreateElement(response, "wsu", "Created");
                                created.InnerText = headerValidity.IssuedStr;
                                timestamp.AppendChild(created);

                                var expires = CreateElement(response, "wsu", "Expires");
                                expires.InnerText = headerValidity.ExpiresStr;
                                timestamp.AppendChild(expires);
                            }

                            security.AppendChild(timestamp);

                            /*var derivedKeyToken = CreateElement(response, "wssc", "DerivedKeyToken");
                            var idAttrib = response.CreateAttribute("ns1", "Id", null);
                            idAttrib.Value = "EncKey";
                            derivedKeyToken.Attributes.Append(idAttrib);
                            derivedKeyToken.SetAttribute("Algorithm", "urn:liveid:SP800-108CTR-HMAC-SHA256");
                            {
                                var nonceEle = CreateElement(response, "wssc", "Nonce");
                                nonceEle.InnerText = nonce;
                                derivedKeyToken.AppendChild(nonceEle);
                            }

                            security.AppendChild(derivedKeyToken);*/
                            {
                                // Create the root element with prefix "wssc" and namespace
                                XmlElement derivedKeyToken = response.CreateElement("wssc", "DerivedKeyToken", "http://schemas.xmlsoap.org/ws/2005/02/sc");

                                // Add xmlns:wssc namespace declaration (necessary for prefix usage)
                                derivedKeyToken.SetAttribute("xmlns:wssc", "http://schemas.xmlsoap.org/ws/2005/02/sc");

                                // Add xmlns:ns1 namespace declaration
                                derivedKeyToken.SetAttribute("xmlns:ns1", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

                                // Add ns1:Id attribute (with prefix)
                                XmlAttribute idAttr = response.CreateAttribute("ns1", "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                                idAttr.Value = "EncKey";
                                derivedKeyToken.Attributes.Append(idAttr);

                                // Add Algorithm attribute (no prefix)
                                derivedKeyToken.SetAttribute("Algorithm", "urn:liveid:SP800-108CTR-HMAC-SHA256");

                                // Create the <wssc:Nonce> element
                                XmlElement nonceEle = response.CreateElement("wssc", "Nonce", "http://schemas.xmlsoap.org/ws/2005/02/sc");
                                nonceEle.InnerText = nonce;

                                // Append Nonce element as child of DerivedKeyToken
                                derivedKeyToken.AppendChild(nonceEle);

                                security.AppendChild(derivedKeyToken);
                            }
                        }

                        header.AppendChild(security);
                    }

                    envelope.AppendChild(header);

                    var body = CreateElement(response, "S", "Body");
                    {
                        var encryptedData = response.CreateElement("EncryptedData");
                        encryptedData.SetAttribute("xmlns", "http://www.w3.org/2001/04/xmlenc#");
                        encryptedData.SetAttribute("Id", "RSTR");
                        encryptedData.SetAttribute("Type", "http://www.w3.org/2001/04/xmlenc#Element");
                        {
                            var encryptionMethod = response.CreateElement("EncryptionMethod");
                            encryptionMethod.SetAttribute("Algorithm", "http://www.w3.org/2001/04/xmlenc#aes256-cbc");
                            encryptedData.AppendChild(encryptionMethod);

                            var keyInfo = response.CreateElement("KeyInfo");
                            keyInfo.SetAttribute("xmlns", "http://www.w3.org/2000/09/xmldsig#");
                            {
                                var securityTokenReference = CreateElement(response, "wsse", "SecurityTokenReference");
                                {
                                    var reference = CreateElement(response, "wsse", "Reference");
                                    reference.SetAttribute("URI", "#EncKey");
                                    securityTokenReference.AppendChild(reference);
                                }

                                keyInfo.AppendChild(securityTokenReference);
                            }

                            encryptedData.AppendChild(keyInfo);

                            var cipherData = response.CreateElement("CipherData");
                            {
                                var cipherValue = response.CreateElement("CipherValue");
                                cipherValue.InnerText = tokenDocumentCipherText;
                                cipherData.AppendChild(cipherValue);
                            }

                            encryptedData.AppendChild(cipherData);
                        }

                        body.AppendChild(encryptedData);
                    }

                    envelope.AppendChild(body);
                }

                response.AppendChild(envelope);

                return Content(response.OuterXml);
                /*
                 * If doesnt work, remove <?xml version="1.0" encoding="UTF-8"?>

                """ + from inner
                 */
            }
        }
        else
        {
            return BadRequest();
        }

        return Ok();

        XmlElement CreateElement(XmlDocument doc, string prefix, string localName)
        {
            return doc.CreateElement(prefix, localName, nsmgr.LookupNamespace(prefix));
        }

        double EvaluateNumber(XmlDocument document, string xpath, XmlNamespaceManager nsmgr)
        {
            var expr = document.CreateNavigator()!.Compile(xpath);
            expr.SetContext(nsmgr);
            object result = document.CreateNavigator()!.Evaluate(expr);
            if (result is double d) return d;
            return 0;
        }
    }

    private static string GenerateNonce()
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(32);

        var bufferSpan = buffer.AsSpan();
        _rng.GetBytes(bufferSpan);
        string base64 = Convert.ToBase64String(bufferSpan);

        ArrayPool<byte>.Shared.Return(buffer);

        return base64;
    }

    private static string GenerateUserId(string username)
    {
        Span<byte> usernameUTF8 = stackalloc byte[51]; //Encoding.UTF8.GetMaxByteCount(16)
        int usernameUTF8Length = Encoding.UTF8.GetBytes(username, usernameUTF8);
        usernameUTF8 = usernameUTF8[..usernameUTF8Length];

        Span<byte> usernameHash = stackalloc byte[16];
        MD5.HashData(usernameUTF8, usernameHash);

        return Convert.ToHexStringLower(usernameHash[..8]);
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        Debug.Assert(password.Length <= 32);

        byte[] passwordUTF8 = Encoding.UTF8.GetBytes(password);

        return Org.BouncyCastle.Crypto.Generators.SCrypt.Generate(passwordUTF8, salt, 16384, 8, 1, 64);
    }

    private static string DoAESEncryption(byte[] sessionKey, string nonceBase64, string plainText)
    {
        byte[] nonce = Convert.FromBase64String(nonceBase64);
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

        byte[]? messageKey;
        using (var hmac = new HMACSHA256(sessionKey))
        {
            int w1= hmac.TransformBlock([0, 0, 0, 1], 0, 4, null, 0);
            byte[] labelBytes = Encoding.UTF8.GetBytes("WS-SecureConversationWS-SecureConversation");
            int w2 = hmac.TransformBlock(labelBytes, 0, labelBytes.Length, null, 0);
            int w3 = hmac.TransformBlock([0], 0, 1, null, 0);
            int w4 = hmac.TransformBlock(nonce, 0, nonce.Length, null, 0);
            byte[] w5 = hmac.TransformFinalBlock([0, 0, 1, 0], 0, 4);

            messageKey = hmac.Hash;
        }

        Debug.Assert(messageKey is not null);

        byte[] iv = new byte[16];
        _rng.GetBytes(iv);

        // Encrypt with AES-256-CBC
        byte[] cipherText;
        using (var aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = messageKey;
            aes.IV = iv;

            using (var encryptor = aes.CreateEncryptor(messageKey, iv))
            {
                byte[] cipherData = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
                cipherText = new byte[iv.Length + cipherData.Length];
                iv.AsSpan().CopyTo(cipherText.AsSpan());
                cipherData.AsSpan().CopyTo(cipherText.AsSpan(iv.Length..));
            }
        }

        return Convert.ToBase64String(cipherText);
    }

    [GeneratedRegex("&da=([^&]*)")]
    private partial Regex GetDeviceDATokenStringRegex();
}
