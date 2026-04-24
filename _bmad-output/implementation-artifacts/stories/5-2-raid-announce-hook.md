# Story 5.2: Raid-Announce-Hook (H4 via Harmony)

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** S
**Decisions referenced:** D-12 (Kein Patch auf Tick), D-13 (Minimal-Harmony), D-18/D-19 (Event-Queue)

## Story
Als Mod-Entwickler möchte ich den **Harmony-Postfix H4 auf `IncidentWorker_RaidEnemy.TryExecuteWorker`** implementieren, der pro erfolgreichem Raid-Announce ein `RaidEvent` in die EventQueue enqueued.

## Acceptance Criteria
1. `Source/Patches/H4_RaidAnnounce.cs` mit `[HarmonyPatch(typeof(IncidentWorker_RaidEnemy), nameof(TryExecuteWorker))]`
2. Postfix enqueued `new RaidEvent(EnqueueTick, RaidKind, PointsEstimate)` in BotGameComponent.EventQueue
3. Exception-Skelett (D-13 §4.1): try/catch + BotErrorBudget + BotSafe.Get → FallbackToOff
4. `HarmonyPriority(Priority.Low)` (D-13)
5. Unit-Tests via Mock-Harmony oder direct-Postfix-Call
6. Integration: Raid-Trigger → Event in Queue

## Tasks
- [ ] `Source/Patches/H4_RaidAnnounce.cs`
- [ ] RaidKind enum (einfach/mech/sapper/drop)
- [ ] Integration mit EventQueue
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Architecture §4.1, D-19 (EventQueue+EnqueueTick).
**Annahmen:** `IncidentParms.points` gibt Raid-Points an.
**Vorausgesetzt:** 1.3 (EventQueue).

## File List
| Pfad | Op |
|---|---|
| `Source/Patches/H4_RaidAnnounce.cs` | create |
| `Source/Events/RaidEvent.cs` | modify (RaidKind) |

## Testing
Unit: Postfix enqueued korrekt. Integration: Raid-Simulation.

## Review-Gate
Code-Review gegen §4.1 Exception-Skelett, D-13, D-19.
