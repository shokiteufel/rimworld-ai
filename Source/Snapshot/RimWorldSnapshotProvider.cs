using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;

namespace RimWorldBot.Snapshot
{
    // Story 2.1 (Epic 2 Foundation) — einziger Mapper RimWorld-Runtime → POCO-Snapshots.
    // Architecture §2.2: alle anderen Bot-Klassen konsumieren ISnapshotProvider, nur diese
    // hier hat echte Map/Pawn/TerrainDef-Refs.
    //
    // GetCells-Performance: AC-6 fordert <500ms auf 250×250 (= 62500 Cells). Stopwatch-Wrap
    // loggt Millisekunden bei langsamen Scans. Story 2.9 splittet das in Coroutine wenn nötig.
    //
    // GetColony/GetPawns: Story 2.1 Stubs — Felder werden in Folge-Stories gefüllt
    // (Story 3.5 FoodDays, Story 4.9a MoodAvg, Story 6.5 Skills, etc.). Hier nur Framework.
    //
    // CR Story 2.1 Refactor (2026-04-25):
    // - CRIT-1: kein BotSafe.SafeTick pro Cell (war silent-swallow → null-Slots im Array bei Exception).
    //   Stattdessen single try/catch um inner mapping mit safe-default fallback.
    // - CRIT-2: keine Closure-Allocation pro Cell (62500× DisplayClass auf 250×250 = GC-Druck).
    //   Inline-Mapping ohne Lambda.
    // - LOW-2: terrainGrid + roofGrid + thingGrid einmalig vor der Schleife cachen.
    public sealed class RimWorldSnapshotProvider : ISnapshotProvider
    {
        // Default-Snapshot bei Cell-Mapping-Exception (CRIT-1 Fix). Stellt sicher dass Consumer
        // niemals null-CellSnapshot konsumieren. Position bleibt korrekt fuer Index-Lookup.
        // Story 2.2: WildPlant default null (= keine wilde Pflanze).
        static CellSnapshot SafeDefault(IntVec3 cell) => new(
            Position: (cell.x, cell.z),
            TerrainDefName: "Unknown",
            Fertility: 0f,
            HasWater: false,
            HazardKind: HazardKind.None,
            HasRoof: false,
            IsMountain: false,
            HasResources: false,
            ChokepointScore: 0f,
            WildPlant: null);

        public CellSnapshot[] GetCells(Map map)
        {
            if (map == null) return Array.Empty<CellSnapshot>();

            var sw = Stopwatch.StartNew();
            // CR LOW-2: Grids einmalig cachen statt pro Cell zu lesen.
            var terrainGrid = map.terrainGrid;
            var roofGrid = map.roofGrid;
            var thingGrid = map.thingGrid;

            var result = new CellSnapshot[map.Area];
            int idx = 0;
            int errorCount = 0;

            // CR Story 2.9 TODO: idx via row-major-Indexing aus (cell.x + cell.z * map.Size.x)
            // ableiten wenn parallelisiert. Heute single-thread → Counter ok.
            // AllCells-Iteration-Order: Vanilla-Implementierungsdetail, NICHT von Bot-Logik
            // angenommen. Wenn Folge-Stories Order-abhaengig werden, muss Annahme dort gepinnt werden.
            foreach (var cell in map.AllCells)
            {
                // CR CRIT-1 Fix: safe-default first, overwrite on success. Niemals null-Slots.
                // CR CRIT-2 Fix: kein Lambda → kein Closure → keine per-Iter-Allokation.
                CellSnapshot snap = SafeDefault(cell);
                try
                {
                    var terrain = terrainGrid?.TerrainAt(cell);
                    // CR Story 2.2 HIGH-1 Fix: single-pass thingGrid-Iteration statt
                    // HasMineableResource + ClassifyWildPlant separat aufzurufen (war 2× ThingsListAtFast-
                    // Iteration pro Cell = 125k Loops auf 250×250-Map). Jetzt 1 Loop pro Cell.
                    ClassifyCellThings(thingGrid, cell, out bool hasResources, out WildPlantKind? wildPlant);
                    snap = new CellSnapshot(
                        Position: (cell.x, cell.z),
                        TerrainDefName: terrain?.defName ?? "Unknown",
                        Fertility: terrain?.fertility ?? 0f,
                        HasWater: terrain?.IsWater ?? false,
                        HazardKind: ClassifyHazard(terrain),
                        HasRoof: roofGrid?.Roofed(cell) ?? false,
                        IsMountain: (roofGrid?.RoofAt(cell)?.isThickRoof) ?? false,
                        HasResources: hasResources,
                        ChokepointScore: 0f,    // Story 2.4 füllt das später
                        WildPlant: wildPlant);
                }
                catch (Exception ex)
                {
                    // SafeDefault bleibt im Array. Errors aggregiert loggen statt 62500× Spam.
                    errorCount++;
                    if (errorCount == 1)
                    {
                        Log.Warning($"[RimWorldBot] GetCells: cell-mapping exception at {cell} (map {map.uniqueID}): {ex.GetType().Name}: {ex.Message}. Further cell-errors aggregated to total count.");
                    }
                }
                result[idx++] = snap;
            }

            if (errorCount > 1)
            {
                Log.Warning($"[RimWorldBot] GetCells: total {errorCount} cell-mapping exceptions (map {map.uniqueID}); affected cells use SafeDefault.");
            }

            sw.Stop();
            // Performance-Probe (AC-6): nur bei langsamen Scans loggen, sonst Spam.
            if (sw.ElapsedMilliseconds > 200)
            {
                Log.Message($"[RimWorldBot] GetCells scan: {sw.ElapsedMilliseconds}ms for {result.Length} cells (map {map.uniqueID}, area {map.Area}).");
            }

            return result;
        }

        public ColonySnapshot GetColony()
        {
            // Story 2.1 Stub — Folge-Stories füllen die Felder mit echten Werten.
            // Aktuell return Empty damit Consumer nicht null-checken müssen.
            return ColonySnapshot.Empty;
        }

        public PawnSnapshot[] GetPawns()
        {
            // Story 2.1 Stub — minimaler Wrapper über alle Free-Colonist-Pawns aller Maps.
            // Skills/Passions/Health kommen in Story 4.10/6.5/3.4.
            // CR HIGH-3 Fix: Dedup via HashSet auf LoadID weil ein Pawn (Caravan-Split, Pocket-Map
            // Story 7.x, Anomaly-Kammer) in mehreren mapPawns.FreeColonistsSpawned auftauchen kann.
            var maps = Find.Maps;
            if (maps == null || maps.Count == 0) return Array.Empty<PawnSnapshot>();

            var seen = new HashSet<string>();
            var collected = new List<PawnSnapshot>();
            foreach (var map in maps)
            {
                if (map?.mapPawns?.FreeColonistsSpawned == null) continue;
                foreach (var pawn in map.mapPawns.FreeColonistsSpawned)
                {
                    if (pawn == null) continue;
                    var loadId = pawn.GetUniqueLoadID();
                    if (string.IsNullOrEmpty(loadId)) continue;
                    if (!seen.Add(loadId)) continue;   // schon gesehen → skip Duplikat
                    collected.Add(PawnSnapshot.Empty(loadId));
                }
            }
            return collected.ToArray();
        }

        // ----- Helpers -----

        // CR MED-1 Klarstellung: Pollution/Toxic/Radiation-Erweiterung passiert genau HIER in dieser
        // Methode (Story 2.3 erweitert die Mapping-Logik), NICHT in einem separaten Scanner-Layer.
        // Snapshots sind immutable — der Hazard-Wert muss zur Erzeugungs-Zeit korrekt sein.
        static HazardKind ClassifyHazard(TerrainDef terrain)
        {
            if (terrain == null) return HazardKind.None;
            // Vanilla Lava-Terrain — defName-Match weil HazardKind enum-stable, defName string-stable
            if (terrain.defName == "Lava" || terrain.defName == "LavaShallow") return HazardKind.Lava;
            // Story 2.3 ergaenzt hier: Pollution-Cell-Lookup (Biotech), ToxicFalloutGameCondition,
            // Anomaly-Radiation. Jeweils via map-state-Lookup ueber zusaetzliche Provider-Parameter.
            return HazardKind.None;
        }

        // CR Story 2.2 HIGH-1: single-pass Refactor — eine ThingsListAtFast-Iteration pro Cell
        // mit zwei out-Outputs statt zwei separaten Helper-Methoden. Spart 50% Lookups auf 250×250
        // (62500 statt 125000 Iterationen). Allokationsfrei (List<T>.GetEnumerator ist struct,
        // out-bool/out-nullable sind Stack-Werte).
        //
        // CR Story 2.2 MED-3 Doku: Multi-Plant-Cell (Cell mit mehreren Wild-Plants übereinander)
        // ist in Vanilla nicht möglich (ThingDef.passability + plant.choosesNeighbor verhindert das),
        // aber Mods können es brechen. Aktuell: erstes Wild-Plant-Match in ThingsListAtFast gewinnt.
        // Deterministische Priorität (z.B. Healroot > Berries > Drugs) ist Story-2.5-Scoring-Concern.
        static void ClassifyCellThings(
            ThingGrid thingGrid, IntVec3 cell,
            out bool hasMineable, out WildPlantKind? wildPlant)
        {
            hasMineable = false;
            wildPlant = null;
            if (thingGrid == null) return;
            var things = thingGrid.ThingsListAtFast(cell);
            if (things == null) return;

            foreach (var thing in things)
            {
                var def = thing?.def;
                if (def == null) continue;

                if (!hasMineable && def.mineable)
                {
                    hasMineable = true;
                }
                if (!wildPlant.HasValue)
                {
                    var kind = WildPlantRegistry.TryClassify(def.defName);
                    if (kind.HasValue) wildPlant = kind;
                }
                // Early-exit wenn beide Outputs gesetzt sind (Allokations- + Branch-frei).
                if (hasMineable && wildPlant.HasValue) return;
            }
        }
    }
}
