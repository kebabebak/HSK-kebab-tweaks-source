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
    /// Problem: Map Preview v1.12.25 expects a fixed vanilla Rand burn (legacy 1) before
    /// Map.FillComponents. On RimWorld 1.6 + HSK Alpha that baseline drifts between
    /// sessions/loadouts (observed 32, 58, 7), so a hardcoded rewrite still logs false
    /// accuracy Errors and SkipIterations desyncs previews.
    ///
    /// Fix: Soft-optional — make the FillComponents_Prefix expected value equal the measured
    /// _prevRandIt (check always passes), capture that delta, and feed it into
    /// ConstructMinimalMapComponents SkipIterations.
    ///
    /// Проблема: Map Preview v1.12.25 ждёт фиксированный burn Rand до FillComponents.
    /// На RW 1.6 + HSK Alpha baseline плавает (32 / 58 / 7) — константный rewrite снова даёт
    /// ложный Error и рассинхрон превью.
    ///
    /// Исправление: Soft-optional — expected в FillComponents_Prefix = измеренный
    /// _prevRandIt, тот же delta идёт в SkipIterations для ConstructMinimal.
    /// </summary>
    public static class MapPreviewRngBaselineFixFeatures
    {
        private static FieldInfo prevRandItField;
        private static uint lastObservedBaseline = 1;

        public static void Apply(Harmony harmony)
        {
            Type patchType = AccessTools.TypeByName("MapPreview.Patches.Patch_Verse_Map");
            Type generatorType = AccessTools.TypeByName("MapPreview.MapPreviewGenerator");
            if (patchType == null || generatorType == null)
            {
                Log.Message("[HSK kebab tweaks] Map Preview not loaded; Map Preview RNG baseline fix skipped.");
                return;
            }

            MethodInfo fillPrefix = AccessTools.Method(patchType, "FillComponents_Prefix");
            MethodInfo constructMinimal = AccessTools.Method(generatorType, "ConstructMinimalMapComponents");
            if (fillPrefix == null || constructMinimal == null)
            {
                Log.Warning("[HSK kebab tweaks] Map Preview target methods not found; RNG baseline fix skipped.");
                return;
            }

            prevRandItField = AccessTools.Field(patchType, "_prevRandIt");
            if (prevRandItField == null)
            {
                Log.Warning("[HSK kebab tweaks] Map Preview _prevRandIt field not found; RNG baseline fix skipped.");
                return;
            }

            harmony.Patch(
                fillPrefix,
                postfix: new HarmonyMethod(
                    typeof(MapPreviewFillComponents_Prefix_Capture),
                    nameof(MapPreviewFillComponents_Prefix_Capture.Postfix)),
                transpiler: new HarmonyMethod(
                    typeof(MapPreviewFillComponents_Prefix_Transpiler),
                    nameof(MapPreviewFillComponents_Prefix_Transpiler.Transpiler)));
            harmony.Patch(
                constructMinimal,
                transpiler: new HarmonyMethod(
                    typeof(MapPreviewConstructMinimalMapComponents_Transpiler),
                    nameof(MapPreviewConstructMinimalMapComponents_Transpiler.Transpiler)));

            Log.Message(
                "[HSK kebab tweaks] Map Preview RNG baseline fix loaded (dynamic expected + SkipIterations).");
        }

        /// <summary>
        /// Last measured Map Preview vanilla component Rand delta (for preview SkipIterations).
        ///
        /// Последний измеренный delta Rand для SkipIterations превью.
        /// </summary>
        public static int GetObservedBaselineOrDefault()
        {
            uint value = lastObservedBaseline;
            return value == 0 ? 1 : (int)value;
        }

        internal static void CaptureObservedBaseline()
        {
            if (prevRandItField == null)
            {
                return;
            }

            object raw = prevRandItField.GetValue(null);
            if (raw is uint u)
            {
                lastObservedBaseline = u == 0 ? 1u : u;
            }
        }

        internal static FieldInfo PrevRandItField => prevRandItField;
    }

    /// <summary>
    /// Captures _prevRandIt after Map Preview's Prefix so ConstructMinimal can match it.
    ///
    /// Сохраняет _prevRandIt после Prefix Map Preview для ConstructMinimal.
    /// </summary>
    internal static class MapPreviewFillComponents_Prefix_Capture
    {
        public static void Postfix()
        {
            MapPreviewRngBaselineFixFeatures.CaptureObservedBaseline();
        }
    }

    /// <summary>
    /// Rewrites expected-baseline ldc.i4.1 to ldsfld _prevRandIt so the check always matches.
    ///
    /// Меняет expected ldc.i4.1 на ldsfld _prevRandIt — проверка всегда проходит.
    /// </summary>
    internal static class MapPreviewFillComponents_Prefix_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo prevRandIt = MapPreviewRngBaselineFixFeatures.PrevRandItField;
            int replaced = 0;
            foreach (CodeInstruction ins in instructions)
            {
                if (ins.opcode == OpCodes.Ldc_I4_1 && replaced < 2 && prevRandIt != null)
                {
                    replaced++;
                    yield return new CodeInstruction(OpCodes.Ldsfld, prevRandIt);
                    continue;
                }

                yield return ins;
            }

            if (replaced != 2)
            {
                Log.Warning(
                    "[HSK kebab tweaks] Map Preview FillComponents_Prefix: expected 2 ldc.i4.1→ldsfld rewrites, got "
                    + replaced
                    + ".");
            }
        }
    }

    /// <summary>
    /// Rewrites SkipIterations(1) to SkipIterations(GetObservedBaselineOrDefault()).
    ///
    /// Меняет SkipIterations(1) на вызов с последним измеренным baseline.
    /// </summary>
    internal static class MapPreviewConstructMinimalMapComponents_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo skipIterations = AccessTools.Method(
                AccessTools.TypeByName("MapPreview.Patches.Patch_Verse_Rand"),
                "SkipIterations");
            MethodInfo getBaseline = AccessTools.Method(
                typeof(MapPreviewRngBaselineFixFeatures),
                nameof(MapPreviewRngBaselineFixFeatures.GetObservedBaselineOrDefault));

            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            int replaced = 0;
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (list[i].opcode == OpCodes.Ldc_I4_1
                    && list[i + 1].opcode == OpCodes.Call
                    && list[i + 1].operand as MethodInfo == skipIterations
                    && getBaseline != null)
                {
                    list[i] = new CodeInstruction(OpCodes.Call, getBaseline);
                    replaced++;
                    break;
                }
            }

            if (replaced != 1)
            {
                Log.Warning(
                    "[HSK kebab tweaks] Map Preview ConstructMinimalMapComponents: expected 1 SkipIterations rewrite, got "
                    + replaced
                    + ".");
            }

            return list;
        }
    }
}
#endif
