# User Checklist — RimWorld Bot Mod

**Was du als User noch machen musst.** Diese Datei wird vom Agent jedes Mal aktualisiert wenn etwas Neues für dich anfällt. Watchdog-Noise im Chat → hier nachschauen statt scrollen.

**Last updated:** 2026-04-25 (Epic 1 komplett ✓ — Test-Marathon offen)

---

## 🟡 Decisions Needed (echte User-Entscheidungen, BMAD liefert nicht die Antwort)

### D-3: Sprint 3 starten oder Epic-1-Test-Marathon zuerst?
**Was:** Epic 1 ist abgeschlossen (14/14 Stories done). Zwei Optionen wie es weitergeht:

- **Variante A:** Erst **Epic-1-Test-Marathon** (siehe MT-3 unten) — du verifizierst alle deferred + nicht-game-getesteten Stories vor Sprint-3-Start. Dein ursprünglicher Wunsch.
- **Variante B:** Sprint 3 sofort starten mit Epic 2 (Map-Analyzer) — Tests aufschieben.

Empfehlung: **Variante A**, weil du es so wolltest und weil mit Epic-2-Code ohne stable Epic-1-Foundation Bug-Hunting brutal wird.

---

## 🔴 Manuelle Tests (du bist der einzige der das ausführen kann)

### MT-3: Epic-1-Test-Marathon — **PFLICHT für Sprint-3-Start (Variante A)**
**Wann:** Vor Sprint-3-Start (deinem Wunsch nach 1.14 done).

**Setup:** RimWorld einmal starten (du hast nach MT-2 RimWorld eh offen).

**Schritte (reihe nach abklingen, je 1× klicken/triggern):**

1. **Stories 1.1-1.3 (Init/Component):** schon implizit verifiziert via MT-2 (`[RimWorldBot] initialized` + `BotGameComponent registered` Player.log-Lines).

2. **Story 1.4 Master-Toggle-Button** (Bottom-Bar „Bot"-Button, schon getestet aber Re-Verifikation wegen Refactor):
   - 1× Bot-Button klicken → State-Change-Log in Player.log.
   - Alle 3 States durchklicken: Off → Advisory → On → Off.

3. **Story 1.5 Ctrl+K-Keybinding:**
   - Ctrl+K drücken (Modifier zwingend, sonst kollidiert mit Vanilla Misc8).
   - Erwartet: gleiches State-Change-Log wie Button-Klick.

4. **Story 1.6 Per-Pawn-Toggle:**
   - Einen Pawn anklicken (Inspector öffnet sich).
   - **„Bot"-Tab** im Pawn-Inspector suchen (zwischen Health/Gear/Social).
   - Klick → Per-Pawn-Toggle-UI mit `playerUse`-Checkbox.
   - 1× toggle → Player.log sollte Decision-Log-Eintrag haben (oder zumindest kein Crash).

5. **Story 1.7 Settings-Window:**
   - Hauptmenü → Options → Mod Settings → "RimWorld Bot" auswählen.
   - 5 Sektionen sichtbar (Master, Ending-Strategy, Phase, Risk, Debug).
   - 1× Setting ändern + Apply → schließen + wieder öffnen → Wert persistiert.

6. **Story 1.8 Localization DE/EN:**
   - Sprache umstellen Hauptmenü → Options → Sprache → English.
   - Alles sollte englisch sein (kein „MissingTranslation"-Marker).
   - Zurück auf Deutsch.

7. **Story 1.12 QuestManager-Polling (MT-1 ehemals optional):**
   - Dev-Mode aktivieren (Options → Dev Mode).
   - Top-Bar Debug-Menu (Käfer-Icon) → "Execute incident" → "GiveQuest_*" auswählen (egal welche).
   - Quest-Letter erscheint.
   - ~21s warten (Bot pollt alle 1250 Ticks).
   - Player.log auf `QuestOfferEvent`/Quest-relevante Bot-Logs prüfen — Note: aktuell wird das nur queued, kein Consumer (Story 7.7/7.9 noch nicht da). Wichtig: **kein Crash**.

8. **Smoke-Save-Load:**
   - Save Game → Load Game.
   - Player.log: kein `[RimWorldBot] Migrate failure` o.ä.
   - Master-State und Per-Pawn-Toggle erhalten.

**Was zurückmelden:** „MT-3 PASS" plus den letzten Player.log-Stand. Falls einzelne Schritte fehlschlagen: nur die Schritt-Nummer + relevante `[RimWorldBot]`-Logs zitieren, ich fixe es.

---

## 🔵 Geplant nach Epic 1 (User-Wunsch 2026-04-25)

_(Konsolidiert in MT-3 oben.)_

---

## 🟢 Optionale Verbesserungen (kein Blocker)

_(Aktuell keine.)_

---

## ✅ Done — Audit-Trail

- **2026-04-25** — D-1 entschieden: **Option A** (Production-csproj refactoren mit Game-Install-Refs, Krafs.Rimworld.Ref weg). Begründung: Standard-Setup für RimWorld-Mods, was getestet wird = was published wird.
- **2026-04-25** — D-2 implizit entschieden: **Variante A** (Story 1.14 sofort starten). MT-1 (Story 1.12 Game-Test) bleibt optional.
- **2026-04-25** — **MT-2 PASS** (Story 1.14 TC-14-PRODUCTION-LOAD): Player.log clean — `[RimWorldBot] initialized`, 5× State-Change-Logs (Off↔Advisory↔On), keine `MissingMethodException`/`TypeLoadException`. Story 1.14 → done. **Epic 1 komplett (14/14 Stories done).**

---

## ℹ️ Konventionen

- 🟡 Decisions = ich brauche deinen Input bevor ich weiterarbeiten kann
- 🔴 Manuelle Tests = nur du kannst das ausführen (RimWorld-Game-Tests, externe Services, etc.)
- 🟢 Optional = nice-to-have, kein Blocker
- 🔵 User Wunsch = Wunschäußerungen deinerseits
- ✅ Done = erledigt, Audit-Eintrag

Wenn du etwas erledigt hast oder eine Entscheidung getroffen hast: sag's mir kurz im Chat („MT-1 PASS" / „D-1 Option A"). Ich update die Datei und arbeite weiter.
