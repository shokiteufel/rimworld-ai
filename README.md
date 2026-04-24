# RimWorld Bot Mod

Autonome Entscheidungs-KI als RimWorld-Mod. Führt Kolonien vom Nackt-Start bis zum Ending, mit Toggle (global + per Pawn) und Cross-Game-Lernsystem.

**Status:** Dev-Phase — Sprint 2 (Epic 1: Mod-Skeleton & Toggle). 99 Stories ready-for-dev nach 3 Review-Passes. Story 1.1 (Mod-Projekt-Skeleton) in Entwicklung.

---

## Projekt-Struktur (Guardian-kanonisch)

```
RimWorld Bot/
├── About/                              RimWorld Mod-Manifest
│   ├── About.xml                       packageId, name, DLC-Deps
│   └── Preview.png                     Mod-Icon (256×256)
├── Source/                             C#-Source + .csproj
│   ├── RimWorldBot.csproj              Build-Config (net472 + Krafs.Rimworld.Ref)
│   └── Core/ModEntry.cs                Skeleton Load-Probe (Story 1.1)
├── Assemblies/                         Build-Output (RimWorldBot.dll)
├── LoadFolders.xml                     Multi-Version-Support (1.5 + 1.6)
│
├── _bmad/                              Guardian-Hilfsmetadaten
│   └── decisions.md                    Decision-Log (chronologisch)
├── _bmad-output/                       Offizielle Artefakte (Autorität)
│   ├── planning-artifacts/
│   │   ├── prd.md                      Product Requirements
│   │   ├── architecture.md             Technische Architektur
│   │   └── epics.md                    Alle 8 Epics + Critical Path
│   └── implementation-artifacts/
│       ├── sprint-status.yaml          Phase + Sprint + Story-Stati (Guardian-Input)
│       ├── api-reference.md            Verifizierte Vanilla-Defs
│       ├── reviews/                    Party-Mode-Review-Reports
│       └── stories/                    Dev-Stories (99 Stories, Epic 1-8)
├── CLAUDE.md                           Projekt-Kontext + BMAD-Regeln
├── README.md                           Diese Datei
│
├── Mod-Leitfaden.md                    Runtime-Spec (Invariants, Phase-Machine, Entscheidungsbäume)
├── Mod-Konzept.md                      Legacy-Design-Doku (durch prd+architecture ersetzt)
├── Skill Cap.md                        Research: Alle Hard-Caps aus XML-Defs
├── Abhängigkeitsbaum.md                Research: Tech-Tree mit Kosten + Research-Chain
└── Ending-Pfade.md                     Research: Überblick der 5 Endings
```

---

## Einstieg für Entwickler

1. **Kontext lesen:** `CLAUDE.md` → versteht Projekt-Setup und BMAD-Workflow.
2. **Anforderungen verstehen:** `_bmad-output/planning-artifacts/prd.md` → funktionale + non-funktionale Requirements.
3. **Architektur verstehen:** `_bmad-output/planning-artifacts/architecture.md` → Komponenten, Hooks, Datenfluss.
4. **Aktuellen Stand prüfen:** `_bmad-output/implementation-artifacts/sprint-status.yaml` → welcher Sprint, welche Story-Stati.
5. **Nächste Arbeit:** Epic-Details in `_bmad-output/planning-artifacts/epics.md` → zugehörige Stories in `_bmad-output/implementation-artifacts/stories/`.

---

## Kern-Features (aus PRD)

- **Toggle-System:** Master-Button (alle Pawns) + Per-Pawn-Checkbox (Player Use). State persistiert im Savegame.
- **Map-Analyse:** Scannt Karte bei Start, bewertet Zellen nach Food, Defense, Ressourcen, Hazards (Lava/Gift/Pollution). Top-3-Sites-Overlay bei AI_ADVISORY.
- **Phase-State-Machine:** 8 sequentielle Phasen (0a Map → 0 Survival → … → 7 Ending).
- **Invariants:** 12 Laufzeit-Checks, die nie verletzt werden dürfen. Verstoß → Emergency-Handler.
- **Opportunistisches Ending:** Bot wählt dynamisch realistischstes Ending, Force-Option per Settings.
- **Cross-Game-Lernen:** Gewichte + Override-Regeln persistieren zwischen Spielen.
- **Full-DLC-Kompatibilität:** Vanilla, Royalty, Ideology, Biotech, Anomaly, Odyssey.

---

## Milestones (Übersicht)

| Milestone | Status | DoD |
|---|---|---|
| MVP | Planning | PRD approved, Architecture approved, Epic 1-3 story-drafted, Phase 0+1 laufen |
| Alpha | Pending | Phase 2-4, Work-Assigner, Build-Planner, Raid-Handler |
| Beta | Pending | Phase 5-6, alle 12 Invariants, Caravan-Manager, Status-Overlay |
| Release | Pending | Alle 5 Endings, DLC-Matrix getestet, Lern-System, Localization DE/EN, GitHub-Release |

Detail: `_bmad-output/implementation-artifacts/sprint-status.yaml`.

---

## Installation (wenn verfügbar)

Noch nicht installierbar. Bei Release:
1. Repo klonen oder ZIP herunterladen
2. Ordner nach `RimWorld/Mods/` verschieben
3. In RimWorld unter „Mods" aktivieren
4. Am Ende der Load-Order einsortieren

---

## Entwicklungs-Leitlinien

- **BMAD-Workflow ist Pflicht** — keine Code-Arbeit ohne approved Story
- **Vanilla-Regeln gelten** — Bot cheatet nicht, trifft Entscheidungen im selben Regel-Rahmen wie ein Spieler
- **Minimal-Patches** — Harmony-Patches nur dort wo unvermeidbar
- **Dokumentation synchron halten** — Code-Änderung ohne Doc-Update ist Finding
- **Tests für pure Logic** — Scoring, Phase-Detection, Feasibility werden unit-getestet
- **Keine Raw-Food-Optimierung ignorieren** — Kochen ist 1.8× effizienter, Default `allow_raw_eating = false` (siehe D-02 im Decision-Log)

---

## Lizenz

TBD (wird vor Release festgelegt — Kandidaten: MIT, GPL-3.0).
