using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Grouped Pawns Lists (PawnTableGrouped.GroupColumnWorker_MedicalCare) keeps a
    /// private careTextures array of length 5 (vanilla MedicalCareCategory 0..4). Mod Medicine
    /// Patch (and similar) assigns MedicalCareCategory values &gt;= 5 and expands
    /// MedicalCareUtility.careTextures. Group header DoCell indexes PTG's fixed array →
    /// IndexOutOfRangeException. PTG catches it, logs that the MedicalCare group header cell
    /// failed, and disables the cell until game restart — so the error appears once after a
    /// full restart, then not again until the next restart.
    ///
    /// Fix: soft Harmony (no hard refs to PTG / MMP). Before DoCell, sync PTG careTextures from
    /// MedicalCareUtility.careTextures. GenerateMenu transpiler replaces hardcoded 5 with the
    /// actual texture count. If PTG is not loaded, patches are skipped. If MMP is not loaded,
    /// lengths stay 5 (no-op).
    ///
    /// Проблема: Grouped Pawns Lists (PawnTableGrouped.GroupColumnWorker_MedicalCare) держит
    /// приватный careTextures длины 5 (vanilla MedicalCareCategory 0..4). Mod Medicine Patch
    /// (и аналоги) задают MedicalCareCategory &gt;= 5 и расширяют MedicalCareUtility.careTextures.
    /// Group header DoCell индексирует фиксированный массив PTG → IndexOutOfRangeException.
    /// PTG ловит исключение, пишет в лог и отключает ячейку до рестарта игры — поэтому ошибка
    /// видна разово после полного рестарта, пока колонка снова не включится.
    ///
    /// Исправление: soft Harmony (без hard-ref на PTG / MMP). Перед DoCell синхронизировать
    /// PTG careTextures с MedicalCareUtility.careTextures. Transpiler GenerateMenu заменяет
    /// hardcoded 5 на фактическую длину. Без PTG патчи не ставятся; без MMP длины остаются 5.
    /// </summary>
    public static class PtgMedicalCareFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                Type workerType = AccessTools.TypeByName("PawnTableGrouped.GroupColumnWorker_MedicalCare");
                if (workerType == null)
                {
                    Log.Message("[PtgMedicalCarePatch] Grouped Pawns Lists not loaded; patch skipped.");
                    return;
                }

                if (!CareTextureSync.TryBind(workerType))
                {
                    Log.Warning("[PtgMedicalCarePatch] Could not bind PTG careTextures field; patch skipped.");
                    return;
                }

                MethodBase doCell = AccessTools.Method(workerType, "DoCell");
                MethodBase generateMenu = AccessTools.Method(workerType, "MedicalCareSelectButton_GenerateMenu");
                if (doCell == null)
                {
                    Log.Warning("[PtgMedicalCarePatch] GroupColumnWorker_MedicalCare.DoCell not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    doCell,
                    prefix: new HarmonyMethod(
                        typeof(GroupColumnWorker_MedicalCare_DoCell_Patch),
                        nameof(GroupColumnWorker_MedicalCare_DoCell_Patch.Prefix)));

                if (generateMenu != null)
                {
                    harmony.Patch(
                        generateMenu,
                        transpiler: new HarmonyMethod(
                            typeof(GroupColumnWorker_MedicalCare_GenerateMenu_Patch),
                            nameof(GroupColumnWorker_MedicalCare_GenerateMenu_Patch.Transpiler)));
                }

                Log.Message(
                    "[PtgMedicalCarePatch] Loaded (soft PTG MedicalCare sync" +
                    (PtgMedicalCareModCompatibility.IsModMedicinePatchLoaded() ? "; Mod Medicine Patch detected" : string.Empty) +
                    ").");
            }
            catch (Exception ex)
            {
                Log.Error("[PtgMedicalCarePatch] Failed to apply patches: " + ex);
            }
        }
    }

    internal static class PtgMedicalCareModCompatibility
    {
        private const string ModMedicinePatchPackageId = "antaioz.modmedicinepatch";

        public static bool IsModMedicinePatchLoaded()
        {
            if (ModsConfig.IsActive(ModMedicinePatchPackageId))
            {
                return true;
            }

            return LoadedModManager.RunningModsListForReading.Exists(
                mod => string.Equals(mod.PackageId, ModMedicinePatchPackageId, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Keeps PTG's private Resource&lt;Texture2D&gt;[] careTextures aligned with
    /// MedicalCareUtility.careTextures (expanded by Mod Medicine Patch when present).
    ///
    /// Держит приватный Resource&lt;Texture2D&gt;[] careTextures PTG в соответствии с
    /// MedicalCareUtility.careTextures (расширяется Mod Medicine Patch при наличии).
    /// </summary>
    internal static class CareTextureSync
    {
        private static FieldInfo ptgCareTexturesField;
        private static FieldInfo vanillaCareTexturesField;
        private static MethodInfo resourceFromTexture;
        private static Type resourceElementType;

        public static bool TryBind(Type workerType)
        {
            ptgCareTexturesField = AccessTools.Field(workerType, "careTextures");
            vanillaCareTexturesField = AccessTools.Field(typeof(MedicalCareUtility), "careTextures");
            if (ptgCareTexturesField == null || vanillaCareTexturesField == null)
            {
                return false;
            }

            resourceElementType = ptgCareTexturesField.FieldType.GetElementType();
            if (resourceElementType == null)
            {
                return false;
            }

            resourceFromTexture = AccessTools.Method(
                resourceElementType,
                "op_Implicit",
                new[] { typeof(Texture2D) });
            return resourceFromTexture != null;
        }

        public static int GetCareTextureCount()
        {
            Texture2D[] vanilla = GetVanillaCareTextures();
            if (vanilla != null && vanilla.Length > 0)
            {
                return vanilla.Length;
            }

            Array ptg = ptgCareTexturesField?.GetValue(null) as Array;
            return ptg?.Length ?? 5;
        }

        public static void SyncFromVanilla()
        {
            if (ptgCareTexturesField == null || resourceFromTexture == null)
            {
                return;
            }

            Texture2D[] vanilla = GetVanillaCareTextures();
            if (vanilla == null || vanilla.Length == 0)
            {
                return;
            }

            Array current = ptgCareTexturesField.GetValue(null) as Array;
            if (current != null && current.Length == vanilla.Length && !HasNullSlots(current))
            {
                return;
            }

            Array rebuilt = Array.CreateInstance(resourceElementType, vanilla.Length);
            for (int i = 0; i < vanilla.Length; i++)
            {
                Texture2D tex = vanilla[i];
                if (tex == null)
                {
                    continue;
                }

                rebuilt.SetValue(resourceFromTexture.Invoke(null, new object[] { tex }), i);
            }

            ptgCareTexturesField.SetValue(null, rebuilt);
        }

        private static Texture2D[] GetVanillaCareTextures()
        {
            return vanillaCareTexturesField?.GetValue(null) as Texture2D[];
        }

        private static bool HasNullSlots(Array array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array.GetValue(i) == null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal static class GroupColumnWorker_MedicalCare_DoCell_Patch
    {
        public static void Prefix()
        {
            if (!KebabTweaksSettings.EnablePtgMedicalCare)
            {
                return;
            }

            CareTextureSync.SyncFromVanilla();
        }
    }

    /// <summary>
    /// Replaces the hardcoded loop bound (5) with MedicalCareUtility / synced careTextures length.
    ///
    /// Заменяет hardcoded границу цикла (5) на длину MedicalCareUtility / синхронизированного
    /// careTextures.
    /// </summary>
    internal static class GroupColumnWorker_MedicalCare_GenerateMenu_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo getCount = AccessTools.Method(typeof(CareTextureSync), nameof(CareTextureSync.GetCareTextureCount));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_5)
                {
                    yield return new CodeInstruction(OpCodes.Call, getCount);
                    continue;
                }

                yield return instruction;
            }
        }
    }
}
