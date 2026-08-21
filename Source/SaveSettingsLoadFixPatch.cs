using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Save Storage Settings writes crafting bills as recipeDefName or recipeDefNameUft from
    /// the bill class at save time. Loading always constructs that class. If the recipe now uses an
    /// unfinished thing, the loaded Bill_Production is not Bill_ProductionWithUft.
    /// MakeUnfinishedThingIfNeeded casts job.bill and throws InvalidCastException; ingredients already
    /// taken from the pawn vanish. Missing RecipeDef or ThingDef in a template make
    /// DefDatabase.GetNamed log Error before Save Storage Settings warns and skips that entry.
    ///
    /// Fix: coerce bills to Bill_ProductionWithUft on BillStack.AddBill and before the unfinished
    /// toil when recipe.UsesUnfinishedThing. Soft-optional Save Storage Settings Load* hooks: missing
    /// RecipeDef or ThingDef inside SSS TryCreateBill / TryCreateDrugPolicyEntry use GetNamedSilentFail
    /// (transpiler on those SSS methods only — DefDatabase.GetNamed is a shared generic and must not
    /// be prefixed). Original Save Storage Settings Warnings and Errors for a fully failed file
    /// (empty, wrong version, exception) still run unchanged. If some of those GetNamed lookups hit
    /// and some miss, log a separate Warning that not all settings may have been loaded. Filter
    /// allowedDefs / disallowedSpecialFilters are SSS set intersection with live defs: unknown names
    /// are dropped without GetNamed, so that Warning does not fire. When Save Storage Settings is
    /// not in the mod list, Load hooks and SSS GetNamed transpilers are skipped; bill coerce still
    /// applies.
    ///
    /// Проблема: Save Storage Settings пишет задания как recipeDefName или recipeDefNameUft по классу
    /// на момент сохранения. Загрузка всегда создаёт этот класс. Если рецепт теперь с незавершёнкой,
    /// загруженный Bill_Production не Bill_ProductionWithUft. MakeUnfinishedThingIfNeeded кастит
    /// job.bill и бросает InvalidCastException; ингредиенты уже сняты с пешки и пропадают. Отсутствующие
    /// RecipeDef или ThingDef в шаблоне сначала дают Error из DefDatabase.GetNamed, затем Warning
    /// Save Storage Settings и пропуск записи.
    ///
    /// Исправление: приводить задание к Bill_ProductionWithUft в BillStack.AddBill и перед toil
    /// незавершёнки, если recipe.UsesUnfinishedThing. Soft-optional хуки Load* у Save Storage
    /// Settings: отсутствующие RecipeDef или ThingDef в TryCreateBill / TryCreateDrugPolicyEntry
    /// через GetNamedSilentFail (transpiler только этих методов SSS — DefDatabase.GetNamed общий
    /// generic, его Prefix нельзя). Исходные Warning и Error при полной ошибке файла (пустой,
    /// неверная версия, exception) остаются как были. Если часть этих GetNamed попала, а часть
    /// нет — отдельный Warning, что могли быть загружены не все настройки. Filter allowedDefs /
    /// disallowedSpecialFilters — пересечение SSS с живыми def: неизвестные имена отбрасываются
    /// без GetNamed, этот Warning не пишется. Если Save Storage Settings нет в списке модов, хуки
    /// Load и transpiler GetNamed пропускаются; coerce заданий всё равно действует.
    /// </summary>
    public static class SaveSettingsLoadFixFeatures
    {
        private const string IoUtilTypeName = "SaveStorageSettings.IOUtil";

        [ThreadStatic]
        private static int silentGetNamedHits;

        [ThreadStatic]
        private static int silentGetNamedMisses;

        /// <summary>
        /// Registers bill coerce hooks and optional Save Storage Settings load guards.
        ///
        /// Вешает coerce заданий и опциональные предохранители загрузки Save Storage Settings.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    AccessTools.Method(typeof(BillStack), nameof(BillStack.AddBill)),
                    prefix: new HarmonyMethod(
                        typeof(SaveSettingsLoadFixFeatures),
                        nameof(BillStack_AddBill_Prefix)));

                harmony.Patch(
                    AccessTools.Method(typeof(Toils_Recipe), nameof(Toils_Recipe.MakeUnfinishedThingIfNeeded)),
                    postfix: new HarmonyMethod(
                        typeof(SaveSettingsLoadFixFeatures),
                        nameof(MakeUnfinishedThingIfNeeded_Postfix)));

                PatchSaveStorageLoadMethods(harmony);
                Log.Message("[SaveSettingsLoadFixPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[SaveSettingsLoadFixPatch] Failed to apply patches: " + e);
            }
        }

        /// <summary>
        /// Replaces the bill argument with a UFT bill when the recipe requires unfinished things.
        ///
        /// Подменяет аргумент на задание UFT, если рецепт требует незавершёнку.
        /// </summary>
        public static void BillStack_AddBill_Prefix(ref Bill bill)
        {
            if (!KebabTweaksSettings.EnableSaveSettingsLoadFix)
            {
                return;
            }

            bill = CoerceBill(bill);
        }

        /// <summary>
        /// Runs coerce on the current job bill before vanilla MakeUnfinishedThingIfNeeded initAction.
        ///
        /// Перед vanilla initAction MakeUnfinishedThingIfNeeded приводит тип текущего задания job.
        /// </summary>
        public static void MakeUnfinishedThingIfNeeded_Postfix(Toil __result)
        {
            if (__result == null || __result.initAction == null)
            {
                return;
            }

            Action original = __result.initAction;
            Toil toil = __result;
            toil.initAction = delegate
            {
                if (KebabTweaksSettings.EnableSaveSettingsLoadFix)
                {
                    Job job = toil.actor == null || toil.actor.jobs == null ? null : toil.actor.jobs.curJob;
                    CoerceJobBill(job);
                }

                original();
            };
        }

        /// <summary>
        /// After a non-list SSS load, warn if TryCreate* GetNamed had hits and misses.
        /// Filter allowedDefs unknown tokens never call GetNamed, so this stays silent.
        ///
        /// После загрузки SSS без списка заданий предупреждает, если GetNamed в TryCreate*
        /// и попал, и промахнулся. Неизвестные имена в filter allowedDefs GetNamed не вызывают,
        /// поэтому здесь тишина.
        /// </summary>
        public static void LoadOther_Postfix()
        {
            if (KebabTweaksSettings.EnableSaveSettingsLoadFix)
            {
                MaybeWarnPartialLoad(silentGetNamedHits > 0);
            }
        }

        /// <summary>
        /// Always clears missing-def counters if a Load* method threw.
        ///
        /// Всегда сбрасывает счётчики отсутствующих def, если Load* бросил исключение.
        /// </summary>
        public static Exception Load_Finalizer(Exception __exception)
        {
            EndSssLoad();
            return __exception;
        }

        /// <summary>
        /// Resets missing-def counters at the start of an SSS Load* parse.
        /// Does not skip the original Load method, so empty-file and wrong-version Warnings stay.
        ///
        /// Сбрасывает счётчики отсутствующих def в начале разбора SSS Load*.
        /// Оригинал Load не пропускается: Warning на пустом файле и неверной версии остаются.
        /// </summary>
        public static void LoadAny_Prefix()
        {
            BeginSssLoad();
        }

        /// <summary>
        /// RecipeDef lookup used from transpiled SSS TryCreateBill. SilentFail when the fix is
        /// enabled so missing recipes do not Error; otherwise vanilla GetNamed.
        ///
        /// Поиск RecipeDef из transpiled SSS TryCreateBill. При включённом фиксе SilentFail,
        /// иначе vanilla GetNamed.
        /// </summary>
        public static RecipeDef SssGetNamedRecipeDef(string defName)
        {
            if (!KebabTweaksSettings.EnableSaveSettingsLoadFix)
            {
                return DefDatabase<RecipeDef>.GetNamed(defName);
            }

            RecipeDef def = DefDatabase<RecipeDef>.GetNamedSilentFail(defName);
            NoteGetNamedResult(def != null);
            return def;
        }

        /// <summary>
        /// ThingDef lookup used from transpiled SSS TryCreateDrugPolicyEntry. SilentFail when the
        /// fix is enabled so missing drugs do not Error; otherwise vanilla GetNamed.
        ///
        /// Поиск ThingDef из transpiled SSS TryCreateDrugPolicyEntry. При включённом фиксе SilentFail,
        /// иначе vanilla GetNamed.
        /// </summary>
        public static ThingDef SssGetNamedThingDef(string defName)
        {
            if (!KebabTweaksSettings.EnableSaveSettingsLoadFix)
            {
                return DefDatabase<ThingDef>.GetNamed(defName);
            }

            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            NoteGetNamedResult(def != null);
            return def;
        }

        /// <summary>
        /// Replaces RecipeDef and ThingDef GetNamed inside SSS create helpers with SilentFail
        /// wrappers. Does not patch DefDatabase.GetNamed itself.
        ///
        /// Подменяет GetNamed для RecipeDef и ThingDef внутри хелперов SSS на SilentFail.
        /// Сам DefDatabase.GetNamed не патчится.
        /// </summary>
        public static IEnumerable<CodeInstruction> SssGetNamedSilentFailTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo recipeHelper = AccessTools.Method(
                typeof(SaveSettingsLoadFixFeatures), nameof(SssGetNamedRecipeDef));
            MethodInfo thingHelper = AccessTools.Method(
                typeof(SaveSettingsLoadFixFeatures), nameof(SssGetNamedThingDef));

            foreach (CodeInstruction instruction in instructions)
            {
                MethodInfo called = instruction.operand as MethodInfo;
                int argc;
                if (IsDefDatabaseGetNamed(called, typeof(RecipeDef), out argc) && recipeHelper != null)
                {
                    if (argc > 1)
                    {
                        yield return new CodeInstruction(OpCodes.Pop).WithLabels(instruction.labels);
                    }

                    CodeInstruction call = new CodeInstruction(OpCodes.Call, recipeHelper);
                    if (argc <= 1)
                    {
                        call.labels.AddRange(instruction.labels);
                    }

                    yield return call;
                    continue;
                }

                if (IsDefDatabaseGetNamed(called, typeof(ThingDef), out argc) && thingHelper != null)
                {
                    if (argc > 1)
                    {
                        yield return new CodeInstruction(OpCodes.Pop).WithLabels(instruction.labels);
                    }

                    CodeInstruction call = new CodeInstruction(OpCodes.Call, thingHelper);
                    if (argc <= 1)
                    {
                        call.labels.AddRange(instruction.labels);
                    }

                    yield return call;
                    continue;
                }

                yield return instruction;
            }
        }
        /// <summary>
        /// After an SSS list load, coerce bill types and warn on a partial load.
        ///
        /// После загрузки списка SSS приводит типы заданий и предупреждает при частичной загрузке.
        /// </summary>
        public static void LoadList_Postfix(List<Bill> __result)
        {
            if (!KebabTweaksSettings.EnableSaveSettingsLoadFix || __result == null)
            {
                return;
            }

            for (int i = 0; i < __result.Count; i++)
            {
                __result[i] = CoerceBill(__result[i]);
            }

            MaybeWarnPartialLoad(__result.Count > 0);
        }

        private static void PatchSaveStorageLoadMethods(Harmony harmony)
        {
            Type ioUtil = AccessTools.TypeByName(IoUtilTypeName);
            if (ioUtil == null)
            {
                Log.Message("[SaveSettingsLoadFixPatch] Save Storage Settings not loaded; IOUtil guards skipped.");
                return;
            }

            PatchSssGetNamedCalls(harmony, ioUtil);

            TryPatchLoad(
                harmony,
                ioUtil,
                "LoadCraftingBills",
                nameof(LoadList_Postfix));
            TryPatchLoad(
                harmony,
                ioUtil,
                "LoadOperationBills",
                nameof(LoadList_Postfix));
            TryPatchLoad(
                harmony,
                ioUtil,
                "LoadFilters",
                nameof(LoadOther_Postfix));
            TryPatchLoad(
                harmony,
                ioUtil,
                "LoadPolicy",
                nameof(LoadOther_Postfix));
            TryPatchLoad(
                harmony,
                ioUtil,
                "LoadFoodRestriction",
                nameof(LoadOther_Postfix));
            TryPatchLoad(
                harmony,
                ioUtil,
                "LoadFoodRestrictionSettings",
                nameof(LoadOther_Postfix));
            TryPatchLoad(
                harmony,
                ioUtil,
                "LoadFoodPolicy",
                nameof(LoadOther_Postfix));
        }

        private static void PatchSssGetNamedCalls(Harmony harmony, Type ioUtil)
        {
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(ioUtil))
            {
                if (method == null
                    || (method.Name != "TryCreateBill" && method.Name != "TryCreateDrugPolicyEntry"))
                {
                    continue;
                }

                try
                {
                    harmony.Patch(
                        method,
                        transpiler: new HarmonyMethod(
                            typeof(SaveSettingsLoadFixFeatures),
                            nameof(SssGetNamedSilentFailTranspiler)));
                }
                catch (Exception e)
                {
                    Log.Warning("[SaveSettingsLoadFixPatch] SSS GetNamed transpiler skipped for "
                        + method.Name + ": " + e.Message);
                }
            }
        }

        private static bool IsDefDatabaseGetNamed(MethodInfo method, Type defType, out int argc)
        {
            argc = 0;
            if (method == null || method.Name != "GetNamed" || defType == null)
            {
                return false;
            }

            Type declaring = method.DeclaringType;
            if (declaring == null || !declaring.IsGenericType)
            {
                return false;
            }

            if (declaring.GetGenericTypeDefinition() != typeof(DefDatabase<>))
            {
                return false;
            }

            Type[] args = declaring.GetGenericArguments();
            if (args.Length != 1 || args[0] != defType)
            {
                return false;
            }

            argc = method.GetParameters().Length;
            return true;
        }

        private static void TryPatchLoad(
            Harmony harmony,
            Type ioUtil,
            string methodName,
            string postfixName)
        {
            MethodInfo method = AccessTools.Method(ioUtil, methodName);
            if (method == null)
            {
                return;
            }

            harmony.Patch(
                method,
                prefix: new HarmonyMethod(typeof(SaveSettingsLoadFixFeatures), nameof(LoadAny_Prefix)),
                postfix: new HarmonyMethod(typeof(SaveSettingsLoadFixFeatures), postfixName),
                finalizer: new HarmonyMethod(typeof(SaveSettingsLoadFixFeatures), nameof(Load_Finalizer)));
        }

        private static void BeginSssLoad()
        {
            if (!KebabTweaksSettings.EnableSaveSettingsLoadFix)
            {
                return;
            }

            silentGetNamedHits = 0;
            silentGetNamedMisses = 0;
        }

        private static void EndSssLoad()
        {
            silentGetNamedHits = 0;
            silentGetNamedMisses = 0;
        }

        private static void NoteGetNamedResult(bool found)
        {
            if (found)
            {
                silentGetNamedHits++;
            }
            else
            {
                silentGetNamedMisses++;
            }
        }

        private static void MaybeWarnPartialLoad(bool someSettingsLoaded)
        {
            if (!someSettingsLoaded || silentGetNamedMisses <= 0)
            {
                return;
            }

            Log.Warning("KebabTweaks.SaveSettingsLoad.PartialLoadWarning".Translate());
        }

        private static Bill CoerceBill(Bill bill)
        {
            if (bill == null || bill.recipe == null || !bill.recipe.UsesUnfinishedThing)
            {
                return bill;
            }

            if (bill is Bill_ProductionWithUft)
            {
                return bill;
            }

            Bill_Production production = bill as Bill_Production;
            if (production == null)
            {
                return bill;
            }

            Bill_ProductionWithUft uft = new Bill_ProductionWithUft(production.recipe);
            CopyInstanceFields(production, uft);
            return uft;
        }

        private static void CoerceJobBill(Job job)
        {
            if (job == null || job.bill == null)
            {
                return;
            }

            Bill coerced = CoerceBill(job.bill);
            if (ReferenceEquals(coerced, job.bill))
            {
                return;
            }

            ReplaceInBillStack(job.bill, coerced);
            job.bill = coerced;
        }

        private static void ReplaceInBillStack(Bill oldBill, Bill newBill)
        {
            BillStack stack = oldBill.billStack;
            if (stack == null)
            {
                return;
            }

            FieldInfo field = AccessTools.Field(typeof(BillStack), "bills");
            IList list = field == null ? null : field.GetValue(stack) as IList;
            if (list == null)
            {
                return;
            }

            int index = list.IndexOf(oldBill);
            if (index < 0)
            {
                return;
            }

            list[index] = newBill;
            newBill.billStack = stack;
        }

        private static void CopyInstanceFields(object source, object dest)
        {
            Type type = typeof(Bill_Production);
            while (type != null && type != typeof(object))
            {
                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    if (field.IsLiteral || field.Name == "billStack")
                    {
                        continue;
                    }

                    try
                    {
                        field.SetValue(dest, field.GetValue(source));
                    }
                    catch (Exception)
                    {
                    }
                }

                type = type.BaseType;
            }
        }
    }
}
