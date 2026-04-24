# Story 4.9: Emergency-Handler für I6-I12 [GESPLITTET — siehe 4-9a bis 4-9g]

**Status:** retired (split 2026-04-24, D-31)
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Supersedes:** —
**Superseded by:** 4-9a, 4-9b, 4-9c, 4-9d, 4-9e, 4-9f, 4-9g

## Begründung
Original-Story 4.9 aggregierte 7 Emergency-Handler (E-MOOD, E-HEALTH, E-MENTALBREAK, E-RAID, E-FOODDAYS, E-MEDICINE, E-PAWNSLEEP) in einer M-Size-Story. Party-Mode-Review Round 1 (CC-STORIES + Individual-Finding 4.9 HIGH) hat festgestellt, dass Code-Review-Gate nicht 7 Handler parallel bewerten kann — bei einem Bug müsste die ganze Story zurück statt nur ein Handler.

Split in 7 S-Size-Stories (je ein Handler) folgt dem Pattern von 3.3–3.6 (E-FIRE, E-BLEED, E-FOOD, E-SHELTER+TEMP).

## Neue Sub-Stories
| Story | Handler | Invariant |
|---|---|---|
| [4-9a](4-9a-emergency-mood.md) | E-MOOD | I6 Mood |
| [4-9b](4-9b-emergency-health.md) | E-HEALTH | I7 Health |
| [4-9c](4-9c-emergency-mentalbreak.md) | E-MENTALBREAK | I8 MentalBreakRisk |
| [4-9d](4-9d-emergency-raid.md) | E-RAID | I9 RaidDefense |
| [4-9e](4-9e-emergency-fooddays.md) | E-FOODDAYS | I10 FoodDaysPerPawn |
| [4-9f](4-9f-emergency-medicine.md) | E-MEDICINE | I11 Medicine |
| [4-9g](4-9g-emergency-pawnsleep.md) | E-PAWNSLEEP | I12 PawnSleep |

## Hinweis für sprint-status.yaml
Eintrag `4-9-emergency-i6-i12` → retired (Status: `optional` mit Kommentar — Guardian-Waisen-Check schlägt sonst an). Neue Einträge `4-9a..g` auf `ready-for-dev`.
