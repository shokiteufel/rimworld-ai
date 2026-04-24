# Story 7.7: PHASE_SHIP AIPersonaCore-Beschaffung

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **AIPersonaCore-Beschaffungs-Strategie**: Quest-Watcher, Raid-Loot-Priority, Archaeology-Mission (Odyssey) als Fallback.

## Acceptance Criteria
1. **DLC-Guard am Plan-Anfang** (MED-Fix, CC-STORIES-05): `if (!DlcCapabilities.EndingAvailable(Ending.Ship)) return AcquisitionStrategy.Empty;` — Ship = Vanilla + no-Royalty-Remove.
2. `AIPersonaCorePlanner.Plan(ColonySnapshot) → AcquisitionStrategy`
3. Strategie-Priorität: Quest > Raid-Loot > Archaeology > Trade (letzter Fallback)
4. Quest-Watcher: auto-accept bei AIPersonaCore-Reward
5. Raid-Loot-Scanner: check nach Raids ob Core droppt
6. Archaeology: bei Odyssey aktiv, Expedition zu Ancient-Site
7. Unit-Tests + Integration

## Tasks
- [ ] `Source/Decision/AIPersonaCorePlanner.cs`
- [ ] Quest-Event-Hook
- [ ] Raid-Drop-Scanner
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md, F-AI-11 (Stranded-Escape bei nicht-Findbarkeit).
**Sub-Phase (Story 7.0):** Implementiert `ShipCoreAcquisition` aus `EndingSubPhaseStateMachine`.
**Annahmen:** 80% Find-Rate bei Quest-Watcher (PRD AC 5 Epic 7).
**Vorausgesetzt:** 7.0, 7.5, 7.6, 1.12 (QuestManager-Polling für moderne Quest-Offers).

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/AIPersonaCorePlanner.cs` | create |

## Testing
Unit: 4 Strategien. Integration: Quest-Reward-Flow.

## Review-Gate
Code-Review gegen D-15, F-AI-11.
