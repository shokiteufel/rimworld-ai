# RimWorldBot Test-Suite

xUnit-Tests fuer Production-Module der RimWorldBot-Mod.

## Voraussetzung

**RimWorld muss installiert sein** — Story 1.14 (D-38) hat sowohl Production-csproj als auch
Tests-csproj auf direkte Game-Install-Refs umgestellt. Ohne RimWorld-Install schlaegt der Build
mit Resolve-Fehler fehl (eindeutiges Signal an den Dev: clone-without-game ist nicht supported).

## Default-Pfad

`Directory.Build.props` im Repo-Root setzt `RimWorldManagedDir` als zentrale Property
(geerbt von Source/ und Tests/). Default:
```
D:\SteamLibrary\steamapps\common\RimWorld\RimWorldWin64_Data\Managed
```

## Cross-Dev-Setup (anderer RimWorld-Pfad)

Wenn dein RimWorld-Install nicht unter `D:\SteamLibrary` liegt, ueberschreibe die Property auf
eine der drei Arten. Override greift fuer **beide** Builds (Production + Tests) dank zentraler
Directory.Build.props.

### Option 1: CLI-Override pro Build

```bash
# Tests:
dotnet test Tests/RimWorldBot.Tests.csproj \
  -p:RimWorldManagedDir="C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed"

# Production:
dotnet build Source/RimWorldBot.csproj -c Release \
  -p:RimWorldManagedDir="C:\Path\To\Custom\Managed"
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

### Option 3: Lokales `Directory.Build.props.user` (NICHT committen)

Erstelle eine lokale `Directory.Build.props.user` neben dem Repo-`Directory.Build.props`,
adde sie in deine `.gitignore`. Aktuell wird das Pattern noch nicht automatisch importiert —
fuer einmaliges Setup bleibt CLI-Override (Option 1) oder Env-Var (Option 2) die einfachste Wahl.

## Tests laufen lassen

```bash
cd "d:/SteamLibrary/steamapps/common/RimWorld/Mods/RimWorld Bot/Tests"
dotnet test --logger "console;verbosity=normal"
```

## Aktuelle Coverage (Stand Story 1.14)

68 Tests gruen, 0 Skip:

- `Tests/Data/SchemaRegistryTests.cs` — 9 Tests (Drift-Detection via InternalsVisibleTo,
  Bumps-Chain, Spec-Locks fuer D-36)
- `Tests/Data/RecentDecisionsBufferTests.cs` — 7 Tests (Add, Auto-Pin, FIFO-Cap)
- `Tests/Decision/PlanArbiterTests.cs` — 15 Tests (Layer-Praezedenz aller 4 Arbitrate-Methoden,
  Same-Layer-Conflict-Logging via DecisionLog)
- `Tests/Events/BoundedEventQueueTests.cs` — 8 Tests (Critical/Normal-Queue, Staleness, Overflow)
- `Tests/Events/QuestManagerPollerTests.cs` — 9 Tests (Diff-Detection, null-Edge-Cases via
  Test-Seams QuestSource + TickProvider)
- `Tests/Core/BotSafeTests.cs` — 14 Tests (SafeTick/SafeApply/Sliding-Window/Poison-Cooldown
  via NowProvider-Mock-Clock)
- `Tests/Meta/TestFrameworkMetaTest.cs` — 6 Cases (3 Facts + 1 Theory mit 3 InlineData:
  Discovery, ImmutableCollections-Production-Roundtrip)

Summe: 9+7+15+8+9+14+6 = 68.

Deferred zu Story 2.1 (FakeSnapshotProvider, TestSnapshotBuilder, MockResolvers):
ISnapshotProvider + Snapshot-Records werden erst in 2.1 definiert.

## Hintergrund: Build-Pipeline-Refactor (Story 1.14, D-38)

Production-csproj wurde von `Krafs.Rimworld.Ref`-NuGet-Stubs auf direkte Game-Install-Refs
umgestellt. Begruendung:
- **Krafs.Rimworld.Ref** liefert Reference-Assemblies mit `[ReferenceAssembly]`-Attribute,
  die .NET-Runtime nicht laden kann (`BadImageFormatException`).
- Krafs lieferte zudem eine eigene Mono-style mscorlib-Stub. Production-Code wurde damit
  gegen Mono-Type-Identity gebaut, xUnit-Runner laeuft aber unter Microsoft .NET 4.7.2 mit
  Microsoft-mscorlib → identische Type-Namen, unterschiedliche Type-Identity →
  `TypeLoadException` beim Laden von `Queue<T>`/`Dictionary<T>`/`HashSet<T>` aus der Production-DLL.

Mit direkten Game-Install-Refs nutzt Production-Code Microsoft net472 mscorlib (via implicit
Standard-Refs), Test-Runtime laed dieselben Types → kein Mismatch mehr.

Tradeoff: Build erfordert RimWorld-Install. Akzeptabel weil RimWorld-Mod-Entwickler ohnehin
das Game brauchen (Game-Tests, Steam-Workshop-Upload via In-Game Dev-Mode).
