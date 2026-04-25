# User Checklist — RimWorld Bot Mod

**Was du als User noch machen musst.** Diese Datei wird vom Agent jedes Mal aktualisiert wenn etwas Neues für dich anfällt. Watchdog-Noise im Chat → hier nachschauen statt scrollen.

**Last updated:** 2026-04-25 (Session: Story 1.14 in Implementation, Option A)

---

## 🟡 Decisions Needed (echte User-Entscheidungen, BMAD liefert nicht die Antwort)

_(Aktuell keine offen.)_

---

## 🔴 Manuelle Tests (du bist der einzige der das ausführen kann)

### MT-1: Story 1.12 Game-Test (QuestManager-Polling) — **OPTIONAL**
**Wann:** Bevor Sprint 2 Epic 1 als komplett abgeschlossen gilt, ODER wenn du Story 1.12 zusätzlich validieren willst.

**Schritte:**
1. RimWorld starten, neues Game beginnen (Phase 1 colony, Bot Off oder Advisory).
2. Quest auslösen — entweder:
   - Dev-Mode aktivieren → Debug-Menu → "Execute incident: GiveQuest_*" auswählen, ODER
   - Organische Quest abwarten (Vanilla-Quest-Spawn alle ~3-15 Tage In-Game-Zeit, Speed-3 nutzen).
3. ~21 Sekunden warten (PollIntervalTicks=1250 @ 60TPS).
4. **Player.log** öffnen: `%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld\Player.log`
5. Suchen nach:
   - Keine `[RimWorldBot]`-Exception (sollte BotSafe.SafeTick abfangen).
   - Falls Debug-Logging aktiv: Eintrag dass `QuestOfferEvent` enqueued wurde.
6. Quest verwerfen (Reject) oder ablaufen lassen → erneut ~21s warten.
7. **Player.log:** `QuestRemovedEvent` enqueued.
8. Save + Load: persistent `lastSeenQuestIds` verhindert Re-Detection bestehender Quests.

**Was zurückmelden:** „PASS" oder Player.log-Excerpt mit `[RimWorldBot]`-Lines + welche Schritte fehlgeschlagen sind.

### MT-2: Story 1.14 Game-Test (TC-14-PRODUCTION-LOAD) — **PFLICHT für Story 1.14 done**
**Wann:** Nachdem ich Production-csproj refactored habe (Krafs.Rimworld.Ref weg, echte Game-DLLs als Refs). Bevor Story 1.14 done werden kann.

**Warum:** Refactor ändert die Build-Pipeline der Production-DLL. Wir müssen sicherstellen dass die Mod weiterhin in RimWorld lädt — wenn Krafs-Stubs und echte RimWorld-Assemblies signature-mäßig leicht abweichen, könnten Verse-API-Calls bei Runtime crashen (`MissingMethodException` / `TypeLoadException`).

**Schritte:**
1. RimWorld starten (komplett neu, falls schon offen).
2. Im Hauptmenü → Mods → "RimWorld Bot" muss in der Mod-Liste auftauchen.
3. Mod aktivieren falls nicht aktiv → RimWorld-Restart.
4. Neues Game beginnen (irgendeine Map, Phase 1 reicht).
5. **Player.log** prüfen: `%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld\Player.log`
6. Erwartet:
   - `[RimWorldBot] skeleton loaded` o.ä. Init-Log
   - Keine `MissingMethodException`, keine `TypeLoadException`, keine `[RimWorldBot]` Errors
7. Master-Toggle testen: Top-Bar-Button drücken → State-Wechsel-Log in Player.log.
8. Game schließen.

**Was zurückmelden:** „MT-2 PASS" oder Player.log-Excerpt + welcher Schritt fehlgeschlagen ist.

---

## 🔵 Geplant nach Epic 1 (User-Wunsch 2026-04-25)

### Epic-1-Abschluss-Test-Marathon
**Wann:** Nach Story 1.14 done (= Epic 1 komplett).
**Was:** Du willst alle deferred + nicht-game-getesteten Stories durchprobieren bevor wir Epic 2 starten. Dazu gehören:

- **MT-1** (Story 1.12 QuestManager-Polling) — Game-Test wie unten beschrieben
- **MT-2** (Story 1.14 Production-Load) — Refactor-Verifikation
- **Stories 1.9 + 1.10 + 1.11** — Cross-Cutting-Infra ohne UI, gemeinsam mit MT-1/MT-2 als Smoke-Test (kein Crash bei Game-Start beweist dass SchemaRegistry/BotSafe/PlanArbiter geladen werden)
- Falls du willst: Re-Verifikation Stories 1.4-1.8 (UI-Stories) — Master-Toggle, Ctrl+K, Per-Pawn-ITab, Settings-Window, Localization DE/EN

Ich erstelle pro Story einen kompakten Test-Schritt + erwartetes Player.log-Output und sammle die Liste hier in einer "MT-3"-Sektion sobald 1.14 done ist.

---

## 🟢 Optionale Verbesserungen (kein Blocker)

_(Aktuell keine.)_

---

## ✅ Done — Audit-Trail

- **2026-04-25** — D-1 entschieden: **Option A** (Production-csproj refactoren mit Game-Install-Refs, Krafs.Rimworld.Ref weg). Begründung: Standard-Setup für RimWorld-Mods, was getestet wird = was published wird.
- **2026-04-25** — D-2 implizit entschieden: **Variante A** (Story 1.14 sofort starten). MT-1 (Story 1.12 Game-Test) bleibt optional.

---

## ℹ️ Konventionen

- 🟡 Decisions = ich brauche deinen Input bevor ich weiterarbeiten kann
- 🔴 Manuelle Tests = nur du kannst das ausführen (RimWorld-Game-Tests, externe Services, etc.)
- 🟢 Optional = nice-to-have, kein Blocker
- 🔵 User Wunsch = Wunschäußerungen deinerseits
- ✅ Done = erledigt, Audit-Eintrag

Wenn du etwas erledigt hast oder eine Entscheidung getroffen hast: sag's mir kurz im Chat („MT-1 PASS" / „D-1 Option A"). Ich update die Datei und arbeite weiter.
