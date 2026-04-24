# Story 1.6: Per-Pawn-Toggle (eigener ITab + XML-Patch)

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle
**Size:** M
**Sprint:** 1
**Decisions referenced:** D-03 (Zwei-Ebenen-Toggle Master + Per-Pawn), D-04 (Start-Pawn-Auswahl via „Player Use"-Flag), D-13 (H8 raus, eigener ITab statt ITab_Pawn_Character-Patch), D-14 (UniqueLoadID-Keying)

---

## Story

Als **Spieler** möchte ich **pro Pawn eine Checkbox „Player Use" im Pawn-Inspector sehen und toggeln können**, damit **ich bestimmte Pawns manuell steuern kann während der Bot die übrigen übernimmt — auch mitten im laufenden Spiel**.

---

## Acceptance Criteria

1. **`Source/UI/ITab_Pawn_BotControl.cs`** implementiert `ITab_Pawn_BotControl : ITab` (eigener ITab, **kein** Patch auf `ITab_Pawn_Character` — D-13 H8 entfernt)
2. **`Defs/ITabDefs.xml`** registriert `ITab_Pawn_BotControl` als `InspectTabBase` (Def-Konvention statt direkte XML-Patch — funktioniert für eigene ITabs)
3. **`Patches/HumanInspectTabs.xml`** fügt via `<Operation Class="PatchOperationAdd">` die ITab-Reference zu `Human.inspectorTabs` hinzu (und analog zu `Colonist`, falls separate Liste in 1.6-Defs)
4. **ITab-Tab-Label** „Bot" (lokalisiert), Tab-Icon optional (default ok)
5. **Tab-Content** zeigt Checkbox „Player Use" — `true` = Spieler steuert manuell, `false` = Bot darf steuern; plus kurzer Hilfstext
6. **Persistenz** über `BotGameComponent.perPawnPlayerUse: Dictionary<string, bool>` keyed by `pawn.GetUniqueLoadID()` (AI-4/D-14)
7. **Default-Wert** für neue Pawns = `false` (Bot steuert) — außer in Start-Character-Creation (siehe Epic-8 Story für Start-Pawn-Override, hier nicht implementiert)
8. **Cleanup-Mechanismus** (aus Story 1.3, alle 60000 Ticks) verifiziert: Destroyed/Banished Pawns werden aus Dict entfernt — hier **keine** zusätzliche Logik nötig, nur Test dass Cleanup mit Story-1.6-Einträgen korrekt läuft
9. **Savegame-Roundtrip**: Checkbox-Zustand bleibt nach Save/Load erhalten (AC 4 Epic 1)
10. **Kein Konflikt** mit Character Editor, EdB Prepare Carefully, Yayo's Animation (D-13-Begründung für eigenen ITab statt Patch auf existierendem Tab)
11. **ITab sichtbar für Colonists UND Slaves** (Slaves werden in Epic 4 teils anders gehandhabt, aber Toggle-Präsenz schon hier)

---

## Tasks

- [ ] `Source/UI/ITab_Pawn_BotControl.cs` implementieren (erbt von `ITab`, override `FillTab`)
- [ ] `Defs/ITabDefs.xml` mit `<InspectTabBase>` → `<compClass>RimWorldBot.ITab_Pawn_BotControl</compClass>` Def
- [ ] `Patches/HumanInspectTabs.xml` mit PatchOperationAdd auf `Defs/ThingDef[defName="Human"]/inspectorTabs` (und optional auf Pawn-Subkind-Defs)
- [ ] Localization-Keys für Tab-Label + Checkbox-Label + Hilfstext (Vorgriff auf 1.8)
- [ ] Integration-Test TC-06-PER-PAWN: Colony mit 3 Pawns → zwei auf „Player Use" setzen → Save/Load → States bleiben
- [ ] Kompat-Test TC-06-COMPAT: Mod + Character Editor / Prepare Carefully aktivieren → beide ITabs sichtbar, kein Overlap

---

## Dev Notes

**Architektur-Kontext (v2.3):**
- **Kein Harmony-Patch** für ITab-Injection (AI-1 + D-13). XML-PatchOperations sind RimWorld-native und patch-bar durch Dritt-Mods ohne Konflikt.
- `perPawnPlayerUse` Keying via `UniqueLoadID` statt `thingIDNumber` (D-14, F-STAB-01/F-SAVE-02-Fix): save-stabil, cleanup-freundlich.
- ITab-Tab-Sichtbarkeit: `IsVisible` default true. Falls spätere Stories restriktieren wollen (z. B. nur bei Colonist, nicht bei Quest-NPC), override dort.

**Nehme an, dass:**
- `Defs/ThingDef[defName="Human"]/inspectorTabs` ist die korrekte XPath-Selektor — Standard in Modding-Community.
- Vanilla `InspectTabBase`-Loader handled eigene ITabs ohne Special-Registration (CompClass-Feld reicht).

**Vorausgesetzt:**
- Story 1.3 is done (`perPawnPlayerUse` Dictionary existiert im BotGameComponent + Cleanup-Logik).

---

## File List

| Pfad | Operation | Zweck |
|---|---|---|
| `Source/UI/ITab_Pawn_BotControl.cs` | create | Eigener Pawn-ITab |
| `Defs/ITabDefs.xml` | create | ITab-Def-Registration |
| `Patches/HumanInspectTabs.xml` | create | XML-Patch: ITab-Eintrag zu Human.inspectorTabs |
| `Languages/Deutsch/Keyed/PerPawnToggle.xml` | create | DE-Labels (Vorgriff 1.8) |
| `Languages/English/Keyed/PerPawnToggle.xml` | create | EN-Labels (Vorgriff 1.8) |

---

## Testing

**Unit-Tests:**
- `perPawnPlayerUse.Set(uniqueLoadID, true/false)` idempotent.
- Cleanup-Methode entfernt stale IDs.

**Integration-Tests:**
- **TC-06-PER-PAWN:** 3 Pawns in Colony, zwei auf PlayerUse = true → Save → Load → States korrekt.
- **TC-06-CLEANUP:** Pawn stirbt → nach 60000 Ticks ist sein Eintrag aus `perPawnPlayerUse` entfernt.
- **TC-06-COMPAT-MATRIX:** Mod + Character Editor + EdB Prepare Carefully + Yayo's Animation: keine Exception, alle ITabs sichtbar.

---

## Review-Gate

- Code-Review gegen D-13 (kein Patch auf ITab_Pawn_Character), D-14 (UniqueLoadID), AI-1 (kein Harmony).
- Kompat-Matrix Ergebnis dokumentiert.
- Visual-Review beim Design-Critic (wenn DEV-Phase aktiv): Tab-Layout, Checkbox-Darstellung.

---

## Aufgelöste Entscheidungen

- **TQ-S6-01 resolved:** **Eigener ITab** (`ITab_Pawn_BotControl`), nicht Gizmo. Begründung: D-13 hat das bereits als Option A oder B spezifiziert; eigener ITab ist weniger intrusiv für ausländische Mods und lässt sich pro Pawn-Typ filtern.
- **TQ-S6-02 resolved:** **PatchOperation statt `MayRequire` auf Human-Def**. Begründung: PatchOperation ist die Standard-Modding-Route für Def-Ergänzung; `MayRequire` ist für optionale Def-Felder (DLC-spezifisch) gedacht.
- **TQ-S6-03 resolved:** **Default `PlayerUse = false`** (Bot steuert). Begründung: PRD FR-03 und Architecture AC 2 Epic 1: Bot ist primäre Kontroll-Quelle, Player-Use ist bewusste Override-Geste durch den User.
