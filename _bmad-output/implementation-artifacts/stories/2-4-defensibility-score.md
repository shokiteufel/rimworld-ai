# Story 2.4: Defensibility-Score (Choke-Points, Cliffs, Natural-Walls)

**Status:** ready-for-dev
**Epic:** Epic 2 — Map-Analyzer
**Size:** M

## Story
Als Mod-Entwickler möchte ich **pro Cell einen Defensibility-Score** berechnen, der Choke-Points, angrenzende Cliffs/Mountains, und Zugangs-Engstellen bewertet, damit **die Site-Scoring-Formel (Story 2.5) `W_DEFENSE`-Beitrag korrekt gewichtet**.

## Acceptance Criteria
1. `DefensibilityAnalyzer.Compute(CellSnapshot[] cells) → float[]` gibt Score-Array (same Länge) zurück
2. Score-Komponenten pro Cell:
   - **Chokepoint-Detection**: Anzahl walkable-Nachbarn ≤ 2 → `chokepoint_bonus = 0.4`
   - **Cliff-Adjacency**: ≥ 1 Nachbar mit `IsMountain == true` → `cliff_bonus = 0.3`
   - **Natural-Wall**: Cell selbst ist Impassable (Cliff/Lava/Water-Ring) → `-∞` (wird von Site-Pool ausgeschlossen, nicht als Defensibility)
3. Score-Range: `[0.0, 1.0]` clamped; Gesamt-Score = `clamp(chokepoint_bonus + cliff_bonus, 0, 1)`
4. CellSnapshot.ChokepointScore wird gesetzt (aus Story 2.1 vorbereitet)
5. Performance: < 200ms für 250×250 (Neighbor-Lookup mit Fixed-Array `[-1,0,1] × [-1,0,1]`)
6. Unit-Tests mit fake-Snapshot-Patterns (isoliertes Choke, Cliff-Row, Open-Field)

## Tasks
- [ ] `Source/Analysis/DefensibilityAnalyzer.cs`
- [ ] `IsWalkable(CellSnapshot)` Helper
- [ ] `CellSnapshot.ChokepointScore`-Write über Provider
- [ ] Unit-Tests für 3-4 Muster
- [ ] Integration auf Mountain-Biome-Seed

## Dev Notes
**Architektur-Kontext:** §2.2 Analysis. Rein auf CellSnapshots (unit-testbar ohne Map).
**Nehme an, dass:** `IsWalkable` = `!IsMountain && HazardKind == None && !HasWater`. Verfeinerung in Epic 3/4 möglich (Winter/Summer-Water).
**Vorausgesetzt:** Story 2.1, Story 2.3 (Hazards bekannt).

## File List
| Pfad | Op |
|---|---|
| `Source/Analysis/DefensibilityAnalyzer.cs` | create |

## Testing
- Unit: 3 fake-Snapshot-Patterns (Choke/Cliff/Open)
- Integration: Mountain-Biome-Seed zeigt hohe Scores an Berg-Kanten

## Review-Gate
Code-Review gegen §2.2, Performance-Budget. Unit-testbar ohne Map-Runtime.
