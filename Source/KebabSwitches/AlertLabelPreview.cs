using System;
using System.Text.RegularExpressions;
using Verse;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Builds checkbox quote text from vanilla keyed alert labels, keeping {0}/{1} placeholders
    /// instead of sample numbers (no Translate with fake grammar args).
    ///
    /// Цитата для чекбоксов из keyed label Alert: шаблон с {0}/{1}, без Translate с фиктивными аргументами.
    /// </summary>
    public static class AlertLabelPreview
    {
        private static readonly Regex PlaceholderPattern = new Regex(
            @"\{[^}]+\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Russian-SK / grammar: {0_numCase ? a : b : c} → {0} c (plural form for settings quote).
        ///
        /// Russian-SK / grammar: {0_numCase ? a : b : c} → {0} c (множественная форма для цитаты в настройках).
        /// </summary>
        private static readonly Regex NumCasePattern = new Regex(
            @"\{(\d+)_numCase\s*\?\s*([^:]+?)\s*:\s*([^:]+?)\s*:\s*([^}]+?)\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string GetQuote(SuppressibleAlertEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (!entry.HardcodedEnglish.NullOrEmpty())
            {
                return entry.HardcodedEnglish;
            }

            if (entry.TranslationKey.NullOrEmpty())
            {
                return entry.Id;
            }

            string template = TryGetKeyedTemplate(entry.TranslationKey);
            if (!template.NullOrEmpty())
            {
                return LetterLabelPreview.SanitizeQuote(FormatTemplateForSettingsQuote(template));
            }

            return entry.TranslationKey;
        }

        /// <summary>
        /// Raw keyed label template (active language, else English catalog) without grammar resolution.
        ///
        /// Сырой keyed-шаблон label (активный язык, затем EN-каталог) без разрешения grammar.
        /// </summary>
        private static string TryGetKeyedTemplate(string translationKey)
        {
            LoadedLanguage active = LanguageDatabase.activeLanguage;
            if (active != null
                && active.TryGetTextFromKey(translationKey, out TaggedString activeTagged))
            {
                string text = TaggedRawText(activeTagged);
                if (!text.NullOrEmpty() && !string.Equals(text, translationKey, StringComparison.Ordinal))
                {
                    return text;
                }
            }

            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (!string.Equals(language.folderName, "English", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (language.TryGetTextFromKey(translationKey, out TaggedString englishTagged))
                {
                    string text = TaggedRawText(englishTagged);
                    if (!text.NullOrEmpty() && !string.Equals(text, translationKey, StringComparison.Ordinal))
                    {
                        return text;
                    }
                }

                break;
            }

            return AlertEnglishCatalog.TryGet(translationKey);
        }

        private static string TaggedRawText(TaggedString tagged)
        {
            if (tagged == null)
            {
                return null;
            }

            return tagged.RawText ?? tagged.ToString();
        }

        /// <summary>
        /// Expands numCase grammar and normalizes other placeholders to {0}, {1}, … for settings display.
        ///
        /// Раскрывает numCase-грамматику и нормализует остальные плейсхолдеры в {0}, {1}, … для настроек.
        /// </summary>
        private static string FormatTemplateForSettingsQuote(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            text = ExpandNumCaseGrammar(text);
            return NormalizePlaceholders(text);
        }

        private static string ExpandNumCaseGrammar(string text)
        {
            return NumCasePattern.Replace(text, match =>
            {
                string index = match.Groups[1].Value;
                string pluralForm = match.Groups[4].Value.Trim();
                return "{" + index + "} " + pluralForm;
            });
        }

        /// <summary>
        /// Replaces RimWorld grammar placeholders ({ANIMAL_labelShort}, …) with {0}, {1}, …
        ///
        /// Заменяет grammar-плейсхолдеры RimWorld на {0}, {1}, …
        /// </summary>
        private static string NormalizePlaceholders(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            int index = 0;
            return PlaceholderPattern.Replace(text, _ => "{" + index++ + "}");
        }
    }
}
