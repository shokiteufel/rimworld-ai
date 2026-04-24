# Architecture — RimWorld Bot Mod

**Version:** 2.3 (PLANNING)
**Letztes Update:** 2026-04-24
**Status:** **Approved (2026-04-24)** — nach Round-4-Review + Option-A-Einarbeitung vom User signed-off
**Review-Historie:**
- v1.0 → Round 1 (REJECT, 7 CRIT / 9 HIGH) → v2.0
- v2.0 → Round 2 (APPROVE-W-MINOR, 2 HIGH / 4 MED / 1 LOW) → v2.1
- v2.1 → Round 3 Full-Panel (APPROVE-W-MINOR × 4, 5 HIGH / 12 MED / 7 LOW + F-STAB-10 PARTIAL) → v2.2
- v2.2 → Round 4 Full-Panel (APPROVE-W-MINOR × 4, 5 HIGH / 9 MED / 7 LOW + 1 Regression + F-STAB-10 erneut PARTIAL) → v2.3
**Begleitdokumente:**
- Review-Report Round 1: `architecture-party-mode-review-2026-04-24.md`
- Review-Report Round 2: `architecture-review-round-2-2026-04-24.md`
- Review-Report Round 3: `architecture-review-round-3-2026-04-24.md`
- Review-Report Round 4: `architecture-review-round-4-2026-04-24.md`
- Decisions: `../../_bmad/decisions.md` (D-11 bis D-26)

---

## 1. Architektur-Überblick

Harmony-geminimalistischer C#-Mod im RimWorld-Standard. Single-Threaded (RimWorld-Constraint), ereignisgesteuert via `GameComponent.GameComponentTick()` und Event-Handler in schmalen Harmony-Postfixes. Strikte Schicht-Trennung: **Observation → Decision (Plan) → Execution (Apply)**. Keine Schicht mutiert RimWorld-State außer der Execution-Schicht.

```
┌─────────────────────────────────────────────────────────────────────┐
│                       RimWorld Core (Vanilla)                       │
└─────────────────────────────────────────────────────────────────────┘
                                  ▲ ▼
┌─────────────────────────────────────────────────────────────────────┐
│              Minimal Harmony Patches Layer (H2–H7)                  │
│          (Event-Hooks only — KEIN Patch auf Tick-Loop)              │
└─────────────────────────────────────────────────────────────────────┘
                                  ▲ ▼
┌─────────────────────────────────────────────────────────────────────┐
│               GameComponent/MapComponent Tick-Host                  │
│    BotGameComponent.Tick()  +  BotMapComponent.MapComponentTick()   │
└─────────────────────────────────────────────────────────────────────┘
                                  ▲ ▼
┌─────────────────────────────────────────────────────────────────────┐
│                           Bot Controller                            │
│     (Composition-Root, Phase-State-Machine, Toggle, Orchestrator)   │
└─────────────────────────────────────────────────────────────────────┘
     ▲              ▲                   ▲                 ▲
     │              │                   │                 │
┌────┴─────┐   ┌────┴────┐    ┌─────────┴──────────┐ ┌────┴─────┐
│ Snapshot │   │Decision │    │    Execution       │ │    UI    │
│ Provider │   │ (Plan)  │    │     (Apply)        │ │          │
│ +Analysis│   │         │    │                    │ │          │
│ on POCOs │   │BuildPlan│    │BlueprintPlacer     │ │Toggle    │
│          │   │BillPlan │    │BillManager         │ │Overlay   │
│Cell-     │   │WorkPlan │    │WorkPriorityWriter  │ │DebugPanel│
│Colony-   │   │DraftOrd │    │DraftController     │ │Settings  │
│Pawn-     │   │Ending-  │    │CaravanExecutor     │ │Decision- │
│Snapshots │   │Feasib.  │    │                    │ │Trail-Tab │
└──────────┘   └─────────┘    └────────────────────┘ └──────────┘
                                  ▲ ▼
┌─────────────────────────────────────────────────────────────────────┐
│                             Data Layer                              │
│  BotGameComponent · BotMapComponent · LearnedConfig · Telemetry     │
│                      ConfigResolver · DlcCapabilities                │
└─────────────────────────────────────────────────────────────────────┘
```

**Architektur-Invarianten**
- AI-1: Kein Harmony-Patch auf Tick-Loops. Tick-Arbeit läuft ausschließlich in `GameComponentTick` / `MapComponentTick`.
- AI-2: Decision-Klassen sind pure — sie lesen Snapshot-DTOs, produzieren Plan-Objekte, mutieren nichts.
- AI-3: Execution-Klassen sind die einzigen, die RimWorld-State ändern (Pawn.jobs, Designations, Bills, Drafter).
- AI-4: Kein Statisches Singleton außer `RimWorldBotMod` als Mod-Entry. Kollaborateure via Konstruktor-Injection über `BotControllerFactory`.
- AI-5: Jede Konfiguration hat dokumentierte Precedence (siehe §5a).
- AI-6: DLC-spezifische Pfade sind via `DlcCapabilities` gemutet — Mod läuft ohne alle DLCs.
- AI-7: Decision-Layer-Priorität: **Emergency > Override > PhaseGoal**. OverrideLibrary darf EmergencyResolver NIEMALS überstimmen. PhaseGoals sind der Default-Pfad wenn weder Emergency noch Override greift (siehe §6.2).

---

## 2. Komponenten-Verzeichnis

### 2.1 Core

| Klasse | Zweck | Abhängigkeiten |
|---|---|---|
| `RimWorldBotMod : Mod` | Entry-Point, Harmony-Init (H2–H6), ModSettings. `[StaticConstructorOnStartup]`-Klasse (siehe unten) ruft `InitializeCompat()` post-DefDatabase-Population | Harmony, `Configuration` |
| `CompatInitializer` | `[StaticConstructorOnStartup]`-Klasse, ruft `RimWorldBotMod.InitializeCompat()` nach Def-Loading (F-RW4-02) | `DefDatabase<CompatPatternDef>` |
| `BotControllerFactory` | Reine Factory (Builder-Pattern): `NewBuilder().With<ISnapshotProvider>(p).With<ConfigResolver>(c)...Build() → BotController`. Explizite Dependency-Liste als Builder-Methoden, `NewTestBuilder(FakeSnapshotProvider)` mit Defaults für Unit-Tests. Try/catch um Build-Pfad mit User-Toast bei Failure (F-STAB-26). | — |
| `BotGameComponent : GameComponent` | Hält `BotController`-Instanz + `EventQueue` + `ConfigResolver` als Felder (explizit deklariert). Konstruktor: initialisiert `eventQueue = new BoundedEventQueue(32, 224)` (vor Harmony-Patch-Möglichkeit, F-ARCH-11/F-ARCH-15). Lifecycle: `FinalizeInit()` → `BuildController()`; `LoadedGame()` → `eventQueue.Clear()` + `configResolver.Invalidate()` + `ReconcilePendingPhase()` + `ReconcilePhaseGoalOrphans()` + Budget-Exhaust-Check (F-STAB-20, F-STAB-23); `StartedNewGame()` → Initial-State + `eventQueue.Clear()` | `BotControllerFactory` |
| `BotMapComponent : MapComponent` | Per-Map Analyse + Per-Map Persistenz (inkl. `stableCounter` für Phase-Transition, `phaseProgressTicks`) | Referenz auf `BotController` via Konstruktor (nicht `Current.Game`-Lookup im Hot-Path) |
| `BotController` | Orchestrator: Invariant-Check → Emergency-Merge → Phase-Eval → Plan → Apply | Per Konstruktor: 10+ Kollaborateure (Factory verdrahtet, siehe §5a) |
| `ToggleState` enum | `Off / Advisory / On / Paused` | — |
| `PhaseStateMachine` | FSM mit Forward/Backward-Transitions, `min_hold_duration`, Rollback | `PhaseDefinition[]`, `InvariantSet`, `EmergencyResolver` |
| `PhaseDefinition` (abstract) | Entry-/Exit-Conditions, Goals, `GoalHealthScore` | — |
| `Phase0a_MapAnalysis`, `Phase0_NakedStart`, `Phase1` … `Phase7_Ending` | Konkrete Phasen | `PhaseDefinition`, `GoalPlanner` |
| `InvariantSet` | I0–I12 Checks auf `ColonySnapshot`, idempotent | — |
| `EmergencyResolver` | Utility-Scoring über alle aktiven Emergencies (kein fixer Index) | `InvariantSet`, `EmergencyHandler[]` |
| `EmergencyHandler` (abstract) | `Eligibility(state) → bool`, `Score(state) → double`, `Claim(pawns) → void`, `Apply(controller)` | — |
| `I0_ColonyExtinct` + `E_EXTINCT` | Neue Invariante: wenn `colonist_count == 0`, Bot stoppt graceful | — |
| `GoalHealthMonitor` | Pro erreichtem Exit-Cond periodischer Regression-Check alle 5000 Ticks | `PhaseStateMachine` |

### 2.2 Snapshot + Analysis

Unit-testbar ohne RimWorld-Runtime: alle Analyzer arbeiten auf POCO-Snapshots.

| Klasse | Zweck | Abhängigkeiten |
|---|---|---|
| `ISnapshotProvider` | Interface: `CellSnapshot[] GetCells(Map)`, `ColonySnapshot GetColony()`, `PawnSnapshot[] GetPawns()` | — |
| `RimWorldSnapshotProvider : ISnapshotProvider` | Mapper RimWorld → POCOs. Einziger Ort mit echten RimWorld-Abhängigkeiten. | `Map`, `Pawn`, `TerrainDef` |
| `CellSnapshot` (POCO) | Fertility:float, HasWater:bool, HazardKind:enum, ChokepointScore:float, … | — |
| `ColonySnapshot` (POCO) | Food-Days, Colonist-Count, Mood-Avg, Faction-Relations, Tech-Level | — |
| `PawnSnapshot` (POCO) | Skills, Passions, Health, Needs | — |
| `MapAnalyzer` | Scoring + Cluster auf `CellSnapshot[]` | `ISnapshotProvider` |
| `HazardScanner` | Lava/Pollution/Toxic via `DlcCapabilities.TerrainHazardTags()` | — |
| `ThreatAssessment` | Raid-Strength, Colony-Strength | `PawnSnapshot[]` |
| `ResourceTracker` | Stock-Monitoring, Day-Forecast | `ColonySnapshot` |
| `PawnAssessment` | Skill-Evaluation, Best-for-Job | `PawnSnapshot[]` |
| `EndingFeasibility` | Score pro Ending, filtert via `DlcCapabilities.AvailableEndings()` | `ColonySnapshot`, `DlcCapabilities` |
| `MapAnalysisSummary` (POCO, `IExposable`) | Schlanker Cache: Top-3-Sites + Scores + Cluster-IDs. KEIN Per-Cell-Array. | — |

### 2.3 Decision (Plan-Producers, pure)

Geben immer Plan-Objekte zurück. Keine Mutation.

| Klasse | Input | Output |
|---|---|---|
| `BuildPlanner` | `CellSnapshot[]`, `PhaseGoals` | `BuildPlan` (Liste Blueprint-Intents) |
| `WorkPlanner` | `PawnSnapshot[]`, `PhaseGoals` | `WorkPriorityPlan` (Pawn → Priority-Map) |
| `BillPlanner` | `ColonySnapshot`, `WorkbenchSnapshot[]` | `BillPlan` (add/remove pro Workbench) |
| `CombatCommander` | `ThreatSnapshot`, `ColonySnapshot` | `DraftOrder` (+ Retreat-Points) |
| `CaravanPlanner` | `ColonySnapshot`, `QuestSnapshot` | `CaravanPlan` |
| `EndingFeasibility` | Siehe §2.2 | `FeasibilityMap` + `EndingChoice` (mit Hysterese-Gate, siehe §6.3) |

### 2.3a Plan-Schemas (D-21, verschärft durch D-23)

Alle Plan-Objekte sind **immutable `record`-Typen** mit Value-Equality. **Identifier-only**: keine RimWorld-Runtime-Typen (`ThingDef`, `Building_*`, `IntVec3`, `Rot4`, `WorkTypeDef`) in Plan-Records — nur strings, ints, enums. Execution-Layer resolved Identifier im Apply (`DefDatabase<ThingDef>.GetNamed()`, `Map.thingGrid.ThingAt(...)`). Das garantiert `§2.2`-Testbarkeits-Invariante (D-23).

```csharp
public record BillIntent(
    BillIntentKind Kind,
    string RecipeDefName,
    int WorkbenchThingIDNumber,
    int TargetCount);
public enum BillIntentKind { Add, Remove, UpdateCount }
public record BillPlan(ImmutableList<BillIntent> Intents);

public record WorkPriorityPlan(
    ImmutableDictionary<string /*pawn UniqueLoadID*/, ImmutableDictionary<string /*WorkTypeDef.defName*/, int>> Priorities);

public record BlueprintIntent(
    string DefName,
    (int x, int z) Position,
    byte Rotation);   // Rot4.AsByte
public record BuildPlan(ImmutableList<BlueprintIntent> Intents);

public record DraftOrder(
    ImmutableHashSet<string /*pawn UniqueLoadID*/> Draft,
    ImmutableHashSet<string> Undraft,
    (int x, int z)? RetreatPoint)
{
    // CC-STORIES-14 (Round-1-Review, 2026-04-24): Combat-Subtypen aus Stories 5.4/5.7/5.8/7.8/7.13.
    // Init-only Properties mit Empty-Defaults — damit existierende 3-arg-Konstruktor-Aufrufe (5.3, 5.5, 5.7, 5.8, 7.8, 7.13)
    // nicht brechen. Stories die Focused-Fire oder Assigned-Positions brauchen erzeugen via `with`-Expression:
    //   new DraftOrder(draft, undraft, retreat) with { FocusedFireTargets = myDict }
    public ImmutableDictionary<string /*shooter UniqueLoadID*/, string /*target UniqueLoadID*/> FocusedFireTargets { get; init; }
        = ImmutableDictionary<string, string>.Empty;
    // Für Killpoint-Shooter-Crews (5.3) + Ship-Defense-Positions (7.8) + Stellarch-Siege-Crew (7.13)
    public ImmutableDictionary<string /*pawn UniqueLoadID*/, (int x, int z)> AssignedPositions { get; init; }
        = ImmutableDictionary<string, (int x, int z)>.Empty;
}

public record CaravanPlan(
    ImmutableList<string /*pawn UniqueLoadID*/> Members,
    int DestinationTile,
    ImmutableList<(string defName, int count)> Supplies);
```

**Immutability-Vertrag (F-ARCH-17):** `ImmutableList<T>` / `ImmutableDictionary` / `ImmutableHashSet` aus `System.Collections.Immutable` — echte Immutabilität, kein View-Leak via Cast-back. Plus: Records-Value-Equality arbeitet auf Content (Strings/Ints/Enums), nicht auf Object-Refs → Test-Assertions via `planA == planB` sind korrekt.

**Vertrag:** Planner erzeugt immer **valide** Plans (kein null, keine Empty-Set-Paradoxe, alle Identifier existieren zum Plan-Zeitpunkt). Apply-Klassen dürfen Plans refusen (z. B. Pawn nicht mehr spawned, Def via Mod-Unload verschwunden) ohne Exception — einfach Skip + Log.

### 2.3b Goal-Phase-Tag-Schema (D-25)

Für `ReconcilePhaseGoalOrphans()` (§5) muss Execution-Layer jeden Bot-platzierten Blueprint/Designation/Job einem Phase-Goal zuordnen können. Vanilla-Klassen haben kein solches Feld — daher führt `BotMapComponent` eigene Tag-Dictionaries (F-STAB-21, F-STAB-10 → RESOLVED):

```csharp
public record PhaseGoalTag(int PhaseIndex, string GoalId);   // z. B. (3, "phase3-build-cold-storage")

public class BotMapComponent : MapComponent {
    // Tag-Maps für Orphan-Cleanup nach Phase-Rollback
    public Dictionary<int /*thing.thingIDNumber*/, PhaseGoalTag> botPlacedThings = new();
    public Dictionary<int /*job.loadID*/, PhaseGoalTag> botAssignedJobs = new();
    // ... (Rest in §5 BotMapComponent-Snippet)
}
```

**Schreib-Pfad:** Execution-Klassen (`BlueprintPlacer.Apply`, `BillManager.Apply`, `WorkPriorityWriter.Apply`, `DraftController.Apply`) tragen nach jedem Apply-Step das Tag ein:
```csharp
// in BlueprintPlacer.Apply:
var thing = designator.DesignateSingleCell(pos);
botMap.botPlacedThings[thing.thingIDNumber] = new PhaseGoalTag(currentPhaseIndex, goalId);
```

**Lese-Pfad:** `ReconcilePhaseGoalOrphans()` iteriert beide Dicts, cancelt was `tag.PhaseIndex > currentPhaseIndex` oder `tag.GoalId` zu einem canceled Goal gehört. Nach Cleanup: Entry aus Dict entfernen.

**Cleanup:** Destroyed/Discarded Things werden alle 60000 Ticks (wie `perPawnPlayerUse`) aus Dicts entfernt via Lookup `Map.thingGrid.ThingsListAt(pos)` / `thing.Destroyed`.

### 2.4 Execution (Plan-Appliers, nur hier Mutation)

| Klasse | Input | Side-Effect |
|---|---|---|
| `BlueprintPlacer` | `BuildPlan` | `Designator_Build.DesignateSingleCell()` |
| `BillManager` | `BillPlan` | `Bill` add/remove auf `Building_WorkTable` |
| `WorkPriorityWriter` | `WorkPriorityPlan` | `pawn.workSettings.priorities[…]` + Read-After-Write-Check |
| `DraftController` | `DraftOrder` | `pawn.drafter.Drafted = …` |
| `CaravanExecutor` | `CaravanPlan` | `CaravanFormingUtility.StartFormingCaravan()` |

### 2.5 UI

| Klasse | Zweck |
|---|---|
| `MainTabWindow_BotControl` | Button via `MainButtonDef` (XML). Zeigt Phase, primary Ending, State. **Kein Harmony-Patch** |
| `ITab_Pawn_BotControl : ITab` | Eigener Pawn-ITab mit Toggle „Player Use". Registriert via XML-Patch auf `Human.inspectorTabs` |
| `StatusOverlay` | Phase, Goal, Blocker als Banner |
| `SiteMarkerOverlay` | Top-3-Kreise + Hover-Tooltip mit Score-Breakdown-Tabelle (W_FOOD, W_DEFENSE, W_HAZARD, …) |
| `DebugPanel` | Invariants-/Goals-/Resources-Inspector + Tab „Decisions" (persistenter Decision-Trail, FIFO-100) |
| `SettingsWindow` | ModSettings-Panel |

### 2.6 Data & Services

| Klasse | Zweck | Abhängigkeiten |
|---|---|---|
| `BotGameComponent : GameComponent` | Game-global State (ToggleState, primaryEnding, endingStrategy, perPawnPlayerUse als `Dictionary<string, bool>` mit `GetUniqueLoadID()`, recentDecisions FIFO) | `Scribe` |
| `BotMapComponent : MapComponent` | Per-Map State (`MapAnalysisSummary`, phase-progress) | `Scribe` |
| `LearnedConfig` | Cross-Game-Lern-Daten, per-Biome-partitioniert, atomic-write + `.bak` + mutex | `GenFilePaths.ConfigFolderPath` |
| `Configuration : ModSettings` | User-Defaults | `ModSettings` |
| `DlcCapabilities` | Feature-Detection via `ModLister.*Installed`, exponiert `AvailableEndings()`, `TerrainHazardTags()`, `Has(dlc) → bool` | `ModLister` |
| `TelemetryLogger` | JSONL-Event-Log. **Default OFF.** ID-only (kein `pawn.Name`). Rotation 10 MB × 3 Files = 30 MB Cap | `GenFilePaths.ConfigFolderPath` |
| `OverrideEvaluator` | Player-Overrides nach 60 s bewerten, Layer-Kontrakt siehe §6.2 (Emergency > Override > PhaseGoal) | `SnapshotProvider` |
| `BotErrorBudget` | Thread-safe Token-Bucket max 5 Errors/min (über `lock`), dann silent. Reset auf `GameComponent.StartedNewGame` + `LoadedGame`. | — |
| `RecentDecisionsBuffer` | Zwei-Tier-Retention: `transient` FIFO-100 (alle Events) + `pinned` FIFO-25. Single `Add(entry)`-API mit **Auto-Pin-Regel** basierend auf `entry.Kind`: `PhaseTransition`/`EndingSwitch`/`CrashRecovery*`/`EndingForcedOverride`/`EndingAutoEscape`/`EndingCommitmentReleased` → automatisch auch in pinned (F-ARCH-16). Kein Caller-seitiger Pin-Call nötig. Beide persistiert. | — |
| `BotSafe` | Static: `Get<T>(Func<T> accessor)` catch-all null-bubble für defensive Zugriffe | — |
| `EventQueue` | Zwei-Klassen-Queue (**Critical** Cap 32 + **Normal** Cap 224, total 256). **Critical** = `MapFinalizedEvent`, `PawnExitMapEvent` (never-drop, Caravan-/Scan-relevant). **Normal** = andere Events mit Drop-Oldest + WARN-Log (einmal pro 100 Drops). Stale-Check vor Dispatch. Ordering: FIFO per Frame per Klasse, Critical hat Dispatch-Priorität. **Transient** — NICHT persistiert. **Initialisierung im `BotGameComponent`-Konstruktor** (vor Harmony-Patch-Möglichkeit, F-ARCH-11/F-ARCH-15). **Clear bei jedem `LoadedGame`/`StartedNewGame`** (F-STAB-20). (D-19, D-24) | — |
| `BotEvent` (abstract) + Subklassen | `MapFinalizedEvent(int MapId, int EnqueueTick)`, `RaidEvent(int EnqueueTick, RaidKind, int? PointsEstimate)`, `DraftedEvent(string PawnUniqueLoadID, bool Drafted, int EnqueueTick)`, `QuestWindowEvent(string WindowTypeName, int EnqueueTick)`, `PawnExitMapEvent(string PawnUniqueLoadID, int MapId, int EnqueueTick)`. Immutable records, nur Identifikatoren. **`EnqueueTick` für ID-Recycling-Stale-Check** (D-19). | — |
| `ConfigResolver` | Single-Entry mit **typisiertem Cache** `Dictionary<object /*ConfigKey<T>*/, object>` + Signatur `Get<T>(ConfigKey<T> key)` — Compile-Time-Typ-Safety gegen Cast-Mismatch (F-ARCH-14). `Invalidate()` clear-all; Hooks: `Mod.WriteSettings()` + `BotGameComponent.LoadedGame()`/`StartedNewGame()`. Precedence siehe §5a. | `BotGameComponent`, `LearnedConfig`, `Configuration` |

---

## 3. Daten-Fluss (Tick-Durchlauf)

**Tick-Host:** `BotGameComponent.GameComponentTick()` (Game-global) + `BotMapComponent.MapComponentTick()` (per-Map). **Kein Harmony-Patch auf Tick-Loop** (siehe D-12).

```
GameComponentTick (jeden Tick, aber budgetiert — arbeitet nur alle 60 Ticks):

  0. Pre-Check: Current.Game != null && Current.Game.CurrentMap != null else skip
  1. Check ToggleState (skip if AI_OFF)
  2. try {
  3.   Invariants.CheckAll(colonySnapshot)          → InvariantResult[]
  4.   EmergencyResolver.Resolve(invariantResults)  → ChosenEmergency? (Utility-max)
  5.   if ChosenEmergency != null:
  6.       Execute ChosenEmergency.Apply()
  7.       PhaseTransition BLOCKED this tick (F-AI-01 Guard)
  8.   else:
  9.       PhaseStateMachine.Evaluate()             → may transition (w/ min_hold_duration + 2-tick-stable)
 10.       GoalHealthMonitor.CheckRegressions()     → may re-trigger completed Goal
 11.       var workPlan  = workPlanner.Plan(state)
 12.       var billPlan  = billPlanner.Plan(state)
 13.       workPriorityWriter.Apply(workPlan)
 14.       billManager.Apply(billPlan)
 15.   UI.Refresh()
 16. } catch (Exception ex) { HandleFatal(ex); FallbackToOff(); }

GameComponentTick (alle 2500 ticks):
  A. ResourceTracker.Update()
  B. EndingFeasibility.Reevaluate()   → Hysterese-Gate (§6.3) entscheidet über Switch
  C. ThreatAssessment.Update()
  D. GC.GetTotalMemory() → if > 5× Budget: FallbackToOff() + Log

GameComponentTick (alle 60000 ticks):
  E. perPawnPlayerUse Cleanup (destroyed/discarded Pawns entfernen)
  F. LearnedConfig.PersistIfDirty()

MapComponentTick (per Map):
  * MapAnalyzer-Tick-budgeted-iterator (2 ms/Tick, nach FullScan im Post-FinalizeInit-Trigger)

Event-driven (Harmony-Postfix → EventQueue → abgearbeitet im nächsten GameComponentTick):
  H2 Map.FinalizeInit     → Queue-Critical(MapFinalizedEvent)
  H3 Pawn.ExitMap          → Queue-Critical(PawnExitMapEvent)   // Caravan-relevant
  H4 IncidentWorker_RaidEnemy.TryExecuteWorker → Queue-Normal(RaidEvent)
  H5 Pawn_DraftController.Drafted → Queue-Normal(DraftedEvent)
  H6 WindowStack.Add       → (Typ-Filter Dialog_NodeTree / MainTabWindow_Quests) → Queue-Normal(QuestWindowEvent)

Event-Dispatch im Tick-Host:
  a) dequeue Critical-Queue vollständig zuerst (kleine Cap, dürfen nicht warten)
  b) dann dequeue Normal-Queue bis leer oder Tick-Budget erschöpft
  c) pro Event: Stale-Check (F-RW4-01 dokumentiert Lookup-Pfade, F-RW4-03 Map-first-Short-Circuit):
       - Map-Events: `target = Find.Maps.Find(m => m.uniqueID == evt.MapId); target != null && target.generationTick <= evt.EnqueueTick` (Recycling-safe, D-19)
       - Pawn-Events: Map-first-Short-Circuit gegen WorldPawns-Kosten:
         `pawn = Find.Maps.SelectMany(m => m.mapPawns.AllPawns).FirstOrDefault(p => p.GetUniqueLoadID() == evt.PawnId)
              ?? Find.WorldPawns.AllPawnsAliveOrDead.FirstOrDefault(p => p.GetUniqueLoadID() == evt.PawnId);
          valid = pawn != null && !pawn.Destroyed`
         (most-common-case Map-Pawn ist klein; WorldPawns-Fallback nur wenn Map-Suche null).
       - Stale → silent drop (erwartet bei Map-Dispose / Pawn-Death)
  d) Normal-Queue Overflow-Policy: Drop-Oldest + WARN-Log (1 pro 100)
  e) Critical-Queue Overflow (sehr unwahrscheinlich bei Cap 32): WARN-Log + temporärer Cap-Boost auf 64 für diese Session
```

**Zwei-Phasen-Commit bei Phase-Transition:** `pendingPhaseIndex` wird gesetzt, Goals initialisiert, erst bei erfolgreicher Init wird `currentPhaseIndex = pendingPhaseIndex` committed. Bei Exception zwischen Stages: Rollback auf `currentPhaseIndex` + Cleanup teil-initialisierter Goals (siehe §5 `ReconcilePhaseGoalOrphans`).

**Stable-Counter-Semantik (D-26 `StableConsecutiveCounter`-Pattern):** Generisches Pattern für alle „N konsekutive Erfüllungen"-Regeln — angewendet auf Phase-Transition (`stableCounter`, F-AI-08) und Ending-Auto-Escape (`autoEscapeStableCounter`, F-AI-15). Vertrag:
- Zählt **konsekutive Eval-Zyklen** in denen Bedingung erfüllt.
- Reset auf 0 sobald Bedingung in einem Eval-Zyklus **nicht** erfüllt.
- Zählt monoton sonst.
- Persistiert (Save-Load-stabil) im passenden Component.

**Phase-Transition (F-AI-08):** `BotMapComponent.stableCounter` auf Home-Map (F-AI-18: Counter-Quelle ist nur die Home-Map, `map.IsPlayerHome && map.ParentFaction == Faction.OfPlayer`; Quest-/Caravan-Maps haben eigene lokale Counter für Ambient-Szenarien). Zählt konsekutive 60er-Tick-Evaluations ohne Invariant-Violation oder Emergency. Phase-Transition-Eligibility: `stableCounter >= 2 && min_hold_duration_elapsed`.

**Post-Load-Reconcile (Crash-Recovery für Zwei-Phasen-Commit):** Läuft in `BotGameComponent.LoadedGame()` (NICHT `FinalizeInit`, weil letzteres auch bei New-Game feuert — siehe CC-3-01). `LoadedGame()`-Reihenfolge: (i) `eventQueue.Clear()` (F-STAB-20, verhindert Dispatch alter Tick-Stamps post-Load), (ii) `configResolver.Invalidate()` (Save kann andere Overrides bringen), (iii) `ReconcilePendingPhase()`, (iv) `ReconcilePhaseGoalOrphans()`, (v) Budget-Exhaust-Check (F-STAB-23).

(a) **`ReconcilePendingPhase`:** wenn `pendingPhaseIndex != currentPhaseIndex` → `pendingPhaseIndex = currentPhaseIndex`, DecisionLog-Eintrag `"crash-recovery-phase-rollback"` (auto-pinned).

(b) **`ReconcilePhaseGoalOrphans` (F-STAB-10 → RESOLVED, F-STAB-21):** iteriert `botMap.botPlacedThings` und `botAssignedJobs` (§2.3b), cancelt alles was `tag.PhaseIndex > currentPhaseIndex` oder zu einem canceled GoalId gehört (Designations removed, Jobs cancelled, Dict-Entries entfernt).

(c) **Budget-Exhaust-Check (F-STAB-23):** wenn `consecutiveSessionBudgetExhausts >= 3` → User-Toast „Bot disabled itself repeatedly — consider reporting a bug"; Counter nur bei sauberem Session-Exit reset.

---

## 4. Harmony-Patch-Inventar

| # | Target | Typ | Priority | Zweck | Risiko |
|---|---|---|---|---|---|
| ~~H1~~ | ~~Game.UpdatePlay~~ | — | — | **ENTFERNT** (D-12: GameComponentTick statt Patch) | — |
| H2 | `Verse.Map.FinalizeInit` | Postfix | `Priority.Low` | Trigger MapAnalyzer.FullScan | Niedrig |
| H3 | `Verse.Pawn.ExitMap` | Prefix | `Priority.Low` | Karawanen-Event | Niedrig |
| H4 | `RimWorld.IncidentWorker_RaidEnemy.TryExecuteWorker` | Postfix | `Priority.Low` | Raid-Announce | Mittel |
| H5 | `Verse.Pawn_DraftController.Drafted` (setter) | Postfix | `Priority.Low` | Draft-Event | Niedrig |
| H6 | `Verse.WindowStack.Add` | Postfix | `Priority.Low` | Quest-Windows inspizieren | Mittel |
| ~~H7~~ | ~~MainButtonsRoot~~ | — | — | **ENTFERNT** (D-13: MainButtonDef via XML) | — |
| ~~H8~~ | ~~ITab_Pawn_Character~~ | — | — | **ENTFERNT** (D-13: eigener ITab_Pawn_BotControl via XML) | — |

**Kein Patch, aber Mod-Subclass-Override (nicht in Harmony-Tabelle):** `RimWorldBotMod.DoSettingsWindowContents()` überschreibt die virtuelle Methode der `Mod`-Basisklasse für das Settings-Panel. Das ist Vanilla-RimWorld-API, kein Harmony-Target.

### 4.1 Harmony-Exception-Skelett (verpflichtend für alle Patches)

```csharp
[HarmonyPatch(typeof(Map), nameof(Map.FinalizeInit))]
[HarmonyPriority(Priority.Low)]
public static class H2_MapFinalizeInit {
  static void Postfix(Map __instance) {
    try {
      if (Current.Game == null) return;
      var bot = Current.Game.GetComponent<BotGameComponent>();
      bot?.EnqueueEvent(new MapFinalizedEvent(__instance.uniqueID));
    }
    catch (Exception ex) {
      if (!BotErrorBudget.CanLog()) return;
      Log.Error($"[RimWorldBot] H2_MapFinalizeInit: {ex}");
      BotSafe.Get(() => Current.Game?.GetComponent<BotGameComponent>()?.FallbackToOff());
    }
  }
}
```

**H6 (WindowStack.Add) mit Typ-Filter (F-HARMONY-09):**

```csharp
[HarmonyPatch(typeof(WindowStack), nameof(WindowStack.Add))]
[HarmonyPriority(Priority.Low)]
public static class H6_WindowStackAdd {
  static void Postfix(Window window) {
    try {
      if (window is not Dialog_NodeTree && window is not MainTabWindow_Quests) return;  // Settings/Pause/Floats rausfiltern
      var bot = Current.Game?.GetComponent<BotGameComponent>();
      bot?.EnqueueEvent(new QuestWindowEvent(window.GetType().Name));
    }
    catch (Exception ex) {
      if (!BotErrorBudget.CanLog()) return;
      Log.Error($"[RimWorldBot] H6_WindowStackAdd: {ex}");
      BotSafe.Get(() => Current.Game?.GetComponent<BotGameComponent>()?.FallbackToOff());
    }
  }
}
```

**Regeln (D-13):**
- Jeder Patch-Body in try/catch
- Exception-Log über `BotErrorBudget` (max 5/min, dann silent)
- `FallbackToOff()` selbst in `BotSafe.Get(…)` (Kaskaden-Schutz)
- Patches enthalten nur Enqueue-Logik und Typ-Filter, keine Analyse oder Entscheidung — die läuft im Tick-Host
- **Events speichern nur Identifikatoren** (Pawn-UniqueLoadID, Map-uniqueID, Window-Typ-Name), keine Live-Referenzen. Referenz-Auflösung erst im Dispatch im Tick-Host — mit Stale-Check (D-18)

### 4.2 Mod-Konflikt-Detektion

Konflikt-Pattern-Liste als **RimWorld-Def** `CompatPatternDef` in `Defs/CompatPatternDefs.xml` (RimWorld-idiomatisch, XML-Patch-bar durch User-Mods, **`DirectXmlLoader` parst automatisch** via RimWorld's Def-Pipeline — `Scribe_*` ist ausschließlich für Save-Load und NICHT hier relevant, F-RW4-01):

```xml
<Defs>
  <RimWorldBot.CompatPatternDef>
    <defName>RocketMan</defName>
    <match>Mlie.RocketMan</match>
    <kind>substring</kind>
  </RimWorldBot.CompatPatternDef>
  <RimWorldBot.CompatPatternDef>
    <defName>PerformanceFish</defName>
    <match>bs\.performance(\.fish)?</match>
    <kind>regex</kind>
  </RimWorldBot.CompatPatternDef>
  <RimWorldBot.CompatPatternDef>
    <defName>DubsPerformanceAnalyzer</defName>
    <match>Uuugggg\.DubsPerformanceAnalyzer</match>
    <kind>regex</kind>
  </RimWorldBot.CompatPatternDef>
</Defs>
```

`InitializeCompat()` wird **post-DefDatabase-Population** aufgerufen via eigene `[StaticConstructorOnStartup]`-Klasse — NICHT aus `RimWorldBotMod`-Konstruktor (F-RW4-02, sonst `DefDatabase` leer):

```csharp
[StaticConstructorOnStartup]
public static class CompatInitializer {
    static CompatInitializer() {
        // läuft nach vollständigem Def-Loading, garantiert DefDatabase populated
        RimWorldBotMod.Instance?.InitializeCompat();
    }
}
```

`Compile(TimeSpan)` ist eine **C#-Methode** auf der `CompatPatternDef`-Klasse (in der `Defs.cs`-Quelle der Mod), nicht XML-konfiguriert — XML liefert nur die Feld-Werte `match`/`kind`/`defName`.

In `InitializeCompat()`:

```csharp
var validDefs = DefDatabase<CompatPatternDef>.AllDefs
    .Where(d => d.match.Length <= 256 && !string.IsNullOrEmpty(d.match) &&
                (d.kind == "substring" || d.kind == "regex"))   // Schema-Whitelist hartkodiert
    .ToList();

if (validDefs.Count > 50) {
    Log.Warning($"[RimWorldBot] {validDefs.Count} CompatPatternDefs loaded, only first 50 evaluated — possible Def-bloat");   // F-STAB-24
}

var patterns = validDefs
    .Take(50)
    .Select(d => d.Compile(TimeSpan.FromMilliseconds(50)))   // Regex mit Timeout (ReDoS-Schutz)
    .ToList();

var hit = false;
foreach (var target in OurPatchTargets) {
  var owners = Harmony.GetPatchInfo(target)?.Owners.ToList() ?? new();
  foreach (var owner in owners) {
    if (patterns.Any(p => p.Matches(owner))) {
      hit = true;
      Log.Warning($"[RimWorldBot] {target} shared with {owner} — matched compat-pattern {p.DefName}");
    }
  }
}
if (hit) CompatMode.IsReduced = true;   // globales Flag: Tick-Intervall ×2
```

**Security (F-STAB-16):**
- `CompatPatternDef.Compile` verwendet `Regex(..., RegexOptions.Compiled, TimeSpan.FromMilliseconds(50))` → ReDoS-Timeout.
- `kind`-Whitelist hardcoded: nur `substring` oder `regex`. Andere Werte → WARN-Log + skip.
- `match`-Length-Limit 256 + max 50 Einträge → DoS-Resistenz auch bei manipulierten XML-Patches.
- RimWorld-Scribe-Loader handled XXE bereits intrinsisch (DTD-Processing prohibited by default).
- **XML-Patch-Resistenz:** Wenn ein Dritt-Mod die Liste via `Patches/`-Operations leert, greifen die hardcoded Compiled-Defaults als Fallback (eingebettet in `CompatPatterns.CompiledDefaults` — Drei Einträge minimum).

**Semantik:** `CompatMode.IsReduced` ist globaler Single-Set-Flag (Tick-Intervall-Änderung ist Tick-Host-weit, nicht per-Patch-Target). Session-scoped — kein Toggle-Zurück bei Mod-Unload. **Nicht in BotGameComponent persistiert** — wird bei jedem Mod-Load neu evaluiert (F-STAB-25). Garantiert dass Mod-Konfig-Änderungen zwischen Sessions (User aktiviert/deaktiviert RocketMan etc.) korrekt detektiert werden.

**Dynamische Konflikt-Liste:** Defs-basiert erlaubt (a) Ergänzung durch Mod-Release ohne Recompile, (b) Ergänzung durch User via `Patches/`-XML ohne Code-Änderung, (c) Schema-Validation durch RimWorld-DefDatabase.

**Nicht gepatcht** (bewusst, unverändert seit v1.0):
- `Verse.Pawn_JobTracker` → Bot nutzt `pawn.jobs.TryTakeOrderedJob()`
- `Verse.AI.JobDriver_*` → Bot arbeitet über `WorkGiver`-Prioritäten
- `Verse.DesignationManager` → Bot nutzt `Designator_Build.DesignateSingleCell()`

---

## 5. Save-Game-Strategie

### BotGameComponent (Game-global)

```csharp
public class BotGameComponent : GameComponent {
    const int CurrentSchemaVersion = 3;
    int schemaVersion = CurrentSchemaVersion;

    // --- Feld-Deklarationen (F-ARCH-15) ---
    BotController controller;
    BoundedEventQueue<BotEvent> eventQueue;
    ConfigResolver configResolver;
    // --------------------------------------

    public ToggleState masterState = ToggleState.Off;
    public int currentPhaseIndex = 0;
    public int pendingPhaseIndex = 0;         // für Zwei-Phasen-Commit
    public EndingStrategy endingStrategy = EndingStrategy.Opportunistic;
    public Ending? primaryEnding = null;
    public EndingCommitment endingCommitment = EndingCommitment.None;   // ab Phase 7 = Locked
    public int autoEscapeStableCounter = 0;   // F-AI-15, StableConsecutiveCounter für Ending-Escape
    public int ticksSincePhase7Entry = 0;     // inkrementiert im 2500er-Tick solange in Phase 7
    public int consecutiveSessionBudgetExhausts = 0;   // F-STAB-23, cross-session Crash-Counter
    public List<PhaseGoal> completedGoals = new();
    public Dictionary<string, bool> perPawnPlayerUse = new();   // UniqueLoadID → playerUse
    public RecentDecisionsBuffer recentDecisions = new(transientCap: 100, pinnedCap: 25);

    // Konstruktor: EventQueue VOR jeder Harmony-Patch-Möglichkeit initialisieren (F-ARCH-11, F-ARCH-15)
    public BotGameComponent(Game game) : base(game) {
        eventQueue = new BoundedEventQueue<BotEvent>(criticalCap: 32, normalCap: 224);
    }

    public override void ExposeData() {
        try {
            Scribe_Values.Look(ref schemaVersion, "schemaVersion", 1);
            if (schemaVersion < CurrentSchemaVersion) Migrate();

            Scribe_Values.Look(ref masterState, "masterState", ToggleState.Off);
            Scribe_Values.Look(ref currentPhaseIndex, "phaseIndex", 0);
            Scribe_Values.Look(ref pendingPhaseIndex, "pendingPhaseIndex", 0);
            Scribe_Values.Look(ref endingStrategy, "endingStrategy", EndingStrategy.Opportunistic);
            Scribe_Values.Look(ref primaryEnding, "primaryEnding");
            Scribe_Values.Look(ref endingCommitment, "endingCommitment", EndingCommitment.None);
            Scribe_Values.Look(ref autoEscapeStableCounter, "autoEscapeStableCounter", 0);
            Scribe_Values.Look(ref ticksSincePhase7Entry, "ticksSincePhase7Entry", 0);
            Scribe_Values.Look(ref consecutiveSessionBudgetExhausts, "consecutiveSessionBudgetExhausts", 0);
            Scribe_Collections.Look(ref completedGoals, "completedGoals", LookMode.Deep);
            Scribe_Collections.Look(ref perPawnPlayerUse, "perPawnPlayerUse", LookMode.Value, LookMode.Value);
            Scribe_Deep.Look(ref recentDecisions, "recentDecisions");
        }
        catch (Exception ex) {
            Log.Error($"[RimWorldBot] ExposeData failure, falling back to defaults: {ex}");
            ResetToDefaults();
        }
    }

    void Migrate() { /* schemaVersion 1 → 2 → 3 … */ }

    // Composition-Root: einmalig am Game-Init (beide Lifecycle-Pfade)
    public override void FinalizeInit() {
        BuildController();   // Factory verdrahtet alle Kollaborateure; eventQueue bereits im ctor initialisiert
    }

    void BuildController() {
        try {
            var snapshotProvider = new RimWorldSnapshotProvider();
            configResolver = new ConfigResolver(this, LearnedConfig.LoadOrDefault(), Configuration);
            controller = BotControllerFactory.NewBuilder()
                .With<ISnapshotProvider>(snapshotProvider)
                .With<ConfigResolver>(configResolver)
                .With<BoundedEventQueue<BotEvent>>(eventQueue)
                .With<MapAnalyzer>(new MapAnalyzer(snapshotProvider))
                .With<EmergencyResolver>(new EmergencyResolver(configResolver))
                .With<PhaseStateMachine>(new PhaseStateMachine(configResolver))
                .With<BuildPlanner>(new BuildPlanner())
                .With<BillPlanner>(new BillPlanner())
                .With<WorkPlanner>(new WorkPlanner())
                .With<GoalHealthMonitor>(new GoalHealthMonitor())
                .With<TelemetryLogger>(TelemetryLogger.GetOrCreate())
                .Build();
        }
        catch (Exception ex) {
            // F-STAB-26: Factory-Failure soll nicht permanent null lassen
            Log.Error($"[RimWorldBot] BuildController failed: {ex}");
            controller = null;
            masterState = ToggleState.Off;
            Messages.Message("RimWorldBot disabled — Controller initialization failed. See log.",
                MessageTypeDefOf.RejectInput, false);
        }
    }

    // Post-Load only (F-AI-08, F-STAB-10, F-STAB-20, F-STAB-23, CC-3-01, CC-4-03)
    public override void LoadedGame() {
        eventQueue.Clear();                   // F-STAB-20: alte Tick-Stamps verwerfen
        configResolver?.Invalidate();         // Cache-Reset nach Load
        if (controller == null) BuildController();   // F-STAB-26: Retry nach vorherigem Factory-Failure
        ReconcilePendingPhase();
        ReconcilePhaseGoalOrphans();
        CheckBudgetExhaustHistory();          // F-STAB-23
    }

    public override void StartedNewGame() {
        eventQueue.Clear();
        configResolver?.Invalidate();
        if (controller == null) BuildController();
        // Initial-State, kein Reconcile nötig (pending == current == 0)
    }

    void CheckBudgetExhaustHistory() {
        if (consecutiveSessionBudgetExhausts >= 3) {
            Messages.Message("RimWorldBot disabled itself repeatedly across sessions — consider reporting a bug.",
                MessageTypeDefOf.CautionInput, false);
        }
    }

    void ReconcilePendingPhase() {
        if (pendingPhaseIndex != currentPhaseIndex) {
            recentDecisions.AddTransient(new DecisionLogEntry(
                kind: "crash-recovery-phase-rollback",
                reason: $"pending={pendingPhaseIndex} != current={currentPhaseIndex}"));
            recentDecisions.AddPinned(/* same entry for long-retention */);
            pendingPhaseIndex = currentPhaseIndex;
        }
    }

    void ReconcilePhaseGoalOrphans() {
        // F-STAB-10 + F-STAB-21 (RESOLVED via D-25 Tag-Schema §2.3b):
        // Iteriert botPlacedThings + botAssignedJobs in jedem BotMapComponent,
        // cancelt was tag.PhaseIndex > currentPhaseIndex oder zu canceled GoalId gehört.
        foreach (var map in Find.Maps) {
            var botMap = map.GetComponent<BotMapComponent>();
            botMap?.CancelOrphanedDesignations(currentPhaseIndex);
            botMap?.CancelOrphanedJobs(currentPhaseIndex);
        }
    }
}
```

### BotMapComponent (per Map)

```csharp
public class BotMapComponent : MapComponent {
    public MapAnalysisSummary analysisSummary;   // schlank: Top-3-Sites + Scores, KEIN Per-Cell
    public int phaseProgressTicks;
    public int stableCounter;                    // F-AI-08: konsekutive 60er-Ticks ohne Violation (nur Home-Map zählt, F-AI-18)

    // D-25 Goal-Phase-Tag-Dicts (F-STAB-21)
    public Dictionary<int /*thing.thingIDNumber*/, PhaseGoalTag> botPlacedThings = new();
    public Dictionary<int /*job.loadID*/, PhaseGoalTag> botAssignedJobs = new();

    public override void ExposeData() {
        try {
            int version = 2;
            Scribe_Values.Look(ref version, "schemaVersion", 2);
            Scribe_Deep.Look(ref analysisSummary, "analysisSummary");
            Scribe_Values.Look(ref phaseProgressTicks, "phaseProgressTicks", 0);
            Scribe_Values.Look(ref stableCounter, "stableCounter", 0);
            Scribe_Collections.Look(ref botPlacedThings, "botPlacedThings", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref botAssignedJobs, "botAssignedJobs", LookMode.Value, LookMode.Deep);
        }
        catch (Exception ex) {
            Log.Error($"[RimWorldBot] BotMapComponent.ExposeData failure: {ex}");
            analysisSummary = null;   // triggers Re-Scan on next tick
            stableCounter = 0;
            botPlacedThings ??= new();
            botAssignedJobs ??= new();
        }
    }

    public void CancelOrphanedDesignations(int currentPhaseIndex) {
        var orphans = botPlacedThings.Where(kv => kv.Value.PhaseIndex > currentPhaseIndex).ToList();
        foreach (var (thingId, _) in orphans) {
            // Lookup + Cancel Designation + Remove Blueprint wenn noch existiert
            var thing = map.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == thingId);
            if (thing != null && !thing.Destroyed) {
                if (thing is Blueprint bp) bp.Destroy(DestroyMode.Cancel);
                map.designationManager.RemoveAllDesignationsOn(thing);
            }
            botPlacedThings.Remove(thingId);
        }
    }

    public void CancelOrphanedJobs(int currentPhaseIndex) {
        var orphans = botAssignedJobs.Where(kv => kv.Value.PhaseIndex > currentPhaseIndex).ToList();
        foreach (var (jobId, _) in orphans) {
            // Alle Pawns durchgehen, job cancel wenn loadID matcht
            foreach (var pawn in map.mapPawns.AllPawns) {
                if (pawn.jobs?.curJob?.loadID == jobId) {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
            botAssignedJobs.Remove(jobId);
        }
    }
}
```

### perPawnPlayerUse-Cleanup (alle 60000 ticks) — defensiv gegen Map-Dispose-Races (F-STAB-19)

```csharp
var validIds = Find.Maps
    .Where(m => m?.mapPawns != null)
    .SelectMany(m => m.mapPawns.AllPawns?.ToList() ?? Enumerable.Empty<Pawn>())
    .Where(p => p != null && !p.Destroyed)
    .Select(p => p.GetUniqueLoadID())
    .ToHashSet();
var stale = perPawnPlayerUse.Keys.Where(id => !validIds.Contains(id)).ToList();
foreach (var id in stale) perPawnPlayerUse.Remove(id);
```

### Kompatibilitäts-Garantie

- Savegame ohne Mod ladbar (GameComponent-Daten werden beim Load verworfen, Vanilla-Path funktional)
- Schema-Migration von v1 → v2 → … in `Migrate()`; bei irreparabler Inkonsistenz: `ResetToDefaults()` + User-Toast
- `Scribe_Deep` nur auf `IExposable`-Klassen mit eigenem try/catch

---

## 5a. Configuration Resolution (neu)

**Drei Konfigurations-Quellen + eindeutige Precedence:**

```
BotGameComponent  >  LearnedConfig  >  Configuration (ModSettings)  >  Compiled Defaults
    (Save-scoped)    (User-scoped)     (User-global)                     (Code)
```

**Single-Entry:** `ConfigResolver.Get<T>(ConfigKey key)` fragt in dieser Reihenfolge ab, erstes non-null gewinnt. Beispiel:

```csharp
double foodWarningThreshold = ConfigResolver.Get<double>(ConfigKeys.FoodDaysWarning);
//  prüft BotGameComponent.overrides → LearnedConfig.Thresholds.FoodDaysWarning →
//        Configuration.FoodDaysWarning → Defaults.FoodDaysWarning (= 11)
```

**Cache-Strategie (F-ARCH-08, F-ARCH-14 typisiert):**
```csharp
// Typisierter Key vermeidet Cast-Fehler zur Laufzeit (F-ARCH-14)
public sealed class ConfigKey<T> {
    public readonly string Name;
    public ConfigKey(string name) => Name = name;
}

public static class ConfigKeys {
    public static readonly ConfigKey<double> FoodDaysWarning = new("FoodDaysWarning");
    public static readonly ConfigKey<double> MoodCritical = new("MoodCritical");
    public static readonly ConfigKey<double> HysteresisMargin = new("HysteresisMargin");
    // ... weitere Keys
}

class ConfigResolver {
    readonly Dictionary<object /*ConfigKey<T>*/, object> _cache = new();

    public T Get<T>(ConfigKey<T> key) {
        if (_cache.TryGetValue(key, out var cached)) return (T)cached;
        var resolved = Clamp<T>(key, ResolveFromSources<T>(key));
        _cache[key] = resolved;
        return resolved;
    }

    public void Invalidate() => _cache.Clear();
}
```

**Compile-Time-Safety:** `Get(ConfigKeys.FoodDaysWarning)` liefert garantiert `double`. Typ-Mismatch zwischen Key und Call-Site wird vom Compiler abgefangen.

**Invalidate-Hooks:**
- `Mod.WriteSettings()` (ModSettings geändert) → Mod-Subclass ruft `configResolver.Invalidate()`
- `BotGameComponent.LoadedGame()` → Invalidate (Save kann andere Overrides bringen)
- `BotGameComponent.StartedNewGame()` → Invalidate

**Sanity-Clamp** in jeder Quelle: Weights `[0,1]`, FoodDays `[1,30]`, MoodCritical `[0.1,0.9]`. Out-of-range-Werte beim Lesen geclamped + WARN-Log.

---

## 6. Lern-System (Cross-Game-Persistenz)

### 6.1 Storage

**Pfad:** `GenFilePaths.ConfigFolderPath + "/RimWorldBot/"` (cross-platform, Windows/macOS/Linux/Proton).

**Dateien:**
- `learned-config.xml` — aktuelle Weights, Thresholds, OverrideLibrary, Statistics
- `learned-config.xml.bak` — letzte erfolgreich geparste Version (Rollback bei Corruption)

**Write-Pattern (F-STAB-17 + F-STAB-22 gehärtet):**
1. Mutex-Name mit **Pfad-Hash** statt nur Suffix: `Local\RimWorldBot_{SHA1(ConfigFolderPath).Substring(0,16)}_v2`. Bindet Mutex an die konkrete XML-Datei, nicht an User-Naming → safe bei Family-Sharing/Multi-User/Mod-Version-Upgrade.
2. Acquire Mutex. Bei `AbandonedMutexException` (vorheriger Writer crashed): Integrity-Check auf `learned-config.xml` (XML-parse-Probe) — bei Fehler rollback auf `.bak`, sonst weitermachen.
3. **Orphan-tmp-Cleanup (F-STAB-22):** `if (File.Exists(tmpPath)) File.Delete(tmpPath);` — Überbleibsel eines crashed Vorgänger-Writers entfernen bevor `CreateNew` versucht wird.
4. Zweite Verteidigungslinie: `new FileStream(tmp, FileMode.CreateNew, FileAccess.Write, FileShare.None)`. Bei `IOException` (anderer Writer im Rennen): Retry mit Backoff 3 × 100 ms, dann Skip-Write + WARN-Log.
5. Schreibe `learned-config.xml.tmp` (neue Werte).
6. Atomic rename `learned-config.xml` → `learned-config.xml.bak`.
7. Atomic rename `learned-config.xml.tmp` → `learned-config.xml`.
8. **`finally`-Block (F-STAB-22):** `if (File.Exists(tmpPath)) File.Delete(tmpPath);` — cleanup falls Rename zwischen 6 und 7 crashed; plus Release Mutex.

**Read-Pattern:**
1. Parse `learned-config.xml` → bei Fehler: rename in `learned-config.corrupt-<unix-ts>.xml`, User-Toast, lade aus `.bak`
2. Bei auch `.bak` corrupt: lade Compiled Defaults, User-Toast „Lern-Daten zurückgesetzt — Backup in Config-Ordner gesichert"

### 6.2a Layer-Interaktion: Emergency > Override > PhaseGoal (AI-7, F-AI-13, D-22)

Drei Decision-Layer können Pawn-Assignments beanspruchen. **Strikte Präzedenz** (AI-7-Invariante):

1. **EmergencyResolver** (§2.1) — höchste Priorität, wählt bei aktiven Invariant-Violations. Override darf Emergency **niemals** überstimmen.
2. **OverrideLibrary** — greift nur wenn (a) EmergencyResolver keine Emergency aktiv hat UND (b) aktueller `SituationHash` exakt einem OverrideLibrary-Eintrag matcht.
3. **PhaseGoals** — Default-Pfad wenn weder Emergency noch Override greift.

Override-`Confidence` beeinflusst **nur** Tie-Break zwischen mehreren match-enden OverrideLibrary-Einträgen (und zwischen Override vs. Default-PhaseGoal-Verhalten). Confidence wird **nie** gegen Emergency-Utility-Score gerechnet.

### 6.2 Schema (XML)

```xml
<LearnedConfig>
  <schemaVersion>2</schemaVersion>

  <!-- Weights per-Biome partitioniert -->
  <BiomeWeights biome="TemperateForest">
    <sampleCount>12</sampleCount>
    <W_FOOD>0.26</W_FOOD>
    <W_DEFENSE>0.23</W_DEFENSE>
    <!-- ... -->
  </BiomeWeights>
  <BiomeWeights biome="IceSheet">
    <sampleCount>3</sampleCount>
    <!-- sampleCount < 5: liest Default aus Compiled Defaults -->
  </BiomeWeights>

  <Thresholds>
    <FoodDaysWarning>11</FoodDaysWarning>
    <MoodCritical>0.37</MoodCritical>
  </Thresholds>

  <OverrideLibrary>
    <Override id="raid_small_killbox_approach">
      <SituationHash>abcdef123456</SituationHash>
      <PreferredAction>DraftAllCombatPawns_RetreatToKillbox</PreferredAction>
      <Confidence>0.7</Confidence>
      <SuccessCount>3</SuccessCount>
      <RejectCount>1</RejectCount>
    </Override>
  </OverrideLibrary>

  <Statistics>
    <TotalRuns>12</TotalRuns>
    <EndingsReached biome="TemperateForest" difficulty="Medium">
      <Ship>3</Ship>
      <Journey>5</Journey>
    </EndingsReached>
    <!-- Nur IDs, niemals pawn.Name -->
  </Statistics>
</LearnedConfig>
```

### 6.3 Update-Logik (revidiert)

**Konvergente Sample-Count-Formel (ersetzt einfaches Bayesian):**
```
new_weight = old_weight + (observed - old_weight) / (sampleCount + k)
// k = 20 (Bayesian-Prior). Bei konstantem `observed` garantiert konvergent.
```

**Per-Biome-Overfitting-Schutz:** sampleCount < 5 → Weights bleiben auf Compiled Default, keine Drift erlaubt.

**Ending-Feasibility mit Hysterese (F-AI-03, F-AI-10, F-AI-11):**
```
HYSTERESIS_MARGIN = 0.15

// Normalisiert auf [0, 0.5] — garantiert dass Switch bei new > cur + 0.65 immer möglich
sunk_cost_penalty(ending) = clamp(resources_invested(ending) / max_resources_for_ending(ending), 0, 0.5)

on_feasibility_reevaluate():
    candidate = argmax(feasibility[e] for e in available_endings)
    if primary == null: primary = candidate; return

    if endingCommitment == Locked:   // ab Phase 7, aber mit Escape-Hatches:
        if endingStrategy == Forced:                                          # Escape 1: User-Override
            primary = user_forced_ending
            log_decision("ending_forced_override")
            autoEscapeStableCounter = 0
            return

        # Escape 2 (F-AI-15 korrekt Reset-Semantik via D-26 StableConsecutiveCounter):
        # Counter inkrementiert nur wenn beide Bedingungen in diesem Reeval erfüllt.
        # Reset auf 0 sobald eine Bedingung bricht.
        escape_condition_met = feasibility[candidate] >= 0.95 and feasibility[primary] <= 0.30
        if escape_condition_met:
            autoEscapeStableCounter += 1
        else:
            autoEscapeStableCounter = 0

        # 2 konsekutive Reevals (= 5000 Ticks bei 2500er-Kadenz)
        if autoEscapeStableCounter >= 2:
            log_decision("ending_auto_escape_stranded", primary → candidate,
                         reason=f"candidate={feasibility[candidate]:.2f} current={feasibility[primary]:.2f} for {autoEscapeStableCounter} consecutive reevals")
            primary = candidate
            autoEscapeStableCounter = 0
            return

        return   // sonst: Lock hält, keine Flips

    new_score = feasibility[candidate]
    cur_score = feasibility[primary]
    if new_score > cur_score + HYSTERESIS_MARGIN + sunk_cost_penalty(primary):
        log_decision("ending_switch", primary → candidate, reason)
        primary = candidate
```

**Phase-7-Entry setzt `endingCommitment = Locked` + `ticksSincePhase7Entry = 0`.** Im Locked-State greifen Switch-Bedingungen:
1. **Game-Breaking-Event** (Reaktor zerstört, Monolith weg, Imperium feindlich) — als explizites Event im Tick-Flow, nicht über Feasibility.
2. **User-Forced-Override** (`endingStrategy == Forced` via ModSettings-Dropdown) — bricht Lock sofort mit Log-Eintrag.
3. **Stranded-Auto-Escape**: `autoEscapeStableCounter ≥ 2` (2 konsekutive Reevals mit `candidate ≥ 0.95` UND `current ≤ 0.30`, D-26-Pattern) — verhindert Truly-Stranded-Bot ohne User-Intervention.

**Auto-Release bei Phase-Regression (F-AI-17):** Wenn `PhaseStateMachine.TransitionBackward(7 → 6)` feuert (z. B. nach Game-Breaking-Event: Reaktor-Hull zerstört, Phase-6-Goal wieder offen), setzt der Transition-Handler `endingCommitment = None`, `autoEscapeStableCounter = 0`, `ticksSincePhase7Entry = 0`. DecisionLog-Eintrag `"ending_commitment_released_phase_regression"` (auto-pinned). Bei erneutem Phase-7-Entry wird Commitment neu gesetzt.

### 6.4 Security / Integrity

- User-Edit der XML: beim Load Sanity-Clamp auf alle Werte, Out-of-range → Clamp + WARN-Log (siehe §5a)
- Keine Signatur (lokale Datei, GDPR-irrelevant), aber `.corrupt-<ts>`-Backup bei Parse-Fehler verhindert stillen Datenverlust
- Parallele RimWorld-Instanzen: Mutex serialisiert Writes

---

## 7. Error-Handling-Strategie

### 7.1 Exception-Policy

**Zwei Ebenen von try/catch:**

1. **Tick-Host-try** (in `GameComponentTick`): fängt alles aus Decision/Execution, führt `FallbackToOff()`.
2. **Patch-Body-try** (in jedem Harmony-Postfix): fängt Exceptions vor Propagation in Vanilla-Caller, über `BotErrorBudget` rate-limited.

```csharp
// Tick-Host
public override void GameComponentTick() {
    if (!_initialized) return;
    try { controller.Tick(); }
    catch (Exception ex) {
        if (BotErrorBudget.CanLog()) Log.Error($"[RimWorldBot] Tick: {ex}");
        BotSafe.Get(() => FallbackToOff());
    }
}
```

**Besondere Exceptions:**
- `StackOverflowException`, `OutOfMemoryException`, `ThreadAbortException` sind **nicht** fangbar → präventive Soft-Self-Disable via GC-Monitoring (siehe §8).
- `FallbackToOff()` selbst ist minimal (`masterState = Off;` + Return) und zusätzlich in `BotSafe.Get(…)` eingehüllt.

### 7.2 Non-fatal Handling

**Goal-Retry-Policy (F-STAB-05):**
- Jedes Goal hat `retryCount` + `lastAttempt`
- Nach 3 Fails in < 60 s: Goal → `poisonedGoals`-Set, Phase-Evaluator überspringt
- Nach 10 min: Poison-Unlock, erneuter Versuch
- Tick-Cap: max 100 Goal-Evaluations pro Tick

**Regression-Detector (F-AI-07):**
- Pro erreichtem Exit-Cond alle 5000 Ticks Re-Check
- Bei Regression: Goal in Phase-Goal-Liste vor allen offenen Goals eingereiht
- `GoalHealthScore` < 0.7 → Priority-Flush: niedriger-prio Goals pausiert bis Score > 0.85

**`GoalHealthScore`-Formel (F-AI-12, verfeinert F-AI-16):**
- **Quantitative Exit-Conds** (z. B. `food_stock_days ≥ 90`): `score = clamp(current_metric / target_metric, 0, 1)`. Beispiel: `food_stock_days = 63, target = 90 → score = 0.7`.
- **Boolean Exit-Conds** (z. B. `has_shelter = true`): `score = 1.0` wenn erfüllt, sonst `0.0`. Plus **Decay-Buffer 250 Ticks** gegen Flackern: wenn Boolean-Cond erst vor < 250 Ticks geflippt ist, bleibt der alte Score gültig.
- **Launch-Critical-Klassifikation pro Exit-Cond (F-AI-16):** Jede Exit-Cond trägt ein Flag `launchCritical: bool` in ihrer Definition. Launch-critical sind typischerweise Survival-Conds (Shelter, Food, Defense, Temp, Bleed-Risk); non-critical sind Quality-of-Life-Conds (Comfort-Research, Beauty, Ideology-Adherence). Die Klassifikation ist pro Phase im `PhaseDefinition`-Code hart-verdrahtet.
- **Aggregation pro Phase:** `phase.GoalHealthScore = min(score across launch-critical exit-conds only)`. Non-critical Conds sind für den Score irrelevant — ein schwaches Comfort-Research löst keinen Priority-Flush mehr aus. Vermeidet Kaskaden bei Saison-driven Rand-Werten orthogonaler Conds.
- Threshold-Paar `0.7 / 0.85` ist bewusst mit 15-Punkte-Hysterese, verhindert Prio-Flush-Flackern bei Rand-Werten launch-critical Conds.

### 7.3 Defensive-Annahmen (erweitert)

- Pre-Tick-Check: `Current.Game != null && Current.Game.CurrentMap != null`
- Pawn-Zugriff: `pawn != null && pawn.Spawned && !pawn.Dead`
- Map-Zugriff: `map != null && map.IsPlayerHome && map.thingGrid != null`
- Thing-Zugriff: `thing != null && !thing.Destroyed`
- Def-Zugriff: `ThingDef` via `Lazy<ThingDef>`-Cache mit null-safe re-resolve
- Faction-Zugriff: `Faction.OfPlayer != null` (Anomaly-Szenarien)
- Mod-Reload-Resilienz: `BotController.ShutdownHooks()` in `Game.FinalizeInit` resettet State
- Utility: `BotSafe.Get<T>(Func<T> accessor)` → catch-all, null-bubble

### 7.4 Stuck-State-Handling (F-AI-04)

- **I0 Colony-Extinct**: `colonist_count == 0` → Handler `E_EXTINCT` setzt Bot auf OFF, UI-Banner „Colony extinct", stoppt alle Tick-Arbeit
- **Phase 0a ohne gültige Sites**: wenn nach FullScan `top_3_sites.empty` → Fallback-Modus „best-available-with-warning" (nimm Top-1 ohne Hard-Filter, Toast an User)
- **Exit-Cond strukturell unerreichbar** (z. B. Crafting-Passion-Pawn tot): Jede Phase definiert `FallbackOnUnreachable()` → entweder Grind-Fallback oder Ending-Switch-Trigger

---

## 8. Performance-Budget

### Tick-Zeit (Ziel)

| Operation | Max ms | Häufigkeit |
|---|---|---|
| Invariant-Checks (12 + I0) | 1.5 | jeden 60er-Tick |
| EmergencyResolver.Score + Apply | 5.0 | bei Trigger |
| Phase-Evaluation | 2.0 | jeden 60er-Tick |
| WorkPlanner + WorkPriorityWriter | 3.0 | alle 2500 Ticks |
| MapAnalyzer-FullScan | 500 total | Event-triggered, tick-budgeted-iterator @ 2 ms/Tick |
| UI-Refresh | 0.5 | jeden 60er-Tick |
| **Durchschnitt pro Tick** | **< 5.0** | — |
| **Maximum pro Tick** | **< 20.0** | — |

**„Coroutine" Begriff entfernt** — korrekte Terminologie: **tick-budgeted iterator** via `IEnumerator` in `MapComponentTick` mit 2 ms-Budget.

### Memory

- Bot-Runtime-Objekte: < 10 MB
- BotGameComponent + BotMapComponent kombiniert: < 100 KB
- LearnedConfig.xml: < 5 MB
- Telemetry-Log: 3 × 10 MB = 30 MB hard cap (Rotation bei 10 MB, keep-3)

### GC-Monitoring (F-STAB-02, F-STAB-13, F-STAB-14, F-RW3-04)

**Relativer Baseline-Vergleich mit sicherem Timing** (verhindert False-Positives bei Modded-Colonies mit legitim hoher Heap-Nutzung UND Race bei Load-direkt-nach-Mod-Enable):

- **Baseline-Messung** verschoben auf **ersten Maintenance-Tick nach mindestens 5000 Ticks Spielzeit seit `FinalizeInit`** — zu diesem Zeitpunkt sind Map-Caches/Texture-Allocs/Trader-State stabilisiert, Late-Game-Saves direkt nach Mod-Enable werden nicht fälschlich auf leere Baseline festgenagelt.
- GC-Monitoring ist **bis zur Baseline-Messung deaktiviert**. Zeitfenster: min. 5000 Ticks = ~83 in-game Minuten.
- Initiale Messung einmalig mit `GC.GetTotalMemory(true)` (forced-GC für genaue Zahl, auf Maintenance-Tick läuft ohnehin Budget → kein sichtbarer Frame-Stutter).
- Floor: `baseline = max(GC.GetTotalMemory(true), 100 MB)` — unter 100 MB kein Monitoring, vermeidet zu strenge Limits bei sehr kleinen Colonies.
- Periodische Samples alle 2500 Ticks: `current = GC.GetTotalMemory(false)` (approximativ, billig — kein Force).
- Trigger: wenn `current > baseline * 5` für ≥ 3 konsekutive Samples → `FallbackToOff()` + Log mit Baseline/Current-Werten.
- **Re-Baseline** alle 60 × 2500 Ticks (1× täglich in-game) mit `GC.GetTotalMemory(false)` (kein Force auf Re-Baseline, akzeptable Approximation) — fängt langsam gewachsene Colony-Größe ab.

Dies adressiert Memory-Leaks im Bot-Code und schützt gleichzeitig vor False-Positives bei legitim großen Late-Game-Colonies sowie bei Mod-Enable-Load-Race.

---

## 9. Testing-Strategie

### 9.1 Unit-Tests (pure, ohne RimWorld-Runtime)

Möglich durch Snapshot-DTOs (§2.2). **Test-Helper:** `FakeSnapshotProvider` (in-memory Implementation von `ISnapshotProvider`), `TestSnapshotBuilder` (Fluent-API zum Konstruieren von `CellSnapshot[]`/`ColonySnapshot`/`PawnSnapshot` aus Code) — beide im Test-Assembly.

- `MapAnalyzer.ScoreCell(CellSnapshot) → double` — POCO-Input, berechenbarer Score
- `EndingFeasibility.Score(ColonySnapshot) → double`
- `EndingFeasibility.HysteresisGate(current, candidate, sunkCost) → bool` — inkl. **Edge-Case „switch-still-possible-at-extreme-sunk-cost"** (sunkCost=0.5, margin=0.15, new > cur + 0.65)
- `EndingFeasibility.StrandedAutoEscape(current, candidate, ticksSincePhase7) → bool` — Escape-Hatch-Logik (F-AI-11)
- `PhaseDefinition.AreExitConditionsMet(ColonySnapshot) → bool`
- `PhaseStateMachine.StableCounterReset(…)` — Counter resetet bei Emergency, zählt monoton sonst
- `EmergencyResolver.Resolve(InvariantResult[], ColonySnapshot) → EmergencyChoice` — Test der Utility-Matrix + Modifier-Kombinationen
- `OverrideEvaluator.CompareOutcomes(Snapshot a, Snapshot b) → double`
- `LearnedConfig.SampleCountUpdate(double old, double observed, int count) → double` — Konvergenz-Tests
- `GoalHealthScore(…)` — quantitative + boolean + Decay-Buffer (F-AI-12)
- `BoundedEventQueue<T>` — Drop-Oldest-Verhalten, Critical/Normal-Separation, Stale-Dispatch, enqueueTick-Ordering
- Plan-Records Value-Equality-Tests (`BillPlan`, `WorkPriorityPlan`, `DraftOrder`, `BuildPlan`, `CaravanPlan`)

### 9.2 Integration-Tests (mit RimWorld-Harness)

- `RimWorldSnapshotProvider` — Mapping `Map → CellSnapshot[]` korrekt
- Harmony-Patch-Application in Isolation
- Schema-Migration `LearnedConfig v1 → v2`

### 9.3 In-Game-Validation

- **TC-01:** Fresh-Start Naked Brutality → Phase 0+1 autonom
- **TC-02:** Raid während Phase 1 → CombatCommander aktiviert
- **TC-03:** Journey-Offer bei Phase 3 → auto-accept
- **TC-04:** Mod toggle während laufender Session
- **TC-05:** Pawn-Player-Use-Flag: 2 Pawns manuell, 2 Bot
- **TC-06:** Alle DLC-Kombinationen + **Top-10-Mods** (RocketMan, Performance Fish, Common Sense, Better Pawn Control, Work Tab, Dubs Performance Analyzer, Allow Tool, Combat Extended, Vanilla Expanded Core, Harmony) laden parallel ohne Fehler
- **TC-07:** Savegame-Load nach Mod-Disable
- **TC-08 (neu):** Schema-Migration `v1 → v2` Savegame

---

## 10. Build & Distribution

### 10.1 Build-Setup

```
Source/
├── RimWorldBot.csproj       (.NET Framework 4.7.2)
├── Core/
├── Snapshot/                (neue Schicht)
├── Analysis/
├── Decision/
├── Execution/
├── UI/
├── Data/
└── Compat/                  (neu: DlcCapabilities, CompatMode)
```

### 10.2 Mod-Paket

```
RimWorldBot/
├── About/
│   ├── About.xml             (Name, Version, Author, SupportedVersions)
│   └── Preview.png
├── Assemblies/
│   └── RimWorldBot.dll
├── Defs/
│   ├── MainButtonDefs.xml    (Toggle-Button via MainButtonDef — siehe Snippet unten)
│   ├── ITabDefs.xml          (ITab_Pawn_BotControl Registration)
│   ├── KeyBindingDefs.xml    (Ctrl+K)
│   └── CompatPatternDefs.xml (Mod-Konflikt-Patterns, siehe §4.2)
├── Patches/
│   └── HumanInspectTabs.xml  (XML-Patch: ITab_Pawn_BotControl zu Human.inspectorTabs)
├── Languages/
│   ├── Deutsch/Keyed/...
│   └── English/Keyed/...
└── LoadFolders.xml
```

**`Defs/MainButtonDefs.xml` Pflichtfelder (F-RW4-04):**
```xml
<Defs>
  <MainButtonDef>
    <defName>BotControl</defName>
    <label>Bot</label>
    <description>RimWorld Bot — autonome Entscheidungs-KI</description>
    <tabWindowClass>RimWorldBot.MainTabWindow_BotControl</tabWindowClass>
    <order>99</order>             <!-- am rechten Rand der Top-Bar -->
    <defaultHidden>false</defaultHidden>
    <iconPath>UI/Buttons/BotIcon</iconPath>
    <minimized>false</minimized>
  </MainButtonDef>
</Defs>
```

### 10.3 GitHub-Release-Workflow

- `main` stabil, taggbar · `develop` aktive Dev · `feature/epic-NN-*` pro Epic
- Tags: `v0.1.0-mvp`, `v0.2.0-alpha`, `v0.3.0-beta`, `v1.0.0-release`
- GitHub-Actions:
  1. Pre-Release-Lint: `About.xml/supportedVersions` ↔ `LoadFolders.xml/li[@IfModActive]` Konsistenz-Check (Skript im Repo)
  2. Build auf Tag
  3. ZIP-Artifact + **SHA256SUMS-File** als Release-Asset
  4. Token-Scope: `contents:write` only

---

## 11. Sicherheits- und Kompatibilitätshinweise

- **Keine Destructive-Harmony-Patches** — ausschließlich Prefix/Postfix, keine Transpiler
- **Reflection minimieren** — nur wo Harmony unzureichend
- **Keine File-IO im Tick-Loop** — nur in Event-Queues oder Maintenance-Ticks (60000er)
- **Keine Async/Threads** — RimWorld Single-Threaded
- **Keine eigenen Incidents/Quests** — nur Vanilla-Events abonnieren
- **Keine Xenotype/Gene-Modification** — Bot interagiert, verändert nicht
- **Telemetry default OFF**, opt-in via ModSettings (F-STAB-07)
- **Keine PII in Logs** — `pawn.Name` niemals, stattdessen `pawn.GetUniqueLoadID()`
- **DLC-Feature-Detection via `DlcCapabilities`** — alle DLC-Defs mit `[MayRequire*]`, `DlcCapabilities.AvailableEndings()` filtert EndingFeasibility
- **Mod-Konflikt-Erkennung** beim Start (§4.2) + Reduced-Mode-Fallback

---

## 12. Offene technische Fragen (aktualisiert)

- ~~**TQ-01:** Genaue TerrainDef-defNames für Lava, Pollution, Toxic-Terrain~~ → verschoben in Story 2.3 (Hazard-Scanner)
- ~~**TQ-02:** Harmony-Patch-Priority bei Konflikten mit Common Sense / Better Pawn Control~~ → **aufgelöst in §4 + §4.2** (`Priority.Low` + Runtime-Konflikt-Scan + Reduced-Mode)
- ~~**TQ-03:** Storage-Größe-Limits für LearnedConfig.xml~~ → aufgelöst in §6.4 (Mutex + atomic-write, keine Size-Limit nötig)
- ~~**TQ-04:** UI-Localization embedded oder XML~~ → XML-only Entscheidung (Story 1.8, keine C#-embedded Strings)
- **TQ-05:** Performance-Profiling-Toolchain → Dev-Mode-Stats + custom `[PerfCounter]`-Attribut-Kombi, finale Auswahl in Story 8.7 (Debug-Panel)

---

## 13. Acceptance / Sign-Off

**Reviewer:** Party-Mode (4 Personas) + User als Projekt-Eigentümer
**Status:** Revised after Party-Mode-Review 2026-04-24. Awaiting Re-Review durch mind. 2 Personas (RimWorld-Specialist + Stability, laut Report §Sign-Off-Anforderungen).

**Vor finalem Approval:**
1. Re-Review bestätigt alle 7 CRITs + 9 HIGHs aus Round 1 adressiert
2. User-Sign-Off nach Re-Review-PASS
3. Decisions D-11 bis D-17 persistiert in `decisions.md`

**Nach Approval:** Story-Drafting für Epic 1–3 (MVP-Kern) startet.

---

## 14. Anhang: Invariant-Katalog-Referenz

Die 12 Invariants I1–I12 sind in `Mod-Leitfaden.md §2` detailliert; I0 ist neu in v2.0:

- **I0** — Colony-Extinct: `colonist_count > 0` — Handler `E_EXTINCT` (F-AI-04)
- **I1–I5** — Shelter, Food, Bleed, Fire, Temp (MVP-Core, Epic 3)
- **I6–I12** — Mood, Health, Mental-Break, Raid-Defense, Food-Days (3/Pawn), Medicine, Pawn-Sleep (Alpha+, Epic 4)

Invariants sind idempotent (gleicher Input → gleicher Output, keine Side-Effects im Check).

**Architektur-Invarianten (AI-1 bis AI-7) siehe §1.** AI-7 neu in v2.2: Decision-Layer-Präzedenz Emergency > Override > PhaseGoal.

---

## 15. Change-Log

- **v2.3 (2026-04-24)** — Re-Review-Round-4 Full-Panel MINOR-CHANGES eingearbeitet (Option A: alle 21 Findings + 1 Regression + F-STAB-10-Promotion, keine Cherry-Picks):
  - **HIGHs (5) + 1 Regression:**
    - `BotControllerFactory` als Builder-Pattern mit vollständiger Kollaborateur-Liste + try/catch + User-Toast bei Failure (§2.1, §5 BuildController-Snippet) — F-ARCH-06-Verfeinerung, F-ARCH-12, F-STAB-26
    - Plan-Records auf **Identifier-only** umgestellt: `string defName`/`int thingIDNumber`/`(int,int)`/`byte rotation` statt `ThingDef`/`Building_WorkTable`/`IntVec3`/`Rot4`. `ImmutableList/Dictionary/HashSet` statt `IReadOnly*` (§2.3a) — F-ARCH-13, F-ARCH-17, D-23
    - `EventQueue` im `BotGameComponent`-**Konstruktor** initialisiert (vor Harmony-Patch-Möglichkeit), + explizite Feld-Deklarationen `controller`/`eventQueue`/`configResolver` in Klassenkopf (§5) — F-ARCH-15, F-ARCH-11-Regression
    - `LoadedGame()` cleared `eventQueue` als erste Aktion (+ `StartedNewGame()`) (§5) — F-STAB-20
    - Goal-Phase-Tag-Schema neu (§2.3b): `BotMapComponent.botPlacedThings`/`botAssignedJobs` Dicts + Execution-Layer trägt Tags ein + `CancelOrphanedDesignations/Jobs` implementiert korrekt — F-STAB-21, F-STAB-10 → **RESOLVED**, D-25
    - `autoEscapeStableCounter` mit D-26-StableConsecutiveCounter-Reset-Semantik statt absoluter Zeit (§6.3) — F-AI-15
  - **MEDs (9):**
    - `[StaticConstructorOnStartup]`-Klasse `CompatInitializer` statt `Mod`-ctor, `DirectXmlLoader`-Terminologie korrekt (§4.2) — F-RW4-02, F-RW4-01
    - Orphan-tmp-Cleanup in LearnedConfig-Write + `finally`-Delete (§6.1) — F-STAB-22
    - `consecutiveSessionBudgetExhausts` persistiert + User-Toast nach ≥3 (§5, §2.6) — F-STAB-23
    - Take(50)-WARN-Log bei Def-Bloat (§4.2) — F-STAB-24
    - `ConfigKey<T>` typisiert, `Get<T>(ConfigKey<T> key)` Compile-Time-Safety (§5a) — F-ARCH-14
    - `RecentDecisionsBuffer.Add()` Single-API mit Auto-Pin-Regel (§2.6) — F-ARCH-16
    - `GoalHealthScore` Launch-Critical-Klassifikation + `min(launch-critical)` Aggregation (§7.2) — F-AI-16
    - `EndingCommitment` Auto-Release bei `TransitionBackward(7→6)` (§6.3) — F-AI-17
    - Zusätzlich: Builder-Pattern in `BotControllerFactory` spezifiziert — F-ARCH-12
  - **LOWs (7):**
    - Stale-Check-Lookup-Pfade + Map-first-Short-Circuit (§3) — F-RW4-01, F-RW4-03
    - `Defs/MainButtonDefs.xml` Snippet mit Pflichtfeldern (§10.2) — F-RW4-04
    - CompatMode-Persistenz-Doku (§4.2) — F-STAB-25
    - BotControllerFactory-Fehler-Pfad dokumentiert (§5) — F-STAB-26
    - Plan-Records als `ImmutableList` statt `IReadOnlyList` (§2.3a) — F-ARCH-17
    - Mock-Provider `FakeSnapshotProvider` + `TestSnapshotBuilder` erwähnt (§9.1, bereits in v2.2) — F-ARCH-10 (bleibt RESOLVED)
    - `stableCounter`-Scope: Home-Map explizit (§3 + §5) — F-AI-18
  - **Neue Decisions:** D-23 (Plan-Schema-Identifier-Pattern), D-24 (Event-Queue-Lifecycle mit Clear-on-Load), D-25 (Goal-Phase-Tag-Schema), D-26 (StableConsecutiveCounter-Pattern generisch)

- **v2.2 (2026-04-24)** — Re-Review-Round-3 Full-Panel MINOR-CHANGES eingearbeitet (24 Findings):
  - **HIGHs (5):**
    - `BotControllerFactory` neu (§2.1): Composition-Root test-konstruierbar ohne GameComponent — D-20
    - `stableCounter` persistiert in `BotMapComponent` + explizite Reset-Semantik (§3, §5): Counter reset bei Violation, zählt monoton sonst — F-AI-08
    - `sunk_cost_penalty` normalisiert auf `[0, 0.5]` (§6.3): garantiert Switch möglich, stranded-Escape-Hatches (User-Forced + Auto-Escape bei ≥0.95/≤0.30 für 5000 Ticks) — F-AI-10, F-AI-11
    - GC-Baseline-Timing: ≥5000 Ticks nach FinalizeInit + Floor 100 MB (§8) — F-STAB-14, F-RW3-04
    - Event-Identifier mit `EnqueueTick` + `target.generationTick <= EnqueueTick`-Stale-Check (§3, §2.6) — F-STAB-15, D-19
  - **CC-3-01 (MED×2):** `FinalizeInit`/`LoadedGame`/`StartedNewGame`-Split in `BotGameComponent` (§5) — F-RW3-03 + F-ARCH-07
  - **MEDs (10):** Plan-Schemas als records §2.3a (F-ARCH-09, D-21), `ConfigResolver`-Cache-Spec §5a (F-ARCH-08), OverrideLibrary-Layer-Kontrakt AI-7 + §6.2 (F-AI-13, D-22), `CompatPatternDef`-Defs + XML-Security + Regex-Timeout §4.2 (F-RW3-02, F-STAB-16), Mutex-Pfad-Hash + `FileShare.None` §6.1 (F-STAB-17), Critical/Normal-Event-Queue §2.6 + §3 (F-STAB-18), Goal-Orphan-Cleanup in Reconcile §5 (F-STAB-10 → RESOLVED), `GoalHealthScore`-Formel §7.2 (F-AI-12), Phase-7-Escape-Hatches §6.3 (F-AI-11)
  - **LOWs (7):** Stale-Check-Lookup-Pfade §3 (F-RW3-01), GC-Baseline-Timing §8 (F-RW3-04), Event-Queue transient dokumentiert D-19 (F-RW3-05), `FakeSnapshotProvider` §9.1 (F-ARCH-10), EventQueue im Konstruktor §5 (F-ARCH-11), Tier-Retention `RecentDecisionsBuffer` §2.6 (F-AI-14), defensiver Cleanup §5 (F-STAB-19)
  - Plus Mod-Leitfaden.md §2 Header-Patch: fixe Prio-Liste ist jetzt `base_prio` für `EmergencyHandler.Score()` (F-AI-09)
  - Neue Decisions: D-19 Event-Identifier-Generation-Tick, D-20 Composition-Root-Factory, D-21 Plan-Schema-Vertrag, D-22 OverrideLibrary-Layer-Kontrakt

- **v2.1 (2026-04-24)** — Re-Review-Round-2 MINOR-CHANGES eingearbeitet:
  - `EventQueue` als `BoundedEventQueue<BotEvent>` spezifiziert (Cap 256, Drop-Oldest, FIFO, Stale-Check) + Event-Klassen-Hierarchie — D-18 (§2.6, §3)
  - `BotGameComponent.FinalizeInit()` Post-Load-Reconcile für pendingPhaseIndex-Rollback (§5, F-STAB-10)
  - H6 `WindowStack.Add` bekommt Typ-Filter (`Dialog_NodeTree` / `MainTabWindow_Quests`) im Patch-Body — F-HARMONY-09
  - `CompatMode` umgebaut: `compat-patterns.xml` als Mod-Asset mit substring/regex-Match, globales `IsReduced`-Flag statt per-Target (§4.2, F-COMPAT-01, F-STAB-04)
  - H9 aus Patch-Tabelle entfernt, als „Mod-Subclass-Override" klargestellt — F-PATCH-02
  - `LearnedConfig` Mutex auf `Local\RimWorldBot_LearnedConfig_v2` + `AbandonedMutexException`-Handling (§6.1, F-STAB-11)
  - `BotErrorBudget` thread-safe (lock) + Reset on `StartedNewGame`/`LoadedGame` (§2.6, F-STAB-12)
  - GC-Monitoring auf relative Baseline (5× statt absolute 50 MB) (§8, F-STAB-13)
  - Events tragen nur Identifikatoren (UniqueLoadID / uniqueID), keine Live-Referenzen (§4.1)

- **v2.0 (2026-04-24)** — Revision nach Party-Mode-Review. Kern-Änderungen:
  - H1 + H7 + H8 entfernt, Tick-Host = `GameComponentTick` (D-12, D-13)
  - `BotMapComponent` eingeführt, `MapAnalysisSummary` statt Per-Cell-Persist (D-14)
  - Plan/Apply-Trennung Decision ↔ Execution (D-15)
  - `EmergencyResolver` Utility-basiert statt fixe Priorität (D-16)
  - Ending-Hysterese + Phase-7-Commitment (D-17)
  - Snapshot-DTOs für Testability (`ISnapshotProvider`)
  - `ConfigResolver` + Precedence (§5a neu)
  - `DlcCapabilities` für Feature-Detection
  - Schema-Versioning in allen Scribe-Blocks
  - LearnedConfig: `GenFilePaths.ConfigFolderPath` + atomic + Mutex + Corruption-Backup + per-Biome + Sample-Count-Formel
  - Harmony-Exception-Skelett verpflichtend (§4.1)
  - GC-Monitoring für OOM-Prevention
  - I0 Colony-Extinct Invariante
  - Goal-Retry-Cap + Poisoned-Queue + Regression-Detector
  - Telemetry default OFF + PII-Scrub
  - GitHub-Release-Lint + SHA256SUMS
- **v1.0 (2026-04-24)** — Initial Draft (REJECTed von Party-Mode)
