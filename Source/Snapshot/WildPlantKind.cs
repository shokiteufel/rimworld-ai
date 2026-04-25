namespace RimWorldBot.Snapshot
{
    // Story 2.2 — wild-essbare/medizinische Pflanzen-Klassifikation pro Cell.
    // Konsumiert von Story 2.5 (Scoring-Formula W_FOOD) + Story 4.10 (Skill-Grinding-Plant-Cut).
    //
    // D-42 (2026-04-25): GlowyMushroom aus Original-AC entfernt — existiert nicht in Vanilla 1.6.
    // Ambrosia ist Core (kein Biotech-DLC-Guard). Verifizierte Vanilla-defNames in WildPlantRegistry.
    //
    // `Other` ist Foundation für Story 2.5 Scoring-Formula: dort wird ein Producer hinzugefügt
    // der alle Plant-Things mit `def.plant != null && def.plant.harvestedThingDef != null` aber
    // NICHT in der WildPlantRegistry-Whitelist als `Other` klassifiziert (= Mod-Plants oder
    // unbekannte Vanilla-Plants mit Yield). Story 2.5 wertet `Other` mit niedrigerem W_FOOD-Beitrag.
    // Bis dahin: Provider klassifiziert nie `Other` (toter Enum-Wert auf der Producer-Seite,
    // aber bewusst reserviert für Story 2.5 — kein YAGNI-Verstoß).
    public enum WildPlantKind
    {
        Berries = 0,
        Healroot,
        Agave,
        AmbrosiaBush,
        PsychoidPlant,
        Smokeleaf,
        Other
    }
}
