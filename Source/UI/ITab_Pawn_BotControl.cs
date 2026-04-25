using RimWorld;
using RimWorldBot.Data;
using UnityEngine;
using Verse;

namespace RimWorldBot.UI
{
    // Eigener Pawn-ITab (D-13 H8 entfernt — kein Patch auf ITab_Pawn_Character).
    // Registriert via Patches/HumanInspectTabs.xml (PatchOperationAdd auf Human.inspectorTabs nach Inheritance-Flattening von BasePawn).
    // Persistiert pro Pawn via BotGameComponent.perPawnPlayerUse keyed by UniqueLoadID (D-14).
    public class ITab_Pawn_BotControl : ITab
    {
        const float ContentPadding = 12f;
        const float CheckboxRowHeight = 28f;
        const float CheckboxToHelpGap = 8f;

        static readonly Vector2 WinSize = new Vector2(360f, 180f);
        // Vanilla-Inspector-Hilfstext-Farbe (Health/Needs/etc. nutzen ~0.6 grau).
        static readonly Color HelpTextColor = new Color(0.6f, 0.6f, 0.6f);

        public ITab_Pawn_BotControl()
        {
            size = WinSize;
            labelKey = "RimWorldBot.PerPawn.TabLabel";
        }

        // Pawn-Tab sichtbar für Player-Faction-Humanlike-Pawns:
        // - Colonists: Faction.IsPlayer = true (sind Free-Player-Pawns)
        // - Slaves (Ideology-DLC): Faction.IsPlayer = true mit IsSlave-Flag — Tab ebenfalls sichtbar (AC 11)
        // - Quest-Lodgers, Wild-Pawns, Visitors: Faction != Player → Tab versteckt
        // - Animals: !Humanlike → Tab versteckt (Bot fokussiert in Phase 0+ auf Humanlike)
        public override bool IsVisible
        {
            get
            {
                var pawn = SelPawn;
                if (pawn == null) return false;
                if (pawn.Faction == null || !pawn.Faction.IsPlayer) return false;
                if (!pawn.RaceProps.Humanlike) return false;
                return true;
            }
        }

        protected override void FillTab()
        {
            var pawn = SelPawn;
            if (pawn == null) return;
            var gameComp = Current.Game?.GetComponent<BotGameComponent>();
            if (gameComp == null)
            {
                // Game-Component noch nicht geladen (Mod-List-Vorschau o.ä.) — graceful skip.
                Widgets.Label(new Rect(ContentPadding, ContentPadding, size.x - ContentPadding * 2f, CheckboxRowHeight),
                    "RimWorldBot.PerPawn.NotLoaded".Translate());
                return;
            }

            var rect = new Rect(ContentPadding, ContentPadding,
                size.x - ContentPadding * 2f, size.y - ContentPadding * 2f);

            var pawnId = pawn.GetUniqueLoadID();
            // Default: Bot steuert (false). Story 1.6 AC 7.
            var currentValue = gameComp.perPawnPlayerUse.TryGetValue(pawnId, out var v) && v;
            var newValue = currentValue;

            // Zeile 1: Checkbox.
            var checkboxRect = new Rect(rect.x, rect.y, rect.width, CheckboxRowHeight);
            Widgets.CheckboxLabeled(checkboxRect, "RimWorldBot.PerPawn.PlayerUseLabel".Translate(), ref newValue);

            if (newValue != currentValue)
            {
                if (newValue)
                {
                    gameComp.perPawnPlayerUse[pawnId] = true;
                }
                else
                {
                    // false ist Default — Eintrag entfernen statt false zu speichern, damit Dict klein bleibt.
                    gameComp.perPawnPlayerUse.Remove(pawnId);
                }
                // DevMode-Guard: vermeidet Log-Spam bei normalen Spielern, hilft bei Mod-Debugging.
                if (Prefs.DevMode)
                {
                    Log.Message($"[RimWorldBot] per-pawn PlayerUse for {pawn.LabelShortCap} ({pawnId}) → {newValue}");
                }
            }

            // Zeile 2: Hilfstext mit dynamischer Höhe via Text.CalcHeight (übersetzungs-resilient).
            var helpText = "RimWorldBot.PerPawn.HelpText".Translate();
            var helpY = checkboxRect.yMax + CheckboxToHelpGap;
            var availableHeight = rect.height - (helpY - rect.y);
            var measuredHeight = Text.CalcHeight(helpText, rect.width);
            var helpHeight = Mathf.Min(measuredHeight, availableHeight);
            var helpRect = new Rect(rect.x, helpY, rect.width, helpHeight);

            var prevColor = GUI.color;
            GUI.color = HelpTextColor;
            Widgets.Label(helpRect, helpText);
            GUI.color = prevColor;
        }
    }
}
