using System.Text.RegularExpressions;
using Verse;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Strips leftover lookup/case grammar from hand-written letter quote keys.
    ///
    /// Убирает остатки lookup/case-грамматики из вручную написанных ключей цитат писем.
    /// </summary>
    public static class LetterLabelPreview
    {
        private static readonly Regex BrokenLookupTailPattern = new Regex(
            @";\s*Case;\s*\d+\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Sanitizes a settings quote string (legacy grammar fragments in LetterQuote.*).
        ///
        /// Очищает строку цитаты для настроек (устаревшие фрагменты грамматики в LetterQuote.*).
        /// </summary>
        public static string SanitizeQuote(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            text = BrokenLookupTailPattern.Replace(text, string.Empty);
            return text.Trim();
        }
    }
}
