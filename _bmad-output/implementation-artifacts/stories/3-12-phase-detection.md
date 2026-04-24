# Story 3.12: Phase-Detection (Mid-Game-Aktivierung)

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** M
**Decisions referenced:** D-05 (Volle Re-Analyse bei nachträglicher Aktivierung), F-AI-01 (Phase-Transition-Guard)

## Story
Als Mod-Entwickler möchte ich dass der Bot bei **nachträglicher Aktivierung mitten im Spiel** die aktuelle Phase korrekt detektiert (nicht fälschlich Phase 0 für eine Colony mit fertiger Stone-Festung) — via ColonySnapshot-Analyse + Goal-Progress-Heuristik.

## Acceptance Criteria
1. `PhaseDetector.Detect(ColonySnapshot, MapAnalysisSummary) → int currentPhaseIndex`
2. Heuristik: pro Phase Definition check wieviele Goals erfüllt sind; höchste Phase mit `>= 75% Goals erfüllt` wird gewählt
3. **Layout-Score-Check** (D-05): wenn Basis-Layout-Score < 0.6 → Vorschlag via `DecisionLogEntry` „Umbau empfohlen"
4. **Konservative Wahl**: bei Unsicherheit wird die **niedrigere** Phase gewählt (lieber Phase 0 als Phase 3 fälschlich)
5. Integration: Mitten-in-Phase-3 Save → Mod aktivieren → `currentPhaseIndex = 3` korrekt
6. Unit-Tests für 5 fake-Scenarios (Phase 0/1/2/3/4)

## Tasks
- [ ] `Source/Phases/PhaseDetector.cs`
- [ ] Goal-Progress-Scoring pro Phase
- [ ] Layout-Score-Check (D-05)
- [ ] DecisionLog-Eintrag bei schlechtem Layout
- [ ] Unit-Tests
- [ ] Integration Mid-Game-Save-Test

## Dev Notes
**Architektur-Kontext:** D-05 (Re-Analyse), F-AI-01 (Transition-Guard verhindert sofortige Forward-Transition nach Detect).
**Nehme an, dass:** Phase-Definitions sind reflektiv abfragbar (PhaseStateMachine hat Liste aller Phasen).
**Vorausgesetzt:** Stories 3.7, 3.11, Stories 4.x (Phase 2-4 Definitions — für Detection müssen alle Phasen definiert sein; diese Story gehört thematisch Epic 4 aber wegen Phase-0+1-Scope platziert hier).

## File List
| Pfad | Op |
|---|---|
| `Source/Phases/PhaseDetector.cs` | create |

## Testing
- Unit: 5 fake-Scenarios für Phase 0-4
- Integration: Mid-Game-Save → Detection korrekt

## Review-Gate
Code-Review gegen D-05, F-AI-01. Regression-Tests für alle Phase-Definitions.
