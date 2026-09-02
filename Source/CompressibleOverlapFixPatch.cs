using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: MapFileCompressor.HashValueForSquare allows one saveCompressible thing per cell
    /// and logs Error when two occupy the same square. Minerals StaticMineral PlaceIsBlocked only
    /// inspects the origin cell, so a 2x2 LargeFossil (or any multi-cell saveCompressible mineral)
    /// can overlap another fossil whose origin is offset by one cell. Independent cluster rolls on
    /// the same stone make that overlap likely. Rock chunks can hit the same compressor
    /// rule when two land in one cell.
    ///
    /// Fix: Postfix Minerals PlaceIsBlocked so OccupiedRect overlap with another compressor-conflict
    /// thing is treated as blocked (soft-optional if Minerals is absent). Prefix
    /// MapFileCompressor.BuildCompressedString and Postfix Map.FinalizeInit move extras to a free
    /// cell, or destroy them if none is free. Items on a cell that already allows more than one
    /// item stay in place, matching CompressibilityDeciderUtility.IsSaveCompressible.
    ///
    /// Проблема: MapFileCompressor.HashValueForSquare допускает один saveCompressible объект на
    /// клетку и пишет Error, если в клетке два. Minerals StaticMineral PlaceIsBlocked смотрит только
    /// клетку-якорь, поэтому 2x2 LargeFossil (или любой многоклеточный saveCompressible минерал)
    /// может пересечься с другой окаменелостью, якорь которой сдвинут на одну клетку. Независимые
    /// кластеры на одном камне делают пересечение вероятным. Камни Chunk* дают ту же ошибку
    /// компрессора, если два попадают в одну клетку.
    ///
    /// Исправление: Postfix Minerals PlaceIsBlocked считает OccupiedRect с другим конфликтом
    /// компрессора занятым (soft-optional, если Minerals нет). Prefix
    /// MapFileCompressor.BuildCompressedString и Postfix Map.FinalizeInit переносят лишние на
    /// свободную клетку или уничтожают, если места нет. Предметы в клетке, где уже разрешено больше
    /// одного предмета, остаются на месте, как в CompressibilityDeciderUtility.IsSaveCompressible.
    /// </summary>
    public static class CompressibleOverlapFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                MethodInfo buildCompressed = AccessTools.Method(typeof(MapFileCompressor), nameof(MapFileCompressor.BuildCompressedString));
                if (buildCompressed != null)
                {
                    harmony.Patch(
                        buildCompressed,
                        prefix: new HarmonyMethod(
                            typeof(MapFileCompressor_BuildCompressedString_Patch),
                            nameof(MapFileCompressor_BuildCompressedString_Patch.Prefix)));
                }
                else
                {
                    Log.Message("[HSK kebab tweaks] MapFileCompressor.BuildCompressedString not found; overlapping save-object save hook skipped.");
                }

                MethodBase finalize = AccessTools.Method(typeof(Map), "FinalizeInit")
                    ?? AccessTools.Method(typeof(Map), "FinalizeLoading");
                if (finalize != null)
                {
                    harmony.Patch(
                        finalize,
                        postfix: new HarmonyMethod(
                            typeof(Map_FinalizeInit_CompressibleOverlap_Patch),
                            nameof(Map_FinalizeInit_CompressibleOverlap_Patch.Postfix)));
                }
                else
                {
                    Log.Message("[HSK kebab tweaks] Map.FinalizeInit/FinalizeLoading not found; overlapping save-object load hook skipped.");
                }

                Type mineralDefType = AccessTools.TypeByName("Minerals.ThingDef_StaticMineral");
                MethodInfo placeBlocked = mineralDefType == null
                    ? null
                    : AccessTools.Method(mineralDefType, "PlaceIsBlocked", new[] { typeof(Map), typeof(IntVec3), typeof(bool) });
                if (placeBlocked != null)
                {
                    harmony.Patch(
                        placeBlocked,
                        postfix: new HarmonyMethod(
                            typeof(Minerals_PlaceIsBlocked_CompressibleOverlap_Patch),
                            nameof(Minerals_PlaceIsBlocked_CompressibleOverlap_Patch.Postfix)));
                }
                else
                {
                    Log.Message("[HSK kebab tweaks] Minerals PlaceIsBlocked not found; overlapping mineral spawn hook skipped.");
                }

                Log.Message(
                    "[HSK kebab tweaks] Overlapping save objects fix loaded (verbose logging " +
                    $"{(KebabTweaksSettings.CompressibleOverlapFixEnableLogging ? "ON" : "OFF")}).");
            }
            catch (Exception ex)
            {
                Log.Error("[HSK kebab tweaks] Failed to apply overlapping save objects fix: " + ex);
            }
        }
    }

    /// <summary>
    /// Moves extra saveCompressible things off shared cells before the compressor hashes the map.
    ///
    /// Убирает лишние saveCompressible с общих клеток до хеша компрессора.
    /// </summary>
    internal static class MapFileCompressor_BuildCompressedString_Patch
    {
        public static void Prefix(MapFileCompressor __instance)
        {
            if (!KebabTweaksSettings.EnableCompressibleOverlapFix)
            {
                return;
            }

            Map map = CompressibleOverlapFix.MapOf(__instance);
            CompressibleOverlapFix.ResolveOverlaps(map, "save");
        }
    }

    /// <summary>
    /// Unstacks overlapping saveCompressible things after map gen or load.
    ///
    /// Разнимает пересекающиеся saveCompressible после генерации или загрузки карты.
    /// </summary>
    internal static class Map_FinalizeInit_CompressibleOverlap_Patch
    {
        public static void Postfix(Map __instance)
        {
            if (!KebabTweaksSettings.EnableCompressibleOverlapFix)
            {
                return;
            }

            CompressibleOverlapFix.ResolveOverlaps(__instance, "map init");
        }
    }

    /// <summary>
    /// Blocks a Minerals spawn whose OccupiedRect would share a cell with another saveCompressible thing.
    /// Postfix is used because the original origin-only scan cannot see a 2x2 overlap from an offset anchor.
    ///
    /// Блокирует спавн Minerals, если OccupiedRect делит клетку с другим saveCompressible.
    /// Postfix: исходная проверка только якоря не видит пересечение 2x2 со сдвинутым якорем.
    /// </summary>
    internal static class Minerals_PlaceIsBlocked_CompressibleOverlap_Patch
    {
        public static void Postfix(ThingDef __instance, Map map, IntVec3 position, ref bool __result)
        {
            if (__result || !KebabTweaksSettings.EnableCompressibleOverlapFix)
            {
                return;
            }

            if (map == null || __instance == null)
            {
                return;
            }

            CellRect rect = GenAdj.OccupiedRect(position, Rot4.North, __instance.size);
            List<string> replace = CompressibleOverlapFix.ThingsToReplaceOf(__instance);
            foreach (IntVec3 cell in rect)
            {
                if (!cell.InBounds(map))
                {
                    __result = true;
                    return;
                }

                int maxItemsInCell = cell.GetMaxItemsAllowedInCell(map);
                List<Thing> list = map.thingGrid.ThingsListAtFast(cell);
                for (int i = 0; i < list.Count; i++)
                {
                    Thing thing = list[i];
                    if (!CompressibleOverlapFix.IsCompressorConflictThing(thing, maxItemsInCell))
                    {
                        continue;
                    }

                    if (replace != null && replace.Count > 0 && replace.Contains(thing.def.defName))
                    {
                        continue;
                    }

                    __result = true;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Shared overlap scan, relocate, and optional Minerals ThingsToReplace lookup.
    ///
    /// Общий поиск пересечений, перенос и опциональный ThingsToReplace Minerals.
    /// </summary>
    internal static class CompressibleOverlapFix
    {
        private const int MaxResolvePasses = 8;
        private const int MaxRelocateRadius = 40;

        private static readonly FieldInfo CompressorMapField = AccessTools.Field(typeof(MapFileCompressor), "map");
        private static readonly Type MineralDefType = AccessTools.TypeByName("Minerals.ThingDef_StaticMineral");
        private static readonly FieldInfo ThingsToReplaceField = MineralDefType == null
            ? null
            : AccessTools.Field(MineralDefType, "ThingsToReplace");

        public static Map MapOf(MapFileCompressor compressor)
        {
            if (compressor == null || CompressorMapField == null)
            {
                return null;
            }

            return CompressorMapField.GetValue(compressor) as Map;
        }

        public static List<string> ThingsToReplaceOf(ThingDef def)
        {
            if (def == null || ThingsToReplaceField == null || MineralDefType == null)
            {
                return null;
            }

            if (!MineralDefType.IsInstanceOfType(def))
            {
                return null;
            }

            return ThingsToReplaceField.GetValue(def) as List<string>;
        }

        public static void ResolveOverlaps(Map map, string reason)
        {
            if (map == null || map.thingGrid == null)
            {
                return;
            }

            int moved = 0;
            int destroyed = 0;
            HashSet<Thing> extras = new HashSet<Thing>();
            List<Thing> batch = new List<Thing>();
            for (int pass = 0; pass < MaxResolvePasses; pass++)
            {
                extras.Clear();
                CollectExtras(map, extras);
                if (extras.Count == 0)
                {
                    break;
                }

                batch.Clear();
                batch.AddRange(extras);
                for (int i = 0; i < batch.Count; i++)
                {
                    Thing thing = batch[i];
                    if (thing == null || thing.Destroyed || !thing.Spawned)
                    {
                        continue;
                    }

                    RelocateResult result = TryRelocate(thing);
                    if (result == RelocateResult.Moved)
                    {
                        moved++;
                    }
                    else if (result == RelocateResult.Destroyed)
                    {
                        destroyed++;
                    }
                }
            }

            if (moved > 0 || destroyed > 0)
            {
                CompressibleOverlapFixLog.Message(
                    $"[CompressibleOverlapFix] {reason}: moved {moved}, destroyed {destroyed} overlapping save objects.");
            }
        }

        private static void CollectExtras(Map map, HashSet<Thing> extras)
        {
            int n = map.cellIndices.NumGridCells;
            for (int i = 0; i < n; i++)
            {
                IntVec3 cell = map.cellIndices.IndexToCell(i);
                List<Thing> list = map.thingGrid.ThingsListAtFast(cell);
                int maxItemsInCell = cell.GetMaxItemsAllowedInCell(map);
                Thing keep = null;
                int compressible = 0;
                for (int j = 0; j < list.Count; j++)
                {
                    Thing other = list[j];
                    if (!IsCompressorConflictThing(other, maxItemsInCell))
                    {
                        continue;
                    }

                    compressible++;
                    if (keep == null || other.thingIDNumber < keep.thingIDNumber)
                    {
                        keep = other;
                    }
                }

                if (compressible < 2 || keep == null)
                {
                    continue;
                }

                for (int j = 0; j < list.Count; j++)
                {
                    Thing other = list[j];
                    if (!IsCompressorConflictThing(other, maxItemsInCell) || other == keep)
                    {
                        continue;
                    }

                    extras.Add(other);
                }
            }
        }

        private static RelocateResult TryRelocate(Thing thing)
        {
            Map map = thing.Map;
            IntVec3 from = thing.Position;
            Rot4 rot = thing.Rotation;
            ThingDef def = thing.def;
            thing.DeSpawn(DestroyMode.Vanish);
            IntVec3 dest;
            if (TryFindFreeCell(map, def, rot, from, out dest))
            {
                GenSpawn.Spawn(thing, dest, map, WipeMode.Vanish);
                CompressibleOverlapFixLog.Message(
                    $"[CompressibleOverlapFix] Moved {def} from {from} to {dest}.");
                return RelocateResult.Moved;
            }

            CompressibleOverlapFixLog.Message(
                $"[CompressibleOverlapFix] Destroyed extra {def} at {from}; no free cell.");
            thing.Destroy(DestroyMode.Vanish);
            return RelocateResult.Destroyed;
        }

        private static bool TryFindFreeCell(Map map, ThingDef def, Rot4 rot, IntVec3 from, out IntVec3 dest)
        {
            dest = from;
            Predicate<IntVec3> ok = cell => CanPlaceCompressible(map, def, rot, cell);
            if (ok(from))
            {
                dest = from;
                return true;
            }

            for (int radius = 1; radius <= MaxRelocateRadius; radius++)
            {
                if (CellFinder.TryFindRandomCellNear(from, map, radius, ok, out dest))
                {
                    return true;
                }
            }

            dest = from;
            return false;
        }

        private static bool CanPlaceCompressible(Map map, ThingDef def, Rot4 rot, IntVec3 cell)
        {
            if (def == null || map == null)
            {
                return false;
            }

            CellRect rect = GenAdj.OccupiedRect(cell, rot, def.size);
            foreach (IntVec3 occupied in rect)
            {
                if (!occupied.InBounds(map))
                {
                    return false;
                }

                List<Thing> list = map.thingGrid.ThingsListAtFast(occupied);
                int maxItemsInCell = occupied.GetMaxItemsAllowedInCell(map);
                for (int i = 0; i < list.Count; i++)
                {
                    if (IsCompressorConflictThing(list[i], maxItemsInCell))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// True when this thing would occupy a compressor hash slot. Matches the def.saveCompressible
        /// flag except for items on a cell that already allows more than one item (vanilla
        /// CompressibilityDeciderUtility.IsSaveCompressible returns false there).
        ///
        /// True, если объект занял бы слот хеша компрессора. Совпадает с флагом def.saveCompressible,
        /// кроме предметов в клетке, где уже разрешено больше одного предмета (ванильный
        /// CompressibilityDeciderUtility.IsSaveCompressible там возвращает false).
        /// </summary>
        internal static bool IsCompressorConflictThing(Thing thing, int maxItemsInCell)
        {
            if (thing == null || thing.Destroyed || !thing.Spawned || thing.def == null || !thing.def.saveCompressible)
            {
                return false;
            }

            if (thing.def.category == ThingCategory.Item && maxItemsInCell > 1)
            {
                return false;
            }

            return true;
        }

        private enum RelocateResult
        {
            Moved,
            Destroyed
        }
    }

    internal static class CompressibleOverlapFixLog
    {
        public static void Message(string text)
        {
            if (KebabTweaksSettings.CompressibleOverlapFixEnableLogging)
            {
                Log.Message(text);
            }
        }
    }
}
