using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using SK;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Runtime leftover vs current for probe-capable Obsolete candidates. Reads known defs or
    /// original method IL once per launch. Closed → Obsolete default off. Alive or Unknown →
    /// current, do not force-disable.
    ///
    /// Рантайм leftover / current для фиксов, которые могут устареть. Читает известные дефы
    /// или IL оригинала один раз за запуск. Closed → Устаревшие, default off. Alive или
    /// Unknown → актуальные, принудительно не гасить.
    /// </summary>
    public static class FixSymptomProbe
    {
        const float BreachAxeMissingWorkToMakeMax = 1.01f;

        static bool catCrazyTimeProbed;
        static bool catCrazyTimeLeftover;
#if RIMWORLD_1_6
        static bool defsProbed;
        static bool burnWeaponLeftover;
        static bool breachAxeLeftover;
#endif
        static bool logged;

        /// <summary>
        /// Refreshes leftover flags when defs exist. Idempotent per launch.
        ///
        /// Обновляет leftover-флаги, когда дефы есть. Повторные вызовы за запуск безвредны.
        /// </summary>
        public static void Ensure()
        {
            ProbeCatCrazyTimeIfNeeded();
#if RIMWORLD_1_6
            ProbeDefBackedFixesIfNeeded();
#endif
            LogOnce();
        }

        public static bool IsCatCrazyTimeLeftover()
        {
            Ensure();
            return catCrazyTimeLeftover;
        }

#if RIMWORLD_1_6
        public static bool IsBurnWeaponBillFixLeftover()
        {
            Ensure();
            return burnWeaponLeftover;
        }

        public static bool IsBreachAxeWorkAmountFixLeftover()
        {
            Ensure();
            return breachAxeLeftover;
        }
#endif

        public static bool DefBackedProbesReady
        {
            get
            {
#if RIMWORLD_1_6
                return defsProbed;
#else
                return true;
#endif
            }
        }

        public static bool HasAnyLeftover()
        {
            if (IsCatCrazyTimeLeftover())
            {
                return true;
            }

#if RIMWORLD_1_6
            if (IsBurnWeaponBillFixLeftover() || IsBreachAxeWorkAmountFixLeftover())
            {
                return true;
            }
#endif
            return false;
        }

        static void ProbeCatCrazyTimeIfNeeded()
        {
            if (catCrazyTimeProbed)
            {
                return;
            }

            catCrazyTimeProbed = true;
            catCrazyTimeLeftover = ProbeCatCrazyTimeClosed();
        }

        /// <summary>
        /// Closed when SK no longer re-rolls CrazyTime num on toil rebuild. Alive when
        /// MakeNewToils still calls Rand.RangeInclusive.
        ///
        /// Closed, если SK больше не бросает num при пересборке toils. Alive, если
        /// MakeNewToils всё ещё зовёт Rand.RangeInclusive.
        /// </summary>
        static bool ProbeCatCrazyTimeClosed()
        {
            Type driver = typeof(JobDriver_CrazyTime);
            if (IteratorCallsRangeInclusive(driver))
            {
                return false;
            }

            MethodInfo tryMake = AccessTools.Method(driver, "TryMakePreToilReservations");
            if (tryMake == null)
            {
                return false;
            }

            if (!MethodCalls(tryMake, typeof(Rand), "RangeInclusive"))
            {
                return true;
            }

            FieldInfo numField = AccessTools.Field(driver, "num");
            return numField != null && MethodLoadsField(tryMake, numField);
        }

        static bool IteratorCallsRangeInclusive(Type driver)
        {
            foreach (Type nested in driver.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (nested == null || nested.Name == null ||
                    nested.Name.IndexOf("MakeNewToils", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                MethodInfo moveNext = AccessTools.Method(nested, "MoveNext");
                if (MethodCalls(moveNext, typeof(Rand), "RangeInclusive"))
                {
                    return true;
                }
            }

            MethodInfo makeNewToils = AccessTools.Method(driver, "MakeNewToils");
            return MethodCalls(makeNewToils, typeof(Rand), "RangeInclusive");
        }

#if RIMWORLD_1_6
        static void ProbeDefBackedFixesIfNeeded()
        {
            if (defsProbed)
            {
                return;
            }

            if (DefDatabase<ThingDef>.DefCount == 0 || DefDatabase<RecipeDef>.DefCount == 0)
            {
                return;
            }

            defsProbed = true;
            BurnWeaponBillFixFeatures.EnsureOriginalsCaptured();
            burnWeaponLeftover = ProbeBurnWeaponClosed();
            BreachAxeWorkAmountFixFeatures.EnsureOriginalsCaptured();
            breachAxeLeftover = ProbeBreachAxeClosed();
        }

        /// <summary>
        /// Closed when each burn recipe either has no requiredGiverWorkType or a DoBill WorkGiver
        /// with that work type covers a recipe user (haulers can start the bill). Alive when a
        /// required type is set and no matching DoBill remains.
        ///
        /// Closed, если у каждого рецепта сжигания нет requiredGiverWorkType либо есть DoBill с
        /// этим work type на пользователе рецепта (грузчик может взять задание). Alive, если тип
        /// задан и подходящего DoBill нет.
        /// </summary>
        static bool ProbeBurnWeaponClosed()
        {
            string[] names =
            {
                "BurnWeapon",
                "BurnApparel",
                "BurnDrugs",
            };
            bool sawRecipe = false;
            foreach (string defName in names)
            {
                RecipeDef recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(defName);
                if (recipe == null)
                {
                    continue;
                }

                sawRecipe = true;
                WorkTypeDef required = BurnWeaponBillFixFeatures.GetCapturedRequiredGiver(defName);
                if (required == null)
                {
                    continue;
                }

                if (!HasMatchingDoBill(recipe, required))
                {
                    return false;
                }
            }

            return sawRecipe;
        }

        static bool HasMatchingDoBill(RecipeDef recipe, WorkTypeDef required)
        {
            foreach (ThingDef user in recipe.AllRecipeUsers)
            {
                if (user == null)
                {
                    continue;
                }

                foreach (WorkGiverDef wg in DefDatabase<WorkGiverDef>.AllDefsListForReading)
                {
                    if (wg == null || wg.giverClass == null || wg.workType != required)
                    {
                        continue;
                    }

                    if (!typeof(WorkGiver_DoBill).IsAssignableFrom(wg.giverClass))
                    {
                        continue;
                    }

                    if (WorkGiverCoversBuilding(wg, user))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool WorkGiverCoversBuilding(WorkGiverDef wg, ThingDef building)
        {
            if (wg.fixedBillGiverDefs == null || wg.fixedBillGiverDefs.Count == 0)
            {
                return false;
            }

            return wg.fixedBillGiverDefs.Contains(building);
        }

        /// <summary>
        /// Closed when captured WorkToMake is above the vanilla missing-stat default of 1.
        ///
        /// Closed, если сохранённый WorkToMake выше ванильного default 1 при отсутствии стата.
        /// </summary>
        static bool ProbeBreachAxeClosed()
        {
            float thingWork;
            if (!BreachAxeWorkAmountFixFeatures.TryGetCapturedThingWorkToMake(out thingWork))
            {
                return false;
            }

            return thingWork > BreachAxeMissingWorkToMakeMax;
        }
#endif

        static void LogOnce()
        {
            if (logged)
            {
                return;
            }

#if RIMWORLD_1_6
            if (!catCrazyTimeProbed || !defsProbed)
            {
                return;
            }
#else
            if (!catCrazyTimeProbed)
            {
                return;
            }
#endif

            logged = true;
#if RIMWORLD_1_6
            Log.Message(
                "[HSK kebab tweaks] Symptom leftover: CatCrazyTime=" + catCrazyTimeLeftover +
                " BurnWeapon=" + burnWeaponLeftover +
                " BreachAxe=" + breachAxeLeftover + ".");
#else
            Log.Message(
                "[HSK kebab tweaks] Symptom leftover: CatCrazyTime=" + catCrazyTimeLeftover + ".");
#endif
        }

        static bool MethodCalls(MethodInfo method, Type declaringType, string methodName)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || declaringType == null || methodName == null)
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                byte op = il[i];
                if (op != 0x28 && op != 0x6F)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    MethodBase resolved = module.ResolveMethod(token);
                    if (resolved != null && resolved.Name == methodName &&
                        resolved.DeclaringType == declaringType)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        static bool MethodLoadsField(MethodInfo method, FieldInfo field)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || field == null)
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] != 0x7B)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    FieldInfo resolved = module.ResolveField(token);
                    if (resolved != null && resolved == field)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }
    }
}
