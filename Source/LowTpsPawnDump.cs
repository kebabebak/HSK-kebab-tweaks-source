using System;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: diagnosing TPS drops needs a snapshot of what spawned pawns are doing when the
    /// HSK on-screen TPS counter (SK.TicksPerSecond.TPSActual) falls below a threshold.
    ///
    /// Fix: soft Harmony Postfix on the SK overlay hook; when TPS drops below threshold and
    /// logging is enabled, dump pawn job state for the current map (optional colonists-only /
    /// skip-when-paused filters and cooldown).
    ///
    /// Проблема: для диагностики просадок TPS нужен снимок того, чем заняты заспавненные пешки,
    /// когда экранный счётчик HSK (SK.TicksPerSecond.TPSActual) падает ниже порога.
    ///
    /// Исправление: soft Harmony Postfix на хук overlay SK; при падении TPS ниже порога и
    /// включённом логе — дамп job-состояния пешек текущей карты (фильтры colonists-only /
    /// skip-when-paused и cooldown).
    /// </summary>
    public static class LowTpsPawnDumpFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                if (!TpsMonitor.TryApplyHskCounterPatch(harmony))
                {
                    if (KebabTweaksSettings.LowTpsLogStartupMessage)
                    {
                        Log.Warning("[LowTpsPawnDump] SK.TicksPerSecond overlay hook not found; TPS monitor disabled.");
                    }

                    return;
                }

                if (KebabTweaksSettings.LowTpsLogStartupMessage)
                {
                    Log.Message(
                        "[LowTpsPawnDump] Loaded. Uses HSK on-screen TPS counter (SK.TicksPerSecond.TPSActual). " +
                        $"TPS drop dump {(KebabTweaksSettings.LowTpsEnableLogging ? "ON" : "OFF")}; " +
                        $"threshold {KebabTweaksSettings.LowTpsThreshold:0.#}; " +
                        $"cooldown {KebabTweaksSettings.LowTpsLogCooldownSeconds:0.##}s.");
                }
            }
            catch (Exception ex)
            {
                Log.Error("[LowTpsPawnDump] Failed to apply patches: " + ex);
            }
        }
    }

    [HarmonyPatch]
    internal static class TicksPerSecond_Overlay_Postfix_Patch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method("SK.TicksPerSecond:RimWorld_GlobalControlsUtility_DoTimespeedControls_Postfix");
        }

        public static void Postfix()
        {
            if (!KebabTweaksSettings.EnableLowTpsPawnDump)
            {
                return;
            }

            TpsMonitor.OnHskOverlayUpdated();
        }
    }

    internal static class TpsMonitor
    {
        private static FieldInfo tpsActualField;
        private static int lastObservedTps = -1;
        private static float logCooldownUntil;

        public static bool TryApplyHskCounterPatch(Harmony harmony)
        {
            ResolveHskCounterField();
            MethodBase overlayMethod = AccessTools.Method("SK.TicksPerSecond:RimWorld_GlobalControlsUtility_DoTimespeedControls_Postfix");
            if (tpsActualField == null || overlayMethod == null)
            {
                return false;
            }

            harmony.Patch(
                overlayMethod,
                postfix: new HarmonyMethod(typeof(TicksPerSecond_Overlay_Postfix_Patch), nameof(TicksPerSecond_Overlay_Postfix_Patch.Postfix)));
            return true;
        }

        private static void ResolveHskCounterField()
        {
            if (tpsActualField != null)
            {
                return;
            }

            Type ticksPerSecondType = AccessTools.TypeByName("SK.TicksPerSecond");
            tpsActualField = ticksPerSecondType != null
                ? AccessTools.Field(ticksPerSecondType, "TPSActual")
                : null;
        }

        public static void OnHskOverlayUpdated()
        {
            if (!KebabTweaksSettings.EnableLowTpsPawnDump
                || !KebabTweaksSettings.LowTpsEnableLogging
                || tpsActualField == null)
            {
                return;
            }

            int tpsActual = (int)tpsActualField.GetValue(null);
            if (tpsActual == lastObservedTps)
            {
                return;
            }

            lastObservedTps = tpsActual;

            int tpsExpected = GetExpectedTps();
            if (KebabTweaksSettings.LowTpsSkipWhenPaused &&
                (Find.TickManager.CurTimeSpeed == TimeSpeed.Paused || tpsExpected == 0))
            {
                return;
            }

            float now = Time.realtimeSinceStartup;
            if (tpsActual >= KebabTweaksSettings.LowTpsThreshold || now < logCooldownUntil)
            {
                return;
            }

            logCooldownUntil = now + KebabTweaksSettings.LowTpsLogCooldownSeconds;
            PawnStateDumper.Dump(tpsActual, tpsExpected);
        }

        public static int GetExpectedTps()
        {
            float tickRateMultiplier = Find.TickManager.TickRateMultiplier;
            return tickRateMultiplier == 0f ? 0 : (int)Math.Round(60f * tickRateMultiplier);
        }
    }

    internal static class PawnStateDumper
    {
        private const string BlockStart = "==== [LowTpsPawnDump] TPS DROP";
        private const string PawnSeparator = " || ";
        private const string FieldSeparator = " | ";

        public static void Dump(int tpsActual, int tpsExpected)
        {
            Map map = Find.CurrentMap;
            StringBuilder sb = new StringBuilder(4096);
            sb.Append(BlockStart);
            sb.Append(FieldSeparator).Append("source=SK.TicksPerSecond.TPSActual");
            sb.Append(FieldSeparator).Append("tps=").Append(tpsActual);
            sb.Append(FieldSeparator).Append("expected=").Append(tpsExpected);
            sb.Append(FieldSeparator).Append("display=").Append(tpsActual).Append("(").Append(tpsExpected).Append(")");
            sb.Append(FieldSeparator).Append("gameTick=").Append(Find.TickManager.TicksGame);
            sb.Append(FieldSeparator).Append("speed=").Append(Find.TickManager.CurTimeSpeed);
            sb.Append(FieldSeparator).Append("map=").Append(map?.Parent?.LabelCap ?? "(none)");
            sb.Append(FieldSeparator).Append("pawnFilter=").Append(KebabTweaksSettings.LowTpsColonistsOnly ? "colonists" : "all");
            sb.Append(" ==== ");

            if (map == null)
            {
                sb.Append("NO_CURRENT_MAP");
                Log.Warning(sb.ToString());
                return;
            }

            int pawnIndex = 0;
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!ShouldIncludePawn(pawn))
                {
                    continue;
                }

                pawnIndex++;
                if (pawnIndex > 1)
                {
                    sb.Append(PawnSeparator);
                }

                AppendPawnState(sb, pawn, pawnIndex);
            }

            if (pawnIndex == 0)
            {
                sb.Append("NO_MATCHING_PAWNS");
            }

            sb.Append(" ==== END ====");
            Log.Warning(sb.ToString());
        }

        private static bool ShouldIncludePawn(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            if (!KebabTweaksSettings.LowTpsColonistsOnly)
            {
                return true;
            }

            return pawn.IsColonist;
        }

        private static void AppendPawnState(StringBuilder sb, Pawn pawn, int index)
        {
            sb.Append("PAWN#").Append(index);
            sb.Append(FieldSeparator).Append("name=").Append(pawn.LabelShort);
            sb.Append(FieldSeparator).Append("id=").Append(pawn.thingIDNumber);
            sb.Append(FieldSeparator).Append("kind=").Append(pawn.kindDef?.defName ?? "-");
            sb.Append(FieldSeparator).Append("faction=").Append(pawn.Faction?.Name ?? "-");
            sb.Append(FieldSeparator).Append("drafted=").Append(pawn.Drafted);
            sb.Append(FieldSeparator).Append("downed=").Append(pawn.Downed);
            sb.Append(FieldSeparator).Append("dead=").Append(pawn.Dead);
            sb.Append(FieldSeparator).Append("pos=").Append(pawn.Position);

            Job curJob = pawn.CurJob;
            if (curJob == null)
            {
                sb.Append(FieldSeparator).Append("job=NONE");
                return;
            }

            sb.Append(FieldSeparator).Append("job=").Append(curJob.def?.defName ?? "-");
            sb.Append(FieldSeparator).Append("jobLabel=").Append(curJob.def?.label ?? "-");
            sb.Append(FieldSeparator).Append("playerForced=").Append(curJob.playerForced);
            sb.Append(FieldSeparator).Append("targetA=").Append(FormatTarget(curJob.targetA));
            sb.Append(FieldSeparator).Append("targetB=").Append(FormatTarget(curJob.targetB));
            sb.Append(FieldSeparator).Append("targetC=").Append(FormatTarget(curJob.targetC));
            sb.Append(FieldSeparator).Append("count=").Append(curJob.count);

            JobDriver driver = pawn.jobs?.curDriver;
            if (driver != null)
            {
                sb.Append(FieldSeparator).Append("driver=").Append(driver.GetType().Name);
            }

            string jobReport = pawn.GetJobReport();
            if (!jobReport.NullOrEmpty())
            {
                sb.Append(FieldSeparator).Append("report=").Append(jobReport);
            }

            if (JobFailReason.HaveReason)
            {
                sb.Append(FieldSeparator).Append("failReason=").Append(JobFailReason.Reason);
            }
        }

        private static string FormatTarget(LocalTargetInfo target)
        {
            if (!target.IsValid)
            {
                return "-";
            }

            if (target.HasThing)
            {
                Thing thing = target.Thing;
                return "Thing:" + thing.LabelShort + "(" + thing.def.defName + ")@" + thing.Position;
            }

            if (target.Cell.IsValid)
            {
                return "Cell:" + target.Cell;
            }

            return target.ToString();
        }
    }
}
