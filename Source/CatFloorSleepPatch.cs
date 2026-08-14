using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using SK;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Core_SK JobGiver_GetRestPawnBedOK assigns domestic cats a LayDown job on a
    /// random floor cell near beds. It only checks whether another cat Thing is standing on the
    /// cell, not ReservationManager reservations or other pawns' LayDown jobs. Multiple cats can
    /// pick the same cell, causing "Could not reserve" / TryMakePreToilReservations failures.
    /// An empty master-bed adjacency list can also trigger RandomElement on an empty collection.
    ///
    /// Fix: Postfix TryGiveJob to validate floor LayDown targets and re-pick a free cell.
    /// Finalizer recovers from empty RandomElement without spamming errors.
    ///
    /// Проблема: Core_SK JobGiver_GetRestPawnBedOK выдаёт домашним кошкам LayDown на случайную
    /// клетку пола у кроватей. Проверяется только, стоит ли на клетке другой Thing кошки, но не
    /// ReservationManager и не LayDown других пешек. Несколько кошек могут выбрать одну клетку →
    /// "Could not reserve" / сбои TryMakePreToilReservations. Пустой список соседних клеток у
    /// master-bed может вызвать RandomElement на пустой коллекции.
    ///
    /// Исправление: Postfix TryGiveJob — проверять floor LayDown и заново выбирать свободную
    /// клетку. Finalizer перехватывает пустой RandomElement без спама ошибок.
    /// </summary>
    public static class CatFloorSleepFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                MethodBase tryGiveJob = AccessTools.Method(typeof(JobGiver_GetRestPawnBedOK), "TryGiveJob");
                if (tryGiveJob == null)
                {
                    Log.Warning("[CatFloorSleepPatch] SK.JobGiver_GetRestPawnBedOK.TryGiveJob not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    tryGiveJob,
                    postfix: new HarmonyMethod(typeof(JobGiver_GetRestPawnBedOK_TryGiveJob_Patch), nameof(JobGiver_GetRestPawnBedOK_TryGiveJob_Patch.Postfix)),
                    finalizer: new HarmonyMethod(typeof(JobGiver_GetRestPawnBedOK_TryGiveJob_Patch), nameof(JobGiver_GetRestPawnBedOK_TryGiveJob_Patch.Finalizer)));

                Log.Message(
                    $"[CatFloorSleepPatch] Loaded (verbose logging {(KebabTweaksSettings.CatFloorSleepEnableLogging ? "ON" : "OFF")}). " +
                    "Enable logging in mod settings for sleep-cell reassignment details.");
            }
            catch (Exception ex)
            {
                Log.Error("[CatFloorSleepPatch] Failed to apply patches: " + ex);
            }
        }
    }

    [HarmonyPatch]
    internal static class JobGiver_GetRestPawnBedOK_TryGiveJob_Patch
    {
        public static void Postfix(Pawn pawn, ref Job __result)
        {
            if (!KebabTweaksSettings.EnableCatFloorSleep)
            {
                return;
            }

            if (__result == null || __result.def != JobDefOf.LayDown)
            {
                return;
            }

            // Bed LayDown targets are handled by vanilla / Sensible Bed Ownership.
            if (__result.targetA.HasThing)
            {
                return;
            }

            if (!__result.targetA.IsValid)
            {
                __result = null;
                return;
            }

            IntVec3 chosen = __result.targetA.Cell;
            if (CatSleepCellUtility.IsSleepCellFree(pawn, chosen))
            {
                return;
            }

            CatFloorSleepPatchLog.Message(
                $"[CatFloorSleepPatch] {pawn.LabelShort}: floor sleep cell {chosen} is blocked; searching alternative.");

            if (TryReassignFloorSleepJob(pawn, chosen, ref __result)
                || TryReassignFloorSleepJob(pawn, pawn.Position, ref __result))
            {
                return;
            }

            CatFloorSleepPatchLog.Message(
                $"[CatFloorSleepPatch] {pawn.LabelShort}: no free floor sleep cell found; cancelled LayDown.");
            __result = null;
        }

        public static Exception Finalizer(Exception __exception, Pawn pawn, ref Job __result)
        {
            if (!KebabTweaksSettings.EnableCatFloorSleep)
            {
                return __exception;
            }

            if (__exception == null || !IsEmptyCollectionException(__exception))
            {
                return __exception;
            }

            CatFloorSleepPatchLog.Message(
                $"[CatFloorSleepPatch] {pawn?.LabelShort}: TryGiveJob hit empty RandomElement; recovering.");

            if (pawn != null && TryReassignFloorSleepJob(pawn, pawn.Position, ref __result))
            {
                return null;
            }

            __result = null;
            return null;
        }

        private static bool TryReassignFloorSleepJob(Pawn pawn, IntVec3 near, ref Job __result)
        {
            if (!CatSleepCellUtility.TryFindSleepCellNear(pawn, near, 12, out IntVec3 freeCell))
            {
                return false;
            }

            __result = JobMaker.MakeJob(JobDefOf.LayDown, freeCell);
            CatFloorSleepPatchLog.Message(
                $"[CatFloorSleepPatch] {pawn.LabelShort}: reassigned LayDown to {freeCell}.");
            return true;
        }

        private static bool IsEmptyCollectionException(Exception ex)
        {
            for (Exception current = ex; current != null; current = current.InnerException)
            {
                if (current is InvalidOperationException
                    && current.Message != null
                    && current.Message.IndexOf("empty collection", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal static class CatSleepCellUtility
    {
        public static bool IsSleepCellFree(Pawn pawn, IntVec3 cell)
        {
            if (pawn == null || !cell.IsValid)
            {
                return false;
            }

            Map map = pawn.Map;
            if (map == null || !cell.InBounds(map) || !cell.Standable(map))
            {
                return false;
            }

            if (cell.GetFirstPawn(map) != null)
            {
                return false;
            }

            if (map.reservationManager.IsReserved(cell))
            {
                return false;
            }

            foreach (Pawn other in map.mapPawns.AllPawnsSpawned)
            {
                if (other == null || other == pawn || other.Destroyed)
                {
                    continue;
                }

                Job job = other.CurJob;
                if (job?.def != JobDefOf.LayDown || !job.targetA.IsValid || job.targetA.HasThing)
                {
                    continue;
                }

                if (job.targetA.Cell == cell)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryFindSleepCellNear(Pawn pawn, IntVec3 center, int maxRadius, out IntVec3 cell)
        {
            cell = IntVec3.Invalid;
            Map map = pawn.Map;
            if (map == null || !center.InBounds(map))
            {
                return false;
            }

            for (int radius = 4; radius <= maxRadius; radius += 4)
            {
                if (CellFinder.TryRandomClosewalkCellNear(
                        center,
                        map,
                        radius,
                        out cell,
                        candidate => IsSleepCellFree(pawn, candidate)
                            && ReachabilityUtility.CanReach(
                                pawn,
                                candidate,
                                PathEndMode.OnCell,
                                Danger.Some)))
                {
                    return true;
                }
            }

            foreach (IntVec3 candidate in GenRadial.RadialCellsAround(center, maxRadius, true))
            {
                if (!candidate.InBounds(map))
                {
                    continue;
                }

                if (!IsSleepCellFree(pawn, candidate))
                {
                    continue;
                }

                if (!ReachabilityUtility.CanReach(pawn, candidate, PathEndMode.OnCell, Danger.Some))
                {
                    continue;
                }

                cell = candidate;
                return true;
            }

            return false;
        }
    }

    internal static class CatFloorSleepPatchLog
    {
        public static void Message(string text)
        {
            if (KebabTweaksSettings.CatFloorSleepEnableLogging)
            {
                Log.Message(text);
            }
        }
    }
}
