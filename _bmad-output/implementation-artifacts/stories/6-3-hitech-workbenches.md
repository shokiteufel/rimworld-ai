# Story 6.3: Hi-Tech-Werkbänke (HiTech-Bench, MultiAnalyzer, Fabrication)

**Status:** ready-for-dev
**Epic:** Epic 6 — Industrialization
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Hi-Tech-Workbench-Placement** (HiTech-Research-Bench, MultiAnalyzer, Fabrication-Bench) in dedizierter Craft-Halle — damit Epic 7 Ending-Research möglich.

## Acceptance Criteria
1. `BuildPlanner` erweitert um Hi-Tech-Hall-Layout (6×8 Minimum, mit 3 Workbenches)
2. Vorgelagerter Power-Check (benötigt 1000+W)
3. Research-Bench-Placement max 6 Tiles vom nächsten MultiAnalyzer
4. DLC-Guard: Royalty-Techprints ggf. via `DlcCapabilities.HasRoyalty`
5. Unit-Tests

## Tasks
- [ ] `BuildPlanner.PlanHiTechHall`
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 5/6.
**Vorausgesetzt:** 3.8, 6.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/BuildPlanner.cs` | modify |

## Testing
Unit: Hi-Tech-Hall-Layout.

## Review-Gate
Code-Review gegen D-15, DlcCapabilities.
