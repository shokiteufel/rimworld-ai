# Story 7.9: PHASE_JOURNEY Quest-Watcher

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** S

## Story
Als Mod-Entwickler möchte ich den **Journey-Quest-Watcher**: automatische Detection + Accept von Journey-Offer-Quests (Royalty: Empire-Journey; Vanilla: Event nach Ship-Landing).

## Acceptance Criteria
1. `JourneyQuestWatcher` hört auf Quest-Events via H6 (WindowStack.Add-Postfix gefiltert auf Dialog_NodeTree)
2. Auto-Accept wenn Ending = Journey (primary oder forced)
3. Quest-Details werden in `BotGameComponent.journeyQuest` gespeichert
4. DecisionLog bei Accept
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/JourneyQuestWatcher.cs`
- [ ] Quest-Dialog-Parser (Vanilla-`Quest`-API)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Journey-Ending.
**Vorausgesetzt:** 1.3, 7.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/JourneyQuestWatcher.cs` | create |

## Testing
Unit: Quest-Parsing. Integration: Journey-Offer-Event.

## Review-Gate
Code-Review gegen D-15, Vanilla-Quest-API.
