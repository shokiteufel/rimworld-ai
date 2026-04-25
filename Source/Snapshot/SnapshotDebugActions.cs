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
        }
    }
}
