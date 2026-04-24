# Story 7.15: PHASE_ARCHONEXUS Transition-Sequenz

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Archonexus-Transition-Management**: Archonexus-Core-Quest-Accept, Caravan zu Archonexus, Items-Mitnahme, Final-Transition.

## Acceptance Criteria
1. `ArchonexusTransitionManager` aktiv ab Wealth-Target-Reached
2. Quest-Accept: Archonexus-Core-Quest
3. Transition-Caravan mit max 3 Items (Vanilla-Regel)
4. Item-Priorität: Bionic-Upgrades, Tech-Items, High-Quality-Waffen
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/ArchonexusTransitionManager.cs`
- [ ] Item-Priority-Matrix
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Archonexus Transition.
**Vorausgesetzt:** 7.14.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ArchonexusTransitionManager.cs` | create |

## Testing
Unit: Item-Priority.

## Review-Gate
Code-Review gegen D-15.
