using System;
using System.Text;
using LudeonTK;
using Verse;

namespace RimWorldBot.Snapshot
{
    // Story 2.1 — DebugAction zum manuellen Triggern von GetCells fuer MT-5 (AC-8 Integration-Test).
    // Erscheint nur in Dev-Mode (Vanilla-DebugAction-Pattern), kategorisiert unter "RimWorldBot"
    // im Debug-Aktionen-Menue.
    //
    // Production-Impact: keine — DebugAction-Attribute sind reine Dev-Mode-Eintraege, kein
    // GameComponent-Tick + kein Memory-Footprint im Normal-Spiel.
    public static class SnapshotDebugActions
    {
        [DebugAction(
            category: "RimWorldBot",
            name: "Trigger Snapshot Scan (Story 2.1)",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        static void TriggerSnapshotScan()
        {
            var map = Find.CurrentMap;
            if (map == null)
            {
                Log.Warning("[RimWorldBot] DebugAction Snapshot: Find.CurrentMap is null.");
                return;
            }
            var provider = new RimWorldSnapshotProvider();
            var cells = provider.GetCells(map);
            // AC-8 Verifikation: Array-Laenge == map.Area.
            // Performance-Probe wird durch GetCells selbst geloggt (ab >200ms).
            // DebugAction-Log immer sichtbar, damit User MT-5 den Scan direkt verifizieren kann.
            Log.Message($"[RimWorldBot] DebugAction Snapshot: {cells.Length} cells via GetCells (expected {map.Area}, match={cells.Length == map.Area}).");

            // Story 2.2 MT-6: Wild-Plant-Aggregation pro Kind. Nur grouped-Output, kein Per-Cell-Spam.
            // CR Story 2.2 MED-2: int[]-Aggregation statt LINQ-GroupBy → keine Iterator-State-Machines,
            // keine Bucket-Allokationen, keine intermediate Lists. O(n) single-pass über cells.
            // Pattern für künftige DebugActions die auf 60k+ Cells aggregieren.
            int kindCount = Enum.GetValues(typeof(WildPlantKind)).Length;
            var counts = new int[kindCount];
            int totalWithPlant = 0;
            foreach (var c in cells)
            {
                if (c.WildPlant.HasValue)
                {
                    counts[(int)c.WildPlant.Value]++;
                    totalWithPlant++;
                }
            }

            if (totalWithPlant > 0)
            {
                var sb = new StringBuilder();
                bool first = true;
                for (int i = 0; i < kindCount; i++)
                {
                    if (counts[i] == 0) continue;
                    if (!first) sb.Append(", ");
                    sb.Append(((WildPlantKind)i).ToString()).Append('=').Append(counts[i]);
                    first = false;
                }
                Log.Message($"[RimWorldBot] DebugAction WildPlants: {totalWithPlant} cells with WildPlant — {sb}.");
            }
            else
            {
                Log.Message($"[RimWorldBot] DebugAction WildPlants: 0 cells with WildPlant on this map.");
            }
        }
    }
}
