# Story 4.3: Rekrutierungs-Logik (Raid-Prisoners + Quest-Rewards)

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Recruiting-Logic** die Raid-Prisoners capture + warden assignment + recruit-try-until-success steuert, damit die Colony auf 2-4 Pawns wächst.

## Acceptance Criteria
1. `RecruitingPlanner.Plan(ColonySnapshot, PawnSnapshot[]) → RecruitingPlan`
2. Bei Raid mit 1-2 überlebenden Raiders: Capture-Priority auf jüngsten, höchsten Skill-Sum
3. Warden-Assignment: bester Social-Skill-Pawn
4. Recruit-Mood-Management: reduzierte Work-Priority für Warden-Candidate bei niedrigem Mood
5. Quest-Pawn-Rewards auto-accept wenn Skills komplementär
6. Unit-Tests

## Tasks
- [ ] `Source/Decision/RecruitingPlanner.cs`
- [ ] Apply: `Pawn.guest.interactionMode = PrisonerInteractionModeDefOf.AttemptRecruit`
- [ ] Quest-Watcher-Event (Vorgriff auf Epic 7)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §5 Recruiting.
**Annahmen:** `PrisonerInteractionModeDefOf.AttemptRecruit` existiert in 1.5/1.6.
**Vorausgesetzt:** 3.1, 3.10 (Work-Priorities).

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/RecruitingPlanner.cs` | create |

## Testing
Unit: Best-Warden-Wahl. Integration: Raid-Capture-Flow.

## Review-Gate
Code-Review gegen D-15, Vanilla-Recruit-API.
