# Story 1.7 Visual-Review

**Verdict:** APPROVE-WITH-CHANGES

Statisches Layout-Review von `Source/UI/SettingsRenderer.cs` + DE/EN-Keyed-XML. Kein Live-Screenshot moeglich; Bewertung gegen Vanilla-RimWorld-Konvention.

## UI-Konvention

`Listing_Standard.Begin/End` mit `CheckboxLabeled`, `ButtonText`, `Label` und `GapLine` ist exakt das Vanilla-Mod-Settings-Pattern (vgl. CCL/HugsLib-Mods, Vanilla-Pawn-Settings). Section-Header in `GameFont.Medium` mit Restore-prevFont-Pattern ist sauber. Enum-Dropdown via `ButtonText` + `FloatMenu` ist der korrekte RimWorld-Standard — `Dropdown<T>` waere Alternative, ist aber nicht idiomatischer. **Konvention: konform.**

## Layout-Qualitaet

Row-Height **28f** ist Vanilla-Standard (Listing_Standard nutzt intern verbHeight 30f als default; 28f ist akzeptabel). `LeftPart(0.45f) / RightPart(0.5f)` laesst 5 % Gap dazwischen — visuell ausgewogen, aber nicht alignt: Label endet bei 45 %, Button startet bei 50 %. Wenn Inhalt schmal ist sieht das tight aus. `GapLine()` zwischen Sections ist die richtige Vanilla-Trennung. Section-Header in Medium hebt sich klar vom Small-Body ab.

## Label/Tooltip-Qualitaet

DE+EN parallel, Keys konsistent. Tooltips nennen Defaults explizit ("Standard: deaktiviert", "Andernfalls (Standard)"). Reset-Confirm warnt prazis vor Datenverlust. "Ending" im DE als Lehnwort ist RimWorld-Convention (Vanilla nutzt "Ending" auch in DE-Localisations). Konsistenz zu Story 1.6: "Spieler steuert" wird im Tooltip wortgetreu referenziert (`PerPawn.PlayerUseLabel = "Spieler steuert"`) — sauber.

## Findings

### HIGH

- **H1 — Enum-Dropdowns nicht lokalisiert.** `EndingStrategy.Opportunistic/Forced` und `Ending.Ship/Journey/Royal/Archonexus/Void` werden via `enum.ToString()` als raw English-CamelCase angezeigt. Im DE-UI bricht das die Sprachkonsistenz; auch im EN ist "Archonexus" ok aber "Opportunistic" ist UX-rauh. **Fix:** Keyed-Strings `RimWorldBot.Ending.Strategy.<Value>` + `RimWorldBot.Ending.<Value>` einfuehren, in beiden Dropdowns mappen.

### MED

- **M1 — Mod-Version "0.0.0" Fallback.** `AssemblyVersion` wird im `.csproj` aktuell nicht gesetzt → User sieht "Mod-Version: 0.0.0". Verwirrend. **Fix:** `AssemblyVersion`/`AssemblyFileVersion` im csproj auf 0.1.0 setzen oder Fallback-Text auf "(dev build)" aendern.
- **M2 — GitHub-URL als Button-Text.** Voller URL als ButtonText ist lang und unschoen ("https://github.com/..."). Vanilla-Pattern: Button mit Label "GitHub" + Tooltip mit URL. **Fix:** Keyed-Label `Settings.Info.GitHubButton = "GitHub-Repo oeffnen"` + Tooltip mit URL.
- **M3 — Placeholder "—" fuer ForcedEnding null.** Ein einzelner em-dash ist kryptisch. RimWorld-Convention waere "(none)" / "(keine)". **Fix:** Keyed-String `Settings.Ending.Forced.None`.

### LOW

- **L1 — Reset-Dialog title doppelt.** `title` und implicit Header zeigen beide "Lerndaten zuruecksetzen". Vanilla-Convention: Title kurz, Body erklaert. Akzeptabel, aber redundant.
- **L2 — GapLine zwischen Telemetry und Info wirkt visuell wie Section-Trennung obwohl Info nur 2 Zeilen hat.** Subjektiv: ok.
- **L3 — `LeftPart(0.45f)/RightPart(0.5f)` summiert auf 0.95** — 5 % Mittelgap ist Geschmackssache, manche Mods nutzen 0.5/0.5 mit Inset.

## Recommendation

Story 1.7 ist solide gebaut und folgt Vanilla-Pattern. **Vor "done": H1 fixen** (Enum-Lokalisierung ist UX-blocking fuer DE-User und unvollstaendige Lokalisierungs-Story). M1-M3 in derselben Iteration mitnehmen — alles unter 30 Min Aufwand. L1-L3 dokumentieren, Fix optional.

Nach Fix: Re-Review nicht noetig, Aenderungen sind text-only + 1 csproj-Property.
