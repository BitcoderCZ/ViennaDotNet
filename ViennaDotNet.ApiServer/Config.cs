using System.Text;
using System.Text.Json.Serialization;

namespace ViennaDotNet.ApiServer;

public sealed record class Config(Config.LoginR Login)
{
    public static readonly Config Default = new Config
    (
        new LoginR(
            SoapHeaderValidityMinutes: 1,
            UserTokenValidityMinutes: 7 * 24 * 60,
            DeviceTokenValidityMinutes: 1,
            XboxTokenValidityMinutes: 7 * 24 * 60,
            UserTokenSecret: "Mf5HWU566mwFuxxyXa2ACPvZVw9DTfzO4DREWk0aoxfkaVEhM6OfJRQ2MR1FhtPpVgkhEBBBG1PJvjy6LoO90A==",
            DeviceTokenSecret: "2MonNUihCGLzZRhMMkZ6GgFFnxj0Jk60Mvhoa2NVaOW51cDd4ZKD8L5RAbgcO1R9vfs4V/JZE6KmWW16I0OesQ==",
            XboxTokenSecret: "Q/cQFxZs/PahNgsNrvEOUAQ6RQ45MTAaRXH9LNpSrZpjQ99RBmyxuJwOcnkX6daCuVqdo8/eefpe1wUamn9YTA==",
            UserTokenSessionKey: "W1oCtEFI0XJjOW0c3oDJ/kWRR4Q7CSlE"
        )
    );

    public sealed record LoginR(
        int SoapHeaderValidityMinutes,
        int UserTokenValidityMinutes,
        int DeviceTokenValidityMinutes,
        int XboxTokenValidityMinutes,
        string UserTokenSecret,
        string DeviceTokenSecret,
        string XboxTokenSecret,
        string UserTokenSessionKey
    )
    {
        private byte[]? _userTokenSecretBytes;
        private byte[]? _deviceTokenSecretBytes;
        private byte[]? _xboxTokenSecretBytes;
        private byte[]? _userTokenSessionKeyBytes;

        [JsonIgnore]
        public byte[] UserTokenSecretBytes => _userTokenSecretBytes ??= Convert.FromBase64String(UserTokenSecret);

        [JsonIgnore]
        public byte[] DeviceTokenSecretBytes => _deviceTokenSecretBytes ??= Convert.FromBase64String(DeviceTokenSecret);

        [JsonIgnore]
        public byte[] XboxTokenSecretBytes => _xboxTokenSecretBytes ??= Convert.FromBase64String(XboxTokenSecret);

        [JsonIgnore]
        public byte[] UserTokenSessionKeyBytes => _userTokenSessionKeyBytes ??= Convert.FromBase64String(UserTokenSessionKey);
    }
}
