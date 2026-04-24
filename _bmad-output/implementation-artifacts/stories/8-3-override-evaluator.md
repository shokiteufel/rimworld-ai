# Story 8.3: OverrideEvaluator (Player-Override nach 60s bewerten)

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** M

## Story
Als Mod-Entwickler möchte ich **OverrideEvaluator** der Player-Overrides (z. B. User macht Ctrl+K Advisory, wechselt Pawn-Work-Priority) nach 60s via Outcome-Snapshot bewertet — nur „besser"-Overrides landen in OverrideLibrary.

## Acceptance Criteria
1. `OverrideEvaluator.EvaluatePending` alle 60s (oder nach definierter Warte-Zeit)
2. Snapshot-Vorher + Nachher → Delta-Score berechnen
3. Score-Delta > Threshold → OverrideLibrary ADD
4. Score-Delta < Threshold → OverrideLibrary REJECT (falls bereits drin)
5. Unit-Tests

## Tasks
- [ ] `Source/Data/OverrideEvaluator.cs`
- [ ] Snapshot-Delta-Berechnung
- [ ] Unit-Tests

## Dev Notes
**Kontext:** D-07, Architecture §6 Update-Logik.
**Vorausgesetzt:** 8.1, 2.1 (Snapshots).

## File List
| Pfad | Op |
|---|---|
| `Source/Data/OverrideEvaluator.cs` | create |

## Testing
Unit: Delta-Berechnung für 3 Scenarios.

## Review-Gate
Code-Review gegen D-07.
