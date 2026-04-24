# Story 2.7: Overlay-Rendering (Top-3-Kreise + Score-Breakdown-Tooltip)

**Status:** ready-for-dev
**Epic:** Epic 2 — Map-Analyzer
**Size:** M
**Decisions referenced:** D-08 (AI_ADVISORY-Overlay-Semantik), F-AI-06 (Score-Breakdown im UI)

## Story
Als Spieler im **AI_ADVISORY-Mode** möchte ich die **Top-3-Sites als farbige Kreise auf der Map sehen**, mit Hover-Tooltip der Score-Breakdown (W_FOOD-Beitrag, W_DEFENSE-Beitrag, W_HAZARD-Penalty etc.) zeigt — damit ich nachvollziehen kann warum der Bot diese Sites vorschlägt (Entscheidungs-Transparenz).

## Acceptance Criteria
1. `SiteMarkerOverlay : MapComponent` in `Source/UI/SiteMarkerOverlay.cs` mit Override `MapComponentOnGUI`
2. Overlay nur aktiv wenn `BotGameComponent.masterState == AI_ADVISORY` (D-08)
3. Drei Kreise mit verschiedenen Farben: Top-1 grün, Top-2 gelb, Top-3 orange; Radius proportional zu `ClusterResult.CellCount`
4. Hover auf Kreis → Tooltip mit `ScoreBreakdown` (aus Story 2.5): Label + Wert pro Weight-Kategorie
5. Toggle-Key (eigenes KeyBinding `RimWorldBot_ToggleOverlay`, default `Alt+K`) zeigt/versteckt Overlay temporär
6. Overlay performanz-neutral im AI_OFF/AI_ON-Mode (kein Cell-Iteration)
7. Integration-Test: AI_ADVISORY → drei Kreise sichtbar → Hover zeigt Breakdown
8. **Exception-Wrapper** (HIGH-Fix Round-2-Stability, CC-STORIES-02): `MapComponentOnGUI()`-Hauptkörper + Overlay-Draw-Aufrufe via Story 1.10 `ExceptionWrapper.TickHost(...)` wrappen. Bei 2 Exceptions/min → `FallbackToOff()`. Begründung: OnGUI-Loop läuft pro Frame; ungefangene Exception in Overlay-Draw blockiert Vanilla-UI.
9. **Schema-Bump** (HIGH-Fix Round-2-Stability, CC-STORIES-01): `overlayVisible` in `BotMapComponent` via Story 1.9 `SchemaVersionRegistry` registriert; Migrate setzt `overlayVisible = true` (default).

## Tasks
- [ ] `Source/UI/SiteMarkerOverlay.cs` als MapComponent
- [ ] Widgets.DrawLine + GUI.Box für Kreise (oder `GenDraw.DrawCircleOutline`)
- [ ] Tooltip via `TooltipHandler.TipRegion`
- [ ] KeyBindingDef `RimWorldBot_ToggleOverlay`
- [ ] `BotMapComponent.overlayVisible: bool` (persistiert, default true)
- [ ] Integration-Test AI_ADVISORY-Sichtbarkeit

## Dev Notes
**Architektur-Kontext:** §2.5 UI + F-AI-06 Score-Breakdown-Tooltip Pflicht.
**Nehme an, dass:** `MapComponentOnGUI` läuft im MainMap-UI-Context (nicht im Event-Window).
**Vorausgesetzt:** Story 2.5/2.6, Story 1.3 (BotGameComponent.masterState), Story 1.5 (KeyBinding-Pattern).

## File List
| Pfad | Op |
|---|---|
| `Source/UI/SiteMarkerOverlay.cs` | create |
| `Defs/KeyBindingDefs.xml` | modify (Toggle-Overlay-Key) |
| `Source/Data/BotMapComponent.cs` | modify (overlayVisible-Feld) |

## Testing
- Integration: AI_ADVISORY aktiv → 3 Kreise; AI_OFF → keine Kreise
- Tooltip-Interaction: Hover auf Top-1 → Breakdown sichtbar

## Review-Gate
Code-Review gegen F-AI-06, D-08 (nur ADVISORY-Overlay). **Visual-Review via Design-Critic-Watchdog** (UI-Komponente, Story 2.7 ist erste UI-heavy Story).
