using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// One work requirement inside Alert_ColonistNeedsSurvivalTool. Checking it means that StatDef
    /// does not count. The alert stays when another requirement is still missing.
    ///
    /// Одно требование к работе внутри Alert_ColonistNeedsSurvivalTool. Отмеченный StatDef не
    /// считается. Оповещение остаётся, если не хватает другого требования.
    /// </summary>
    public sealed class ToolNeedEntry
    {
        public string StatDefName { get; }
        public string EnglishLabel { get; }
        public string RussianLabel { get; }

        public ToolNeedEntry(string statDefName, string englishLabel, string russianLabel)
        {
            StatDefName = statDefName;
            EnglishLabel = englishLabel;
            RussianLabel = russianLabel;
        }

        public string Id => ToolNeedIgnore.IdPrefix + StatDefName;

        public string Quote
        {
            get
            {
                string folder = LanguageDatabase.activeLanguage?.folderName;
                if (!folder.NullOrEmpty()
                    && folder.StartsWith("Russian", StringComparison.OrdinalIgnoreCase))
                {
                    return RussianLabel;
                }

                return EnglishLabel;
            }
        }

        public string CheckboxLabel =>
            "KebabSwitches.IgnoreAlert".Translate(Quote.Named("QUOTE"));

        public bool IsSuppressEnabled => KebabTweaksSettings.IsAlertSuppressed(Id);

        public void SetSuppressEnabled(bool value) => KebabTweaksSettings.SetAlertSuppressed(Id, value);
    }

    /// <summary>
    /// Stats that SurvivalTools work givers require and that a missing tool can raise the alert for.
    ///
    /// Stat, которые work giver SurvivalTools требуют и из-за которых оповещение может загореться.
    /// </summary>
    public static class ToolNeedIgnore
    {
        public const string RetiredWholeAlertId = "ColonistNeedsSurvivalTool";
        public const string IdPrefix = "ToolNeed.";

        public static readonly ToolNeedEntry[] All =
        {
            new ToolNeedEntry("MiningSpeed", "Colonists lack tools: mining speed", "Поселенцы нуждаются в инструменте: скорость горных работ"),
            new ToolNeedEntry("MiningYieldDigging", "Colonists lack tools: mining yield (digging)", "Поселенцы нуждаются в инструменте: добыча полезных ископаемых"),
            new ToolNeedEntry("ConstructionSpeed", "Colonists lack tools: construction speed", "Поселенцы нуждаются в инструменте: скорость строительства"),
            new ToolNeedEntry("PlantWorkSpeed", "Colonists lack tools: plant sowing speed", "Поселенцы нуждаются в инструменте: скорость посадки растений"),
            new ToolNeedEntry("PlantHarvestingSpeed", "Colonists lack tools: plant harvesting speed", "Поселенцы нуждаются в инструменте: скорость сбора растений"),
            new ToolNeedEntry("TreeFellingSpeed", "Colonists lack tools: tree felling speed", "Поселенцы нуждаются в инструменте: скорость рубки деревьев"),
        };

        public static bool IsSuppressed(string statDefName)
        {
            if (statDefName.NullOrEmpty())
            {
                return false;
            }

            return KebabTweaksSettings.IsAlertSuppressed(IdPrefix + statDefName);
        }

        public static bool AnySuppressed()
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].IsSuppressEnabled)
                {
                    return true;
                }
            }

            return false;
        }

        public static int SuppressedCount()
        {
            int count = 0;
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].IsSuppressEnabled)
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>
    /// Alert_ColonistNeedsSurvivalTool stays one alert and is not force-removed. WorkingToolless is
    /// true when any assigned tool stat is missing. A checked requirement is skipped, so the alert
    /// remains while any unchecked requirement is still missing. The explanation omits ignored work.
    ///
    /// Alert_ColonistNeedsSurvivalTool остаётся одним оповещением и не снимается целиком.
    /// WorkingToolless истинен, если не хватает любого назначенного StatDef. Отмеченное требование
    /// пропускается, поэтому оповещение остаётся, пока не хватает любого неотмеченного.
    /// Пояснение не пишет проигнорированные работы.
    /// </summary>
    public static class SurvivalToolNeedPatch
    {
        private static bool resolved;
        private static bool available;
        private static Type alertType;
        private static PropertyInfo toollessWorkers;
        private static MethodInfo statList;
        private static MethodInfo hasTool;
        private static MethodInfo giverList;
        private static MethodInfo getExtension;
        private static FieldInfo requiredStats;

        public static void Apply(Harmony harmony)
        {
            alertType = AccessTools.TypeByName("SurvivalToolsLite.Alert_ColonistNeedsSurvivalTool");
            if (alertType == null)
            {
                return;
            }

            MethodInfo working = AccessTools.Method(alertType, "WorkingToolless");
            MethodInfo explanation = AccessTools.Method(alertType, "GetExplanation");
            if (working != null)
            {
                harmony.Patch(working, postfix: new HarmonyMethod(typeof(SurvivalToolNeedPatch), nameof(WorkingToolless_Postfix)));
            }

            if (explanation != null)
            {
                harmony.Patch(explanation, postfix: new HarmonyMethod(typeof(SurvivalToolNeedPatch), nameof(GetExplanation_Postfix)));
            }
        }

        public static void WorkingToolless_Postfix(Pawn pawn, ref bool __result)
        {
            if (!__result || pawn == null || !ToolNeedIgnore.AnySuppressed() || !EnsureApi())
            {
                return;
            }

            __result = HasUnignoredMissingStat(pawn);
        }

        public static void GetExplanation_Postfix(object __instance, ref TaggedString __result)
        {
            if (__instance == null || !ToolNeedIgnore.AnySuppressed() || !EnsureApi())
            {
                return;
            }

            try
            {
                __result = BuildExplanation(__instance);
            }
            catch (Exception)
            {
                // Original explanation stays if the tool API moved.
            }
        }

        private static bool HasUnignoredMissingStat(Pawn pawn)
        {
            object stats = statList.Invoke(null, new object[] { pawn });
            if (!(stats is IEnumerable list))
            {
                return true;
            }

            foreach (object statObj in list)
            {
                StatDef stat = statObj as StatDef;
                if (stat == null || ToolNeedIgnore.IsSuppressed(stat.defName))
                {
                    continue;
                }

                if (!(bool)hasTool.Invoke(null, new object[] { pawn, stat }))
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildExplanation(object alert)
        {
            var text = new StringBuilder();
            text.Append("ColonistNeedsSurvivalToolDesc".Translate());
            text.Append(":\n");
            object workers = toollessWorkers.GetValue(alert);
            if (workers is IEnumerable pawns)
            {
                foreach (object pawnObj in pawns)
                {
                    Pawn pawn = pawnObj as Pawn;
                    if (pawn == null)
                    {
                        continue;
                    }

                    string works = WorkLabels(pawn);
                    if (works.NullOrEmpty())
                    {
                        continue;
                    }

                    text.Append("\n    ");
                    text.Append(pawn.LabelShort);
                    text.Append(" (");
                    text.Append(works);
                    text.Append(')');
                }
            }

            return text.ToString();
        }

        private static string WorkLabels(Pawn pawn)
        {
            object givers = giverList.Invoke(null, new object[] { pawn });
            if (!(givers is IEnumerable giverEnum))
            {
                return string.Empty;
            }

            var types = new List<string>();
            foreach (object giverObj in giverEnum)
            {
                WorkGiver giver = giverObj as WorkGiver;
                if (giver?.def == null)
                {
                    continue;
                }

                object extension = getExtension.Invoke(giver.def, null);
                if (extension == null)
                {
                    continue;
                }

                object stats = requiredStats.GetValue(extension);
                if (!(stats is IEnumerable statEnum))
                {
                    continue;
                }

                string gerund = giver.def.workType?.gerundLabel;
                if (gerund.NullOrEmpty())
                {
                    continue;
                }

                foreach (object statObj in statEnum)
                {
                    StatDef stat = statObj as StatDef;
                    if (stat == null || ToolNeedIgnore.IsSuppressed(stat.defName))
                    {
                        continue;
                    }

                    if ((bool)hasTool.Invoke(null, new object[] { pawn, stat }))
                    {
                        continue;
                    }

                    if (!types.Contains(gerund))
                    {
                        types.Add(gerund);
                    }

                    break;
                }
            }

            if (types.Count == 0)
            {
                return string.Empty;
            }

            return types.ToCommaList().CapitalizeFirst();
        }

        private static bool EnsureApi()
        {
            if (resolved)
            {
                return available;
            }

            resolved = true;
            Type util = AccessTools.TypeByName("SurvivalToolsLite.SurvivalToolUtility");
            Type extensionType = AccessTools.TypeByName("SurvivalToolsLite.WorkGiverExtension");
            if (util == null || extensionType == null || alertType == null)
            {
                return false;
            }

            statList = AccessTools.Method(util, "AssignedToolRelevantWorkGiversStatDefs");
            hasTool = AccessTools.Method(util, "HasSurvivalToolFor", new[] { typeof(Pawn), typeof(StatDef) });
            giverList = AccessTools.Method(util, "AssignedToolRelevantWorkGivers");
            requiredStats = AccessTools.Field(extensionType, "requiredStats");
            toollessWorkers = AccessTools.Property(alertType, "ToollessWorkers");
            MethodInfo open = AccessTools.Method(typeof(Def), "GetModExtension");
            if (statList == null || hasTool == null || giverList == null
                || requiredStats == null || toollessWorkers == null || open == null)
            {
                return false;
            }

            getExtension = open.MakeGenericMethod(extensionType);
            available = getExtension != null;
            return available;
        }
    }
}
