using Serilog.Core;
using ViennaDotNet.Buildplate.Model;
using ViennaDotNet.BuildplateRenderer;

Console.WriteLine("Hello, World!");

WorldData? worldData;

using (var fs = File.OpenRead("test.zip"))
{
    worldData = await WorldData.LoadFromZipAsync(fs, Logger.None);
}

if (worldData is null)
{
    Console.WriteLine("Failed to load world data.");
    return;
}

await MeshGenerator.GenerateAsync(worldData);