# Arves100's Every File Explorer Fork

<img src="https://i.imgur.com/TpcRlb3.png" height="256px" align="right"/>

Hello, this is a fork of [Every File Explorer by Gericom](https://github.com/Gericom/EveryFileExplorer).

Every File Explorer is a dynamic tool to browse, edit, view, explore and modify files. It is plugin-based, so you can make your own plugins for it aswell!

## Changes
- Migrated from Tao to OpenTK using NuGet. No need to carry the Tao DLLs anymore. :stuck_out_tongue:
- Builded with Visual Studio 2019

## Next work
- Migrate to Avalonia with .NET Core (See branch dotnetcore)
- Add other compressions (like LZO, ZLib)
- Fix the warnings

## Stuff required to test
- Other 3D rendering with the exception of the NDS that was already tested by me.

## Building
Just open the project in Visual Studio, and build it. You may need to specify the path to the 2 dlls in the Libraries directory. Afterwards, copy them in the Plugins directory aswell. Don't forget to unblock it from external sources!

## Provided Plugins
* 3DS
* Common Compressors
* Common Files
* Lego Pirates of the Caribbean
* Mario Kart
* NDS
