# Story 7.4: Force-Option (User-Override)

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** S

## Story
Als Spieler möchte ich via ModSettings (Story 1.7) **ein bestimmtes Ending erzwingen** — der Bot folgt der User-Wahl, unabhängig von Feasibility-Score.

## Acceptance Criteria
1. Settings-Panel-Dropdown (Story 1.7) aktiv: `endingStrategy = Forced` + `forcedEnding = X`
2. `EndingFeasibility.ApplyForce(forcedEnding)` überschreibt `primary`-Selection
3. Force bricht Phase-7-Commitment-Lock (D-17)
4. DecisionLog-Eintrag `ending_forced_override`
5. Unit-Tests

## Tasks
- [ ] `EndingFeasibility.ApplyForce`
- [ ] Settings-Panel-Integration (aus 1.7 erweitert)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** D-06 (Force-Option in Settings), D-17.
**Vorausgesetzt:** 1.7, 7.1, 7.3.

## File List
| Pfad | Op |
|---|---|
| `Source/Analysis/EndingFeasibility.cs` | modify |

## Testing
Unit: Force überschreibt Score-Ranking.

## Review-Gate
Code-Review gegen D-06, D-17.
