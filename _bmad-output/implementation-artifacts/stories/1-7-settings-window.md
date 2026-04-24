# Story 1.7: Settings-Window (Mod.DoSettingsWindowContents)

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle
**Size:** M
**Sprint:** 1
**Decisions referenced:** D-06 (Ending-Strategy + Force-Option in Settings), D-07 (Reset-Button für LearnedConfig), D-17 (EndingStrategy.Forced bricht Commitment-Lock), F-STAB-07 (Telemetry default OFF, opt-in)

---

## Story

Als **Spieler** möchte ich **ein Settings-Panel in den Mod-Settings öffnen und dort** grundlegende Präferenzen einstellen können (Ending-Strategie, Ending-Force-Override, Telemetry opt-in, LearnedConfig-Reset), damit **ich den Bot zu meinem Spielstil konfigurieren kann ohne Code zu editieren**.

---

## Acceptance Criteria

1. **`Source/Data/Configuration.cs`** implementiert `Configuration : ModSettings` mit Feldern:
   - `EndingStrategy endingStrategy` (default `Opportunistic`)
   - `Ending? forcedEnding` (default `null`, nur genutzt wenn `endingStrategy == Forced`)
   - `bool telemetryEnabled` (default `false` — F-STAB-07)
   - `bool perPawnDefaultPlayerUse` (default `false` — PRD FR-03)
2. **`Configuration.ExposeData()`** override persistiert alle Felder
3. **`RimWorldBotMod.GetSettings<Configuration>()`** gibt die Config-Instanz zurück (RimWorld-API Standard)
4. **`RimWorldBotMod.DoSettingsWindowContents(Rect inRect)`** implementiert das UI:
   - **Section „General"**: Checkbox „Per-Pawn Default: Player Use" (beeinflusst Default für neue Pawns in Story 1.6)
   - **Section „Ending"**: Dropdown `EndingStrategy` (Opportunistic / Forced); wenn Forced gewählt → zweites Dropdown `Ending` (Ship/Journey/Royal/Archonexus/Void)
   - **Section „Learning"**: Button „Reset LearnedConfig" mit Confirmation-Dialog — setzt LearnedConfig zurück auf Compiled Defaults (D-07)
   - **Section „Telemetry"**: Checkbox „Enable Telemetry" mit Hilfstext „Logs colony IDs only (no pawn names). Requires restart." (F-STAB-07)
   - **Section „Settings Info"**: Read-only Text mit Mod-Version + Link zum GitHub-Repo
5. **`RimWorldBotMod.WriteSettings()`** override ruft `configResolver.Invalidate()` (aus Story 1.3/2.x — placeholder hier) nach `base.WriteSettings()` (F-ARCH-08 Hook)
6. **`SettingsCategory()`** override gibt `"RimWorld Bot"` zurück (für RimWorld-Mod-Settings-Menü-Listing)
7. **LearnedConfig-Reset-Button** zeigt nach Klick Confirmation-Dialog („Really reset learned data? All cross-game learning will be lost."); bei Bestätigung wird LearnedConfig-File gelöscht und neu mit Defaults angelegt; User-Toast „Learning data reset"
8. **Telemetry-Opt-in benötigt Restart** — Checkbox-Änderung zeigt User-Hint „Restart required"
9. **Settings persistiert** in Vanilla-RimWorld-Settings-Save (`ModsConfig.xml` bzw. mod-specific `HugsLibSettings.xml`-Äquivalent, wird automatisch von ModSettings-Base handled)

---

## Tasks

- [ ] `Source/Data/Configuration.cs` mit ModSettings-Subclass + ExposeData
- [ ] `RimWorldBotMod`: `GetSettings<Configuration>()`, `DoSettingsWindowContents`, `WriteSettings`, `SettingsCategory` implementieren
- [ ] UI-Helper-Methoden für die 5 Sections (modularer Code, nicht eine Mega-Methode)
- [ ] LearnedConfig-Reset-Logik: `LearnedConfig.DeleteFile()` + `LearnedConfig.LoadOrDefault()` neu (Placeholder bis Epic 8, hier stub `File.Delete(path)` reicht)
- [ ] `Defs/SoundDefs.xml` optional für Settings-Klick-Sound (oder Vanilla-Sound verwenden)
- [ ] Localization-Keys für alle Labels/Tooltips/Confirmation-Text (Vorgriff 1.8)
- [ ] Integration-Test TC-07-SETTINGS: Settings öffnen → alle Fields sichtbar → Werte ändern → Save/Load → persistent

---

## Dev Notes

**Architektur-Kontext (v2.3):**
- `Configuration` ist eine der drei Konfig-Quellen in §5a Precedence (`BotGameComponent > LearnedConfig > Configuration > Defaults`). Änderungen hier triggern `ConfigResolver.Invalidate()` (F-ARCH-08-Fix) damit Cache aktualisiert wird.
- **Kein Harmony-Patch** für Settings-Panel (AI-1, D-13: „Mod-Subclass-Override" in §4 ersetzt H9).
- Telemetry-Opt-in hart auf `default false` (F-STAB-07) — verhindert stille Datenlogs.

**Nehme an, dass:**
- `DoSettingsWindowContents` kann beliebig komplexe Widgets rendern (via `Widgets.Dropdown`, `Listing_Standard` etc.). Keine Custom-Window-Logic nötig.
- LearnedConfig-Pfad aus Architecture §6.1 (`GenFilePaths.ConfigFolderPath + "/RimWorldBot/learned-config.xml"`) — volle Implementierung in Epic 8, hier nur Placeholder-Delete.
- Confirmation-Dialog via `Find.WindowStack.Add(new Dialog_MessageBox(...))` ist Standard-API.

**Vorausgesetzt:**
- Story 1.2 is done (`RimWorldBotMod : Mod` existiert).
- Story 1.3 is done (falls Settings-Werte auf BotGameComponent-State zugreifen — in 1.7 NICHT, aber Vorgriff).

---

## File List

| Pfad | Operation | Zweck |
|---|---|---|
| `Source/Data/Configuration.cs` | create | ModSettings-Subclass |
| `Source/Core/RimWorldBotMod.cs` | modify | Add 4 overrides: GetSettings/DoSettingsWindowContents/WriteSettings/SettingsCategory |
| `Source/UI/SettingsRenderer.cs` | create | UI-Helper (eigentliche Widget-Logik) |
| `Languages/Deutsch/Keyed/Settings.xml` | create | DE-Labels |
| `Languages/English/Keyed/Settings.xml` | create | EN-Labels |

---

## Testing

**Unit-Tests:**
- `Configuration.ExposeData` Roundtrip (fake Scribe).
- `SettingsCategory` gibt erwarteten String.

**Integration-Tests:**
- **TC-07-SETTINGS:** Mod-Settings öffnen → alle Sections sichtbar → Werte ändern → Spiel-Restart → Werte bleiben.
- **TC-07-FORCED-ENDING:** Strategy auf Forced + Ending auf Ship → Save → Load → `BotGameComponent.endingStrategy == Forced`, `forcedEnding == Ship`.
- **TC-07-LEARN-RESET:** Fake-LearnedConfig mit Daten → Reset-Button + Confirm → File ist Defaults-leer.
- **TC-07-TELEMETRY-DEFAULT-OFF:** Fresh-Install → Settings öffnen → „Enable Telemetry" = unchecked.

---

## Review-Gate

- Code-Review gegen AI-1, D-13 (Mod-Subclass-Override), F-STAB-07 (default OFF).
- Visual-Review: Settings-Panel-Layout, keine Overflows, Tooltips lesbar.

---

## Aufgelöste Entscheidungen

- **TQ-S7-01 resolved:** **Telemetry-Opt-in benötigt Restart**, nicht Hot-Reload. Begründung: `TelemetryLogger` wird im Konstruktor konfiguriert (aus `Configuration`), mid-session-Wechsel würde halb-geloggte Runs produzieren. Restart-Hint im UI transparent.
- **TQ-S7-02 resolved:** **Dropdown für `EndingStrategy` mit zweitem Dropdown bei Forced**. Begründung: Zwei-Stufen-UI ist standard (PropertyGrid-Style), vermeidet 7-Item-Dropdown (`Opportunistic + 5 Forced-Variants`), kommt später in Story 1.7 oder Story 7.4 erweitert.
- **TQ-S7-03 resolved:** **Reset-Button in 1.7, volle Implementierung aber erst Epic 8** (LearnedConfig-Klasse + atomic-write). Begründung: Reset-Button ist UI-Teil von 1.7 AC 6 Epic 1; Placeholder-Delete reicht für Story-Review, Epic 8 füllt die Logik.
- **TQ-S7-04 resolved:** **Kein Custom-Sound** für Settings-Interaktionen. Begründung: Vanilla-Widget-Sounds sind UX-konsistent, Custom-Sound wäre Polish (Epic 8).
