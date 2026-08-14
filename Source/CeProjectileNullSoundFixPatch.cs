using System;
using HarmonyLib;
using Verse;
using Verse.Sound;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Combat Extended ProjectileCE.ImpactSomething calls SoundStarter.PlayOneShot with a
    /// null SoundDef when a projectile or ammo def has no impact sound assigned. Vanilla logs
    /// "Tried to PlayOneShot with null SoundDef" each impact (often ProjectileCE_Explosive).
    ///
    /// Fix: Harmony Prefix on SoundStarter.PlayOneShot skips the call when def is null and this
    /// fix is enabled. Vanilla always; no CE assembly reference.
    ///
    /// Проблема: Combat Extended ProjectileCE.ImpactSomething вызывает SoundStarter.PlayOneShot с
    /// null SoundDef, если у снаряда/патрона не задан звук попадания. Vanilla пишет
    /// «Tried to PlayOneShot with null SoundDef» на каждое попадание (часто ProjectileCE_Explosive).
    ///
    /// Исправление: Harmony Prefix на SoundStarter.PlayOneShot пропускает вызов при null def и
    /// включённом фиксе. Только vanilla; без ссылки на CE.
    /// </summary>
    public static class CeProjectileNullSoundFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    AccessTools.Method(typeof(SoundStarter), nameof(SoundStarter.PlayOneShot),
                        new[] { typeof(SoundDef), typeof(SoundInfo) }),
                    prefix: new HarmonyMethod(
                        typeof(SoundStarter_PlayOneShot_NullSoundDef_Patch),
                        nameof(SoundStarter_PlayOneShot_NullSoundDef_Patch.Prefix)));

                Log.Message("[CeProjectileNullSoundFixPatch] Patches applied (null SoundDef PlayOneShot guard).");
            }
            catch (Exception e)
            {
                Log.Error("[CeProjectileNullSoundFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// Skips PlayOneShot when SoundDef is null and the fix is enabled.
    ///
    /// Пропускает PlayOneShot при null SoundDef и включённом фиксе.
    /// </summary>
    [HarmonyPatch(typeof(SoundStarter), nameof(SoundStarter.PlayOneShot),
        new[] { typeof(SoundDef), typeof(SoundInfo) })]
    public static class SoundStarter_PlayOneShot_NullSoundDef_Patch
    {
        public static bool Prefix(SoundDef soundDef)
        {
            if (soundDef != null)
            {
                return true;
            }

            return !KebabTweaksSettings.EnableCeProjectileNullSoundFix;
        }
    }
}
