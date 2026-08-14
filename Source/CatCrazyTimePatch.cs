using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using SK;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: JobDriver_CrazyTime.MakeNewToils always rolls num = Rand.RangeInclusive(3, 8)
    /// even after ExposeData restored num from a save. On load SetupToils rebuilds toils with a
    /// new count while curToilIndex still points at the saved progress →
    /// "tried to get CurToil with curToilIndex=N but only has N toils".
    ///
    /// Fix: Postfix MakeNewToils — on resume (num &gt; 0) replace __result before enumeration so
    /// saved num is kept; fresh jobs (num &lt;= 0) leave the vanilla iterator to roll num.
    /// Postfix SetupToils clamps curToilIndex for already-corrupted saves.
    ///
    /// Проблема: JobDriver_CrazyTime.MakeNewToils всегда бросает num = Rand.RangeInclusive(3, 8)
    /// даже после того, как ExposeData восстановил num из сейва. При load SetupToils пересобирает
    /// toils с новым числом, а curToilIndex всё ещё указывает на сохранённый прогресс →
    /// "tried to get CurToil with curToilIndex=N but only has N toils".
    ///
    /// Исправление: Postfix MakeNewToils — при resume (num &gt; 0) подменить __result до
    /// перечисления, сохранив num из сейва; новые job (num &lt;= 0) оставляют vanilla iterator.
    /// Postfix SetupToils ограничивает curToilIndex для уже битых сейвов.
    /// </summary>
    public static class CatCrazyTimeFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                MethodBase makeNewToils = AccessTools.Method(typeof(JobDriver_CrazyTime), "MakeNewToils");
                if (makeNewToils == null)
                {
                    Log.Warning("[CatCrazyTimePatch] SK.JobDriver_CrazyTime.MakeNewToils not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    makeNewToils,
                    postfix: new HarmonyMethod(
                        typeof(JobDriver_CrazyTime_MakeNewToils_Patch),
                        nameof(JobDriver_CrazyTime_MakeNewToils_Patch.Postfix)));

                MethodBase setupToils = AccessTools.Method(typeof(JobDriver), "SetupToils");
                if (setupToils != null)
                {
                    harmony.Patch(
                        setupToils,
                        postfix: new HarmonyMethod(
                            typeof(JobDriver_SetupToils_CrazyTimeClamp_Patch),
                            nameof(JobDriver_SetupToils_CrazyTimeClamp_Patch.Postfix)));
                }

                Log.Message(
                    $"[CatCrazyTimePatch] Loaded (verbose logging {(KebabTweaksSettings.CatCrazyTimeEnableLogging ? "ON" : "OFF")}). " +
                    "Enable logging in mod settings for toil-count / index clamp details.");
            }
            catch (Exception ex)
            {
                Log.Error("[CatCrazyTimePatch] Failed to apply patches: " + ex);
            }
        }
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.Last)]
    internal static class JobDriver_CrazyTime_MakeNewToils_Patch
    {
        public static void Postfix(JobDriver_CrazyTime __instance, ref IEnumerable<Toil> __result)
        {
            if (!KebabTweaksSettings.EnableCatCrazyTime)
            {
                return;
            }

            if (__instance.num <= 0)
            {
                CatCrazyTimePatchLog.Message(
                    $"[CatCrazyTimePatch] {__instance.pawn?.LabelShort}: new CrazyTime job, using vanilla num roll.");
                return;
            }

            CatCrazyTimePatchLog.Message(
                $"[CatCrazyTimePatch] {__instance.pawn?.LabelShort}: resume CrazyTime, keeping num={__instance.num}.");
            __result = EnumerateToils(__instance);
        }

        private static IEnumerable<Toil> EnumerateToils(JobDriver_CrazyTime driver)
        {
            int count = driver.num;
            for (int i = 0; i < count; i++)
            {
                yield return driver.CrazyTime();
            }
        }
    }

    internal static class JobDriver_SetupToils_CrazyTimeClamp_Patch
    {
        private static readonly FieldInfo ToilsField =
            AccessTools.Field(typeof(JobDriver), "toils");

        private static readonly FieldInfo CurToilIndexField =
            AccessTools.Field(typeof(JobDriver), "curToilIndex");

        public static void Postfix(JobDriver __instance)
        {
            if (!KebabTweaksSettings.EnableCatCrazyTime)
            {
                return;
            }

            if (!(__instance is JobDriver_CrazyTime crazy))
            {
                return;
            }

            if (ToilsField == null || CurToilIndexField == null)
            {
                return;
            }

            var toils = ToilsField.GetValue(__instance) as List<Toil>;
            if (toils == null || toils.Count == 0)
            {
                return;
            }

            int index = (int)CurToilIndexField.GetValue(__instance);
            if (index < toils.Count)
            {
                return;
            }

            int clamped = toils.Count - 1;
            CurToilIndexField.SetValue(__instance, clamped);
            CatCrazyTimePatchLog.Message(
                $"[CatCrazyTimePatch] {crazy.pawn?.LabelShort}: clamped curToilIndex {index} → {clamped} " +
                $"(num={crazy.num}, toils={toils.Count}).");
        }
    }

    internal static class CatCrazyTimePatchLog
    {
        public static void Message(string text)
        {
            if (KebabTweaksSettings.CatCrazyTimeEnableLogging)
            {
                Log.Message(text);
            }
        }
    }
}
