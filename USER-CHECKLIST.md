# User Checklist — RimWorld Bot Mod

**Was du als User noch machen musst.** Diese Datei wird vom Agent jedes Mal aktualisiert wenn etwas Neues für dich anfällt. Watchdog-Noise im Chat → hier nachschauen statt scrollen.

**Last updated:** 2026-04-25 (Sprint 3 / Epic 2 gestartet, Story 2.1 in-progress)

---

## 🟡 Decisions Needed (echte User-Entscheidungen, BMAD liefert nicht die Antwort)

_(Aktuell keine offen — Sprint 3 mit Story 2.1 läuft.)_

Empfehlung: **Variante A**, weil du es so wolltest und weil mit Epic-2-Code ohne stable Epic-1-Foundation Bug-Hunting brutal wird.

---

## 🔴 Manuelle Tests (du bist der einzige der das ausführen kann)

_(Aktuell keine offen — Sprint 3 wird neue MTs bringen sobald Stories laufen.)_

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

---

## ℹ️ Konventionen

- 🟡 Decisions = ich brauche deinen Input bevor ich weiterarbeiten kann
- 🔴 Manuelle Tests = nur du kannst das ausführen (RimWorld-Game-Tests, externe Services, etc.)
- 🟢 Optional = nice-to-have, kein Blocker
- 🔵 User Wunsch = Wunschäußerungen deinerseits
- ✅ Done = erledigt, Audit-Eintrag

Wenn du etwas erledigt hast oder eine Entscheidung getroffen hast: sag's mir kurz im Chat („MT-1 PASS" / „D-1 Option A"). Ich update die Datei und arbeite weiter.
