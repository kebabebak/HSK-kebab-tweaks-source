using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: a right-click manning order creates JobDefOf.ManTurret with no expiryInterval.
    /// JobDriver_ManTurret finishes on a toil whose complete mode is Never. Combat Extended
    /// reload puts the pawn back on that same job. An undrafted player colonist therefore
    /// stays at the gun after the gun's own target check finds nothing to shoot.
    ///
    /// Fix: a postfix on Building_TurretGunCE.TryStartShootSomething. The instance parameter is
    /// Building_Turret: that gun extends Building_Turret, not Building_TurretGun, and Harmony
    /// rejects an instance type that is not a base of the target. The hook is installed
    /// when CETeam.CombatExtended is active. While the header checkbox is off, the postfix
    /// returns and the gun behaves as before. While it is on, an
    /// undrafted player colonist (IsColonist, so a colony slave is included and a raider
    /// is not) whose current job is ManTurret on that gun counts ticks when that call ends
    /// with no current target, no forced target, and no world target, and the call was not
    /// hold fire, not a reload, and not a roof that blocks an overhead shell. A call that stores a target, including a new target after the
    /// previous one was destroyed, zeroes the count. The count also stays zero during the
    /// burst and the cooldown, because this method is not called then; the next search is
    /// the moment the count can start. At CombatExtendedLeaveGunTicks the job ends with
    /// InterruptForced and the colonist can take other work. The count is cleared while the
    /// job is not ManTurret, so a Combat Extended reload can still restore it, and
    /// CompSuppressable.AddSuppression can still resume it: that StartJob passes
    /// resumeCurJobAfterwards when the current job is ManTurret, and this postfix does not
    /// end RunForCover or HunkerDown. Pawn_HealthTracker.MakeDowned already calls
    /// ClearMind_NewTemp for a pawn who is not on a downed duty and is not a mutant allowed
    /// to attack while crawling, which drops ManTurret without starting it again. While
    /// Downed this postfix does not end the job. The hook is installed only when ModsConfig
    /// lists CETeam.CombatExtended and Building_TurretGunCE is loaded from that mod folder.
    ///
    /// Проблема: приказ правым кликом создаёт JobDefOf.ManTurret без expiryInterval.
    /// JobDriver_ManTurret заканчивается тойлом с режимом Never. Перезарядка Combat Extended
    /// возвращает пешку на ту же работу. Колонист игрока без призыва поэтому остаётся за
    /// орудием после того, как собственная проверка орудия никого не находит.
    ///
    /// Исправление: postfix на Building_TurretGunCE.TryStartShootSomething. Параметр экземпляра -
    /// Building_Turret: это орудие наследует Building_Turret, не Building_TurretGun, и Harmony
    /// отвергает тип экземпляра, который не является базовым для цели. Хук ставится,
    /// когда активен CETeam.CombatExtended. Пока выключен заголовок, postfix выходит и
    /// орудие ведёт себя как раньше. Пока он включён, колонист игрока
    /// без призыва (IsColonist: раб колонии входит, рейдер нет), чья текущая работа -
    /// ManTurret на этом орудии, копит тики, когда вызов закончился без текущей цели, без
    /// принудительной цели и без цели на мировой карте, и это был не hold fire, не
    /// перезарядка и не крыша, которая блокирует навесной снаряд. Вызов, который записал цель, в том числе новую после
    /// уничтожения предыдущей, обнуляет счёт. Счёт также остаётся нулём во время очереди и
    /// кулдауна, потому что этот метод тогда не вызывается; следующий поиск - момент, с
    /// которого счёт может начаться. На CombatExtendedLeaveGunTicks работа кончается через
    /// InterruptForced, и колонист может взять другую. Счёт сбрасывается, пока работа не
    /// ManTurret, поэтому перезарядка Combat Extended по-прежнему может её вернуть, и
    /// CompSuppressable.AddSuppression по-прежнему может её возобновить: этот StartJob
    /// передаёт resumeCurJobAfterwards, когда текущая работа - ManTurret, а этот postfix не
    /// кончает RunForCover и HunkerDown. Pawn_HealthTracker.MakeDowned уже зовёт
    /// ClearMind_NewTemp для пешки, которая не на должности «лёжа» и не мутант, которому
    /// разрешено атаковать ползком; это снимает ManTurret и не запускает её снова. Пока
    /// пешка Downed, этот postfix работу не кончает. Хук ставится только если в ModsConfig
    /// есть CETeam.CombatExtended и Building_TurretGunCE загружен из папки этого мода.
    /// </summary>
    public static class CombatExtendedLeaveGunFeatures
    {
        public const string PackageId = "CETeam.CombatExtended";

        private static FieldInfo holdFireField;
        private static FieldInfo isReloadingField;
        private static FieldInfo targetingWorldMapField;
        private static FieldInfo globalTargetInfoField;
        private static FieldInfo forcedTargetField;
        private static PropertyInfo globalTargetIsValidProperty;
        private static PropertyInfo compAmmoProperty;
        private static PropertyInfo projectileProperty;
        private static PropertyInfo canBeFiredNowProperty;
        private static MethodInfo canBeFiredNowMethod;
        private static readonly ConditionalWeakTable<Building_Turret, IdleMark> IdleByTurret =
            new ConditionalWeakTable<Building_Turret, IdleMark>();

        public static void Apply(Harmony harmony)
        {
            if (!IsPackageActive())
            {
                FeatureAvailability.SkipMissingMods(
                    FeatureAvailability.CombatExtendedLeaveGun,
                    "ComP: Combat Extended",
                    "Combat Extended (CETeam.CombatExtended)");
                return;
            }

            Type type = AccessTools.TypeByName("CombatExtended.Building_TurretGunCE");
            MethodInfo method = type == null
                ? null
                : AccessTools.Method(type, "TryStartShootSomething", new[] { typeof(bool) });
            if (method == null || !AssemblyIsThisMod(type))
            {
                FeatureAvailability.SkipHandled(
                    FeatureAvailability.CombatExtendedLeaveGun,
                    "ComP: Combat Extended",
                    "Building_TurretGunCE.TryStartShootSomething is not from CETeam.CombatExtended.");
                return;
            }

            holdFireField = AccessTools.Field(type, "holdFire");
            isReloadingField = AccessTools.Field(type, "isReloading");
            targetingWorldMapField = AccessTools.Field(type, "targetingWorldMap");
            globalTargetInfoField = AccessTools.Field(type, "globalTargetInfo");
            forcedTargetField = AccessTools.Field(typeof(Building_Turret), "forcedTarget");
            compAmmoProperty = AccessTools.Property(type, "CompAmmo");
            projectileProperty = AccessTools.Property(type, "Projectile");
            globalTargetIsValidProperty = globalTargetInfoField?.FieldType.GetProperty("IsValid");
            if (holdFireField == null || isReloadingField == null || targetingWorldMapField == null
                || globalTargetInfoField == null || forcedTargetField == null
                || globalTargetIsValidProperty == null
                || compAmmoProperty == null || projectileProperty == null)
            {
                FeatureAvailability.SkipHandled(
                    FeatureAvailability.CombatExtendedLeaveGun,
                    "ComP: Combat Extended",
                    "Building_TurretGunCE is missing a target or hold-fire field.");
                return;
            }

            harmony.Patch(method, postfix: new HarmonyMethod(
                typeof(CombatExtendedLeaveGunFeatures),
                nameof(TryStartShootSomething_Postfix)));
            Log.Message("[HSK kebab tweaks] CombatExtendedLeaveGun loaded.");
        }

        /// <summary>
        /// ModsConfig compares package ids without case, so CETeam.CombatExtended matches the About packageId.
        ///
        /// ModsConfig сравнивает package id без учёта регистра, поэтому CETeam.CombatExtended совпадает с packageId из About.
        /// </summary>
        public static bool IsPackageActive()
        {
            return ModsConfig.IsActive(PackageId);
        }

        public static void TryStartShootSomething_Postfix(Building_Turret __instance)
        {
            try
            {
                Consider(__instance);
            }
            catch (Exception ex)
            {
                FeatureAvailability.WarnOnce(
                    "ce-leave-gun",
                    "[HSK kebab tweaks] Combat Extended leave-gun check failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Counts quiet ticks only after this search stored no target. A stored target zeroes the count.
        ///
        /// Считает тихие тики только после того, как этот поиск не записал цель. Записанная цель обнуляет счёт.
        /// </summary>
        private static void Consider(Building_Turret turret)
        {
            if (turret == null)
            {
                return;
            }

            if (!KebabTweaksSettings.EnableCombatExtendedCompat
                || !IsPackageActive())
            {
                ClearIdle(turret);
                return;
            }

            if (!UndraftedColonistManning(turret, out Pawn pawn))
            {
                ClearIdle(turret);
                return;
            }

            if (SearchDidNotFail(turret))
            {
                ClearIdle(turret);
                return;
            }

            int now = GenTicks.TicksGame;
            IdleMark mark = IdleByTurret.GetOrCreateValue(turret);

            if (mark.lastTick == now)
            {
                return;
            }

            if (mark.lastTick == now - 1)
            {
                mark.ticks++;
            }
            else
            {
                mark.ticks = 0;
            }

            mark.lastTick = now;
            int delay = Mathf.Clamp(
                KebabTweaksSettings.CombatExtendedLeaveGunTicks,
                KebabTweaksSettings.MinCombatExtendedLeaveGunTicks,
                KebabTweaksSettings.MaxCombatExtendedLeaveGunTicks);
            if (mark.ticks < delay)
            {
                return;
            }

            ClearIdle(turret);
            if (pawn.Downed || pawn.Dead || pawn.Drafted || pawn.CurJobDef != JobDefOf.ManTurret)
            {
                return;
            }

            if (pawn.CurJob.targetA.Thing != turret)
            {
                return;
            }

            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
        }

        /// <summary>
        /// True when this call kept a reason to stay: hold fire, reload, a roof over an overhead shell,
        /// or any target the search stored. A destroyed target that was replaced in this same call is a stored target.
        ///
        /// Истина, если у вызова осталась причина сидеть: hold fire, перезарядка, крыша над навесным снарядом
        /// или любая цель, которую поиск записал. Уничтоженная цель, заменённая в этом же вызове, - записанная цель.
        /// </summary>
        private static bool SearchDidNotFail(Building_Turret turret)
        {
            // Tick already clears holdFire when the gun has no hold-fire button.
            if (holdFireField.GetValue(turret) is bool holdFire && holdFire)
            {
                return true;
            }

            if (isReloadingField.GetValue(turret) is bool reloading && reloading)
            {
                return true;
            }

            if (!CanBeFiredNow(turret))
            {
                return true;
            }

            if (OverheadBlockedByRoof(turret))
            {
                return true;
            }

            if (ForcedTargetIsValid(turret) || turret.CurrentTarget.IsValid)
            {
                return true;
            }

            if (targetingWorldMapField.GetValue(turret) is bool world && world)
            {
                return true;
            }

            object global = globalTargetInfoField.GetValue(turret);
            return global != null && globalTargetIsValidProperty.GetValue(global, null) is bool valid && valid;
        }

        private static bool ForcedTargetIsValid(Building_Turret turret)
        {
            return forcedTargetField.GetValue(turret) is LocalTargetInfo forced && forced.IsValid;
        }

        private static bool UndraftedColonistManning(Building_Turret turret, out Pawn pawn)
        {
            pawn = null;
            CompMannable mannable = turret.GetComp<CompMannable>();
            if (mannable == null || !mannable.MannedNow)
            {
                return false;
            }

            pawn = mannable.ManningPawn;
            if (pawn == null || pawn.Downed || pawn.Dead || pawn.Drafted || !pawn.IsColonist)
            {
                return false;
            }

            return pawn.CurJobDef == JobDefOf.ManTurret && pawn.CurJob.targetA.Thing == turret;
        }

        /// <summary>
        /// False when the magazine cannot fire, which is the path that orders a reload before any target search.
        /// A gun with no ammo component can still search.
        ///
        /// Ложь, когда магазин не может выстрелить: это путь, который ставит перезарядку до любого поиска цели.
        /// Орудие без компонента патронов поиск всё ещё делает.
        /// </summary>
        private static bool CanBeFiredNow(Building_Turret turret)
        {
            object ammo = compAmmoProperty.GetValue(turret, null);
            if (ammo == null)
            {
                return true;
            }

            if (canBeFiredNowProperty == null || canBeFiredNowProperty.DeclaringType != ammo.GetType())
            {
                canBeFiredNowProperty = AccessTools.Property(ammo.GetType(), "CanBeFiredNow");
                canBeFiredNowMethod = canBeFiredNowProperty == null
                    ? AccessTools.Method(ammo.GetType(), "CanBeFiredNow")
                    : null;
            }

            if (canBeFiredNowProperty != null)
            {
                return canBeFiredNowProperty.GetValue(ammo, null) is bool canFire && canFire;
            }

            if (canBeFiredNowMethod == null)
            {
                return false;
            }

            return canBeFiredNowMethod.Invoke(ammo, null) is bool fired && fired;
        }

        private static bool OverheadBlockedByRoof(Building_Turret turret)
        {
            if (turret.Map == null)
            {
                return false;
            }

            ThingDef projectile = projectileProperty.GetValue(turret, null) as ThingDef;
            if (projectile?.projectile == null || !projectile.projectile.flyOverhead)
            {
                return false;
            }

            return turret.Map.roofGrid.Roofed(turret.Position);
        }

        private static void ClearIdle(Building_Turret turret)
        {
            if (IdleByTurret.TryGetValue(turret, out IdleMark mark))
            {
                mark.ticks = 0;
                mark.lastTick = -1;
            }
        }

        /// <summary>
        /// True when this type was loaded from the CETeam.CombatExtended mod folder.
        ///
        /// Истина, если этот тип загружен из папки мода CETeam.CombatExtended.
        /// </summary>
        private static bool AssemblyIsThisMod(Type type)
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

        private sealed class IdleMark
        {
            public int ticks;
            public int lastTick = -1;
        }
    }
}
