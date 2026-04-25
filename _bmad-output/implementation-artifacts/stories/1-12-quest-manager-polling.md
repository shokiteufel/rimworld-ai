# Story 1.12: QuestManager-Polling-Hook (Cross-Cutting)

**Status:** done
**Epic:** Epic 1 (Cross-Cutting-Infrastruktur)
**Size:** S
**Decisions referenced:** D-18/D-19 (EventQueue), CC-STORIES-09

## Story
Als Mod-Entwickler möchte ich einen **GameComponentTick-Polling-Hook auf `Find.QuestManager.QuestsListForReading`** der neue Quests detektiert und als `QuestOfferEvent` in die EventQueue enqueued — damit Story 7.9 (Journey-Quest), 7.11, 7.14, 7.15, 7.7 (AIPersonaCore-Quest) moderne Quest-API nutzen statt Legacy-Dialog_NodeTree-Pattern.

## Acceptance Criteria
1. `Source/Events/QuestManagerPoller.cs` — wird in `BotGameComponent.GameComponentTick` alle 1250 Ticks aufgerufen (CR HIGH-1: Poll-Interval halbiert auf 1250 damit < BoundedEventQueue.StalenessThresholdTicks=2500; vermeidet Stale-Drop von Critical-QuestOfferEvents)
2. Pollt `Find.QuestManager.QuestsListForReading`, vergleicht mit `lastSeenQuestIds: HashSet<int>` (persistiert)
3. Neue Quest-IDs → `QuestOfferEvent(int questId, string questDefName, int enqueueTick)` in EventQueue (Critical-Queue, weil Quest-Offer-Akzeptanz-Zeitfenster limited)
4. Removed Quest-IDs (expired/failed) → `QuestRemovedEvent`
5. **(Producer-Side, 1.12-Scope)** Event-Records `QuestOfferEvent` + `QuestRemovedEvent` sind public und werden korrekt klassifiziert (Critical/Normal). Consumer-Integration in 7.7 (AIPersonaCore) und 7.9 (JourneyQuestWatcher) erfolgt in den jeweiligen Stories — nicht Teil von 1.12.
6. **(Deferred to Story 1.13)** Unit-Tests mit Fake-QuestManager — verschoben auf Story 1.13 (Test-Infrastructure), zusammen mit Carry-Over aus Stories 1.9 (Schema-Migration-Tests) und 1.10 (BotSafe-Tests). Begründung: aktuell existiert kein Test-Framework im Projekt; Story 1.13 etabliert das Setup einmalig für alle Cross-Cutting-Stories.
7. H6 (WindowStack.Add) wird erhalten für Finale-Dialog-Events (Ship-Start-Incident) — nicht ersetzt. Verifikation: `QuestWindowEvent`-Record bleibt in `BotEvent.cs` mit Erhalt-Comment; QuestManagerPoller greift den H6-Pfad nicht an.

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
| `Source/Events/BotEvent.cs` | modify (`QuestOfferEvent` + `QuestRemovedEvent` records — konsolidiert mit bestehenden Event-Records statt separater Files; Pattern-Konsistenz mit `MapFinalizedEvent`/`RaidEvent`/etc.) |
| `Source/Data/BotGameComponent.cs` | modify (lastSeenQuestIds, Schema v3→v4 Migration, Tick-Hook) |
| `Source/Data/SchemaRegistry.cs` | modify (BotGame v3→v4 als Story 1.12 Applied; bisherige Planned-Bumps um eine Version verschoben) |

## Testing
- Unit: Poller mit Fake-QuestManager, detectiert neue IDs korrekt
- Integration: Quest-Offer-Event erreicht 7.9-Consumer

## Review-Gate
Code-Review gegen CC-STORIES-09, Quest-API-Korrektheit.

## Transient/Persistent
`lastSeenQuestIds` ist **persistent** (sonst würden nach Save-Load alle aktiven Quests fälschlich als „neu" detektiert).
