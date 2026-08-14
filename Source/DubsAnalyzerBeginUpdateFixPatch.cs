using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Dubs Performance Analyzer ProfileController.BeginUpdate logs CRITICAL when
    /// midUpdate is still true — previous EndUpdate did not run (exception, nested Update, or
    /// opening the analyzer UI between H_RootUpdate Prefix and Postfix on Root_Play.Update).
    ///
    /// Fix: soft HarmonyBefore Prefix on BeginUpdate — if midUpdate is set, call EndUpdate() to
    /// close the stale cycle before Dubs logs CRITICAL. No hard ref on PerformanceAnalyzer.dll.
    ///
    /// Проблема: Dubs ProfileController.BeginUpdate пишет CRITICAL, если midUpdate ещё true —
    /// прошлый EndUpdate не вызван (исключение, вложенный Update или открытие UI Analyzer между
    /// Prefix и Postfix H_RootUpdate на Root_Play.Update).
    ///
    /// Исправление: soft HarmonyBefore Prefix на BeginUpdate — если midUpdate установлен,
    /// вызвать EndUpdate() и закрыть stale-цикл до CRITICAL в Dubs. Без hard ref на Analyzer.dll.
    /// </summary>
    public static class DubsAnalyzerBeginUpdateFixFeatures
    {
        private static Type profileControllerType;
        private static FieldInfo midUpdateField;
        private static MethodInfo endUpdateMethod;

        public static void Apply(Harmony harmony)
        {
            try
            {
                profileControllerType = AccessTools.TypeByName("Analyzer.Profiling.ProfileController");
                if (profileControllerType == null)
                {
                    Log.Message(
                        "[DubsAnalyzerBeginUpdateFixPatch] ProfileController not found; patch skipped.");
                    return;
                }

                MethodBase beginUpdate = AccessTools.Method(profileControllerType, "BeginUpdate");
                endUpdateMethod = AccessTools.Method(profileControllerType, "EndUpdate");
                midUpdateField = AccessTools.Field(profileControllerType, "midUpdate");
                if (beginUpdate == null || endUpdateMethod == null || midUpdateField == null)
                {
                    Log.Warning(
                        "[DubsAnalyzerBeginUpdateFixPatch] BeginUpdate/EndUpdate/midUpdate not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    beginUpdate,
                    prefix: new HarmonyMethod(
                        typeof(ProfileController_BeginUpdate_Patch),
                        nameof(ProfileController_BeginUpdate_Patch.Prefix))
                    {
                        priority = Priority.First
                    });

                Log.Message("[DubsAnalyzerBeginUpdateFixPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[DubsAnalyzerBeginUpdateFixPatch] Failed to apply patches: " + e);
            }
        }

        internal static bool IsMidUpdate()
        {
            return midUpdateField != null
                && midUpdateField.GetValue(null) is bool mid
                && mid;
        }

        internal static void InvokeEndUpdate()
        {
            if (endUpdateMethod != null)
            {
                endUpdateMethod.Invoke(null, null);
            }
        }
    }

    /// <summary>
    /// Closes a stale analyzer update cycle before Dubs BeginUpdate logs CRITICAL.
    ///
    /// Закрывает застрявший цикл Analyzer до CRITICAL в Dubs BeginUpdate.
    /// </summary>
    public static class ProfileController_BeginUpdate_Patch
    {
        public static void Prefix()
        {
            if (!KebabTweaksSettings.EnableDubsAnalyzerBeginUpdateFix)
            {
                return;
            }

            if (DubsAnalyzerBeginUpdateFixFeatures.IsMidUpdate())
            {
                DubsAnalyzerBeginUpdateFixFeatures.InvokeEndUpdate();
            }
        }
    }
}
