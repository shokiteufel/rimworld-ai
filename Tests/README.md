# RimWorldBot Test-Suite

xUnit-Tests fuer pure-Logic-Module der RimWorldBot-Mod.

## Voraussetzung

**RimWorld muss installiert sein** — die Test-Assembly referenziert `Assembly-CSharp.dll` + `UnityEngine.*.dll` direkt aus dem Game-Install (nicht via Krafs.Rimworld.Ref-NuGet-Stubs, weil die Reference-Assemblies sind und xUnit-Runtime sie nicht laden kann; siehe D-37).

## Default-Pfad

Der Default `RimWorldManagedDir` zeigt auf:
```
D:\SteamLibrary\steamapps\common\RimWorld\RimWorldWin64_Data\Managed
```

## Cross-Dev-Setup (anderer RimWorld-Pfad)

Wenn dein RimWorld-Install nicht unter `D:\SteamLibrary` liegt, ueberschreibe die Property auf eine der drei Arten:

### Option 1: CLI-Override pro Build

```bash
dotnet test Tests/RimWorldBot.Tests.csproj \
  -p:RimWorldManagedDir="C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed"
```

### Option 2: Environment-Variable (persistent)

```bash
# bash (Linux/Mac/WSL/Git-Bash):
export MSBUILD_RIMWORLD_MANAGED_DIR="/path/to/RimWorld/RimWorldMac.app/Contents/Resources/Data/Managed"
dotnet test Tests/

# PowerShell:
$env:MSBUILD_RIMWORLD_MANAGED_DIR = "C:\Games\RimWorld\RimWorldWin64_Data\Managed"
dotnet test Tests
```

### Option 3: `Tests/Directory.Build.props` (NICHT committen)

Erstelle eine lokale `Directory.Build.props` neben dem csproj mit deinem Pfad:
```xml
<Project>
  <PropertyGroup>
    <RimWorldManagedDir>C:\Custom\Path\Managed</RimWorldManagedDir>
  </PropertyGroup>
</Project>
```
und adde sie zur `.gitignore` damit dein Pfad nicht im Repo landet.

## Tests laufen lassen

```bash
cd "d:/SteamLibrary/steamapps/common/RimWorld/Mods/RimWorld Bot/Tests"
dotnet test --logger "console;verbosity=normal"
```

## Aktuelle Coverage

Story 1.13 deliver-Stand (siehe Story-File + D-37):
- `Tests/Data/SchemaRegistryTests.cs` — Drift-Detection + Edge-Cases + Spec-Locks (Carry-Over Story 1.9)
- `Tests/Decision/PlanArbiterTests.cs` — Layer-Praezedenz + Conflict-Resolution + AssignedPositions/FocusedFire (Carry-Over Story 1.11; 2 Tests Skip-deferred zu 1.14)
- `Tests/Meta/TestFrameworkMetaTest.cs` — Discovery + Theory + ImmutableCollections-Smoke

Deferred zu Story 1.14 (Test-Runtime-Infrastructure-Refactor):
- BotSafeTests (Carry-Over Story 1.10)
- QuestManagerPollerTests (Carry-Over Story 1.12)
- BoundedEventQueueTests
- RecentDecisionsBufferTests
- Same-Layer-Conflict-Logging-Tests in PlanArbiter (markiert als `[Fact(Skip="...")]`)

Deferred zu Story 2.1 (Map-Cell-Data-Basic-Scan):
- FakeSnapshotProvider + TestSnapshotBuilder + MockResolvers (depend on `ISnapshotProvider`)

## Hintergrund: Warum Game-Install statt NuGet-Stubs?

`Krafs.Rimworld.Ref` (das von Production verwendet wird) liefert Reference-Assemblies (`[ReferenceAssembly]`-Attribute). .NET-Runtime weigert sich solche zur Ausfuehrung zu laden — nur ReflectionOnly-Context. Tests brauchen echte DLLs mit Method-Bodies → wir nehmen sie aus dem Game-Install. Type-Identity matcht weil FullName-Identity wichtiger ist als Assembly-Sources fuer die meisten Verse-Types.

Das mscorlib-Type-Identity-Problem (xUnit-Microsoft-mscorlib vs. Production-Krafs-Mono-mscorlib) ist Topic von Story 1.14.
