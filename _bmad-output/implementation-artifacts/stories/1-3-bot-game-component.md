# Story 1.3: BotGameComponent + BotMapComponent (Persistenz-Skeleton)

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle
**Size:** M
**Sprint:** 1
**Decisions referenced:** D-12 (Tick-Host = GameComponentTick), D-14 (Persistence-Scoping + Schema-Versioning + UniqueLoadID-Keying), D-17 (EndingCommitment-Feld), D-18/D-19 (EventQueue transient, im Konstruktor initialisiert), D-23 (keine RimWorld-Runtime-Typen wenn vermeidbar), D-25 (Goal-Phase-Tag-Dicts), D-26 (StableConsecutiveCounter: stableCounter, autoEscapeStableCounter)

---

## Story

Als **Mod-Entwickler** möchte ich **`BotGameComponent : GameComponent` und `BotMapComponent : MapComponent` als Persistenz-Skeleton anlegen** — mit allen Feldern aus Architecture §5 Code-Snippet, Scribe-Logik mit `schemaVersion`, Crash-Recovery-Hooks, und EventQueue-Init im Konstruktor —, damit **ab Story 1.4 (Toggle-Button) persistenter Mod-State existiert und ab Story 3.x (Phase-State-Machine) die Goal-Tag-Dicts verfügbar sind**.

---

## Acceptance Criteria

1. **`BotGameComponent : GameComponent`** in `Source/Data/BotGameComponent.cs` mit allen Feldern aus Architecture §5 Code-Snippet (inkl. `schemaVersion = 3`, `controller` / `eventQueue` / `configResolver` als private Felder, `masterState`, `currentPhaseIndex`, `pendingPhaseIndex`, `endingStrategy`, `primaryEnding`, `endingCommitment`, `autoEscapeStableCounter`, `ticksSincePhase7Entry`, `consecutiveSessionBudgetExhausts`, `completedGoals`, `perPawnPlayerUse: Dictionary<string, bool>`, `recentDecisions: RecentDecisionsBuffer`)
2. **Konstruktor `BotGameComponent(Game game) : base(game)`** initialisiert `eventQueue = new BoundedEventQueue<BotEvent>(criticalCap: 32, normalCap: 224)` **vor** jeder Harmony-Patch-Möglichkeit (D-24)
3. **`ExposeData()`-Override** mit `schemaVersion` als erster Scribe-Zeile + try/catch + `ResetToDefaults()`-Fallback, alle Felder persistiert wie im Architecture-Snippet (§5)
4. **`FinalizeInit()`-Override** ruft `BuildController()` (Placeholder-Methode, Factory kommt ab Story 2.x — in dieser Story setzt `BuildController()` nur `controller = null` und loggt „BotController build deferred to Epic 2")
5. **`LoadedGame()`-Override** ruft sequenziell: `eventQueue.Clear()`, `configResolver?.Invalidate()`, `BuildController() if controller == null`, `ReconcilePendingPhase()`, `ReconcilePhaseGoalOrphans()`, `CheckBudgetExhaustHistory()`
6. **`StartedNewGame()`-Override** ruft `eventQueue.Clear()` + `configResolver?.Invalidate()` + `BuildController() if controller == null`
7. **`ReconcilePendingPhase()`-Methode** prüft `pendingPhaseIndex != currentPhaseIndex` → Rollback + DecisionLog (`crash-recovery-phase-rollback`, auto-pinned via `RecentDecisionsBuffer.Add()`)
8. **`BotMapComponent : MapComponent`** in `Source/Data/BotMapComponent.cs` mit `analysisSummary`, `phaseProgressTicks`, `stableCounter`, `botPlacedThings`, `botAssignedJobs` (siehe Architecture §5 Snippet)
9. **`CancelOrphanedDesignations(int currentPhaseIndex)` + `CancelOrphanedJobs(int currentPhaseIndex)`** in BotMapComponent, implementiert wie im Architecture-Snippet
10. **`RecentDecisionsBuffer`-Klasse** in `Source/Data/RecentDecisionsBuffer.cs` mit `Add(DecisionLogEntry entry)` + Auto-Pin-Regel (PhaseTransition/EndingSwitch/CrashRecovery*/EndingForcedOverride/EndingAutoEscape/EndingCommitmentReleased → auch in pinned-Queue)
11. **`DecisionLogEntry`-Record** mit `Kind: string`, `Reason: string`, `Tick: int`, `Pinned: bool`
12. **`PhaseGoalTag`-Record** `(int PhaseIndex, string GoalId)` für Goal-Tag-Schema (§2.3b)
13. **Savegame-Roundtrip-Test:** State speichern, laden, Werte bleiben erhalten (AC 5 Epic 1)
14. **Schema-Migration-Test:** Savegame mit `schemaVersion = 1` laden → `Migrate()` läuft ohne Crash, `schemaVersion` auf 3 gesetzt

---

## Tasks

- [ ] `Source/Data/BotGameComponent.cs` anlegen gemäß Architecture §5 Snippet
- [ ] `Source/Data/BotMapComponent.cs` anlegen gemäß Architecture §5 Snippet
- [ ] `Source/Data/RecentDecisionsBuffer.cs` anlegen (Zwei-Queue-Struktur mit Auto-Pin-Regel)
- [ ] `Source/Data/DecisionLogEntry.cs` (Record) + `Source/Data/PhaseGoalTag.cs` (Record)
- [ ] `Source/Events/BoundedEventQueue.cs` anlegen (Critical/Normal-Klassen-Queue mit Cap-Spec)
- [ ] `Source/Events/BotEvent.cs` (abstract) + stub-Implementierungen `MapFinalizedEvent`, `RaidEvent`, `DraftedEvent`, `QuestWindowEvent`, `PawnExitMapEvent` (alle als record, nur Identifikatoren + EnqueueTick)
- [ ] `BotGameComponent.BuildController()` als Placeholder (sagt „deferred to Epic 2") — wird in Story 2.x mit echter Factory ersetzt
- [ ] `ReconcilePhaseGoalOrphans()` iteriert `Find.Maps` und ruft BotMapComponent-Methoden (Architecture-§5-Snippet)
- [ ] `CheckBudgetExhaustHistory()` mit Toast bei ≥3 consecutive exhausts
- [ ] Integration-Test TC-03-SAVE-ROUNDTRIP: State persistiert korrekt
- [ ] Integration-Test TC-08-SCHEMA-MIGRATION: Savegame mit v1-Schema lädt ohne Crash

---

## Dev Notes

**Architektur-Kontext (v2.3):**
- `BuildController()` ist in dieser Story ein Placeholder. Die echte Factory-basierte Implementierung (`BotControllerFactory.NewBuilder()...Build()`) kommt in Epic 2 / Story 2.x, sobald `ISnapshotProvider`, `ConfigResolver` etc. existieren.
- **Event-Queue ist transient** (D-19): NICHT in ExposeData() persistieren. Konstruktor-Init garantiert Verfügbarkeit ab Game-Init vor jeder Harmony-Postfix-Event.
- **perPawnPlayerUse-Cleanup alle 60000 Ticks** ist Teil dieser Story (Code-Snippet §5) — läuft in `GameComponentTick()`-Override (auch wenn `controller == null`, der Cleanup ist controller-unabhängig).

**Nehme an, dass:**
- `RecentDecisionsBuffer` ist `IExposable` (implementiert eigenes ExposeData für die zwei internen Queues). Alternative wäre `Scribe_Collections.Look(ref _transient, …)` zweimal — aber IExposable-Wrap ist sauberer.
- `EndingStrategy` und `Ending` und `EndingCommitment` sind Enums in eigener `.cs`-Datei (z. B. `Source/Core/Enums.cs`) — gehören aber zum Skeleton dieser Story, damit BotGameComponent kompiliert.
- `ToggleState` enum wird in Story 1.4 (MainTabWindow) genutzt — hier schon definiert.

**Vorausgesetzt:**
- Story 1.2 ist done (RimWorldBotMod-Entry existiert).
- RimWorld Scribe-API ist stabil zwischen 1.5 und 1.6.

---

## File List

| Pfad | Operation | Zweck |
|---|---|---|
| `Source/Data/BotGameComponent.cs` | create | Game-global Persistenz + Composition-Root-Placeholder |
| `Source/Data/BotMapComponent.cs` | create | Per-Map Persistenz + Tag-Dicts |
| `Source/Data/RecentDecisionsBuffer.cs` | create | Tier-Retention-Buffer |
| `Source/Data/DecisionLogEntry.cs` | create | Record für Decision-Entries |
| `Source/Data/PhaseGoalTag.cs` | create | Record für Goal-Phase-Assoziation |
| `Source/Events/BoundedEventQueue.cs` | create | Zwei-Klassen-Queue |
| `Source/Events/BotEvent.cs` | create | Abstract + 5 Subklassen |
| `Source/Core/Enums.cs` | create | `ToggleState`, `EndingStrategy`, `Ending`, `EndingCommitment` |
| `Source/Core/RimWorldBotMod.cs` | modify | Update Log-Message nach Harmony-Init um „BotGameComponent registered" |

---

## Testing

**Unit-Tests:**
- `RecentDecisionsBuffer.Add()`: Auto-Pin-Regel für jeden Kind-Wert verifizieren (6 Cases).
- `BoundedEventQueue`: Critical-never-drop, Normal-drop-oldest, FIFO, Stale-Check.
- `PhaseGoalTag` record Value-Equality.

**Integration-Tests:**
- **TC-03-SAVE-ROUNDTRIP:** State in BotGameComponent setzen → Savegame schreiben → neu laden → Werte identisch.
- **TC-08-SCHEMA-MIGRATION:** Fake-Savegame mit `schemaVersion = 1` → `Migrate()` ohne Crash → `schemaVersion` danach = 3.
- **TC-09-CRASH-RECOVERY:** `pendingPhaseIndex = 3, currentPhaseIndex = 2` in Save → Load → `LoadedGame()` rollt zurück, DecisionLog-Entry `crash-recovery-phase-rollback` existiert.

---

## Review-Gate

Code-Review gegen:
- Architecture §5 Code-Snippet (BotGameComponent + BotMapComponent)
- D-14 (UniqueLoadID-Keying für `perPawnPlayerUse`)
- D-19 (Event-Queue transient, NICHT in ExposeData)
- D-24 (EventQueue-Init im Konstruktor)
- D-25 (Goal-Tag-Schema — BotMapComponent-Dicts korrekt)

Visual-Review: nicht relevant (keine UI).

---

## Aufgelöste Entscheidungen

- **TQ-S3-01 resolved:** **`RecentDecisionsBuffer` implementiert `IExposable`**. Begründung: Kapselt Zwei-Queue-Logik (transient + pinned) in einer serialisierbaren Einheit; `Scribe_Deep.Look` am Aufruf-Punkt; vermeidet zwei separate Scribe-Calls pro Use-Site.
- **TQ-S3-02 resolved:** **`BuildController()` bleibt Placeholder in dieser Story**, echte Factory kommt Epic 2. Begründung: Factory braucht `ISnapshotProvider`, `MapAnalyzer`, `PhaseStateMachine` etc., die alle noch nicht existieren. Placeholder-Log ist ausreichend für Save-Load-Tests.
- **TQ-S3-03 resolved:** **Enums `ToggleState`, `EndingStrategy`, `Ending`, `EndingCommitment` in eigener `Enums.cs`-Datei** statt inline in BotGameComponent. Begründung: Wiederverwendung in UI (Story 1.4, 1.7), Tests, späteren Decision-Klassen; zentrale Ort für enum-Definitionen ist Konvention.
