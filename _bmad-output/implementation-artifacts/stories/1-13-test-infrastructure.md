# Story 1.13: Test-Infrastructure (FakeSnapshotProvider + TestSnapshotBuilder, Cross-Cutting)

**Status:** ready-for-dev
**Epic:** Epic 1 (Cross-Cutting-Infrastruktur)
**Size:** S
**Decisions referenced:** F-ARCH-10, CC-STORIES-13

## Story
Als Mod-Entwickler möchte ich **Test-Helper-Infrastruktur** (FakeSnapshotProvider + TestSnapshotBuilder) die Unit-Tests ohne RimWorld-Runtime ermöglicht — damit alle Stories mit „Unit-Tests via Fake-Snapshots"-AC (viele Stories in Epic 2-7) konsistent testen.

## Acceptance Criteria
1. `Source/Testing/FakeSnapshotProvider.cs` implementiert `ISnapshotProvider` mit konfigurierbaren Snapshots (via Setter)
2. `Source/Testing/TestSnapshotBuilder.cs` Fluent-API:
   - `.WithColony(ColonySnapshot)` / `.WithColonistCount(int)` / `.WithFoodDays(double)` / `.WithMood(double)`
   - `.WithPawn(PawnSnapshot)` / `.AddPawn(uniqueLoadID, skills)`
   - `.WithCell(CellSnapshot)` / `.AddHazardCell(pos, kind)` / `.AddWildPlant(pos, kind)`
   - `.Build()` → `FakeSnapshotProvider`
3. **Pflicht-AC-Template** für alle Unit-Test-ACs ab Story 2.1: „Unit-Tests via `TestSnapshotBuilder.X.Build()` — kein RimWorld-Runtime-Mock nötig"
4. `MockEmergencyResolver` + `MockConfigResolver` für Planner-Tests (ConfigResolver.Get liefert konfigurierbare Werte)
5. Alle Test-Helpers im separaten Assembly `RimWorldBot.Testing.dll` (kein Ship-Dependency)
6. Unit-Tests für die Test-Helper selbst (meta-tests)

## Tasks
- [ ] `Source/Testing/FakeSnapshotProvider.cs`
- [ ] `Source/Testing/TestSnapshotBuilder.cs`
- [ ] `Source/Testing/MockEmergencyResolver.cs`
- [ ] `Source/Testing/MockConfigResolver.cs`
- [ ] Separates Test-Assembly-Setup in `.csproj`
- [ ] Meta-Tests
- [ ] Cross-Story-Documentation in Stories 2.x, 3.x, …

## Dev Notes
**Kontext:** Architecture §9.1 + CC-STORIES-13. Ohne diese Story schreibt jeder Dev sein eigenes Mock → Inconsistency.
**Nehme an, dass:** xUnit oder NUnit als Test-Framework (Dev-Präferenz, nicht Story-Entscheidung).
**Vorausgesetzt:** 2.1 (ISnapshotProvider-Interface + Snapshot-Records).

## File List
| Pfad | Op |
|---|---|
| `Source/Testing/FakeSnapshotProvider.cs` | create |
| `Source/Testing/TestSnapshotBuilder.cs` | create |
| `Source/Testing/MockEmergencyResolver.cs` | create |
| `Source/Testing/MockConfigResolver.cs` | create |
| `RimWorldBot.Testing.csproj` | create (optional eigenes Test-Assembly) |

## Testing
- Meta-Tests: Builder erzeugt gültige Snapshots
- Integration: Story-2.1-Unit-Test nutzt Builder

## Review-Gate
Code-Review gegen F-ARCH-10, Nutzbarkeit für alle Planner-Tests.

## Transient/Persistent
Test-Helpers sind **nicht Ship-Code** — nicht persistiert, nicht im Release-ZIP.
