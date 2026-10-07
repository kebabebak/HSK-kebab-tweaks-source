using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Search text for one ignore row: the drawn label plus the same label in the active
    /// language, English, and Russian (including Russian-SK), so a query in another language
    /// still matches.
    ///
    /// Текст поиска одной строки игнора: нарисованная подпись и та же подпись на активном
    /// языке, английском и русском (включая Russian-SK), чтобы запрос на другом языке тоже
    /// находил строку.
    /// </summary>
    public static class NotificationSearchTexts
    {
        private const string IgnoreLetterKey = "KebabSwitches.IgnoreLetter";
        private const string IgnoreAlertKey = "KebabSwitches.IgnoreAlert";

        private static readonly Dictionary<SuppressibleScreenMessageEntry, string> ScreenHaystacks =
            new Dictionary<SuppressibleScreenMessageEntry, string>();

        private static readonly Dictionary<SuppressibleLetterEntry, string> LetterHaystacks =
            new Dictionary<SuppressibleLetterEntry, string>();

        private static readonly Dictionary<SuppressibleAlertEntry, string> AlertHaystacks =
            new Dictionary<SuppressibleAlertEntry, string>();

        private static readonly Dictionary<LoadedLanguage, Dictionary<string, string>> KeyTextByLanguage =
            new Dictionary<LoadedLanguage, Dictionary<string, string>>();

        private static LoadedLanguage englishLanguage;

        public static string For(SuppressibleScreenMessageEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (ScreenHaystacks.TryGetValue(entry, out string cached))
            {
                return cached;
            }

            string haystack = Join(entry.CheckboxLabel, ScreenParts(entry));
            ScreenHaystacks[entry] = haystack;
            return haystack;
        }

        public static string For(SuppressibleLetterEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (LetterHaystacks.TryGetValue(entry, out string cached))
            {
                return cached;
            }

            string haystack = Join(entry.CheckboxLabel, LetterParts(entry));
            LetterHaystacks[entry] = haystack;
            return haystack;
        }

        public static string For(SuppressibleAlertEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (AlertHaystacks.TryGetValue(entry, out string cached))
            {
                return cached;
            }

            string haystack = Join(entry.CheckboxLabel, AlertParts(entry));
            AlertHaystacks[entry] = haystack;
            return haystack;
        }

        /// <summary>
        /// Missing keys are skipped. Translator's English fallback is already the drawn label.
        ///
        /// Нет ключа - пропуск. Запасной английский Translator уже есть в нарисованной подписи.
        /// </summary>
        private static List<string> ScreenParts(SuppressibleScreenMessageEntry entry)
        {
            List<string> parts = new List<string>();
            List<LoadedLanguage> languages = IndexedLanguages();
            for (int i = 0; i < languages.Count; i++)
            {
                if (TryOwn(languages[i], entry.CheckboxLabelKey, out string text))
                {
                    parts.Add(text);
                }
            }

            return parts;
        }

        /// <summary>
        /// Letter checkbox as it would be drawn in each indexed language: IgnoreLetter wrapper
        /// plus the sanitized LetterQuote, with English used when that language has no own text.
        ///
        /// Чекбокс письма, как он был бы нарисован на каждом учитываемом языке: обёртка
        /// IgnoreLetter и очищенная LetterQuote; если своего текста нет, берётся английский.
        /// </summary>
        private static List<string> LetterParts(SuppressibleLetterEntry entry)
        {
            List<string> parts = new List<string>();
            List<LoadedLanguage> languages = IndexedLanguages();
            for (int i = 0; i < languages.Count; i++)
            {
                LoadedLanguage language = languages[i];
                string quote = LetterQuote(language, entry);
                parts.Add(Compose(language, IgnoreLetterKey, quote));
            }

            return parts;
        }

        /// <summary>
        /// Alert checkbox as it would be drawn in each indexed language. The quote uses the same
        /// placeholder formatting as AlertLabelPreview, then the IgnoreAlert wrapper.
        ///
        /// Чекбокс оповещения, как он был бы нарисован на каждом учитываемом языке. Цитата
        /// форматирует плейсхолдеры так же, как AlertLabelPreview, затем обёртка IgnoreAlert.
        /// </summary>
        private static List<string> AlertParts(SuppressibleAlertEntry entry)
        {
            List<string> parts = new List<string>();
            List<LoadedLanguage> languages = IndexedLanguages();
            for (int i = 0; i < languages.Count; i++)
            {
                LoadedLanguage language = languages[i];
                string quote = AlertQuote(language, entry);
                parts.Add(Compose(language, IgnoreAlertKey, quote));
            }

            return parts;
        }

        private static string LetterQuote(LoadedLanguage language, SuppressibleLetterEntry entry)
        {
            string raw = Resolve(language, entry.QuoteKey);
            if (raw == null)
            {
                raw = entry.QuoteKey;
            }

            return LetterLabelPreview.SanitizeQuote(raw);
        }

        private static string AlertQuote(LoadedLanguage language, SuppressibleAlertEntry entry)
        {
            if (!entry.HardcodedEnglish.NullOrEmpty())
            {
                return entry.HardcodedEnglish;
            }

            if (entry.TranslationKey.NullOrEmpty())
            {
                return entry.Id;
            }

            string template = Resolve(language, entry.TranslationKey);
            if (template == null && IsEnglish(language))
            {
                template = AlertEnglishCatalog.TryGet(entry.TranslationKey);
            }

            if (template == null)
            {
                return entry.TranslationKey;
            }

            return LetterLabelPreview.SanitizeQuote(AlertLabelPreview.FormatTemplateForSettingsQuote(template));
        }

        /// <summary>
        /// Haystack line is the checkbox label a language switch would show.
        ///
        /// Строка набора - подпись чекбокса, которую покажет смена языка.
        /// </summary>
        private static string Compose(LoadedLanguage language, string wrapperKey, string quote)
        {
            string wrapper = Resolve(language, wrapperKey);
            if (wrapper.NullOrEmpty())
            {
                return quote;
            }

            return wrapper.Replace("{QUOTE}", quote ?? string.Empty);
        }

        /// <summary>
        /// Keyed value in this language, otherwise English.
        ///
        /// Значение ключа на этом языке, иначе английский.
        /// </summary>
        private static string Resolve(LoadedLanguage language, string key)
        {
            if (TryOwn(language, key, out string own))
            {
                return own;
            }

            LoadedLanguage english = EnglishLanguage();
            if (english != null && !ReferenceEquals(english, language) && TryOwn(english, key, out string fallback))
            {
                return fallback;
            }

            return null;
        }

        private static bool TryOwn(LoadedLanguage language, string key, out string text)
        {
            text = null;
            if (language == null || key.NullOrEmpty())
            {
                return false;
            }

            if (!KeyTextByLanguage.TryGetValue(language, out Dictionary<string, string> map))
            {
                map = new Dictionary<string, string>();
                KeyTextByLanguage[language] = map;
            }

            if (map.TryGetValue(key, out string cached))
            {
                text = cached;
                return cached != null;
            }

            string resolved = null;
            if (language.TryGetTextFromKey(key, out TaggedString tagged))
            {
                string raw = tagged.RawText;
                if (raw.NullOrEmpty())
                {
                    raw = tagged;
                }

                if (!raw.NullOrEmpty() && !string.Equals(raw, key, StringComparison.Ordinal))
                {
                    resolved = raw;
                }
            }

            map[key] = resolved;
            text = resolved;
            return resolved != null;
        }

        /// <summary>
        /// Languages whose checkbox text is searched: the active language, English, and Russian
        /// (folder or friendly name, so Russian-SK is included).
        ///
        /// Языки, чьи подписи чекбоксов ищутся: активный, английский и русский
        /// (папка или английское имя, поэтому Russian-SK тоже).
        /// </summary>
        private static List<LoadedLanguage> IndexedLanguages()
        {
            List<LoadedLanguage> languages = new List<LoadedLanguage>();
            LoadedLanguage active = LanguageDatabase.activeLanguage;
            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (language == null)
                {
                    continue;
                }

                if (ReferenceEquals(language, active) || IsEnglish(language) || IsRussian(language))
                {
                    languages.Add(language);
                }
            }

            return languages;
        }

        private static LoadedLanguage EnglishLanguage()
        {
            if (englishLanguage != null)
            {
                return englishLanguage;
            }

            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (IsEnglish(language))
                {
                    englishLanguage = language;
                    return englishLanguage;
                }
            }

            englishLanguage = LanguageDatabase.defaultLanguage;
            return englishLanguage;
        }

        private static bool IsEnglish(LoadedLanguage language)
        {
            return NameStartsWith(language, "English");
        }

        private static bool IsRussian(LoadedLanguage language)
        {
            return NameStartsWith(language, "Russian");
        }

        private static bool NameStartsWith(LoadedLanguage language, string prefix)
        {
            string folder = language.folderName;
            if (!folder.NullOrEmpty()
                && folder.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string friendly = language.FriendlyNameEnglish;
            return !friendly.NullOrEmpty()
                && friendly.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private static string Join(string displayed, List<string> parts)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            AppendUnique(sb, seen, displayed);
            for (int i = 0; i < parts.Count; i++)
            {
                AppendUnique(sb, seen, parts[i]);
            }

            return sb.ToString();
        }

        private static void AppendUnique(StringBuilder sb, HashSet<string> seen, string text)
        {
            if (text.NullOrEmpty() || !seen.Add(text))
            {
                return;
            }

            if (sb.Length > 0)
            {
                sb.Append('\n');
            }

            sb.Append(text);
        }
    }
}
