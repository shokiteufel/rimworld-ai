namespace RimWorldBot.Decision
{
    // Story 1.11 (CC-STORIES-04, AI-7) — Layer-Präzedenz für PlanArbiter.
    // Höhere Layer überschreiben niedrigere bei Pawn/Cell/Workbench-Konflikten.
    // Sortier-Order: Emergency > Override > PhaseGoal > SkillGrind > Default.
    public enum PlanLayer
    {
        // Initial-Default damit Producer ohne expliziten Layer immer von höheren überstimmt werden.
        Default = 0,
        // Skill-Grinding (Story 4.10) — Trainings-Tasks für Skill-Aufbau, niedrige Priorität.
        SkillGrind = 1,
        // Standard-Phase-Goals (BuildPlanner, BillPlanner, WorkPlanner aus Stories 3.x).
        PhaseGoal = 2,
        // OverrideLibrary (Stories 8.3-8.4) — User-Learnings die PhaseGoals überstimmen.
        Override = 3,
        // EmergencyResolver (Story 3.1) — höchste Priorität, blockiert alles andere.
        Emergency = 4
    }
}
