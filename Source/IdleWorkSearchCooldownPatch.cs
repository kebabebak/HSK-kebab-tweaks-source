using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Idle colonists repeatedly run vanilla JobGiver_Work.TryIssueJobPackage after
    /// short Wait_Wander / GotoWander jobs. When no work exists, the giver walks the entire
    /// WorkGiversInOrderNormal list (map scans + reachability). Many idle pawns tank TPS.
    /// Postfix alone cannot avoid that cost — the expensive scan already ran.
    ///
    /// Fix: after a non-emergency NoJob result, remember GenTicks.TicksGame + configured cooldown
    /// (settings, ticks, max 100000). Prefix skips the original with ThinkResult.NoJob while the
    /// cooldown is active. Finding a job clears stale cooldown state; changing Work tab priorities
    /// or allowed area clears cooldown so the next wander can run a full scan with new rules.
    /// Emergency JobGiver_Work is never throttled. Cooldown adds a minimum interval between full
    /// scans after NoJob; vanilla wander (125–200 ticks) still gates how often think tree runs.
    /// Prefix return false only for the cooldown window (documented; Postfix cannot skip the scan).
    ///
    /// Проблема: Idle-колонисты снова и снова вызывают vanilla JobGiver_Work.TryIssueJobPackage
    /// после коротких Wait_Wander / GotoWander. Если работы нет, giver проходит весь список
    /// WorkGiversInOrderNormal (сканы карты + reachability). Много idle-пешек роняет TPS.
    /// Один Postfix не спасает — дорогой scan уже выполнен.
    ///
    /// Исправление: после non-emergency NoJob запомнить GenTicks.TicksGame + cooldown из настроек
    /// (тики, макс. 100000). Prefix пропускает оригинал с ThinkResult.NoJob, пока cooldown
    /// активен. Найденная работа сбрасывает stale-состояние; смена приоритетов Work tab или
    /// allowed area сбрасывает cooldown, чтобы следующий wander сделал полный scan с новыми
    /// правилами. Emergency JobGiver_Work не троттлится. Cooldown — минимальный интервал между
    /// полными scan после NoJob; vanilla wander (125–200 тиков) всё равно задаёт частоту think tree.
    /// Prefix return false только в окне cooldown (документировано; Postfix не может пропустить scan).
    /// </summary>
    public static class IdleWorkSearchCooldownFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                MethodBase tryIssue = AccessTools.Method(
                    typeof(JobGiver_Work),
                    nameof(JobGiver_Work.TryIssueJobPackage));
                if (tryIssue == null)
                {
                    Log.Warning(
                        "[IdleWorkSearchCooldownPatch] JobGiver_Work.TryIssueJobPackage not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    tryIssue,
                    prefix: new HarmonyMethod(
                        typeof(JobGiver_Work_TryIssueJobPackage_Cooldown_Patch),
                        nameof(JobGiver_Work_TryIssueJobPackage_Cooldown_Patch.Prefix)),
                    postfix: new HarmonyMethod(
                        typeof(JobGiver_Work_TryIssueJobPackage_Cooldown_Patch),
                        nameof(JobGiver_Work_TryIssueJobPackage_Cooldown_Patch.Postfix)));

                MethodBase setPriority = AccessTools.Method(
                    typeof(Pawn_WorkSettings),
                    nameof(Pawn_WorkSettings.SetPriority));
                if (setPriority != null)
                {
                    harmony.Patch(
                        setPriority,
                        postfix: new HarmonyMethod(
                            typeof(Pawn_WorkSettings_InvalidateWorkSearchCooldown_Patch),
                            nameof(Pawn_WorkSettings_InvalidateWorkSearchCooldown_Patch.Postfix_SetPriority)));
                }

                MethodBase notifyPriorities = AccessTools.Method(
                    typeof(Pawn_WorkSettings),
                    nameof(Pawn_WorkSettings.Notify_UseWorkPrioritiesChanged));
                if (notifyPriorities != null)
                {
                    harmony.Patch(
                        notifyPriorities,
                        postfix: new HarmonyMethod(
                            typeof(Pawn_WorkSettings_InvalidateWorkSearchCooldown_Patch),
                            nameof(Pawn_WorkSettings_InvalidateWorkSearchCooldown_Patch.Postfix_Notify)));
                }

                MethodBase areaSetter = AccessTools.PropertySetter(
                    typeof(Pawn_PlayerSettings),
                    nameof(Pawn_PlayerSettings.AreaRestrictionInPawnCurrentMap));
                if (areaSetter != null)
                {
                    harmony.Patch(
                        areaSetter,
                        postfix: new HarmonyMethod(
                            typeof(Pawn_PlayerSettings_AreaRestriction_Invalidate_Patch),
                            nameof(Pawn_PlayerSettings_AreaRestriction_Invalidate_Patch.Postfix)));
                }

                Log.Message("[IdleWorkSearchCooldownPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[IdleWorkSearchCooldownPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// Per-pawn next allowed Game tick for a full non-emergency JobGiver_Work scan.
    ///
    /// На пешку: следующий разрешённый Game tick для полного non-emergency JobGiver_Work scan.
    /// </summary>
    internal static class IdleWorkSearchCooldownState
    {
        private static readonly Dictionary<int, int> NextAllowedScanTickByPawnId =
            new Dictionary<int, int>();

        private static readonly FieldInfo WorkSettingsPawnField =
            AccessTools.Field(typeof(Pawn_WorkSettings), "pawn");

        private static readonly FieldInfo PlayerSettingsPawnField =
            AccessTools.Field(typeof(Pawn_PlayerSettings), "pawn");

        public static bool IsOnCooldown(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            if (!NextAllowedScanTickByPawnId.TryGetValue(pawn.thingIDNumber, out int nextTick))
            {
                return false;
            }

            if (GenTicks.TicksGame >= nextTick)
            {
                NextAllowedScanTickByPawnId.Remove(pawn.thingIDNumber);
                return false;
            }

            return true;
        }

        public static void ArmCooldown(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            int cooldown = KebabTweaksSettings.IdleWorkSearchCooldownTicks;
            if (cooldown <= 0)
            {
                Clear(pawn);
                return;
            }

            NextAllowedScanTickByPawnId[pawn.thingIDNumber] = GenTicks.TicksGame + cooldown;
        }

        public static void Clear(Pawn pawn)
        {
            if (pawn == null)
            {
                return;
            }

            NextAllowedScanTickByPawnId.Remove(pawn.thingIDNumber);
        }

        public static void ClearForWorkSettings(Pawn_WorkSettings settings)
        {
            Clear(WorkSettingsPawnField?.GetValue(settings) as Pawn);
        }

        public static void ClearForPlayerSettings(Pawn_PlayerSettings settings)
        {
            Clear(PlayerSettingsPawnField?.GetValue(settings) as Pawn);
        }
    }

    /// <summary>
    /// Skips JobGiver_Work while cooldown is armed; records NoJob / clears on success.
    /// Prefix return false is required so the full WorkGiver list is not scanned again.
    /// __state marks that the original ran — Postfix must not re-arm after a Prefix skip.
    ///
    /// Пропускает JobGiver_Work при активном cooldown; при NoJob ставит окно, при успехе
    /// сбрасывает. Prefix return false нужен, иначе полный список WorkGiver снова сканируется.
    /// __state = оригинал реально бежал; после Prefix-skip Postfix не продлевает cooldown.
    /// </summary>
    public static class JobGiver_Work_TryIssueJobPackage_Cooldown_Patch
    {
        public static bool Prefix(
            JobGiver_Work __instance,
            Pawn pawn,
            ref ThinkResult __result,
            out bool __state)
        {
            __state = false;

            if (!KebabTweaksSettings.EnableIdleWorkSearchCooldown
                || KebabTweaksSettings.IdleWorkSearchCooldownTicks <= 0
                || __instance == null
                || __instance.emergency
                || pawn == null)
            {
                __state = true;
                return true;
            }

            if (!IdleWorkSearchCooldownState.IsOnCooldown(pawn))
            {
                __state = true;
                return true;
            }

            if (KebabTweaksSettings.IdleWorkSearchCooldownEnableLogging)
            {
                Log.Message(
                    $"[IdleWorkSearchCooldownPatch] Skip full work scan for {pawn.LabelShort} " +
                    $"(cooldown {KebabTweaksSettings.IdleWorkSearchCooldownTicks} ticks).");
            }

            __result = ThinkResult.NoJob;
            return false;
        }

        public static void Postfix(
            JobGiver_Work __instance,
            Pawn pawn,
            ThinkResult __result,
            bool __state)
        {
            if (!__state
                || !KebabTweaksSettings.EnableIdleWorkSearchCooldown
                || KebabTweaksSettings.IdleWorkSearchCooldownTicks <= 0
                || __instance == null
                || __instance.emergency
                || pawn == null)
            {
                return;
            }

            if (__result.IsValid)
            {
                IdleWorkSearchCooldownState.Clear(pawn);
                return;
            }

            IdleWorkSearchCooldownState.ArmCooldown(pawn);
        }
    }

    /// <summary>
    /// Clears work-search cooldown when Work tab priorities change for that pawn.
    ///
    /// Сбрасывает cooldown поиска работы при смене приоритетов Work tab у пешки.
    /// </summary>
    public static class Pawn_WorkSettings_InvalidateWorkSearchCooldown_Patch
    {
        public static void Postfix_SetPriority(Pawn_WorkSettings __instance)
        {
            if (!KebabTweaksSettings.EnableIdleWorkSearchCooldown)
            {
                return;
            }

            IdleWorkSearchCooldownState.ClearForWorkSettings(__instance);
        }

        public static void Postfix_Notify(Pawn_WorkSettings __instance)
        {
            if (!KebabTweaksSettings.EnableIdleWorkSearchCooldown)
            {
                return;
            }

            IdleWorkSearchCooldownState.ClearForWorkSettings(__instance);
        }
    }

    /// <summary>
    /// Clears work-search cooldown when the pawn's allowed area restriction changes.
    ///
    /// Сбрасывает cooldown поиска работы при смене allowed area у пешки.
    /// </summary>
    public static class Pawn_PlayerSettings_AreaRestriction_Invalidate_Patch
    {
        public static void Postfix(Pawn_PlayerSettings __instance)
        {
            if (!KebabTweaksSettings.EnableIdleWorkSearchCooldown || __instance == null)
            {
                return;
            }

            IdleWorkSearchCooldownState.ClearForPlayerSettings(__instance);
        }
    }
}
