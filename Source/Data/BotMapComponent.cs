using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorldBot.Analysis;
using Verse;
using Verse.AI;

namespace RimWorldBot.Data
{
    // Per-Map-Persistenz + D-25 Goal-Phase-Tag-Dicts (§2.3b, F-STAB-21).
    public class BotMapComponent : MapComponent
    {
        // schemaVersion pro-Map: v1 hatte nur analysisSummary; v2 kamen Goal-Tag-Dicts dazu;
        // v3 (Story 1.3 AC 16) fügt excludedCells hinzu. Migrate füllt fehlende Felder mit Defaults.
        // CR Story 1.13 HIGH-2-Fix: internal statt private (siehe BotGameComponent für Details).
        internal const int CurrentSchemaVersion = 3;
        int schemaVersion = CurrentSchemaVersion;

        public MapAnalysisSummary analysisSummary;   // schlank: Top-3-Sites, KEIN Per-Cell (§5)
        public int phaseProgressTicks;
        public int stableCounter;                    // F-AI-08 konsekutive 60er-Ticks ohne Violation

        // D-25: bot-platzierte Things + bot-assigned Jobs mit Phase-Goal-Tag für Orphan-Cleanup.
        public Dictionary<int, PhaseGoalTag> botPlacedThings = new();
        public Dictionary<int, PhaseGoalTag> botAssignedJobs = new();

        // Story 2.3 Hazard-Scanner: Cells mit HazardKind != None + hazardProximity < 3 (D-23 identifier-only via (int,int)).
        // Persistenz-Format: flatten zu List<int> _excludedCellsFlat (x0,z0,x1,z1,...) weil Scribe_Values kein Tuple unterstützt.
        public HashSet<(int x, int z)> excludedCells = new();
        List<int> _excludedCellsFlat;   // nur zur Scribe-Zeit benutzt

        public BotMapComponent(Map map) : base(map) { }

        public override void ExposeData()
        {
            try
            {
                Scribe_Values.Look(ref schemaVersion, "schemaVersion", 1);
                Scribe_Deep.Look(ref analysisSummary, "analysisSummary");
                Scribe_Values.Look(ref phaseProgressTicks, "phaseProgressTicks", 0);
                Scribe_Values.Look(ref stableCounter, "stableCounter", 0);
                Scribe_Collections.Look(ref botPlacedThings, "botPlacedThings", LookMode.Value, LookMode.Deep);
                Scribe_Collections.Look(ref botAssignedJobs, "botAssignedJobs", LookMode.Value, LookMode.Deep);

                // excludedCells: flatten vor Save, rehydrate nach Load.
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    _excludedCellsFlat = new List<int>(excludedCells.Count * 2);
                    foreach (var (x, z) in excludedCells)
                    {
                        _excludedCellsFlat.Add(x);
                        _excludedCellsFlat.Add(z);
                    }
                }
                Scribe_Collections.Look(ref _excludedCellsFlat, "excludedCells", LookMode.Value);

                if (Scribe.mode == LoadSaveMode.PostLoadInit)
                {
                    botPlacedThings ??= new Dictionary<int, PhaseGoalTag>();
                    botAssignedJobs ??= new Dictionary<int, PhaseGoalTag>();

                    // Rehydrate excludedCells aus flat-List (leer wenn null oder v<3).
                    excludedCells = new HashSet<(int, int)>();
                    if (_excludedCellsFlat != null)
                    {
                        for (int i = 0; i + 1 < _excludedCellsFlat.Count; i += 2)
                        {
                            excludedCells.Add((_excludedCellsFlat[i], _excludedCellsFlat[i + 1]));
                        }
                    }
                    _excludedCellsFlat = null;   // nicht persistent im Memory halten

                    if (schemaVersion < CurrentSchemaVersion)
                    {
                        Migrate();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[RimWorldBot] BotMapComponent.ExposeData failure: {ex}");
                analysisSummary = null;         // triggers Re-Scan on next tick
                stableCounter = 0;
                botPlacedThings ??= new Dictionary<int, PhaseGoalTag>();
                botAssignedJobs ??= new Dictionary<int, PhaseGoalTag>();
                excludedCells ??= new HashSet<(int, int)>();
            }
        }

        // v1→v2: botPlacedThings/botAssignedJobs wurden hinzugefügt (bereits durch ??= abgedeckt).
        // v2→v3: excludedCells leer initialisieren; Re-Analyse durch nächsten Map.FinalizeInit populiert.
        void Migrate()
        {
            int from = schemaVersion;
            if (schemaVersion < 3)
            {
                excludedCells ??= new HashSet<(int, int)>();
            }
            schemaVersion = CurrentSchemaVersion;
            Log.Message($"[RimWorldBot] Migrated BotMapComponent (map {map?.uniqueID}) from schema v{from} to v{CurrentSchemaVersion}");
        }

        public void CancelOrphanedDesignations(int currentPhaseIndex)
        {
            var orphans = botPlacedThings
                .Where(kv => kv.Value != null && kv.Value.PhaseIndex > currentPhaseIndex)
                .Select(kv => kv.Key)
                .ToList();
            foreach (var thingId in orphans)
            {
                var thing = map.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == thingId);
                if (thing != null && !thing.Destroyed)
                {
                    if (thing is Blueprint bp)
                    {
                        bp.Destroy(DestroyMode.Cancel);
                    }
                    else
                    {
                        map.designationManager.RemoveAllDesignationsOn(thing);
                    }
                }
                botPlacedThings.Remove(thingId);
            }
        }

        public void CancelOrphanedJobs(int currentPhaseIndex)
        {
            var orphans = botAssignedJobs
                .Where(kv => kv.Value != null && kv.Value.PhaseIndex > currentPhaseIndex)
                .Select(kv => kv.Key)
                .ToList();
            foreach (var jobId in orphans)
            {
                foreach (var pawn in map.mapPawns.AllPawns)
                {
                    if (pawn?.jobs?.curJob?.loadID == jobId)
                    {
                        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                }
                botAssignedJobs.Remove(jobId);
            }
        }
    }
}
