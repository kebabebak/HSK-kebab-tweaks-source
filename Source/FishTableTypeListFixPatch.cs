using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Performance Fish keeps a Fishery IndexedFishSet / FishTable type cache on
    /// ListerThings (ThingsPrepatches.AddToTypeList / RemoveFromTypeList). When that FishTable
    /// chain corrupts (empty System.Object tail links), every region Register/Deregister during
    /// Thing.set_Position throws InvalidOperationException ("Failed to find parent index" /
    /// "Operation is not valid"), spamming Exception ticking for walking pawns. Surgery timing
    /// (e.g. RepairScratch) is coincidental — the throw is on pathing Add/Remove, not on
    /// RemoveHediff.
    ///
    /// Fix: soft-optional Finalizers on AddToTypeList / RemoveFromTypeList swallow
    /// InvalidOperationException, clear and rebuild ThingsByType from listsByDef, then retry the
    /// failed add/remove once so pathing can continue.
    ///
    /// Проблема: Performance Fish держит type-cache ListerThings на Fishery IndexedFishSet /
    /// FishTable (ThingsPrepatches.AddToTypeList / RemoveFromTypeList). Когда цепочка FishTable
    /// портится (пустые хвосты System.Object), каждый Register/Deregister региона при
    /// Thing.set_Position бросает InvalidOperationException («Failed to find parent index» /
    /// «Operation is not valid»), спамя Exception ticking у идущих пешек. Операция (напр.
    /// RepairScratch) совпадает по времени — падение на Add/Remove при pathing, не на RemoveHediff.
    ///
    /// Исправление: soft-optional Finalizer на AddToTypeList / RemoveFromTypeList глотает
    /// InvalidOperationException, очищает и пересобирает ThingsByType из listsByDef, один раз
    /// повторяет add/remove, чтобы pathing продолжался.
    /// </summary>
    public static class FishTableTypeListFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                if (!TypeListRepair.Resolve())
                {
                    Log.Message(
                        "[FishTableTypeListFixPatch] Performance Fish ThingsPrepatches not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    TypeListRepair.AddToTypeListMethod,
                    finalizer: new HarmonyMethod(
                        typeof(ThingsPrepatches_AddToTypeList_Patch),
                        nameof(ThingsPrepatches_AddToTypeList_Patch.Finalizer)));
                harmony.Patch(
                    TypeListRepair.RemoveFromTypeListMethod,
                    finalizer: new HarmonyMethod(
                        typeof(ThingsPrepatches_RemoveFromTypeList_Patch),
                        nameof(ThingsPrepatches_RemoveFromTypeList_Patch.Finalizer)));

                Log.Message("[FishTableTypeListFixPatch] Patches applied (Performance Fish type-list guard).");
            }
            catch (Exception e)
            {
                Log.Error("[FishTableTypeListFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// Resolves Performance Fish type-list helpers by name and rebuilds corrupted ThingsByType sets.
    ///
    /// Резолвит helpers type-list Performance Fish по имени и пересобирает испорченные ThingsByType.
    /// </summary>
    internal static class TypeListRepair
    {
        private const string PrepatchesTypeName = "PerformanceFish.Listers.ThingsPrepatches";

        private static int _repairDepth;
        private static bool _loggedRepair;

        public static MethodInfo AddToTypeListMethod { get; private set; }
        public static MethodInfo RemoveFromTypeListMethod { get; private set; }
        public static MethodInfo ClearTypeListsMethod { get; private set; }

        public static bool Resolve()
        {
            Type prepatches = AccessTools.TypeByName(PrepatchesTypeName);
            if (prepatches == null)
            {
                return false;
            }

            AddToTypeListMethod = AccessTools.DeclaredMethod(prepatches, "AddToTypeList");
            RemoveFromTypeListMethod = AccessTools.DeclaredMethod(prepatches, "RemoveFromTypeList");
            ClearTypeListsMethod = AccessTools.DeclaredMethod(prepatches, "ClearTypeLists");

            return AddToTypeListMethod != null && RemoveFromTypeListMethod != null;
        }

        public static Exception HandleTypeListFailure(
            Exception exception,
            ListerThings lister,
            Thing thing,
            bool isAdd)
        {
            if (!KebabTweaksSettings.EnableFishTableTypeListFix)
            {
                return exception;
            }

            if (exception == null)
            {
                return null;
            }

            if (!(exception is InvalidOperationException))
            {
                return exception;
            }

            if (lister == null || thing == null)
            {
                return null;
            }

            if (_repairDepth > 0)
            {
                return null;
            }

            _repairDepth++;
            try
            {
                if (!_loggedRepair)
                {
                    _loggedRepair = true;
                    Log.Warning(
                        "[FishTableTypeListFixPatch] Performance Fish ListerThings type-list FishTable " +
                        "corrupted; clearing and rebuilding from listsByDef. First failure: " +
                        exception.Message);
                }

                RebuildTypeLists(lister);

                try
                {
                    MethodInfo retry = isAdd ? AddToTypeListMethod : RemoveFromTypeListMethod;
                    retry?.Invoke(null, new object[] { lister, thing });
                }
                catch (TargetInvocationException)
                {
                    // Still broken after rebuild — keep tick alive; save/reload may still help.
                }
                catch (InvalidOperationException)
                {
                }
            }
            finally
            {
                _repairDepth--;
            }

            return null;
        }

        private static void RebuildTypeLists(ListerThings lister)
        {
            if (ClearTypeListsMethod != null)
            {
                try
                {
                    ClearTypeListsMethod.Invoke(null, new object[] { lister });
                }
                catch (Exception)
                {
                    // Clear itself can throw on a corrupted IndexedFishSet; still try rebuild adds.
                }
            }

            if (AddToTypeListMethod == null)
            {
                return;
            }

            List<Thing> sources = CollectThingsForRebuild(lister);
            for (int i = 0; i < sources.Count; i++)
            {
                Thing t = sources[i];
                if (t == null)
                {
                    continue;
                }

                try
                {
                    AddToTypeListMethod.Invoke(null, new object[] { lister, t });
                }
                catch (Exception)
                {
                    // Skip one bad entry; continue rebuilding the rest.
                }
            }
        }

        private static List<Thing> CollectThingsForRebuild(ListerThings lister)
        {
            var result = new List<Thing>();
            FieldInfo listsByDefField = AccessTools.Field(typeof(ListerThings), "listsByDef");
            if (listsByDefField?.GetValue(lister) is IDictionary dict)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    if (entry.Value is IList list)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i] is Thing thing)
                            {
                                result.Add(thing);
                            }
                        }
                    }
                }

                if (result.Count > 0)
                {
                    return result;
                }
            }

            List<Thing> allThings = lister.AllThings;
            if (allThings != null)
            {
                result.AddRange(allThings);
            }

            return result;
        }
    }

    /// <summary>
    /// Swallows FishTable InvalidOperationException on type-list add and triggers rebuild.
    ///
    /// Глотает InvalidOperationException FishTable при add type-list и запускает пересборку.
    /// </summary>
    internal static class ThingsPrepatches_AddToTypeList_Patch
    {
        public static Exception Finalizer(Exception __exception, ListerThings lister, Thing thing)
        {
            return TypeListRepair.HandleTypeListFailure(__exception, lister, thing, isAdd: true);
        }
    }

    /// <summary>
    /// Swallows FishTable InvalidOperationException on type-list remove and triggers rebuild.
    ///
    /// Глотает InvalidOperationException FishTable при remove type-list и запускает пересборку.
    /// </summary>
    internal static class ThingsPrepatches_RemoveFromTypeList_Patch
    {
        public static Exception Finalizer(Exception __exception, ListerThings lister, Thing thing)
        {
            return TypeListRepair.HandleTypeListFailure(__exception, lister, thing, isAdd: false);
        }
    }
}
