using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: SK.Rimatomic_BetterHEL.TryFindNewTarget runs after Rimatomics.Building_HEL.TryFindNewTarget.
    /// When the laser has no mortar shell, the HSK postfix walks ListerThings.AllThings and allocates a LINQ
    /// check per thing. Building_EnergyWeapon.TickGuns calls that search every 10 ticks while the laser is
    /// powered, a weapons console on the same power net is on, and a radar is working. The colony stays slow
    /// until the laser is switched off. The scan is only there to notice MeteorIncoming and RazorRainIncoming.
    ///
    /// Fix: While this fix is on, a prefix skips the HSK postfix body, and a postfix on Building_HEL.TryFindNewTarget
    /// checks only those two defs. A postfix alone cannot stop the HSK method: Harmony calls it on its own.
    /// Soft-skips when Rimatomics is absent. When the loaded HSK method does not call ListerThings.get_AllThings,
    /// the row is leftover and stays off unless the Obsolete section is on.
    ///
    /// Проблема: SK.Rimatomic_BetterHEL.TryFindNewTarget выполняется после Rimatomics.Building_HEL.TryFindNewTarget.
    /// Когда у лазера нет миномётного снаряда, postfix HSK обходит ListerThings.AllThings и на каждую вещь
    /// выделяет проверку LINQ. Building_EnergyWeapon.TickGuns зовёт этот поиск каждые 10 тиков, пока лазер
    /// включён, на той же сети включена оружейная консоль и работает радар. Колония остаётся медленной, пока
    /// лазер не выключить. Обход нужен только чтобы заметить MeteorIncoming и RazorRainIncoming.
    ///
    /// Исправление: Пока фикс включён, prefix пропускает тело postfix HSK, а postfix на Building_HEL.TryFindNewTarget
    /// смотрит только эти два дефа. Одного postfix недостаточно: Harmony вызывает метод HSK отдельно.
    /// Без Rimatomics патч пропускается. Если загруженный метод HSK не вызывает ListerThings.get_AllThings,
    /// строка устаревшая и остаётся выключенной, пока не включён раздел «Устаревшие».
    /// </summary>
    public static class RimatomicsHelFpsFixFeatures
    {
        static FieldInfo powerCompField;
        static FieldInfo helsField;
        static MethodInfo mapCompGetter;
        static MethodInfo rangeGetter;
        static MethodInfo rangeMinGetter;
        static MethodInfo powerNetGetter;
        static MethodInfo currentTargetGetter;
        static MethodInfo thingsOfDef;
        static MethodInfo lineOfSight;
        static ThingDef meteorDef;
        static ThingDef razorDef;
        static bool loggedMissingDefs;

        public static void Apply(Harmony harmony)
        {
            Type helType = AccessTools.TypeByName("Rimatomics.Building_HEL");
            if (helType == null)
            {
                FeatureAvailability.SkipMissingMods(
                    FeatureAvailability.RimatomicsHel,
                    "High-Energy Laser Frame Rate Fix",
                    "Rimatomics (Dubwise.Rimatomics)");
                return;
            }

            MethodInfo tryFind = AccessTools.Method(helType, "TryFindNewTarget");
            if (tryFind == null)
            {
                FeatureAvailability.SkipHandled(
                    FeatureAvailability.RimatomicsHel,
                    "High-Energy Laser Frame Rate Fix",
                    "Rimatomics Building_HEL.TryFindNewTarget was not found.");
                return;
            }

            if (!ResolveHelpers())
            {
                FeatureAvailability.SkipHandled(
                    FeatureAvailability.RimatomicsHel,
                    "High-Energy Laser Frame Rate Fix",
                    "Rimatomics HEL target helpers were not found.");
                return;
            }

            harmony.Patch(
                tryFind,
                postfix: new HarmonyMethod(typeof(RimatomicsHelFpsFixFeatures), nameof(TryFindNewTarget_Postfix)));

            Type better = AccessTools.TypeByName("SK.Rimatomic_BetterHEL");
            MethodInfo betterMethod = better == null ? null : AccessTools.Method(better, "TryFindNewTarget");
            if (betterMethod != null)
            {
                harmony.Patch(
                    betterMethod,
                    prefix: new HarmonyMethod(typeof(RimatomicsHelFpsFixFeatures), nameof(SkipAllThingsScan)));
            }

            Log.Message("[HSK kebab tweaks] High-energy laser frame rate fix loaded.");
        }

        /// <summary>
        /// Prefix on SK.Rimatomic_BetterHEL.TryFindNewTarget. Returns false while this fix is on so that
        /// method does not walk ListerThings.AllThings. A postfix on Building_HEL.TryFindNewTarget cannot
        /// stop it: Harmony calls the HSK postfix on its own.
        ///
        /// Prefix на SK.Rimatomic_BetterHEL.TryFindNewTarget. Пока фикс включён, возвращает false, чтобы
        /// этот метод не обходил ListerThings.AllThings. Postfix на Building_HEL.TryFindNewTarget это не
        /// останавливает: Harmony вызывает postfix HSK отдельно.
        /// </summary>
        public static bool SkipAllThingsScan()
        {
            return !KebabTweaksSettings.IsRimatomicsHelFpsFixEnabled();
        }

        /// <summary>
        /// After the Rimatomics search, picks MeteorIncoming or RazorRainIncoming from ThingsOfDef
        /// when the laser still has no target.
        ///
        /// После поиска Rimatomics берёт MeteorIncoming или RazorRainIncoming через ThingsOfDef,
        /// если у лазера всё ещё нет цели.
        /// </summary>
        public static void TryFindNewTarget_Postfix(Thing __instance, ref LocalTargetInfo __result)
        {
            if (!KebabTweaksSettings.IsRimatomicsHelFpsFixEnabled() || __instance == null || __result.IsValid)
            {
                return;
            }

            object mapComp = mapCompGetter.Invoke(__instance, null);
            IList hels = mapComp == null ? null : helsField.GetValue(mapComp) as IList;
            object powerNet = PowerNetOf(__instance);
            float min = (float)rangeMinGetter.Invoke(__instance, null);
            float max = (float)rangeGetter.Invoke(__instance, null);
            FloatRange range = new FloatRange(min, max);
            if (TryFindTarget(__instance, ThingsOf(__instance.Map, Meteor()), hels, powerNet, range, out Thing target)
                || TryFindTarget(__instance, ThingsOf(__instance.Map, Razor()), hels, powerNet, range, out target))
            {
                __result = target;
            }
        }

        /// <summary>
        /// First skyfaller in this list that this power net is not already shooting and that the laser can see.
        ///
        /// Первый скайфоллер из этого списка, по которому эта сеть ещё не стреляет и которого лазер видит.
        /// </summary>
        static bool TryFindTarget(
            Thing hel,
            List<Thing> candidates,
            IList hels,
            object powerNet,
            FloatRange range,
            out Thing target)
        {
            target = null;
            if (candidates == null)
            {
                return false;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                Thing candidate = candidates[i];
                if (candidate == null || IsTargetedByNetwork(hels, powerNet, candidate))
                {
                    continue;
                }

                object seen = lineOfSight.Invoke(null, new object[]
                {
                    candidate.Position,
                    candidate.DrawPos.ToIntVec3(),
                    hel.Position,
                    hel.Map,
                    range,
                    20,
                    false
                });
                if (seen is bool ok && ok)
                {
                    target = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// True when another laser on the same power net already has this thing as its current target.
        ///
        /// Истина, если другой лазер на той же сети уже держит эту вещь текущей целью.
        /// </summary>
        static bool IsTargetedByNetwork(IList hels, object powerNet, Thing candidate)
        {
            if (hels == null)
            {
                return false;
            }

            for (int i = 0; i < hels.Count; i++)
            {
                object other = hels[i];
                if (other == null || PowerNetOf(other) != powerNet)
                {
                    continue;
                }

                object raw = currentTargetGetter.Invoke(other, null);
                if (raw is LocalTargetInfo current && current.Thing == candidate)
                {
                    return true;
                }
            }

            return false;
        }

        static List<Thing> ThingsOf(Map map, ThingDef def)
        {
            if (map == null || def == null)
            {
                return null;
            }

            return thingsOfDef.Invoke(map.listerThings, new object[] { def }) as List<Thing>;
        }

        static object PowerNetOf(object weapon)
        {
            object comp = powerCompField.GetValue(weapon);
            if (comp == null)
            {
                return null;
            }

            return powerNetGetter.Invoke(comp, null);
        }

        static ThingDef Meteor()
        {
            if (meteorDef == null)
            {
                meteorDef = DefDatabase<ThingDef>.GetNamedSilentFail("MeteorIncoming");
            }

            return meteorDef;
        }

        static ThingDef Razor()
        {
            if (razorDef == null)
            {
                razorDef = DefDatabase<ThingDef>.GetNamedSilentFail("RazorRainIncoming");
            }

            if (meteorDef == null && razorDef == null && !loggedMissingDefs)
            {
                loggedMissingDefs = true;
                Log.Warning("[HSK kebab tweaks] High-energy laser frame rate fix: MeteorIncoming and RazorRainIncoming were not found.");
            }

            return razorDef;
        }

        static bool ResolveHelpers()
        {
            Type energy = AccessTools.TypeByName("Rimatomics.Building_EnergyWeapon");
            Type mapCompType = AccessTools.TypeByName("Rimatomics.MapComponent_Rimatomics");
            Type sight = AccessTools.TypeByName("Rimatomics.DubSight");
            if (energy == null || mapCompType == null || sight == null)
            {
                return false;
            }

            powerCompField = AccessTools.Field(energy, "powerComp");
            mapCompGetter = AccessTools.PropertyGetter(energy, "MapComp");
            rangeGetter = AccessTools.PropertyGetter(energy, "Range");
            rangeMinGetter = AccessTools.PropertyGetter(energy, "RangeMin");
            helsField = AccessTools.Field(mapCompType, "HELS");
            powerNetGetter = AccessTools.PropertyGetter(typeof(CompPower), nameof(CompPower.PowerNet));
            currentTargetGetter = AccessTools.PropertyGetter(typeof(Building_Turret), nameof(Building_Turret.CurrentTarget));
            thingsOfDef = AccessTools.Method(typeof(ListerThings), nameof(ListerThings.ThingsOfDef));
            lineOfSight = AccessTools.Method(sight, "LineOfSightProjectile", new[]
            {
                typeof(IntVec3),
                typeof(IntVec3),
                typeof(IntVec3),
                typeof(Map),
                typeof(FloatRange),
                typeof(int),
                typeof(bool)
            });
            return powerCompField != null
                && mapCompGetter != null
                && rangeGetter != null
                && rangeMinGetter != null
                && helsField != null
                && powerNetGetter != null
                && currentTargetGetter != null
                && thingsOfDef != null
                && lineOfSight != null;
        }
    }
}
