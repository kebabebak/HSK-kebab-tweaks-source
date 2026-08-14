#if RIMWORLD_1_6
using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Tribal_ChiefMelee_Neanderthal leaders fail PawnGenerator with incapable Melee
    /// (CE skill floor on Tribal_ChiefMelee inherited via TribalChiefBase) then
    /// Faction.TryGenerateNewLeader NREs on null leader during worldgen.
    ///
    /// Fix: Shippable 1.6 XML removes inherited skills on Neanderthal chief kinds. Finalizer on
    /// TryGenerateNewLeader swallows NullReferenceException when enabled so WorldGenStep_Factions
    /// can continue.
    ///
    /// Проблема: Лидеры Tribal_ChiefMelee_Neanderthal падают в PawnGenerator (Melee incapable),
    /// CE skill floor на Tribal_ChiefMelee наследуется через TribalChiefBase; TryGenerateNewLeader
    /// даёт NRE на null leader при worldgen.
    ///
    /// Исправление: XML 1.6 снимает skills у Neanderthal chiefs. Finalizer на TryGenerateNewLeader
    /// глотает NullReferenceException при включённом фиксе.
    /// </summary>
    public static class NeanderthalChiefLeaderFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Faction), nameof(Faction.TryGenerateNewLeader)),
                finalizer: new HarmonyMethod(
                    typeof(NeanderthalChiefLeaderFixFeatures),
                    nameof(TryGenerateNewLeader_Finalizer)));
        }

        private static bool _logged;

        private static Exception TryGenerateNewLeader_Finalizer(Exception __exception)
        {
            if (!KebabTweaksSettings.EnableNeanderthalChiefLeaderFix)
            {
                return __exception;
            }

            if (__exception is NullReferenceException)
            {
                if (!_logged)
                {
                    _logged = true;
                    Log.Warning(
                        "[HSK kebab tweaks] Neanderthal chief leader fix swallowed NullReferenceException "
                        + "in Faction.TryGenerateNewLeader (leader pawn generation returned null).");
                }

                return null;
            }

            return __exception;
        }
    }
}
#endif
