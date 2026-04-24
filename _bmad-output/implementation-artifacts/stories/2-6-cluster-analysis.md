# Story 2.6: Cluster-Analyse (Top-3-Region-Selection)

**Status:** ready-for-dev
**Epic:** Epic 2 — Map-Analyzer
**Size:** M

## Story
Als Mod-Entwickler möchte ich aus den per-Cell-Scores **Top-3 Cluster (zusammenhängende high-score Regionen)** identifizieren, damit das Overlay (Story 2.7) und der BuildPlanner (Epic 3) einen klaren Basis-Mittelpunkt-Vorschlag haben.

## Acceptance Criteria
1. `ClusterAnalyzer.FindTop3Clusters(double[] scores, CellSnapshot[] cells) → ClusterResult[3]` in `Source/Analysis/ClusterAnalyzer.cs`
2. `ClusterResult` record mit `(int x, int z) Center, double AvgScore, int CellCount, (int x, int z)[] BoundingCells`
3. Algorithmus: Flood-Fill ausgehend von Top-N Score-Peaks, Cluster-Merge bei < 10 Cells Distance, Mindest-Cluster-Size = 30 Cells (circa 5×6-Bauplatz)
4. Cluster-Overlap-Prevention: zwei Top-Cluster dürfen sich nicht überlappen (Bounding-Box-Check)
5. Fallback F-AI-04 (Architecture §7.4): wenn keine 3 Cluster mit AvgScore > 0.3 gefunden → return `ClusterResult[]` kleiner als 3, plus Warnung-Flag
6. Performance: < 300ms für 250×250
7. Unit-Tests mit fake-Score-Arrays (einzelner Peak, drei Peaks, kein Peak → Fallback)

## Tasks
- [ ] `Source/Analysis/ClusterAnalyzer.cs`
- [ ] `ClusterResult` record
- [ ] Flood-Fill mit Score-Threshold
- [ ] Cluster-Merge bei Nachbarschaft
- [ ] Fallback-Handling (< 3 Cluster)
- [ ] Unit-Tests

## Dev Notes
**Architektur-Kontext:** §2.2 `MapAnalyzer.Cluster` (hier extrahiert in eigene Klasse für Testbarkeit).
**Nehme an, dass:** 30-Cell-Minimum ist ausreichend für MVP-Basis-Layout (erweiterbar in Epic 3).
**Vorausgesetzt:** Story 2.5 (Score-Array verfügbar).

## File List
| Pfad | Op |
|---|---|
| `Source/Analysis/ClusterAnalyzer.cs` | create |
| `Source/Analysis/ClusterResult.cs` | create |

## Testing
- Unit: einzelner Peak, 3 Peaks, 0 Peaks (Fallback), überlappende Peaks (Merge)
- Integration: echte Map → 3 Cluster

## Review-Gate
Code-Review gegen F-AI-04 Fallback-Handling (Stuck-State-Prevention). Unit-testbar.
