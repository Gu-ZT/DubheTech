<div align="center">

<img src="About/ModIcon.png" width="256" height="256" alt="天枢科技图标">

# 天枢科技 DubheTech

**以北斗之首「天枢」为名的 RimWorld 1.6 科技扩展 Mod。**

[English](README.md) | 简体中文

</div>

## 简介

天枢科技（DubheTech）是一个 RimWorld 科技扩展 Mod，为边缘世界带来全新的科技树与工业体系。项目目前正在积极建设中。

- 官网：<https://rimworld.dubhe.dev>
- Package ID：`dubhe.dubhetech`
- 支持游戏版本：**1.6**

## 前置依赖

- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)（必需，需在本 Mod 之前加载）

## 仓库结构

```text
About/       Mod 元数据（About.xml）、图标与预览图
Assemblies/  编译输出目录（DubheTech.dll，已被 git 忽略）
Defs/        XML Def 定义（建筑、物品、研究项目等）
Patches/     针对其他 Mod 的 XML PatchOperation 补丁
Languages/   翻译文件（ChineseSimplified / English 的 Keyed 与 DefInjected）
Textures/    贴图资源
Source/      C# 源码（SDK 风格 csproj，.NET Framework 4.7.2）
```

## 构建

需要任意支持编译 `net472` 的 .NET SDK。项目通过 [Krafs.Rimworld.Ref](https://www.nuget.org/packages/Krafs.Rimworld.Ref) 引用程序集编译，因此**无需**本机安装 RimWorld：

```powershell
dotnet build Source/DubheTech.sln -c Release
```

编译产物 `DubheTech.dll` 会直接输出到 `Assemblies/` 目录。开发调试时，将本仓库符号链接或复制到 RimWorld 的 `Mods` 目录即可在游戏内加载。
