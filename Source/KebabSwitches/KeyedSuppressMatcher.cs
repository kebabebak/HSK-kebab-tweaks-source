using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Shared RimWorld keyed-template matching: strip rich text, turn {0} / gender branches into regex.
    ///
    /// Общее сопоставление keyed-шаблонов RimWorld: снятие rich text, regex из {0} и gender-веток.
    /// </summary>
    public static class KeyedSuppressMatcher
    {
        private static readonly Regex RichTextPattern = new Regex(
            @"<[^>]+>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex GenderBranchPattern = new Regex(
            @"\{[A-Za-z0-9_]+_gender\s*\?\s*([^:}]+)\s*:\s*([^:}]+)(?:\s*:\s*([^}]+))?\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex PlaceholderPattern = new Regex(
            @"\{[^}]+\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return RichTextPattern.Replace(text, string.Empty).Trim();
        }

        public static Regex TemplateToRegex(string template)
        {
            string normalized = Normalize(template);
            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            if (normalized.IndexOf('{') < 0)
            {
                return new Regex(
                    "^" + Regex.Escape(normalized) + "$",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            }

            var sb = new StringBuilder("^");
            int i = 0;
            while (i < normalized.Length)
            {
                Match gender = GenderBranchPattern.Match(normalized, i);
                if (gender.Success && gender.Index == i)
                {
                    var alts = new List<string>
                    {
                        Regex.Escape(gender.Groups[1].Value.Trim()),
                        Regex.Escape(gender.Groups[2].Value.Trim()),
                    };
                    if (gender.Groups[3].Success && !string.IsNullOrWhiteSpace(gender.Groups[3].Value))
                    {
                        alts.Add(Regex.Escape(gender.Groups[3].Value.Trim()));
                    }

                    sb.Append("(?:").Append(string.Join("|", alts)).Append(')');
                    i = gender.Index + gender.Length;
                    continue;
                }

                Match placeholder = PlaceholderPattern.Match(normalized, i);
                if (placeholder.Success && placeholder.Index == i)
                {
                    sb.Append(".+?");
                    i = placeholder.Index + placeholder.Length;
                    continue;
                }

                sb.Append(Regex.Escape(normalized[i].ToString()));
                i++;
            }

            sb.Append('$');
            try
            {
                return new Regex(
                    sb.ToString(),
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
