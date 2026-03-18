using Serilog;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using ViennaDotNet.Common;
using ViennaDotNet.Launcher.Utils;

namespace ViennaDotNet.Launcher;

internal static class AutoUpdater
{
    const string repoOwner = "BitcoderCZ";
    const string repoName = "ViennaDotNet";

    static Version currentVersion => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

    public static async Task CheckAndUpdate()
    {
        using HttpClient client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(3),
        };

        Log.Information($"Current version: {currentVersion}");
        Log.Information("Getting new version");

        JsonObject? releaseInfo;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest");
            request.Headers.Add("User-Agent", "request");

            using var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error($"Bad response status: {response.StatusCode}");
                U.PAK();
                return;
            }

            releaseInfo = Json.Deserialize<JsonObject>(await response.Content.ReadAsStringAsync());

            if (releaseInfo is null)
            {
                Log.Error("Invalid response (null)");
                U.PAK();
                return;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error while getting new version: {ex}");
            U.PAK();
            return;
        }

        string? tagName;
        if (!releaseInfo.TryGetPropertyValue("tag_name", out var tagNameToken) || tagNameToken!.GetValueKind() is not JsonValueKind.String || string.IsNullOrEmpty(tagName = tagNameToken.GetValue<string>()))
        {
            Log.Error("Invalid response");
            U.PAK();
            return;
        }

        tagName = tagName.Trim();
        if (tagName.StartsWith('v'))
        {
            tagName = tagName[1..];
        }

        Version? newVersion;
        if (!Version.TryParse(tagName, out newVersion))
        {
            Log.Error($"New version string is invalid: {tagName}");
            U.PAK();
            return;
        }

        if (newVersion <= currentVersion)
        {
            Log.Information("Current version is up to date");
            return;
        }
        else
            Log.Information($"Newer version detected: {newVersion}");

        try
        {
            JsonArray? assets = releaseInfo["assets"] as JsonArray;
            if (assets is null)
            {
                Log.Error("Invalid response");
                U.PAK();
                return;
            }

            string os;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    os = "win";
                    break;
                case PlatformID.Unix:
                    os = "unix";
                    break;
                default:
                    Log.Error($"You os is unsupported (somehow): {Environment.OSVersion.Platform}");
                    U.PAK();
                    return;
            }

            string architecture = RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm or Architecture.Armv6 => "arm",
                Architecture.Arm64 => "arm64",
                _ => Environment.Is64BitOperatingSystem ? "x64" : "x86",
            };
            string lookFor = (os + "-" + architecture).ToLowerInvariant();

            JsonObject? compatibleAsset = null;
            foreach (var assetToken in assets)
            {
                var asset = assetToken as JsonObject;

                string? name;
                if (asset is null || !asset.TryGetPropertyValue("name", out var nameToken) || nameToken!.GetValueKind() != JsonValueKind.String || string.IsNullOrEmpty(name = nameToken.GetValue<string>()))
                {
                    continue;
                }

                if (name.Contains(lookFor, StringComparison.OrdinalIgnoreCase) && asset.ContainsKey("browser_download_url"))
                {
                    compatibleAsset = asset;
                    break;
                }
            }

            if (compatibleAsset is null)
            {
                Log.Error($"No compatible file found in release for: {lookFor}");
                U.PAK();
                return;
            }

            Log.Information("Downloading new release");

            using var request = new HttpRequestMessage(HttpMethod.Get, compatibleAsset["browser_download_url"]!.GetValue<string>());
            request.Headers.Add("User-Agent", "request");

            using var response = await client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error($"Bad response status: {response.StatusCode}");
                U.PAK();
                return;
            }

            Log.Information("Extracting...");

            using (ZipArchive archive = new ZipArchive(await response.Content.ReadAsStreamAsync(), ZipArchiveMode.Read, false))
                foreach (var entry in archive.Entries)
                    using (Stream stream = entry.Open())
                    using (FileStream fs = File.OpenWrite(entry.FullName))
                        stream.CopyTo(fs);

            Log.Information("Files extracted");

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Console.WriteLine("Press any key to restart the launcher...");
                Console.ReadKey(true);

                Process.Start("CMD.exe", $"timeout /t 2 & start \"\" \"{Assembly.GetExecutingAssembly().Location}\" & exit");
                Environment.Exit(0);
            }
            else
            {
                U.PAK("exit");
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error while downloading new release: {ex}");
            U.PAK();
            return;
        }
    }
}
