namespace RimWorldBot.Data
{
    // D-25 Goal-Phase-Tag-Schema (§2.3b). Identifier-only (D-23).
    public record PhaseGoalTag(int PhaseIndex, string GoalId);
}
