#if RIMWORLD_1_6
using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: HSK 1.5 overwrites RawFungus onto MushroomBase so it sits in FungusPlantRaw.
    /// On 1.6 that overwrite is commented out; Unified keeps vanilla RawFungus in PlantFoodRaw.
    /// Recipes such as Makeyeast still list RawFungus in fixedIngredientFilter, so the bill
    /// tree shows the item (Russian label Грибы, same word as the fungi category) with a
    /// stockpile count. The ingredient slot is the FungusPlantRaw category, so
    /// WorkGiver_DoBill and What's Missing ignore that item. Pawns cannot start the bill from
    /// it; other fungi in that category still fill the slot.
    ///
    /// Fix: After defs load, move RawFungus into FungusPlantRaw like 1.5 and SetAllow it on
    /// recipe ingredient filters that already accept that category. Toggle restores the
    /// captured categories and removes the extra allows. No Prefix skip.
    ///
    /// Проблема: на HSK 1.5 overwrite кладёт RawFungus на MushroomBase в FungusPlantRaw.
    /// На 1.6 overwrite закомментирован; Unified оставляет ванильный RawFungus в PlantFoodRaw.
    /// Рецепты вроде Makeyeast по-прежнему держат RawFungus в fixedIngredientFilter, поэтому
    /// в дереве задания вещь видна (русская подпись «Грибы», как у категории) со складским
    /// счётом. Слот ингредиентов — категория FungusPlantRaw, поэтому WorkGiver_DoBill и
    /// What's Missing эту вещь не берут. Пешка не начнёт задание из неё; другие виды грибов
    /// в этой категории слот по-прежнему закрывают.
    ///
    /// Исправление: после загрузки дефов переносит RawFungus в FungusPlantRaw как на 1.5 и
    /// делает SetAllow в фильтрах слотов рецептов, которые уже принимают эту категорию.
    /// Выключение возвращает сохранённые категории и снимает лишние allow. Prefix-skip нет.
    /// </summary>
    public static class RawFungusBillFixFeatures
    {
        const string RawFungusDefName = "RawFungus";
        const string FungusCategoryDefName = "FungusPlantRaw";

        static bool captured;
        static List<ThingCategoryDef> originalCategories;
        static bool lastEnabled;
        static bool capturedInFungusCategory;

        public static void Apply(Harmony harmony)
        {
            try
            {
                SyncCategory(force: true);
                Log.Message("[RawFungusBillFixPatch] Raw fungus category synced.");
            }
            catch (Exception e)
            {
                Log.Error("[RawFungusBillFixPatch] Failed to apply: " + e);
            }
        }

        /// <summary>
        /// Moves RawFungus into FungusPlantRaw or restores the captured categories.
        ///
        /// Переносит RawFungus в FungusPlantRaw или возвращает сохранённые категории.
        /// </summary>
        public static void SyncCategory(bool force = false)
        {
            bool enable = KebabTweaksSettings.IsRawFungusBillFixEnabled();
            if (!EnsureOriginalsCaptured())
            {
                return;
            }

            if (!force && enable == lastEnabled)
            {
                return;
            }

            ThingDef rawFungus = DefDatabase<ThingDef>.GetNamedSilentFail(RawFungusDefName);
            ThingCategoryDef fungusCat = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(FungusCategoryDefName);
            if (rawFungus == null || fungusCat == null)
            {
                return;
            }

            if (enable)
            {
                MoveToCategory(rawFungus, fungusCat);
                SetFungusSlotAllowsRawFungus(rawFungus, fungusCat, allow: true);
            }
            else
            {
                RestoreOriginalCategories(rawFungus);
                SetFungusSlotAllowsRawFungus(rawFungus, fungusCat, allow: false);
            }

            lastEnabled = enable;
        }

        /// <summary>
        /// Stores RawFungus thingCategories before kebab moves the def.
        ///
        /// Запоминает thingCategories у RawFungus до переноса.
        /// </summary>
        public static bool EnsureOriginalsCaptured()
        {
            if (captured)
            {
                return originalCategories != null;
            }

            ThingDef rawFungus = DefDatabase<ThingDef>.GetNamedSilentFail(RawFungusDefName);
            ThingCategoryDef fungusCat = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(FungusCategoryDefName);
            captured = true;
            if (rawFungus == null || fungusCat == null)
            {
                return false;
            }

            originalCategories = new List<ThingCategoryDef>();
            if (rawFungus.thingCategories != null)
            {
                originalCategories.AddRange(rawFungus.thingCategories);
            }

            capturedInFungusCategory = originalCategories.Contains(fungusCat);
            return true;
        }

        public static bool CapturedInFungusCategory()
        {
            EnsureOriginalsCaptured();
            return capturedInFungusCategory;
        }

        static void MoveToCategory(ThingDef def, ThingCategoryDef target)
        {
            DetachFromCurrentCategories(def, keep: target);
            if (def.thingCategories == null)
            {
                def.thingCategories = new List<ThingCategoryDef>();
            }

            if (!def.thingCategories.Contains(target))
            {
                def.thingCategories.Add(target);
            }

            AttachChild(target, def);
            target.ResolveReferences();
        }

        static void RestoreOriginalCategories(ThingDef def)
        {
            DetachFromCurrentCategories(def, keep: null);
            if (def.thingCategories == null)
            {
                def.thingCategories = new List<ThingCategoryDef>();
            }

            foreach (ThingCategoryDef cat in originalCategories)
            {
                if (cat == null)
                {
                    continue;
                }

                if (!def.thingCategories.Contains(cat))
                {
                    def.thingCategories.Add(cat);
                }

                AttachChild(cat, def);
                cat.ResolveReferences();
            }
        }

        static void DetachFromCurrentCategories(ThingDef def, ThingCategoryDef keep)
        {
            if (def.thingCategories == null)
            {
                return;
            }

            for (int i = def.thingCategories.Count - 1; i >= 0; i--)
            {
                ThingCategoryDef old = def.thingCategories[i];
                if (old == null || old == keep)
                {
                    continue;
                }

                if (old.childThingDefs != null)
                {
                    old.childThingDefs.Remove(def);
                }

                old.ResolveReferences();
                def.thingCategories.RemoveAt(i);
            }
        }

        static void AttachChild(ThingCategoryDef cat, ThingDef def)
        {
            if (cat.childThingDefs == null)
            {
                cat.childThingDefs = new List<ThingDef>();
            }

            if (!cat.childThingDefs.Contains(def))
            {
                cat.childThingDefs.Add(def);
            }
        }

        static void SetFungusSlotAllowsRawFungus(ThingDef rawFungus, ThingCategoryDef fungusCat, bool allow)
        {
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
            {
                if (recipe == null)
                {
                    continue;
                }

                if (recipe.ingredients == null)
                {
                    continue;
                }

                foreach (IngredientCount ingredient in recipe.ingredients)
                {
                    if (ingredient == null)
                    {
                        continue;
                    }

                    TouchFilter(ingredient.filter, rawFungus, fungusCat, allow);
                }
            }
        }

        static void TouchFilter(
            ThingFilter filter,
            ThingDef rawFungus,
            ThingCategoryDef fungusCat,
            bool allow)
        {
            if (filter == null || !FilterUsesFungusCategory(filter, rawFungus, fungusCat))
            {
                return;
            }

            if (allow)
            {
                if (!filter.Allows(rawFungus))
                {
                    filter.SetAllow(rawFungus, true);
                }

                return;
            }

            if (!capturedInFungusCategory && filter.Allows(rawFungus))
            {
                filter.SetAllow(rawFungus, false);
            }
        }

        static bool FilterUsesFungusCategory(
            ThingFilter filter,
            ThingDef rawFungus,
            ThingCategoryDef fungusCat)
        {
            foreach (ThingDef def in filter.AllowedThingDefs)
            {
                if (def == null || def == rawFungus)
                {
                    continue;
                }

                if (def.IsWithinCategory(fungusCat))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif
