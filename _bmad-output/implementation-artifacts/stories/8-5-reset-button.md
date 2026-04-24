# Story 8.5: Reset-Button (volle Implementation)

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** S

## Story
Als Spieler möchte ich **volle Reset-Button-Implementierung** (Story 1.7 Placeholder ersetzen): LearnedConfig-File wird gelöscht, alle Learning-Daten weg, Bot startet mit Compiled-Defaults.

## Acceptance Criteria
1. Settings-Panel-Button „Reset LearnedConfig" (aus 1.7) ruft `LearnedConfig.ResetToDefaults()`
2. Confirmation-Dialog mit Warnung „All cross-game learning will be lost"
3. File-Delete via atomic-rename-to-backup (`.reset-<ts>` als Safety)
4. User-Toast „Learning data reset — defaults restored"
5. Unit-Tests

## Tasks
- [ ] `LearnedConfig.ResetToDefaults` implementieren
- [ ] Confirmation-Dialog in Settings-Panel verbinden
- [ ] Unit-Tests

## Dev Notes
**Kontext:** D-07 Reset-Button, Story 1.7 Placeholder.
**Vorausgesetzt:** 8.1, 1.7.

## File List
| Pfad | Op |
|---|---|
| `Source/Data/LearnedConfig.cs` | modify |
| `Source/UI/SettingsRenderer.cs` | modify |

## Testing
Unit: Reset-Sicherheit.

## Review-Gate
Code-Review gegen D-07, Safety-Backup.
