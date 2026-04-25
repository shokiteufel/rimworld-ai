# Story 1.14: Test-Runtime-Infrastructure-Refactor (Cross-Cutting)

**Status:** ready-for-dev
**Epic:** Epic 1 (Cross-Cutting-Infrastruktur)
**Size:** M
**Decisions referenced:** D-37 (Story 1.13 Scope-Cut + 1.14 Begründung), CC-STORIES-13

## Story
Als Mod-Entwickler möchte ich **Production-DLL-Build-Pipeline so refactoren, dass Microsoft net472 mscorlib für Standard-Types (Queue/Dictionary/HashSet/List) verwendet wird** — damit Carry-Over-Tests aus Story 1.10 (BotSafe), 1.12 (QuestManagerPoller) und alle künftigen Tests, die Production-Klassen mit Mono-mscorlib-Type-Use instantiieren, im xUnit-Test-Runner unter Microsoft .NET Framework 4.7.2 laufen können.

## Hintergrund (D-37)
Story 1.13 hat ein strukturelles Test-Runtime-Limit identifiziert: Die `Krafs.Rimworld.Ref`-NuGet-Pkg-built Production-DLL referenziert Mono-style mscorlib (Krafs Stub) für Standard-Collection-Types. xUnit-Test-Runner läuft unter Microsoft .NET Framework 4.7.2 mit Microsoft mscorlib. Identische Types (`Queue<T>`, `Dictionary<T,U>`, `HashSet<T>`) haben in den beiden mscorlib-Assemblies unterschiedliche Type-Identity → Production-DLL-Klassen werfen `TypeLoadException` beim Laden im Test-AppDomain. Konkret aktuell betroffen: `BoundedEventQueue<T>` (Queue), `BotSafe` (Dictionary/List/HashSet), `QuestManagerPoller` (HashSet/List), `RecentDecisionsBuffer` (List).

## Acceptance Criteria
1. **Production-Build-Pipeline-Refactor** entlang einer der drei in D-37 dokumentierten Optionen:
   - **Option A (bevorzugt)**: `Krafs.Rimworld.Ref` aus Production-csproj entfernen, durch manuelle `<Reference>` zu `Assembly-CSharp.dll` + `UnityEngine.*.dll` (HintPath via Game-Install-Property) ersetzen. Microsoft net472 mscorlib via implicit Standard-Refs. Tradeoff: Build erfordert RimWorld-Install (akzeptabel für Dev-Maschine; CI braucht RimWorld-Headless-Setup oder lokale DLL-Cache).
   - **Option B**: Dual-Target Production csproj — Standard-Build mit Krafs (Distribution-DLL für Mod-Release), Test-Build mit Game-Install-Refs. Tradeoff: doppelte Build-Pipeline, mehr CI-Komplexität.
   - **Option C**: Mono-Test-Runner statt Microsoft xUnit. Tradeoff: ungewöhnliches Setup, weniger IDE-Integration.

   Entscheidung welche Option im Story-Kickoff fixiert in einem Decision-Log-Eintrag (D-XX).

2. **Verifikation**: nach Refactor müssen die folgenden Test-Files laufen und grün sein (in `Tests/Events/BoundedEventQueueTests.cs` etc., ggf. wieder hergestellt):
   - `BoundedEventQueueTests` (8 Tests aus 1.13-Wegwurf wiederherstellen)
   - `BotSafeTests` (Carry-Over Story 1.10): SafeTick mit mock-Exception → ErrorBudget; SafeApply Caller-Pattern (false bei poisoned); Sliding-Window mit `Time.realtimeSinceStartup`-Mock (Test-Seam via injectable `Func<float>`); Poison-Cooldown-Ablauf nach 600s
   - `QuestManagerPollerTests` (Carry-Over Story 1.12): Fake-Quest-List → Poll detektiert neue/entfernte IDs → Events enqueued; Re-Poll ohne Diff → keine Events; null-`quest.root`-Fallback nutzt `UnknownQuestDef`-Sentinel
   - `RecentDecisionsBufferTests`: Add → Auto-Pin für PhaseTransition/EndingSwitch/CrashRecovery*-Kinds; Trim auf transientCap/pinnedCap

3. **csproj-Parametrisierung** (Carry-Over MED-1 aus Story 1.13 Code-Review):
   ```xml
   <RimWorldManagedDir Condition="'$(RimWorldManagedDir)' == ''">D:\SteamLibrary\steamapps\common\RimWorld\RimWorldWin64_Data\Managed</RimWorldManagedDir>
   ```
   Plus `Tests/README.md` mit Override-Anleitung für Cross-Dev (Steam-Default-Pfad, Custom-Library-Pfad, Mac/Linux).

4. **Build-Smoke-Test** nach Refactor: Production-DLL muss weiterhin in RimWorld 1.6 laden ohne Verse-API-Mismatches. Manuelle Game-Test (TC-14-PRODUCTION-LOAD): RimWorld starten, neues Game beginnen, `Player.log` auf `[RimWorldBot]`-Init-Logs prüfen, keine `MissingMethodException`/`TypeLoadException`.

5. **Test-Run-Output committen**: nach Implementation `dotnet test --logger "console;verbosity=normal"` ausführen, Output als `Tests/test-run-1-14-completion.txt` ins Repo.

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
