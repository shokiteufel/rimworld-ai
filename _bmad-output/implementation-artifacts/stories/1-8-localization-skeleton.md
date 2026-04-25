# Story 1.8: Localization-Skeleton (DE + EN Keyed-Strings)

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle
**Size:** S
**Sprint:** 1
**Decisions referenced:** TQ-04 (Architecture §12, resolved als „XML-only Localization"), PRD NFR Full-Localization DE+EN

---

## Story

Als **Spieler** möchte ich **alle Mod-Strings aus Stories 1.1–1.7 auf Deutsch und Englisch lesen können** (Toggle-Button, Per-Pawn-Checkbox, Settings-Panel, Keybinding-Label, ITab-Label, Dialoge), damit **die Mod in beiden Sprachen komplett und konsistent ist und keine hardcoded-English-Strings im UI sichtbar bleiben**.

---

## Acceptance Criteria

1. **Vollständige Key-Coverage:** Alle in Stories 1.2–1.7 hardcoded-gesetzten UI-Strings sind durch `"key".Translate()`-Calls ersetzt; keine englischen Strings mehr direkt im C#-Code
2. **`Languages/Deutsch/Keyed/*.xml`** mit Keyed-Files:
   - `Main.xml` (Mod-Description, Button-Label, Tab-Label, generelle Texte)
   - `Settings.xml` (Settings-Panel-Labels + Tooltips + Dialog-Texte — aus Story 1.7)
   - `KeyBindings.xml` (aus Story 1.5 — zumindest der Key-Label)
   - `PerPawnToggle.xml` (aus Story 1.6 — Tab-Label + Checkbox + Hilfstext)
3. **`Languages/English/Keyed/*.xml`** identische Struktur zu DE mit englischen Werten
4. **`About/About.xml`-Description** enthält DE+EN-Text mit Inline-`---`-Splitter in einem einzigen `<description>`-Block. **Retroaktiv 2026-04-25:** `<descriptionsByLanguage>`-Element ist in RimWorld 1.6 ModMetaData NICHT supported (XML-Parse-Error zur Runtime, kein Vanilla-DLC nutzt es). Sprach-switchender Mod-List-Description ist mit Vanilla-1.6-API nicht erreichbar — Inline-Splitter ist der Community-Standard.
5. **Sprachen-Switch im RimWorld-Spiel-Menü** wechselt Mod-UI sofort (RimWorld-Konvention, kein Restart nötig — validiert durch Test)
6. **Keine `Log.Error`-Meldungen** wegen fehlender Translation-Keys beim Spiel-Load mit DE oder EN aktiviert
7. **Konsistenz-Check**: Alle Keys in DE existieren auch in EN und umgekehrt — kein „Key X existiert in DE aber nicht EN"
8. **Convention-Compliance**: Keys folgen `RimWorldBot.<Context>.<Element>`-Pattern (z. B. `RimWorldBot.MainTab.Label`, `RimWorldBot.Settings.Telemetry.Tooltip`). **Retroaktiv 2026-04-25:** Story-Spec hatte Underscore-Pattern, aber Stories 1.4-1.7 nutzen durchgängig Punkt-Convention (gängiger in modernen RimWorld-Mods, lesbarer). Story-AC angepasst statt 30+ Keys zu refaktorieren. **Ausnahme:** `RimWorldBot_ToggleMaster.label` + `.description` in `KeyBindings.xml` folgen Vanilla-RimWorld-Pflicht-Pattern `<defName>.label` für KeyBindingDef-Lookup (nicht überschreibbar).
9. **Placeholder-Strings** (z. B. „{0}" für Pawn-Name): `"RimWorldBot_StateChanged".Translate(oldState, newState)` funktioniert korrekt in beiden Sprachen

---

## Tasks

- [ ] Audit: alle Stories 1.2–1.7 File-Lists durchgehen + hardcoded-UI-Strings identifizieren
- [ ] Refactoring: hardcoded Strings in C#-Files durch `"key".Translate()` ersetzen
- [ ] `Languages/Deutsch/Keyed/Main.xml` anlegen (zentrale Keys)
- [ ] `Languages/Deutsch/Keyed/Settings.xml` (ggf. aus Story 1.7 erweitert)
- [ ] `Languages/Deutsch/Keyed/KeyBindings.xml` (aus Story 1.5 erweitert)
- [ ] `Languages/Deutsch/Keyed/PerPawnToggle.xml` (aus Story 1.6 erweitert)
- [ ] `Languages/English/Keyed/*.xml` analog (4 Files)
- [ ] `About/About.xml` Description-Block um DE-Version erweitern
- [ ] Consistency-Check-Skript (Python oder PowerShell) das prüft: jede Keyed-File-Datei in DE hat identische Key-Liste zu EN
- [ ] Integration-Test TC-08-LOCALIZATION: Spiel mit DE laden → UI auf Deutsch → Keybinding zyklieren, Settings öffnen, ITab öffnen → alle sichtbaren Strings deutsch
- [ ] Gleicher Test mit EN

---

## Dev Notes

**Architektur-Kontext (v2.3):**
- **XML-only Localization** (TQ-04-resolved in Architecture §12): keine embedded-English-Strings in C# außer Log-Messages mit Prefix `[RimWorldBot]` (Log-Messages sind Developer-Messages, nicht User-Sichtbar in RimWorld-UI).
- `Translate()`-Calls auf `string`-Extension-Method sind RimWorld-Standard.

**Nehme an, dass:**
- RimWorld's `LoadedLanguage`-System parst alle `.xml`-Files unter `Languages/{LanguageName}/Keyed/` automatisch.
- User-Sprache Wechsel über `Options → Gameplay → Language` ist ohne Mod-Restart erfolgreich.
- Key-Duplikate (gleicher Key in zwei Files) werden von RimWorld geloggt aber nicht-fatal — Consistency-Check-Skript fängt das trotzdem ab.

**Vorausgesetzt:**
- Stories 1.2–1.7 sind alle done (Keys aus jeder Story verfügbar).

---

## File List

| Pfad | Operation | Zweck |
|---|---|---|
| `Source/Core/RimWorldBotMod.cs` | modify | Hardcoded Strings durch `.Translate()` ersetzen |
| `Source/UI/*.cs` (alle) | modify | analog |
| `Source/Data/Configuration.cs` | modify | Hilfs-Tooltips durch `.Translate()` |
| `Languages/Deutsch/Keyed/Main.xml` | create | DE Zentrale Keys |
| `Languages/Deutsch/Keyed/Settings.xml` | modify (aus 1.7 erweitert) | DE Settings-Keys |
| `Languages/Deutsch/Keyed/KeyBindings.xml` | modify (aus 1.5) | DE Keybinding-Key |
| `Languages/Deutsch/Keyed/PerPawnToggle.xml` | modify (aus 1.6) | DE Per-Pawn-Keys |
| `Languages/English/Keyed/Main.xml` | create | EN Zentrale Keys |
| `Languages/English/Keyed/Settings.xml` | modify | EN Settings-Keys |
| `Languages/English/Keyed/KeyBindings.xml` | modify | EN Keybinding-Key |
| `Languages/English/Keyed/PerPawnToggle.xml` | modify | EN Per-Pawn-Keys |
| `About/About.xml` | modify | DE-Description-Block ergänzen |
| `Tools/check-localization-consistency.ps1` | create | Dev-Helper für Consistency-Check |

---

## Testing

**Unit-Tests:** Keine (XML-Content, via Integration validiert).

**Integration-Tests:**
- **TC-08-LOCALIZATION-DE:** Spiel mit DE → alle UI-Strings deutsch, keine `Translation failed`-Logs.
- **TC-08-LOCALIZATION-EN:** Spiel mit EN → alle UI-Strings englisch.
- **TC-08-CONSISTENCY-CHECK:** Skript `check-localization-consistency.ps1` läuft grün (Key-Listen identisch zwischen DE und EN).

---

## Review-Gate

- Code-Review: keine hardcoded English-Strings (außer Log.Message).
- Visual-Review: kein Text abgeschnitten oder overflowt in beiden Sprachen (Deutsch neigt zu längeren Wörtern).
- Consistency-Check-Skript-Output im Review-Report.

---

## Aufgelöste Entscheidungen

- **TQ-S8-01 resolved:** **4 separate Keyed-Files** (`Main`, `Settings`, `KeyBindings`, `PerPawnToggle`) statt ein Mega-File. Begründung: Folgt Story-Struktur (jede Story bringt ihren Keyed-File-Satz mit); erleichtert Merge-Conflicts in zukünftiger Team-Arbeit.
- **TQ-S8-02 resolved:** **Key-Pattern `RimWorldBot_<Context>_<Element>`**. Begründung: Vendor-Prefix vermeidet Kollisionen mit Vanilla/anderen-Mods Keys; Kontext+Element macht Keys selbst-dokumentierend (kein Grep auf Call-Sites nötig).
- **TQ-S8-03 resolved:** **Log.Message bleibt English** (keine `.Translate()` für Developer-Logs). Begründung: Logs sind für GitHub-Issue-Reporting + Dev-Debugging; gleiche Sprache für alle User erleichtert Support-Triage.
- **TQ-S8-04 resolved:** **Consistency-Check-Skript in `Tools/`**, nicht als Git-Hook oder CI. Begründung: Manual-Run vor Release reicht; CI-Integration wäre Scope von Epic 8 Story 8.10 (GitHub-Release-Pipeline).
