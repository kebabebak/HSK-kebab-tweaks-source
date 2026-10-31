using System.Text.RegularExpressions;
using HarmonyLib;
using Verse;
using Verse.Grammar;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: RimWorld-ru Odyssey rulesStrings for NamerUniqueWeapon and NamerGravship
    /// put a gender or case suffix after the weight, as in weapon_noun(p=2)_mas->.
    /// Rule_String then stores a null keyword, GrammarResolver.AddRule throws
    /// ArgumentNullException, and the name falls back to English. One gravship line
    /// has no keyword at all, and one weapon adjective line glues two rules together.
    /// Language DefInjected in this mod should replace those lists; this Prefix is a
    /// load-order safety net.
    ///
    /// Fix: Prefix on Rule_String(string) moves mas/fem/neu/gen in front of (p=N),
    /// splits the glued adjective back to weapon_adjective_mas(p=2), and gives the
    /// bare AdjectiveLarge_fem line the narrative_name keyword.
    ///
    /// Проблема: русский Odyssey для NamerUniqueWeapon и NamerGravship ставит род или
    /// падеж после веса, например weapon_noun(p=2)_mas->. Rule_String оставляет
    /// пустой ключ, GrammarResolver.AddRule кидает ArgumentNullException, имя берётся
    /// по-английски. У гравикорабля одна строка без ключа, у оружия две строки
    /// склеены в одну. DefInjected в моде должен заменить списки; этот Prefix -
    /// страховка по load-order.
    ///
    /// Исправление: Prefix на Rule_String(string) переносит mas/fem/neu/gen перед
    /// (p=N), оставляет от склейки weapon_adjective_mas(p=2) и ставит narrative_name
    /// на строку AdjectiveLarge_fem.
    /// </summary>
    public static class OdysseyRuRuleKeywordFixFeatures
    {
        private static readonly Regex SuffixAfterWeight = new Regex(
            @"^([A-Za-z0-9_/]+)\(([^)]+)\)_(mas|fem|neu|gen)->(.*)$",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static bool loggedRewrite;

        public static void Apply(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    AccessTools.Constructor(typeof(Rule_String), new[] { typeof(string) }),
                    prefix: new HarmonyMethod(
                        typeof(Rule_String_Ctor_OdysseyRuKeyword_Patch),
                        nameof(Rule_String_Ctor_OdysseyRuKeyword_Patch.Prefix)));

                Log.Message(
                    "[OdysseyRuRuleKeywordFix] Rule_String keyword safety Prefix applied (DefInjected XML also ships).");
            }
            catch (System.Exception ex)
            {
                Log.Error("[OdysseyRuRuleKeywordFix] Failed to apply patches: " + ex);
            }
        }

        internal static void MaybeRewriteKeyword(ref string rawString)
        {
            if (rawString.NullOrEmpty())
            {
                return;
            }

            string rewritten = null;
            if (rawString == "weapon_adjective(p=2)->[trait_adjective]weapon_adjective(p=2)_mas->[trait_adjective_mas]")
            {
                rewritten = "weapon_adjective_mas(p=2)->[trait_adjective_mas]";
            }
            else if (rawString == "[AdjectiveLarge_fem] [thing_badass_fem]")
            {
                rewritten = "narrative_name->[AdjectiveLarge_fem] [thing_badass_fem]";
            }
            else
            {
                Match match = SuffixAfterWeight.Match(rawString);
                if (match.Success)
                {
                    rewritten = match.Groups[1].Value + "_" + match.Groups[3].Value
                        + "(" + match.Groups[2].Value + ")->" + match.Groups[4].Value;
                }
            }

            if (rewritten == null)
            {
                return;
            }

            rawString = rewritten;
            if (!loggedRewrite)
            {
                loggedRewrite = true;
                Log.Message(
                    "[OdysseyRuRuleKeywordFix] Rewrote RimWorld-ru Odyssey rule with a null keyword.");
            }
        }
    }

    /// <summary>
    /// Rewrites known Odyssey Russian rule lines that leave Rule_String.keyword null.
    /// Parameter name must be rawString (RW 1.6 Rule_String ctor).
    ///
    /// Переписывает известные строки русского Odyssey, из-за которых у Rule_String
    /// пустой ключ. Имя параметра - rawString (ctor RW 1.6).
    /// </summary>
    internal static class Rule_String_Ctor_OdysseyRuKeyword_Patch
    {
        public static void Prefix(ref string rawString)
        {
            OdysseyRuRuleKeywordFixFeatures.MaybeRewriteKeyword(ref rawString);
        }
    }
}
