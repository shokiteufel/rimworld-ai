# Story 8.8: Telemetry-Logger (JSONL, opt-in, PII-frei)

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** M
**Decisions referenced:** F-STAB-07 (Telemetry default OFF + PII-Scrub + Rotation)

## Story
Als Mod-Entwickler möchte ich **Telemetry-Logger** — JSONL-Events (Emergencies, Phase-Transitions, Ending-Switches), default OFF, opt-in via Settings, keine PII.

## Acceptance Criteria
1. `Source/Data/TelemetryLogger.cs`
2. Default OFF (F-STAB-07); Opt-in via Story 1.7 Settings
3. Log-Pfad: `GenFilePaths.ConfigFolderPath + "/RimWorldBot/telemetry.jsonl"`
4. Events: Emergency-Resolve, Phase-Transition, Ending-Switch, CrashRecovery
5. Pawn-Namen immer via `GetUniqueLoadID()`, nie `pawn.Name.ToStringShort`
6. Rotation: 10 MB → `.1.jsonl`, keep-3, total 30 MB
7. Restart-Required-Hint bei Opt-in-Flip
8. Unit-Tests PII-Scrub + Rotation

## Tasks
- [ ] `Source/Data/TelemetryLogger.cs`
- [ ] PII-Scrub-Helper (UniqueLoadID statt Name)
- [ ] Rotation-Logik
- [ ] Unit-Tests

## Dev Notes
**Kontext:** §2.6 + F-STAB-07, NFR-06.
**Vorausgesetzt:** 1.7 (Settings Opt-in).

## File List
| Pfad | Op |
|---|---|
| `Source/Data/TelemetryLogger.cs` | create |

## Testing
Unit: PII-Scrub (Pawn-Name nicht in Output), Rotation bei 10MB.

## Review-Gate
Code-Review gegen F-STAB-07 strict.
