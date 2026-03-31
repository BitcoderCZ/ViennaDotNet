using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ViennaDotNet.Launcher.Client;

// https://github.com/HomagGroup/Blazor3D
var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();