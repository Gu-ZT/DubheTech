<div align="center">

<img src="About/ModIcon.png" width="256" height="256" alt="DubheTech icon">

# 天枢科技 DubheTech

**A technology expansion mod for RimWorld 1.6, named after Dubhe, the first star of the Big Dipper.**

English | [简体中文](README.zh_CN.md)

</div>

## Introduction

DubheTech (天枢科技) is a technology expansion mod for RimWorld. It brings a brand-new tech tree and industrial system to the rimworld. The project is currently under active construction.

- Website: <https://rimworld.dubhe.dev>
- Package ID: `dubhe.dubhetech`
- Supported game version: **1.6**

## Dependencies

- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) (required, load before this mod)

## Repository Layout

```text
About/       Mod metadata (About.xml), icon and preview images
Assemblies/  Build output (DubheTech.dll, git-ignored)
Defs/        XML Defs (buildings, items, research, ...)
Patches/     XML PatchOperations for other mods
Languages/   Translations (ChineseSimplified / English keyed & def-injected)
Textures/    Texture assets
Source/      C# source (SDK-style csproj, .NET Framework 4.7.2)
```

## Build

Requires any .NET SDK capable of building `net472` (the build uses [Krafs.Rimworld.Ref](https://www.nuget.org/packages/Krafs.Rimworld.Ref) reference assemblies, so a local RimWorld installation is **not** needed):

```powershell
dotnet build Source/DubheTech.sln -c Release
```

The compiled `DubheTech.dll` is written directly to `Assemblies/`. To develop in-game, symlink or copy this repository into RimWorld's `Mods` folder.
