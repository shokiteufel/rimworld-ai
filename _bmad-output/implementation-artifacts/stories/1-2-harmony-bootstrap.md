# Story 1.2: Harmony-Bootstrap (RimWorldBotMod Entry-Point)

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle
**Size:** S
**Sprint:** 1
**Decisions referenced:** D-01, D-12 (Tick-Host in GameComponentTick, kein Patch auf UpdatePlay), D-13 (Minimal-Harmony-Strategie), D-24 (Event-Queue-Lifecycle — hier noch nicht aktiv aber Mod-Klasse wird Composition-Root später)

---

## Story

Als **Mod-Entwickler** möchte ich **die `RimWorldBotMod : Mod`-Klasse mit Harmony-Init einrichten**, damit **alle verbleibenden Harmony-Patches (H2–H6, kommen ab Story 1.3/1.4/1.6) einen stabilen Init-Punkt haben und der `[StaticConstructorOnStartup]`-Skeleton-Probe aus Story 1.1 durch den produktiven Entry-Point ersetzt ist**.

---

## Acceptance Criteria

1. **`RimWorldBotMod : Mod`-Klasse** existiert in `Source/Core/RimWorldBotMod.cs` mit Konstruktor `public RimWorldBotMod(ModContentPack content) : base(content)`
2. **Harmony-Instanz** wird im Konstruktor mit Harmony-ID `mediainvita.rimworldbot` erzeugt (`new Harmony("mediainvita.rimworldbot")`)
3. **`harmony.PatchAll(Assembly.GetExecutingAssembly())`** wird im Konstruktor aufgerufen — findet zu diesem Zeitpunkt keine Patches (die kommen ab 1.3), aber Call ist da
4. **Singleton-Zugriff** `RimWorldBotMod.Instance` via `public static RimWorldBotMod Instance { get; private set; }` (im Konstruktor gesetzt). Entspricht AI-4 (einziges zulässiges statisches Singleton)
5. **Log-Message** beim Mod-Load: `Log.Message("[RimWorldBot] initialized, Harmony patches: 0")` (Patch-Zahl via `harmony.GetPatchedMethods().Count()`)
6. **Skeleton-Probe aus 1.1 entfernt** — `Source/Core/ModEntry.cs` gelöscht oder umbenannt, weil `RimWorldBotMod`-Konstruktor seine Funktion übernimmt
7. **Mod lädt weiterhin** ohne Fehler (vergleich TC-01-SKELETON, AC 6+7+8 aus Story 1.1)
8. **Keine Harmony-Exception-Logs** beim Mod-Load (auch wenn `PatchAll` nichts findet, sollte keine Warning kommen)

---

## Tasks

- [ ] `Source/Core/RimWorldBotMod.cs` anlegen mit Klasse `RimWorldBotMod : Verse.Mod`
- [ ] `using HarmonyLib;` + Harmony als `modDependencies`-Eintrag in About.xml prüfen (aus Story 1.1 übernommen)
- [ ] Im Konstruktor: `Instance = this; var harmony = new Harmony("mediainvita.rimworldbot"); harmony.PatchAll(Assembly.GetExecutingAssembly()); Log.Message($"[RimWorldBot] initialized, Harmony patches: {harmony.GetPatchedMethods().Count()}");`
- [ ] `Source/Core/ModEntry.cs` (Skeleton-Probe aus Story 1.1) löschen
- [ ] Build-Probe: `msbuild` / `dotnet build` → `Assemblies/RimWorldBot.dll` kompiliert ohne Warnings
- [ ] Integration-Test TC-01-HARMONY: Mod aktivieren → Game-Load → Player.log zeigt „RimWorldBot initialized, Harmony patches: 0" ohne Errors

---

## Dev Notes

**Architektur-Kontext (v2.3):**
- `RimWorldBotMod` ist der **einzige** statische Singleton im Projekt (AI-4). Alle anderen Klassen (BotController, Planner, etc.) werden über `BotControllerFactory` per Konstruktor-Injection verdrahtet (Story 1.3).
- **Kein Harmony-Patch auf Tick-Loop** (AI-1, D-12). `harmony.PatchAll()` findet in dieser Story noch keine Patches — `[HarmonyPatch]`-Klassen existieren noch nicht. Das ist ok, `PatchAll` ist idempotent.
- **CompatInitializer** (`[StaticConstructorOnStartup]`-Klasse, §4.2) wird **nicht** in dieser Story angelegt — erst wenn tatsächliche Patches da sind (frühestens Story 2.x für H2-Map.FinalizeInit). Für 1.2 ist Harmony-Init ohne Konflikt-Scan ausreichend.

**Nehme an, dass:**
- Harmony-Package `brrainz.harmony` ist als `modDependencies` in About.xml deklariert (Story 1.1 ACs) und wird von RimWorld vor unserer Mod geladen. `HarmonyLib`-NuGet-Reference in `.csproj` ist Build-Time only.
- Harmony-ID `mediainvita.rimworldbot` ist eindeutig (nicht von anderer Mod beansprucht) — matcht packageId aus 1.1.
- `ModContentPack` parameter wird von RimWorld automatisch injiziert, manuelles Wiring nicht nötig.

**Vorausgesetzt:**
- Story 1.1 ist `done` (Skeleton + About.xml + DLL-Build funktioniert).
- Harmony-Dependency im About.xml erwähnt Version ≥ 2.2.

---

## File List

| Pfad | Operation | Zweck |
|---|---|---|
| `Source/Core/RimWorldBotMod.cs` | create | Mod-Entry-Point mit Harmony-Init |
| `Source/Core/ModEntry.cs` | delete | Story-1.1-Skeleton-Probe wird ersetzt |
| `Assemblies/RimWorldBot.dll` | update (via build) | Neue DLL mit RimWorldBotMod-Klasse |

---

## Testing

**Unit-Tests:** Keine (Framework-Init, nicht pure-Logik).

**Integration-Tests:**
- **TC-01-HARMONY:** Mod aktivieren → Load → Log-Message mit Patch-Count 0 sichtbar, keine Errors.
- **TC-02-HARMONY-CONFLICTS:** Mit anderen Harmony-Mods parallel aktiviert (z. B. Common Sense, Allow Tool) → kein Konflikt (da Patch-Count 0 in dieser Story).

**Manuelle Checks:**
- `RimWorldBotMod.Instance` ist non-null nach Game-Load (via Dev-Mode Inspector).

---

## Review-Gate (BMAD-Loop)

Code-Review gegen:
- AI-4-Invariante (Singleton-Korrektheit)
- Harmony-Init-Pattern (Standard-Modding-Konvention)
- Log-Message-Format konsistent mit Story-1.1-Skeleton

Kein Visual-Review (keine UI).

---

## Aufgelöste Entscheidungen (waren TBDs)

- **TQ-S2-01 resolved:** **Harmony-ID = packageId = `mediainvita.rimworldbot`**. Begründung: Vereinfacht Debugging (ein Identifier für Mod-Paket + Harmony-Instanz); Konvention in der Modding-Community.
- **TQ-S2-02 resolved:** **Keine `[StaticConstructorOnStartup]`-Klasse in 1.2** — Init läuft im `Mod`-Konstruktor. Begründung: `CompatInitializer` (§4.2) wird erst gebraucht wenn echte Patches + `DefDatabase<CompatPatternDef>`-Lesung nötig sind (ab Story 2.x oder später). Kein Pre-Optimize.
