using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Toils_Recipe.CalculateDominantIngredient picks stuffed-item material from any
    /// IsStuff ingredient (weighted random by stackCount) or from ingredients[0] when
    /// productHasIngredientStuff is set. It does not check stuffProps.CanMake on the product.
    /// HSK apparel often has stuffCategories (fabric) plus costList parts (components, metals).
    /// MakeUnfinishedThingIfNeeded and GenRecipe.MakeRecipeProducts both use that pick as Stuff,
    /// so the unfinished item and the finished craft can be a component T-Shirt (or steel parka)
    /// even when the bill only allowed cloth.
    ///
    /// Fix: Harmony Postfix on CalculateDominantIngredient keeps a stuffed product's dominant
    /// ingredient to stuff that CanMake the product, preferring the largest stack among valid
    /// stuff. Extra costList parts are ignored as material. Live-gated; vanilla types only.
    ///
    /// Проблема: Toils_Recipe.CalculateDominantIngredient берёт материал stuffed-предмета из
    /// любого IsStuff ингредиента (случайный вес по stackCount) или из ingredients[0] при
    /// productHasIngredientStuff. Не проверяет stuffProps.CanMake у продукта. В HSK одежда часто
    /// имеет stuffCategories (ткань) плюс costList (компоненты, металлы). MakeUnfinishedThingIfNeeded
    /// и GenRecipe.MakeRecipeProducts ставят этот выбор в Stuff — и незавершёнка, и готовый
    /// предмет могут быть футболкой из компонента (или стальной паркой), хотя в задании была
    /// только ткань.
    ///
    /// Исправление: Harmony Postfix на CalculateDominantIngredient оставляет dominant у
    /// stuffed-продукта только stuff, из которого CanMake этот продукт, с приоритетом большего
    /// stackCount. Лишний costList как материал не используется. Включается без рестарта;
    /// только vanilla-типы.
    /// </summary>
    public static class DominantIngredientStuffFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                var target = AccessTools.Method(
                    typeof(Toils_Recipe),
                    "CalculateDominantIngredient",
                    new[] { typeof(Job), typeof(List<Thing>) });
                if (target == null)
                {
                    Log.Message(
                        "[DominantIngredientStuffFixPatch] Toils_Recipe.CalculateDominantIngredient not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    target,
                    postfix: new HarmonyMethod(
                        typeof(Toils_Recipe_CalculateDominantIngredient_Patch),
                        nameof(Toils_Recipe_CalculateDominantIngredient_Patch.Postfix)));

                Log.Message("[DominantIngredientStuffFixPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[DominantIngredientStuffFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// After vanilla CalculateDominantIngredient, keeps stuffed-recipe material to an ingredient
    /// whose stuff CanMake the product (largest stackCount among those).
    ///
    /// После vanilla CalculateDominantIngredient оставляет материал stuffed-рецепта ингредиентом,
    /// чей stuff CanMake продукт (больший stackCount среди таких).
    /// </summary>
    public static class Toils_Recipe_CalculateDominantIngredient_Patch
    {
        public static void Postfix(Job job, List<Thing> ingredients, ref Thing __result)
        {
            if (!KebabTweaksSettings.EnableDominantIngredientStuffFix)
            {
                return;
            }

            if (ingredients == null || ingredients.Count == 0)
            {
                return;
            }

            ThingDef product = FindStuffedProduct(job);
            if (product == null || !product.MadeFromStuff)
            {
                return;
            }

            Thing chosen = null;
            int bestCount = -1;
            for (int i = 0; i < ingredients.Count; i++)
            {
                Thing ingredient = ingredients[i];
                if (ingredient == null || ingredient.def == null || !ingredient.def.IsStuff)
                {
                    continue;
                }

                StuffProperties stuffProps = ingredient.def.stuffProps;
                if (stuffProps == null || !stuffProps.CanMake(product))
                {
                    continue;
                }

                if (ingredient.stackCount > bestCount)
                {
                    bestCount = ingredient.stackCount;
                    chosen = ingredient;
                }
            }

            if (chosen != null)
            {
                __result = chosen;
            }
        }

        /// <summary>
        /// First recipe product that is MadeFromStuff, else the first product def.
        ///
        /// Первый продукт рецепта с MadeFromStuff, иначе def первого продукта.
        /// </summary>
        private static ThingDef FindStuffedProduct(Job job)
        {
            RecipeDef recipe = job == null ? null : job.RecipeDef;
            if (recipe == null || recipe.products == null || recipe.products.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < recipe.products.Count; i++)
            {
                ThingDef def = recipe.products[i] == null ? null : recipe.products[i].thingDef;
                if (def != null && def.MadeFromStuff)
                {
                    return def;
                }
            }

            return recipe.products[0] == null ? null : recipe.products[0].thingDef;
        }
    }
}
