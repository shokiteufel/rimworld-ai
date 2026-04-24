using Verse;

namespace RimWorldBot.Analysis
{
    // Placeholder — wird in Story 2.5/2.6 mit Top-3-Sites + Score-Breakdown + ClusterResults befüllt.
    // Story 1.3 nutzt nur IExposable-Contract damit BotMapComponent.analysisSummary Scribe-fähig ist.
    public class MapAnalysisSummary : IExposable
    {
        public int SchemaVersion = 1;

        public void ExposeData()
        {
            Scribe_Values.Look(ref SchemaVersion, "summarySchemaVersion", 1);
            // Felder kommen mit Story 2.5/2.6 dazu.
        }
    }
}
