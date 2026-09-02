#if RIMWORLD_1_6
using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Vanilla BurnWeapon, BurnApparel, and BurnDrugs set requiredGiverWorkType to
    /// Hauling. HSK puts kiln, crematorium, campfire, and burn-pit WorkGiver_DoBill on Crafting,
    /// Processing, or Misc. WorkGiver_DoBill then skips the bill, so stored burnable weapons
    /// look allowed in the filter (map count) but the job never starts. What's Missing paints
    /// a red ingredient number next to work amount from ResourceCounter.GetCount; weapons are
    /// not counted as stockpile resources, so that overlay stays red even when the job can start.
    ///
    /// Fix: After defs load, clear requiredGiverWorkType on those three recipes when the
    /// setting is on. Toggle restores the captured original. No Prefix skip of WorkGiver_DoBill.
    ///
    /// Проблема: ваниль у BurnWeapon, BurnApparel и BurnDrugs ставит requiredGiverWorkType
    /// Hauling. HSK вешает WorkGiver_DoBill горна, крематория, костра и ямы на Crafting,
    /// Processing или Misc. WorkGiver_DoBill пропускает задание: в фильтре оружие видно
    /// (счётчик на карте), но работу никто не начинает. What's Missing рисует красное число
    /// ингредиентов рядом с объёмом работ через ResourceCounter.GetCount; оружие не считается
    /// складским ресурсом, поэтому оверлей остаётся красным даже когда работу можно взять.
    ///
    /// Исправление: после загрузки дефов при включённой настройке обнуляет
    /// requiredGiverWorkType у этих трёх рецептов. Выключение возвращает сохранённое значение.
    /// WorkGiver_DoBill Prefix-skip не используется.
    /// </summary>
    public static class BurnWeaponBillFixFeatures
    {
        static readonly string[] RecipeDefNames =
        {
            "BurnWeapon",
            "BurnApparel",
            "BurnDrugs",
        };

        static Dictionary<string, WorkTypeDef> originalGiverWorkTypes;
        static bool lastEnabled;

        public static void Apply(Harmony harmony)
        {
            try
            {
                SyncRecipeWorkTypes(force: true);
                Log.Message("[BurnWeaponBillFixPatch] Recipe work types synced.");
            }
            catch (Exception e)
            {
                Log.Error("[BurnWeaponBillFixPatch] Failed to apply: " + e);
            }
        }

        /// <summary>
        /// Clears or restores requiredGiverWorkType on the burn recipes to match the enable flag.
        ///
        /// Обнуляет или восстанавливает requiredGiverWorkType у рецептов сжигания по флагу Enable.
        /// </summary>
        public static void SyncRecipeWorkTypes(bool force = false)
        {
            bool enable = KebabTweaksSettings.IsBurnWeaponBillFixEnabled();
            EnsureOriginalsCaptured();
            if (originalGiverWorkTypes == null || originalGiverWorkTypes.Count == 0)
            {
                return;
            }

            if (!force && enable == lastEnabled)
            {
                return;
            }

            foreach (KeyValuePair<string, WorkTypeDef> entry in originalGiverWorkTypes)
            {
                RecipeDef recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(entry.Key);
                if (recipe == null)
                {
                    continue;
                }

                recipe.requiredGiverWorkType = enable ? null : entry.Value;
            }

            lastEnabled = enable;
        }

        /// <summary>
        /// Stores requiredGiverWorkType from XML/HSK before kebab clears the field.
        ///
        /// Запоминает requiredGiverWorkType из XML/HSK до того, как kebab обнулит поле.
        /// </summary>
        public static void EnsureOriginalsCaptured()
        {
            if (originalGiverWorkTypes != null && originalGiverWorkTypes.Count > 0)
            {
                return;
            }

            var captured = new Dictionary<string, WorkTypeDef>();
            foreach (string defName in RecipeDefNames)
            {
                RecipeDef recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(defName);
                if (recipe == null)
                {
                    continue;
                }

                captured[defName] = recipe.requiredGiverWorkType;
            }

            if (captured.Count == 0)
            {
                return;
            }

            originalGiverWorkTypes = captured;
        }

        public static WorkTypeDef GetCapturedRequiredGiver(string defName)
        {
            EnsureOriginalsCaptured();
            if (originalGiverWorkTypes == null || defName == null)
            {
                return null;
            }

            WorkTypeDef workType;
            return originalGiverWorkTypes.TryGetValue(defName, out workType) ? workType : null;
        }
    }
}
#endif
