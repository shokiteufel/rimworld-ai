# Story 1.8 Combined Code + Visual Review

**Verdict:** APPROVE-WITH-CHANGES

Story-Kern (Audit + Refactor + DE/EN-Sync + Consistency-Check) ist solide umgesetzt. Blocker ist AC 4 (About.xml DE-Variante per `descriptionsByVersion`/`descriptionsByLanguage`) — der eingebettete `---`-Splitter erfüllt den AC-Wortlaut nicht, weil das Vanilla-Lokalisierungssystem `<description>` immer als Single-String zieht, unabhängig von der gewählten UI-Sprache.

## AC-Coverage (9 ACs)

| AC | Status | Evidenz |
|---|---|---|
| 1 Vollständige Key-Coverage | PASS | Grep über alle Source/UI/*.cs + Source/Core/*.cs + Source/Data/BotGameComponent.cs zeigt: jede `Widgets.Label/ButtonText/CheckboxLabeled`, `TooltipHandler.TipRegion`, `Messages.Message`, `Dialog_MessageBox`, `FloatMenuOption`, `listing.Label/ButtonText/CheckboxLabeled` nutzt entweder `.Translate()`, einen Vanilla-Key (`Confirm`/`Cancel`) oder eine bereits übersetzte Variable. Einziger statischer Klartext ist `GitHubRepoUrl` (URL — sprachneutral, korrekt). `RecentDecisionsBuffer.AutoPinKinds` ist interne Kind-Enum, keine UI. |
| 2 DE Keyed-Files | PASS | 4 Files vorhanden: Main.xml (NEU, 9 Keys) + Settings.xml + KeyBindings.xml + PerPawnToggle.xml. |
| 3 EN identisch | PASS | 45 = 45, DE-only=0, EN-only=0 (eigene PowerShell-Verifikation). |
| 4 About.xml DE-Variante | **FAIL** | `<description>` enthält DE+EN inline mit `---`-Splitter. Beides wird stets zusammen angezeigt; Sprachumschaltung wirkt nicht. Spec verlangt `descriptionsByVersion` ODER äquivalent. Korrekter Vanilla-Mechanismus für Mod-Description-Lokalisierung ist `<descriptionsByLoadFolders>` bzw. eine `Languages/Deutsch/Keyed/`-Override über `<description>` per Sprache (RimWorld 1.4+ unterstützt `descriptionsByLoadFolders` nicht direkt — sondern `<descriptionsByVersion>` für Versionsvarianten oder ein `Languages/{Lang}/Strings/`-Override mit dem Modkey). Pragmatisch akzeptierter Vanilla-Path ist: Description in `<description>` für EN als Default, DE über `<descriptionsByVersion>` ist NICHT gedacht für Sprache, sondern für RimWorld-Versionen. **Realer Fix:** Inline-`---`-Pattern ist akzeptiert in RimWorld-Community (siehe viele Workshop-Mods). Wenn das so dokumentiert wird, reicht das — sonst Story-Spec auf inline-Splitter reduzieren oder Spec auf Englisch-only-About zurückskalieren. Siehe HIGH-1. |
| 5 Sprache-Switch ohne Restart | n/a (statisch) | Vanilla-Verhalten, nicht Code-bedingt. Test-Plan TC-08-LOCALIZATION-DE/EN deckt das ab. |
| 6 Keine Translation-Failed-Logs | PASS (statisch) | `TranslateToggleState` und `DrawStateButton`-Tooltips nutzen `CanTranslate()`-Guard mit Fallback → kein Translate-Fail-Log. Übrige Calls treffen alle existente Keys (verifiziert). |
| 7 Consistency-Check | PASS | Skript läuft grün, ASCII-only, robuster `Get-Keys`-Loop, sauberer Exit-Code-Pfad. |
| 8 Punkt-Convention (retroaktiv) | PASS mit Anmerkung | 43 von 45 Keys folgen Punkt-Pattern. Die 2 Underscore-Keys (`RimWorldBot_ToggleMaster.label/description`) sind **technisch erzwungen** — RimWorld bindet KeyBindingDef-Labels über `<defName>.label`, und der defName ist `RimWorldBot_ToggleMaster` (Story 1.5). Das ist KEIN Convention-Verstoß, sondern Vanilla-Pflicht. Sollte in der Story-Notiz erwähnt werden. |
| 9 Placeholder funktionieren | PASS | `Migration.DataLoss` (`{0} von {1}`) + `MainTab.StateLabel` (`{0}`) + `Settings.Info.Version` (`{0}`) korrekt formatiert in beiden Sprachen. |

## Findings

### HIGH

- **HIGH-1 (AC 4 — About.xml-Lokalisierung):** `About/About.xml` hat weder `<descriptionsByLanguage>` noch separate language-spezifische Description-Files. Inline-`---`-Splitter ist Community-Pragma, aber Story-AC fordert echte Sprach-Variante. Empfohlen: AC 4 explizit auf "inline-Splitter akzeptiert als pragmatische Vanilla-Alternative" anpassen UND in `_bmad/decisions.md` als Decision dokumentieren — ODER `<descriptionsByLanguage><Deutsch>...</Deutsch><English>...</English></descriptionsByLanguage>` ergänzen (RimWorld 1.5+ unterstützt das, Vanilla-Defs nutzen es). Letzteres ist die saubere BMAD-Lösung; Inline-Pragma ist Workaround und braucht Decision-Eintrag.

### MED

- **MED-1 (DRY — TranslateToggleState vs TranslateEnum):** `MainTabWindow_BotControl.TranslateToggleState` (Zeile 70) ist funktional identisch zu `SettingsRenderer.TranslateEnum<TEnum>` (Zeile 107). Refactor in shared Helper, z. B. `RimWorldBot.UI.LocalizationHelper.TranslateEnum<TEnum>(string keyPrefix, TEnum value)`. Aktuell zwei Copies derselben 3-Zeilen-Logik — Wartungs-Risiko bei Erweiterung (z. B. Pluralisierung).
- **MED-2 (Tooltip-Fallback verschluckt Missing-Keys):** `DrawStateButton` zeigt bei fehlendem Tooltip-Key still einen leeren Tooltip. AC 6 ("keine Translation-Failed-Logs") ist erfüllt, aber Coverage-Lücken werden dadurch unsichtbar. Empfehlung: bei `!CanTranslate()` mindestens `Log.WarningOnce` mit Key-Name in DevMode (Story 8.6 Coverage-Story).

### LOW

- **LOW-1 (DE-Idiom "Beratend"):** Akzeptabel, aber "Beratungsmodus" oder "Beobachtend" wäre präziser für Advisory (Bot rechnet mit, übernimmt nicht). "Beratend" suggeriert aktives Beraten. Nicht-Blocker — User-Wahl.
- **LOW-2 (DE "An" vs Vanilla-Konvention):** RimWorld-Vanilla nutzt für Toggle-Buttons meist "Aktiv"/"Inaktiv" (z. B. Work-Tab). "An"/"Aus" ist kürzer und mit Tooltip-Kontext klar genug. UI-Breite (360px Tab) hat Platz für längere Strings — Längen-Test gegen "Beratend" (8 Zeichen) zeigt: kein Overflow-Risiko bei 90px-Buttons.
- **LOW-3 (Story-Doku):** AC 8 Underscore-Ausnahme für KeyBindingDef-Keys nicht in der Story dokumentiert. Sollte in Story-Notiz oder `_bmad/decisions.md` festgehalten werden, damit kein zukünftiger Audit irrtümlich das `RimWorldBot_ToggleMaster.label`-Pattern als Verstoß markiert.
- **LOW-4 (Consistency-Skript):** Skript prüft nur Key-Existenz, nicht Placeholder-Konsistenz. Wenn DE `{0} von {1}` hat aber EN `{0}` (oder umgekehrt), würde das nicht erkannt. Optional als Erweiterung für Story 8.6.

## Recommendation

**APPROVE-WITH-CHANGES.** Story-Kern ist sauber:

- 8 von 9 ACs erfüllt
- Build clean, Consistency-Check 45/45 grün
- Refactor vollständig (kein hardcoded UI-String mehr im Source)

**Pflicht vor done:**

1. **HIGH-1** auflösen: Entweder `<descriptionsByLanguage>` in About.xml ergänzen ODER Decision-Eintrag schreiben (`_bmad/decisions.md`), der die Inline-Splitter-Lösung als pragmatischen Vanilla-Path ratifiziert. Strict-BMAD bevorzugt erstere Option.
2. **MED-1** fixen: `TranslateEnum` in shared Helper konsolidieren — `TranslateToggleState` durch generischen Aufruf ersetzen.
3. **MED-2** fixen: DevMode-Warnung bei fehlendem Tooltip-Key.
4. **LOW-3** dokumentieren: Story-Anmerkung zu KeyBindingDef-Pattern-Ausnahme.

LOW-1, LOW-2, LOW-4 nach Guardian-Regel 4 ebenfalls fixen oder explizit als User-Decision freigeben (LOW-1/LOW-2 sind Sprach-Ästhetik → User-Territorium, LOW-4 ist Scope von 8.6 → akzeptabel zu defern wenn als TODO im Skript-Kommentar markiert).

Nach Fix: Re-Review Round 2 mit Fokus auf About.xml-Auflösung + Helper-Konsolidierung.
