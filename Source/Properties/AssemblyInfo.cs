using System.Runtime.CompilerServices;

// CR Story 1.13 HIGH-2-Fix: ermoeglicht dem Test-Assembly auf internal-Members zuzugreifen
// (z.B. BotGameComponent.CurrentSchemaVersion fuer Drift-Detection ohne Reflection).
[assembly: InternalsVisibleTo("RimWorldBot.Tests")]
