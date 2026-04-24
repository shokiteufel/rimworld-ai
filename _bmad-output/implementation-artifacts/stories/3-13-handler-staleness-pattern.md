# Story 3.13: Handler-Staleness-Pattern (Cross-Cutting)

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1 (Framework-Erweiterung)
**Size:** S
**Decisions referenced:** F-AI-04 (Stuck-State-Handling), CC-STORIES-07

## Story
Als Mod-Entwickler möchte ich ein **Handler-Staleness-Pattern** im EmergencyResolver: wenn Emergency-Handler 3× in 60s `Apply` returned ohne detektable Colony-State-Änderung → Handler geht in `cooldown_until_tick` für 5000 Ticks. Verhindert Dead-Lock wenn Emergency-Target unreachable.

## Acceptance Criteria
1. `EmergencyResolver` erweitert um:
   - `handlerStalenessCounters: Dictionary<string EmergencyId, int>`
   - `handlerCooldowns: Dictionary<string EmergencyId, int /*unlock_tick*/>`
2. Nach `Apply`: Snapshot-Diff (Colony-State vorher vs. nachher) via Hashes auf relevanten Feldern
3. Kein Diff → `stalenessCounter[handlerId]++`
4. `stalenessCounter >= 3` AND innerhalb 60s → `cooldowns[handlerId] = now + 5000`; Counter reset
5. Resolver überspringt Handler mit aktivem Cooldown — Utility-Score wird nicht berücksichtigt
6. **Retroaktive Integration** in Stories 3.3–3.6, 4-9a..g, 7.7, 7.8: jeder Handler markiert welche Colony-State-Felder sein Apply beeinflusst (für Staleness-Diff-Check)
7. DecisionLog-Eintrag bei Staleness-Trigger (auto-pinned analog RecentDecisionsBuffer)
8. Unit-Tests: Handler mit „Null-Apply" 3× → Cooldown aktiv

## Tasks
- [ ] `EmergencyResolver` erweitern (aus 3.1)
- [ ] Snapshot-Diff-Logic
- [ ] Cooldown-Tracker
- [ ] Unit-Tests
- [ ] Cross-Story-Documentation

## Dev Notes
**Kontext:** F-AI-04 + CC-STORIES-07.
**Nehme an, dass:** Colony-State-Hashing ist performant genug bei jedem Apply (einmal pro Tick pro Handler, meist max 1 aktiver Handler).
**Vorausgesetzt:** 3.1, 3.0 (Framework).

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/EmergencyResolver.cs` | modify |

## Testing
- Unit: 3× Null-Apply → Cooldown
- Integration: Unreachable Fire → E-FIRE-Handler in Cooldown, Resolver wählt nächsten Handler

## Review-Gate
Code-Review gegen F-AI-04, D-16 (Utility-Scoring nicht gebrochen durch Cooldown).

## Transient/Persistent
`handlerStalenessCounters` + `handlerCooldowns` sind **transient** (re-initialisiert bei LoadedGame — in-flight Emergency-Handler-State wird nicht persistiert).
