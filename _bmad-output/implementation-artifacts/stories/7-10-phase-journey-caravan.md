# Story 7.10: PHASE_JOURNEY Karawanen-Vorbereitung

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Karawanen-Vorbereitung für Journey-Ending**: Supplies-Pack (60-Tage-Pemmikan pro Pawn laut Ending-Pfade.md), Medizin, Ersatzkleidung, Waffen; alle Pawns in Caravan.

## Acceptance Criteria
1. `JourneyCaravanPlanner.Plan(ColonySnapshot) → CaravanPlan` (analog 5.5)
2. Supply-Priority: 60 Pemmikan × Reise-Tage × Pawn-Anzahl
3. Medizin: min. 20 Herbal + 5 Industrial
4. Alle lebenden Pawns eingepackt
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/JourneyCaravanPlanner.cs`
- [ ] Supply-Berechnung
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Journey.
**Vorausgesetzt:** 5.5, 7.9.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/JourneyCaravanPlanner.cs` | create |

## Testing
Unit: Pemmikan-Count für 3/5/8 Pawn-Colony.

## Review-Gate
Code-Review gegen D-15, D-23.
