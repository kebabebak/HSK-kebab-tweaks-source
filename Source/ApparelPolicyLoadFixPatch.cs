using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: [KV] Save Storage Settings loads saved apparel policies into the currently selected
    /// ApparelPolicy filter only (IOUtil.LoadFilters); the policy name stays unchanged. Food policy
    /// loads apply the saved name from the file (IOUtil.LoadFoodRestriction / FoodPolicy flow), and
    /// "Load as new" already creates a fresh apparel policy before opening the file list.
    ///
    /// Fix: when the fix is enabled and Save Storage Settings opens LoadFilterDialog for
    /// Apparel_Management on an existing named policy, create a new ApparelPolicy, select it, and
    /// load into that policy; after load, set the policy label from the save file name (filter saves
    /// have no embedded name field). Skips fresh "Unnamed" policies so "Load as new" is unchanged.
    /// Soft-optional: no-op when Save Storage Settings is absent.
    ///
    /// Проблема: [KV] Save Storage Settings при загрузке гардероба пишет только в ThingFilter
    /// выбранной ApparelPolicy (IOUtil.LoadFilters); имя политики не меняется. Для диет имя
    /// подхватывается из файла; «Загрузить как новый» для гардероба уже создаёт новую политику.
    ///
    /// Исправление: при включённом фиксе и LoadFilterDialog Apparel_Management для уже именованной
    /// политики — создать новую ApparelPolicy, выбрать её и загрузить в неё; после загрузки имя
    /// из имени файла. Для свежей «Unnamed» не вмешиваемся («Загрузить как новый»). Без мода
    /// Save Storage Settings — тихий пропуск.
    /// </summary>
    public static class ApparelPolicyLoadFixFeatures
    {
        private const string ApparelStorageType = "Apparel_Management";
        private const string LoadFilterDialogTypeName = "SaveStorageSettings.Dialog.LoadFilterDialog";
        private const string FileListDialogTypeName = "SaveStorageSettings.Dialog.FileListDialog";

        private static ApparelPolicy pendingRenamePolicy;

        /// <summary>
        /// Registers WindowStack and LoadFilterDialog hooks when Save Storage Settings is present.
        ///
        /// Подключает хуки WindowStack и LoadFilterDialog при наличии Save Storage Settings.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            if (AccessTools.TypeByName(LoadFilterDialogTypeName) == null)
            {
                Log.Message("[ApparelPolicyLoadFixPatch] Save Storage Settings not loaded; patch skipped.");
                return;
            }

            harmony.Patch(
                AccessTools.Method(typeof(WindowStack), nameof(WindowStack.Add)),
                prefix: new HarmonyMethod(typeof(ApparelPolicyLoadFixFeatures), nameof(WindowStack_Add_Prefix)));

            MethodInfo doFileInteraction = AccessTools.Method(
                AccessTools.TypeByName(LoadFilterDialogTypeName),
                "DoFileInteraction");
            if (doFileInteraction == null)
            {
                Log.Warning("[ApparelPolicyLoadFixPatch] LoadFilterDialog.DoFileInteraction not found; rename skipped.");
                return;
            }

            harmony.Patch(
                doFileInteraction,
                postfix: new HarmonyMethod(typeof(ApparelPolicyLoadFixFeatures), nameof(LoadFilterDialog_DoFileInteraction_Postfix)));
        }

        /// <summary>
        /// Redirects apparel load into a new policy when loading over an existing named policy.
        ///
        /// Перенаправляет загрузку гардероба в новую политику вместо перезаписи выбранной.
        /// </summary>
        public static void WindowStack_Add_Prefix(Window window)
        {
            if (!KebabTweaksSettings.EnableApparelPolicyLoadFix || window == null)
            {
                return;
            }

            Type windowType = window.GetType();
            if (windowType.FullName != LoadFilterDialogTypeName)
            {
                return;
            }

            if (!string.Equals(ReadStorageTypeName(window), ApparelStorageType, StringComparison.Ordinal))
            {
                return;
            }

            Dialog_ManageApparelPolicies manageDialog = FindManageApparelPoliciesDialog();
            if (manageDialog == null)
            {
                return;
            }

            ApparelPolicy selected = GetSelectedApparelPolicy(manageDialog);
            if (selected == null || IsFreshUnnamedPolicy(selected))
            {
                pendingRenamePolicy = null;
                return;
            }

            ApparelPolicy newPolicy = Current.Game.outfitDatabase.MakeNewOutfit();
            SetSelectedApparelPolicy(manageDialog, newPolicy);
            SetLoadFilterTarget(window, newPolicy.filter);
            pendingRenamePolicy = newPolicy;
        }

        /// <summary>
        /// Applies the saved file name as the new apparel policy label after a successful load.
        ///
        /// После успешной загрузки задаёт имя новой политики из имени файла.
        /// </summary>
        public static void LoadFilterDialog_DoFileInteraction_Postfix(object __instance, FileInfo fi)
        {
            if (!KebabTweaksSettings.EnableApparelPolicyLoadFix || fi == null)
            {
                return;
            }

            if (__instance == null
                || __instance.GetType().FullName != LoadFilterDialogTypeName
                || !string.Equals(ReadStorageTypeName(__instance), ApparelStorageType, StringComparison.Ordinal))
            {
                return;
            }

            ApparelPolicy policy = pendingRenamePolicy;
            pendingRenamePolicy = null;
            if (policy == null)
            {
                return;
            }

            string name = Path.GetFileNameWithoutExtension(fi.Name);
            if (!name.NullOrEmpty())
            {
                policy.label = name;
            }
        }

        private static Dialog_ManageApparelPolicies FindManageApparelPoliciesDialog()
        {
            foreach (Window window in Find.WindowStack.Windows)
            {
                if (window is Dialog_ManageApparelPolicies apparelDialog)
                {
                    return apparelDialog;
                }
            }

            return null;
        }

        private static ApparelPolicy GetSelectedApparelPolicy(Dialog_ManageApparelPolicies dialog)
        {
            PropertyInfo selectedPolicy = dialog.GetType().GetProperty(
                "SelectedPolicy",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return selectedPolicy?.GetValue(dialog, null) as ApparelPolicy;
        }

        private static void SetSelectedApparelPolicy(Dialog_ManageApparelPolicies dialog, ApparelPolicy policy)
        {
            PropertyInfo selectedPolicy = dialog.GetType().GetProperty(
                "SelectedPolicy",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            selectedPolicy?.SetValue(dialog, policy, null);
        }

        private static bool IsFreshUnnamedPolicy(ApparelPolicy policy)
        {
            if (policy.label.NullOrEmpty())
            {
                return true;
            }

            return policy.label == "UnnamedPolicy".Translate();
        }

        private static string ReadStorageTypeName(object fileListDialog)
        {
            FieldInfo field = AccessTools.Field(
                AccessTools.TypeByName(FileListDialogTypeName),
                "StorageTypeName");
            return field?.GetValue(fileListDialog) as string;
        }

        private static void SetLoadFilterTarget(object loadFilterDialog, ThingFilter filter)
        {
            FieldInfo field = AccessTools.Field(
                AccessTools.TypeByName(LoadFilterDialogTypeName),
                "ThingFilter");
            field?.SetValue(loadFilterDialog, filter);
        }
    }
}
