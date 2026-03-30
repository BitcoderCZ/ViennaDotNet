using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ViennaDotNet.Launcher.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();