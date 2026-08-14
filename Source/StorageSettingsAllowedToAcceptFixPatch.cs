using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: StorageSettings.AllowedToAccept can NullReferenceException when filter is null
    /// or thing is null during haul/store search (e.g. While You're Up detour on unload inventory).
    /// Vanilla and PerformanceFish patches do not guard; the tick throws and JobDriver error-recovers.
    ///
    /// Fix: Harmony Prefix returns false (not allowed) when thing or filter is null and this fix
    /// is enabled. Vanilla types only.
    ///
    /// Проблема: StorageSettings.AllowedToAccept может дать NullReferenceException при null filter
    /// или null thing во время поиска склада (напр. detour While You're Up при unload inventory).
    /// Vanilla и патчи PerformanceFish не страхуют; тик падает, JobDriver уходит в error-recover.
    ///
    /// Исправление: Harmony Prefix при null thing или filter возвращает false (не разрешено) при
    /// включённом фиксе. Только vanilla-типы.
    /// </summary>
    public static class StorageSettingsAllowedToAcceptFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                var target = AccessTools.Method(
                    typeof(StorageSettings),
                    nameof(StorageSettings.AllowedToAccept),
                    new[] { typeof(Thing) });
                if (target == null)
                {
                    Log.Message("[StorageSettingsAllowedToAcceptFixPatch] StorageSettings.AllowedToAccept(Thing) not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    target,
                    prefix: new HarmonyMethod(
                        typeof(StorageSettings_AllowedToAccept_Patch),
                        nameof(StorageSettings_AllowedToAccept_Patch.Prefix)));

                Log.Message("[StorageSettingsAllowedToAcceptFixPatch] Patches applied.");
            }
            catch (System.Exception e)
            {
                Log.Error("[StorageSettingsAllowedToAcceptFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// Guards AllowedToAccept(Thing) against null thing or filter.
    ///
    /// Страхует AllowedToAccept(Thing) от null thing или filter.
    /// </summary>
    public static class StorageSettings_AllowedToAccept_Patch
    {
        public static bool Prefix(StorageSettings __instance, Thing t, ref bool __result)
        {
            if (!KebabTweaksSettings.EnableStorageSettingsAllowedToAcceptFix)
            {
                return true;
            }

            if (t == null || __instance.filter == null)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
