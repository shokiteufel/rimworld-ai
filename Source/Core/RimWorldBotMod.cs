using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorldBot.Data;
using RimWorldBot.UI;
using UnityEngine;
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

        Configuration _settings;
        public Configuration Settings => _settings ??= GetSettings<Configuration>();

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

        // ----- Story 1.7 ModSettings-Overrides -----

        public override string SettingsCategory() => "RimWorld Bot";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            SettingsRenderer.Draw(Settings, inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();

            // F-ARCH-08: Settings-Change → ConfigResolver-Cache invalidieren damit neue Werte sofort
            // wirken (ohne Game-Restart, außer Telemetry — das hat eigenen Restart-Hint).
            //
            // Story 1.3 ConfigResolver ist Placeholder mit no-op Invalidate(); echte Cache-Logik
            // kommt in Story 2.x. Wir rufen die Methode trotzdem JETZT auf damit:
            //   1. Wir die Hook-Stelle nicht vergessen wenn Story 2.x den ConfigResolver befüllt
            //   2. Story-2.x-Reviewer den Hook-Call sieht und nur die Methode implementiert
            //
            // gameComp ist null wenn User Settings im Main-Menu ändert (kein aktives Spiel) — Story 2.x
            // BuildController() wird beim nächsten StartedNewGame()/LoadedGame() neu mit aktuellen
            // Settings verdrahtet, daher ist null hier kein Problem.
            //
            // TODO(Story 2.x): public Accessor für ConfigResolver auf BotGameComponent + echter Cache-Reset.
            var gameComp = Current.Game?.GetComponent<BotGameComponent>();
            gameComp?.OnSettingsChanged();
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
