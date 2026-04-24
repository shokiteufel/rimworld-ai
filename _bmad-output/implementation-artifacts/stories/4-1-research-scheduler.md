# Story 4.1: Research-Scheduler

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** M

## Story
Als Mod-Entwickler möchte ich einen **Research-Scheduler** der pro Phase die passende Research-Queue setzt (Phase 2: Agriculture + Stonecutting; Phase 3: Winter-Tech; Phase 4: Complex-Furniture + Smithing), damit der Bot Tech-Progression autonom steuert.

## Acceptance Criteria
1. `ResearchScheduler.PlanQueue(ColonySnapshot, PhaseIndex) → List<string projectDefName>`
2. Queue-Priorisierung nach Phase + Colony-Needs (kalte Biome → Winter-Tech priorisiert)
3. `ResearchScheduler.Apply` setzt via `Find.ResearchManager.currentProj`
4. Respect DlcCapabilities (Royalty/Ideology-spezifische Research-Projects)
5. Auto-switch wenn current Project done
6. Unit-Tests pro Phase
7. Integration: Phase-2-Start → Agriculture in Queue

## Tasks
- [ ] `Source/Decision/ResearchScheduler.cs` + Apply
- [ ] `ResearchPriorityTable` pro Phase (config-driven via Defs)
- [ ] DlcCapabilities-Guards
- [ ] Unit-Tests
- [ ] Integration

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 2 Research.
**Annahmen:** `ResearchProjectDef.defName` ist stabil.
**Vorausgesetzt:** Story 1.3, 3.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ResearchScheduler.cs` | create |
| `Defs/ResearchPriorityDefs.xml` | create |

## Testing
Unit: Queue-Wahl pro Phase. Integration: Research startet.

## Review-Gate
Code-Review gegen D-15 (Plan/Apply), DlcCapabilities.
