using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RimWorldBot.Core
{
    // Einziger statischer Singleton im Projekt (AI-4). Alle weiteren Klassen werden
    // ab Story 1.3 via BotControllerFactory per Konstruktor-Injection verdrahtet.
    public class RimWorldBotMod : Mod
    {
        /// <summary>
        /// Set exactly once during Mod-Load on the main thread via <see cref="Verse.LoadedModManager.CreateModClasses"/>;
        /// safe to read from any thread thereafter. No locking needed.
        /// </summary>
        public static RimWorldBotMod Instance { get; private set; }

        public RimWorldBotMod(ModContentPack content) : base(content)
        {
            // Instance zuerst setzen, falls künftige [HarmonyPatch]-Klassen während PatchAll
            // (via TargetMethod-Resolver o.ä.) auf RimWorldBotMod.Instance zugreifen.
            Instance = this;
            var harmony = new Harmony("mediainvita.rimworldbot");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message($"[RimWorldBot] initialized, Harmony patches: {harmony.GetPatchedMethods().Count()}");
        }
    }
}
