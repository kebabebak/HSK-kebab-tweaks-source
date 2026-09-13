#if RIMWORLD_1_6
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Stratum solar roof tiles feed PowerNet.CurrentEnergyGainRate from
    /// SolarRoofMapComponent, so batteries charge, but Energy Tab only sums colonist
    /// CompPowerTrader buildings. Roof tiles never appear in generation.
    ///
    /// Fix: soft Postfix on Energy Tab GetTraderDefs / GetCurrentTrade plus Tick and
    /// History.UpdateThingCountAndMax. Adds one generation row using the kebab marker
    /// ThingDef and watts from GetAdditionalPowerFor. Does not spawn a fake generator
    /// or change Stratum PowerNet injection. Rooftop solar plants already have
    /// CompPowerPlant. Soft skip if Energy Tab or Stratum is missing.
    ///
    /// Проблема: плитки солнечной крыши Stratum кормят PowerNet.CurrentEnergyGainRate из
    /// SolarRoofMapComponent, батареи заряжаются, но Energy Tab суммирует только здания
    /// колонии с CompPowerTrader. Плитки в выработке не появляются.
    ///
    /// Исправление: soft Postfix GetTraderDefs / GetCurrentTrade плюс Tick и
    /// History.UpdateThingCountAndMax. Одна строка легенды на маркерном ThingDef kebab
    /// и ватты из GetAdditionalPowerFor. Фейковый генератор не спавнится, впрыск Stratum
    /// в PowerNet не меняется. Крышные панели-здания с CompPowerPlant уже есть. Skip,
    /// если нет Energy Tab или Stratum.
    /// </summary>
    public static class StratumSolarRoofEnergyTabFixFeatures
    {
        internal const string MarkerDefName = "KebabTweaks_StratumSolarRoof";

        private static Type solarRoofMapComponentType;
        private static Type historyType;
        private static Type chapterType;
        private static FieldInfo traderDefsField;
        private static FieldInfo tradersField;
        private static FieldInfo managerField;
        private static FieldInfo tradingHistoryField;
        private static FieldInfo chaptersField;
        private static FieldInfo chapterThingDefCountField;
        private static FieldInfo solarCellsField;
        private static MethodInfo getAdditionalPowerForMethod;
        private static MethodInfo addShownChapterMethod;
        private static ConstructorInfo chapterCtor;
        private static Map lastTickMap;

        public static void Apply(Harmony harmony)
        {
            try
            {
                Type tabType = AccessTools.TypeByName("EnergyTab.ManagerTab_Power");
                historyType = AccessTools.TypeByName("EnergyTab.History");
                solarRoofMapComponentType =
                    AccessTools.TypeByName("SolarWeb.Stratum.MapComponents.SolarRoofMapComponent");
                if (tabType == null || historyType == null)
                {
                    Log.Message(
                        "[StratumSolarRoofEnergyTabFixPatch] Energy Tab not loaded; patch skipped.");
                    return;
                }

                if (solarRoofMapComponentType == null)
                {
                    Log.Message(
                        "[StratumSolarRoofEnergyTabFixPatch] Stratum solar roof component not loaded; patch skipped.");
                    return;
                }

                traderDefsField = AccessTools.Field(tabType, "_traderDefs");
                tradersField = AccessTools.Field(tabType, "_traders");
                managerField = AccessTools.Field(tabType, "manager")
                    ?? AccessTools.Field(tabType.BaseType, "manager");
                tradingHistoryField = AccessTools.Field(tabType, "tradingHistory");
                chaptersField = AccessTools.Field(historyType, "_chapters");
                chapterType = AccessTools.Inner(historyType, "Chapter");
                if (chapterType != null)
                {
                    chapterThingDefCountField = AccessTools.Field(chapterType, "ThingDefCount");
                    chapterCtor = AccessTools.Constructor(
                        chapterType,
                        new[] { typeof(ThingDefCountClass), typeof(int), typeof(Color) });
                    addShownChapterMethod = AccessTools.Method(
                        historyType,
                        "addShownChapter",
                        new[] { chapterType });
                }

                solarCellsField = AccessTools.Field(solarRoofMapComponentType, "solarRoofCells");
                getAdditionalPowerForMethod = AccessTools.Method(
                    solarRoofMapComponentType,
                    "GetAdditionalPowerFor",
                    new[] { typeof(PowerNet) });

                MethodBase getTraderDefs = AccessTools.Method(tabType, "GetTraderDefs");
                MethodBase getCurrentTrade = AccessTools.Method(tabType, "GetCurrentTrade");
                MethodBase tick = AccessTools.Method(tabType, "Tick");
                MethodBase updateCounts = AccessTools.Method(
                    historyType,
                    "UpdateThingCountAndMax",
                    new[] { typeof(int[]), typeof(int[]) });
                if (getTraderDefs == null || getCurrentTrade == null || tick == null || updateCounts == null
                    || traderDefsField == null || tradingHistoryField == null || chaptersField == null
                    || getAdditionalPowerForMethod == null)
                {
                    Log.Message(
                        "[StratumSolarRoofEnergyTabFixPatch] Energy Tab power members missing; patch skipped.");
                    return;
                }

                Type hooks = typeof(StratumSolarRoofEnergyTabFixHooks);
                harmony.Patch(
                    getTraderDefs,
                    postfix: new HarmonyMethod(hooks, nameof(StratumSolarRoofEnergyTabFixHooks.GetTraderDefs_Postfix)));
                harmony.Patch(
                    tick,
                    prefix: new HarmonyMethod(hooks, nameof(StratumSolarRoofEnergyTabFixHooks.Tick_Prefix)));
                harmony.Patch(
                    getCurrentTrade,
                    postfix: new HarmonyMethod(hooks, nameof(StratumSolarRoofEnergyTabFixHooks.GetCurrentTrade_Postfix)));
                harmony.Patch(
                    updateCounts,
                    prefix: new HarmonyMethod(
                        hooks,
                        nameof(StratumSolarRoofEnergyTabFixHooks.UpdateThingCountAndMax_Prefix)));

                Log.Message("[StratumSolarRoofEnergyTabFixPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[StratumSolarRoofEnergyTabFixPatch] Failed to apply patches: " + e);
            }
        }

        internal static ThingDef MarkerDef()
        {
            return DefDatabase<ThingDef>.GetNamedSilentFail(MarkerDefName);
        }

        /// <summary>
        /// Keeps the Energy Tab trader list and history chapters aligned with the solar roof
        /// marker so a loaded save whose History was scribed without that chapter does not
        /// mismatch Update array lengths.
        ///
        /// Держит список трейдеров Energy Tab и главы истории на маркере солнечной крыши,
        /// чтобы сейв без этой главы не расходился по длине массива Update.
        /// </summary>
        internal static void EnsureSolarRoofChapter(object tab)
        {
            try
            {
                ThingDef marker = MarkerDef();
                if (tab == null || marker == null || traderDefsField == null)
                {
                    return;
                }

                IList defs = traderDefsField.GetValue(tab) as IList;
                if (defs == null)
                {
                    return;
                }

                if (IndexOfDef(defs, marker) < 0)
                {
                    defs.Add(marker);
                }

                IList traders = tradersField != null ? tradersField.GetValue(tab) as IList : null;
                if (traders != null)
                {
                    while (traders.Count < defs.Count)
                    {
                        traders.Add(new List<CompPowerTrader>());
                    }
                }

                object history = tradingHistoryField.GetValue(tab);
                if (history == null || chaptersField == null || chapterCtor == null
                    || chapterThingDefCountField == null)
                {
                    return;
                }

                if (FindChapterIndex(history, marker) >= 0)
                {
                    return;
                }

                object chapter = chapterCtor.Invoke(
                    new object[]
                    {
                        new ThingDefCountClass(marker, 0),
                        100,
                        new Color(0.92f, 0.72f, 0.12f)
                    });
                IList chapters = chaptersField.GetValue(history) as IList;
                chapters?.Add(chapter);
                if (addShownChapterMethod != null && chapter != null)
                {
                    addShownChapterMethod.Invoke(history, new[] { chapter });
                }
            }
            catch (Exception e)
            {
                Log.ErrorOnce(
                    "[StratumSolarRoofEnergyTabFixPatch] Failed to ensure solar roof chapter: " + e,
                    "StratumSolarRoofEnergyTabFix_Ensure".GetHashCode());
            }
        }

        internal static int FindChapterIndex(object history, ThingDef marker)
        {
            if (history == null || marker == null || chaptersField == null || chapterThingDefCountField == null)
            {
                return -1;
            }

            IList chapters = chaptersField.GetValue(history) as IList;
            if (chapters == null)
            {
                return -1;
            }

            for (int i = 0; i < chapters.Count; i++)
            {
                ThingDefCountClass count = chapterThingDefCountField.GetValue(chapters[i]) as ThingDefCountClass;
                if (count != null && count.thingDef == marker)
                {
                    return i;
                }
            }

            return -1;
        }

        internal static int MarkerTraderIndex(object tab)
        {
            ThingDef marker = MarkerDef();
            IList defs = traderDefsField != null ? traderDefsField.GetValue(tab) as IList : null;
            if (marker == null || defs == null)
            {
                return -1;
            }

            return IndexOfDef(defs, marker);
        }

        internal static Map MapOf(object tab)
        {
            if (tab == null || managerField == null)
            {
                return null;
            }

            return (managerField.GetValue(tab) as MapComponent)?.map;
        }

        internal static void RememberTickMap(object tab)
        {
            lastTickMap = MapOf(tab);
        }

        internal static int CurrentSolarWatts(Map map)
        {
            object comp = SolarComp(map);
            if (comp == null || getAdditionalPowerForMethod == null || map?.powerNetManager == null)
            {
                return 0;
            }

            try
            {
                float sum = 0f;
                List<PowerNet> nets = map.powerNetManager.AllNetsListForReading;
                if (nets == null)
                {
                    return 0;
                }

                for (int i = 0; i < nets.Count; i++)
                {
                    object boxed = getAdditionalPowerForMethod.Invoke(comp, new object[] { nets[i] });
                    if (boxed is float watts)
                    {
                        sum += watts;
                    }
                }

                return (int)sum;
            }
            catch (Exception e)
            {
                Log.ErrorOnce(
                    "[StratumSolarRoofEnergyTabFixPatch] Failed to read solar roof watts: " + e,
                    "StratumSolarRoofEnergyTabFix_Watts".GetHashCode());
                return 0;
            }
        }

        internal static int CurrentSolarTileCount()
        {
            object comp = SolarComp(lastTickMap);
            if (comp == null || solarCellsField == null)
            {
                return 0;
            }

            ICollection cells = solarCellsField.GetValue(comp) as ICollection;
            return cells != null ? cells.Count : 0;
        }

        private static int IndexOfDef(IList defs, ThingDef marker)
        {
            for (int i = 0; i < defs.Count; i++)
            {
                if (defs[i] as ThingDef == marker)
                {
                    return i;
                }
            }

            return -1;
        }

        private static object SolarComp(Map map)
        {
            if (map == null || solarRoofMapComponentType == null)
            {
                return null;
            }

            return map.GetComponent(solarRoofMapComponentType);
        }
    }

    /// <summary>
    /// Energy Tab hooks that inject Stratum solar roof watts into the generation legend.
    ///
    /// Хуки Energy Tab: ватты солнечной крыши Stratum в легенде выработки.
    /// </summary>
    public static class StratumSolarRoofEnergyTabFixHooks
    {
        public static void GetTraderDefs_Postfix(ref IEnumerable<ThingDef> __result)
        {
            ThingDef marker = StratumSolarRoofEnergyTabFixFeatures.MarkerDef();
            if (marker == null || __result == null)
            {
                return;
            }

            List<ThingDef> list = __result as List<ThingDef> ?? __result.ToList();
            if (!list.Contains(marker))
            {
                list.Add(marker);
            }

            __result = list;
        }

        public static void Tick_Prefix(object __instance)
        {
            StratumSolarRoofEnergyTabFixFeatures.RememberTickMap(__instance);
            StratumSolarRoofEnergyTabFixFeatures.EnsureSolarRoofChapter(__instance);
        }

        public static void GetCurrentTrade_Postfix(object __instance, ref int[] __result)
        {
            if (!KebabTweaksSettings.EnableStratumSolarRoofEnergyTabFix || __result == null)
            {
                return;
            }

            StratumSolarRoofEnergyTabFixFeatures.EnsureSolarRoofChapter(__instance);
            int idx = StratumSolarRoofEnergyTabFixFeatures.MarkerTraderIndex(__instance);
            if (idx < 0)
            {
                return;
            }

            Map map = StratumSolarRoofEnergyTabFixFeatures.MapOf(__instance);
            int watts = StratumSolarRoofEnergyTabFixFeatures.CurrentSolarWatts(map);
            if (idx < __result.Length)
            {
                __result[idx] = watts;
                return;
            }

            int[] next = new int[idx + 1];
            Array.Copy(__result, next, __result.Length);
            next[idx] = watts;
            __result = next;
        }

        public static void UpdateThingCountAndMax_Prefix(object __instance, int[] counts)
        {
            if (!KebabTweaksSettings.EnableStratumSolarRoofEnergyTabFix || counts == null)
            {
                return;
            }

            ThingDef marker = StratumSolarRoofEnergyTabFixFeatures.MarkerDef();
            int idx = StratumSolarRoofEnergyTabFixFeatures.FindChapterIndex(__instance, marker);
            if (idx < 0 || idx >= counts.Length)
            {
                return;
            }

            counts[idx] = StratumSolarRoofEnergyTabFixFeatures.CurrentSolarTileCount();
        }
    }
}
#endif
