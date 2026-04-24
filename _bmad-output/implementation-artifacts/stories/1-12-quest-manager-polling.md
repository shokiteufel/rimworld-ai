# Story 1.12: QuestManager-Polling-Hook (Cross-Cutting)

**Status:** ready-for-dev
**Epic:** Epic 1 (Cross-Cutting-Infrastruktur)
**Size:** S
**Decisions referenced:** D-18/D-19 (EventQueue), CC-STORIES-09

## Story
Als Mod-Entwickler möchte ich einen **GameComponentTick-Polling-Hook auf `Find.QuestManager.QuestsListForReading`** der neue Quests detektiert und als `QuestOfferEvent` in die EventQueue enqueued — damit Story 7.9 (Journey-Quest), 7.11, 7.14, 7.15, 7.7 (AIPersonaCore-Quest) moderne Quest-API nutzen statt Legacy-Dialog_NodeTree-Pattern.

## Acceptance Criteria
1. `Source/Events/QuestManagerPoller.cs` — wird in `BotGameComponent.GameComponentTick` alle 2500 Ticks aufgerufen
2. Pollt `Find.QuestManager.QuestsListForReading`, vergleicht mit `lastSeenQuestIds: HashSet<int>` (persistiert)
3. Neue Quest-IDs → `QuestOfferEvent(int questId, string questDefName, int enqueueTick)` in EventQueue (Critical-Queue, weil Quest-Offer-Akzeptanz-Zeitfenster limited)
4. Removed Quest-IDs (expired/failed) → `QuestRemovedEvent`
5. Consumers (7.9 JourneyQuestWatcher, 7.7 CoreAcquisition-Watcher) konsumieren Events im Tick-Host
6. Unit-Tests mit Fake-QuestManager
7. H6 (WindowStack.Add) wird erhalten für Finale-Dialog-Events (Ship-Start-Incident) — nicht ersetzt

## Tasks
- [ ] `Source/Events/QuestManagerPoller.cs`
- [ ] `QuestOfferEvent` + `QuestRemovedEvent` records
- [ ] `lastSeenQuestIds` persistent in BotGameComponent (Schema-Bump via Story 1.9)
- [ ] Unit-Tests
- [ ] Integration mit 7.9-Consumer

## Dev Notes
**Kontext:** CC-STORIES-09. RimWorld 1.3+ Standard-Quest-API via `Find.QuestManager`, nicht mehr `Dialog_NodeTree` als Primary.
**Nehme an, dass:** `Find.QuestManager.QuestsListForReading` liefert alle aktiven Quests + `Quest.id` ist save-stabil.
**Vorausgesetzt:** 1.3 (EventQueue), 1.9 (Schema-Version-Registry).

## File List
| Pfad | Op |
|---|---|
| `Source/Events/QuestManagerPoller.cs` | create |
| `Source/Events/QuestOfferEvent.cs` | create |
| `Source/Events/QuestRemovedEvent.cs` | create |
| `Source/Data/BotGameComponent.cs` | modify (lastSeenQuestIds) |

## Testing
- Unit: Poller mit Fake-QuestManager, detectiert neue IDs korrekt
- Integration: Quest-Offer-Event erreicht 7.9-Consumer

## Review-Gate
Code-Review gegen CC-STORIES-09, Quest-API-Korrektheit.

## Transient/Persistent
`lastSeenQuestIds` ist **persistent** (sonst würden nach Save-Load alle aktiven Quests fälschlich als „neu" detektiert).
