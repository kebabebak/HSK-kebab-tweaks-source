using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Hospitality JobGiver_OptimizeApparel_Guest calls vanilla
    /// JobGiver_OptimizeApparel.TryGiveJob for guest pawns. Vanilla only allows colonists and
    /// logs ErrorOnce "Non-colonist {name} tried to optimize apparel." for everyone else.
    ///
    /// Fix: Harmony Prefix on TryGiveJob returns no job for non-colonists when enabled, skipping
    /// vanilla and the log. Colonists unchanged. No Hospitality assembly reference.
    ///
    /// Проблема: Hospitality JobGiver_OptimizeApparel_Guest вызывает vanilla
    /// JobGiver_OptimizeApparel.TryGiveJob для гостей. Vanilla разрешает только колонистам и
    /// пишет ErrorOnce «Non-colonist {name} tried to optimize apparel.» для остальных.
    ///
    /// Исправление: Harmony Prefix на TryGiveJob при включённом фиксе отдаёт no job для
    /// не-колонистов, не вызывая vanilla и лог. Колонисты без изменений. Без ссылки на Hospitality.
    /// </summary>
    public static class HospitalityGuestApparelOptimizeFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                harmony.CreateClassProcessor(typeof(JobGiver_OptimizeApparel_TryGiveJob_Patch)).Patch();
                Log.Message("[HospitalityGuestApparelOptimizeFixPatch] Patches applied.");
            }
            catch (System.Exception e)
            {
                Log.Error("[HospitalityGuestApparelOptimizeFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// Skips apparel optimize for non-colonists (guests) before vanilla ErrorOnce.
    ///
    /// Пропускает optimize apparel для не-колонистов (гостей) до vanilla ErrorOnce.
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
    public static class JobGiver_OptimizeApparel_TryGiveJob_Patch
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            if (!KebabTweaksSettings.EnableHospitalityGuestApparelOptimizeFix)
            {
                return true;
            }

            if (pawn.IsColonist)
            {
                return true;
            }

            __result = null;
            return false;
        }
    }
}
