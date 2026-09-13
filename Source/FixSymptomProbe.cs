using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using SK;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Runtime leftover vs current for probe-capable Obsolete candidates, plus partial
    /// relevance for Craft Stuff / Fermenter Fill. Reads known defs or original method IL
    /// once per launch. Closed → Obsolete default off. Partial → current with a suffix.
    /// Alive or Unknown → current, no suffix, do not force-disable.
    ///
    /// Рантайм leftover / current для фиксов, которые могут устареть, плюс частичная
    /// актуальность Craft Stuff / Fermenter Fill. Читает известные дефы или IL оригинала
    /// один раз за запуск. Closed → Устаревшие, default off. Partial → актуальные с суффиксом.
    /// Alive или Unknown → актуальные без суффикса, принудительно не гасить.
    /// </summary>
    public static class FixSymptomProbe
    {
        const float BreachAxeMissingWorkToMakeMax = 1.01f;
        const int CraftStuffPartialRelevancePercent = 25;
        const int UfFillPartialRelevancePercent = 65;
        const string SkDominantIngredientPrefixTypeName =
            "SK.Patch_Toils_Recipe_CalculateDominantIngredient";
        const string UfFillJobDriverTypeName = "UniversalFermenterSK.JobDriver_FillUF";
        const string UfFillJobDriverTypeNameAlt = "UniversalFermenter.JobDriver_FillUF";
        const string UfFillAbortWithoutConsumingMarker =
            "Aborting job without consuming them";

        static bool catCrazyTimeProbed;
        static bool catCrazyTimeLeftover;
        static bool craftStuffProbed;
        static bool craftStuffLeftover;
        static bool craftStuffPartial;
#if RIMWORLD_1_6
        static bool defsProbed;
        static bool burnWeaponLeftover;
        static bool breachAxeLeftover;
        static bool rawFungusLeftover;
        static bool ufFillProbed;
        static bool ufFillLeftover;
        static bool ufFillPartial;
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
            ProbeCraftStuffIfNeeded();
#if RIMWORLD_1_6
            ProbeUfFillIfNeeded();
            ProbeDefBackedFixesIfNeeded();
#endif
            LogOnce();
        }

        public static bool IsCatCrazyTimeLeftover()
        {
            Ensure();
            return catCrazyTimeLeftover;
        }

        public static bool IsCraftStuffFixLeftover()
        {
            Ensure();
            return craftStuffLeftover;
        }

        /// <summary>
        /// Header suffix for Craft Stuff: 0 leftover, 25 when SK already filters CanMake but
        /// still picks random or ingredients[0], null when the SK Prefix is absent or has no
        /// CanMake (omit = 100%).
        ///
        /// Суффикс Craft Stuff: 0 leftover, 25 если SK уже фильтрует CanMake, но ещё берёт
        /// random или ingredients[0], null если Prefix SK нет или в нём нет CanMake (без
        /// суффикса = 100%).
        /// </summary>
        public static int? GetCraftStuffRelevancePercent()
        {
            Ensure();
            if (craftStuffLeftover)
            {
                return 0;
            }

            if (craftStuffPartial)
            {
                return CraftStuffPartialRelevancePercent;
            }

            return null;
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

        public static bool IsRawFungusBillFixLeftover()
        {
            Ensure();
            return rawFungusLeftover;
        }

        public static bool IsUfFillFixLeftover()
        {
            Ensure();
            return ufFillLeftover;
        }

        /// <summary>
        /// Header suffix for Fermenter Fill: 0 leftover, 65 when CommitIngredients still aborts
        /// extra stacks after IngredientPortion RemainingCount, null when that abort exists
        /// without RemainingCount (omit = 100%) or commit/driver is missing.
        ///
        /// Суффикс Fermenter Fill: 0 leftover, 65 если CommitIngredients всё ещё рвёт лишние
        /// стаки после RemainingCount у IngredientPortion, null если abort есть без
        /// RemainingCount (без суффикса = 100%) или нет commit/драйвера.
        /// </summary>
        public static int? GetUfFillRelevancePercent()
        {
            Ensure();
            if (ufFillLeftover)
            {
                return 0;
            }

            if (ufFillPartial)
            {
                return UfFillPartialRelevancePercent;
            }

            return null;
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
            if (IsCatCrazyTimeLeftover() || IsCraftStuffFixLeftover())
            {
                return true;
            }

#if RIMWORLD_1_6
            if (IsUfFillFixLeftover() || IsBurnWeaponBillFixLeftover() ||
                IsBreachAxeWorkAmountFixLeftover() || IsRawFungusBillFixLeftover())
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

        static void ProbeCraftStuffIfNeeded()
        {
            if (craftStuffProbed)
            {
                return;
            }

            craftStuffProbed = true;
            craftStuffLeftover = false;
            craftStuffPartial = false;
            ProbeCraftStuffBucket();
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

        /// <summary>
        /// Closed when SK already keeps stuffed material to CanMake stuff without random or
        /// ingredients[0]. Partial when that Prefix exists and still uses those picks. Alive
        /// when the Prefix type is missing or has no CanMake.
        ///
        /// Closed, если SK уже оставляет stuffed-материал CanMake stuff без random и
        /// ingredients[0]. Partial, если Prefix есть и всё ещё так выбирает. Alive, если типа
        /// Prefix нет или в нём нет CanMake.
        /// </summary>
        static void ProbeCraftStuffBucket()
        {
            Type prefixType = AccessTools.TypeByName(SkDominantIngredientPrefixTypeName);
            if (prefixType == null)
            {
                return;
            }

            bool canMake = TypeTreeCalls(prefixType, typeof(StuffProperties), "CanMake");
            if (!canMake)
            {
                return;
            }

            bool randomWeight = TypeTreeCalls(prefixType, typeof(GenCollection), "RandomElementByWeight")
                || TypeTreeCallsNamed(prefixType, "RandomElementByWeight");
            bool listIndexer = TypeTreeCallsListIndexer(prefixType);
            if (randomWeight || listIndexer)
            {
                craftStuffPartial = true;
                return;
            }

            craftStuffLeftover = true;
        }

#if RIMWORLD_1_6
        static void ProbeUfFillIfNeeded()
        {
            if (ufFillProbed)
            {
                return;
            }

            ufFillProbed = true;
            ufFillLeftover = false;
            ufFillPartial = false;
            ProbeUfFillBucket();
        }

        /// <summary>
        /// Closed when FillUF.CommitIngredients exists and no longer aborts with the extra-stack
        /// warning. Partial when that abort remains after IngredientPortion RemainingCount
        /// accounting. Alive when the abort is present without RemainingCount, or commit/driver
        /// is missing (Unknown stays current).
        ///
        /// Closed, если FillUF.CommitIngredients есть и больше не рвёт работу предупреждением
        /// про лишние стаки. Partial, если abort остался после учёта RemainingCount у
        /// IngredientPortion. Alive, если abort есть без RemainingCount, либо нет commit/драйвера
        /// (Unknown остаётся актуальным).
        /// </summary>
        static void ProbeUfFillBucket()
        {
            Type fill = AccessTools.TypeByName(UfFillJobDriverTypeName)
                ?? AccessTools.TypeByName(UfFillJobDriverTypeNameAlt);
            if (fill == null)
            {
                return;
            }

            MethodInfo commit = AccessTools.Method(fill, "CommitIngredients");
            if (commit == null)
            {
                return;
            }

            if (!MethodContainsString(commit, UfFillAbortWithoutConsumingMarker))
            {
                ufFillLeftover = true;
                return;
            }

            if (TypeTreeCallsNamed(fill, "RemainingCount"))
            {
                ufFillPartial = true;
            }
        }

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
            RawFungusBillFixFeatures.EnsureOriginalsCaptured();
            rawFungusLeftover = ProbeRawFungusClosed();
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

        /// <summary>
        /// Closed when captured RawFungus already belongs to FungusPlantRaw (HSK 1.5 overwrite
        /// or a later Unified fix). Alive when it is still only PlantFoodRaw.
        ///
        /// Closed, если сохранённый RawFungus уже в FungusPlantRaw (overwrite HSK 1.5 или
        /// поздний фикс Unified). Alive, если он всё ещё только в PlantFoodRaw.
        /// </summary>
        static bool ProbeRawFungusClosed()
        {
            return RawFungusBillFixFeatures.CapturedInFungusCategory();
        }
#endif

        static void LogOnce()
        {
            if (logged)
            {
                return;
            }

#if RIMWORLD_1_6
            if (!catCrazyTimeProbed || !craftStuffProbed || !ufFillProbed || !defsProbed)
            {
                return;
            }
#else
            if (!catCrazyTimeProbed || !craftStuffProbed)
            {
                return;
            }
#endif

            logged = true;
#if RIMWORLD_1_6
            Log.Message(
                "[HSK kebab tweaks] Symptom leftover: CatCrazyTime=" + catCrazyTimeLeftover +
                " CraftStuff=" + craftStuffLeftover +
                " CraftStuffPartial=" + craftStuffPartial +
                " UfFill=" + ufFillLeftover +
                " UfFillPartial=" + ufFillPartial +
                " BurnWeapon=" + burnWeaponLeftover +
                " BreachAxe=" + breachAxeLeftover +
                " RawFungus=" + rawFungusLeftover + ".");
#else
            Log.Message(
                "[HSK kebab tweaks] Symptom leftover: CatCrazyTime=" + catCrazyTimeLeftover +
                " CraftStuff=" + craftStuffLeftover +
                " CraftStuffPartial=" + craftStuffPartial + ".");
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

        static bool TypeTreeCalls(Type root, Type declaringType, string methodName)
        {
            bool found = false;
            ForEachDeclaredMethod(root, method =>
            {
                if (!found && MethodCalls(method, declaringType, methodName))
                {
                    found = true;
                }
            });
            return found;
        }

        static bool TypeTreeCallsNamed(Type root, string methodName)
        {
            bool found = false;
            ForEachDeclaredMethod(root, method =>
            {
                if (!found && MethodCallsNamed(method, methodName))
                {
                    found = true;
                }
            });
            return found;
        }

        static bool TypeTreeCallsListIndexer(Type root)
        {
            bool found = false;
            ForEachDeclaredMethod(root, method =>
            {
                if (!found && MethodCallsListIndexer(method))
                {
                    found = true;
                }
            });
            return found;
        }

        static void ForEachDeclaredMethod(Type type, Action<MethodInfo> visit)
        {
            if (type == null || visit == null)
            {
                return;
            }

            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                BindingFlags.Static | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methods.Length; i++)
            {
                visit(methods[i]);
            }

            Type[] nested = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < nested.Length; i++)
            {
                ForEachDeclaredMethod(nested[i], visit);
            }
        }

        static bool MethodCallsNamed(MethodInfo method, string methodName)
        {
            return MethodCallsWhere(method, resolved =>
                resolved != null && resolved.Name == methodName);
        }

        static bool MethodCallsListIndexer(MethodInfo method)
        {
            return MethodCallsWhere(method, resolved =>
            {
                if (resolved == null || resolved.Name != "get_Item")
                {
                    return false;
                }

                Type declaring = resolved.DeclaringType;
                return declaring != null && declaring.IsGenericType &&
                    declaring.GetGenericTypeDefinition() == typeof(List<>);
            });
        }

        static bool MethodCallsWhere(MethodInfo method, Func<MethodBase, bool> match)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || match == null)
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
                    if (match(resolved))
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

        static bool MethodContainsString(MethodInfo method, string marker)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || string.IsNullOrEmpty(marker))
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] != 0x72)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    string value = module.ResolveString(token);
                    if (value != null &&
                        value.IndexOf(marker, StringComparison.Ordinal) >= 0)
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
