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
    /// Problem: Numbers OptionsMaker.LoadDefault only walks Numbers_Settings.storedPawnTableDefs
    /// for a comma-split Default label matching the current PawnTableDef. Built-in presets
    /// (medical, combat, work, needs, psycasting) overwrite Numbers_MainTable.columns and never
    /// store a Default. Startup columns are already cached in
    /// StaticConstructorOnGameStart.PawnTableDef_Columns (after trainables and RemainingSpace),
    /// and the Numbers mod-settings reset button uses that cache, but Load default view under
    /// Presets does not.
    /// The player then gets Numbers_NoDefaultStoredForThisView until they use Set current view
    /// as default or restart.
    ///
    /// Fix: when this patch is enabled and no stored Default exists for the current table, restore
    /// columns from PawnTableDef_Columns for that table, then UpdateFilter and
    /// RefreshAndStoreSessionInWorldComp. If the player already used Set current view as default,
    /// the original LoadDefault runs unchanged. Does not write factory columns into
    /// storedPawnTableDefs. Soft-optional: no-op when Numbers is absent. Prefix return false is
    /// required only on the fallback path: Postfix cannot retract Messages.Message from the
    /// original reject branch.
    ///
    /// Проблема: Numbers OptionsMaker.LoadDefault смотрит только Numbers_Settings.storedPawnTableDefs
    /// на метку Default для текущего PawnTableDef. Встроенные пресеты перезаписывают
    /// Numbers_MainTable.columns и Default не сохраняют. Стартовые колонки уже лежат в
    /// StaticConstructorOnGameStart.PawnTableDef_Columns (после trainables и RemainingSpace),
    /// кнопка сброса в настройках Numbers этот кэш использует, пункт «Загрузить шаблон по
    /// умолчанию» в «Шаблоны» — нет.
    /// Игрок получает Numbers_NoDefaultStoredForThisView, пока сам не выберет «Установить
    /// текущий шаблон по умолчанию» или не перезапустит игру.
    ///
    /// Исправление: если патч включён и сохранённого Default для текущей таблицы нет —
    /// восстановить колонки из PawnTableDef_Columns, затем UpdateFilter и
    /// RefreshAndStoreSessionInWorldComp. Если игрок уже выбрал «Установить текущий шаблон
    /// по умолчанию», оригинальный LoadDefault не трогаем. Factory в storedPawnTableDefs не
    /// пишем. Без мода Numbers — тихий пропуск. Prefix return false только на пути fallback:
    /// Postfix не уберёт Messages.Message из ветки отказа оригинала.
    /// </summary>
    public static class NumbersLoadDefaultFallbackFeatures
    {
        private const string OptionsMakerTypeName = "Numbers.OptionsMaker";
        private const string SettingsTypeName = "Numbers.Numbers_Settings";
        private const string StaticCtorTypeName = "Numbers.StaticConstructorOnGameStart";
        private const string StoredDefaultLabel = "Default";

        private static FieldInfo numbersField;
        private static FieldInfo settingsField;
        private static FieldInfo storedPawnTableDefsField;
        private static FieldInfo pawnTableDefField;
        private static PropertyInfo pawnTableDefColumnsProperty;
        private static MethodInfo updateFilterMethod;
        private static MethodInfo refreshSessionMethod;
        private static bool loggedRestoreFailure;

        /// <summary>
        /// Registers the LoadDefault Prefix when Numbers is present and the required members resolve.
        ///
        /// Подключает Prefix на LoadDefault, если Numbers загружен и нужные члены находятся.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            Type optionsMakerType = AccessTools.TypeByName(OptionsMakerTypeName);
            if (optionsMakerType == null)
            {
                Log.Message("[NumbersLoadDefaultFallbackPatch] Numbers not loaded; patch skipped.");
                return;
            }

            MethodInfo loadDefault = AccessTools.Method(optionsMakerType, "LoadDefault");
            Type settingsType = AccessTools.TypeByName(SettingsTypeName);
            Type staticCtorType = AccessTools.TypeByName(StaticCtorTypeName);
            Type numbersWindowType = AccessTools.TypeByName("Numbers.MainTabWindow_Numbers");
            if (loadDefault == null || settingsType == null || staticCtorType == null || numbersWindowType == null)
            {
                Log.Warning("[NumbersLoadDefaultFallbackPatch] Numbers LoadDefault members missing; patch skipped.");
                return;
            }

            numbersField = AccessTools.Field(optionsMakerType, "numbers");
            settingsField = AccessTools.Field(optionsMakerType, "settings");
            storedPawnTableDefsField = AccessTools.Field(settingsType, "storedPawnTableDefs");
            pawnTableDefField = AccessTools.Field(numbersWindowType, "pawnTableDef");
            pawnTableDefColumnsProperty = AccessTools.Property(staticCtorType, "PawnTableDef_Columns");
            updateFilterMethod = AccessTools.Method(numbersWindowType, "UpdateFilter");
            refreshSessionMethod = AccessTools.Method(numbersWindowType, "RefreshAndStoreSessionInWorldComp");
            if (numbersField == null || settingsField == null || storedPawnTableDefsField == null
                || pawnTableDefField == null || pawnTableDefColumnsProperty == null
                || updateFilterMethod == null || refreshSessionMethod == null)
            {
                Log.Warning("[NumbersLoadDefaultFallbackPatch] Numbers reflection targets missing; patch skipped.");
                return;
            }

            HarmonyMethod prefix = new HarmonyMethod(
                typeof(NumbersLoadDefaultFallbackFeatures),
                nameof(LoadDefault_Prefix));
            prefix.priority = Priority.First;
            harmony.Patch(loadDefault, prefix: prefix);
        }

        /// <summary>
        /// Restores startup columns when no player Default is stored; otherwise runs the original.
        /// Prefix skip is only the factory fallback: the original reject message cannot be undone
        /// from a Postfix.
        ///
        /// Восстанавливает стартовые колонки, если игрок не сохранял Default; иначе идёт оригинал.
        /// Пропуск оригинала только на fallback: сообщение отказа из оригинала Postfix не снимет.
        /// </summary>
        public static bool LoadDefault_Prefix(object __instance)
        {
            if (!KebabTweaksSettings.EnableNumbersLoadDefaultFallback || __instance == null)
            {
                return true;
            }

            try
            {
                if (HasStoredDefaultForCurrentTable(__instance))
                {
                    return true;
                }

                if (TryRestoreFactoryColumns(__instance))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!loggedRestoreFailure)
                {
                    loggedRestoreFailure = true;
                    Log.Warning("[NumbersLoadDefaultFallbackPatch] LoadDefault fallback failed: " + ex);
                }
            }

            return true;
        }

        private static bool HasStoredDefaultForCurrentTable(object optionsMaker)
        {
            PawnTableDef table = GetCurrentPawnTable(optionsMaker);
            if (table == null || table.defName.NullOrEmpty())
            {
                return false;
            }

            object settings = settingsField.GetValue(optionsMaker);
            if (settings == null)
            {
                return false;
            }

            IList stored = storedPawnTableDefsField.GetValue(settings) as IList;
            if (stored == null)
            {
                return false;
            }

            for (int i = 0; i < stored.Count; i++)
            {
                string item = stored[i] as string;
                if (item.NullOrEmpty())
                {
                    continue;
                }

                string[] parts = item.Split(',');
                if (parts.Length < 2)
                {
                    continue;
                }

                if (parts[1] == StoredDefaultLabel && parts[0] == table.defName)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryRestoreFactoryColumns(object optionsMaker)
        {
            object numbers = numbersField.GetValue(optionsMaker);
            if (numbers == null)
            {
                return false;
            }

            PawnTableDef table = pawnTableDefField.GetValue(numbers) as PawnTableDef;
            if (table == null || table.defName.NullOrEmpty())
            {
                return false;
            }

            IDictionary cache = pawnTableDefColumnsProperty.GetValue(null) as IDictionary;
            if (cache == null || !cache.Contains(table.defName))
            {
                return false;
            }

            IEnumerable names = cache[table.defName] as IEnumerable;
            if (names == null)
            {
                return false;
            }

            List<PawnColumnDef> columns = new List<PawnColumnDef>();
            foreach (object nameObj in names)
            {
                string name = nameObj as string;
                if (name.NullOrEmpty())
                {
                    continue;
                }

                PawnColumnDef column = DefDatabase<PawnColumnDef>.GetNamedSilentFail(name);
                if (column != null)
                {
                    columns.Add(column);
                }
            }

            if (columns.Count == 0)
            {
                return false;
            }

            table.columns = columns;
            updateFilterMethod.Invoke(numbers, null);
            refreshSessionMethod.Invoke(numbers, null);
            return true;
        }

        private static PawnTableDef GetCurrentPawnTable(object optionsMaker)
        {
            object numbers = numbersField.GetValue(optionsMaker);
            if (numbers == null)
            {
                return null;
            }

            return pawnTableDefField.GetValue(numbers) as PawnTableDef;
        }
    }
}
