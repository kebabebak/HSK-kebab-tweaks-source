using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Debug Log (LudeonTK.EditWindow_Log) starts splitter drag on MouseDown and
    /// clears borderDragging only on Event.rawType == MouseUp. Alt+Tab while held
    /// drops the MouseUp; after focus returns the pane still follows the cursor without LMB.
    ///
    /// Fix: soft Postfix on DoMessageDetails — if borderDragging is set but
    /// Input.GetMouseButton(0) is false, clear the flag (no synthetic click).
    ///
    /// Проблема: Debug Log начинает drag сплиттера на MouseDown и сбрасывает
    /// borderDragging только на Event.rawType == MouseUp. Alt+Tab при зажатой ЛКМ
    /// теряет MouseUp; после возврата панель едет за курсором без кнопки.
    ///
    /// Исправление: soft Postfix на DoMessageDetails — если borderDragging и
    /// Input.GetMouseButton(0) == false, сбросить флаг (без автоклика).
    /// </summary>
    public static class DebugLogSplitterDragFixFeatures
    {
        private static FieldInfo borderDraggingField;

        public static void Apply(Harmony harmony)
        {
            try
            {
                Type logType = AccessTools.TypeByName("LudeonTK.EditWindow_Log");
                if (logType == null)
                {
                    Log.Message("[DebugLogSplitterDragFix] LudeonTK.EditWindow_Log not found; patch skipped.");
                    return;
                }

                MethodBase target = AccessTools.Method(logType, "DoMessageDetails");
                borderDraggingField = AccessTools.Field(logType, "borderDragging");
                if (target == null || borderDraggingField == null || borderDraggingField.FieldType != typeof(bool))
                {
                    Log.Warning(
                        "[DebugLogSplitterDragFix] DoMessageDetails/borderDragging not found; patch skipped.");
                    return;
                }

                harmony.Patch(
                    target,
                    postfix: new HarmonyMethod(
                        typeof(EditWindow_Log_DoMessageDetails_Patch),
                        nameof(EditWindow_Log_DoMessageDetails_Patch.Postfix)));

                Log.Message("[DebugLogSplitterDragFix] Patches applied.");
            }
            catch (Exception ex)
            {
                Log.Error("[DebugLogSplitterDragFix] Failed to apply patches: " + ex);
            }
        }

        internal static void ClearDragIfMouseUp(object window)
        {
            if (window == null || borderDraggingField == null)
            {
                return;
            }

            try
            {
                if (borderDraggingField.GetValue(window) is bool dragging
                    && dragging
                    && !Input.GetMouseButton(0))
                {
                    borderDraggingField.SetValue(window, false);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("[DebugLogSplitterDragFix] Clear drag failed: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Clears stuck Debug Log details-pane drag when LMB is not held.
    ///
    /// Сбрасывает застрявший drag панели деталей Debug Log, если ЛКМ не зажата.
    /// </summary>
    internal static class EditWindow_Log_DoMessageDetails_Patch
    {
        public static void Postfix(object __instance)
        {
            if (!KebabTweaksSettings.EnableDebugLogSplitterDragFix)
            {
                return;
            }

            DebugLogSplitterDragFixFeatures.ClearDragIfMouseUp(__instance);
        }
    }
}
