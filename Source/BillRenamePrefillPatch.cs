using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Better Workbench Management opens Dialog_RenameBill with an empty curName field,
    /// so players cannot edit the current bill label.
    ///
    /// Fix: when BWM is present, prefill curName from ExtendedBillData / linked Bill_Production
    /// LabelCap on dialog open and select all text on first focus. Soft-optional when Improved
    /// Workbenches is absent.
    ///
    /// Проблема: Better Workbench Management открывает Dialog_RenameBill с пустым curName — нельзя
    /// отредактировать текущее имя работы.
    ///
    /// Исправление: при наличии BWM подставляет curName из ExtendedBillData / связанного
    /// Bill_Production.LabelCap и выделяет весь текст при первом фокусе. Мягко опционально без
    /// Improved Workbenches.
    /// </summary>
    public static class BillRenamePrefillFeatures
    {
        private const string DialogRenameBillTypeName = "ImprovedWorkbenches.Dialog_RenameBill";
        private const string ExtendedBillDataTypeName = "ImprovedWorkbenches.ExtendedBillData";
        private const string ExtendedBillDataStorageTypeName = "ImprovedWorkbenches.ExtendedBillDataStorage";
        private const string MainTypeName = "ImprovedWorkbenches.Main";

        private static Type dialogRenameBillType;
        private static Type extendedBillDataType;
        private static Type extendedBillDataStorageType;
        private static Type mainType;
        private static FieldInfo curNameField;
        private static FieldInfo extendedBillField;
        private static FieldInfo extendedBillNameField;
        private static FieldInfo storageStoreField;
        private static PropertyInfo mainInstanceProperty;
        private static MethodInfo mainGetStorageMethod;
        private static MethodInfo worldGetComponentMethod;
        private static MethodInfo renameDialogDoWindowContentsMethod;
        private static readonly HashSet<int> SelectAllApplied = new HashSet<int>();

        public static void Apply(Harmony harmony)
        {
            try
            {
                dialogRenameBillType = AccessTools.TypeByName(DialogRenameBillTypeName);
                if (dialogRenameBillType == null)
                {
                    Log.Message("[BillRenamePrefillPatch] Better Workbench Management not loaded; patch skipped.");
                    return;
                }

                extendedBillDataType = AccessTools.TypeByName(ExtendedBillDataTypeName);
                extendedBillDataStorageType = AccessTools.TypeByName(ExtendedBillDataStorageTypeName);
                mainType = AccessTools.TypeByName(MainTypeName);
                curNameField = FindInheritedField(dialogRenameBillType, "curName");
                extendedBillField = AccessTools.Field(dialogRenameBillType, "_extendedBill");
                extendedBillNameField = extendedBillDataType != null
                    ? AccessTools.Field(extendedBillDataType, "Name")
                    : null;
                storageStoreField = extendedBillDataStorageType != null
                    ? AccessTools.Field(extendedBillDataStorageType, "_store")
                    : null;
                mainInstanceProperty = mainType != null
                    ? mainType.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    : null;
                mainGetStorageMethod = mainType != null
                    ? AccessTools.Method(mainType, "GetExtendedBillDataStorage")
                    : null;
                worldGetComponentMethod = AccessTools.Method(typeof(World), "GetComponent", new[] { typeof(Type) });

                Type renameDialogBaseType = AccessTools.TypeByName("Verse.Dialog_Rename`1");
                if (renameDialogBaseType == null)
                {
                    Log.Warning("[BillRenamePrefillPatch] Verse.Dialog_Rename`1 not found; patch skipped.");
                    return;
                }

                Type genericRenameType = renameDialogBaseType.MakeGenericType(typeof(IRenameable));
                renameDialogDoWindowContentsMethod = AccessTools.DeclaredMethod(
                    genericRenameType,
                    "DoWindowContents",
                    new[] { typeof(Rect) });

                ConstructorInfo ctor = extendedBillDataType != null
                    ? dialogRenameBillType.GetConstructor(new[] { extendedBillDataType })
                    : null;

                if (curNameField == null || renameDialogDoWindowContentsMethod == null || ctor == null)
                {
                    Log.Warning("[BillRenamePrefillPatch] Dialog_RenameBill hooks not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    ctor,
                    postfix: new HarmonyMethod(typeof(BillRenamePrefillFeatures), nameof(Dialog_RenameBill_Ctor_Postfix)));

                harmony.Patch(
                    renameDialogDoWindowContentsMethod,
                    prefix: new HarmonyMethod(typeof(BillRenamePrefillFeatures), nameof(Dialog_Rename_DoWindowContents_Prefix)),
                    postfix: new HarmonyMethod(typeof(BillRenamePrefillFeatures), nameof(Dialog_Rename_DoWindowContents_Postfix)));

                Log.Message("[BillRenamePrefillPatch] Patches applied (BWM bill rename prefill).");
            }
            catch (Exception ex)
            {
                Log.Error("[BillRenamePrefillPatch] Failed to apply patches: " + ex);
            }
        }

        /// <summary>
        /// Prefills curName right after Dialog_RenameBill construction.
        ///
        /// Подставляет curName сразу после создания Dialog_RenameBill.
        /// </summary>
        public static void Dialog_RenameBill_Ctor_Postfix(Window __instance)
        {
            if (!KebabTweaksSettings.EnableBillRenamePrefill)
            {
                return;
            }

            TryPrefill(__instance);
        }

        /// <summary>
        /// Ensures curName is set before the rename field is drawn.
        ///
        /// Гарантирует curName до отрисовки поля переименования.
        /// </summary>
        public static void Dialog_Rename_DoWindowContents_Prefix(Window __instance)
        {
            if (!KebabTweaksSettings.EnableBillRenamePrefill)
            {
                return;
            }

            TryPrefill(__instance);
        }

        /// <summary>
        /// Selects the whole rename field after vanilla focuses it.
        ///
        /// Выделяет всё поле после фокуса vanilla.
        /// </summary>
        public static void Dialog_Rename_DoWindowContents_Postfix(Window __instance)
        {
            if (!KebabTweaksSettings.EnableBillRenamePrefill
                || dialogRenameBillType == null
                || !dialogRenameBillType.IsInstanceOfType(__instance))
            {
                return;
            }

            int key = __instance.GetHashCode();
            if (SelectAllApplied.Contains(key))
            {
                return;
            }

            if (GUI.GetNameOfFocusedControl() != "RenameField")
            {
                return;
            }

            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (editor == null)
            {
                return;
            }

            editor.SelectAll();
            SelectAllApplied.Add(key);
        }

        private static void TryPrefill(Window window)
        {
            if (dialogRenameBillType == null || curNameField == null || !dialogRenameBillType.IsInstanceOfType(window))
            {
                return;
            }

            string current = curNameField.GetValue(window) as string;
            if (!current.NullOrEmpty())
            {
                return;
            }

            string label = ResolvePrefillLabel(window);
            if (label.NullOrEmpty())
            {
                return;
            }

            curNameField.SetValue(window, label);
            SelectAllApplied.Remove(window.GetHashCode());
        }

        private static string ResolvePrefillLabel(Window window)
        {
            object extendedBill = extendedBillField?.GetValue(window);
            if (extendedBill == null)
            {
                return null;
            }

            if (extendedBillNameField != null)
            {
                string customName = extendedBillNameField.GetValue(extendedBill) as string;
                if (!customName.NullOrEmpty())
                {
                    return customName;
                }
            }

            return ResolveBillLabelFromStorage(extendedBill);
        }

        private static object ResolveStorage()
        {
            if (mainType != null && mainInstanceProperty != null && mainGetStorageMethod != null)
            {
                object main = mainInstanceProperty.GetValue(null, null);
                if (main != null)
                {
                    object storage = mainGetStorageMethod.Invoke(main, null);
                    if (storage != null)
                    {
                        return storage;
                    }
                }
            }

            if (extendedBillDataStorageType == null || worldGetComponentMethod == null || Find.World == null)
            {
                return null;
            }

            return worldGetComponentMethod.Invoke(Find.World, new object[] { extendedBillDataStorageType });
        }

        private static string ResolveBillLabelFromStorage(object extendedBill)
        {
            object storage = ResolveStorage();
            if (extendedBill == null || storage == null || storageStoreField == null)
            {
                return null;
            }

            if (!(storageStoreField.GetValue(storage) is IDictionary store))
            {
                return null;
            }

            foreach (DictionaryEntry entry in store)
            {
                if (!ReferenceEquals(entry.Value, extendedBill))
                {
                    continue;
                }

                if (entry.Key is Bill_Production production)
                {
                    if (!production.LabelCap.NullOrEmpty())
                    {
                        return production.LabelCap;
                    }

                    return production.recipe?.LabelCap;
                }

                if (entry.Key is Bill bill)
                {
                    if (!bill.LabelCap.NullOrEmpty())
                    {
                        return bill.LabelCap;
                    }
                }
            }

            return null;
        }

        private static FieldInfo FindInheritedField(Type type, string name)
        {
            while (type != null)
            {
                FieldInfo field = AccessTools.Field(type, name);
                if (field != null)
                {
                    return field;
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
