# Story 8.6: Localization-Full-Coverage (alle Stories DE+EN)

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** M

## Story
Als Spieler möchte ich **vollständige DE + EN Localization** — alle UI-Strings aus Epic 1–7 in beiden Sprachen, Consistency-Check-Skript sauber.

## Acceptance Criteria
1. Alle C#-Hardcoded-UI-Strings durch `.Translate()` ersetzt
2. Neue Keyed-Files aus Stories 4-7: `Phases.xml`, `Combat.xml`, `Endings.xml`, `Settings.xml` (erweitert)
3. DE + EN parallel
4. Consistency-Check-Skript (Story 1.8 aus Tools/) grün
5. Integration: DE/EN-Switch ingame zeigt alle Texte korrekt

## Tasks
- [ ] Audit: alle Stories 4-7 für hardcoded-Strings
- [ ] Refactoring auf `.Translate()`
- [ ] 4 neue Keyed-Files pro Sprache = 8 Files
- [ ] Consistency-Check

## Dev Notes
**Kontext:** Story 1.8 Pattern, TQ-04 resolved.
**Vorausgesetzt:** 1.8, alle Epic 4-7.

## File List
| Pfad | Op |
|---|---|
| `Languages/Deutsch/Keyed/*.xml` | create/modify |
| `Languages/English/Keyed/*.xml` | create/modify |

## Testing
Integration: DE/EN-Switch, Consistency-Check.

## Review-Gate
Code-Review: keine hardcoded Strings. Visual: keine Overflow in DE.
