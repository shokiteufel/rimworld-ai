# Story 6.8: Flak-Rüstung-Verteilung

**Status:** ready-for-dev
**Epic:** Epic 6 — Industrialization
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Flak-Armor-Rollout**: alle Combat-Specialists mit Flak-Vest + Flak-Pants + Flak-Helmet ausgestattet, via BillPlanner + Outfit-Assignment.

## Acceptance Criteria
1. `BillPlanner` erweitert um Flak-Crafting (Plasteel + Cloth)
2. `OutfitPlanner.Plan` erzeugt Combat-Outfit mit Flak-Items als Required
3. Target: 1 Flak-Set pro Combat-Specialist
4. Unit-Tests

## Tasks
- [ ] `Source/Decision/OutfitPlanner.cs`
- [ ] BillPlanner-Erweiterung
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 6.
**Vorausgesetzt:** 3.9, 6.5.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/OutfitPlanner.cs` | create |
| `Source/Decision/BillPlanner.cs` | modify |

## Testing
Unit: Flak-Outfit-Assignment.

## Review-Gate
Code-Review gegen D-15.
