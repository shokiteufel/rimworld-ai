# Story 1.13: Test-Infrastructure (FakeSnapshotProvider + TestSnapshotBuilder, Cross-Cutting)

**Status:** done
**Epic:** Epic 1 (Cross-Cutting-Infrastruktur)
**Size:** S
**Decisions referenced:** F-ARCH-10, CC-STORIES-13

## Story
Als Mod-Entwickler möchte ich **Test-Helper-Infrastruktur** (FakeSnapshotProvider + TestSnapshotBuilder) die Unit-Tests ohne RimWorld-Runtime ermöglicht — damit alle Stories mit „Unit-Tests via Fake-Snapshots"-AC (viele Stories in Epic 2-7) konsistent testen.

## Acceptance Criteria
1. **(Deferred to Story 2.1)** `Source/Testing/FakeSnapshotProvider.cs` implementiert `ISnapshotProvider` mit konfigurierbaren Snapshots (via Setter) — verschoben weil `ISnapshotProvider`-Interface + Snapshot-Records (ColonySnapshot/PawnSnapshot/CellSnapshot) erst in Story 2.1 definiert werden. Story Dev Notes hat das schon als „Vorausgesetzt: 2.1" dokumentiert.
2. **(Deferred to Story 2.1)** `Source/Testing/TestSnapshotBuilder.cs` Fluent-API — selber Grund.
3. **Pflicht-AC-Template** für alle Unit-Test-ACs ab Story 2.1: „Unit-Tests via `TestSnapshotBuilder.X.Build()` — kein RimWorld-Runtime-Mock nötig" (Template bleibt verbindlich auch wenn die Implementation in 2.1 erfolgt). **Verifikation:** in CC-STORIES-13 dokumentiert; Story-Drafting-Workflow muss in 2.x-Story-Files explizit eine "Testing"-Sektion enthalten die das Template referenziert. Aktuell kein automatisierter Watchdog-Check — Eskalation: wenn 2.x-Stories ohne Test-Template gedraftet werden, wird das beim CR-Review sichtbar und Story zurueckgewiesen.
4. **(Deferred to Story 2.1)** `MockEmergencyResolver` + `MockConfigResolver` — selber Grund (depend on 2.1-Resolver-Interfaces).
5. Test-Assembly `Tests/RimWorldBot.Tests.csproj` separat (kein Ship-Dependency, eigene bin/-Output, NICHT in `Assemblies/`). Verwendet xUnit 2.9.x + Microsoft.NET.Test.Sdk 17.10. Production-DLL via ProjectReference. Pre-Requisite: RimWorld muss installiert sein (Test-Pipeline referenziert `Assembly-CSharp.dll` direkt aus `RimWorldWin64_Data/Managed/`).
6. Meta-Tests in `Tests/Meta/TestFrameworkMetaTest.cs` — Xunit-Discovery + Theory-Pattern + ImmutableCollections-Verfügbarkeit.
7. **Carry-Over aus vorherigen Stories** (Status pro Story):
   - **Story 1.9 SchemaRegistry**: ✅ DELIVERED — `Tests/Data/SchemaRegistryTests.cs` (7 Tests): LatestAppliedVersion matcht CurrentSchemaVersion-const, Bumps-Chain ist kontiguierlich, kein Applied-nach-Planned, Story 1.12 Bump ist Applied, Story 4.3 Bump auf v4→v5 verschoben (D-36 Verifikation).
   - **Story 1.11 PlanArbiter**: ✅ DELIVERED (vorgezogen aus erweitertem Carry-Over) — `Tests/Decision/PlanArbiterTests.cs` (7 Tests): Higher-Layer-Wins für DraftOrder/WorkPriority/BuildPlan/BillPlan, RetreatPoint-Layer-Selection, Different-Cell-Persistence, leere Producer-Liste.
   - **Story 1.10 BotSafe**: ❌ DEFERRED zu Folge-Story 1.14 (Test-Runtime-Infrastructure-Refactor) — Begründung: Krafs.Rimworld.Ref-built Production-DLL referenziert Mono-style mscorlib (Queue<T>/Dictionary<T> in mscorlib.dll), Microsoft .NET Framework 4.7.2 hat dieselben Types in derselben Assembly aber unterschiedlicher Type-Identity → TypeLoadException beim Laden im xUnit-Runner. Fix-Optionen siehe D-37.
   - **Story 1.12 QuestManagerPoller**: ❌ DEFERRED zu Folge-Story 1.14 — selber Grund (nutzt `HashSet<int>` + `List<int>` aus Production-DLL).

## Tasks
- [ ] `Source/Testing/FakeSnapshotProvider.cs` **(deferred zu 2.1, D-37)**
- [ ] `Source/Testing/TestSnapshotBuilder.cs` **(deferred zu 2.1, D-37)**
- [ ] `Source/Testing/MockEmergencyResolver.cs` **(deferred zu 2.1, D-37)**
- [ ] `Source/Testing/MockConfigResolver.cs` **(deferred zu 2.1, D-37)**
- [x] Separates Test-Assembly-Setup in `Tests/RimWorldBot.Tests.csproj` (xUnit 2.9 + Microsoft.NET.Test.Sdk 17.10 + parametrisierter Game-Install-Pfad)
- [x] Meta-Tests in `Tests/Meta/TestFrameworkMetaTest.cs` (Discovery + Theory + ImmutableCollections-Roundtrip via BuildPlan-Production-Record)
- [x] Carry-Over Story 1.9 SchemaRegistry-Tests (8 Tests, inkl. Drift-Detection via InternalsVisibleTo + null/empty Edge-Cases + Spec-Locks fuer D-36)
- [x] Carry-Over Story 1.11 PlanArbiter-Tests (13 aktiv + 2 Skip-deferred zu 1.14, alle 4 Arbitrate-Methoden + FocusedFire/AssignedPositions + NullPriorities + Same-Layer-First-Wins)
- [x] Tests/README.md mit Cross-Dev-Setup-Anleitung
- [x] Test-Run-Output committed (`Tests/test-run-2026-04-25.txt`: 28 grun + 2 sichtbar Skip-deferred)
- [ ] Cross-Story-Documentation in Stories 2.x, 3.x, … **(Pflicht-Template per AC-3, Enforcement via CR-Gate ab Story 2.1)**

## Dev Notes
**Kontext:** Architecture §9.1 + CC-STORIES-13. Ohne diese Story schreibt jeder Dev sein eigenes Mock → Inconsistency.
**Nehme an, dass:** xUnit oder NUnit als Test-Framework (Dev-Präferenz, nicht Story-Entscheidung).
**Vorausgesetzt:** 2.1 (ISnapshotProvider-Interface + Snapshot-Records).

## File List
| Pfad | Op |
|---|---|
| `Tests/RimWorldBot.Tests.csproj` | create (xUnit + ProjectRef + echte RimWorld-DLL-Refs) |
| `Tests/Data/SchemaRegistryTests.cs` | create (7 Tests, Carry-Over 1.9) |
| `Tests/Decision/PlanArbiterTests.cs` | create (7 Tests, Carry-Over 1.11) |
| `Tests/Meta/TestFrameworkMetaTest.cs` | create (3 Tests, AC-6 Meta) |
| `Source/Testing/FakeSnapshotProvider.cs` | **deferred to 2.1** |
| `Source/Testing/TestSnapshotBuilder.cs` | **deferred to 2.1** |
| `Source/Testing/MockEmergencyResolver.cs` | **deferred to 2.1** |
| `Source/Testing/MockConfigResolver.cs` | **deferred to 2.1** |

## Testing
- Meta-Tests: Builder erzeugt gültige Snapshots
- Integration: Story-2.1-Unit-Test nutzt Builder

## Review-Gate
Code-Review gegen F-ARCH-10, Nutzbarkeit für alle Planner-Tests.

## Transient/Persistent
Test-Helpers sind **nicht Ship-Code** — nicht persistiert, nicht im Release-ZIP.
