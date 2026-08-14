using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: SeedsPlease WorkGiver_GrowerSowWithSeeds.JobOnCell compares CarriedThing.def to
    /// job.targetB.Thing.def after calling vanilla WorkGiver_GrowerSow.JobOnCell, which never sets
    /// TargetB. When the pawn is carrying anything, TargetB.Thing is null → NullReferenceException
    /// (logged by While You Are Nearby as GrowerSow). JobDriver_PlantSowWithSeeds.MakeNewToils uses
    /// the same unsafe TargetB.def check; MakeNewToils also yields a variable toil count, so on save
    /// load curToilIndex can exceed the rebuilt list ("only has N toils").
    ///
    /// Fix: transpiler replaces TargetB.Thing.def with plantDefToSow.blueprintDef in JobOnCell and
    /// MakeNewToils; Finalizer on PlantableCells soft-fails NRE to 0; SetupToils Postfix clamps
    /// curToilIndex for JobDriver_PlantSowWithSeeds. Soft-skips if SeedsPlease types are absent.
    ///
    /// Проблема: SeedsPlease WorkGiver_GrowerSowWithSeeds.JobOnCell сравнивает CarriedThing.def с
    /// job.targetB.Thing.def после вызова vanilla WorkGiver_GrowerSow.JobOnCell, где TargetB не
    /// задаётся. Если пешка что-то несёт, TargetB.Thing == null → NullReferenceException (While You
    /// Are Nearby пишет это как ошибку GrowerSow). Тот же небезопасный TargetB.def в
    /// JobDriver_PlantSowWithSeeds.MakeNewToils; число toils зависит от ветки, поэтому при load
    /// curToilIndex может выйти за длину списка ("only has N toils").
    ///
    /// Исправление: transpiler подменяет TargetB.Thing.def на plantDefToSow.blueprintDef в
    /// JobOnCell и MakeNewToils; Finalizer PlantableCells при NRE возвращает 0; Postfix SetupToils
    /// ограничивает curToilIndex для JobDriver_PlantSowWithSeeds. Если SeedsPlease нет — патч
    /// пропускается.
    /// </summary>
    public static class SeedsPleaseSowFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                Type workGiverType = AccessTools.TypeByName("SeedsPlease.WorkGiver_GrowerSowWithSeeds");
                Type driverType = AccessTools.TypeByName("SeedsPlease.JobDriver_PlantSowWithSeeds");
                if (workGiverType == null || driverType == null)
                {
                    Log.Message("[SeedsPleaseSowFixPatch] SeedsPlease types not found; patch skipped.");
                    return;
                }

                MethodBase jobOnCell = AccessTools.Method(workGiverType, "JobOnCell");
                if (jobOnCell != null)
                {
                    harmony.Patch(
                        jobOnCell,
                        transpiler: new HarmonyMethod(
                            typeof(TargetBDefToBlueprintDefTranspiler),
                            nameof(TargetBDefToBlueprintDefTranspiler.Transpile)));
                }
                else
                {
                    Log.Warning("[SeedsPleaseSowFixPatch] WorkGiver_GrowerSowWithSeeds.JobOnCell not found.");
                }

                // MakeNewToils is an iterator; the TargetB.Thing.def comparison lives in MoveNext.
                MethodBase makeNewToilsMoveNext = FindMakeNewToilsMoveNext(driverType);
                if (makeNewToilsMoveNext != null)
                {
                    harmony.Patch(
                        makeNewToilsMoveNext,
                        transpiler: new HarmonyMethod(
                            typeof(TargetBDefToBlueprintDefTranspiler),
                            nameof(TargetBDefToBlueprintDefTranspiler.Transpile)));
                }
                else
                {
                    Log.Warning("[SeedsPleaseSowFixPatch] JobDriver_PlantSowWithSeeds MakeNewToils MoveNext not found.");
                }

                MethodBase plantableCells = AccessTools.Method(workGiverType, "PlantableCells");
                if (plantableCells != null)
                {
                    harmony.Patch(
                        plantableCells,
                        finalizer: new HarmonyMethod(
                            typeof(WorkGiver_PlantableCells_Finalizer),
                            nameof(WorkGiver_PlantableCells_Finalizer.Finalizer)));
                }

                MethodBase setupToils = AccessTools.Method(typeof(JobDriver), "SetupToils");
                if (setupToils != null)
                {
                    harmony.Patch(
                        setupToils,
                        postfix: new HarmonyMethod(
                            typeof(JobDriver_SetupToils_SowWithSeedsClamp_Patch),
                            nameof(JobDriver_SetupToils_SowWithSeedsClamp_Patch.Postfix)));
                }

                Log.Message("[SeedsPleaseSowFixPatch] Loaded (SeedsPlease GrowerSow TargetB NRE + SowWithSeeds toil clamp).");
            }
            catch (Exception ex)
            {
                Log.Error("[SeedsPleaseSowFixPatch] Failed to apply patches: " + ex);
            }
        }

        private static MethodBase FindMakeNewToilsMoveNext(Type driverType)
        {
            foreach (Type nested in driverType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (nested == null || nested.Name == null || nested.Name.IndexOf("MakeNewToils", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                MethodBase moveNext = AccessTools.Method(nested, "MoveNext");
                if (moveNext != null)
                {
                    return moveNext;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Replaces Job.targetB.Thing.def loads with Job.plantDefToSow.blueprintDef (seed ThingDef).
    /// SeedsPlease typically uses ldflda targetB (struct address) then LocalTargetInfo.get_Thing.
    ///
    /// Подменяет обращения Job.targetB.Thing.def на Job.plantDefToSow.blueprintDef.
    /// SeedsPlease обычно делает ldflda targetB (адрес структуры) и LocalTargetInfo.get_Thing.
    /// </summary>
    internal static class TargetBDefToBlueprintDefTranspiler
    {
        public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            List<CodeInstruction> codes = instructions.ToList();
            FieldInfo targetB = AccessTools.Field(typeof(Job), nameof(Job.targetB));
            FieldInfo plantDefToSow = AccessTools.Field(typeof(Job), nameof(Job.plantDefToSow));
            FieldInfo blueprintDef = AccessTools.Field(typeof(BuildableDef), nameof(BuildableDef.blueprintDef));
            FieldInfo thingDef = AccessTools.Field(typeof(Thing), nameof(Thing.def));
            MethodInfo getThing = AccessTools.PropertyGetter(typeof(LocalTargetInfo), nameof(LocalTargetInfo.Thing));

            if (targetB == null || plantDefToSow == null || blueprintDef == null || thingDef == null || getThing == null)
            {
                Log.Warning($"[SeedsPleaseSowFixPatch] Transpiler field/method resolve failed for {original?.Name}; leaving IL unchanged.");
                return codes;
            }

            int replacements = 0;
            for (int i = 0; i < codes.Count - 2; i++)
            {
                if (!IsTargetBLoad(codes[i], targetB))
                {
                    continue;
                }

                if (!IsCallTo(codes[i + 1], getThing))
                {
                    continue;
                }

                if (!codes[i + 2].LoadsField(thingDef))
                {
                    continue;
                }

                codes[i] = new CodeInstruction(OpCodes.Ldfld, plantDefToSow).WithLabels(codes[i].labels);
                codes[i + 1] = new CodeInstruction(OpCodes.Ldfld, blueprintDef);
                codes[i + 2] = new CodeInstruction(OpCodes.Nop);
                replacements++;
            }

            if (replacements == 0)
            {
                Log.Message(
                    $"[SeedsPleaseSowFixPatch] No TargetB.Thing.def pattern in {original?.DeclaringType?.Name}.{original?.Name}; "
                    + "IL unchanged (upstream may have fixed this path).");
            }
            else
            {
                Log.Message($"[SeedsPleaseSowFixPatch] Transpiler replaced {replacements} TargetB.Thing.def → blueprintDef in {original?.DeclaringType?.Name}.{original?.Name}.");
            }

            return codes;
        }

        private static bool IsTargetBLoad(CodeInstruction instruction, FieldInfo targetB)
        {
            if (instruction == null || instruction.operand as FieldInfo != targetB)
            {
                return false;
            }

            // ldfld (copy) or ldflda (address for get_Thing on LocalTargetInfo)
            return instruction.opcode == OpCodes.Ldfld || instruction.opcode == OpCodes.Ldflda;
        }

        private static bool IsCallTo(CodeInstruction instruction, MethodInfo method)
        {
            return instruction != null
                && (instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt)
                && instruction.operand as MethodInfo == method;
        }
    }

    /// <summary>
    /// Soft-fail PlantableCells if another pawn's SowWithSeeds job has a null TargetB Thing.
    ///
    /// Мягкий отказ PlantableCells при null TargetB у чужого SowWithSeeds job.
    /// </summary>
    internal static class WorkGiver_PlantableCells_Finalizer
    {
        public static Exception Finalizer(Exception __exception, ref int __result)
        {
            if (!KebabTweaksSettings.EnableSeedsPleaseSowFix)
            {
                return __exception;
            }

            if (__exception is NullReferenceException)
            {
                __result = 0;
                return null;
            }

            return __exception;
        }
    }

    /// <summary>
    /// Clamps curToilIndex when SowWithSeeds toil count shrinks on load/resume.
    ///
    /// Ограничивает curToilIndex, когда при load/resume число toils SowWithSeeds уменьшилось.
    /// </summary>
    internal static class JobDriver_SetupToils_SowWithSeedsClamp_Patch
    {
        private static readonly Type DriverType =
            AccessTools.TypeByName("SeedsPlease.JobDriver_PlantSowWithSeeds");

        private static readonly FieldInfo ToilsField =
            AccessTools.Field(typeof(JobDriver), "toils");

        private static readonly FieldInfo CurToilIndexField =
            AccessTools.Field(typeof(JobDriver), "curToilIndex");

        public static void Postfix(JobDriver __instance)
        {
            if (!KebabTweaksSettings.EnableSeedsPleaseSowFix)
            {
                return;
            }

            if (DriverType == null || !DriverType.IsInstanceOfType(__instance))
            {
                return;
            }

            if (ToilsField == null || CurToilIndexField == null)
            {
                return;
            }

            var toils = ToilsField.GetValue(__instance) as List<Toil>;
            if (toils == null || toils.Count == 0)
            {
                return;
            }

            int index = (int)CurToilIndexField.GetValue(__instance);
            if (index < toils.Count)
            {
                return;
            }

            int clamped = toils.Count - 1;
            CurToilIndexField.SetValue(__instance, clamped);
            Log.Message(
                $"[SeedsPleaseSowFixPatch] {__instance.pawn?.LabelShort}: clamped SowWithSeeds curToilIndex {index} → {clamped} (toils={toils.Count}).");
        }
    }
}
