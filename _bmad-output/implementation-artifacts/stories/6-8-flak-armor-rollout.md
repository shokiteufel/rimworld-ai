# Story 6.8: Flak-Rüstung-Verteilung

**Status:** ready-for-dev
**Epic:** Epic 6 — Industrialization
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Flak-Armor-Rollout**: alle Combat-Specialists mit Flak-Vest + Flak-Pants + Flak-Helmet ausgestattet, via BillPlanner + Outfit-Assignment.

## Acceptance Criteria
1. `BillPlanner` erweitert um Flak-Crafting (Plasteel + Cloth)
2. `OutfitPlanner.Plan(PawnSnapshot[]) → OutfitPlan` (pure Decision, kein `pawn.outfitTracker`-Write)
3. `OutfitWriter.Apply(plan, map)` schreibt `pawn.outfits.CurrentOutfit` + `pawn.outfitTracker.assignment` (Plan/Apply-Split analog BillPlanner → BillWriter)
4. **Read-After-Write-Check** (MED-Fix, CC-STORIES-10, F-STAB-04): nach `pawn.outfits.CurrentOutfit = targetOutfit` Read-Back: `pawn.outfits.CurrentOutfit.uniqueId == targetOutfit.uniqueId`. Bei Mismatch (Outfit-Mods wie Pawn Rules, Simple Sidearms patchen Outfit-Assign): WARN-Log + Retry 1× nach 60 Ticks + Poisoned-Set.
5. Target: 1 Flak-Set pro Combat-Specialist
6. Unit-Tests inkl. Outfit-Mismatch-Simulation
7. **Exception-Wrapper** (HIGH-Fix Round-2-Stability, CC-STORIES-02): `OutfitWriter.Apply(plan, map)`-Hauptkörper + Per-Pawn-Outfit-Assign-Loop wrapped via Story 1.10 `ExceptionWrapper.Execution(...)`. Bei 2 Exceptions/min → `FallbackToOff()`.

## Tasks
- [ ] `Source/Decision/OutfitPlanner.cs` (pure)
- [ ] `Source/Decision/OutfitPlan.cs` (record, identifier-only)
- [ ] `Source/Execution/OutfitWriter.cs` (Apply + Read-After-Write + Poisoned-Set)
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
