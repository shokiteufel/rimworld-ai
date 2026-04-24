namespace RimWorldBot.Core
{
    public enum ToggleState
    {
        Off = 0,
        Advisory = 1,
        On = 2
    }

    public enum EndingStrategy
    {
        Opportunistic = 0,
        Forced = 1
    }

    public enum Ending
    {
        Ship = 0,
        Journey = 1,
        Royal = 2,
        Archonexus = 3,
        Void = 4
    }

    public enum EndingCommitment
    {
        None = 0,
        Locked = 1,
        Released = 2
    }
}
