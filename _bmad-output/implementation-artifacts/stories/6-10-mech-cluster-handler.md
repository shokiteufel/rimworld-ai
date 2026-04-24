# Story 6.10: Mech-Cluster-Handler

**Status:** ready-for-dev
**Epic:** Epic 6 — Industrialization
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Mech-Cluster-Handler**: Cluster-Detection + Bedrohungs-Bewertung + Ausschaltungs-Strategie (EMP-Granaten, Focused-Fire, Retreat).

## Acceptance Criteria
1. `MechClusterHandler.Assess(MapSnapshot, ColonySnapshot) → MechClusterStrategy`
2. Strategien: `ImmediateAssault` (kleine Cluster), `LatePawnEquipment` (EMP-Granaten craften, 10+ Tage warten), `Avoid` (Cluster bleibt)
3. `MechClusterDetector` iteriert Map für MechStructure-Things
4. Integration mit CombatCommander
5. Unit-Tests pro Strategie
6. DLC-Guard: nur wenn keine DLC-spezifischen Mech-Cluster

## Tasks
- [ ] `Source/Analysis/MechClusterDetector.cs`
- [ ] `Source/Decision/MechClusterHandler.cs`
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §8.1 Mech-Cluster.
**Vorausgesetzt:** 5.1, 5.3.

## File List
| Pfad | Op |
|---|---|
| `Source/Analysis/MechClusterDetector.cs` | create |
| `Source/Decision/MechClusterHandler.cs` | create |

## Testing
Unit: 3 Strategien. Integration: Mech-Cluster-Event.

## Review-Gate
Code-Review gegen D-15, D-16.
