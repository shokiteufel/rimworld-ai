using RimWorld;
using RimWorldBot.Core;
using RimWorldBot.Data;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorldBot.UI
{
    // Top-Bar-Toggle-Fenster. Registriert via Defs/MainButtonDefs.xml (kein Harmony-Patch, D-13).
    // Zeigt aktuellen masterState + drei State-Buttons (Off/Advisory/On), State-Wechsel
    // geht durch BotGameComponent.SetMasterState damit Log + DecisionLog + Persistenz konsistent laufen.
    public class MainTabWindow_BotControl : MainTabWindow
    {
        const float RowHeight = 32f;
        const float Padding = 10f;
        const float ButtonWidth = 90f;
        const float ButtonSpacing = 8f;
        // Zusätzlicher vertikaler Abstand zwischen Label-Baseline und Button-Row; visuell getrennte Sektionen.
        const float LabelToButtonsGap = 14f;
        // Aktiv-Indikator-Properties: Umrandung um Active-Button (VR L1: DrawBox statt halbtransparentem Fill).
        const float ActiveOutlineExpansion = 2f;
        const int ActiveOutlineThickness = 2;

        static readonly Color ActiveOutlineColor = new Color(1f, 0.85f, 0.2f);   // volles Gelb
        // Cache um Per-Frame-Allokation in DrawBox zu vermeiden (Round-2-Review Minor-Observation).
        static UnityEngine.Texture2D _activeOutlineTexture;
        static UnityEngine.Texture2D ActiveOutlineTexture =>
            _activeOutlineTexture ??= SolidColorMaterials.NewSolidColorTexture(ActiveOutlineColor);

        public override Vector2 RequestedTabSize => new Vector2(360f, 140f);

        public override void DoWindowContents(Rect inRect)
        {
            var game = Current.Game;
            var gameComp = game?.GetComponent<BotGameComponent>();
            if (gameComp == null)
            {
                // Defensiv: ohne laufendes Spiel (Main-Menu) wird das Fenster normalerweise nicht geöffnet,
                // aber Vanilla kann die Klasse instantiieren. Graceful fallback-Message.
                Widgets.Label(inRect, "RimWorldBot.MainTab.WaitingForGame".Translate());
                return;
            }

            var state = gameComp.masterState;

            // Zeile 1: State-Label mit lokalisiertem State-Name (DRY: shared LocalizationHelper).
            var stateLocalized = LocalizationHelper.TranslateEnum("RimWorldBot.ToggleState", state);
            var labelRect = new Rect(inRect.x, inRect.y + Padding, inRect.width, RowHeight);
            var prevAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(labelRect, "RimWorldBot.MainTab.StateLabel".Translate(stateLocalized));
            Text.Anchor = prevAnchor;

            // Zeile 2: drei Buttons (Off, Advisory, On), aktiver State hat Umrandung.
            var buttonsTotalWidth = ButtonWidth * 3 + ButtonSpacing * 2;
            var buttonsStartX = inRect.x + (inRect.width - buttonsTotalWidth) / 2f;
            var buttonsY = labelRect.yMax + LabelToButtonsGap;

            DrawStateButton(new Rect(buttonsStartX, buttonsY, ButtonWidth, RowHeight),
                ToggleState.Off, state, gameComp);
            DrawStateButton(new Rect(buttonsStartX + ButtonWidth + ButtonSpacing, buttonsY, ButtonWidth, RowHeight),
                ToggleState.Advisory, state, gameComp);
            DrawStateButton(new Rect(buttonsStartX + (ButtonWidth + ButtonSpacing) * 2, buttonsY, ButtonWidth, RowHeight),
                ToggleState.On, state, gameComp);
        }

        static void DrawStateButton(Rect rect, ToggleState target, ToggleState current, BotGameComponent gameComp)
        {
            var label = LocalizationHelper.TranslateEnum("RimWorldBot.ToggleState", target);
            var tooltipKey = $"RimWorldBot.MainTab.Tooltip.{target}";
            var tooltip = tooltipKey.CanTranslate() ? tooltipKey.Translate().ToString() : string.Empty;

            var isActive = current == target;
            if (isActive)
            {
                // Sichtbarer Aktiv-Indikator: gelbe Linien-Umrandung 2px um Button (VR L1-Fix — Outline statt Fill).
                var outline = rect.ExpandedBy(ActiveOutlineExpansion);
                Widgets.DrawBox(outline, ActiveOutlineThickness, ActiveOutlineTexture);
            }

            if (!string.IsNullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }

            if (Widgets.ButtonText(rect, label))
            {
                // Sound nur spielen wenn State-Wechsel tatsächlich passiert (CR-LOW: no-op click silent).
                if (!isActive)
                {
                    gameComp.SetMasterState(target);
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
            }
        }
    }
}
