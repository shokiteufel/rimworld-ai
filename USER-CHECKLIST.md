# User Checklist — RimWorld Bot Mod

**Was du als User noch machen musst.** Diese Datei wird vom Agent jedes Mal aktualisiert wenn etwas Neues für dich anfällt. Watchdog-Noise im Chat → hier nachschauen statt scrollen.

**Last updated:** 2026-04-25 (Story 2.1 done MT-5 PASS, Story 1.4 retroactive Bug-Fix D-41, bereit für Story 2.2)

---

## 🟡 Decisions Needed (echte User-Entscheidungen, BMAD liefert nicht die Antwort)

_(Aktuell keine offen — Sprint 3 mit Story 2.1 läuft.)_

Empfehlung: **Variante A**, weil du es so wolltest und weil mit Epic-2-Code ohne stable Epic-1-Foundation Bug-Hunting brutal wird.

---

## 🔴 Manuelle Tests (du bist der einzige der das ausführen kann)

_(Aktuell keine offen — Story 2.2 wird neue MTs bringen wenn nötig.)_

<!-- MT-5 erledigt, archiviert weiter unten

### MT-5: Story 2.1 Snapshot-Scan-Verifikation — **PFLICHT für Story 2.1 done**
**Wann:** Nach R2 APPROVE.
**Warum:** Story 2.1 AC-8 fordert Integration-Test mit echter Map (Array-Länge = `map.Area`, Performance < 500ms). Statisch nicht testbar — User-Game-Test pflicht.

**Schritte:**
1. RimWorld neu starten (frischer Build).
2. Spiel laden ODER neue Kolonie starten (irgendeine Map-Größe geht, Default 250×250 bevorzugt für Performance-Probe).
3. Im Spiel: Dev-Mode aktiv (war schon bei MT-3) → Top-Bar **Debug-Aktionen-Menü** (3. Icon, X-Stern).
4. Im Menü scrollen bis zur Kategorie **„RimWorldBot"** → Eintrag **„Trigger Snapshot Scan (Story 2.1)"** klicken.
5. **Player.log** öffnen: `%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log`
6. Erwartet (Beispiel-Output):
   ```
   [RimWorldBot] GetCells scan: 87ms for 62500 cells (map 1, area 62500).
   [RimWorldBot] DebugAction Snapshot: 62500 cells via GetCells (expected 62500, match=True).
   ```
7. Verifizieren:
   - **`match=True`** → AC-8 PASS (Array-Länge == map.Area).
   - **Erste Zahl in `GetCells scan`** unter 500ms → AC-6 PASS.
   - Keine `[RimWorldBot] GetCells: cell-mapping exception` Warnings → CRIT-1 nicht getriggert.

**Was zurückmelden:** „MT-5 PASS" + Player.log-Excerpt mit den 2 `[RimWorldBot]`-Lines. Falls Performance >500ms oder match=False: Player.log-Excerpt + Map-Größe (Hauptmenü → World → Map-Size, oder Save-File-Name).
-->

---

## 🟢 Optionale Verbesserungen (kein Blocker)

_(Aktuell keine.)_

---

## ✅ Done — Audit-Trail

- **2026-04-25** — D-1 entschieden: **Option A** (Production-csproj refactoren mit Game-Install-Refs, Krafs.Rimworld.Ref weg). Begründung: Standard-Setup für RimWorld-Mods, was getestet wird = was published wird.
- **2026-04-25** — D-2 implizit entschieden: **Variante A** (Story 1.14 sofort starten). MT-1 (Story 1.12 Game-Test) bleibt optional.
- **2026-04-25** — **MT-2 PASS** (Story 1.14 TC-14-PRODUCTION-LOAD): Player.log clean — `[RimWorldBot] initialized`, 5× State-Change-Logs (Off↔Advisory↔On), keine `MissingMethodException`/`TypeLoadException`. Story 1.14 → done.
- **2026-04-25** — **User-Bug-Report → D-39 Folder-Rename**: User entdeckte dass Mod-Aktivierung Vanilla-Texte auf Englisch zwang. Root-Cause: `Languages/Deutsch/` matcht nicht Vanilla-Konvention `German (Deutsch)/`. Story 1.8 retroactive zurück auf in-progress, Folder umbenannt, Specs aktualisiert.
- **2026-04-25** — **MT-4 PASS** (Story 1.8 Re-Verifikation nach D-39): Spiel + Mod beide auf Deutsch, EINE deutsche Sprach-Option. Story 1.8 → done. **Epic 1 komplett (14/14 Stories done).**
- **2026-04-25** — **MT-3 PASS** (Epic-1-Test-Marathon, alle 8 Schritte): Toggle-Button + Ctrl+K (6× state changes), Per-Pawn-Tab (Onesan PlayerUse=True), Settings-Window (kein Crash), Quest-Polling (Bot-Code clean trotz Vanilla-Grammar-Warnings), Save-Load von 2 Saves (State persistiert). Eine LOW-Anomaly entdeckt: `RegisterforPostLoadInit DecisionLogEntry`-Doppel-Warning bei Save-Load (kosmetisch, kein Daten-Verlust, escalated für Improvement Agent). **Sprint 2 vollständig in-game verifiziert.**
- **2026-04-25** — D-4 entschieden: **Sprint 3 starten** mit Story 2.1 (Map-Cell-Data-Basic-Scan). D-40 Sprint-Transition. Epic 2 (Map-Analyzer) in Bearbeitung.
- **2026-04-25** — **MT-5 PASS** (Story 2.1 Snapshot-Scan): Player.log `[RimWorldBot] DebugAction Snapshot: 62500 cells via GetCells (expected 62500, match=True)`. Scan unter 200ms-Log-Schwelle (deutlich unter AC-6 500ms-Limit). Story 2.1 → done. **Side-Finding** entdeckt: Story 1.4 MainTabWindow_BotControl benötigt `[StaticConstructorOnStartup]` für Texture2D-Asset-Loading → Story 1.4 retroactive auf in-progress, Fix angewendet (D-41), wieder done.

---

## ℹ️ Konventionen

- 🟡 Decisions = ich brauche deinen Input bevor ich weiterarbeiten kann
- 🔴 Manuelle Tests = nur du kannst das ausführen (RimWorld-Game-Tests, externe Services, etc.)
- 🟢 Optional = nice-to-have, kein Blocker
- 🔵 User Wunsch = Wunschäußerungen deinerseits
- ✅ Done = erledigt, Audit-Eintrag

Wenn du etwas erledigt hast oder eine Entscheidung getroffen hast: sag's mir kurz im Chat („MT-1 PASS" / „D-1 Option A"). Ich update die Datei und arbeite weiter.
