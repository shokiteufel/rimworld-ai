# Story 1.2 Code-Review

**Datum:** 2026-04-24
**Reviewer:** BMAD Code-Review-Subagent
**Scope:** Statische Verifikation gegen 8 ACs + AI-1/AI-4. Build wurde vom Dev-Subagent bereits als 0-warn/0-err bestätigt. TC-01-HARMONY Laufzeit-Test ist nicht lokal reproduzierbar (Game-Start) — Code-Smell-Prüfung stattdessen.
**Verdict:** APPROVE

## AC-Coverage

- **AC 1 `RimWorldBotMod : Mod` + Konstruktor — PASS.** `Source/Core/RimWorldBotMod.cs` Z.10-14: Klasse erbt `Mod` (aus `Verse`), Konstruktor-Signatur `public RimWorldBotMod(ModContentPack content) : base(content)` exakt wie spezifiziert. Namespace `RimWorldBot.Core` konsistent mit Story 1.1.
- **AC 2 Harmony-Instanz mit ID `mediainvita.rimworldbot` — PASS.** Z.17 `new Harmony("mediainvita.rimworldbot")`, matcht packageId aus About.xml (TQ-S2-01 resolution).
- **AC 3 `PatchAll(Assembly.GetExecutingAssembly())` — PASS.** Z.18. `using System.Reflection` korrekt.
- **AC 4 Singleton `Instance` (AI-4) — PASS.** Z.12 `public static RimWorldBotMod Instance { get; private set; }`, Set in Z.16. Einziger static-state-holder im `Source/`-Baum (via Grep verifiziert — nur das eine Match).
- **AC 5 Log-Message-Format — PASS.** Z.19 exakt `"[RimWorldBot] initialized, Harmony patches: {count}"` via `GetPatchedMethods().Count()`. `using System.Linq` korrekt für `.Count()`.
- **AC 6 `ModEntry.cs` gelöscht — PASS.** `git status` zeigt `deleted: Source/Core/ModEntry.cs`, `ls Source/Core/` listet nur noch `RimWorldBotMod.cs`.
- **AC 7 Mod lädt weiterhin — PASS (statisch).** Keine Null-Refs, kein File-IO, kein Reflection-Throw-Risiko im Konstruktor. `ModContentPack` wird von RimWorld injiziert. Build ist clean.
- **AC 8 Keine Harmony-Exception-Logs — PASS (statisch).** `PatchAll` auf Assembly ohne `[HarmonyPatch]`-Klassen ist idempotent und wirft nicht. Keine `try/catch`-Noise nötig.

## Architektur-Invarianten

- **AI-4 (einziger Singleton):** PASS. Grep `static\s+\w+\s+\w+\s*(\{|=|;)` über `Source/` liefert exakt einen Treffer — `Instance` in RimWorldBotMod.
- **AI-1 (keine Harmony-Tick-Patches):** PASS. Assembly enthält keine `[HarmonyPatch]`-Klassen, `PatchAll` no-op — erwarteter Zustand für Sprint 1.

## Findings

### HIGH
Keine.

### MED
Keine.

### LOW

**L-1: Singleton-Setter-Thread-Safety nicht dokumentiert.** RimWorld instanziiert `Mod`-Subklassen einmalig im Main-Thread via `LoadedModManager.CreateModClasses()` vor dem `UIRoot`-Start — de facto race-frei. Der Code dokumentiert diese Annahme nicht. Empfehlung: Kurzer XML-Doc-Kommentar an `Instance` („Set exactly once during Mod-Load on the main thread; safe to read from any thread thereafter."). Trivial, nicht release-blockierend.

**L-2: Reihenfolge `Instance = this;` vor `PatchAll` — defensiv korrekt, aber kommentierungswürdig.** Falls später `[HarmonyPatch]`-Klassen im Konstruktor der Patch-Klasse auf `RimWorldBotMod.Instance` zugreifen (sollten sie nicht, aber theoretisch möglich via `TargetMethod()`-Resolver), würde eine umgekehrte Reihenfolge NRE verursachen. Aktuelle Reihenfolge ist safe. Kommentar `// Instance zuerst, falls künftige Patches während PatchAll darauf zugreifen` wäre hilfreich für Story 2.x Reviewer.

**L-3: `Log.Message` läuft nach `Instance`-Setzung — keine Race.** Einzel-Thread-Konstruktor, keine Gegenprüfung nötig. Notiert zur Vollständigkeit.

**L-4: csproj — `Lib.Harmony 2.3.* ExcludeAssets="runtime" PrivateAssets="all"` — PASS.** `ExcludeAssets="runtime"` verhindert Bundling der Harmony-DLL in `Assemblies/` → Runtime-Harmony kommt aus `brrainz.harmony` via `modDependencies` (Story 1.1 About.xml). `PrivateAssets="all"` verhindert transitive Weitergabe. Inline-Kommentar im csproj erklärt das Setup korrekt. M-1 aus Story-1.1-Review damit aufgelöst.

**L-5: `using`-Statements minimal.** `System.Linq`, `System.Reflection`, `HarmonyLib`, `Verse` — exakt die vier benötigten, keine Redundanz, alphabetisch-ok (System.* zuerst). Keine Änderung nötig.

**L-6: Description in csproj noch auf „Skeleton — Story 1.1" gepinnt.** Z.13 `<Description>` erwähnt Story 1.1. Kosmetisch, für Story-8.x-Polish vormerken — kein 1.2-Blocker.

## Sekundär-Checks

- **Namespace-Konsistenz:** `RimWorldBot.Core` matcht Story-1.1-ModEntry-Namespace. Wechsel auf Root `RimWorldBot` nicht nötig (RimWorld-Reflektor scannt namespace-agnostisch).
- **Mod-Liste/Activation-Smoke:** `About/About.xml` packageId weiterhin `mediainvita.rimworldbot`, daher Harmony-ID-Match bleibt kohärent. Kein Re-Test nötig in dieser Story.

## Recommendation

**APPROVE.** Alle 8 ACs PASS, AI-1 + AI-4 erfüllt, csproj-Setup korrekt (ExcludeAssets runtime ist der richtige Weg bei brrainz.harmony-Dependency). Die vier LOW-Findings sind Dokumentationshinweise, keine Korrekturen — können optional in einem Mini-Patch vor `done` adressiert werden (XML-Doc-Kommentare an `Instance` und Reihenfolge-Kommentar). Guardian-Regel 4 (alle Findings werden gefixt) greift hier: **L-1 und L-2 vor `done` als XML-Doc-Comments ergänzen**; L-3 ist informativ (kein Fix nötig); L-4 ist bereits PASS; L-5 ist PASS; L-6 wird für Story 8.x vorgemerkt (Decision-Log-Eintrag oder TODO in csproj).

Nächster Schritt: Dev-Agent fügt L-1 + L-2 XML-Doc-Kommentare hinzu, dann Status `ready-for-visual-review` übersprungen (keine UI) → direkt `done` nach Re-Review-Check.
