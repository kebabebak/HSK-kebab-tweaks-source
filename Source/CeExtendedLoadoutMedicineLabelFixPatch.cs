using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Combat Extended Extended Loadout MedicineDefs.Initialize runs from HugsLib
    /// OnDefsLoaded on a LongEvent worker thread. It formats CE_Extended.Medicines with the
    /// medicine LabelCap. Vanilla GrammarResolverSimple.Formatted and
    /// GrammarResolverSimpleStringExtensions share unsynchronized static lists. Concurrent
    /// List.Clear / Array.Clear throws IndexOutOfRangeException (index + length &gt; size).
    /// HugsLib reports the exception and CEEL_GenericMedicine loadout defs are not added.
    ///
    /// Fix: Harmony transpiler on MedicineDefs.Initialize replaces that Translate call with a
    /// keyed lookup plus {0} replace so Formatted is not used. Applied in the Mod ctor before
    /// HugsLib OnDefsLoaded. Soft-skips if Extended Loadout is absent. Restart to toggle.
    /// Does not skip Initialize.
    ///
    /// Проблема: Combat Extended Extended Loadout MedicineDefs.Initialize вызывается из
    /// HugsLib OnDefsLoaded на worker-потоке LongEvent. Строка CE_Extended.Medicines
    /// форматируется с LabelCap лекарства. У vanilla GrammarResolverSimple.Formatted и
    /// GrammarResolverSimpleStringExtensions общие несинхронизированные static-списки.
    /// Параллельный List.Clear / Array.Clear даёт IndexOutOfRangeException
    /// (index + length &gt; size). HugsLib пишет ошибку, дефы CEEL_GenericMedicine не
    /// добавляются.
    ///
    /// Исправление: Harmony transpiler на MedicineDefs.Initialize подменяет этот Translate
    /// на keyed-lookup и замену {0}, без Formatted. Ставится в ctor Mod до HugsLib
    /// OnDefsLoaded. Если Extended Loadout нет — пропуск. Переключение — после рестарта.
    /// Initialize не пропускается.
    /// </summary>
    public static class CeExtendedLoadoutMedicineLabelFixFeatures
    {
        public static void ApplyEarly(Harmony harmony)
        {
            Type medicineDefsType = AccessTools.TypeByName("CombatExtended.ExtendedLoadout.MedicineDefs");
            if (medicineDefsType == null)
            {
                Log.Message(
                    "[HSK kebab tweaks] Combat Extended Extended Loadout not loaded; medicine label fix skipped.");
                return;
            }

            MethodInfo target = AccessTools.Method(medicineDefsType, "Initialize");
            if (target == null)
            {
                Log.Warning(
                    "[HSK kebab tweaks] CombatExtended.ExtendedLoadout.MedicineDefs.Initialize not found; medicine label fix skipped.");
                return;
            }

            harmony.Patch(
                target,
                transpiler: new HarmonyMethod(
                    typeof(MedicineDefs_Initialize_MedicinesTranslate_Transpiler),
                    nameof(MedicineDefs_Initialize_MedicinesTranslate_Transpiler.Transpiler)));

            Log.Message(
                "[HSK kebab tweaks] CE Extended Loadout medicine label fix loaded (Translate without GrammarResolverSimple.Formatted).");
        }

        /// <summary>
        /// Builds the CE_Extended.Medicines loadout label without GrammarResolverSimple.Formatted.
        /// When the fix is off, uses the original one-argument Translate.
        ///
        /// Собирает подпись CE_Extended.Medicines без GrammarResolverSimple.Formatted.
        /// Если фикс выключен — исходный Translate с одним аргументом.
        /// </summary>
        public static TaggedString TranslateMedicinesLoadoutLabel(string key, NamedArgument arg1)
        {
            if (!KebabTweaksSettings.EnableCeExtendedLoadoutMedicineLabelFix)
            {
                return key.Translate(arg1);
            }

            string template = key.Translate();
            string name = arg1.arg != null ? arg1.arg.ToString() : string.Empty;
            if (template.NullOrEmpty())
            {
                return name;
            }

            return GenText.CapitalizeSentences(template.Replace("{0}", name), capitalizeFirstSentence: false);
        }
    }

    /// <summary>
    /// Replaces the one-argument Translate in MedicineDefs.Initialize with
    /// TranslateMedicinesLoadoutLabel. Other Initialize logic is unchanged.
    ///
    /// Подменяет Translate с одним аргументом в MedicineDefs.Initialize на
    /// TranslateMedicinesLoadoutLabel. Остальная логика Initialize без изменений.
    /// </summary>
    internal static class MedicineDefs_Initialize_MedicinesTranslate_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo originalTranslate = AccessTools.Method(
                typeof(TranslatorFormattedStringExtensions),
                nameof(TranslatorFormattedStringExtensions.Translate),
                new[] { typeof(string), typeof(NamedArgument) });
            MethodInfo replacement = AccessTools.Method(
                typeof(CeExtendedLoadoutMedicineLabelFixFeatures),
                nameof(CeExtendedLoadoutMedicineLabelFixFeatures.TranslateMedicinesLoadoutLabel));

            int replaced = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                MethodInfo called = instruction.operand as MethodInfo;
                if (originalTranslate != null && replacement != null && called == originalTranslate)
                {
                    yield return new CodeInstruction(OpCodes.Call, replacement).WithLabels(instruction.labels);
                    replaced++;
                    continue;
                }

                yield return instruction;
            }

            if (replaced == 0)
            {
                Log.Warning(
                    "[HSK kebab tweaks] CE Extended Loadout medicine label transpiler found no Translate(string, NamedArgument) call.");
            }
        }
    }
}
