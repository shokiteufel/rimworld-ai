using System;
using System.Collections.Generic;
using Verse;

namespace RimWorldBot.UI
{
    // Story 1.8 — DRY-Helper für lokalisierte Enum-Strings.
    // Vorher dupliziert in MainTabWindow_BotControl.TranslateToggleState + SettingsRenderer.TranslateEnum.
    public static class LocalizationHelper
    {
        // Cache für bereits gemeldete Missing-Keys damit DevMode-Warning nicht jeden Frame feuert.
        static readonly HashSet<string> _warnedMissingKeys = new();

        // Übersetzt enum-Wert via Key "{prefix}.{enumValue}". Bei Missing-Key:
        // - Returnt raw enum-Name als Fallback (UI-resilient gegen unvollständige Übersetzungen)
        // - Im DevMode: Log.Warning ONCE pro Key (vermeidet Spam, hilft Story-1.8-Audit)
        public static string TranslateEnum<TEnum>(string keyPrefix, TEnum value) where TEnum : struct, Enum
        {
            var key = $"{keyPrefix}.{value}";
            if (key.CanTranslate())
            {
                return key.Translate().ToString();
            }
            if (Prefs.DevMode && _warnedMissingKeys.Add(key))
            {
                Log.Warning($"[RimWorldBot] Missing translation key: {key} (using raw enum-name fallback)");
            }
            return value.ToString();
        }
    }
}
