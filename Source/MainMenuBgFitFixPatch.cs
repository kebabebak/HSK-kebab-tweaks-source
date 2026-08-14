#if RIMWORLD_1_6
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: RimThemesLite NX (1.6) sizes animated main-menu VideoPlayer dest rect with
    /// Screen.width/height while GUI.DrawTexture uses UI.screenWidth/Height. UI Scale &gt; 1 clips
    /// the background.
    ///
    /// Fix: Soft-optional transpiler on RimThemesLite.UI_BackgroundMain_Patch.Prefix replaces
    /// Screen.width/height with UI.screenWidth/Height fields. Applied only when enabled at load.
    ///
    /// Проблема: RimThemesLite NX (1.6) считает dest-rect видео через Screen.*, а DrawTexture в
    /// UI-пространстве RimWorld; при UI Scale &gt; 1 фон обрезается.
    ///
    /// Исправление: Soft-optional транспайлер заменяет Screen.* на UI.screen*. Ставится только при
    /// enable на загрузке.
    /// </summary>
    public static class MainMenuBgFitFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            Type patchType = AccessTools.TypeByName("RimThemesLite.UI_BackgroundMain_Patch");
            if (patchType == null)
            {
                Log.Message("[HSK kebab tweaks] RimThemesLite not loaded; main menu BG fit fix skipped.");
                return;
            }

            MethodInfo target = AccessTools.Method(patchType, "Prefix");
            if (target == null)
            {
                Log.Warning("[HSK kebab tweaks] RimThemesLite UI_BackgroundMain_Patch.Prefix not found; skipped.");
                return;
            }

            if (!MainMenuBgFit_RimThemesVideoBgRect_Transpiler.PrefixUsesScreenDimensions(target))
            {
                Log.Message(
                    "[HSK kebab tweaks] RimThemes main menu BG already uses UI.screenWidth/Height; fit fix skipped.");
                return;
            }

            harmony.Patch(
                target,
                transpiler: new HarmonyMethod(
                    typeof(MainMenuBgFit_RimThemesVideoBgRect_Transpiler),
                    nameof(MainMenuBgFit_RimThemesVideoBgRect_Transpiler.Transpiler)));

            Log.Message("[HSK kebab tweaks] RimThemes main menu BG fit fix loaded (UI.screenWidth/Height).");
        }
    }

    internal static class MainMenuBgFit_RimThemesVideoBgRect_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo screenWidth = AccessTools.PropertyGetter(typeof(Screen), nameof(Screen.width));
            MethodInfo screenHeight = AccessTools.PropertyGetter(typeof(Screen), nameof(Screen.height));
            FieldInfo uiWidth = AccessTools.Field(typeof(UI), nameof(UI.screenWidth));
            FieldInfo uiHeight = AccessTools.Field(typeof(UI), nameof(UI.screenHeight));

            if (uiWidth == null || uiHeight == null)
            {
                Log.Warning(
                    "[HSK kebab tweaks] UI.screenWidth/Height fields not found; RimThemes Prefix unchanged.");
                foreach (CodeInstruction code in instructions)
                {
                    yield return code;
                }

                yield break;
            }

            int replaced = 0;
            foreach (CodeInstruction code in instructions)
            {
                if (code.Calls(screenWidth))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, uiWidth);
                    replaced++;
                    continue;
                }

                if (code.Calls(screenHeight))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, uiHeight);
                    replaced++;
                    continue;
                }

                yield return code;
            }

            if (replaced == 0)
            {
                Log.Message(
                    "[HSK kebab tweaks] RimThemes Prefix: no Screen.width/height calls (upstream may already use UI.screen*).");
            }
        }

        /// <summary>
        /// True when Prefix IL still calls UnityEngine.Screen.width/height getters.
        ///
        /// Истина, если IL Prefix всё ещё вызывает геттеры UnityEngine.Screen.width/height.
        /// </summary>
        internal static bool PrefixUsesScreenDimensions(MethodInfo method)
        {
            if (method?.GetMethodBody()?.GetILAsByteArray() == null)
            {
                return false;
            }

            MethodInfo screenWidth = AccessTools.PropertyGetter(typeof(Screen), nameof(Screen.width));
            MethodInfo screenHeight = AccessTools.PropertyGetter(typeof(Screen), nameof(Screen.height));
            byte[] il = method.GetMethodBody().GetILAsByteArray();
            Module module = method.Module;

            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] != 0x28 && il[i] != 0x6F)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    MethodInfo called = module.ResolveMethod(token) as MethodInfo;
                    if (called == screenWidth || called == screenHeight)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Ignore unresolved metadata tokens while scanning.
                }
            }

            return false;
        }
    }
}
#endif
