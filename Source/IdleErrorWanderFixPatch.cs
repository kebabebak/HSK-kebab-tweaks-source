using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: pawns under a non-voluntarily-joinable lord (e.g. Hospitality
    /// LordJob_VisitColony) still enter LordDuty through ThinkNode_JoinVoluntarilyJoinableLord
    /// because DutyDef.hook defaults to HighPriority. When ThinkNode_Duty returns no job and
    /// JobGiver_Wander cannot find a GotoWander cell (maxDanger None, polite_wander / bathroom
    /// filters, load timing), Wander returns null and LordDuty falls through to
    /// JobGiver_IdleError ("issued IdleError wait job").
    ///
    /// Fix: for lord pawns only, Postfix JobGiver_Wander.TryGiveJob so a null result becomes
    /// Wait_Wander (same as the wait half of the vanilla wander cycle) without blocking think-tree
    /// fallthrough for pawns with no lord. Safety Prefix on JobGiver_IdleError skips ErrorOnce
    /// only for non-voluntarily-joinable lords (the LordDuty IdleError branch) and issues Wait.
    ///
    /// Проблема: пешки под non-VJ lord (напр. Hospitality LordJob_VisitColony) всё равно входят
    /// в LordDuty через ThinkNode_JoinVoluntarilyJoinableLord, т.к. DutyDef.hook по умолчанию
    /// HighPriority. Если ThinkNode_Duty не дал job, а JobGiver_Wander не находит клетку
    /// GotoWander (maxDanger None, polite_wander / туалеты, тайминг load), Wander возвращает
    /// null и LordDuty доходит до JobGiver_IdleError («issued IdleError wait job»).
    ///
    /// Исправление: только для пешек с lord Postfix JobGiver_Wander.TryGiveJob подменяет null
    /// на Wait_Wander (как wait-ветка vanilla), не блокируя fallthrough у пешек без lord.
    /// Safety Prefix на JobGiver_IdleError: ErrorOnce глушится только для non-VJ lord
    /// (ветка IdleError в LordDuty), выдаётся Wait.
    /// </summary>
    public static class IdleErrorWanderFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                harmony.CreateClassProcessor(typeof(JobGiver_Wander_TryGiveJob_Patch)).Patch();
                harmony.CreateClassProcessor(typeof(JobGiver_IdleError_TryGiveJob_Patch)).Patch();
                Log.Message("[IdleErrorWanderFixPatch] Patches applied.");
            }
            catch (System.Exception e)
            {
                Log.Error("[IdleErrorWanderFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// LordDuty can hit IdleError when wander returns null. Only rewrite null for pawns that
    /// currently have a lord, so colonists without a lord still fall through to later think nodes.
    ///
    /// LordDuty может дойти до IdleError, если wander вернул null. Null переписывается только у
    /// пешек с lord, чтобы колонисты без lord по-прежнему шли к следующим узлам дерева.
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_Wander), "TryGiveJob")]
    [HarmonyAfter("avilmask.CommonSense")]
    public static class JobGiver_Wander_TryGiveJob_Patch
    {
        public static void Postfix(Pawn pawn, ref Job __result)
        {
            if (!KebabTweaksSettings.EnableIdleErrorWanderFix)
            {
                return;
            }

            if (__result != null || pawn == null || pawn.Destroyed || !pawn.Spawned)
            {
                return;
            }

            if (pawn.GetLord() == null)
            {
                return;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Wait_Wander);
            job.expiryInterval = 125;
            __result = job;
        }
    }

    /// <summary>
    /// Safety net for the LordDuty IdleError branch (non-VJ lords only). Prefix skip is required
    /// because Postfix cannot undo Log.ErrorOnce. Voluntarily joinable lords keep vanilla logging.
    ///
    /// Страховка для ветки IdleError в LordDuty (только non-VJ). Prefix skip нужен: Postfix не
    /// отменит Log.ErrorOnce. У VJ-лордов остаётся vanilla-лог.
    /// </summary>
    [HarmonyPatch(typeof(JobGiver_IdleError), "TryGiveJob")]
    public static class JobGiver_IdleError_TryGiveJob_Patch
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            if (!KebabTweaksSettings.EnableIdleErrorWanderFix)
            {
                return true;
            }

            if (pawn == null)
            {
                return true;
            }

            Lord lord = pawn.GetLord();
            if (lord == null || lord.LordJob is LordJob_VoluntarilyJoinable)
            {
                return true;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Wait);
            job.expiryInterval = 100;
            __result = job;
            return false;
        }
    }
}
