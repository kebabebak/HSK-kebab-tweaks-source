#if RIMWORLD_1_6
using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Enemy Self Preservation Postfix on Pawn.PostApplyDamage starts SelfPreservation
    /// for every Humanlike, including Anomaly mutants whose MutantDef preventsMentalBreaks
    /// (shamblers, ghouls, awoken corpses). Pain flee percent does not apply to them. The job
    /// still posts ESP.injured (injured and fleeing) and may Notify_PawnLost as Incapped, while
    /// the shambler think tree keeps fighting.
    ///
    /// Fix: soft Prefix on MUR_ESP.Pawn_PostApplyDamage.Postfix. When flee does not apply,
    /// skip only that ESP postfix (vanilla damage and other PostApplyDamage patches still run).
    /// Does not mute Messages.Message. Notifications Ignore for ESP.injured stays a separate
    /// global mute and still hides every matching toast, including real raider flees.
    ///
    /// Проблема: Enemy Self Preservation в Postfix Pawn.PostApplyDamage запускает
    /// SelfPreservation на любом Humanlike, в том числе на мутантах Anomaly с
    /// preventsMentalBreaks (шамблеры, гули, пробуждённые трупы). Порог боли на бегство на них
    /// не действует. Job всё равно пишет ESP.injured и может Notify_PawnLost как Incapped, а
    /// дерево шамблера продолжает бой.
    ///
    /// Исправление: soft Prefix на MUR_ESP.Pawn_PostApplyDamage.Postfix. Если бегство не
    /// применяется, пропускается только этот postfix ESP (ванильный урон и чужие патчи
    /// PostApplyDamage идут как обычно). Messages.Message не глушится. Ignore в Уведомлениях
    /// для ESP.injured остаётся отдельным глобальным mute и по-прежнему скрывает все такие
    /// тосты, включая настоящее бегство рейдеров.
    /// </summary>
    public static class AnomalyEspFleeFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                Type targetType = AccessTools.TypeByName("MUR_ESP.Pawn_PostApplyDamage");
                if (targetType == null)
                {
                    Log.Message(
                        "[AnomalyEspFleeFixPatch] MUR_ESP.Pawn_PostApplyDamage not found; patch skipped.");
                    return;
                }

                MethodBase target = AccessTools.Method(targetType, "Postfix");
                if (target == null)
                {
                    Log.Message(
                        "[AnomalyEspFleeFixPatch] MUR_ESP.Pawn_PostApplyDamage.Postfix not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(EspPawnPostApplyDamage_MutantSkip_Patch),
                        nameof(EspPawnPostApplyDamage_MutantSkip_Patch.Prefix)));

                Log.Message("[AnomalyEspFleeFixPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[AnomalyEspFleeFixPatch] Failed to apply patches: " + e);
            }
        }

        /// <summary>
        /// True when Anomaly mutant rules block panic-flee.
        ///
        /// True, если правила мутанта Anomaly запрещают panic-flee.
        /// </summary>
        internal static bool FleeMechanicDoesNotApply(Pawn pawn)
        {
            if (pawn == null || !ModsConfig.AnomalyActive)
            {
                return false;
            }

            if (pawn.IsMutant)
            {
                MutantDef mutantDef = pawn.mutant?.Def;
                if (mutantDef != null && mutantDef.preventsMentalBreaks)
                {
                    return true;
                }
            }

            HediffDef shambler = HediffDefOf.Shambler;
            return shambler != null && pawn.health?.hediffSet != null && pawn.health.hediffSet.HasHediff(shambler);
        }
    }

    /// <summary>
    /// Prefix-skips the static ESP Postfix for mutants that cannot panic-flee. Postfix cannot
    /// undo Notify_PawnLost, StopAll, StartJob, or ESP.injured. Do not skip vanilla
    /// Pawn.PostApplyDamage. Bind argument 0 as __0: Harmony treats __instance as this, and
    /// this target is static.
    ///
    /// Prefix пропускает static postfix ESP для мутантов без panic-flee. Postfix не откатит
    /// Notify_PawnLost, StopAll, StartJob и ESP.injured. Ванильный Pawn.PostApplyDamage не
    /// пропускать. Первый аргумент - __0: Harmony считает __instance this, а цель static.
    /// </summary>
    public static class EspPawnPostApplyDamage_MutantSkip_Patch
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref Pawn __0)
        {
            if (!KebabTweaksSettings.EnableAnomalyEspFleeFix)
            {
                return true;
            }

            return !AnomalyEspFleeFixFeatures.FleeMechanicDoesNotApply(__0);
        }
    }
}
#endif
