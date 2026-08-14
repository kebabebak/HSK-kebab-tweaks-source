#if RIMWORLD_1_6
using System;
using HarmonyLib;
using Verse;
using Verse.Grammar;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: RimWorld-ru Odyssey DefInjected for NamerFactionTradersGuild can leave a
    /// rulesStrings line as [tradeAdj_fem] [tradeNoun_fem] without r_name->, so
    /// Rule_String stores a null keyword → Bad string pass / GrammarResolver ArgumentNullException
    /// during Traders Guild faction naming. Language DefInjected in this mod should replace the list;
    /// this Prefix is a load-order safety net.
    ///
    /// Fix: Prefix on Rule_String(string) prepends r_name-> when the raw line is that
    /// Odyssey typo (no ->, contains [tradeAdj_fem]).
    ///
    /// Проблема: RimWorld-ru Odyssey DefInjected для NamerFactionTradersGuild может оставить
    /// строку [tradeAdj_fem] [tradeNoun_fem] без r_name-> → null keyword → Bad string
    /// pass / ArgumentNullException при именовании Traders Guild. DefInjected в моде должен
    /// заменить список; этот Prefix — страховка по load order.
    ///
    /// Исправление: Prefix на Rule_String(string) добавляет r_name-> для этой опечатки.
    /// </summary>
    public static class OdysseyRuTradersGuildNamerFixFeatures
    {
        private static bool loggedRewrite;

        public static void Apply(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    AccessTools.Constructor(typeof(Rule_String), new[] { typeof(string) }),
                    prefix: new HarmonyMethod(
                        typeof(Rule_String_Ctor_OdysseyRuTypo_Patch),
                        nameof(Rule_String_Ctor_OdysseyRuTypo_Patch.Prefix)));

                Log.Message(
                    "[OdysseyRuTradersGuildNamerFix] Rule_String typo safety Prefix applied (DefInjected XML also ships).");
            }
            catch (Exception ex)
            {
                Log.Error("[OdysseyRuTradersGuildNamerFix] Failed to apply patches: " + ex);
            }
        }

        internal static void MaybeRewriteTypo(ref string rawString)
        {
            if (rawString.NullOrEmpty() || rawString.Contains("->"))
            {
                return;
            }

            if (rawString.IndexOf("[tradeAdj_fem]", StringComparison.Ordinal) < 0)
            {
                return;
            }

            rawString = "r_name->" + rawString;
            if (!loggedRewrite)
            {
                loggedRewrite = true;
                Log.Message(
                    "[OdysseyRuTradersGuildNamerFix] Rewrote RimWorld-ru Odyssey namer line missing r_name->.");
            }
        }
    }

    /// <summary>
    /// Prepends r_name-> to the known broken Odyssey Russian Traders Guild rule line.
    /// Parameter name must be rawString (RW 1.6 Rule_String ctor).
    ///
    /// Добавляет r_name-> к известной битой строке русского Odyssey Traders Guild.
    /// Имя параметра — rawString (ctor RW 1.6).
    /// </summary>
    internal static class Rule_String_Ctor_OdysseyRuTypo_Patch
    {
        public static void Prefix(ref string rawString)
        {
            OdysseyRuTradersGuildNamerFixFeatures.MaybeRewriteTypo(ref rawString);
        }
    }
}
#endif
