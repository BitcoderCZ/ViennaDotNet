# ViennaDotNet
An unofficial port of [Project Vienna](https://github.com/Project-Genoa/Vienna) to .NET

> [!WARNING]
> **Work In Progress (WIP):** This project is currently under active development. Some features may be incomplete, and you may encounter bugs or breaking changes. Use at your own risk!

## New Features
In addition to the original Vienna feature set, this port adds:
- Shop
- Map rendering
- Admin panel

## Setup

- Make sure you have the [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed;
- Run "publish.ps1";
- Go to build/{configuration}/{profile};
- Run "run_launcher.ps1";
- Now on the same device open http://localhost:5000, create an account and login;
- In "Server Options" set "Network/IPv4 Address" to your PC's IP address and either disable "Map/Enable Tile Rendering" or set the "Map/MapTiler API Key" (it can be found [here](https://cloud.maptiler.com/account/keys/) when logged in);
- In "Server Status" click "Start";
- Accept the Minecraft Server's EULA when prompted in the "Launcher" log
- Download and move the re"sourcepack" file as described in the "Launcher" log