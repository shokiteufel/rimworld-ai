# Architecture — Re-Review Round 3 (Full Panel)

**Dokument:** `_bmad-output/planning-artifacts/architecture.md` v2.1
**Review-Datum:** 2026-04-24
**Review-Modus:** Full-Panel-Party-Mode (alle 4 Personas parallel: RimWorld-Specialist, C#-Architect, Game-AI-Expert, Stability-Engineer)
**Gesamtverdict:** **APPROVE-WITH-MINOR-CHANGES** — alle 4 Personas stimmen zu; Revision auf v2.2 erforderlich

---

## Executive Summary

Alle vier Reviewer konvergieren auf **APPROVE-WITH-MINOR-CHANGES**. Kein REJECT, keine neuen CRITs. Die vorhergehenden Runden (Round 1 + Round 2) haben die fundamentalen Architektur-Fehler beseitigt; Round 3 findet Präzisierungs-Lücken in den neu eingeführten Mechanismen (Composition-Root, Event-Queue-Identifier, Hysterese-Math, GC-Baseline-Timing).

### Reviewer-Verdicts
| Reviewer | Round-1-Verdict | Round-2-Verdict | Round-3-Verdict | Neue HIGHs | Neue MEDs | Neue LOWs |
|---|---|---|---|---|---|---|
| RimWorld-Specialist | REJECT | APPROVE-W-MINOR | **APPROVE-W-MINOR** | 0 | 2 | 3 |
| C#-Architect | APPROVE-W-CHANGES | nicht dabei | **APPROVE-W-MINOR** | 1 | 3 | 2 |
| Game-AI-Expert | APPROVE-W-CHANGES | nicht dabei | **APPROVE-W-MINOR** | 2 | 4 | 1 |
| Stability-Engineer | APPROVE-W-CHANGES | APPROVE-W-MINOR | **APPROVE-W-MINOR** | 2 | 3 | 1 |
| **Summe** | — | — | — | **5** | **12** | **7** |

### Status der vorherigen Findings in v2.1
- **Round-1-Findings (alle 4 Reviewer):** 25 von 26 RESOLVED + 1 bereits in Round 2 zu RESOLVED promoviert (F-STAB-04). Nach Round-3-Deep-Check: **F-STAB-10 von RESOLVED auf PARTIAL korrigiert** (Stability-Reviewer entdeckt dass Rollback zwar pendingPhaseIndex fixt, aber teil-initialisierte Goals / platzierte Designations orphaned lässt).
- **Round-2-Findings (RimWorld + Stability):** Alle 7 RESOLVED in v2.1.
- **Regressions:** Keine.

---

## Cross-Cutting Findings (mehrfach unabhängig)

### CC-3-01 (MED) — `FinalizeInit()` Lifecycle-Miss + SRP-Verletzung
Gefunden von: **RimWorld (F-RW3-03)** + **C#-Architect (F-ARCH-07)**

**Kern:** `GameComponent.FinalizeInit()` wird bei BEIDEN Pfaden gerufen (LoadedGame + StartedNewGame), nicht nur Post-Load. Die Reconcile-Logik in v2.1 §5 läuft bei New-Game unnötig und könnte fälschlich „crash-recovery"-Log triggern. Zusätzlich: §5 packt drei Verantwortlichkeiten in eine Override (Composition-Root bauen + Post-Load-Reconcile + First-Tick-Schedule).

**Fix konsolidiert:** Reconcile-Block aus `FinalizeInit()` in `LoadedGame()` ziehen. `BotGameComponent` exponiert getrennt: `BuildController()` (Composition-Root, aufgerufen aus `FinalizeInit()` einmalig), `ReconcilePendingPhase()` (aus `LoadedGame()`), `StartedNewGame()` baut Initial-State. Konsistent mit bereits bestehender `BotErrorBudget`-Reset-Policy (§2.6: Reset on `StartedNewGame` + `LoadedGame`).

---

## Neue HIGH-Findings (5)

### F-ARCH-06 (HIGH) — Composition-Root nicht spezifiziert, `BotController` nicht test-konstruierbar
**Reviewer:** C#-Architect
**Location:** §2.1 + §5
**Issue:** `BotGameComponent` wird als „Composition-Root" deklariert, aber nirgends steht wie `BotController` mit seinen 10+ Kollaborateuren gebaut wird. Hart gekoppelt an `GameComponent`-Lifecycle → Unit-Tests müssten `Game`-Mock bauen, was §9.1 explizit ausschließen wollte. `BotMapComponent` greift via `Current.Game` → versteckte globale Kopplung.
**Fix:** Neue `BotControllerFactory.Create(Configuration, ISnapshotProvider, …)` als reine Factory ohne `GameComponent`-Bezug. `BotGameComponent.FinalizeInit()` ruft Factory auf. Tests instanzieren direkt via Factory mit Mock-Provider.

### F-AI-08 (HIGH) — „2-Tick-Stable" Counter-Reset-Semantik unterspezifiziert
**Reviewer:** Game-AI
**Location:** §3 + §2.1 PhaseStateMachine
**Issue:** Bei 1-Tick-Emergency und danach Nicht-Emergency: läuft der Stable-Counter weiter oder wird reset? Zwei verschiedene Verhalten — ohne Spec implementiert der Dev rateweise. Szenario: Counter läuft weiter → Bot springt 60 Ticks nach Crisis in neue Phase obwohl Crisis direkt davor war.
**Fix:** Explizite Semantik in §3: „Stable-Counter reset auf 0 sobald in einem Eval-Tick Invariant-Violation oder Emergency aktiv. Zählt nur monotone Ticks ohne Violation." Plus: `stableCounter` als Feld in `BotMapComponent` persistieren (Save-Load-stabil).

### F-AI-10 (HIGH) — `sunk_cost_penalty`-Math kann Switch unmöglich machen
**Reviewer:** Game-AI
**Location:** §6.3
**Issue:** `new > cur + margin + sunk_cost_penalty(sunk_cost)` mit `sunk_cost = resources_invested * 0.7`. Wenn `resources_invested` in absoluten Units (5000 Steel), ist Penalty = 3500 → Feasibility-Scores in [0,1] können das nie schlagen → Bot stranded in totem Ending.
**Fix:** Normalisierung: `sunk_cost_penalty = clamp(resources_invested / max_resources_for_ending, 0, 0.5)`. Garantiert Switch möglich bei `new > cur + 0.65`. Unit-Test `HysteresisGate` in §9.1 muss „switch-still-possible-at-extreme-sunk-cost"-Case enthalten.

### F-STAB-14 (HIGH) — GC-Baseline-Race direkt nach Mod-Enable + Save-Load
**Reviewer:** Stability
**Location:** §8
**Issue:** Baseline „beim ersten Tick nach `FinalizeInit`" — vor Map-Caches/Textures. User lädt Late-Game-Save direkt nach Mod-Enable: Baseline 5 MB, 30 s später legitim 50 MB → 10× Ratio → sofortiger `FallbackToOff()`. User-Frustration.
**Fix:** Baseline-Messung verschieben auf „ersten Maintenance-Tick (erster 2500er) UND ≥ 5000 Ticks Spielzeit nach FinalizeInit". GC-Monitoring bis dahin disabled. Alternative: Floor `max(GC.GetTotalMemory(true), 100 MB)`.

### F-STAB-15 (HIGH) — `Map.uniqueID`-Recycling umgeht Stale-Check
**Reviewer:** Stability
**Location:** §2.6 EventQueue + D-18
**Issue:** RimWorld recycelt `Map.uniqueID` nicht reserviert-intern, aber über Save-Edit oder Mod-Versions-Wechsel kann eine neue Map dieselbe ID wie eine gedroppte haben. `MapFinalizedEvent(uniqueID=42)` für alte Quest-Map sitzt 200 Ticks in Queue → Stale-Check findet Map mit ID 42 (die neue!) → `MapAnalyzer.FullScan` läuft auf falscher Map.
**Fix:** Event trägt `enqueueTick = Find.TickManager.TicksGame`. Stale-Check: `target.generationTick <= event.enqueueTick` — Maps die NACH Enqueue erzeugt wurden sind nicht der Referent. Analog für Pawn via `pawn.ageTracker.BirthAbsTicks` oder `Pawn.thingIDNumber`-Generation.

---

## Neue MED-Findings (12)

| ID | Reviewer | Location | Kern | Fix |
|---|---|---|---|---|
| F-RW3-02 | RimWorld | §4.2 + §10.2 | `compat-patterns.xml` in `About/` unkonventionell, im Mod-Paket-Baum fehlend | Als `<CompatPatternDef>` in `Defs/CompatPatternDefs.xml` modellieren (RimWorld-idiomatisch, XML-Patch-bar). §10.2 aktualisieren. |
| F-RW3-03 | RimWorld | §5, §3 | Siehe CC-3-01 | Siehe CC-3-01 |
| F-ARCH-07 | C#-Architect | §5 | Siehe CC-3-01 | Siehe CC-3-01 |
| F-ARCH-08 | C#-Architect | §5a | `ConfigResolver` Cache-Strategie + Invalidate-Hook unspezifiziert | `Dictionary<ConfigKey, object>` Cache; `Invalidate()` clear-all; Hooks in `Mod.WriteSettings()` + `BotGameComponent` post-load. |
| F-ARCH-09 | C#-Architect | §2.3 | Plan-Objekte (`BillPlan` etc.) ohne Schema-Spec | Neue §2.3a mit Plan-Schemas als `record` (immutable). Mindestens `BillPlan` + `WorkPriorityPlan` vollständig. |
| F-AI-09 | Game-AI | Mod-Leitfaden.md §2 | Fixe Prio-Liste widerspricht D-16, Dev liest Leitfaden → implementiert falsch | Mod-Leitfaden §2 Header-Patch: „Diese Liste ist `base_prio` für `EmergencyHandler.Score()` (siehe D-16). Reine Reihenfolge nicht mehr gültig." |
| F-AI-11 | Game-AI | §6.3 + D-17 | Phase-7-Lock + nicht-Game-Breaking-Stranding ohne Escape-Hatch | (a) `EndingStrategy.Forced` (User-Override) bricht Lock immer. (b) Auto-Escape bei `candidate ≥ 0.95 UND current ≤ 0.3 für ≥ 5000 Ticks`. |
| F-AI-12 | Game-AI | §7.2 | `GoalHealthScore`-Formel undokumentiert | §7.2 ergänzen: `Score = current_metric / target_metric` clamped [0,1] für quantitative Exit-Conds; boolean-Exit-Conds: 1.0/0.0 mit Decay-Buffer 250 Ticks gegen Flackern. |
| F-AI-13 | Game-AI | §6.2 | OverrideLibrary-Layer-Kontrakt fehlt: kann Override EmergencyResolver übersteuern? | Neue AI-7-Invariante: „Emergency > Override > PhaseGoal". §6.2-Absatz „Layer-Interaktion": (1) Override nur wenn Resolver keine Emergency wählt UND `SituationHash` matcht. (2) Override NIE Emergency-überstimmend. |
| F-STAB-16 | Stability | §4.2 | `compat-patterns.xml` XXE/ReDoS/XML-Patch-Resistenz fehlt | `XmlReader` + `DtdProcessing.Prohibit` + `XmlResolver=null`. Regex-Timeout 50 ms. Schema-Whitelist: max 50 Patterns, Länge ≤ 256. Datei-Hash-Check gegen Defaults. |
| F-STAB-17 | Stability | §6.1 | `Local\`-Mutex scope-Lücken bei Terminal-Server / Family-Sharing / Mod-Version-Upgrade | Mutex-Name inkludiert Datei-Pfad-Hash: `Local\RimWorldBot_{SHA1(ConfigFolderPath).Take(16)}_v2`. Plus `FileShare.None`-Lock als zweite Verteidigungslinie. |
| F-STAB-18 | Stability | §2.6 + §3 | Drop-Oldest kann kritisches `MapFinalizedEvent` verlieren | Pin-Whitelist: `MapFinalizedEvent`, `PawnExitMapEvent` never-drop. Bei Overflow: drop ältesten NICHT-Pin-Event. Alternativ Zwei-Klassen-Queue (Critical-32 / Normal-224). |

## Neue LOW-Findings (7)

| ID | Reviewer | Kern | Fix |
|---|---|---|---|
| F-RW3-01 | RimWorld | Stale-Check-Lookup-Pfad undokumentiert | Halbsatz in §3: „Stale-Check via `Find.Maps.Find(m => m.uniqueID == evt.MapId)` / `pawn.Destroyed`." |
| F-RW3-04 | RimWorld | Forced-GC im ersten Tick erzeugt Frame-Stutter | Baseline auf ersten 2500er-Tick verschieben (ohnehin maintenance-budgeted). Re-Baseline explizit mit `GC.GetTotalMemory(false)`. |
| F-RW3-05 | RimWorld | Event-Queue Persistenz-Status unklar | §2.6 / D-18 ergänzen: „EventQueue transient, NICHT persistiert. Post-Load triggert RimWorld Map.FinalizeInit ohnehin neu; andere Events Vanilla-Replay-fähig." |
| F-ARCH-10 | C#-Architect | Test-Mock-Provider nicht erwähnt | §9.1 ergänzen: `TestSnapshotBuilder` / `FakeSnapshotProvider` als Test-Helper. |
| F-ARCH-11 | C#-Architect | EventQueue Pre-Init-Race (Konstruktor vs. FinalizeInit) | EventQueue im `BotGameComponent`-Konstruktor initialisieren (vor Harmony-Patch-Möglichkeit). |
| F-AI-14 | Game-AI | `recentDecisions` FIFO-100 verliert seltene Ending-Switches | Tier-Retention: `transient` FIFO-100 (alle) + `pinned` FIFO-25 (PhaseTransition / EndingSwitch / CrashRecovery — überlebt Rotation). |
| F-STAB-19 | Stability | Cleanup `Find.Maps.SelectMany` ohne Null-Guard | Defensive: `.Where(m => m?.mapPawns != null).SelectMany(m => m.mapPawns.AllPawns?.ToList() ?? Enumerable.Empty<Pawn>())`. |

---

## Status-Korrektur: F-STAB-10 RESOLVED → PARTIAL

**Ursprünglich (Round 2):** `FinalizeInit()`-Post-Load-Reconcile setzt `pendingPhaseIndex = currentPhaseIndex`.

**Round 3 Entdeckung (Stability):** Rollback fixt nur die Index-Variable, aber wenn Crash zwischen Pending-Set und Goal-Init erfolgte (current=2, pending=3), können in der teil-initialisierten Phase 3 bereits Blueprints/Designations platziert worden sein → nach Rollback sind die orphan.

**Fix für v2.2:** `LoadedGame()`-Reconcile muss auch Phase-N+1-Goals cleanup: Map-Durchlauf, Blueprints ohne gültige Phase-Assoziation abräumen, Jobs cancellen. Als Sub-Task in `ReconcilePendingPhase()`.

---

## Konsolidierte Action-Matrix für v2.2

| Priorität | Findings | Fix-Scope |
|---|---|---|
| **HIGH (5)** | F-ARCH-06, F-AI-08, F-AI-10, F-STAB-14, F-STAB-15 | Composition-Root-Factory (D-20), 2-Tick-Counter-Reset-Semantik, sunk_cost-Normalisierung, GC-Baseline-Timing, Event-enqueueTick-Stamping (D-19) |
| **CC-3-01 (MED×2)** | F-RW3-03 + F-ARCH-07 | `FinalizeInit` splitten, Reconcile nach `LoadedGame`, `BuildController`/`ReconcilePendingPhase`/`ScheduleFirstTickEval` separat |
| **MED (10)** | F-RW3-02, F-ARCH-08, F-ARCH-09, F-AI-09, F-AI-11, F-AI-12, F-AI-13, F-STAB-16, F-STAB-17, F-STAB-18 | siehe Tabelle oben |
| **LOW (7)** | F-RW3-01, F-RW3-04, F-RW3-05, F-ARCH-10, F-ARCH-11, F-AI-14, F-STAB-19 | siehe Tabelle oben |
| **PARTIAL→RESOLVED** | F-STAB-10 | Goal-Cleanup im Reconcile-Pfad |

---

## Sign-Off-Anforderung für v2.2

1. Alle 5 HIGHs adressiert
2. CC-3-01 (`FinalizeInit`-Split) umgesetzt
3. Alle 10 MEDs adressiert (Guardian-Rule 4: keine Cherry-Picks)
4. Alle 7 LOWs adressiert
5. F-STAB-10 von PARTIAL auf RESOLVED promoviert
6. Mod-Leitfaden.md §2 Header aktualisiert (F-AI-09)
7. Neue Decisions: D-19 (Event-Identifier mit enqueueTick), D-20 (Composition-Root-Factory), D-21 (Plan-Schema-Vertrag), D-22 (OverrideLibrary-Layer-Kontrakt)
8. **Optional Round 4** nur wenn User explizit zusätzlichen Review wünscht — keiner der 4 Reviewer hat REJECT ausgesprochen und keiner hat neue CRITs gefunden. Nach v2.2-Einarbeitung ist User-Sign-Off der nächste logische Schritt.
