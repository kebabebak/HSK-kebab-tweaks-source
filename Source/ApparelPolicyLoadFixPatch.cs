using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: [KV] Save Storage Settings writes a loaded apparel policy only into the ThingFilter of the
    /// ApparelPolicy selected in Dialog_ManageApparelPolicies (IOUtil.LoadFilters) and leaves the policy name
    /// unchanged. The wardrobe window does not show that Load overwrites the selected policy. The stand-in
    /// policy created when the file list opens stayed in the list when that list closed without a file.
    /// Load as new already calls OutfitDatabase.MakeNewOutfit and selects that policy before the list opens.
    /// MakeNewOutfit names it ApparelPolicy plus its id, so a name check for UnnamedPolicy does not see it.
    ///
    /// Fix: when the fix is enabled and LoadFilterDialog for Apparel_Management opens on a selected policy
    /// that MakeNewOutfit did not return on this frame, create a new ApparelPolicy, select it, and point the
    /// dialog filter at it. After a file is chosen, set the label from the file name. If that dialog closes
    /// without a file, OutfitDatabase.TryDelete removes only that created instance and the previous selection
    /// is restored. TryDelete does not open Dialog_Confirm. The created policy is kept when the label no
    /// longer matches the name from MakeNewOutfit, it is the default outfit, it is already absent from
    /// OutfitDatabase, or a living pawn still has it (TryDelete rejects and no message is shown). Load as new
    /// is left to Save Storage Settings.
    ///
    /// Проблема: [KV] Save Storage Settings пишет загруженную политику одежды только в ThingFilter выбранной
    /// в Dialog_ManageApparelPolicies ApparelPolicy (IOUtil.LoadFilters) и не меняет имя. По окну гардероба
    /// не видно, что «Загрузить» перезаписывает выбранную политику. Политика, созданная при открытии списка
    /// файлов, оставалась в списке, если список закрывали без файла. «Загрузить как новый» уже вызывает
    /// OutfitDatabase.MakeNewOutfit и выбирает эту политику до открытия списка. MakeNewOutfit называет её
    /// ApparelPolicy и id, поэтому проверка имени UnnamedPolicy её не узнаёт.
    ///
    /// Исправление: при включённом фиксе, если LoadFilterDialog для Apparel_Management открывается на политике,
    /// которую MakeNewOutfit не вернул в этом кадре, создать новую ApparelPolicy, выбрать её и направить фильтр
    /// диалога на неё. После выбора файла имя берётся из имени файла. Если диалог закрыт без файла,
    /// OutfitDatabase.TryDelete удаляет только этот созданный экземпляр и возвращает прежний выбор. TryDelete
    /// не открывает Dialog_Confirm. Созданная политика остаётся, если имя уже не то, что дал MakeNewOutfit, это
    /// политика по умолчанию, её уже нет в OutfitDatabase, или её носит живой поселенец (TryDelete отклоняет
    /// удаление и сообщение не показывается). «Загрузить как новый» остаётся на Save Storage Settings.
    /// </summary>
    public static class ApparelPolicyLoadFixFeatures
    {
        private const string ApparelStorageType = "Apparel_Management";
        private const string LoadFilterDialogTypeName = "SaveStorageSettings.Dialog.LoadFilterDialog";
        private const string FileListDialogTypeName = "SaveStorageSettings.Dialog.FileListDialog";

        private static ApparelPolicy pendingRenamePolicy;
        private static ApparelPolicy pendingPreviousPolicy;
        private static string pendingCreatedLabel;
        private static string pendingFileName;
        private static Window pendingLoadWindow;
        private static bool pendingLoadCommitted;
        private static ApparelPolicy outfitCreatedThisFrame;
        private static int outfitCreatedFrame = -1;

        /// <summary>
        /// Registers wardrobe load hooks when Save Storage Settings is present.
        ///
        /// Подключает хуки загрузки гардероба при наличии Save Storage Settings.
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
            harmony.Patch(
                AccessTools.Method(typeof(Window), nameof(Window.PostClose)),
                postfix: new HarmonyMethod(typeof(ApparelPolicyLoadFixFeatures), nameof(Window_PostClose_Postfix)));
            harmony.Patch(
                AccessTools.Method(typeof(OutfitDatabase), nameof(OutfitDatabase.MakeNewOutfit)),
                postfix: new HarmonyMethod(typeof(ApparelPolicyLoadFixFeatures), nameof(OutfitDatabase_MakeNewOutfit_Postfix)));

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
                prefix: new HarmonyMethod(typeof(ApparelPolicyLoadFixFeatures), nameof(LoadFilterDialog_DoFileInteraction_Prefix)),
                postfix: new HarmonyMethod(typeof(ApparelPolicyLoadFixFeatures), nameof(LoadFilterDialog_DoFileInteraction_Postfix)));
        }

        /// <summary>
        /// Redirects apparel load into a new policy when loading over an existing policy.
        /// Load as new already created the selected policy in this frame, so that click is left alone.
        ///
        /// Перенаправляет загрузку гардероба в новую политику. «Загрузить как новый» в этом кадре уже создал
        /// выбранную политику, этот клик не дублируется.
        /// </summary>
        public static void WindowStack_Add_Prefix(Window window)
        {
            ReleaseClosedLoadIfPending();
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
            ThingFilter dialogFilter = ReadLoadFilterTarget(window);
            if (IsLoadAsNewTarget(selected, dialogFilter))
            {
                return;
            }

            if (selected == null)
            {
                return;
            }

            FieldInfo filterField = AccessTools.Field(
                AccessTools.TypeByName(LoadFilterDialogTypeName),
                "ThingFilter");
            if (filterField == null || Current.Game == null)
            {
                return;
            }

            ApparelPolicy created = Current.Game.outfitDatabase.MakeNewOutfit();
            pendingRenamePolicy = created;
            pendingPreviousPolicy = selected;
            pendingCreatedLabel = created.label;
            pendingFileName = null;
            pendingLoadWindow = window;
            pendingLoadCommitted = false;
            SetSelectedApparelPolicy(manageDialog, created);
            filterField.SetValue(window, created.filter);
        }

        /// <summary>
        /// Records the chosen file before Close. Window.PostClose runs inside that Close, before this method returns.
        ///
        /// Запоминает выбранный файл до Close. Window.PostClose выполняется внутри этого Close, до возврата из метода.
        /// </summary>
        public static void LoadFilterDialog_DoFileInteraction_Prefix(object __instance, FileInfo fi)
        {
            if (fi == null || pendingLoadWindow == null || !ReferenceEquals(__instance, pendingLoadWindow))
            {
                return;
            }

            pendingLoadCommitted = true;
            pendingFileName = fi.Name;
        }

        /// <summary>
        /// Applies the saved file name if Close did not already run the rename.
        ///
        /// Задаёт имя из файла, если Close ещё не переименовал политику.
        /// </summary>
        public static void LoadFilterDialog_DoFileInteraction_Postfix(object __instance, FileInfo fi)
        {
            if (pendingLoadWindow == null || !ReferenceEquals(__instance, pendingLoadWindow))
            {
                return;
            }

            if (pendingLoadCommitted && fi != null)
            {
                pendingFileName = fi.Name;
                ApplyPendingFileName();
            }

            ClearPendingLoad();
        }

        /// <summary>
        /// After a chosen file, renames the stand-in policy. If the file list closed without one, deletes that policy.
        ///
        /// После выбранного файла переименовывает новую политику. Если список закрыт без файла, удаляет её.
        /// </summary>
        public static void Window_PostClose_Postfix(Window __instance)
        {
            if (pendingLoadWindow == null || !ReferenceEquals(__instance, pendingLoadWindow))
            {
                return;
            }

            if (pendingLoadCommitted)
            {
                ApplyPendingFileName();
                ClearPendingLoad();
                return;
            }

            DiscardUnloadedPolicy();
        }

        /// <summary>
        /// Remembers the policy MakeNewOutfit just returned so the same click's Load as new is not duplicated.
        ///
        /// Запоминает политику, которую только что вернул MakeNewOutfit, чтобы «Загрузить как новый» в этом клике не дублировался.
        /// </summary>
        public static void OutfitDatabase_MakeNewOutfit_Postfix(ApparelPolicy __result)
        {
            outfitCreatedThisFrame = __result;
            outfitCreatedFrame = Time.frameCount;
        }

        /// <summary>
        /// Removes only the stand-in policy created for this file list. OutfitDatabase.TryDelete does not open
        /// Dialog_Confirm. A living pawn using the policy makes TryDelete reject it, and no message is shown.
        /// The default outfit is the first entry; DefaultOutfit is not called because an empty list makes it create one.
        ///
        /// Удаляет только политику, созданную для этого списка файлов. OutfitDatabase.TryDelete не открывает
        /// Dialog_Confirm. Если политику носит живой поселенец, TryDelete отклоняет удаление, сообщение не показывается.
        /// Политика по умолчанию - первая в списке; DefaultOutfit не вызывается, потому что при пустом списке он создаёт новую.
        /// </summary>
        private static void DiscardUnloadedPolicy()
        {
            ApparelPolicy policy = pendingRenamePolicy;
            ApparelPolicy previous = pendingPreviousPolicy;
            string createdLabel = pendingCreatedLabel;
            ClearPendingLoad();
            if (policy == null || Current.Game == null)
            {
                return;
            }

            OutfitDatabase database = Current.Game.outfitDatabase;
            if (database == null || ReferenceEquals(policy, previous))
            {
                return;
            }

            bool listed = database.AllOutfits.Contains(policy);
            bool isDefault = database.AllOutfits.Count > 0 && ReferenceEquals(policy, database.AllOutfits[0]);
            bool safeToRemove = listed && !isDefault && policy.label == createdLabel;
            Dialog_ManageApparelPolicies manageDialog = FindManageApparelPoliciesDialog();
            if (!listed)
            {
                RestoreSelection(manageDialog, policy, previous, database);
                return;
            }

            if (!safeToRemove)
            {
                return;
            }

            RestoreSelection(manageDialog, policy, previous, database);
            if (!database.TryDelete(policy).Accepted
                && manageDialog != null
                && database.AllOutfits.Contains(policy))
            {
                SetSelectedApparelPolicy(manageDialog, policy);
            }
        }

        /// <summary>
        /// Points the wardrobe dialog back at the policy that was selected before the stand-in, when it still lists that stand-in.
        ///
        /// Возвращает в диалоге гардероба политику, выбранную до временной, если в диалоге всё ещё выбрана временная.
        /// </summary>
        private static void RestoreSelection(
            Dialog_ManageApparelPolicies manageDialog,
            ApparelPolicy policy,
            ApparelPolicy previous,
            OutfitDatabase database)
        {
            if (manageDialog == null || !ReferenceEquals(GetSelectedApparelPolicy(manageDialog), policy))
            {
                return;
            }

            if (previous != null && !ReferenceEquals(previous, policy) && database.AllOutfits.Contains(previous))
            {
                SetSelectedApparelPolicy(manageDialog, previous);
                return;
            }

            SetSelectedApparelPolicy(manageDialog, null);
        }

        /// <summary>
        /// Finishes a stand-in whose file list is already gone, when PostClose did not run.
        ///
        /// Завершает временную политику, если список файлов уже закрыт, а PostClose не вызывался.
        /// </summary>
        private static void ReleaseClosedLoadIfPending()
        {
            if (pendingLoadWindow == null || IsWindowOnStack(pendingLoadWindow))
            {
                return;
            }

            if (pendingLoadCommitted)
            {
                ApplyPendingFileName();
                ClearPendingLoad();
                return;
            }

            DiscardUnloadedPolicy();
        }

        /// <summary>
        /// Sets the stand-in label from the chosen file while the label is still the one MakeNewOutfit assigned.
        ///
        /// Ставит имя временной политики из выбранного файла, пока имя всё ещё то, что задал MakeNewOutfit.
        /// </summary>
        private static void ApplyPendingFileName()
        {
            ApparelPolicy policy = pendingRenamePolicy;
            if (policy == null || pendingFileName.NullOrEmpty() || Current.Game == null)
            {
                return;
            }

            OutfitDatabase database = Current.Game.outfitDatabase;
            if (database == null || !database.AllOutfits.Contains(policy) || policy.label != pendingCreatedLabel)
            {
                return;
            }

            string name = Path.GetFileNameWithoutExtension(pendingFileName);
            if (!name.NullOrEmpty())
            {
                policy.label = name;
            }
        }

        private static void ClearPendingLoad()
        {
            pendingRenamePolicy = null;
            pendingPreviousPolicy = null;
            pendingCreatedLabel = null;
            pendingFileName = null;
            pendingLoadWindow = null;
            pendingLoadCommitted = false;
        }

        private static bool IsLoadAsNewTarget(ApparelPolicy selected, ThingFilter dialogFilter)
        {
            return selected != null
                && dialogFilter != null
                && ReferenceEquals(dialogFilter, selected.filter)
                && ReferenceEquals(selected, outfitCreatedThisFrame)
                && outfitCreatedFrame == Time.frameCount;
        }

        private static bool IsWindowOnStack(Window window)
        {
            foreach (Window open in Find.WindowStack.Windows)
            {
                if (ReferenceEquals(open, window))
                {
                    return true;
                }
            }

            return false;
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

        private static string ReadStorageTypeName(object fileListDialog)
        {
            FieldInfo field = AccessTools.Field(
                AccessTools.TypeByName(FileListDialogTypeName),
                "StorageTypeName");
            return field?.GetValue(fileListDialog) as string;
        }

        private static ThingFilter ReadLoadFilterTarget(object loadFilterDialog)
        {
            FieldInfo field = AccessTools.Field(
                AccessTools.TypeByName(LoadFilterDialogTypeName),
                "ThingFilter");
            return field?.GetValue(loadFilterDialog) as ThingFilter;
        }
    }
}
