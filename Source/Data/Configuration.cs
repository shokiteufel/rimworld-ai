using RimWorldBot.Core;
using Verse;

namespace RimWorldBot.Data
{
    // ModSettings-Subclass — User-Defaults für Bot-Verhalten (§5a Precedence:
    // BotGameComponent > LearnedConfig > Configuration > Defaults).
    // Persistiert via Vanilla-RimWorld-ModSettings-Save (Mods/RimWorldBot/...);
    // ChangeS triggern ConfigResolver.Invalidate() in WriteSettings (F-ARCH-08).
    public class Configuration : ModSettings
    {
        public EndingStrategy endingStrategy = EndingStrategy.Opportunistic;

        // Nur genutzt wenn endingStrategy == Forced. Default null (kein Forced-Ending gewählt).
        // Vanilla-Pattern: zwei non-nullable Felder + Property-Wrapper damit Scribe_Values
        // mit primitiven Typen arbeitet (Nullable-Enum-Scribe ist in RimWorld 1.6 unzuverlässig).
        Ending forcedEndingValue = Ending.Ship;   // Dummy-Default; nur relevant wenn forcedEndingHasValue
        bool forcedEndingHasValue;
        public Ending? ForcedEnding
        {
            get => forcedEndingHasValue ? forcedEndingValue : (Ending?)null;
            set
            {
                forcedEndingHasValue = value.HasValue;
                if (value.HasValue) forcedEndingValue = value.Value;
            }
        }

        // F-STAB-07: Telemetry default OFF (opt-in only).
        public bool telemetryEnabled;

        // PRD FR-03: Default für Per-Pawn-PlayerUse-Checkbox bei neuen Pawns.
        // Bot steuert default (false) — Player-Override per Story 1.6 ITab.
        public bool perPawnDefaultPlayerUse;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref endingStrategy, "endingStrategy", EndingStrategy.Opportunistic);
            Scribe_Values.Look(ref forcedEndingHasValue, "forcedEndingHasValue", false);
            Scribe_Values.Look(ref forcedEndingValue, "forcedEnding", Ending.Ship);
            Scribe_Values.Look(ref telemetryEnabled, "telemetryEnabled", false);
            Scribe_Values.Look(ref perPawnDefaultPlayerUse, "perPawnDefaultPlayerUse", false);
        }
    }
}
