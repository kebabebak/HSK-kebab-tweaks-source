using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: On RimWorld 1.7 the map resource list throws every frame. Toggleable Readouts
    /// replaces ResourceReadout.DoReadoutCategorized and DoReadoutSimple, then writes
    /// GUIContent.m_Text and calls other Unity IMGUI members the 1.7 stub does not have
    /// (GUIStyle.Internal_Draw_Injected, GUI.roundedRectMaterial, GUI.s_ButonHash, GUIUtility.HitTest,
    /// GUI.GrabMouseControl, GUI.HasMouseControl, GUI.ReleaseMouseControl). Root.OnGUI catches the
    /// exception, so the rest of that pass does not finish. Unity 1.5 and 1.6 still have those members.
    ///
    /// Fix: When GUIContent has no instance field m_Text, copy the methods whose IL names a missing
    /// Unity member and retarget those calls onto GUIContent.set_text, GUIStyle.Draw, a stable
    /// control-id hint, and ReadoutMouse. FastGUI.DrawTextureFast and ButtonImageFast become
    /// GUI.DrawTexture and the same mouse helpers, so FastGUI's static constructor never runs: it
    /// calls GUI.roundedRectMaterial, which this stub does not have. Harmony cannot transpile a method
    /// that still contains a missing token: MethodBodyReader resolves every token and throws first.
    /// On the 1.7 runtime that failure is an ArgumentException naming only the token, so the member
    /// name and declaring type are read from the assembly MemberRef table. A detour compiles the
    /// original method before it can replace it, so the copy is not installed onto those methods.
    /// ResourceReadout reaches the list through DoReadoutSimple and DoReadoutCategorized. Those two,
    /// and DoCategoryChildren, resolve, and a Harmony transpiler points their calls at the copies.
    /// The copy is a static DynamicMethod (instance methods take this as the first argument).
    /// Update's finally still disposes its enumerator. Category rows test Event.mousePosition against
    /// the row rect only while HitTest is absent. If the Unity members are present, nothing is
    /// installed, so enabling the leftover row on Unity leaves the original mouse capture alone.
    ///
    /// Проблема: На RimWorld 1.7 список ресурсов на карте падает каждый кадр. Toggleable Readouts
    /// подменяет ResourceReadout.DoReadoutCategorized и DoReadoutSimple, затем пишет
    /// GUIContent.m_Text и вызывает другие члены Unity IMGUI, которых нет в заглушке 1.7
    /// (GUIStyle.Internal_Draw_Injected, GUI.roundedRectMaterial, GUI.s_ButonHash, GUIUtility.HitTest,
    /// GUI.GrabMouseControl, GUI.HasMouseControl, GUI.ReleaseMouseControl). Root.OnGUI ловит
    /// исключение, и остаток этого прохода не выполняется. В Unity 1.5 и 1.6 эти члены есть.
    ///
    /// Исправление: Если у GUIContent нет поля экземпляра m_Text, копируются методы, в IL которых
    /// назван отсутствующий член Unity, и эти вызовы подменяются на GUIContent.set_text, GUIStyle.Draw,
    /// стабильный hint идентификатора контрола и ReadoutMouse. FastGUI.DrawTextureFast и
    /// ButtonImageFast становятся GUI.DrawTexture и теми же помощниками мыши, поэтому статический
    /// конструктор FastGUI не выполняется: он вызывает GUI.roundedRectMaterial, которого в этой
    /// заглушке нет. Harmony не может транспилировать метод, в котором остался отсутствующий токен:
    /// MethodBodyReader резолвит каждый токен и падает раньше. На рантайме 1.7 это ArgumentException
    /// только с токеном, поэтому имя члена и объявляющий тип читаются из таблицы MemberRef сборки.
    /// Детур компилирует исходный метод до подмены, поэтому копия не ставится на эти методы.
    /// ResourceReadout доходит до списка через DoReadoutSimple и DoReadoutCategorized. Эти два метода
    /// и DoCategoryChildren резолвятся, и транспайлер Harmony направляет их вызовы в копии.
    /// Копия - статический DynamicMethod (у методов экземпляра this идёт первым аргументом).
    /// finally в Update по-прежнему освобождает перечислитель. Строки категорий сверяют
    /// Event.mousePosition с прямоугольником строки только пока нет HitTest. Если члены Unity на месте,
    /// копия не ставится, поэтому включение leftover-строки на Unity не трогает исходный захват мыши.
    /// </summary>
    public static class ToggleableReadoutsGuiFixFeatures
    {
        const int ButtonControlHint = 1849430;

        static readonly List<DynamicMethod> keptCopies = new List<DynamicMethod>();

        static readonly Dictionary<MethodBase, DynamicMethod> redirects = new Dictionary<MethodBase, DynamicMethod>();

        /// <summary>
        /// Veils the row when Toggleable Readouts is not loaded. Called even if the checkbox is off,
        /// so a disabled leftover row still shows the missing-mod veil.
        ///
        /// Скрывает строку вуалью, если Toggleable Readouts не загружен. Вызывается и при выключенном
        /// чекбоксе, чтобы выключенная leftover-строка всё равно показывала вуаль отсутствующего мода.
        /// </summary>
        public static void NoteIfMissing()
        {
            if (AccessTools.TypeByName("ToggleableReadouts.ToggleableReadoutsUtility") != null)
            {
                return;
            }

            FeatureAvailability.SkipMissingMods(
                FeatureAvailability.ToggleableReadouts,
                "Resource readout fix",
                "Toggleable Readouts (Owlchemist.ToggleableReadouts)");
        }

        /// <summary>
        /// Copies the methods that name a missing Unity member, then points the resolvable callers
        /// at those copies. The broken methods themselves are not detoured: the detour compiles them
        /// first and that compile throws.
        ///
        /// Копирует методы, которые называют отсутствующий член Unity, и направляет резолвящихся
        /// вызывающих в эти копии. Сами сломанные методы не детурятся: детур сначала их компилирует,
        /// и эта компиляция падает.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            if (harmony == null
                || FeatureAvailability.IsBlocked(FeatureAvailability.ToggleableReadouts))
            {
                return;
            }

            Type utility = AccessTools.TypeByName("ToggleableReadouts.ToggleableReadoutsUtility");
            Type cache = AccessTools.TypeByName("ToggleableReadouts.ReadoutCache");
            if (utility == null || cache == null)
            {
                NoteIfMissing();
                return;
            }

            MethodBase[] broken =
            {
                AccessTools.Method(utility, "DrawResourceSimple"),
                AccessTools.Method(utility, "DoCategory"),
                AccessTools.Method(utility, "DoThingDef"),
                AccessTools.Method(cache, "Update")
            };
            MethodBase[] callers =
            {
                AccessTools.Method(utility, "DoReadoutSimple"),
                AccessTools.Method(utility, "DoReadoutCategorized"),
                AccessTools.Method(utility, "DoCategoryChildren")
            };
            foreach (MethodBase method in broken)
            {
                if (method == null)
                {
                    Log.Warning("[HSK kebab tweaks] Resource readout fix skipped a missing method.");
                    return;
                }
            }

            foreach (MethodBase method in callers)
            {
                if (method == null)
                {
                    Log.Warning("[HSK kebab tweaks] Resource readout fix skipped a missing method.");
                    return;
                }
            }

            try
            {
                redirects.Clear();
                Dictionary<MethodBase, List<IlCopy.Op>> parsed = new Dictionary<MethodBase, List<IlCopy.Op>>();
                bool anyMissing = false;
                foreach (MethodBase method in broken)
                {
                    List<IlCopy.Op> ops = IlCopy.Parse(method);
                    parsed.Add(method, ops);
                    if (ContainsMissing(ops))
                    {
                        anyMissing = true;
                    }
                }

                if (!anyMissing)
                {
                    return;
                }

                foreach (MethodBase method in broken)
                {
                    redirects.Add(method, IlCopy.Create(method));
                }

                foreach (MethodBase method in broken)
                {
                    Rewrite(parsed[method]);
                    RequireResolved(method, parsed[method]);
                    IlCopy.Write(redirects[method], method, parsed[method]);
                    keptCopies.Add(redirects[method]);
                }

                HarmonyMethod transpiler = new HarmonyMethod(
                    typeof(ToggleableReadoutsGuiFixFeatures),
                    nameof(RedirectCopiedCalls));
                int installed = 0;
                foreach (MethodBase method in callers)
                {
                    harmony.Patch(method, transpiler: transpiler);
                    installed++;
                }

                Log.Message("[HSK kebab tweaks] Resource readout fix installed " + installed + " readout entries.");
            }
            catch (Exception ex)
            {
                Log.Error("[HSK kebab tweaks] Resource readout fix left the resource list unchanged: " + OpenMessage(ex));
            }
        }

        /// <summary>
        /// Swaps a call to a copied readout method for the copy. Used on methods whose own tokens resolve.
        ///
        /// Меняет вызов скопированного метода списка на копию. Стоит на методах, чьи собственные токены резолвятся.
        /// </summary>
        static IEnumerable<CodeInstruction> RedirectCopiedCalls(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt)
                    && instruction.operand is MethodBase method
                    && redirects.TryGetValue(method, out DynamicMethod copy))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = copy;
                }

                yield return instruction;
            }
        }

        static string OpenMessage(Exception ex)
        {
            while (ex is TargetInvocationException && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            return ex.Message;
        }

        /// <summary>
        /// Retargets unresolved Unity tokens. Returns how many instructions changed, or -1 when a
        /// draw call has no address load to turn into a value.
        ///
        /// Подменяет нерезолвящиеся токены Unity. Возвращает число изменённых инструкций или -1,
        /// если у вызова отрисовки нет загрузки адреса, которую нужно сделать значением.
        /// </summary>
        static int Rewrite(List<IlCopy.Op> ops)
        {
            MethodInfo setText = typeof(GUIContent).GetProperty("text")?.GetSetMethod();
            MethodInfo draw = typeof(GUIStyle).GetMethod(
                "Draw",
                new[]
                {
                    typeof(Rect), typeof(GUIContent), typeof(bool), typeof(bool), typeof(bool), typeof(bool)
                });
            MethodInfo drawTexture = AccessTools.Method(typeof(ReadoutDraw), nameof(ReadoutDraw.DrawTextureFast));
            MethodInfo buttonImage = AccessTools.Method(typeof(ReadoutDraw), nameof(ReadoutDraw.ButtonImageFast));
            int changes = 0;
            for (int i = 0; i < ops.Count; i++)
            {
                IlCopy.Op op = ops[i];
                if ((op.Opcode == OpCodes.Call || op.Opcode == OpCodes.Callvirt)
                    && op.Operand is MethodBase called)
                {
                    if (redirects.TryGetValue(called, out DynamicMethod copy))
                    {
                        op.Opcode = OpCodes.Call;
                        op.Operand = copy;
                        changes++;
                        continue;
                    }

                    if (called.DeclaringType != null && called.DeclaringType.Name == "FastGUI")
                    {
                        if (called.Name == "DrawTextureFast")
                        {
                            op.Opcode = OpCodes.Call;
                            op.Operand = drawTexture;
                            changes++;
                            continue;
                        }

                        if (called.Name == "ButtonImageFast")
                        {
                            op.Opcode = OpCodes.Call;
                            op.Operand = buttonImage;
                            changes++;
                            continue;
                        }

                        throw new MissingMethodException(called.DeclaringType.FullName, called.Name);
                    }
                }

                if (IsMissing(op, "m_Text", true, "GUIContent"))
                {
                    if (setText == null)
                    {
                        throw new MissingMethodException(typeof(GUIContent).FullName, "set_text");
                    }

                    op.Opcode = OpCodes.Callvirt;
                    op.Operand = setText;
                    changes++;
                    continue;
                }

                if (IsMissing(op, "Internal_Draw_Injected", false, "GUIStyle"))
                {
                    if (draw == null)
                    {
                        throw new MissingMethodException(typeof(GUIStyle).FullName, "Draw");
                    }

                    int address = FindDrawAddress(ops, i);
                    if (address < 0)
                    {
                        throw new InvalidOperationException("Internal_Draw_Injected has no byref argument to retarget.");
                    }

                    IlCopy.Op load = ops[address];
                    if (load.Opcode == OpCodes.Ldarga || load.Opcode == OpCodes.Ldarga_S)
                    {
                        load.Opcode = OpCodes.Ldarg_S;
                    }
                    else if (load.Opcode == OpCodes.Ldflda)
                    {
                        load.Opcode = OpCodes.Ldfld;
                    }
                    else if (load.Opcode != OpCodes.Ldarg && load.Opcode != OpCodes.Ldarg_S
                        && load.Opcode != OpCodes.Ldfld)
                    {
                        throw new InvalidOperationException("Internal_Draw_Injected address is " + load.Opcode + ".");
                    }

                    op.Opcode = OpCodes.Callvirt;
                    op.Operand = draw;
                    changes++;
                    continue;
                }

                if (IsMissing(op, "get_roundedRectMaterial", false, "GUI"))
                {
                    op.Opcode = OpCodes.Ldnull;
                    op.Operand = null;
                    changes++;
                    continue;
                }

                if (IsMissing(op, "s_ButonHash", true, "GUI"))
                {
                    op.Opcode = OpCodes.Ldc_I4;
                    op.Operand = ButtonControlHint;
                    changes++;
                    continue;
                }

                if (IsMissing(op, "HitTest", false, "GUIUtility"))
                {
                    op.Opcode = OpCodes.Call;
                    op.Operand = AccessTools.Method(typeof(ReadoutMouse), nameof(ReadoutMouse.HitTest));
                    changes++;
                    continue;
                }

                if (IsMissing(op, "GrabMouseControl", false, "GUI"))
                {
                    op.Opcode = OpCodes.Call;
                    op.Operand = AccessTools.Method(typeof(ReadoutMouse), nameof(ReadoutMouse.GrabMouseControl));
                    changes++;
                    continue;
                }

                if (IsMissing(op, "HasMouseControl", false, "GUI"))
                {
                    op.Opcode = OpCodes.Call;
                    op.Operand = AccessTools.Method(typeof(ReadoutMouse), nameof(ReadoutMouse.HasMouseControl));
                    changes++;
                    continue;
                }

                if (IsMissing(op, "ReleaseMouseControl", false, "GUI"))
                {
                    op.Opcode = OpCodes.Call;
                    op.Operand = AccessTools.Method(typeof(ReadoutMouse), nameof(ReadoutMouse.ReleaseMouseControl));
                    changes++;
                }
            }

            return changes;
        }

        static bool ContainsMissing(List<IlCopy.Op> ops)
        {
            for (int i = 0; i < ops.Count; i++)
            {
                if (ops[i].Operand is IlCopy.MissingMember)
                {
                    return true;
                }
            }

            return false;
        }

        static void RequireResolved(MethodBase method, List<IlCopy.Op> ops)
        {
            for (int i = 0; i < ops.Count; i++)
            {
                if (ops[i].Operand is IlCopy.MissingMember missing)
                {
                    throw new MissingMemberException(method.DeclaringType?.Name + "." + method.Name + " still uses " + missing.ClassName + "." + missing.Name);
                }
            }
        }

        static int FindDrawAddress(List<IlCopy.Op> ops, int callIndex)
        {
            int index = callIndex - 1;
            for (int n = 0; n < 4; n++)
            {
                if (index < 0 || !IsIntZero(ops[index]))
                {
                    return -1;
                }

                index--;
            }

            if (index < 0 || ops[index].Opcode != OpCodes.Ldsfld)
            {
                return -1;
            }

            return index - 1;
        }

        static bool IsIntZero(IlCopy.Op op)
        {
            if (op.Opcode == OpCodes.Ldc_I4_0)
            {
                return true;
            }

            if (op.Opcode == OpCodes.Ldc_I4_S && op.Operand is sbyte small && small == 0)
            {
                return true;
            }

            return op.Opcode == OpCodes.Ldc_I4 && op.Operand is int value && value == 0;
        }

        static bool IsMissing(IlCopy.Op op, string member, bool field, string typeName)
        {
            IlCopy.MissingMember missing = op.Operand as IlCopy.MissingMember;
            if (missing == null || missing.IsField != field || missing.Name != member)
            {
                return false;
            }

            return SimpleName(missing.ClassName) == typeName;
        }

        static string SimpleName(string className)
        {
            if (className == null)
            {
                return "";
            }

            int dot = className.LastIndexOf('.');
            string simple = dot >= 0 ? className.Substring(dot + 1) : className;
            int tick = simple.IndexOf('`');
            if (tick >= 0)
            {
                simple = simple.Substring(0, tick);
            }

            return simple;
        }

        /// <summary>
        /// Draws a readout icon without touching FastGUI. FastGUI's static constructor calls
        /// GUI.roundedRectMaterial, which the 1.7 stub does not have, and any use of that type runs it.
        ///
        /// Рисует иконку списка, не трогая FastGUI. Статический конструктор FastGUI вызывает
        /// GUI.roundedRectMaterial, которого нет в заглушке 1.7, и любое обращение к этому типу его запускает.
        /// </summary>
        public static class ReadoutDraw
        {
            public static void DrawTextureFast(Rect position, Texture image, Color color)
            {
                if (image == null)
                {
                    return;
                }

                Color previous = GUI.color;
                try
                {
                    GUI.color = color;
                    GUI.DrawTexture(position, image);
                }
                finally
                {
                    GUI.color = previous;
                }
            }

            public static bool ButtonImageFast(Rect butRect, int controlID, Event current, EventType eventType, Texture2D tex)
            {
                DrawTextureFast(butRect, tex, Color.white);
                if (current == null)
                {
                    return false;
                }

                if (eventType == EventType.MouseDown && ReadoutMouse.HitTest(butRect, current))
                {
                    ReadoutMouse.GrabMouseControl(controlID);
                    current.Use();
                }

                if (eventType == EventType.MouseUp && ReadoutMouse.HasMouseControl(controlID))
                {
                    ReadoutMouse.ReleaseMouseControl();
                    current.Use();
                    if (ReadoutMouse.HitTest(butRect, current))
                    {
                        GUI.changed = true;
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Same signatures as GUIUtility.HitTest, GUI.GrabMouseControl, GUI.HasMouseControl, and
        /// GUI.ReleaseMouseControl. One held id, and the row rect tested against Event.mousePosition.
        ///
        /// Те же сигнатуры, что у GUIUtility.HitTest, GUI.GrabMouseControl, GUI.HasMouseControl и
        /// GUI.ReleaseMouseControl. Один удерживаемый id, прямоугольник строки сверяется с
        /// Event.mousePosition.
        /// </summary>
        public static class ReadoutMouse
        {
            static int heldControlId;

            public static bool HitTest(Rect rect, Event current)
            {
                return current != null && rect.Contains(current.mousePosition);
            }

            public static void GrabMouseControl(int id)
            {
                heldControlId = id;
            }

            public static bool HasMouseControl(int id)
            {
                return heldControlId == id;
            }

            public static void ReleaseMouseControl()
            {
                heldControlId = 0;
            }
        }

        /// <summary>
        /// Copies one method body, including a single non-nested finally or catch, into a DynamicMethod.
        /// Unresolved tokens stay as MissingMember until Rewrite replaces them. Branch offsets become labels.
        ///
        /// Копирует тело одного метода, включая один невложенный finally или catch, в DynamicMethod.
        /// Нерезолвящиеся токены остаются MissingMember, пока Rewrite их не заменит. Смещения переходов
        /// становятся метками.
        /// </summary>
        internal static class IlCopy
        {
            static readonly OpCode[] oneByte = new OpCode[256];
            static readonly OpCode[] twoByte = new OpCode[256];

            static IlCopy()
            {
                foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (field.FieldType != typeof(OpCode))
                    {
                        continue;
                    }

                    OpCode op = (OpCode)field.GetValue(null);
                    ushort value = (ushort)op.Value;
                    if (value <= 0xFF)
                    {
                        oneByte[value] = op;
                    }
                    else if ((value & 0xFF00) == 0xFE00)
                    {
                        twoByte[value & 0xFF] = op;
                    }
                }
            }

            internal sealed class MissingMember
            {
                public string ClassName;
                public string Name;
                public bool IsField;
            }

            internal sealed class Op
            {
                public int Offset;
                public OpCode Opcode;
                public object Operand;
                public int BranchTarget = -1;
                public int[] SwitchTargets;
            }

            sealed class Clause
            {
                public int TryStart;
                public int HandlerStart;
                public int HandlerEnd;
                public ExceptionHandlingClauseOptions Kind;
                public Type CatchType;
            }

            public static List<Op> Parse(MethodBase method)
            {
                MethodBody body = method.GetMethodBody();
                if (body == null)
                {
                    throw new InvalidOperationException(method.Name + " has no body.");
                }

                byte[] bytes = body.GetILAsByteArray();
                Type[] typeArgs = method.DeclaringType != null && method.DeclaringType.IsGenericType
                    ? method.DeclaringType.GetGenericArguments()
                    : null;
                MethodInfo generic = method as MethodInfo;
                Type[] methodArgs = generic != null && generic.IsGenericMethod
                    ? generic.GetGenericArguments()
                    : null;
                Module module = method.Module;
                List<Op> ops = new List<Op>();
                int pos = 0;
                while (pos < bytes.Length)
                {
                    int offset = pos;
                    byte raw = bytes[pos++];
                    OpCode opcode = raw == 0xFE ? twoByte[bytes[pos++]] : oneByte[raw];
                    if (string.IsNullOrEmpty(opcode.Name))
                    {
                        throw new InvalidOperationException("Unknown opcode at " + offset + " in " + method.Name + ".");
                    }

                    Op op = new Op { Offset = offset, Opcode = opcode };
                    switch (opcode.OperandType)
                    {
                        case OperandType.InlineNone:
                            break;
                        case OperandType.InlineI:
                            op.Operand = BitConverter.ToInt32(bytes, pos);
                            pos += 4;
                            break;
                        case OperandType.InlineI8:
                            op.Operand = BitConverter.ToInt64(bytes, pos);
                            pos += 8;
                            break;
                        case OperandType.ShortInlineI:
                            op.Operand = (sbyte)bytes[pos++];
                            break;
                        case OperandType.InlineR:
                            op.Operand = BitConverter.ToDouble(bytes, pos);
                            pos += 8;
                            break;
                        case OperandType.ShortInlineR:
                            op.Operand = BitConverter.ToSingle(bytes, pos);
                            pos += 4;
                            break;
                        case OperandType.InlineString:
                            op.Operand = module.ResolveString(BitConverter.ToInt32(bytes, pos));
                            pos += 4;
                            break;
                        case OperandType.InlineMethod:
                            op.Operand = ResolveMethod(module, BitConverter.ToInt32(bytes, pos), typeArgs, methodArgs);
                            pos += 4;
                            break;
                        case OperandType.InlineField:
                            op.Operand = ResolveField(module, BitConverter.ToInt32(bytes, pos), typeArgs, methodArgs);
                            pos += 4;
                            break;
                        case OperandType.InlineType:
                            op.Operand = module.ResolveType(BitConverter.ToInt32(bytes, pos), typeArgs, methodArgs);
                            pos += 4;
                            break;
                        case OperandType.InlineTok:
                            op.Operand = module.ResolveMember(BitConverter.ToInt32(bytes, pos), typeArgs, methodArgs);
                            pos += 4;
                            break;
                        case OperandType.InlineBrTarget:
                            op.BranchTarget = pos + 4 + BitConverter.ToInt32(bytes, pos);
                            pos += 4;
                            break;
                        case OperandType.ShortInlineBrTarget:
                            op.BranchTarget = pos + 1 + (sbyte)bytes[pos];
                            pos++;
                            break;
                        case OperandType.InlineSwitch:
                            int count = BitConverter.ToInt32(bytes, pos);
                            pos += 4;
                            int next = pos + (count * 4);
                            op.SwitchTargets = new int[count];
                            for (int i = 0; i < count; i++)
                            {
                                op.SwitchTargets[i] = next + BitConverter.ToInt32(bytes, pos);
                                pos += 4;
                            }

                            break;
                        case OperandType.InlineVar:
                            op.Operand = (int)BitConverter.ToUInt16(bytes, pos);
                            pos += 2;
                            break;
                        case OperandType.ShortInlineVar:
                            op.Operand = bytes[pos++];
                            break;
                        default:
                            throw new InvalidOperationException(opcode + " in " + method.Name + " is not copied.");
                    }

                    ops.Add(op);
                }

                return ops;
            }

            public static DynamicMethod Create(MethodBase method)
            {
                Type returnType = method is MethodInfo info ? info.ReturnType : typeof(void);
                ParameterInfo[] sourceParams = method.GetParameters();
                Type[] parameters;
                if (method.IsStatic)
                {
                    parameters = new Type[sourceParams.Length];
                    for (int i = 0; i < sourceParams.Length; i++)
                    {
                        parameters[i] = sourceParams[i].ParameterType;
                    }
                }
                else
                {
                    parameters = new Type[sourceParams.Length + 1];
                    parameters[0] = method.DeclaringType;
                    for (int i = 0; i < sourceParams.Length; i++)
                    {
                        parameters[i + 1] = sourceParams[i].ParameterType;
                    }
                }

                string copyName = method.Name;
                if (copyName.Length == 0 || copyName[0] == '.')
                {
                    copyName = "cctor";
                }

                return new DynamicMethod(
                    copyName + "_ReadoutGui",
                    returnType,
                    parameters,
                    method.DeclaringType,
                    true);
            }

            public static void Write(DynamicMethod copy, MethodBase method, List<Op> ops)
            {
                MethodBody body = method.GetMethodBody();
                byte[] bytes = body.GetILAsByteArray();
                List<Clause> clauses = ReadClauses(body);
                ILGenerator il = copy.GetILGenerator();
                LocalBuilder[] locals = new LocalBuilder[body.LocalVariables.Count];
                foreach (LocalVariableInfo local in body.LocalVariables)
                {
                    locals[local.LocalIndex] = il.DeclareLocal(local.LocalType, local.IsPinned);
                }

                Dictionary<int, Label> labels = new Dictionary<int, Label>();
                Label LabelAt(int offset)
                {
                    if (!labels.TryGetValue(offset, out Label label))
                    {
                        label = il.DefineLabel();
                        labels.Add(offset, label);
                    }

                    return label;
                }

                foreach (Op op in ops)
                {
                    if (op.BranchTarget >= 0)
                    {
                        LabelAt(op.BranchTarget);
                    }

                    if (op.SwitchTargets != null)
                    {
                        foreach (int target in op.SwitchTargets)
                        {
                            LabelAt(target);
                        }
                    }
                }

                foreach (Clause clause in clauses)
                {
                    LabelAt(clause.TryStart);
                    LabelAt(clause.HandlerStart);
                    LabelAt(clause.HandlerEnd);
                }

                int clauseIndex = 0;
                bool inTry = false;
                foreach (Op op in ops)
                {
                    if (clauseIndex < clauses.Count && inTry && op.Offset == clauses[clauseIndex].HandlerEnd)
                    {
                        il.EndExceptionBlock();
                        inTry = false;
                        clauseIndex++;
                    }

                    if (clauseIndex < clauses.Count && !inTry && op.Offset == clauses[clauseIndex].TryStart)
                    {
                        il.BeginExceptionBlock();
                        inTry = true;
                    }
                    else if (clauseIndex < clauses.Count && inTry && op.Offset == clauses[clauseIndex].HandlerStart)
                    {
                        Clause clause = clauses[clauseIndex];
                        if (clause.Kind == ExceptionHandlingClauseOptions.Finally)
                        {
                            il.BeginFinallyBlock();
                        }
                        else if (clause.Kind == ExceptionHandlingClauseOptions.Clause)
                        {
                            il.BeginCatchBlock(clause.CatchType);
                        }
                        else
                        {
                            throw new InvalidOperationException("Unsupported handler in " + method.Name + ".");
                        }
                    }

                    if (labels.TryGetValue(op.Offset, out Label here))
                    {
                        il.MarkLabel(here);
                    }

                    if (op.Opcode.Value == OpCodes.Endfinally.Value)
                    {
                        continue;
                    }

                    EmitOp(il, op, locals, labels);
                }

                if (inTry)
                {
                    if (clauseIndex < clauses.Count && clauses[clauseIndex].HandlerEnd == bytes.Length)
                    {
                        il.EndExceptionBlock();
                        inTry = false;
                    }
                }

                if (inTry)
                {
                    throw new InvalidOperationException("Handler did not end in " + method.Name + ".");
                }

                if (labels.TryGetValue(bytes.Length, out Label end))
                {
                    il.MarkLabel(end);
                }
            }

            static List<Clause> ReadClauses(MethodBody body)
            {
                List<Clause> clauses = new List<Clause>();
                foreach (ExceptionHandlingClause clause in body.ExceptionHandlingClauses)
                {
                    if (clause.Flags != ExceptionHandlingClauseOptions.Finally
                        && clause.Flags != ExceptionHandlingClauseOptions.Clause)
                    {
                        throw new InvalidOperationException("Unsupported handler " + clause.Flags + ".");
                    }

                    clauses.Add(new Clause
                    {
                        TryStart = clause.TryOffset,
                        HandlerStart = clause.HandlerOffset,
                        HandlerEnd = clause.HandlerOffset + clause.HandlerLength,
                        Kind = clause.Flags,
                        CatchType = clause.Flags == ExceptionHandlingClauseOptions.Clause ? clause.CatchType : null
                    });
                }

                clauses.Sort((a, b) => a.TryStart.CompareTo(b.TryStart));
                for (int i = 1; i < clauses.Count; i++)
                {
                    if (clauses[i].TryStart < clauses[i - 1].HandlerEnd)
                    {
                        throw new InvalidOperationException("Nested handler is not copied.");
                    }
                }

                return clauses;
            }

            static void EmitOp(ILGenerator il, Op op, LocalBuilder[] locals, Dictionary<int, Label> labels)
            {
                OpCode opcode = LongBranch(op.Opcode);
                if (op.BranchTarget >= 0)
                {
                    il.Emit(opcode, labels[op.BranchTarget]);
                    return;
                }

                if (op.SwitchTargets != null)
                {
                    Label[] targets = new Label[op.SwitchTargets.Length];
                    for (int i = 0; i < targets.Length; i++)
                    {
                        targets[i] = labels[op.SwitchTargets[i]];
                    }

                    il.Emit(OpCodes.Switch, targets);
                    return;
                }

                switch (opcode.OperandType)
                {
                    case OperandType.InlineNone:
                        il.Emit(opcode);
                        return;
                    case OperandType.InlineI:
                        il.Emit(opcode, (int)op.Operand);
                        return;
                    case OperandType.InlineI8:
                        il.Emit(opcode, (long)op.Operand);
                        return;
                    case OperandType.ShortInlineI:
                        il.Emit(opcode, (sbyte)op.Operand);
                        return;
                    case OperandType.InlineR:
                        il.Emit(opcode, (double)op.Operand);
                        return;
                    case OperandType.ShortInlineR:
                        il.Emit(opcode, (float)op.Operand);
                        return;
                    case OperandType.InlineString:
                        il.Emit(opcode, (string)op.Operand);
                        return;
                    case OperandType.InlineVar:
                    case OperandType.ShortInlineVar:
                        EmitVar(il, opcode, Convert.ToInt32(op.Operand), locals);
                        return;
                    case OperandType.InlineMethod:
                        if (op.Operand is ConstructorInfo ctor)
                        {
                            il.Emit(opcode, ctor);
                        }
                        else
                        {
                            il.Emit(opcode, (MethodInfo)op.Operand);
                        }

                        return;
                    case OperandType.InlineField:
                        il.Emit(opcode, (FieldInfo)op.Operand);
                        return;
                    case OperandType.InlineType:
                        il.Emit(opcode, (Type)op.Operand);
                        return;
                    case OperandType.InlineTok:
                        if (op.Operand is Type tokenType)
                        {
                            il.Emit(opcode, tokenType);
                        }
                        else if (op.Operand is MethodInfo tokenMethod)
                        {
                            il.Emit(opcode, tokenMethod);
                        }
                        else
                        {
                            il.Emit(opcode, (FieldInfo)op.Operand);
                        }

                        return;
                    default:
                        throw new InvalidOperationException("Cannot emit " + opcode + ".");
                }
            }

            static void EmitVar(ILGenerator il, OpCode opcode, int index, LocalBuilder[] locals)
            {
                if (opcode.Name.IndexOf("loc", StringComparison.Ordinal) >= 0)
                {
                    if (opcode.Name.IndexOf("loca", StringComparison.Ordinal) >= 0)
                    {
                        il.Emit(OpCodes.Ldloca, locals[index]);
                    }
                    else if (opcode.Name.StartsWith("stloc", StringComparison.Ordinal))
                    {
                        il.Emit(OpCodes.Stloc, locals[index]);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, locals[index]);
                    }

                    return;
                }

                if (opcode.Name.IndexOf("arga", StringComparison.Ordinal) >= 0)
                {
                    il.Emit(OpCodes.Ldarga, (short)index);
                }
                else if (opcode.Name.StartsWith("starg", StringComparison.Ordinal))
                {
                    il.Emit(OpCodes.Starg, (short)index);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg, (short)index);
                }
            }

            static OpCode LongBranch(OpCode op)
            {
                if (op == OpCodes.Br_S) return OpCodes.Br;
                if (op == OpCodes.Brfalse_S) return OpCodes.Brfalse;
                if (op == OpCodes.Brtrue_S) return OpCodes.Brtrue;
                if (op == OpCodes.Beq_S) return OpCodes.Beq;
                if (op == OpCodes.Bge_S) return OpCodes.Bge;
                if (op == OpCodes.Bgt_S) return OpCodes.Bgt;
                if (op == OpCodes.Ble_S) return OpCodes.Ble;
                if (op == OpCodes.Blt_S) return OpCodes.Blt;
                if (op == OpCodes.Bne_Un_S) return OpCodes.Bne_Un;
                if (op == OpCodes.Bge_Un_S) return OpCodes.Bge_Un;
                if (op == OpCodes.Bgt_Un_S) return OpCodes.Bgt_Un;
                if (op == OpCodes.Ble_Un_S) return OpCodes.Ble_Un;
                if (op == OpCodes.Blt_Un_S) return OpCodes.Blt_Un;
                if (op == OpCodes.Leave_S) return OpCodes.Leave;
                return op;
            }

            static object ResolveField(Module module, int token, Type[] typeArgs, Type[] methodArgs)
            {
                try
                {
                    return module.ResolveField(token, typeArgs, methodArgs);
                }
                catch (Exception ex)
                {
                    return MissingOrRethrow(module, token, true, ex);
                }
            }

            static object ResolveMethod(Module module, int token, Type[] typeArgs, Type[] methodArgs)
            {
                try
                {
                    return module.ResolveMethod(token, typeArgs, methodArgs);
                }
                catch (Exception ex)
                {
                    return MissingOrRethrow(module, token, false, ex);
                }
            }

            static object MissingOrRethrow(Module module, int token, bool field, Exception ex)
            {
                if (MemberRefs.TryName(module, token, out string className, out string member)
                    && IsRetarget(member, field, className))
                {
                    return new MissingMember { ClassName = className, Name = member, IsField = field };
                }

                if (ex is MissingFieldException || ex is MissingMethodException)
                {
                    return MissingFrom(null, null, ex.Message, field);
                }

                throw ex;
            }

            static bool IsRetarget(string member, bool field, string className)
            {
                string simple = SimpleName(className);
                if (field)
                {
                    return (member == "m_Text" && simple == "GUIContent")
                        || (member == "s_ButonHash" && simple == "GUI");
                }

                return (member == "Internal_Draw_Injected" && simple == "GUIStyle")
                    || (member == "get_roundedRectMaterial" && simple == "GUI")
                    || (member == "HitTest" && simple == "GUIUtility")
                    || (member == "GrabMouseControl" && simple == "GUI")
                    || (member == "HasMouseControl" && simple == "GUI")
                    || (member == "ReleaseMouseControl" && simple == "GUI");
            }

            static MissingMember MissingFrom(string className, string member, string message, bool field)
            {
                if (!string.IsNullOrEmpty(member))
                {
                    return new MissingMember { ClassName = className, Name = member, IsField = field };
                }

                if (message != null)
                {
                    int open = message.IndexOf('\'');
                    int close = open >= 0 ? message.IndexOf('\'', open + 1) : -1;
                    if (open >= 0 && close > open)
                    {
                        string full = message.Substring(open + 1, close - open - 1);
                        int dot = full.LastIndexOf('.');
                        if (dot > 0)
                        {
                            return new MissingMember
                            {
                                ClassName = full.Substring(0, dot),
                                Name = full.Substring(dot + 1),
                                IsField = field
                            };
                        }
                    }
                }

                throw new InvalidOperationException("Unresolved " + (field ? "field" : "method") + " token has no name.");
            }

            /// <summary>
            /// Reads a MemberRef name and declaring type from the assembly file. Reflection on the 1.7
            /// runtime throws ArgumentException for a missing field and the message has no member name.
            ///
            /// Читает имя MemberRef и объявляющий тип из файла сборки. Рефлексия на рантайме 1.7 для
            /// отсутствующего поля бросает ArgumentException, и в тексте нет имени члена.
            /// </summary>
            static class MemberRefs
            {
                static readonly Dictionary<string, Dictionary<int, NamedRef>> cache =
                    new Dictionary<string, Dictionary<int, NamedRef>>(StringComparer.OrdinalIgnoreCase);

                internal static bool TryName(Module module, int token, out string className, out string member)
                {
                    className = null;
                    member = null;
                    if ((token >> 24) != 0x0A)
                    {
                        return false;
                    }

                    string path = module?.Assembly?.Location;
                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        path = module?.FullyQualifiedName;
                    }

                    if (string.IsNullOrEmpty(path) || path[0] == '<' || !File.Exists(path))
                    {
                        return false;
                    }

                    if (!cache.TryGetValue(path, out Dictionary<int, NamedRef> names))
                    {
                        try
                        {
                            names = Read(path);
                        }
                        catch (Exception)
                        {
                            names = new Dictionary<int, NamedRef>();
                        }

                        cache[path] = names;
                    }

                    if (!names.TryGetValue(token, out NamedRef named) || named == null || named.Member.Length == 0)
                    {
                        return false;
                    }

                    className = named.ClassName;
                    member = named.Member;
                    return true;
                }

                static Dictionary<int, NamedRef> Read(string path)
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    int pe = I32(bytes, 0x3C);
                    ushort sections = U16(bytes, pe + 6);
                    ushort optSize = U16(bytes, pe + 20);
                    int opt = pe + 24;
                    int sectionStart = opt + optSize;
                    bool pe32Plus = U16(bytes, opt) == 0x20B;
                    int dataDirectory = opt + (pe32Plus ? 112 : 96);
                    int cli = RvaToOffset(bytes, sectionStart, sections, I32(bytes, dataDirectory + (14 * 8)));
                    int metadata = RvaToOffset(bytes, sectionStart, sections, I32(bytes, cli + 8));
                    if (I32(bytes, metadata) != 0x424A5342)
                    {
                        throw new InvalidOperationException("Metadata signature is not BSJB.");
                    }

                    int versionLength = I32(bytes, metadata + 12);
                    int afterVersion = metadata + 16 + versionLength;
                    ushort streamCount = U16(bytes, afterVersion + 2);
                    int cursor = afterVersion + 4;
                    int strings = -1;
                    int tables = -1;
                    for (int i = 0; i < streamCount; i++)
                    {
                        int offset = I32(bytes, cursor);
                        cursor += 8;
                        int nameAt = cursor;
                        while (bytes[cursor] != 0)
                        {
                            cursor++;
                        }

                        string streamName = Encoding.ASCII.GetString(bytes, nameAt, cursor - nameAt);
                        cursor = (cursor + 4) & ~3;
                        if (streamName == "#Strings")
                        {
                            strings = metadata + offset;
                        }
                        else if (streamName == "#~" || streamName == "#-")
                        {
                            tables = metadata + offset;
                        }
                    }

                    if (strings < 0 || tables < 0)
                    {
                        throw new InvalidOperationException("Metadata streams are missing.");
                    }

                    byte heapSizes = bytes[tables + 6];
                    bool string4 = (heapSizes & 1) != 0;
                    bool guid4 = (heapSizes & 2) != 0;
                    bool blob4 = (heapSizes & 4) != 0;
                    ulong valid = U64(bytes, tables + 8);
                    int[] rowCount = new int[64];
                    int rowPos = tables + 24;
                    for (int table = 0; table < 64; table++)
                    {
                        if ((valid & (1UL << table)) != 0)
                        {
                            rowCount[table] = I32(bytes, rowPos);
                            rowPos += 4;
                        }
                    }

                    int StringIndex()
                    {
                        return string4 ? 4 : 2;
                    }

                    int GuidIndex()
                    {
                        return guid4 ? 4 : 2;
                    }

                    int BlobIndex()
                    {
                        return blob4 ? 4 : 2;
                    }

                    int SimpleIndex(int table)
                    {
                        return rowCount[table] < 65536 ? 2 : 4;
                    }

                    int CodedIndex(int tagBits, int table0, int table1, int table2, int table3, int table4)
                    {
                        int max = rowCount[table0];
                        if (rowCount[table1] > max) max = rowCount[table1];
                        if (table2 >= 0 && rowCount[table2] > max) max = rowCount[table2];
                        if (table3 >= 0 && rowCount[table3] > max) max = rowCount[table3];
                        if (table4 >= 0 && rowCount[table4] > max) max = rowCount[table4];
                        return max < (1 << (16 - tagBits)) ? 2 : 4;
                    }

                    int resolutionScope = CodedIndex(2, 0x00, 0x1A, 0x23, 0x01, -1);
                    int typeDefOrRef = CodedIndex(2, 0x02, 0x01, 0x1B, -1, -1);
                    int memberRefParent = CodedIndex(3, 0x02, 0x01, 0x1A, 0x06, 0x1B);
                    int str = StringIndex();
                    int[] rowSize = new int[11];
                    rowSize[0x00] = 2 + str + GuidIndex() + GuidIndex() + GuidIndex();
                    rowSize[0x01] = resolutionScope + str + str;
                    rowSize[0x02] = 4 + str + str + typeDefOrRef + SimpleIndex(0x04) + SimpleIndex(0x06);
                    rowSize[0x03] = SimpleIndex(0x04);
                    rowSize[0x04] = 2 + str + BlobIndex();
                    rowSize[0x05] = SimpleIndex(0x06);
                    rowSize[0x06] = 4 + 2 + 2 + str + BlobIndex() + SimpleIndex(0x08);
                    rowSize[0x07] = SimpleIndex(0x08);
                    rowSize[0x08] = 2 + 2 + str;
                    rowSize[0x09] = SimpleIndex(0x02) + typeDefOrRef;
                    rowSize[0x0A] = memberRefParent + str + BlobIndex();
                    int[] tableStart = new int[11];
                    int tableCursor = rowPos;
                    for (int table = 0; table <= 0x0A; table++)
                    {
                        if ((valid & (1UL << table)) == 0)
                        {
                            continue;
                        }

                        tableStart[table] = tableCursor;
                        tableCursor += rowSize[table] * rowCount[table];
                    }

                    var names = new Dictionary<int, NamedRef>();
                    for (int row = 1; row <= rowCount[0x0A]; row++)
                    {
                        int record = tableStart[0x0A] + ((row - 1) * rowSize[0x0A]);
                        int parent = ReadIndex(bytes, record, memberRefParent);
                        int nameIndex = ReadIndex(bytes, record + memberRefParent, str);
                        int tag = parent & 7;
                        int parentRow = parent >> 3;
                        string className = "";
                        if (tag == 1)
                        {
                            className = TypeRefName(bytes, tableStart, rowSize, strings, parentRow, resolutionScope, str);
                        }
                        else if (tag == 0)
                        {
                            className = TypeDefName(bytes, tableStart, rowSize, strings, parentRow, str);
                        }

                        names[0x0A000000 | row] = new NamedRef
                        {
                            ClassName = className,
                            Member = Utf8(bytes, strings + nameIndex)
                        };
                    }

                    return names;
                }

                static string TypeRefName(byte[] bytes, int[] tableStart, int[] rowSize, int strings, int row, int scopeSize, int stringSize)
                {
                    if (row <= 0)
                    {
                        return "";
                    }

                    int record = tableStart[0x01] + ((row - 1) * rowSize[0x01]);
                    string name = Utf8(bytes, strings + ReadIndex(bytes, record + scopeSize, stringSize));
                    string ns = Utf8(bytes, strings + ReadIndex(bytes, record + scopeSize + stringSize, stringSize));
                    return ns.Length == 0 ? name : ns + "." + name;
                }

                static string TypeDefName(byte[] bytes, int[] tableStart, int[] rowSize, int strings, int row, int stringSize)
                {
                    if (row <= 0)
                    {
                        return "";
                    }

                    int record = tableStart[0x02] + ((row - 1) * rowSize[0x02]);
                    string name = Utf8(bytes, strings + ReadIndex(bytes, record + 4, stringSize));
                    string ns = Utf8(bytes, strings + ReadIndex(bytes, record + 4 + stringSize, stringSize));
                    return ns.Length == 0 ? name : ns + "." + name;
                }

                static int RvaToOffset(byte[] bytes, int sectionStart, int sectionCount, int rva)
                {
                    for (int i = 0; i < sectionCount; i++)
                    {
                        int section = sectionStart + (i * 40);
                        int virtualAddress = I32(bytes, section + 12);
                        int virtualSize = I32(bytes, section + 8);
                        int rawSize = I32(bytes, section + 16);
                        int raw = I32(bytes, section + 20);
                        int span = virtualSize > rawSize ? virtualSize : rawSize;
                        if (rva >= virtualAddress && rva < virtualAddress + span)
                        {
                            return raw + (rva - virtualAddress);
                        }
                    }

                    throw new InvalidOperationException("Metadata RVA is outside the sections.");
                }

                static string Utf8(byte[] bytes, int at)
                {
                    int end = at;
                    while (bytes[end] != 0)
                    {
                        end++;
                    }

                    return Encoding.UTF8.GetString(bytes, at, end - at);
                }

                static int ReadIndex(byte[] bytes, int at, int size)
                {
                    return size == 2 ? U16(bytes, at) : I32(bytes, at);
                }

                static int I32(byte[] bytes, int at)
                {
                    return BitConverter.ToInt32(bytes, at);
                }

                static ushort U16(byte[] bytes, int at)
                {
                    return BitConverter.ToUInt16(bytes, at);
                }

                static ulong U64(byte[] bytes, int at)
                {
                    return BitConverter.ToUInt64(bytes, at);
                }

                sealed class NamedRef
                {
                    public string ClassName;
                    public string Member;
                }
            }
        }
    }
}
