using System.Collections.Immutable;

namespace RimWorldBot.Snapshot
{
    // Story 2.1 — minimaler PawnSnapshot-Stub als Forward-Declaration für ISnapshotProvider.
    // Vollständige Implementation der Felder erfolgt in Folge-Stories (Architecture §2.2):
    //   - Skills: Story 4.10 (Skill-Grinding) + 6.5 (Specialist-Roles)
    //   - Passions: zusammen mit Skills
    //   - Health: Story 3.4 (Emergency-Bleed) + 4.9b (Emergency-Health)
    //   - Needs: Story 4.9a (Mood) + 4.9g (PawnSleep)
    //
    // Identifier-only-Pattern (D-23 analog):
    //   - PawnLoadId statt Pawn-Ref
    //   - Skills als ImmutableDictionary<string SkillDefName, int Level>
    //   - Passions als ImmutableDictionary<string SkillDefName, int Passion-0..2>
    //   - HealthSummary als enum statt HediffSet
    //
    // Persistence (D-14): NICHT persistiert.
    public sealed record PawnSnapshot(
        string PawnLoadId,
        ImmutableDictionary<string, int> Skills,
        ImmutableDictionary<string, int> Passions,
        HealthSummary Health,
        double MoodCurrent)
    {
        public static PawnSnapshot Empty(string loadId) => new(
            PawnLoadId: loadId,
            Skills: ImmutableDictionary<string, int>.Empty,
            Passions: ImmutableDictionary<string, int>.Empty,
            Health: HealthSummary.Healthy,
            MoodCurrent: 0.5);
    }

    // Vereinfachte Health-Klassifikation (Story 2.1 Stub). Detail-Auflösung in Story 3.4/4.9b.
    // CR Story 2.1 MED-5: Werte sind NICHT verbindlich — Folge-Stories (3.4 Bleed, 4.9b Health)
    // dürfen das Enum erweitern (z.B. um InfectionRisk, ChronicCondition) ohne Decision-Log,
    // solange existierende Werte stabil bleiben (numerisch + semantisch).
    public enum HealthSummary
    {
        Healthy = 0,
        InjuredMinor,
        InjuredMajor,
        Bleeding,
        Downed,
        Dead
    }
}
