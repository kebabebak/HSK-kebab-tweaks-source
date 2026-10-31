using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Building_TurretGunCE.BeginBurst spawns the shell when the warmup count reaches zero.
    /// That warmup branch does not read holdFire. CompMannable.MannedNow stays true for one tick after
    /// the ManTurret job ends. Root.Update runs those ticks before Root.OnGUI reads the click, so undraft
    /// or Hold fire still releases the shell that was already aiming.
    ///
    /// Fix: a prefix on BeginBurst, queued with LongEventHandler.QueueLongEvent. Patching BeginBurst on
    /// the play-data thread runs Building_TurretGunCE's static constructor off the main thread, and the
    /// target line material stays null. The original has to be skipped on a refused shot: a postfix
    /// would already have spawned the shell. HarmonyPriority Last lets earlier prefixes run first.
    /// While this fix is off, the prefix returns true. Hold fire, or a still-manned gun whose pawn is
    /// no longer on ManTurret, calls ResetCurrentTarget and skips the shot. The forced target stays.
    /// A manned gun whose pawn is still on ManTurret waits until the next frame and keeps one warmup
    /// tick, so this frame does not start a new aim. An unmanned gun fires on the same tick. A pawn
    /// who is still on ManTurret, including a siege pawn, fires on the next frame. The hook is installed
    /// only when CETeam.CombatExtended is active and the type is loaded from that mod. While
    /// SK.TurretGunCEPatches already refuses the shot, waits for the next frame, and queues the detour,
    /// this fix is leftover and stays off unless the Obsolete section enables it.
    ///
    /// Проблема: Building_TurretGunCE.BeginBurst выпускает снаряд, когда счётчик прицела доходит до нуля.
    /// Ветка прицела не читает holdFire. CompMannable.MannedNow остаётся истинным один тик после конца
    /// работы ManTurret. Root.Update выполняет эти тики раньше, чем Root.OnGUI читает клик, поэтому снятие
    /// призыва или «Остановить огонь» всё ещё выпускает снаряд, который уже целился.
    ///
    /// Исправление: prefix на BeginBurst, поставленный через LongEventHandler.QueueLongEvent. Патч
    /// BeginBurst в потоке загрузки проигрывает статический конструктор Building_TurretGunCE вне главного
    /// потока, и материал линии цели остаётся пустым. На отказанном выстреле оригинал нужно пропустить:
    /// postfix уже выпустил бы снаряд. HarmonyPriority Last даёт более ранним prefix отработать первыми.
    /// Пока этот фикс выключен, prefix возвращает true. Hold fire или всё ещё занятое орудие, чья пешка
    /// уже не на ManTurret, зовёт ResetCurrentTarget и пропускает выстрел. Принудительная цель остаётся.
    /// Занятое орудие, чья пешка всё ещё на ManTurret, ждёт следующего кадра и держит один тик прицела,
    /// чтобы этот кадр не начал новый прицел. Орудие без расчёта стреляет в том же тике. Пешка, которая
    /// всё ещё на ManTurret, в том числе осадная, стреляет на следующем кадре. Хук ставится только если
    /// активен CETeam.CombatExtended и тип загружен из папки этого мода. Пока SK.TurretGunCEPatches уже
    /// отказывает в выстреле, ждёт следующего кадра и ставит детур в очередь, этот фикс leftover и остаётся
    /// выключенным, пока раздел «Устаревшие» его не включит.
    /// </summary>
    public static class TurretUndraftLastShotFixFeatures
    {
        public const string PackageId = "CETeam.CombatExtended";

        static FieldInfo holdFireField;
        static FieldInfo burstWarmupTicksLeftField;
        static MethodInfo resetCurrentTarget;
        static bool patched;
        static readonly ConditionalWeakTable<Building_Turret, ReleaseFrame> releaseOnFrame =
            new ConditionalWeakTable<Building_Turret, ReleaseFrame>();

        public static void Apply(Harmony harmony)
        {
            if (!ModsConfig.IsActive(PackageId))
            {
                FeatureAvailability.SkipMissingMods(
                    FeatureAvailability.TurretUndraftLastShot,
                    "Turret Undraft Last Shot Fix",
                    "Combat Extended (CETeam.CombatExtended)");
                return;
            }

            Type type = AccessTools.TypeByName("CombatExtended.Building_TurretGunCE");
            MethodInfo method = type == null ? null : AccessTools.Method(type, "BeginBurst");
            if (method == null || !AssemblyIsThisMod(type))
            {
                FeatureAvailability.SkipHandled(
                    FeatureAvailability.TurretUndraftLastShot,
                    "Turret Undraft Last Shot Fix",
                    "Building_TurretGunCE.BeginBurst is not from CETeam.CombatExtended.");
                return;
            }

            holdFireField = AccessTools.Field(type, "holdFire");
            burstWarmupTicksLeftField = AccessTools.Field(type, "burstWarmupTicksLeft");
            resetCurrentTarget = AccessTools.Method(type, "ResetCurrentTarget");
            if (holdFireField == null || burstWarmupTicksLeftField == null || resetCurrentTarget == null)
            {
                FeatureAvailability.SkipHandled(
                    FeatureAvailability.TurretUndraftLastShot,
                    "Turret Undraft Last Shot Fix",
                    "Building_TurretGunCE is missing holdFire, burstWarmupTicksLeft, or ResetCurrentTarget.");
                return;
            }

            LongEventHandler.QueueLongEvent(() => Patch(harmony, method), null, false, null);
        }

        static void Patch(Harmony harmony, MethodInfo method)
        {
            if (patched || harmony == null || method == null)
            {
                return;
            }

            try
            {
                harmony.Patch(
                    method,
                    prefix: new HarmonyMethod(
                        typeof(TurretUndraftLastShotFixFeatures),
                        nameof(BeginBurst_Prefix)));
                patched = true;
                Log.Message("[HSK kebab tweaks] TurretUndraftLastShot loaded.");
            }
            catch (Exception ex)
            {
                FeatureAvailability.WarnOnce(
                    "turret-last-shot",
                    "[HSK kebab tweaks] Turret undraft last-shot patch failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Skips the shell when the aim already finished but the manning order or Hold fire says not to fire.
        /// Returns true while the fix is off so the original and any other prefix still run.
        ///
        /// Пропускает снаряд, когда прицел уже дошёл, а приказ обслуживания или Hold fire стрелять не велят.
        /// Возвращает true, пока фикс выключен, чтобы оригинал и любой другой prefix всё ещё выполнились.
        /// </summary>
        [HarmonyPriority(Priority.Last)]
        public static bool BeginBurst_Prefix(Building_Turret __instance)
        {
            if (!KebabTweaksSettings.IsTurretUndraftLastShotFixEnabled() || __instance == null)
            {
                return true;
            }

            try
            {
                bool holdFire = holdFireField.GetValue(__instance) is bool held && held;
                CompMannable mannable = __instance.GetComp<CompMannable>();
                bool manned = mannable != null && mannable.MannedNow;
                if (holdFire || (manned && mannable.ManningPawn?.CurJobDef != JobDefOf.ManTurret))
                {
                    ClearRelease(__instance);
                    resetCurrentTarget.Invoke(__instance, null);
                    return false;
                }

                if (!manned)
                {
                    ClearRelease(__instance);
                    return true;
                }

                int frame = Time.frameCount;
                ReleaseFrame release = releaseOnFrame.GetOrCreateValue(__instance);
                if (release.frame == frame)
                {
                    release.frame = -1;
                    return true;
                }

                release.frame = frame + 1;
                burstWarmupTicksLeftField.SetValue(__instance, 1);
                return false;
            }
            catch (Exception ex)
            {
                FeatureAvailability.WarnOnce(
                    "turret-last-shot",
                    "[HSK kebab tweaks] Turret undraft last-shot check failed: " + ex.Message);
                return true;
            }
        }

        static void ClearRelease(Building_Turret turret)
        {
            if (releaseOnFrame.TryGetValue(turret, out ReleaseFrame release))
            {
                release.frame = -1;
            }
        }

        /// <summary>
        /// True when this type was loaded from the CETeam.CombatExtended mod folder.
        ///
        /// Истина, если этот тип загружен из папки мода CETeam.CombatExtended.
        /// </summary>
        static bool AssemblyIsThisMod(Type type)
        {
            if (type == null)
            {
                return false;
            }

            string location = type.Assembly.Location;
            if (location.NullOrEmpty())
            {
                return false;
            }

            ModMetaData meta = ModLister.GetActiveModWithIdentifier(PackageId);
            DirectoryInfo rootDir = meta?.RootDir;
            if (rootDir == null)
            {
                return false;
            }

            string root = Path.GetFullPath(rootDir.FullName)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string file = Path.GetFullPath(location);
            if (file.Length < root.Length
                || !file.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return file.Length == root.Length
                || file[root.Length] == Path.DirectorySeparatorChar
                || file[root.Length] == Path.AltDirectorySeparatorChar;
        }

        sealed class ReleaseFrame
        {
            public int frame = -1;
        }
    }
}
