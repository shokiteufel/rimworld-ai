using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RimWorld;
using RimWorldBot.Core;
using RimWorldBot.Data;
using UnityEngine;
using Verse;

namespace RimWorldBot.UI
{
    // Trennt Widget-Logik von der RimWorldBotMod-Klasse damit DoSettingsWindowContents
    // nicht zur Mega-Methode wird (Story 1.7 Task: modular). 5 Sections:
    // General / Ending / Learning / Telemetry / Info.
    public static class SettingsRenderer
    {
        // Modul-Version — wird in Settings-Info-Sektion angezeigt.
        // Statt hardcoded: aus Assembly-Version gelesen damit Bumps automatisch durchschlagen.
        static string ModVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

        // GitHub-Repo-URL aus About.xml — für jetzt hardcoded; Epic 8 könnte aus ModContentPack
        // ziehen falls dort verfügbar.
        const string GitHubRepoUrl = "https://github.com/shokiteufel/rimworld-ai";

        public static void Draw(Configuration config, Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            DrawSectionGeneral(listing, config);
            listing.GapLine();

            DrawSectionEnding(listing, config);
            listing.GapLine();

            DrawSectionLearning(listing);
            listing.GapLine();

            DrawSectionTelemetry(listing, config);
            listing.GapLine();

            DrawSectionInfo(listing);

            listing.End();
        }

        static void DrawSectionGeneral(Listing_Standard listing, Configuration config)
        {
            DrawSectionHeader(listing, "RimWorldBot.Settings.General.Header".Translate());
            listing.CheckboxLabeled(
                "RimWorldBot.Settings.General.PerPawnDefault.Label".Translate(),
                ref config.perPawnDefaultPlayerUse,
                "RimWorldBot.Settings.General.PerPawnDefault.Tooltip".Translate());
        }

        static void DrawSectionEnding(Listing_Standard listing, Configuration config)
        {
            DrawSectionHeader(listing, "RimWorldBot.Settings.Ending.Header".Translate());

            // EndingStrategy-Dropdown mit lokalisierten Labels (VR HIGH-1: nicht raw enum.ToString()).
            var strategyLabel = "RimWorldBot.Settings.Ending.Strategy.Label".Translate();
            var strategyButtonText = TranslateEnum("RimWorldBot.EndingStrategy", config.endingStrategy);
            var strategyRect = listing.GetRect(28f);
            var labelRect = strategyRect.LeftPart(0.45f);
            var buttonRect = strategyRect.RightPart(0.5f);
            Widgets.Label(labelRect, strategyLabel);
            if (Widgets.ButtonText(buttonRect, strategyButtonText))
            {
                var options = new List<FloatMenuOption>
                {
                    new FloatMenuOption(TranslateEnum("RimWorldBot.EndingStrategy", EndingStrategy.Opportunistic),
                        () => config.endingStrategy = EndingStrategy.Opportunistic),
                    new FloatMenuOption(TranslateEnum("RimWorldBot.EndingStrategy", EndingStrategy.Forced),
                        () => config.endingStrategy = EndingStrategy.Forced)
                };
                Find.WindowStack.Add(new FloatMenu(options));
            }

            // Forced-Ending-Dropdown nur wenn Strategy = Forced
            if (config.endingStrategy == EndingStrategy.Forced)
            {
                var endingLabel = "RimWorldBot.Settings.Ending.Forced.Label".Translate();
                var endingButtonText = config.ForcedEnding.HasValue
                    ? TranslateEnum("RimWorldBot.Ending", config.ForcedEnding.Value)
                    : "RimWorldBot.Settings.Ending.Forced.NotSelected".Translate().ToString();
                var endingRect = listing.GetRect(28f);
                var endingLabelRect = endingRect.LeftPart(0.45f);
                var endingButtonRect = endingRect.RightPart(0.5f);
                Widgets.Label(endingLabelRect, endingLabel);
                if (Widgets.ButtonText(endingButtonRect, endingButtonText))
                {
                    var options = new List<FloatMenuOption>();
                    foreach (Ending e in Enum.GetValues(typeof(Ending)))
                    {
                        var captured = e;
                        options.Add(new FloatMenuOption(TranslateEnum("RimWorldBot.Ending", captured),
                            () => config.ForcedEnding = captured));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
        }

        // Generischer Enum-Translator: sucht Key "{prefix}.{enumValue}" in Languages-Files.
        // Fallback bei Missing-Key: raw enum-Name (Story 1.8 wird die Coverage abschließen).
        static string TranslateEnum<TEnum>(string keyPrefix, TEnum value) where TEnum : struct, Enum
        {
            var key = $"{keyPrefix}.{value}";
            return key.CanTranslate() ? key.Translate().ToString() : value.ToString();
        }

        static void DrawSectionLearning(Listing_Standard listing)
        {
            DrawSectionHeader(listing, "RimWorldBot.Settings.Learning.Header".Translate());
            if (listing.ButtonText("RimWorldBot.Settings.Learning.ResetButton".Translate()))
            {
                Find.WindowStack.Add(new Dialog_MessageBox(
                    text: "RimWorldBot.Settings.Learning.ResetConfirm".Translate(),
                    buttonAText: "Confirm".Translate(),
                    buttonAAction: ResetLearnedConfig,
                    buttonBText: "Cancel".Translate(),
                    title: "RimWorldBot.Settings.Learning.ResetButton".Translate()));
            }
        }

        static void DrawSectionTelemetry(Listing_Standard listing, Configuration config)
        {
            DrawSectionHeader(listing, "RimWorldBot.Settings.Telemetry.Header".Translate());
            var prevValue = config.telemetryEnabled;
            listing.CheckboxLabeled(
                "RimWorldBot.Settings.Telemetry.Enable.Label".Translate(),
                ref config.telemetryEnabled,
                "RimWorldBot.Settings.Telemetry.Enable.Tooltip".Translate());
            if (config.telemetryEnabled != prevValue)
            {
                Messages.Message(
                    "RimWorldBot.Settings.Telemetry.RestartHint".Translate(),
                    MessageTypeDefOf.CautionInput, historical: false);
            }
        }

        static void DrawSectionInfo(Listing_Standard listing)
        {
            DrawSectionHeader(listing, "RimWorldBot.Settings.Info.Header".Translate());

            // Version-Anzeige: bei "0.0.0"-Fallback (csproj <Version> nicht gesetzt) klar markieren
            // damit User nicht denkt das wäre eine echte Release-Version. csproj-Bump-TODO siehe MED-2.
            var versionString = ModVersion == "0.0.0"
                ? "RimWorldBot.Settings.Info.VersionUnknown".Translate().ToString()
                : "RimWorldBot.Settings.Info.Version".Translate(ModVersion).ToString();
            listing.Label(versionString);

            // GitHub-Link: Label-Zeile + URL-Anzeige + separater "Open"-Button.
            // Vermeidet missverständlichen "Klick-auf-URL-Text"-Pattern.
            listing.Label("RimWorldBot.Settings.Info.GitHubLabel".Translate());
            listing.Label(GitHubRepoUrl);
            if (listing.ButtonText("RimWorldBot.Settings.Info.GitHubButton".Translate()))
            {
                Application.OpenURL(GitHubRepoUrl);
            }
        }

        static void DrawSectionHeader(Listing_Standard listing, string text)
        {
            var prevFont = Text.Font;
            Text.Font = GameFont.Medium;
            listing.Label(text);
            Text.Font = prevFont;
        }

        // Placeholder — volle LearnedConfig-Implementation kommt in Epic 8 Story 8.5.
        // Hier nur File-Delete + Toast damit Story 1.7 AC 7 erfüllt ist.
        static void ResetLearnedConfig()
        {
            try
            {
                var path = Path.Combine(GenFilePaths.ConfigFolderPath, "RimWorldBot", "learned-config.xml");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                Messages.Message(
                    "RimWorldBot.Settings.Learning.ResetSuccess".Translate(),
                    MessageTypeDefOf.PositiveEvent, historical: false);
            }
            catch (Exception ex)
            {
                Log.Error($"[RimWorldBot] LearnedConfig reset failed: {ex}");
                Messages.Message(
                    "RimWorldBot.Settings.Learning.ResetFailed".Translate(),
                    MessageTypeDefOf.RejectInput, historical: false);
            }
        }
    }
}
