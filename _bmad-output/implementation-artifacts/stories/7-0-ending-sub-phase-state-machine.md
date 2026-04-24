# Story 7.0: EndingSubPhaseStateMachine (Cross-Cutting, Epic 7 Fundament)

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings (Framework vor allen Ending-Sub-Phase-Stories)
**Size:** M
**Decisions referenced:** D-17 (Ending-Commitment), D-26 (StableConsecutiveCounter), F-AI-11, F-AI-15, F-AI-17, CC-STORIES-03

## Story
Als Mod-Entwickler möchte ich eine **`EndingSubPhaseStateMachine`** pro Ending: explizite Sub-Phase-Sequenz + Exit-Conditions + Rollback-Semantik bei Game-Breaking-Events. Ohne dies ist unklar wie Ship-Research→Bauplatz→Core→Finale-Transition läuft, und wie Phase-7-Auto-Release (F-AI-17) mit Sub-Phase-State interagiert.

## Acceptance Criteria
1. `Source/Endings/EndingSubPhaseStateMachine.cs`
2. Pro Ending eine Sub-Phase-Liste (via `EndingSubPhaseDef`):
   - **Ship:** `ShipResearch → ShipBauplatz → ShipCoreAcquisition → ShipFinale`
   - **Journey:** `QuestWatch → CaravanPrep → WorldTravel`
   - **Royal:** `HonorFarm → StellarchSiege`
   - **Archonexus:** `WealthFarm → Transition`
   - **Void:** `MonolithActivation → DarkStudy → VoidProvocation`
3. Jede Sub-Phase hat Exit-Conditions (`Func<ColonySnapshot, bool>`) + Goals + Rollback-Targets (welche Sub-Phase bei Game-Breaking-Event)
4. `Advance()`-Method: prüft Exit-Conditions + `stableCounter >= 2` (D-26-Pattern) → transitioniert zu nächster Sub-Phase
5. `TransitionBackward(cause: GameBreakingEvent)`-Method: rollt Sub-Phase zurück analog F-AI-17 (Phase-7→6)
6. `currentSubPhase` persistiert in `BotGameComponent` (Schema-Bump via Story 1.9)
7. DecisionLog bei Sub-Phase-Transition (auto-pinned wenn EndingSwitch/Rollback)
8. **Integration** mit 7.3 EndingSwitchEvaluator: Switch resetet Sub-Phase auf erste (Research/QuestWatch/HonorFarm/WealthFarm/MonolithActivation) des neuen Endings
9. Unit-Tests: Transitions + Rollback pro Ending
10. **Retroaktive Integration** in Stories 7.5–7.18: jede Story referenziert welche Sub-Phase sie implementiert

## Tasks
- [ ] `Source/Endings/EndingSubPhaseStateMachine.cs`
- [ ] `EndingSubPhase` enum pro Ending (Typ-sicher via getrennte Enums oder ein gemeinsames mit Namespace-Prefix)
- [ ] `Defs/EndingSubPhaseDefs.xml` (Sub-Phase-Reihenfolge konfigurierbar)
- [ ] `BotGameComponent.currentSubPhase`-Feld + Schema-Bump
- [ ] `Advance()` + `TransitionBackward()` implementieren
- [ ] Integration mit 7.3
- [ ] Unit-Tests für alle 5 Endings
- [ ] Cross-Story-Documentation in 7.5–7.18

## Dev Notes
**Kontext:** CC-STORIES-03 + D-17 + F-AI-17.
**Nehme an, dass:** Ship-Ending hat exakt 4 Sub-Phasen (kleinster Schritt). Andere Endings haben unterschiedlich viele.
**Vorausgesetzt:** 1.3 (BotGameComponent), 1.9 (Schema-Registry), 7.1 (EndingFeasibility), 7.3 (EndingSwitchEvaluator).

## File List
| Pfad | Op |
|---|---|
| `Source/Endings/EndingSubPhaseStateMachine.cs` | create |
| `Source/Endings/EndingSubPhase.cs` | create (Enum-Familie) |
| `Defs/EndingSubPhaseDefs.xml` | create |
| `Source/Data/BotGameComponent.cs` | modify (currentSubPhase) |

## Testing
- Unit: Transitions pro Ending, Rollback-Szenarien
- Integration: Ship-Reactor zerstört (Sub-Phase ShipFinale) → Rollback zu ShipCoreAcquisition

## Review-Gate
Code-Review gegen D-17, F-AI-11/15/17, konsistente Naming-Konvention.

## Transient/Persistent
`currentSubPhase` ist **persistent** (Phase-7-Progress muss Save-Load überleben). `subPhaseStableCounter` ebenfalls persistent analog `stableCounter`.
