using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Combat Extended CompSuppressable.AddSuppression calls
    /// PawnDestinationReservationManager.Reserve for RunForCover before StartJob, then
    /// JobDriver_Goto.TryMakePreToilReservations Reserves again. Reserve always runs
    /// ObsoleteAllClaimedBy first, so the CE entry stays obsolete with the RunForCover job
    /// still attached. When the pawn later has no CurJob, vanilla VerifyReservations
    /// ErrorOnce-logs "Pawn destination reservation manager failed to clean up properly"
    /// and clears reservations.
    ///
    /// Fix: soft Harmony Postfix on AddSuppression. When CurJob is RunForCover, call
    /// ReleaseAllObsoleteClaimedBy so the duplicate obsolete entry is dropped while the
    /// active Goto reservation remains. HunkerDown is untouched (single Reserve, no Goto
    /// double). No Prefix skip. Without CE, AccessTools.Method miss → patch skipped.
    ///
    /// Проблема: Combat Extended CompSuppressable.AddSuppression вызывает
    /// PawnDestinationReservationManager.Reserve для RunForCover до StartJob, затем
    /// JobDriver_Goto.TryMakePreToilReservations резервирует снова. Reserve сначала делает
    /// ObsoleteAllClaimedBy, поэтому запись CE остаётся obsolete со ссылкой на job
    /// RunForCover. Когда у пешки уже нет CurJob, vanilla VerifyReservations пишет
    /// ErrorOnce «Pawn destination reservation manager failed to clean up properly» и
    /// чистит резервации.
    ///
    /// Исправление: soft Harmony Postfix на AddSuppression. Если CurJob — RunForCover,
    /// вызвать ReleaseAllObsoleteClaimedBy: obsolete-дубликат снимается, активная
    /// резервация Goto остаётся. HunkerDown не трогаем (один Reserve, без Goto). Без
    /// Prefix skip. Без CE AccessTools.Method не находит цель → патч пропускается.
    /// </summary>
    public static class CeRunForCoverDestFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                MethodBase target = AccessTools.Method("CombatExtended.CompSuppressable:AddSuppression");
                if (target == null)
                {
                    Log.Message("[CeRunForCoverDestFixPatch] Combat Extended CompSuppressable.AddSuppression not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    target,
                    postfix: new HarmonyMethod(
                        typeof(CompSuppressable_AddSuppression_Patch),
                        nameof(CompSuppressable_AddSuppression_Patch.Postfix)));

                Log.Message("[CeRunForCoverDestFixPatch] Patches applied (RunForCover obsolete destination cleanup).");
            }
            catch (Exception e)
            {
                Log.Error("[CeRunForCoverDestFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// After CE may have double-Reserved RunForCover, drop obsolete destination entries only.
    /// Active Goto reservation is kept. No-op when CurJob is not RunForCover.
    ///
    /// После возможного double-Reserve RunForCover снимает только obsolete-записи назначения.
    /// Активная резервация Goto сохраняется. No-op, если CurJob не RunForCover.
    /// </summary>
    public static class CompSuppressable_AddSuppression_Patch
    {
        private const string RunForCoverDefName = "RunForCover";

        public static void Postfix(ThingComp __instance)
        {
            if (!KebabTweaksSettings.EnableCeRunForCoverDestFix)
            {
                return;
            }

            if (__instance?.parent is not Pawn pawn)
            {
                return;
            }

            if (!pawn.Spawned || pawn.Map == null || pawn.Destroyed)
            {
                return;
            }

            JobDef curDef = pawn.CurJobDef;
            if (curDef == null || curDef.defName != RunForCoverDefName)
            {
                return;
            }

            PawnDestinationReservationManager mgr = pawn.Map.pawnDestinationReservationManager;
            if (mgr == null)
            {
                return;
            }

            mgr.ReleaseAllObsoleteClaimedBy(pawn);
        }
    }
}
