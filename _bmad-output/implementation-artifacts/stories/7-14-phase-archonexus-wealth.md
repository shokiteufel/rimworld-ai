# Story 7.14: PHASE_ARCHONEXUS Wealth-Farm

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Wealth-Farming für Archonexus-Ending**: Plantagen (Tee/Smokeleaf), Crafting-Revenue, Trading-Priorität, bis Wealth-Threshold erreicht.

## Acceptance Criteria
1. `ArchonexusWealthFarmer.Plan(ColonySnapshot) → WealthPlan`
2. Wealth-Target: Vanilla-Archonexus-Threshold (aus IncidentDef)
3. Strategien: High-Value-Crafting, Trade-Caravan, Art-Production
4. Unit-Tests

## Tasks
- [ ] `Source/Decision/ArchonexusWealthFarmer.cs`
- [ ] Wealth-Ratio-Calculation
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Archonexus.
**Vorausgesetzt:** 7.1, 6.6.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ArchonexusWealthFarmer.cs` | create |

## Testing
Unit: Wealth-Target-Progression.

## Review-Gate
Code-Review gegen D-15.
