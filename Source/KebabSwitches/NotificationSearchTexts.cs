using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Ignore-row search checks four lists at once, character by character: the checkbox the
    /// player sees in English, the checkbox in Russian, the English game sentence, and the
    /// Russian game sentence (label and body, not only the short quote).
    ///
    /// Поиск строки игнора проверяет четыре списка сразу, посимвольно: чекбокс, который игрок
    /// видит по-английски, чекбокс по-русски, английскую фразу игры и русскую фразу игры
    /// (заголовок и текст, не только короткая цитата).
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

        private static readonly Dictionary<string, string> KeyedHaystacks = new Dictionary<string, string>();

        /// <summary>
        /// Search text for a keyed label on the Notifications tab: the drawn translation plus
        /// the same key in every indexed language.
        ///
        /// Текст поиска подписи по ключу на вкладке Уведомления: нарисованный перевод и тот же
        /// ключ на каждом учитываемом языке.
        /// </summary>
        public static string ForKeyed(string key)
        {
            if (key.NullOrEmpty())
            {
                return string.Empty;
            }

            if (KeyedHaystacks.TryGetValue(key, out string cached))
            {
                return cached;
            }

            List<string> parts = new List<string>();
            List<LoadedLanguage> languages = IndexedLanguages();
            for (int i = 0; i < languages.Count; i++)
            {
                if (TryOwn(languages[i], key, out string text))
                {
                    parts.Add(text);
                }
            }

            string haystack = Join(key.Translate(), parts);
            KeyedHaystacks[key] = haystack;
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
        /// True when the query is a character sequence inside any of the four ignore-row lists.
        ///
        /// Истина, если запрос - последовательность символов в любом из четырёх списков строки игнора.
        /// </summary>
        public static bool Matches(SuppressibleScreenMessageEntry entry, string query)
        {
            if (entry == null || query.NullOrEmpty())
            {
                return false;
            }

            if (!ScreenQuads.TryGetValue(entry, out SearchQuad quad))
            {
                quad = BuildScreenQuad(entry);
                ScreenQuads[entry] = quad;
            }

            return QuadHits(quad, query);
        }

        public static bool Matches(SuppressibleLetterEntry entry, string query)
        {
            if (entry == null || query.NullOrEmpty())
            {
                return false;
            }

            if (!LetterQuads.TryGetValue(entry, out SearchQuad quad))
            {
                quad = BuildLetterQuad(entry);
                LetterQuads[entry] = quad;
            }

            return QuadHits(quad, query);
        }

        public static bool Matches(SuppressibleAlertEntry entry, string query)
        {
            if (entry == null || query.NullOrEmpty())
            {
                return false;
            }

            if (!AlertQuads.TryGetValue(entry, out SearchQuad quad))
            {
                quad = BuildAlertQuad(entry);
                AlertQuads[entry] = quad;
            }

            return QuadHits(quad, query);
        }

        private struct SearchQuad
        {
            public string DisplayedEnglish;
            public string DisplayedRussian;
            public string GameEnglish;
            public string GameRussian;
        }

        private static readonly Dictionary<SuppressibleScreenMessageEntry, SearchQuad> ScreenQuads =
            new Dictionary<SuppressibleScreenMessageEntry, SearchQuad>();

        private static readonly Dictionary<SuppressibleLetterEntry, SearchQuad> LetterQuads =
            new Dictionary<SuppressibleLetterEntry, SearchQuad>();

        private static readonly Dictionary<SuppressibleAlertEntry, SearchQuad> AlertQuads =
            new Dictionary<SuppressibleAlertEntry, SearchQuad>();

        private static readonly string[] FoodCauseKeys =
        {
            "FoodPoisonCause_IncompetentCook",
            "FoodPoisonCause_FilthyKitchen",
            "FoodPoisonCause_Rotten",
            "FoodPoisonCause_DangerousFoodType",
        };

        private static SearchQuad BuildScreenQuad(SuppressibleScreenMessageEntry entry)
        {
            SearchQuad quad = new SearchQuad
            {
                DisplayedEnglish = DisplayedIn(entry.CheckboxLabelKey, english: true),
                DisplayedRussian = DisplayedIn(entry.CheckboxLabelKey, english: false),
                GameEnglish = ScreenGame(entry, russian: false),
                GameRussian = ScreenGame(entry, russian: true),
            };
            FillEmptyGameText(ref quad);
            return quad;
        }

        private static SearchQuad BuildLetterQuad(SuppressibleLetterEntry entry)
        {
            SearchQuad quad = new SearchQuad
            {
                DisplayedEnglish = DisplayedLetter(entry, english: true),
                DisplayedRussian = DisplayedLetter(entry, english: false),
                GameEnglish = LetterGame(entry, russian: false),
                GameRussian = LetterGame(entry, russian: true),
            };
            FillEmptyGameText(ref quad);
            return quad;
        }

        private static SearchQuad BuildAlertQuad(SuppressibleAlertEntry entry)
        {
            SearchQuad quad = new SearchQuad
            {
                DisplayedEnglish = DisplayedAlert(entry, english: true),
                DisplayedRussian = DisplayedAlert(entry, english: false),
                GameEnglish = AlertGame(entry, russian: false),
                GameRussian = AlertGame(entry, russian: true),
            };
            FillEmptyGameText(ref quad);
            return quad;
        }

        /// <summary>
        /// When a language has no translation, the game shows the other language's sentence.
        /// That sentence is stored on the empty side so the row still has both game lists.
        ///
        /// Если у языка нет перевода, игра показывает фразу другого языка. Эта фраза кладётся
        /// в пустой список, чтобы у строки были оба игровых списка.
        /// </summary>
        private static void FillEmptyGameText(ref SearchQuad quad)
        {
            if (quad.GameRussian.NullOrEmpty())
            {
                quad.GameRussian = quad.GameEnglish;
            }

            if (quad.GameEnglish.NullOrEmpty())
            {
                quad.GameEnglish = quad.GameRussian;
            }
        }

        private static bool QuadHits(SearchQuad quad, string query)
        {
            return Hit(quad.DisplayedEnglish, query)
                || Hit(quad.DisplayedRussian, query)
                || Hit(quad.GameEnglish, query)
                || Hit(quad.GameRussian, query);
        }

        private static bool Hit(string haystack, string query)
        {
            if (haystack.NullOrEmpty() || query.NullOrEmpty())
            {
                return false;
            }

            string foldedHay = KeyedSuppressMatcher.Normalize(haystack);
            string foldedQuery = KeyedSuppressMatcher.Normalize(query);
            if (foldedQuery.Length == 0)
            {
                return false;
            }

            return foldedHay.IndexOf(foldedQuery, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string DisplayedIn(string checkboxKey, bool english)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (language == null || (english ? !IsEnglish(language) : !IsRussian(language)))
                {
                    continue;
                }

                if (TryOwn(language, checkboxKey, out string text))
                {
                    AppendUnique(sb, seen, text);
                }
            }

            return sb.ToString();
        }

        private static string DisplayedLetter(SuppressibleLetterEntry entry, bool english)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (language == null || (english ? !IsEnglish(language) : !IsRussian(language)))
                {
                    continue;
                }

                string quote = LetterQuote(language, entry);
                AppendUnique(sb, seen, Compose(language, IgnoreLetterKey, quote));
            }

            return sb.ToString();
        }

        private static string DisplayedAlert(SuppressibleAlertEntry entry, bool english)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (language == null || (english ? !IsEnglish(language) : !IsRussian(language)))
                {
                    continue;
                }

                string quote = AlertQuote(language, entry);
                AppendUnique(sb, seen, Compose(language, IgnoreAlertKey, quote));
            }

            return sb.ToString();
        }

        private static string ScreenGame(SuppressibleScreenMessageEntry entry, bool russian)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!entry.TranslationKey.NullOrEmpty())
            {
                AppendLanguageText(sb, seen, entry.TranslationKey, russian);
                if (!russian)
                {
                    AppendUnique(sb, seen, NotificationEnglishCatalog.TryGet(entry.TranslationKey));
                }
            }

            if (!russian && !entry.HardcodedEnglish.NullOrEmpty())
            {
                AppendUnique(sb, seen, entry.HardcodedEnglish);
            }

            AppendFilterExacts(sb, seen, entry.Id, russian, screen: true);
            if (string.Equals(entry.Id, "FoodPoisoning", StringComparison.Ordinal))
            {
                for (int i = 0; i < FoodCauseKeys.Length; i++)
                {
                    AppendLanguageText(sb, seen, FoodCauseKeys[i], russian);
                }
            }

            NotificationStoredGameText.Append(entry.Id, russian, sb, seen);
            return sb.ToString();
        }

        private static string LetterGame(SuppressibleLetterEntry entry, bool russian)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!entry.TranslationKey.NullOrEmpty())
            {
                AppendLanguageText(sb, seen, entry.TranslationKey, russian);
                if (!russian)
                {
                    AppendUnique(sb, seen, NotificationEnglishCatalog.TryGet(entry.TranslationKey));
                }

                foreach (string bodyKey in LetterSuppressFilter.BodyKeys(entry.TranslationKey))
                {
                    AppendLanguageText(sb, seen, bodyKey, russian);
                    if (!russian)
                    {
                        AppendUnique(sb, seen, NotificationEnglishCatalog.TryGet(bodyKey));
                    }
                }
            }

            AppendFilterExacts(sb, seen, entry.Id, russian, screen: false);
            NotificationStoredGameText.Append(entry.Id, russian, sb, seen);
            return sb.ToString();
        }

        private static string AlertGame(SuppressibleAlertEntry entry, bool russian)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!entry.HardcodedEnglish.NullOrEmpty() && !russian)
            {
                AppendUnique(sb, seen, entry.HardcodedEnglish);
            }

            if (!entry.TranslationKey.NullOrEmpty())
            {
                AppendLanguageText(sb, seen, entry.TranslationKey, russian);
                if (!russian)
                {
                    AppendUnique(sb, seen, AlertEnglishCatalog.TryGet(entry.TranslationKey));
                }
            }

            NotificationStoredGameText.Append(entry.Id, russian, sb, seen);
            return sb.ToString();
        }

        private static void AppendLanguageText(StringBuilder sb, HashSet<string> seen, string key, bool russian)
        {
            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (language == null || (russian ? !IsRussian(language) : !IsEnglish(language)))
                {
                    continue;
                }

                if (TryOwn(language, key, out string text))
                {
                    AppendUnique(sb, seen, text);
                }
            }
        }

        private static void AppendFilterExacts(
            StringBuilder sb,
            HashSet<string> seen,
            string id,
            bool russian,
            bool screen)
        {
            List<string> exact = new List<string>();
            if (screen)
            {
                ScreenMessageSuppressFilter.CollectExact(id, exact);
            }
            else
            {
                LetterSuppressFilter.CollectExact(id, exact);
            }

            for (int i = 0; i < exact.Count; i++)
            {
                string text = exact[i];
                if (HasCyrillic(text) == russian)
                {
                    AppendUnique(sb, seen, text);
                }
            }
        }

        private static bool HasCyrillic(string text)
        {
            if (text == null)
            {
                return false;
            }

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= '\u0400' && c <= '\u04FF')
                {
                    return true;
                }
            }

            return false;
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
