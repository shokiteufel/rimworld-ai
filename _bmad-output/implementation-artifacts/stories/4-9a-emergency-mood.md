# Story 4.9a: Emergency-Handler E-MOOD

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** S
**Decisions referenced:** D-16 (Utility-Scoring), CC-STORIES-06 (Pawn-Exclusivity-Lock), CC-STORIES-07 (Staleness via 3.13)

## Story
Als Mod-Entwickler möchte ich den **E-MOOD-Handler** implementieren, der bei Mood-Invariant-Violation (I6, AvgMood<0.37) Recreation/Joy-Tasks priorisiert + Work-Reduce für Low-Mood-Pawns.

## Acceptance Criteria
1. `E_Mood : EmergencyHandler` mit `BasePrio = 6`, `LockPriority = 20` (niedrig)
2. `Eligibility`: `I6_Mood.Violated == true`
3. `Score`: `base + (mood_deficit * 100)` — Modifier je niedriger AvgMood
4. `Apply`: `WorkPriorityPlan`-Addendum: Joy-Tasks auf Priority 2, Work-Tasks auf Priority 4 für Pawns mit Mood<0.4 (6h ingame)
5. Launch-Critical: nein (kein Survival-Impact im Einzelschritt)
6. **Staleness-Tracking** via 3.13: wenn Mood nach Apply nicht steigt in 3 Evals → Cooldown
7. Unit-Tests

## Tasks
- [ ] `Source/Emergency/E_Mood.cs`
- [ ] Unit-Tests
- [ ] Staleness-Integration

## Dev Notes
**Kontext:** Mod-Leitfaden §2 E-MOOD, D-16.
**Nehme an, dass:** Joy-Activities sind Vanilla-JobGivers (Meditate, Socialize, etc.).
**Vorausgesetzt:** 3.1 (Framework inkl. Pawn-Exclusivity-Lock), 3.13 (Staleness), 4.8 (I6).

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_Mood.cs` | create |

## Testing
Unit: Score-Modifier, Apply erzeugt WorkPriorityPlan mit Joy-Priority.

## Review-Gate
Code-Review gegen D-16, Lock-Priority-Matrix.

## Transient/Persistent
Handler-State ist **transient** (via EmergencyResolver).
