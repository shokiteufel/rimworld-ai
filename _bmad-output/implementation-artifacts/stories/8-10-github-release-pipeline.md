# Story 8.10: GitHub-Release-Pipeline (Actions + SHA256 + Lint)

**Status:** ready-for-dev
**Epic:** Epic 8 — Polish, Learning, Localization, DLC-Matrix
**Size:** M
**Decisions referenced:** F-STAB-08 (SHA256SUMS + Token-Scope + Lint)

## Story
Als Mod-Entwickler möchte ich **GitHub-Actions-Release-Pipeline**: auf Git-Tag `v*` wird automatisch gebaut, gelintet, gezippt, SHA256-geprüft und als Release-Asset publiziert.

## Acceptance Criteria
1. `.github/workflows/release.yml` GitHub-Actions-Workflow
2. Trigger: Push von Tag `v0.1.0-mvp`, `v1.0.0-release`, etc.
3. Steps: Checkout → setup-dotnet → msbuild → Pre-Release-Lint (About.xml ↔ LoadFolders.xml) → ZIP → SHA256SUMS → gh release create
4. Token-Scope: `contents:write` only
5. Artifact-Naming: `RimWorldBot-v{version}.zip`
6. Integration-Test: lokal mit `act` oder via Dummy-Tag auf develop-Branch

## Tasks
- [ ] `.github/workflows/release.yml`
- [ ] `Tools/pre-release-lint.ps1` (About.xml/LoadFolders-Consistency-Check)
- [ ] Test-Tag

## Dev Notes
**Kontext:** §10.3 + F-STAB-08.
**Vorausgesetzt:** 1.1 (CSProj), 1.8 (Localization), 8.9 (DLC-Matrix als Prerequisite).

## File List
| Pfad | Op |
|---|---|
| `.github/workflows/release.yml` | create |
| `Tools/pre-release-lint.ps1` | create |

## Testing
Integration: Test-Tag `v0.0.1-test` → Release-Draft erstellt.

## Review-Gate
Code-Review gegen §10.3, F-STAB-08.
