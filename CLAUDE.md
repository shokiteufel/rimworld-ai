# RimWorld Bot Mod — Projekt-Kontext

Dieses Projekt ist ein **BMAD-Projekt**. Guardian-Rules aus `~/.claude/CLAUDE.md` sind in jeder Session aktiv.

## Kurz-Beschreibung

Autonome Entscheidungs-KI als RimWorld-Mod. Führt Kolonien vom Nackt-Start bis zum Ending, mit Toggle (global + per Pawn), Map-Analyse bei Start und Cross-Game-Lernsystem.

## Artefakt-Hierarchie (Autorität, Guardian-kanonische Pfade)

1. **`_bmad-output/planning-artifacts/prd.md`** — Product Requirements. Was die Mod tut.
2. **`_bmad-output/planning-artifacts/architecture.md`** — Technische Architektur. Wie die Mod aufgebaut ist.
3. **`_bmad-output/planning-artifacts/epics.md`** — Alle 8 Epics konsolidiert mit Critical Path.
4. **`_bmad-output/implementation-artifacts/stories/*.md`** — Dev-Stories mit Akzeptanzkriterien (wird pro Sprint gefüllt).
5. **`_bmad-output/implementation-artifacts/sprint-status.yaml`** — Phase + Sprint + Story-Stati. Von Guardian gelesen.
6. **`_bmad/decisions.md`** — Chronologisches Decision-Log.

## Sekundäre Referenzen (Runtime-Spec + Research)

Diese Dokumente sind **nicht** Design-Autoritäten, aber wichtige Wissensquellen:

- **`Mod-Leitfaden.md`** — Runtime-Spezifikation (Invariants, Emergency-Handler, Phase-State-Machine, Entscheidungsbäume). Wird als Laufzeit-Regelwerk genutzt, bleibt aktuell.
- **`Mod-Konzept.md`** — Legacy-Design-Dokument. Wurde in prd.md + architecture.md aufgesplittet. Nicht mehr Autorität, aber historisch erhalten für Kontext.
- **`Skill Cap.md`** — Alle Hard-Caps aus den XML-Defs verifiziert.
- **`Abhängigkeitsbaum.md`** — Tech-Tree mit Skills, Ressourcen, Research pro Item.
- **`Ending-Pfade.md`** — Menschlicher Überblick der fünf Endings.

Bei Widerspruch zwischen PRD/Architecture und diesen Sekundär-Docs → PRD/Architecture gewinnt, Sekundär-Doc wird upgedatet.

## BMAD-Workflow-Regeln (projektspezifisch)

- **Session-Start:** Immer zuerst `/guardian` aufrufen. Guardian liest `_bmad-output/implementation-artifacts/sprint-status.yaml` und startet die passenden Watchdogs.
- **Phase-Wechsel:** Nur per expliziter Entscheidung. Eintrag in `_bmad/decisions.md` und Update von `sprint-status.yaml`.
- **Neue Entscheidung:** Vor Implementation zuerst in `_bmad/decisions.md` dokumentieren.
- **Story-Drafting:** Erst wenn PRD + Architecture approved sind. Stories leben in `_bmad-output/implementation-artifacts/stories/`.
- **Code schreiben:** Erst nach Story-Draft mit Akzeptanzkriterien. Keine „mal eben"-Features.
- **Findings:** Jeder Watchdog-Fund wird gefixt — kein Cherry-Picking, kein „später".

## Zielsetzung der Mod (Kurz)

- **Typ:** Harmony-C#-Mod, Vanilla + alle DLCs
- **Distribution:** GitHub primär
- **Sprachen:** Deutsch + Englisch
- **Ziel-Feature:** Spielt RimWorld autonom, hält alle 5 Endings offen, wechselt Ending-Strategie opportunistisch
- **Scope:** Regelbasiertes Learning, kein ML

## Werkzeuge & Pfade

- **Game-Install:** `D:\SteamLibrary\steamapps\common\RimWorld\`
- **XML-Defs als Truth-Source:** `D:\SteamLibrary\steamapps\common\RimWorld\Data\Core|Royalty|Ideology|Biotech|Anomaly|Odyssey\`
- **Save-Data (für Lern-Datei):** `%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld\Config\RimWorldBot_LearnedConfig.xml`
