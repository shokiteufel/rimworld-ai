using Verse;

namespace RimWorldBot.Phases
{
    // Placeholder — wird in Story 3.7 (Phase-0-Goals) mit Progress-Func + LaunchCritical-Flag befüllt.
    // Story 1.3 braucht nur IExposable damit BotGameComponent.completedGoals Scribe-fähig ist.
    public class PhaseGoal : IExposable
    {
        public string Id;
        public int PhaseIndex;

        public PhaseGoal() { }

        public PhaseGoal(string id, int phaseIndex)
        {
            Id = id;
            PhaseIndex = phaseIndex;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Id, "id");
            Scribe_Values.Look(ref PhaseIndex, "phaseIndex", 0);
        }
    }
}
