using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Cybranian - Diseases+ HediffComp_DiseaseAfterTicks.IsReady passes the per-tick
    /// severityAdjustment argument to FloatRange.Includes. For flu, plague, malaria, sleeping
    /// sickness, and wound infection that argument is 0, so a range such as 0.5-1.0 never matches.
    /// Each miss calls UpdateData, the complication timer never elapses, and pneumonia, sepsis,
    /// and pulmonary edema do not appear. The 1.7 build of the same mod already passes the
    /// hediff's current severity.
    ///
    /// Fix: A transpiler on IsReady replaces that argument with Hediff.Severity. The timer reset
    /// while severity is outside the range stays. The row is veiled when Diseases+ is not loaded.
    /// The probe is leftover when IsReady already passes get_Severity, which is the 1.7 mod and a
    /// later 1.6 mod that does the same. An unrecognized method body stays current and is not patched.
    ///
    /// Проблема: Cybranian - Diseases+ в HediffComp_DiseaseAfterTicks.IsReady передаёт в
    /// FloatRange.Includes аргумент severityAdjustment за один тик. У гриппа, чумы, малярии,
    /// сонной болезни и раневой инфекции это 0, поэтому диапазон вроде 0.5-1.0 не совпадает.
    /// Каждый промах зовёт UpdateData, таймер осложнения не копится, и пневмония, сепсис и
    /// отёк лёгких не появляются. Сборка 1.7 того же мода уже передаёт текущую тяжесть hediff.
    ///
    /// Исправление: транспайлер IsReady подменяет этот аргумент на Hediff.Severity. Сброс таймера,
    /// пока тяжесть вне диапазона, остаётся. Строка под вуалью, если Diseases+ не загружен.
    /// Зонд даёт leftover, когда IsReady уже передаёт get_Severity: это мод 1.7 и поздний мод 1.6
    /// с тем же сравнением. Неузнанное тело метода остаётся актуальным и не патчится.
    /// </summary>
    public static class DiseaseComplicationFixFeatures
    {
        const string CompTypeName = "Diseases.Hediffs.HediffComp_DiseaseAfterTicks";

        /// <summary>
        /// Veils the row when Diseases+ is not loaded. Called even if the checkbox is off.
        ///
        /// Вешает вуаль, если Diseases+ не загружен. Вызывается и при выключенном чекбоксе.
        /// </summary>
        public static void NoteIfMissing()
        {
            if (AccessTools.TypeByName(CompTypeName) != null)
            {
                return;
            }

            FeatureAvailability.SkipMissingMods(
                FeatureAvailability.DiseaseComplication,
                "Disease Complication Fix",
                "Cybranian - Diseases+ (DimonSever000.DiseasesPlus.Specific)");
        }

        /// <summary>
        /// Patches IsReady only while its Includes call still reads the per-tick argument.
        ///
        /// Патчит IsReady только пока его вызов Includes читает аргумент за тик.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            if (harmony == null ||
                FeatureAvailability.IsBlocked(FeatureAvailability.DiseaseComplication))
            {
                return;
            }

            Type compType = AccessTools.TypeByName(CompTypeName);
            MethodInfo isReady = AccessTools.DeclaredMethod(compType, "IsReady");
            if (compType == null || isReady == null)
            {
                NoteIfMissing();
                return;
            }

            if (ReadOriginalCompare() != DiseaseSeverityCompare.Parameter)
            {
                FeatureAvailability.WarnOnce(
                    FeatureAvailability.DiseaseComplication + ":il",
                    "[HSK kebab tweaks] Disease Complication Fix skipped: IsReady no longer passes the per-tick severity argument.");
                return;
            }

            harmony.Patch(
                isReady,
                transpiler: new HarmonyMethod(
                    typeof(DiseaseComplicationFixFeatures),
                    nameof(ReplaceParameterWithSeverity)));
            Log.Message("[HSK kebab tweaks] Disease Complication Fix applied.");
        }

        /// <summary>
        /// How the original IsReady feeds FloatRange.Includes. Absent means the comp type is not loaded.
        ///
        /// Как исходный IsReady передаёт аргумент в FloatRange.Includes. Absent - тип компонента не загружен.
        /// </summary>
        public static DiseaseSeverityCompare ReadOriginalCompare()
        {
            Type compType = AccessTools.TypeByName(CompTypeName);
            if (compType == null)
            {
                return DiseaseSeverityCompare.Absent;
            }

            MethodInfo isReady = AccessTools.DeclaredMethod(compType, "IsReady");
            byte[] il = isReady?.GetMethodBody()?.GetILAsByteArray();
            if (il == null)
            {
                return DiseaseSeverityCompare.Unrecognized;
            }

            Module module = isReady.Module;
            bool sawParameter = false;
            bool sawSeverity = false;
            bool sawOther = false;
            int previous = -1;
            int beforePrevious = -1;
            int index = 0;
            while (index < il.Length)
            {
                int start = index;
                int next = NextInstruction(il, index);
                if (next <= start)
                {
                    break;
                }

                if (IsIncludesCall(il, start, module))
                {
                    if (beforePrevious >= 0 && il[beforePrevious] == 0x03 &&
                        previous >= 0 && il[previous] == 0x4E)
                    {
                        sawParameter = true;
                    }
                    else if (previous >= 0 && IsSeverityGetter(il, previous, module))
                    {
                        sawSeverity = true;
                    }
                    else
                    {
                        sawOther = true;
                    }
                }

                beforePrevious = previous;
                previous = start;
                index = next;
            }

            if (sawParameter)
            {
                return DiseaseSeverityCompare.Parameter;
            }

            if (sawSeverity && !sawOther)
            {
                return DiseaseSeverityCompare.CurrentSeverity;
            }

            return DiseaseSeverityCompare.Unrecognized;
        }

        /// <summary>
        /// Replaces Includes(severityAdjustment) with Includes(parent.Severity) and leaves the rest of IsReady.
        ///
        /// Заменяет Includes(severityAdjustment) на Includes(parent.Severity) и остальной IsReady не трогает.
        /// </summary>
        public static IEnumerable<CodeInstruction> ReplaceParameterWithSeverity(
            IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo parent = AccessTools.Field(typeof(HediffComp), nameof(HediffComp.parent));
            MethodInfo severity = AccessTools.PropertyGetter(typeof(Hediff), nameof(Hediff.Severity));
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            for (int i = 2; i < list.Count; i++)
            {
                if (!IsIncludesInstruction(list[i]))
                {
                    continue;
                }

                if (list[i - 2].opcode != OpCodes.Ldarg_1 || list[i - 1].opcode != OpCodes.Ldind_R4)
                {
                    continue;
                }

                CodeInstruction loadThis = new CodeInstruction(OpCodes.Ldarg_0);
                loadThis.labels.AddRange(list[i - 2].labels);
                loadThis.blocks.AddRange(list[i - 2].blocks);
                CodeInstruction loadParent = new CodeInstruction(OpCodes.Ldfld, parent);
                loadParent.labels.AddRange(list[i - 1].labels);
                loadParent.blocks.AddRange(list[i - 1].blocks);
                list[i - 2] = loadThis;
                list[i - 1] = loadParent;
                list.Insert(i, new CodeInstruction(OpCodes.Callvirt, severity));
                i++;
            }

            return list;
        }

        static bool IsIncludesInstruction(CodeInstruction instruction)
        {
            MethodBase method = instruction?.operand as MethodBase;
            return method != null && method.Name == "Includes" &&
                method.DeclaringType == typeof(FloatRange) &&
                (instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt);
        }

        static bool IsIncludesCall(byte[] il, int start, Module module)
        {
            if (start >= il.Length)
            {
                return false;
            }

            byte op = il[start];
            if (op != 0x28 && op != 0x6F)
            {
                return false;
            }

            MethodBase method = ResolveCall(il, start, module);
            return method != null && method.Name == "Includes" &&
                method.DeclaringType == typeof(FloatRange);
        }

        static bool IsSeverityGetter(byte[] il, int start, Module module)
        {
            MethodBase method = ResolveCall(il, start, module);
            return method != null && method.Name == "get_Severity";
        }

        static MethodBase ResolveCall(byte[] il, int start, Module module)
        {
            if (module == null || start < 0 || start + 5 > il.Length)
            {
                return null;
            }

            byte op = il[start];
            if (op != 0x28 && op != 0x6F)
            {
                return null;
            }

            int token = BitConverter.ToInt32(il, start + 1);
            try
            {
                return module.ResolveMethod(token);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (BadImageFormatException)
            {
                return null;
            }
        }

        static int NextInstruction(byte[] il, int index)
        {
            if (index >= il.Length)
            {
                return index;
            }

            byte op = il[index++];
            if (op == 0xFE)
            {
                if (index >= il.Length)
                {
                    return il.Length;
                }

                index += PrefixOperandSize(il[index++]);
                return index;
            }

            if (op == 0x45)
            {
                if (index + 4 > il.Length)
                {
                    return il.Length;
                }

                int count = BitConverter.ToInt32(il, index);
                return index + 4 + (count * 4);
            }

            return index + OperandSize(op);
        }

        static int PrefixOperandSize(byte second)
        {
            switch (second)
            {
                case 0x06:
                case 0x09:
                case 0x0A:
                case 0x0B:
                case 0x0C:
                case 0x0D:
                case 0x0E:
                case 0x15:
                case 0x16:
                case 0x1C:
                    return 4;
                case 0x12:
                    return 1;
                default:
                    return 0;
            }
        }

        static int OperandSize(byte op)
        {
            switch (op)
            {
                case 0x0E:
                case 0x0F:
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x1F:
                case 0x2B:
                case 0x2C:
                case 0x2D:
                case 0x2E:
                case 0x2F:
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37:
                case 0xDE:
                    return 1;
                case 0x20:
                case 0x21:
                    return 8;
                case 0x22:
                case 0x28:
                case 0x29:
                case 0x38:
                case 0x39:
                case 0x3A:
                case 0x3B:
                case 0x3C:
                case 0x3D:
                case 0x3E:
                case 0x3F:
                case 0x40:
                case 0x41:
                case 0x42:
                case 0x43:
                case 0x44:
                case 0x6F:
                case 0x70:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x79:
                case 0x7B:
                case 0x7C:
                case 0x7D:
                case 0x7E:
                case 0x7F:
                case 0x80:
                case 0x81:
                case 0x8C:
                case 0x8D:
                case 0x8F:
                case 0xA3:
                case 0xA4:
                case 0xA5:
                case 0xC2:
                case 0xC6:
                case 0xD0:
                    return 4;
                default:
                    return 0;
            }
        }
    }

    /// <summary>
    /// What HediffComp_DiseaseAfterTicks.IsReady passes into FloatRange.Includes.
    ///
    /// Что HediffComp_DiseaseAfterTicks.IsReady передаёт в FloatRange.Includes.
    /// </summary>
    public enum DiseaseSeverityCompare
    {
        Absent,
        Parameter,
        CurrentSeverity,
        Unrecognized
    }
}
