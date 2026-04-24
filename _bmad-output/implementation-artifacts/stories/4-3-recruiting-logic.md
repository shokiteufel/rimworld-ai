# Story 4.3: Rekrutierungs-Logik (Raid-Prisoners + Quest-Rewards)

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Recruiting-Logic** die Raid-Prisoners capture + warden assignment + recruit-try-until-success steuert, damit die Colony auf 2-4 Pawns wächst.

## Acceptance Criteria
1. **`RecruitingPlanner.Plan(ColonySnapshot, PawnSnapshot[]) → RecruitingPlan`** (HIGH-Fix, AI-2 pure): reiner Decision-Klasse, **mutiert KEINE RimWorld-State** (keine `pawn.guest.interactionMode`-Schreibung, keine `pawn.playerSettings`-Zuweisung).
2. **`RecruitingPlan` Record** (D-23 identifier-only): `(ImmutableList<PrisonerRecruitEntry> PrisonerEntries, string WardenUniqueLoadID, ImmutableList<string> AcceptQuestIds)` — `PrisonerRecruitEntry = (string PrisonerUniqueLoadID, PrisonerInteractionMode TargetMode)` (enum-spiegel, nicht `PrisonerInteractionModeDef`-Runtime-Ref).
3. Bei Raid mit 1-2 überlebenden Raiders: Capture-Priority auf jüngsten, höchsten Skill-Sum
4. Warden-Assignment: bester Social-Skill-Pawn
5. Recruit-Mood-Management: reduzierte Work-Priority für Warden-Candidate bei niedrigem Mood
6. Quest-Pawn-Rewards auto-accept wenn Skills komplementär
7. **`RecruitingWriter` Execution-Klasse** (HIGH-Fix, Plan/Apply-Split analog BillPlanner): `Source/Execution/RecruitingWriter.cs` — applied `RecruitingPlan` → mutiert `pawn.guest.interactionMode` + `pawn.playerSettings.AreaRestriction` für Warden. **Read-After-Write-Check** (CC-STORIES-10): nach Mutation `pawn.guest.interactionMode == target` verifizieren; bei Mismatch WARN + Retry nach 60 Ticks + Poisoned-Set analog 3.10.
8. Unit-Tests: Plan-Output deterministisch; Writer-Apply + Read-After-Write-Mismatch-Handling
9. **Exception-Wrapper** (HIGH-Fix Round-2-Stability, CC-STORIES-02): `RecruitingWriter.Apply(plan, map)`-Hauptkörper wrapped via Story 1.10 `ExceptionWrapper.Execution(...)`. Bei 2 Exceptions/min → `FallbackToOff()`.

## Tasks
- [ ] `Source/Decision/RecruitingPlanner.cs` (pure, Plan-only)
- [ ] `Source/Decision/RecruitingPlan.cs` (record, ImmutableList)
- [ ] `Source/Execution/RecruitingWriter.cs` (Apply + Read-After-Write + Poisoned-Set)
- [ ] Quest-Integration über Story 1.12 QuestManager-Polling (AcceptQuestIds aus Plan)
- [ ] Unit-Tests (Plan + Writer separat)

## Dev Notes
**Kontext:** Mod-Leitfaden §5 Recruiting. Plan/Apply-Split analog `BillPlanner → BillWriter` aus Story 3.9.
**Annahmen:** `PrisonerInteractionModeDefOf.AttemptRecruit` existiert in 1.5/1.6.
**Vorausgesetzt:** 3.1, 3.10 (Work-Priorities + Poisoned-Set), 3.9 (Plan/Apply-Pattern-Vorbild), 1.12 (Quest-Polling).

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/RecruitingPlanner.cs` | create |
| `Source/Decision/RecruitingPlan.cs` | create (record) |
| `Source/Execution/RecruitingWriter.cs` | create |

## Testing
Unit: Best-Warden-Wahl. Integration: Raid-Capture-Flow.

## Review-Gate
Code-Review gegen D-15, Vanilla-Recruit-API.
