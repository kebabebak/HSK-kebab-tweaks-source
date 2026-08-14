# HSKKebabTweaks — build kit

Files to compile `HSKKebabTweaks.dll` for RimWorld HSK 1.5 / 1.6.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) (builds `net48`)
- Harmony and RimWorld refs from NuGet (`Lib.Harmony`, `Krafs.Rimworld.Ref`)
- Place `Core_SK.dll` at `libs\Core_SK.dll` (from Core SK Assemblies)
- For 1.5 builds only: also place `libs\HaulToBuilding.dll` and `libs\Mending.dll` (Take From Mending)

## Build

```powershell
dotnet build HSKKebabTweaks.csproj -c Release
```

For RimWorld 1.6:

```powershell
dotnet build HSKKebabTweaks.csproj -c Release16
```

Output: `out\HSKKebabTweaks.dll`
