# Story 7.14: PHASE_ARCHONEXUS Wealth-Farm

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Wealth-Farming für Archonexus-Ending**: Plantagen (Tee/Smokeleaf), Crafting-Revenue, Trading-Priorität, bis Wealth-Threshold erreicht.

## Acceptance Criteria
1. **Ideology-DLC-Guard am Plan-Anfang** (HIGH-Fix, CC-STORIES-05): `if (!DlcCapabilities.EndingAvailable(Ending.Archonexus)) return WealthPlan.Empty;` als allererste Zeile von `Plan()`. Wealth-Farm-Ending ist Ideology-gated — ohne DLC ist Archonexus nicht erreichbar (`IncidentDef GameEnded_Archonexus` wird in Defs nur bei aktivem Ideology-DLC registriert). `EndingFeasibility` (Story 7.1) gibt für Archonexus `undefined`-Score wenn Ideology fehlt.
2. `ArchonexusWealthFarmPlanner.Plan(ColonySnapshot) → WealthPlan`
3. Wealth-Target: Vanilla-Archonexus-Threshold (aus IncidentDef)
4. Strategien: High-Value-Crafting, Trade-Caravan, Art-Production
5. Unit-Tests: DLC-absent-Case (ohne Ideology → Empty-Plan + keine Exception), DLC-present-Case (Plan nicht-leer)

## Tasks
- [ ] `Source/Decision/ArchonexusWealthFarmPlanner.cs`
- [ ] Wealth-Ratio-Calculation
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Archonexus.
**Sub-Phase (Story 7.0):** Implementiert `WealthFarm` aus `EndingSubPhaseStateMachine`.
**Vorausgesetzt:** 6.6, 7.0, 7.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ArchonexusWealthFarmPlanner.cs` | create |

## Testing
Unit: Wealth-Target-Progression.

## Review-Gate
Code-Review gegen D-15.
