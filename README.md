# Mm-Saver (MiMieSaver)

纯 C# 存档内核 + Unity UPM 包 双形态仓库

## 结构

```
proto/                 protoc 源文件
src/MiMieSaver.Core/   netstandard2.1 核心 可在 Cursor 里 dotnet test
src/MiMieSaver.Schema/ Protobuf 生成代码
tests/                 xUnit
unity/                 Unity Package Manager 子目录
```

## Unity 安装

在 `Packages/manifest.json` 添加

```json
"com.hakisheep.mm-saver": "git@github.com:Haki-sheep/MmCSharp-Saver.git?path=unity"
```

`unity/` 目录下所有资源必须提交对应 `.meta` 文件 否则 UPM 导入后会报 immutable folder ignored

Protobuf 与 Newtonsoft 依赖使用宿主项目 Plugins 中的 DLL

`MiMieSaver.asmdef` 通过 `precompiledReferences` 引用 `Google.Protobuf.dll` 与 `Newtonsoft.Json.dll`

宿主项目需具备其一

- `Assets/.../Plugins/Protobuf/Google.Protobuf.dll`
- 官方 `com.unity.nuget.newtonsoft-json`
或在 MieMie 模块中枢里对「存档系统」点 **导入**

HTTPS 备选

```json
"com.hakisheep.mm-saver": "https://github.com/Haki-sheep/Mm-Saver.git?path=unity"
```

## 纯 .NET 开发

```bash
dotnet build
dotnet test
```

## 命名空间

- `MiMieSaver` — ArchiveMgr IArchiveMgr IArchiveModule 等
- `Game.Save` — Protobuf SaveData
