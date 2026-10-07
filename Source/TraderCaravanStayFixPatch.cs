using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: IncidentWorker_TraderCaravanArrival.TryExecuteWorker sends the arrival letter, then
    /// looks for a wait cell with RCellFinder.TryFindRandomSpotJustOutsideColony. On 1.5 a false
    /// return is ignored and LordJob_TradeWithColony is still built with that cell. A failed search
    /// leaves IntVec3.Invalid. LordToil_Travel assigns DutyDefOf.TravelOrLeave; when
    /// JobGiver_GotoTravelDestination cannot reach the cell, JobGiver_ExitMapRandom walks the
    /// caravan off the map. On 1.6 the same worker returns false without MakeNewLord, so the
    /// spawned pawns have no lord and the humanlike think tree's non-colonist exit sends them off.
    ///
    /// Fix: Prefix on LordJob_TradeWithColony.CreateGraph replaces an invalid chillSpot with a
    /// spawned member's cell before the travel and defend toils copy it. Postfix on TryExecuteWorker,
    /// when the worker returns false after pawns were spawned and none of them has a lord, creates
    /// the trade lord at that cell. The capture list is installed only when
    /// IncidentParms.storeGeneratedNeutralPawns is null and is cleared afterwards. A valid chill
    /// spot is left alone.
    ///
    /// Проблема: IncidentWorker_TraderCaravanArrival.TryExecuteWorker отправляет письмо о прибытии,
    /// затем ищет клетку ожидания через RCellFinder.TryFindRandomSpotJustOutsideColony. На 1.5
    /// false игнорируется, и LordJob_TradeWithColony всё равно строится с этой клеткой. Неудачный
    /// поиск оставляет IntVec3.Invalid. LordToil_Travel ставит DutyDefOf.TravelOrLeave; если
    /// JobGiver_GotoTravelDestination не доходит до клетки, JobGiver_ExitMapRandom уводит караван
    /// с карты. На 1.6 тот же worker возвращает false без MakeNewLord, у появившихся пешек нет
    /// lord, и выход «не колонист» в дереве humanlike уводит их с карты.
    ///
    /// Исправление: Prefix на LordJob_TradeWithColony.CreateGraph подставляет вместо невалидного
    /// chillSpot клетку появившегося члена группы до того, как toil пути и обороны её скопируют.
    /// Postfix на TryExecuteWorker, если worker вернул false после спавна и ни у кого нет lord,
    /// создаёт торговый lord на этой клетке. Список захвата ставится только когда
    /// IncidentParms.storeGeneratedNeutralPawns равен null, и потом очищается. Валидный chill
    /// spot не меняется.
    /// </summary>
    public static class TraderCaravanStayFixFeatures
    {
        /// <summary>
        /// Registers the invalid wait-cell repair and the no-lord arrival recovery.
        ///
        /// Подключает замену невалидной клетки ожидания и восстановление прибытия без lord.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            try
            {
                MethodBase createGraph = AccessTools.Method(
                    typeof(LordJob_TradeWithColony),
                    nameof(LordJob_TradeWithColony.CreateGraph));
                if (createGraph != null)
                {
                    harmony.Patch(
                        createGraph,
                        prefix: new HarmonyMethod(
                            typeof(LordJob_TradeWithColony_CreateGraph_StayFix),
                            nameof(LordJob_TradeWithColony_CreateGraph_StayFix.Prefix)));
                }
                else
                {
                    Log.Warning("[TraderCaravanStayFix] CreateGraph not found; invalid wait cell left unchanged.");
                }

                MethodBase execute = AccessTools.Method(
                    typeof(IncidentWorker_TraderCaravanArrival),
                    "TryExecuteWorker",
                    new[] { typeof(IncidentParms) });
                if (execute != null)
                {
                    HarmonyMethod capture = new HarmonyMethod(
                        typeof(IncidentWorker_TraderCaravanArrival_StayFix),
                        nameof(IncidentWorker_TraderCaravanArrival_StayFix.Prefix))
                    {
                        priority = Priority.Last
                    };
                    harmony.Patch(
                        execute,
                        prefix: capture,
                        postfix: new HarmonyMethod(
                            typeof(IncidentWorker_TraderCaravanArrival_StayFix),
                            nameof(IncidentWorker_TraderCaravanArrival_StayFix.Postfix)),
                        finalizer: new HarmonyMethod(
                            typeof(IncidentWorker_TraderCaravanArrival_StayFix),
                            nameof(IncidentWorker_TraderCaravanArrival_StayFix.Finalizer)));
                }
                else
                {
                    Log.Warning("[TraderCaravanStayFix] TryExecuteWorker not found; failed arrival is not recovered.");
                }

                Log.Message("[TraderCaravanStayFix] Patches applied (invalid wait cell, no-lord arrival).");
            }
            catch (Exception ex)
            {
                Log.Error("[TraderCaravanStayFix] Failed to apply patches: " + ex);
            }
        }
    }

    /// <summary>
    /// Replaces an invalid trade-caravan wait cell with a spawned member's cell.
    ///
    /// Подставляет вместо невалидной клетки ожидания торгового каравана клетку появившегося члена.
    /// </summary>
    public static class LordJob_TradeWithColony_CreateGraph_StayFix
    {
        private static readonly FieldInfo ChillSpotField =
            AccessTools.Field(typeof(LordJob_TradeWithColony), "chillSpot");

        public static void Prefix(LordJob_TradeWithColony __instance)
        {
            if (!KebabTweaksSettings.EnableTraderCaravanStayFix || ChillSpotField == null || __instance == null)
            {
                return;
            }

            IntVec3 spot = (IntVec3)ChillSpotField.GetValue(__instance);
            if (spot.IsValid)
            {
                return;
            }

            Pawn anchor = FindSpawnedAnchor(__instance.lord);
            if (anchor == null)
            {
                return;
            }

            IntVec3 cell = anchor.Position;
            Map map = anchor.Map;
            if (map == null || !cell.IsValid || !cell.InBounds(map) || !cell.Standable(map))
            {
                return;
            }

            ChillSpotField.SetValue(__instance, cell);
        }

        /// <summary>
        /// Picks a spawned humanlike member, or any spawned member when none is humanlike.
        ///
        /// Берёт появившегося humanlike, либо любого появившегося, если humanlike нет.
        /// </summary>
        private static Pawn FindSpawnedAnchor(Lord lord)
        {
            if (lord == null)
            {
                return null;
            }

            Pawn anchor = null;
            List<Pawn> pawns = lord.ownedPawns;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn == null || !pawn.Spawned)
                {
                    continue;
                }

                if (anchor == null)
                {
                    anchor = pawn;
                }

                if (pawn.RaceProps != null && pawn.RaceProps.Humanlike)
                {
                    return pawn;
                }
            }

            return anchor;
        }
    }

    /// <summary>
    /// Gives a trade lord to pawns spawned by an arrival that returned false before MakeNewLord.
    ///
    /// Даёт торговый lord пешкам, которых прибытие заспавнило и вернуло false до MakeNewLord.
    /// </summary>
    public static class IncidentWorker_TraderCaravanArrival_StayFix
    {
        public static void Prefix(IncidentParms parms, ref List<Pawn> __state)
        {
            if (!KebabTweaksSettings.EnableTraderCaravanStayFix || parms == null)
            {
                return;
            }

            if (parms.storeGeneratedNeutralPawns != null)
            {
                return;
            }

            __state = new List<Pawn>();
            parms.storeGeneratedNeutralPawns = __state;
        }

        public static void Postfix(IncidentParms parms, ref bool __result, List<Pawn> __state)
        {
            if (!KebabTweaksSettings.EnableTraderCaravanStayFix || __result || parms == null || __state == null || __state.Count == 0)
            {
                return;
            }

            Map map = parms.target as Map;
            if (map == null || parms.faction == null)
            {
                return;
            }

            List<Pawn> staying = new List<Pawn>();
            Pawn anchor = null;
            bool trader = false;
            for (int i = 0; i < __state.Count; i++)
            {
                Pawn pawn = __state[i];
                if (pawn == null || pawn.Destroyed || !pawn.Spawned || pawn.Map != map || pawn.GetLord() != null)
                {
                    continue;
                }

                staying.Add(pawn);
                if (anchor == null || (pawn.RaceProps != null && pawn.RaceProps.Humanlike && (anchor.RaceProps == null || !anchor.RaceProps.Humanlike)))
                {
                    anchor = pawn;
                }

                if (pawn.mindState != null && pawn.mindState.wantsToTradeWithColony)
                {
                    trader = true;
                }
            }

            if (!trader || anchor == null || staying.Count == 0)
            {
                return;
            }

            IntVec3 cell = anchor.Position;
            if (!cell.IsValid || !cell.InBounds(map) || !cell.Standable(map))
            {
                return;
            }

            LordMaker.MakeNewLord(
                parms.faction,
                new LordJob_TradeWithColony(parms.faction, cell),
                map,
                staying);
        }

        /// <summary>
        /// Drops the capture list so a reused IncidentParms does not keep it.
        ///
        /// Снимает список захвата, чтобы повторно использованный IncidentParms его не хранил.
        /// </summary>
        public static Exception Finalizer(IncidentParms parms, List<Pawn> __state)
        {
            if (__state != null && parms != null && ReferenceEquals(parms.storeGeneratedNeutralPawns, __state))
            {
                parms.storeGeneratedNeutralPawns = null;
            }

            return null;
        }
    }
}
