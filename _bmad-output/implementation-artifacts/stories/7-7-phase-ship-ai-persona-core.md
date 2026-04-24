# Story 7.7: PHASE_SHIP AIPersonaCore-Beschaffung

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **AIPersonaCore-Beschaffungs-Strategie**: Quest-Watcher, Raid-Loot-Priority, Archaeology-Mission (Odyssey) als Fallback.

## Acceptance Criteria
1. `AIPersonaCoreAcquisition.Plan(ColonySnapshot) → AcquisitionStrategy`
2. Strategie-Priorität: Quest > Raid-Loot > Archaeology > Trade (letzter Fallback)
3. Quest-Watcher: auto-accept bei AIPersonaCore-Reward
4. Raid-Loot-Scanner: check nach Raids ob Core droppt
5. Archaeology: bei Odyssey aktiv, Expedition zu Ancient-Site
6. Unit-Tests + Integration

## Tasks
- [ ] `Source/Decision/AIPersonaCoreAcquisition.cs`
- [ ] Quest-Event-Hook
- [ ] Raid-Drop-Scanner
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md, F-AI-11 (Stranded-Escape bei nicht-Findbarkeit).
**Annahmen:** 80% Find-Rate bei Quest-Watcher (PRD AC 5 Epic 7).
**Vorausgesetzt:** 7.5, 7.6.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/AIPersonaCoreAcquisition.cs` | create |

## Testing
Unit: 4 Strategien. Integration: Quest-Reward-Flow.

## Review-Gate
Code-Review gegen D-15, F-AI-11.
