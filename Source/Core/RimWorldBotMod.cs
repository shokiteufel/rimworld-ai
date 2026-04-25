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

            // Keybinding-Collision-Check (Story 1.5 AC 6): Log alle Vanilla KeyBindingDefs die unser
            // defaultKeyCodeA (K) belegen. Nur Info-Log — User entscheidet via Options → Keyboard Config.
            // LongEventHandler.QueueLongEvent verzögert bis DefDatabase populated ist.
            LongEventHandler.QueueLongEvent(LogKeybindingCollisions, "RimWorldBot.InitKeybindingScan", false, null);

            Log.Message($"[RimWorldBot] initialized, Harmony patches: {harmony.GetPatchedMethods().Count()}, BotGameComponent registered");
        }

        static void LogKeybindingCollisions()
        {
            var ourDef = DefDatabase<KeyBindingDef>.GetNamedSilentFail("RimWorldBot_ToggleMaster");
            if (ourDef == null) return;
            var collisions = DefDatabase<KeyBindingDef>.AllDefsListForReading
                .Where(d => d != ourDef
                    && (d.defaultKeyCodeA == ourDef.defaultKeyCodeA || d.defaultKeyCodeB == ourDef.defaultKeyCodeA))
                .Select(d => d.defName)
                .ToList();
            if (collisions.Count > 0)
            {
                Log.Message($"[RimWorldBot] Keybinding info: RimWorldBot_ToggleMaster defaultKey={ourDef.defaultKeyCodeA} shared with [{string.Join(", ", collisions)}]. Ctrl-modifier gate in code; user may rebind via Options → Keyboard Configuration.");
            }
        }
    }
}
