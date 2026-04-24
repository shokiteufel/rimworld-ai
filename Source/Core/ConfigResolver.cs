namespace RimWorldBot.Core
{
    // Placeholder — wird in Story 2.x mit Präzedenz-Pipeline (PhaseDefault → Learned → Override → User)
    // und transientem Cache befüllt. Story 1.3 nutzt nur Invalidate() null-safe in LoadedGame/StartedNewGame.
    public class ConfigResolver
    {
        public void Invalidate() { /* Placeholder; Cache-Clear kommt mit Story 2.x */ }
    }
}
