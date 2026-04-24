# Story 8.9: DLC-Matrix-Testing + Top-10-Mod-Compat

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** M
**Decisions referenced:** F-STAB-04 (Top-10-Mods-Kompat), F-DLC-01 (DlcCapabilities)

## Story
Als Mod-Entwickler möchte ich **automatisiertes DLC-Matrix-Testing** + **Kompat-Check mit Top-10-Mods** (RocketMan, Performance Fish, Common Sense, Better Pawn Control, Work Tab, Dubs Performance Analyzer, Allow Tool, Combat Extended, Vanilla Expanded Core, Harmony).

## Acceptance Criteria
1. Test-Matrix: 7 DLC-Varianten × 10 Mod-Kombinationen = 70 Smoke-Tests
2. Pro Test: Mod laden, Log-Check auf Errors, Basic-Feature-Check (Button + ITab funktional)
3. Skript `Tools/run-dlc-matrix-test.ps1` als Dev-Tool
4. Compat-Report als Markdown `docs/compat-matrix.md`
5. Compat-Blocker als Issue-Template

## Tasks
- [ ] `Tools/run-dlc-matrix-test.ps1`
- [ ] Log-Parser (Error-Detection)
- [ ] Compat-Report-Generator
- [ ] Issue-Template

## Dev Notes
**Kontext:** F-STAB-04, F-DLC-01, TC-06 erweitert (aus Architecture §9.3).
**Vorausgesetzt:** alle Epic 1–7 (funktionale Features).

## File List
| Pfad | Op |
|---|---|
| `Tools/run-dlc-matrix-test.ps1` | create |
| `docs/compat-matrix.md` | create |
| `.github/ISSUE_TEMPLATE/compat-blocker.md` | create |

## Testing
Integration: Top-3-Mod-Kombi läuft grün.

## Review-Gate
Compat-Report im Review-Report, Issue-Template validiert.
