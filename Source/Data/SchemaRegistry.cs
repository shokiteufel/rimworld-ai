using System.Collections.Generic;

namespace RimWorldBot.Data
{
    // Story 1.9 (CC-STORIES-01) — zentrale Single-Source-of-Truth für alle Schema-Bumps.
    //
    // Jede Story die ein neues persistentes Feld auf BotGameComponent oder BotMapComponent hinzufügt:
    //   1. Status der Bump-Tabelle hier prüfen → nächste freie Version pro Komponente nehmen
    //   2. `CurrentSchemaVersion`-const in der entsprechenden Komponente auf neue Version bumpen
    //   3. Migration-Step in `Migrate()` der Komponente hinzufügen (defensive Default-Init bei v < N)
    //   4. Eintrag in `Bumps`-Liste hier ergänzen mit Story-ID + Reason
    //   5. Story-AC: "Savegame-Roundtrip v(N-1)→v(N) ohne Data-Loss" als Test-Plan dokumentieren
    //
    // Idempotenz-Vertrag: Migrate-Steps prüfen `if (schemaVersion < N)` — mehrfaches Apply
    // ist deshalb safe, weil schemaVersion am Ende auf CurrentSchemaVersion gesetzt wird.
    //
    // Test-Plan (Implementation in Story 1.13 Test-Infrastructure):
    //   - Pro Bump: Fake-Save mit FromVersion → Migrate ausführen → assert Felder == Defaults
    //   - Kaskadiert: Fake-Save mit ältester Version → Migrate ausführen → assert alle Felder migriert
    //   - Doppel-Apply: Migrate zweimal → assert idempotent (gleicher Endzustand)
    public static class SchemaRegistry
    {
        public enum BumpStatus
        {
            Applied,
            Planned
        }

        public sealed record SchemaBump(
            string Component,
            int FromVersion,
            int ToVersion,
            string FieldChanges,
            string StoryId,
            BumpStatus Status);

        public const string ComponentBotGame = "BotGameComponent";
        public const string ComponentBotMap = "BotMapComponent";

        // Chronologische History + geplante Bumps. Order matters für Migrations-Chains.
        public static readonly IReadOnlyList<SchemaBump> Bumps = new List<SchemaBump>
        {
            // --- BotGameComponent ---
            new(ComponentBotGame, 1, 2,
                "perPawnPlayerUse keys: int thingIDNumber → string UniqueLoadID (D-14 Migrate v1→v2)",
                StoryId: "1.3", Status: BumpStatus.Applied),
            new(ComponentBotGame, 2, 3,
                "Reservierter Versions-Sprung — synchronisiert die Version-Achse mit BotMapComponent v3 (excludedCells); keine Felder geändert in BotGameComponent",
                StoryId: "1.3", Status: BumpStatus.Applied),
            new(ComponentBotGame, 3, 4,
                "+botManagedGuests Dictionary für Recruiting-PlayerUse-Override",
                StoryId: "4.3", Status: BumpStatus.Planned),
            new(ComponentBotGame, 4, 5,
                "+pawnSpecializations Dictionary<UniqueLoadID, Specialization> für Crafter/Medic/Researcher/Combat-Rollen",
                StoryId: "6.5", Status: BumpStatus.Planned),
            new(ComponentBotGame, 5, 6,
                "+journeyQuest: JourneyQuestRef? für Journey-Ending-Quest-Tracking",
                StoryId: "7.9", Status: BumpStatus.Planned),

            // --- BotMapComponent ---
            new(ComponentBotMap, 1, 2,
                "+botPlacedThings + botAssignedJobs Tag-Dicts (D-25)",
                StoryId: "1.3", Status: BumpStatus.Applied),
            new(ComponentBotMap, 2, 3,
                "+excludedCells: HashSet<(int,int)> für Hazard-Filter",
                StoryId: "1.3 (vorgezogen aus 2.3)", Status: BumpStatus.Applied),
            new(ComponentBotMap, 3, 4,
                "+botManagedBills Dictionary<int, PhaseGoalTag> für Bill-Manager-Tag-Tracking",
                StoryId: "3.9", Status: BumpStatus.Planned),
            new(ComponentBotMap, 4, 5,
                "+overlayVisible: bool für Site-Marker-Toggle",
                StoryId: "2.7", Status: BumpStatus.Planned),
        };

        // Helper: aktuelle Version pro Komponente lookup.
        // Baseline = 1: V1 ist die initiale Schema-Version JEDER Komponente — auch ohne Bumps
        // ist eine frische Component bei v1 (keine Migrations nötig). Sollte für beide Components
        // mit der hartcodierten `CurrentSchemaVersion`-const übereinstimmen
        // (Konsistenz-Check kommt in Story 1.13 Unit-Test).
        public static int LatestAppliedVersion(string component)
        {
            int latest = 1;
            foreach (var bump in Bumps)
            {
                if (bump.Component == component && bump.Status == BumpStatus.Applied && bump.ToVersion > latest)
                {
                    latest = bump.ToVersion;
                }
            }
            return latest;
        }
    }
}
