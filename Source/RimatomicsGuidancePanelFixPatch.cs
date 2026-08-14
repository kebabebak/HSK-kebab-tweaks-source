#if RIMWORLD_1_6
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Rimatomics MainTabWindow_Rimatomics.DrawPanel calls
    /// Widgets.BeginScrollView, then early-returns when the selected project has no
    /// unlock icons (Unlocks / RecipeUnlocks empty). That path skips
    /// Widgets.EndScrollView, so Verse logs
    /// "Mouse position stack is not empty… BeginScrollView than EndScrollView" every frame.
    /// Guidance System (ResearchGuidenceSystem) is the stock project with no unlocks,
    /// so its panel also never draws steps / Start / debug buttons.
    ///
    /// Fix: Soft-optional Harmony transpiler retargets that empty-unlock early br to the
    /// same continue label as the non-empty path (draw zero icons, then the rest of the panel,
    /// then EndScrollView). Skips cleanly when Rimatomics is absent. Applied only when
    /// enabled at load.
    ///
    /// Проблема: MainTabWindow_Rimatomics.DrawPanel в Rimatomics открывает
    /// Widgets.BeginScrollView и при пустом списке иконок разблокировок делает ранний
    /// return без Widgets.EndScrollView. Verse каждый кадр пишет
    /// "Mouse position stack is not empty…". У проекта Guidance System
    /// (ResearchGuidenceSystem) нет unlocks, поэтому не рисуются шаги / Start / debug.
    ///
    /// Исправление: Soft-optional транспайлер перенаправляет ранний br на ту же метку
    /// продолжения, что и при непустом списке (0 иконок → остальной UI → EndScrollView).
    /// Без Rimatomics патч пропускается. Ставится только при enable на загрузке.
    /// </summary>
    public static class RimatomicsGuidancePanelFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            Type windowType = AccessTools.TypeByName("Rimatomics.MainTabWindow_Rimatomics");
            if (windowType == null)
            {
                Log.Message("[HSK kebab tweaks] Rimatomics not loaded; guidance panel fix skipped.");
                return;
            }

            MethodInfo drawPanel = AccessTools.Method(windowType, "DrawPanel");
            if (drawPanel == null)
            {
                Log.Warning("[HSK kebab tweaks] Rimatomics DrawPanel not found; guidance panel fix skipped.");
                return;
            }

            harmony.Patch(
                drawPanel,
                transpiler: new HarmonyMethod(
                    typeof(RimatomicsDrawPanel_EmptyUnlocks_Transpiler),
                    nameof(RimatomicsDrawPanel_EmptyUnlocks_Transpiler.Transpiler)));

            Log.Message("[HSK kebab tweaks] Rimatomics guidance panel fix loaded (DrawPanel empty-unlocks).");
        }
    }

    /// <summary>
    /// Retargets the empty unlock-list branch so it falls through into the unlock-icon loop
    /// (no icons) instead of jumping past EndScrollView.
    ///
    /// Перенаправляет ветку пустого списка unlocks в цикл иконок (без отрисовки), а не за
    /// EndScrollView.
    /// </summary>
    internal static class RimatomicsDrawPanel_EmptyUnlocks_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo endScrollView = AccessTools.Method(typeof(Widgets), nameof(Widgets.EndScrollView));
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            int endIdx = codes.FindIndex(c => c.Calls(endScrollView));
            if (endIdx < 0)
            {
                Log.Warning(
                    "[HSK kebab tweaks] Rimatomics DrawPanel: EndScrollView not found; no changes.");
                return codes;
            }

            int fixedCount = 0;
            for (int i = 1; i < endIdx; i++)
            {
                CodeInstruction br = codes[i];
                if (br.opcode != OpCodes.Br && br.opcode != OpCodes.Br_S)
                {
                    continue;
                }

                CodeInstruction brfalse = codes[i - 1];
                if (brfalse.opcode != OpCodes.Brfalse && brfalse.opcode != OpCodes.Brfalse_S)
                {
                    continue;
                }

                if (!(br.operand is Label earlyLabel))
                {
                    continue;
                }

                int targetIdx = codes.FindIndex(c => c.labels != null && c.labels.Contains(earlyLabel));
                // Empty-unlocks bug: branch lands on / after EndScrollView (skips the matching End).
                if (targetIdx < 0 || targetIdx <= endIdx)
                {
                    continue;
                }

                br.operand = brfalse.operand;
                fixedCount++;
            }

            if (fixedCount == 0)
            {
                Log.Warning(
                    "[HSK kebab tweaks] Rimatomics DrawPanel: no empty-unlocks early exit past EndScrollView; "
                    + "upstream may already be fixed.");
            }
            else
            {
                Log.Message(
                    "[HSK kebab tweaks] Rimatomics DrawPanel: retargeted " + fixedCount
                    + " empty-unlock early exit(s).");
            }

            return codes;
        }
    }
}
#endif
