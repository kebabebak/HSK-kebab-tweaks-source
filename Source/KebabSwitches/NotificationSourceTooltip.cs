using System;
using System.Text;
using Verse;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// RimWorld tooltip text: translation key plus English keyed template (or hardcoded literal).
    ///
    /// Текст RimWorld-тултипа: ключ перевода и английский keyed-шаблон (или литерал).
    /// </summary>
    public static class NotificationSourceTooltip
    {
        /// <summary>
        /// Tooltip for a suppressible upper-left screen Message row.
        ///
        /// Тултип для строки suppressible screen Message.
        /// </summary>
        public static string ForScreenMessage(SuppressibleScreenMessageEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (!entry.HardcodedEnglish.NullOrEmpty())
            {
                return entry.HardcodedEnglish;
            }

            return FormatKeyedSource(entry.TranslationKey);
        }

        /// <summary>
        /// Tooltip for a suppressible right-side Letter row.
        ///
        /// Тултип для строки suppressible Letter.
        /// </summary>
        public static string ForLetter(SuppressibleLetterEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (entry.IsSurgeryFailed)
            {
                return "SurgeryOutcomeEffectDefs letter label\n"
                    + "EN: Surgery failed on {pawn}\n"
                    + "RU: Операция над ... провалилась / Операция провалилась: ...";
            }

            return FormatKeyedSource(entry.TranslationKey);
        }

        /// <summary>
        /// Tooltip for a suppressible right-side HUD Alert row.
        ///
        /// Тултип для строки suppressible HUD Alert.
        /// </summary>
        public static string ForAlert(SuppressibleAlertEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (!entry.HardcodedEnglish.NullOrEmpty())
            {
                return entry.HardcodedEnglish;
            }

            return FormatKeyedSource(entry.TranslationKey);
        }

        private static string FormatKeyedSource(string translationKey)
        {
            if (translationKey.NullOrEmpty())
            {
                return string.Empty;
            }

            string english = TryEnglishKeyed(translationKey);
            if (english.NullOrEmpty())
            {
                return translationKey;
            }

            var sb = new StringBuilder();
            sb.Append(translationKey);
            sb.Append('\n');
            sb.Append(english);
            return sb.ToString();
        }

        private static string TryEnglishKeyed(string key)
        {
            if (key.NullOrEmpty())
            {
                return null;
            }

            LoadedLanguage english = FindEnglishLanguage();
            if (english != null && english.TryGetTextFromKey(key, out TaggedString tagged))
            {
                string text = tagged;
                if (!text.NullOrEmpty() && !string.Equals(text, key, StringComparison.Ordinal))
                {
                    return text;
                }
            }

            return NotificationEnglishCatalog.TryGet(key)
                ?? AlertEnglishCatalog.TryGet(key);
        }

        private static LoadedLanguage FindEnglishLanguage()
        {
            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (string.Equals(language.folderName, "English", StringComparison.OrdinalIgnoreCase))
                {
                    return language;
                }
            }

            return null;
        }
    }
}
