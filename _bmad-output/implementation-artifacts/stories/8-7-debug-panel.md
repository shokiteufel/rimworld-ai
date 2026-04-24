# Story 8.7: Debug-Panel (Invariants / Goals / Resources / Decision-Trail)

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** M
**Decisions referenced:** F-AI-06 (Decision-Transparenz), F-AI-14 (Tier-Retention RecentDecisionsBuffer)

## Story
Als Spieler möchte ich **Debug-Panel** mit Tabs: Invariants-Status, Phase-Goals-Progress, Resources, Decisions-Trail (persistent FIFO-100 + Pinned-25).

## Acceptance Criteria
1. `Source/UI/DebugPanel.cs` als Window oder ITab
2. 4 Tabs: Invariants, Goals, Resources, **Decisions** (mit Pinned-Prominenz)
3. Decisions-Trail zeigt Kind + Reason + Tick, Pinned-Entries highlighted
4. Toggle-Key (Ctrl+F9)
5. Read-only, keine Mutation
6. Unit-Tests für UI-Helpers

## Tasks
- [ ] `Source/UI/DebugPanel.cs`
- [ ] 4 Tab-Renderer
- [ ] KeyBindingDef
- [ ] Unit-Tests

## Dev Notes
**Kontext:** §2.5, F-AI-06, F-AI-14.
**Vorausgesetzt:** 1.3 (RecentDecisionsBuffer).

## File List
| Pfad | Op |
|---|---|
| `Source/UI/DebugPanel.cs` | create |
| `Defs/KeyBindingDefs.xml` | modify |

## Testing
Integration: Debug-Panel öffnet + zeigt Live-Daten.

## Review-Gate
Code-Review gegen F-AI-06, F-AI-14. Visual-Review via Design-Critic.
