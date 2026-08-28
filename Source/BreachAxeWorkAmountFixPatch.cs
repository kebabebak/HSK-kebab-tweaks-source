#if RIMWORLD_1_6
using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: AutoGenPatches replace MeleeWeapon_BreachAxe statBases without Merge and without
    /// WorkToMake. Vanilla WorkToMake is 5000. StatDef WorkToMake defaultBaseValue is 1, so the
    /// generated Make recipe shows work amount 1 and finishes instantly.
    ///
    /// Fix: After defs load, set WorkToMake 5000 on the thing and the same value on the
    /// generated make RecipeDef when the setting is on. Toggle writes back the captured
    /// originals. No Prefix skip.
    ///
    /// Проблема: AutoGenPatches подменяют statBases у MeleeWeapon_BreachAxe без Merge и без
    /// WorkToMake. Ванильный WorkToMake 5000. У StatDef WorkToMake defaultBaseValue 1, поэтому
    /// сгенерированный рецепт изготовления показывает объём работ 1 и заканчивается сразу.
    ///
    /// Исправление: после загрузки дефов при включённой настройке ставит WorkToMake 5000
    /// на вещи и то же значение на сгенерированном RecipeDef изготовления. Выключение
    /// пишет сохранённые оригиналы. Prefix-skip не используется.
    /// </summary>
    public static class BreachAxeWorkAmountFixFeatures
    {
        const string ThingDefName = "MeleeWeapon_BreachAxe";
        const float RestoredWorkToMake = 5000f;

        static bool captured;
        static float originalThingWorkToMake;
        static float originalRecipeWorkAmount;
        static bool lastEnabled;

        public static void Apply(Harmony harmony)
        {
            try
            {
                SyncWorkAmount(force: true);
                Log.Message("[BreachAxeWorkAmountFixPatch] Work amount synced.");
            }
            catch (Exception e)
            {
                Log.Error("[BreachAxeWorkAmountFixPatch] Failed to apply: " + e);
            }
        }

        /// <summary>
        /// Writes WorkToMake 5000 or the captured originals onto the breach axe and its make recipe.
        ///
        /// Пишет WorkToMake 5000 или сохранённые оригиналы на штурмовой топор и его рецепт изготовления.
        /// </summary>
        public static void SyncWorkAmount(bool force = false)
        {
            bool enable = KebabTweaksSettings.IsObsoleteFixEnabled(
                KebabTweaksSettings.EnableBreachAxeWorkAmountFix);
            if (!CaptureOriginalsIfNeeded())
            {
                return;
            }

            if (!force && enable == lastEnabled)
            {
                return;
            }

            ApplyValues(enable ? RestoredWorkToMake : originalThingWorkToMake,
                enable ? RestoredWorkToMake : originalRecipeWorkAmount);
            lastEnabled = enable;
        }

        static bool CaptureOriginalsIfNeeded()
        {
            if (captured)
            {
                return true;
            }

            ThingDef thing = DefDatabase<ThingDef>.GetNamedSilentFail(ThingDefName);
            if (thing == null)
            {
                Log.Message("[BreachAxeWorkAmountFixPatch] MeleeWeapon_BreachAxe not found; skipped.");
                return false;
            }

            RecipeDef recipe = FindMakeRecipe(thing);
            if (recipe == null)
            {
                Log.Message("[BreachAxeWorkAmountFixPatch] Make recipe for MeleeWeapon_BreachAxe not found; skipped.");
                return false;
            }

            originalThingWorkToMake = thing.GetStatValueAbstract(StatDefOf.WorkToMake);
            originalRecipeWorkAmount = recipe.workAmount;
            captured = true;
            return true;
        }

        static void ApplyValues(float thingWork, float recipeWork)
        {
            ThingDef thing = DefDatabase<ThingDef>.GetNamedSilentFail(ThingDefName);
            if (thing == null)
            {
                return;
            }

            thing.SetStatBaseValue(StatDefOf.WorkToMake, thingWork);
            RecipeDef recipe = FindMakeRecipe(thing);
            if (recipe != null)
            {
                recipe.workAmount = recipeWork;
            }
        }

        static RecipeDef FindMakeRecipe(ThingDef product)
        {
            RecipeDef named = DefDatabase<RecipeDef>.GetNamedSilentFail("Make_" + product.defName);
            if (named != null)
            {
                return named;
            }

            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                if (recipe.products == null)
                {
                    continue;
                }

                for (int i = 0; i < recipe.products.Count; i++)
                {
                    if (recipe.products[i].thingDef == product)
                    {
                        return recipe;
                    }
                }
            }

            return null;
        }
    }
}
#endif
