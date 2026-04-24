# Story 7.2: Feasibility-Daily-Recompute (Reevaluation-Kadenz)

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** S

## Story
Als Mod-Entwickler möchte ich **tägliche Re-Evaluation der Feasibility-Scores** (alle 2500 Ticks = 1 in-game Stunde, aber bei Änderungen im Colony-State zusätzlich event-basiert).

## Acceptance Criteria
1. `BotGameComponent` triggert `EndingFeasibility.Reevaluate()` alle 2500 Ticks
2. Event-basiertes Recompute bei: Research-Done, Raid-End, Quest-Accept, Pawn-Death
3. Neue Scores werden in `BotGameComponent.lastFeasibilityMap` gespeichert (transient)
4. DecisionLog-Eintrag bei `primary`-Änderung
5. Unit-Tests

## Tasks
- [ ] `BotGameComponent.MaintenanceTick` erweitert
- [ ] Event-Trigger-Hooks
- [ ] Unit-Tests

## Dev Notes
**Kontext:** §6.3, D-17 Hysterese wird beim Re-Compute geprüft.
**Vorausgesetzt:** 7.1, 1.3.

## File List
| Pfad | Op |
|---|---|
| `Source/Data/BotGameComponent.cs` | modify |

## Testing
Unit: Re-Compute bei Research-Done.

## Review-Gate
Code-Review gegen §6.3, D-17.
