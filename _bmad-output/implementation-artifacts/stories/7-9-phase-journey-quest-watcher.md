# Story 7.9: PHASE_JOURNEY Quest-Watcher

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** S

## Story
Als Mod-Entwickler möchte ich den **Journey-Quest-Watcher**: automatische Detection + Accept von Journey-Offer-Quests (Royalty: Empire-Journey; Vanilla: Event nach Ship-Landing).

## Acceptance Criteria
1. **`JourneyQuestPlanner` konsumiert `QuestOfferEvent` aus EventQueue** (HIGH-Fix, CC-STORIES-09): nicht mehr H6 Dialog_NodeTree-Hook als Primär-Pfad. Story 1.12 QuestManager-Polling enqueued `QuestOfferEvent(int questId, string questDefName, EnqueueTick)`. Planner iteriert Events, filtert auf Journey-relevante QuestDefs (z. B. `OpportunitySite_AncientComplex` + Journey-Offer-Variants). H6 bleibt nur als Fallback für Finale-Dialog-Events (z. B. Ship-Start).
2. Auto-Accept wenn Ending = Journey (primary oder forced)
3. **Quest-Details persistiert als `int questId`** (HIGH-Fix, D-23 identifier-only): NICHT `Quest`-Runtime-Ref in `BotGameComponent.journeyQuest`. Record: `JourneyQuestRef = (int QuestId, string QuestDefName, int AcceptedTick)`. Resolve bei Use-Site via `Find.QuestManager.QuestsListForReading.FirstOrDefault(q => q.id == questId)` (mit `poisonedQuestIds: HashSet<int>` bei nicht-Findbarkeit analog F-STAB-06).
4. DecisionLog bei Accept (auto-pinned)
5. Unit-Tests: QuestOfferEvent-Consumption, QuestId-Persistence, Resolve-Failure-Handling
6. **Schema-Bump** (HIGH-Fix Round-2-Stability, CC-STORIES-01): `journeyQuest: JourneyQuestRef?` in `BotGameComponent` ist neues persistent-Feld → via Story 1.9 `SchemaVersionRegistry` registrieren; Migrate setzt `journeyQuest = null`. `poisonedQuestIds: HashSet<int>` bleibt transient (kein Schema-Eintrag).

## Tasks
- [ ] `Source/Decision/JourneyQuestPlanner.cs`
- [ ] Quest-Dialog-Parser (Vanilla-`Quest`-API)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Journey-Ending.
**Sub-Phase (Story 7.0):** Implementiert `QuestWatch` aus `EndingSubPhaseStateMachine`.
**Vorausgesetzt:** 1.3, 1.12 (QuestManager-Polling), 7.0, 7.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/JourneyQuestPlanner.cs` | create |

## Testing
Unit: Quest-Parsing. Integration: Journey-Offer-Event.

## Review-Gate
Code-Review gegen D-15, Vanilla-Quest-API.
