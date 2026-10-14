using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Shared RimWorld keyed-template matching: strip rich text, turn {0} / gender branches into regex.
    /// A placeholder may contain nested braces ({lookup: {FOOD_labelShort}; Case; 4}); the whole
    /// brace group is one wildcard. NBSP and em dash are folded so the template matches the live line.
    ///
    /// Общее сопоставление keyed-шаблонов RimWorld: снятие rich text, regex из {0} и gender-веток.
    /// Плейсхолдер может содержать вложенные скобки ({lookup: {FOOD_labelShort}; Case; 4}); вся
    /// группа скобок - один wildcard. NBSP и длинное тире схлопываются, чтобы шаблон совпал с живой строкой.
    /// </summary>
    public static class KeyedSuppressMatcher
    {
        private static readonly Regex RichTextPattern = new Regex(
            @"<[^>]+>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex GenderBranchPattern = new Regex(
            @"\{[A-Za-z0-9_]+_gender\s*\?\s*([^:}]+)\s*:\s*([^:}]+)(?:\s*:\s*([^}]+))?\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            string stripped = RichTextPattern.Replace(text, string.Empty);
            var sb = new StringBuilder(stripped.Length);
            for (int i = 0; i < stripped.Length; i++)
            {
                char c = stripped[i];
                if (c == '\u00A0' || c == '\u202F' || c == '\u2009')
                {
                    sb.Append(' ');
                }
                else if (c == '\u2014' || c == '\u2013' || c == '\u2212')
                {
                    sb.Append('-');
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Trim();
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

                if (normalized[i] == '{')
                {
                    int end = BalancedClose(normalized, i);
                    if (end > i)
                    {
                        sb.Append(".+?");
                        i = end + 1;
                        continue;
                    }
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

        private static int BalancedClose(string text, int open)
        {
            int depth = 0;
            for (int i = open; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '{')
                {
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }
}
