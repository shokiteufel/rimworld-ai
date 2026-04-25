using System.Collections.Generic;

namespace RimWorldBot.Snapshot
{
    // Story 2.2 — Lookup-Service: Vanilla-defName-String → WildPlantKind.
    // Identifier-only-Pattern (D-23): keine ThingDef-Refs, nur strings — ermöglicht Whitelist-
    // Erweiterung ohne Def-Database-Lookup zur Laufzeit + erlaubt Snapshot-Tests ohne Verse.
    //
    // Vanilla-defNames verifiziert gegen RimWorld 1.6 (D-42 + CR R2-Korrektur 2026-04-25):
    // CORE (Data/Core/Defs/ThingDefs_Plants/):
    // - Plants_Wild_Temperate.xml: Plant_Berry
    // - Plants_Wild_General.xml: Plant_HealrootWild
    // - Plants_Wild_Arid.xml: Plant_Agave
    // - Plants_Cultivated_Farm.xml: Plant_Healroot, Plant_Psychoid, Plant_Smokeleaf
    // - Plants_Special.xml: Plant_Ambrosia
    // ODYSSEY-DLC (Data/Odyssey/Defs/ThingDefs_Plants/):
    // - Plants_Wild.xml: Plant_Berry_Leafless (Winter-Variante)
    // - Plants_WildCrops.xml: Plant_Psychoid_Wild, Plant_Smokeleaf_Wild
    //
    // KEIN expliziter ModsConfig.IsActive("Ludeon.RimWorld.Odyssey")-Guard notwendig:
    // String-Whitelist gegen ungespawnte Defs ist no-op (Cell hat den Plant nie → kein Match).
    // Risiko: defName-Match auf Mod-Plants die zufaellig gleich heissen — sehr selten, durch
    // packageId-Konvention der Mod-Devs vermieden.
    //
    // Folge-Stories die weitere DLC-spezifische Wild-Plants ergaenzen (z.B. Anomaly-essbares falls
    // je hinzukommt): WildPlantRegistry.Map erweitern + Source-Comment + Decision-Log-Update.
    public static class WildPlantRegistry
    {
        // Map defName → WildPlantKind. Lookup ist O(1) via Dictionary-Hash.
        // `internal` damit Tests via InternalsVisibleTo zugreifen koennen.
        internal static readonly IReadOnlyDictionary<string, WildPlantKind> Map = new Dictionary<string, WildPlantKind>
        {
            // Berries (essbar, raw food)
            ["Plant_Berry"] = WildPlantKind.Berries,
            ["Plant_Berry_Leafless"] = WildPlantKind.Berries,

            // Healroot (medizinisch)
            ["Plant_Healroot"] = WildPlantKind.Healroot,
            ["Plant_HealrootWild"] = WildPlantKind.Healroot,

            // Agave (essbar)
            ["Plant_Agave"] = WildPlantKind.Agave,

            // Ambrosia (essbar, mood-buff) — Core-Vanilla, kein DLC-Guard (D-42)
            ["Plant_Ambrosia"] = WildPlantKind.AmbrosiaBush,

            // Psychoid (Drug-Plant, Trade-Crop-Wert)
            ["Plant_Psychoid"] = WildPlantKind.PsychoidPlant,
            ["Plant_Psychoid_Wild"] = WildPlantKind.PsychoidPlant,

            // Smokeleaf (Drug-Plant, Trade-Crop-Wert)
            ["Plant_Smokeleaf"] = WildPlantKind.Smokeleaf,
            ["Plant_Smokeleaf_Wild"] = WildPlantKind.Smokeleaf,
        };

        // Public Lookup-API. Returnt null wenn defName nicht in Whitelist (= keine relevante
        // Wild-Plant, oder Mod-Plant — Story 2.5 entscheidet ob `Other` zugeordnet werden soll).
        public static WildPlantKind? TryClassify(string defName)
        {
            if (string.IsNullOrEmpty(defName)) return null;
            return Map.TryGetValue(defName, out var kind) ? kind : (WildPlantKind?)null;
        }

        // CR Story 2.2 LOW-1: stable Test-API anstatt direktem Map-Access. Falls Map intern auf
        // FrozenDictionary o.a. refactored wird, brechen Tests nicht.
        public static int VerifiedDefNameCount => Map.Count;
    }
}
