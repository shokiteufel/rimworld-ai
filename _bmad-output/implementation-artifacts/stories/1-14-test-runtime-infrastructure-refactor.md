# Story 1.14: Test-Runtime-Infrastructure-Refactor (Cross-Cutting)

**Status:** review
**Epic:** Epic 1 (Cross-Cutting-Infrastruktur)
**Size:** M
**Decisions referenced:** D-37 (Story 1.13 Scope-Cut + 1.14 Begründung), CC-STORIES-13

## Story
Als Mod-Entwickler möchte ich **Production-DLL-Build-Pipeline so refactoren, dass Microsoft net472 mscorlib für Standard-Types (Queue/Dictionary/HashSet/List) verwendet wird** — damit Carry-Over-Tests aus Story 1.10 (BotSafe), 1.12 (QuestManagerPoller) und alle künftigen Tests, die Production-Klassen mit Mono-mscorlib-Type-Use instantiieren, im xUnit-Test-Runner unter Microsoft .NET Framework 4.7.2 laufen können.

## Hintergrund (D-37)
Story 1.13 hat ein strukturelles Test-Runtime-Limit identifiziert: Die `Krafs.Rimworld.Ref`-NuGet-Pkg-built Production-DLL referenziert Mono-style mscorlib (Krafs Stub) für Standard-Collection-Types. xUnit-Test-Runner läuft unter Microsoft .NET Framework 4.7.2 mit Microsoft mscorlib. Identische Types (`Queue<T>`, `Dictionary<T,U>`, `HashSet<T>`) haben in den beiden mscorlib-Assemblies unterschiedliche Type-Identity → Production-DLL-Klassen werfen `TypeLoadException` beim Laden im Test-AppDomain. Konkret aktuell betroffen: `BoundedEventQueue<T>` (Queue), `BotSafe` (Dictionary/List/HashSet), `QuestManagerPoller` (HashSet/List), `RecentDecisionsBuffer` (List).

## Acceptance Criteria
1. ✅ **Option A umgesetzt** (D-38): `Krafs.Rimworld.Ref` aus `Source/RimWorldBot.csproj` entfernt, durch manuelle `<Reference>` mit HintPath auf `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.IMGUIModule.dll`, `UnityEngine.TextRenderingModule.dll`, `UnityEngine.InputLegacyModule.dll` aus `$(RimWorldManagedDir)` ersetzt. Production-DLL nutzt jetzt Microsoft net472 mscorlib (via implicit Standard-Refs) statt Mono-Stub.

2. ✅ **Verifikation Carry-Over-Tests** alle grün:
   - `Tests/Events/BoundedEventQueueTests.cs` — 8 Tests (wiederhergestellt aus 1.13-Wegwurf)
   - `Tests/Core/BotSafeTests.cs` — 13 Tests (SafeTick mit mock-Exception → ErrorBudget; SafeApply Caller-Pattern; Sliding-Window mit `NowProvider`-Mock-Clock; Poison-Cooldown-Boundary; Multi-Context-Isolation)
   - `Tests/Events/QuestManagerPollerTests.cs` — 9 Tests (neue/entfernte IDs; Re-Poll ohne Diff; null-defName-Fallback; Defensive null-EventQueue/null-Seen)
   - `Tests/Data/RecentDecisionsBufferTests.cs` — 7 Tests (Add → Auto-Pin für PhaseTransition/EndingSwitch/CrashRecovery*; FIFO-Cap)
   - PlanArbiter Skip-deferred-Tests aus 1.13 reaktiviert (Same-Layer-Conflict-Logging via DecisionLog)
   - **Total: 68 grün, 0 skip** (vorher 28 grün, 2 skip)

3. ✅ **csproj-Parametrisierung** + **Konsolidierung**: `Directory.Build.props` zentralisiert `RimWorldManagedDir` mit Override-Chain (CLI-Property > Env-Var > Default). Source/ und Tests/ csprojs erben automatisch. `Tests/README.md` aus Story 1.13 dokumentiert Cross-Dev-Setup unverändert anwendbar.

4. ⏳ **Build-Smoke-Test TC-14-PRODUCTION-LOAD**: User-Aufgabe (MT-2 in USER-CHECKLIST.md). Production-DLL build grün auf Dev-Maschine; aber tatsächliches Laden in RimWorld 1.6 muss user-seitig verifiziert werden bevor Story done.

5. ✅ **Test-Run-Output**: `Tests/test-run-1-14-completion.txt` committed mit `dotnet test --logger "console;verbosity=normal"`-Output (68 Bestanden, 0 Übersprungen).

6. ✅ **Test-Seams in Production-Code**: `internal`-static Felder mit Defaults zur Production-Runtime, von Tests via `InternalsVisibleTo` mockbar:
   - `BotSafe.NowProvider: Func<float>` (default `Time.realtimeSinceStartup`)
   - `BotSafe.ErrorLogger: Action<string>` (default `Verse.Log.Error`)
   - `BotSafe.WarningLogger: Action<string>` (default `Verse.Log.Warning`)
   - `QuestManagerPoller.QuestSource: Func<IEnumerable<(int Id, string DefName)>>` (default `Find.QuestManager.QuestsListForReading.Select(...)`)
   - `QuestManagerPoller.TickProvider: Func<int>` (default `GenTicks.TicksGame`)
   - Reset-Helper-Methoden (`ResetNowProviderForTesting`/`ResetForTesting`) damit Tests Default-State herstellen können.

7. ✅ **Test-Collection-Isolation**: `[Collection("StaticStateMutators")]` mit `DisableParallelization=true` für Tests die shared static state in BotSafe/QuestManagerPoller manipulieren — verhindert Race-Conditions zwischen parallel-laufenden Test-Klassen.

## Tasks
- [ ] Decision: Option A vs B vs C (Decision-Log)
- [ ] Production-csproj-Refactor entsprechend der Decision
- [ ] BoundedEventQueueTests wiederherstellen (8 Tests aus 1.13-Wegwurf)
- [ ] BotSafe Test-Seam: `internal static Func<float> NowProviderForTesting` einbauen, default = `() => Time.realtimeSinceStartup`
- [ ] BotSafeTests schreiben (Carry-Over Story 1.10)
- [ ] QuestManagerPoller Test-Seam: `internal static Func<IEnumerable<(int id, string defName)>> QuestSourceForTesting` oder ähnliches
- [ ] QuestManagerPollerTests schreiben (Carry-Over Story 1.12)
- [ ] RecentDecisionsBufferTests schreiben
- [ ] csproj-Parametrisierung + Tests/README
- [ ] Manueller Game-Test TC-14-PRODUCTION-LOAD
- [ ] Test-Run-Output committen

## Dev Notes
**Kontext:** D-37 (Story 1.13 Scope-Cut). Test-Runtime-Limit ist real, nicht Cosmetic — ohne Fix können Stories 2.x-7.x ihre Unit-Test-ACs nicht erfüllen, was Coverage in Production-Code deutlich reduziert.

**Nehme an, dass:** RimWorld-Install verfügbar ist auf jeder Dev-Maschine die Tests laufen lässt. CI-Setup ist out-of-Scope für 1.14, kommt in Story 8.10 (GitHub-Release-Pipeline).

**Vorausgesetzt:** 1.13 (Test-Framework xUnit setup), 1.10 (BotSafe), 1.12 (QuestManagerPoller). Entweder Production-Refactor (Option A/B) oder Tooling-Wechsel (Option C) — beides ist substantiell genug für eigene Story.

## File List (vorläufig, abhängig von gewählter Option)
| Pfad | Op |
|---|---|
| `Source/RimWorldBot.csproj` | modify (Option A: Krafs entfernen + manuelle Refs; Option B: dual-target) |
| `Source/Core/BotSafe.cs` | modify (Test-Seam für Now-Provider) |
| `Source/Events/QuestManagerPoller.cs` | modify (Test-Seam für Quest-Source) |
| `Tests/Events/BoundedEventQueueTests.cs` | create (wiederhergestellt aus 1.13) |
| `Tests/Core/BotSafeTests.cs` | create |
| `Tests/Events/QuestManagerPollerTests.cs` | create |
| `Tests/Data/RecentDecisionsBufferTests.cs` | create |
| `Tests/RimWorldBot.Tests.csproj` | modify (RimWorldManagedDir-Property condition) |
| `Tests/README.md` | create |
| `_bmad/decisions.md` | append (Option-Decision) |

## Testing
- Unit: Wiederhergestellte Carry-Over-Tests + neue RecentDecisionsBuffer-Tests (Ziel: ≥30 grüne Tests insgesamt)
- Integration: TC-14-PRODUCTION-LOAD (manueller Game-Test)

## Review-Gate
Code-Review gegen D-37, Test-Coverage-Vollständigkeit, Production-Build-Robustheit (kein Verse-API-Bruch).

## Transient/Persistent
Test-Helpers + Test-Seams sind **nicht Ship-Code** — Test-Seams sind `internal`-static Felder mit Production-Default, werden nicht persistiert.
