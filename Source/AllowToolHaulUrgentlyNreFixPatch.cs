using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Allow Tool HaulUrgentlyCacheHandler.GetHaulUrgentlyDesignatedThings reads
    /// map.designationManager.AllDesignations without null checks. During map transitions or
    /// HugsLib OnFixedUpdate, designationManager can be null → NRE spam via AllowToolController.
    ///
    /// Fix: soft Prefix on the private method — clear targetList and skip when map,
    /// targetList, or designationManager is null. No hard ref on AllowTool.dll.
    ///
    /// Проблема: Allow Tool GetHaulUrgentlyDesignatedThings читает
    /// map.designationManager.AllDesignations без null-check. При переходах карты или в
    /// HugsLib OnFixedUpdate designationManager может быть null → NRE через AllowToolController.
    ///
    /// Исправление: soft Prefix на private-метод — очистить targetList и пропустить, если map,
    /// targetList или designationManager null. Без hard ref на AllowTool.dll.
    /// </summary>
    public static class AllowToolHaulUrgentlyNreFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                Type handlerType = AccessTools.TypeByName("AllowTool.HaulUrgentlyCacheHandler");
                if (handlerType == null)
                {
                    Log.Message(
                        "[AllowToolHaulUrgentlyNreFixPatch] AllowTool.HaulUrgentlyCacheHandler not found; patch skipped.");
                    return;
                }

                MethodBase target = AccessTools.Method(
                    handlerType,
                    "GetHaulUrgentlyDesignatedThings",
                    new[] { typeof(Map), typeof(ICollection<Thing>) });
                if (target == null)
                {
                    Log.Warning(
                        "[AllowToolHaulUrgentlyNreFixPatch] GetHaulUrgentlyDesignatedThings not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(AllowTool_HaulUrgentlyDesignatedThings_Patch),
                        nameof(AllowTool_HaulUrgentlyDesignatedThings_Patch.Prefix)));

                Log.Message("[AllowToolHaulUrgentlyNreFixPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[AllowToolHaulUrgentlyNreFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// Skips Allow Tool designation scan when map or designationManager is not ready.
    ///
    /// Пропускает scan designations Allow Tool, если map или designationManager не готовы.
    /// </summary>
    public static class AllowTool_HaulUrgentlyDesignatedThings_Patch
    {
        public static bool Prefix(Map map, ICollection<Thing> targetList)
        {
            if (!KebabTweaksSettings.EnableAllowToolHaulUrgentlyNreFix)
            {
                return true;
            }

            if (map == null || targetList == null || map.designationManager == null)
            {
                if (targetList != null)
                {
                    targetList.Clear();
                }

                return false;
            }

            return true;
        }
    }
}
