# Story 8.4: Override-Library mit Layer-Kontrakt (AI-7)

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** M
**Decisions referenced:** D-22 (OverrideLibrary-Layer-Kontrakt: Emergency > Override > PhaseGoal), AI-7

## Story
Als Mod-Entwickler möchte ich **OverrideLibrary** als Dictionary `SituationHash → OverrideEntry` mit strikt AI-7-Layer-Kontrakt.

## Acceptance Criteria
1. `Source/Data/OverrideLibrary.cs` in LearnedConfig
2. `OverrideEntry` record: `(string SituationHash, string PreferredAction, double Confidence, int SuccessCount, int RejectCount)`
3. `Lookup(situationHash) → OverrideEntry?` — nur wenn Confidence > 0.5
4. `Add(entry)` + `Reinforce` + `Reject`-Mutationen
5. **AI-7-Layer-Kontrakt**: OverrideLibrary NIEMALS EmergencyResolver überstimmend; nur wenn keine Emergency aktiv
6. Unit-Tests Confidence-Progression + AI-7-Violation-Test

## Tasks
- [ ] `Source/Data/OverrideLibrary.cs`
- [ ] Integration in Decision-Pipeline (nach EmergencyResolver)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** §6.2a + D-22.
**Vorausgesetzt:** 8.1, 3.1 (EmergencyResolver-Koord).

## File List
| Pfad | Op |
|---|---|
| `Source/Data/OverrideLibrary.cs` | create |

## Testing
Unit: Confidence-Update, AI-7-Enforcement (keine Override wenn Emergency aktiv).

## Review-Gate
Code-Review gegen D-22, AI-7.
