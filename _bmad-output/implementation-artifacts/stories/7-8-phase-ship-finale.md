# Story 7.8: PHASE_SHIP Finale-Belagerung (15 Tage)

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **15-Tage-Belagerungs-Management** nach Ship-Reactor-Aktivierung: alle 15 Tage Mech- und Raider-Wellen autonom abwehren, Ship-Structural-Integrity schützen.

## Acceptance Criteria
1. **DLC-Guard** (MED-Fix, CC-STORIES-05): PhaseRunner nur aktiv wenn `DlcCapabilities.EndingAvailable(Ending.Ship)` — Ship = Vanilla + no-Royalty-Remove. Bei `false` setzt PhaseRunner eigenes `inactive`-Flag und skippt alle Tick-Operationen.
2. `ShipFinalePhaseRunner` aktiv ab Reactor-On
3. Supplies-Check vor Start: 30+ Tage Food, Medicine, Ammo (Ending-Pfade.md)
4. Welle-Handler: CombatCommander-Draft auf Killpoint + Ship-Defense
5. Integrity-Check: Ship-Structural-Repairs priorisiert
6. Endzustand: 15-Tage-Counter abgelaufen → Ship-Start-Event akzeptieren
7. Unit-Tests + Integration

## Tasks
- [ ] `Source/Decision/ShipFinalePhaseRunner.cs`
- [ ] 15-Tage-Counter (persistent)
- [ ] Ship-Repair-Priority
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Ship-Finale.
**Sub-Phase (Story 7.0):** Implementiert `ShipFinale` aus `EndingSubPhaseStateMachine` (letzte Sub-Phase des Ship-Endings).
**Annahmen:** Vanilla-Ship-Start-Incident läuft nach Reactor-Activation + 15 Tage.
**Vorausgesetzt:** 7.0, 7.5-7.7, 5.x (Combat).

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ShipFinalePhaseRunner.cs` | create |

## Testing
Unit: Wellen-Counter. Integration: Reactor-Activation-Flow.

## Review-Gate
Code-Review gegen D-15.
