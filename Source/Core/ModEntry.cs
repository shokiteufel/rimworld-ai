using RimWorld;
using Verse;

namespace RimWorldBot.Core
{
    // Skeleton Load-Probe aus Story 1.1 AC 5. Wird in Story 1.2 (Harmony-Bootstrap)
    // durch RimWorldBotMod : Mod ersetzt (AI-4 Singleton-Invariante gilt ab 1.2).
    [StaticConstructorOnStartup]
    public static class ModEntry
    {
        static ModEntry()
        {
            Log.Message("[RimWorldBot] skeleton loaded");
        }
    }
}
