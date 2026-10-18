using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Matches live letter label or body text against suppressible catalog entries (keyed templates and
    /// surgery-failure regex). A long-range mineral lump on 1.7 is a quest whose title is not
    /// LetterLabelFoundPreciousLump; its body is matched by the scanner phrase. Rebuilds after language load.
    /// The first matching id for a given text does not depend on checkboxes, so it is remembered until that rebuild.
    ///
    /// Сопоставляет label или текст письма с каталогом (keyed-шаблоны и regex провала хирургии).
    /// Далёкие залежи в 1.7 - квест, заголовок которого не LetterLabelFoundPreciousLump; текст ловится
    /// фразой про сканер. Кэш пересобирается после загрузки языка.
    /// Первый совпавший id для данного текста не зависит от чекбоксов и запоминается до этой пересборки.
    /// </summary>
    public static class LetterSuppressFilter
    {
        private static readonly Regex SurgeryFailedEnglishPattern = new Regex(
            @"^Surgery failed on\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex SurgeryFailedRussianStandardPattern = new Regex(
            @"^Операция над .+ провалилась$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex SurgeryFailedRussianSterilizedPattern = new Regex(
            @"^Операция провалилась:",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex LongRangeScannerEnglishPattern = new Regex(
            "long-range mineral scanner",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex LongRangeScannerRussianPattern = new Regex(
            "сканера дальнего действия",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex DeepScannerEnglishPattern = new Regex(
            "ground-penetrating scanner",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Regex DeepScannerRussianPattern = new Regex(
            "глубинного сканера",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Dictionary<string, List<Regex>> PatternsById =
            new Dictionary<string, List<Regex>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, HashSet<string>> ExactById =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, string> MatchIdByText =
            new Dictionary<string, string>(StringComparer.Ordinal);

        private const int MatchIdCacheLimit = 128;

        public static int TrackedVariantCount
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<string, HashSet<string>> pair in ExactById)
                {
                    count += pair.Value.Count;
                }

                foreach (KeyValuePair<string, List<Regex>> pair in PatternsById)
                {
                    count += pair.Value.Count;
                }

                return count;
            }
        }

        public static void RebuildCache()
        {
            PatternsById.Clear();
            ExactById.Clear();
            MatchIdByText.Clear();

            foreach (SuppressibleLetterEntry entry in SuppressibleLetters.All)
            {
                EnsureBuckets(entry.Id);

                if (!string.IsNullOrEmpty(entry.TranslationKey))
                {
                    AddFromTranslationKey(entry.Id, entry.TranslationKey);
                    foreach (string bodyKey in BodyKeys(entry.TranslationKey))
                    {
                        if (IsLongerBody(entry.TranslationKey, bodyKey))
                        {
                            AddFromTranslationKey(entry.Id, bodyKey);
                        }
                    }
                }

                if (string.Equals(entry.Id, "FoundPreciousLump", StringComparison.Ordinal))
                {
                    AddPattern(entry.Id, LongRangeScannerEnglishPattern);
                    AddPattern(entry.Id, LongRangeScannerRussianPattern);
                }

                if (string.Equals(entry.Id, "DeepScannerFoundLump", StringComparison.Ordinal))
                {
                    AddPattern(entry.Id, DeepScannerEnglishPattern);
                    AddPattern(entry.Id, DeepScannerRussianPattern);
                }

                NotificationStoredGameText.AppendDefLetterLabels(entry.Id, text => AddExact(entry.Id, text));

                if (entry.IsSurgeryFailed)
                {
                    AddPattern(entry.Id, SurgeryFailedEnglishPattern);
                    AddPattern(entry.Id, SurgeryFailedRussianStandardPattern);
                    AddPattern(entry.Id, SurgeryFailedRussianSterilizedPattern);
                }
            }

            foreach (string id in new List<string>(ExactById.Keys))
            {
                foreach (string template in ExactById[id])
                {
                    Regex regex = KeyedSuppressMatcher.TemplateToRegex(template);
                    if (regex != null)
                    {
                        AddPattern(id, regex);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the catalog entry id that matches this live letter label or body, or null.
        /// LettersOnGUI asks again every draw. The remembered id is the first catalog hit, including a miss,
        /// and a later entry does not override it. RebuildCache drops the memory; a large set is dropped so letter bodies are not kept for the whole session.
        ///
        /// Id записи каталога, совпавшей с label или текстом письма, либо null.
        /// LettersOnGUI спрашивает снова на каждой отрисовке. Запоминается первый id каталога, в том числе промах,
        /// и более поздняя запись его не перекрывает. RebuildCache сбрасывает память; большой набор сбрасывается, чтобы тексты писем не держались всю сессию.
        /// </summary>
        public static string MatchEntryId(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return null;
            }

            if (MatchIdByText.TryGetValue(label, out string cachedId))
            {
                return cachedId;
            }

            string id = null;
            string normalized = KeyedSuppressMatcher.Normalize(label);
            foreach (SuppressibleLetterEntry entry in SuppressibleLetters.All)
            {
                if (ExactById.TryGetValue(entry.Id, out HashSet<string> exact)
                    && exact.Contains(normalized))
                {
                    id = entry.Id;
                    break;
                }

                if (PatternsById.TryGetValue(entry.Id, out List<Regex> patterns))
                {
                    for (int i = 0; i < patterns.Count; i++)
                    {
                        if (patterns[i].IsMatch(normalized))
                        {
                            id = entry.Id;
                            break;
                        }
                    }
                }

                if (id != null)
                {
                    break;
                }
            }

            if (MatchIdByText.Count >= MatchIdCacheLimit)
            {
                MatchIdByText.Clear();
            }

            MatchIdByText[label] = id;
            return id;
        }

        public static void CollectExact(string id, List<string> target)
        {
            if (target == null || string.IsNullOrEmpty(id))
            {
                return;
            }

            if (ExactById.TryGetValue(id, out HashSet<string> exact))
            {
                foreach (string text in exact)
                {
                    target.Add(text);
                }
            }
        }

        public static SuppressibleLetterEntry FindEntry(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            for (int i = 0; i < SuppressibleLetters.All.Length; i++)
            {
                if (string.Equals(SuppressibleLetters.All[i].Id, id, StringComparison.Ordinal))
                {
                    return SuppressibleLetters.All[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Body key next to a LetterLabel* title: RoofCollapsed, or Letter + the rest of the name.
        ///
        /// Ключ текста рядом с заголовком LetterLabel*: RoofCollapsed либо Letter + хвост имени.
        /// </summary>
        internal static IEnumerable<string> BodyKeys(string labelKey)
        {
            const string prefix = "LetterLabel";
            if (string.IsNullOrEmpty(labelKey) || !labelKey.StartsWith(prefix, StringComparison.Ordinal))
            {
                yield break;
            }

            string rest = labelKey.Substring(prefix.Length);
            if (rest.Length == 0)
            {
                yield break;
            }

            yield return rest;
            yield return "Letter" + rest;
        }

        private static bool IsLongerBody(string labelKey, string bodyKey)
        {
            if (string.IsNullOrEmpty(bodyKey) || !bodyKey.CanTranslate())
            {
                return false;
            }

            string body;
            try
            {
                body = bodyKey.Translate().ToString();
            }
            catch (Exception)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(body)
                || string.Equals(body, bodyKey, StringComparison.Ordinal))
            {
                return false;
            }

            string label = labelKey;
            if (labelKey.CanTranslate())
            {
                try
                {
                    label = labelKey.Translate().ToString();
                }
                catch (Exception)
                {
                    label = labelKey;
                }
            }

            return !string.Equals(body, label, StringComparison.Ordinal) && body.Length > label.Length;
        }

        private static void AddFromTranslationKey(string entryId, string key)
        {
            try
            {
                if (key.CanTranslate())
                {
                    string translated = key.Translate().ToString();
                    if (!string.IsNullOrWhiteSpace(translated)
                        && !string.Equals(translated, key, StringComparison.Ordinal))
                    {
                        AddExact(entryId, translated);
                    }
                }
                else
                {
                    string english = NotificationEnglishCatalog.TryGet(key);
                    if (!string.IsNullOrWhiteSpace(english))
                    {
                        AddExact(entryId, english);
                    }
                }
            }
            catch (Exception)
            {
                // Format-only keys still covered by keyed lookup at runtime.
            }
        }

        private static void EnsureBuckets(string id)
        {
            if (!PatternsById.ContainsKey(id))
            {
                PatternsById[id] = new List<Regex>();
            }

            if (!ExactById.ContainsKey(id))
            {
                ExactById[id] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void AddExact(string id, string text)
        {
            EnsureBuckets(id);
            string normalized = KeyedSuppressMatcher.Normalize(text);
            if (!string.IsNullOrEmpty(normalized))
            {
                ExactById[id].Add(normalized);
            }
        }

        private static void AddPattern(string id, Regex regex)
        {
            if (regex == null)
            {
                return;
            }

            EnsureBuckets(id);
            PatternsById[id].Add(regex);
        }
    }
}
