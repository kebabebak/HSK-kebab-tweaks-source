using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Armor Racks CompAssignableToPawn_ArmorRacks.AssignedAnything and TryAssignPawn
    /// use pawn.Map.listerBuildings with no null check. Vanilla CompAssignableToPawn.AssigningCandidates
    /// returns mapPawns.FreeColonists, and FreeColonists is built from AllPawns (spawned plus
    /// AllPawnsUnspawned via ThingOwnerUtility.GetAllThingsRecursively). Colonists in cryptosleep
    /// caskets / other ThingHolders therefore appear in the Set Owner dialog with Map == null.
    /// DrawUnassignedRow then calls AssignedAnything after ThingIcon; the NRE aborts the rest of
    /// DoWindowContents (later rows never draw, Close / scroll GUIClips break). TryAssignPawn
    /// fails the same way for those pawns. FreeColonistsSpawned would exclude them; Armor Racks
    /// does not use that list.
    ///
    /// Fix: soft Harmony Prefix on both methods. When pawn.Map is set, the original runs.
    /// When it is null, Prefix return false and reimplements the same rack scan / assign logic
    /// using the rack building's Map (parent.Map), so cryptosleep candidates stay visible and
    /// assignable. Prefix skip is required: the original throws before any Postfix can run.
    /// Without Armor Racks, AccessTools miss → patch skipped. Live-gated by
    /// EnableArmorRacksAssignFix.
    ///
    /// Проблема: Armor Racks CompAssignableToPawn_ArmorRacks.AssignedAnything и TryAssignPawn
    /// берут pawn.Map.listerBuildings без проверки на null. Ванильный AssigningCandidates даёт
    /// mapPawns.FreeColonists, а FreeColonists строится из AllPawns (spawned плюс
    /// AllPawnsUnspawned через ThingOwnerUtility.GetAllThingsRecursively). Колонисты в криптосне
    /// / других ThingHolder поэтому есть в диалоге Set Owner при Map == null. DrawUnassignedRow
    /// после ThingIcon вызывает AssignedAnything; NRE обрывает DoWindowContents (ниже строки не
    /// рисуются, ломаются Close / GUIClips scroll-view). TryAssignPawn падает так же.
    /// FreeColonistsSpawned таких пешек не включает; Armor Racks этот список не использует.
    ///
    /// Исправление: soft Harmony Prefix на оба метода. Если pawn.Map задан — идёт оригинал.
    /// Если null — Prefix return false и та же логика скана / назначения через Map стойки
    /// (parent.Map): кандидаты из криптосна остаются видимыми и назначаемыми. Prefix skip
    /// обязателен: оригинал падает до Postfix. Без Armor Racks AccessTools не находит цель →
    /// патч пропускается. Live-gate: EnableArmorRacksAssignFix.
    /// </summary>
    public static class ArmorRacksAssignFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                Type assignCompType = AccessTools.TypeByName("ArmorRacks.ThingComps.CompAssignableToPawn_ArmorRacks");
                if (assignCompType == null)
                {
                    Log.Message("[ArmorRacksAssignFixPatch] Armor Racks not loaded; patch skipped.");
                    return;
                }

                MethodBase assignedAnything = AccessTools.Method(assignCompType, "AssignedAnything", new[] { typeof(Pawn) });
                MethodBase tryAssignPawn = AccessTools.Method(assignCompType, "TryAssignPawn", new[] { typeof(Pawn) });
                if (assignedAnything == null || tryAssignPawn == null)
                {
                    Log.Warning("[ArmorRacksAssignFixPatch] Armor Racks assign methods not found; patch skipped.");
                    return;
                }

                Type armorRackType = AccessTools.TypeByName("ArmorRacks.Things.ArmorRack");
                if (armorRackType == null)
                {
                    Log.Warning("[ArmorRacksAssignFixPatch] ArmorRacks.Things.ArmorRack not found; patch skipped.");
                    return;
                }

                if (!ArmorRacksAssignHelpers.TryInit(armorRackType, assignCompType))
                {
                    Log.Warning("[ArmorRacksAssignFixPatch] Failed to resolve Armor Racks helpers; patch skipped.");
                    return;
                }

                harmony.Patch(
                    assignedAnything,
                    prefix: new HarmonyMethod(
                        typeof(CompAssignableToPawn_ArmorRacks_AssignedAnything_Patch),
                        nameof(CompAssignableToPawn_ArmorRacks_AssignedAnything_Patch.Prefix)));
                harmony.Patch(
                    tryAssignPawn,
                    prefix: new HarmonyMethod(
                        typeof(CompAssignableToPawn_ArmorRacks_TryAssignPawn_Patch),
                        nameof(CompAssignableToPawn_ArmorRacks_TryAssignPawn_Patch.Prefix)));

                Log.Message("[ArmorRacksAssignFixPatch] Patches applied (null Map assign dialog / TryAssignPawn).");
            }
            catch (Exception e)
            {
                Log.Error("[ArmorRacksAssignFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// Shared soft-resolved Armor Racks types and list/scan helpers (no hard DLL reference).
    ///
    /// Общие soft-resolved типы Armor Racks и хелперы списка/скана (без жёсткой ссылки на DLL).
    /// </summary>
    internal static class ArmorRacksAssignHelpers
    {
        private static Type armorRackType;
        private static Type assignCompType;
        private static MethodInfo allBuildingsColonistOfClass;
        private static MethodInfo getComp;
        private static FieldInfo assignedPawnsField;
        private static MethodInfo sortAssignedPawns;

        public static bool TryInit(Type rackType, Type compType)
        {
            armorRackType = rackType;
            assignCompType = compType;

            MethodInfo openAll = AccessTools.Method(typeof(ListerBuildings), "AllBuildingsColonistOfClass");
            if (openAll == null)
            {
                return false;
            }

            allBuildingsColonistOfClass = openAll.MakeGenericMethod(armorRackType);
            getComp = typeof(ThingWithComps).GetMethod("GetComp", Type.EmptyTypes)?.MakeGenericMethod(assignCompType);
            assignedPawnsField = AccessTools.Field(typeof(CompAssignableToPawn), "assignedPawns");
            sortAssignedPawns = AccessTools.Method(typeof(CompAssignableToPawn), "SortAssignedPawns");

            return allBuildingsColonistOfClass != null
                && getComp != null
                && assignedPawnsField != null
                && sortAssignedPawns != null;
        }

        public static Map ResolveMap(CompAssignableToPawn comp, Pawn pawn)
        {
            if (pawn?.Map != null)
            {
                return pawn.Map;
            }

            return comp?.parent?.Map;
        }

        public static bool AssignedAnythingOnMap(Map map, Pawn pawn)
        {
            if (map == null || pawn == null)
            {
                return false;
            }

            foreach (CompAssignableToPawn rackComp in EnumerateRackAssignComps(map))
            {
                if (rackComp.AssignedPawns.Contains(pawn))
                {
                    return true;
                }
            }

            return false;
        }

        public static void TryAssignPawnOnMap(CompAssignableToPawn target, Map map, Pawn pawn)
        {
            List<Pawn> assigned = assignedPawnsField.GetValue(target) as List<Pawn>;
            if (assigned == null)
            {
                return;
            }

            if (assigned.Contains(pawn))
            {
                return;
            }

            foreach (CompAssignableToPawn rackComp in EnumerateRackAssignComps(map))
            {
                rackComp.TryUnassignPawn(pawn);
            }

            assigned.Add(pawn);
            sortAssignedPawns.Invoke(target, null);
        }

        private static IEnumerable<CompAssignableToPawn> EnumerateRackAssignComps(Map map)
        {
            object enumerable = allBuildingsColonistOfClass.Invoke(map.listerBuildings, null);
            if (enumerable is not IEnumerable racks)
            {
                yield break;
            }

            foreach (object rackObj in racks)
            {
                if (rackObj is not ThingWithComps rack)
                {
                    continue;
                }

                object compObj = getComp.Invoke(rack, null);
                if (compObj is CompAssignableToPawn rackComp)
                {
                    yield return rackComp;
                }
            }
        }
    }

    /// <summary>
    /// When pawn.Map is null, skip the original AssignedAnything (it NREs) and scan racks on parent.Map.
    ///
    /// Если pawn.Map null — пропускает оригинал AssignedAnything (NRE) и сканирует стойки на parent.Map.
    /// </summary>
    internal static class CompAssignableToPawn_ArmorRacks_AssignedAnything_Patch
    {
        public static bool Prefix(CompAssignableToPawn __instance, Pawn pawn, ref bool __result)
        {
            if (!KebabTweaksSettings.EnableArmorRacksAssignFix)
            {
                return true;
            }

            if (pawn?.Map != null)
            {
                return true;
            }

            Map map = ArmorRacksAssignHelpers.ResolveMap(__instance, pawn);
            __result = ArmorRacksAssignHelpers.AssignedAnythingOnMap(map, pawn);
            return false;
        }
    }

    /// <summary>
    /// When pawn.Map is null, skip the original TryAssignPawn (it NREs) and assign using parent.Map.
    ///
    /// Если pawn.Map null — пропускает оригинал TryAssignPawn (NRE) и назначает через parent.Map.
    /// </summary>
    internal static class CompAssignableToPawn_ArmorRacks_TryAssignPawn_Patch
    {
        public static bool Prefix(CompAssignableToPawn __instance, Pawn pawn)
        {
            if (!KebabTweaksSettings.EnableArmorRacksAssignFix)
            {
                return true;
            }

            if (pawn?.Map != null)
            {
                return true;
            }

            Map map = ArmorRacksAssignHelpers.ResolveMap(__instance, pawn);
            if (map == null || pawn == null)
            {
                return false;
            }

            ArmorRacksAssignHelpers.TryAssignPawnOnMap(__instance, map, pawn);
            return false;
        }
    }
}
