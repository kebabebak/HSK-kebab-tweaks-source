using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: HSK 1.6 CompBillableDrill (Core_SK Masaki module, not vanilla) stops a deep drill
    /// when the map holds enough of the drilled resource. The slider is Dialog_Slider from -1
    /// (no limit) to a hardcoded 5000, and the counted def is locked to the drill resource.
    /// HSK 1.5 has no CompBillableDrill and no control that chooses what a deep drill extracts.
    /// The quarry mine-resources mode is a different building, so a deep drill there has no limit.
    ///
    /// Fix: one live patch. On 1.6 it replaces the Set Limit window and owns the CanDrillNow count
    /// (the HSK postfix is skipped). On 1.5 the same window and count are added, without a control
    /// for what the drill extracts. Extra options:
    /// type the number, a slider maximum, and a per-drill stored item. Russian text for the 1.6
    /// drill keys is added only when that language has no own string.
    ///
    /// Проблема: в HSK 1.6 CompBillableDrill (модуль Masaki Core_SK, не vanilla) останавливает
    /// буровую установку, когда на карте достаточно добываемого ресурса. Ползунок - Dialog_Slider
    /// от -1 (без лимита) до зашитых 5000, а считаемый def привязан к ресурсу буровой установки.
    /// В HSK 1.5 нет CompBillableDrill и нет выбора, что именно добывает буровая установка.
    /// Режим добычи ресурсов есть у карьера, это другая постройка, поэтому лимита там нет.
    ///
    /// Исправление: один live-патч. В 1.6 он подменяет окно Set Limit и сам считает лимит в
    /// CanDrillNow (postfix HSK пропускается). В 1.5 добавляются то же окно и тот же подсчёт,
    /// без выбора того, что буровая установка добывает.
    /// Дополнительно: ввод числа, максимум ползунка и отдельный хранимый предмет. Русский текст
    /// ключей буровой установки в 1.6 добавляется, только если у языка ещё нет своей строки.
    /// </summary>
    public static class DrillLimitFeatures
    {
        private const int CountCacheTicks = 60;

        private static readonly Dictionary<int, int> CountCacheTicksByKey = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> CountCacheValueByKey = new Dictionary<int, int>();

        public static void Apply(Harmony harmony)
        {
            MethodInfo canDrill = AccessTools.Method(typeof(CompDeepDrill), "CanDrillNow");
            if (canDrill != null)
            {
                harmony.Patch(
                    canDrill,
                    postfix: new HarmonyMethod(typeof(DrillLimitFeatures), nameof(CanDrillNowPostfix))
                    {
                        priority = Priority.Last
                    });
            }

            MethodInfo deepGizmos = AccessTools.Method(typeof(CompDeepDrill), "CompGetGizmosExtra");
            if (deepGizmos != null)
            {
                harmony.Patch(deepGizmos, postfix: new HarmonyMethod(typeof(DrillLimitFeatures), nameof(DeepDrillGizmosPostfix)));
            }

            MethodInfo inspect = AccessTools.Method(typeof(CompDeepDrill), "CompInspectStringExtra");
            if (inspect != null)
            {
                harmony.Patch(inspect, postfix: new HarmonyMethod(typeof(DrillLimitFeatures), nameof(InspectPostfix)));
            }

            MethodInfo hskCheck = AccessTools.Method("SK.BillableDrillPatch:Check");
            if (hskCheck != null)
            {
                harmony.Patch(hskCheck, prefix: new HarmonyMethod(typeof(DrillLimitFeatures), nameof(SkipHskLimitPrefix)));
            }

            Type billable = DrillLimitHsk.BillableType;
            MethodInfo billableGizmos = billable == null ? null : AccessTools.Method(billable, "CompGetGizmosExtra");
            if (billableGizmos != null)
            {
                harmony.Patch(billableGizmos, postfix: new HarmonyMethod(typeof(DrillLimitFeatures), nameof(BillableGizmosPostfix)));
            }

            MethodInfo loadData = AccessTools.Method(typeof(LoadedLanguage), nameof(LoadedLanguage.LoadData));
            if (loadData != null)
            {
                harmony.Patch(loadData, postfix: new HarmonyMethod(typeof(DrillLimitFeatures), nameof(LoadDataPostfix)));
            }

            InjectLoadedButtonTranslations();
            PatchLimitReachedMenu(harmony);
        }

        /// <summary>
        /// A reached limit makes HasJobOnThing false, so the drill order and the already-drilling
        /// line are absent. This adds the one inactive line.
        ///
        /// Достигнутый лимит делает HasJobOnThing ложным, поэтому приказа бурить и строки о том,
        /// что поселенец уже начал бурить, нет. Сюда добавляется одна неактивная.
        /// </summary>
        private static void PatchLimitReachedMenu(Harmony harmony)
        {
#if RIMWORLD_1_6
            MethodInfo giverOption = AccessTools.Method(typeof(FloatMenuOptionProvider_WorkGivers), "GetWorkGiverOption");
            if (giverOption != null)
            {
                harmony.Patch(giverOption, postfix: new HarmonyMethod(typeof(DrillLimitFeatures), nameof(WorkGiverOptionPostfix)));
            }
#else
            MethodInfo workOrders = AccessTools.Method(typeof(FloatMenuMakerMap), "AddJobGiverWorkOrders");
            if (workOrders != null)
            {
                harmony.Patch(workOrders, postfix: new HarmonyMethod(typeof(DrillLimitFeatures), nameof(AddJobGiverWorkOrdersPostfix)));
            }
#endif
        }

        public static void LoadDataPostfix(LoadedLanguage __instance)
        {
            InjectButtonTranslations(__instance);
        }

        private static void InjectLoadedButtonTranslations()
        {
            if (LanguageDatabase.AllLoadedLanguages == null)
            {
                return;
            }

            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                InjectButtonTranslations(language);
            }
        }

        /// <summary>
        /// Russian drill keys, only when that language has none. 1.5 has no such keys.
        /// Inspect lines use the 1.5 wording. Selected resource keeps {0_label} because the
        /// call passes a ThingDef.
        ///
        /// Русские ключи буровой установки, только если у языка их нет. В 1.5 таких ключей нет.
        /// Строки инфокарты берут формулировки 1.5. У выбранного ресурса остаётся {0_label},
        /// потому что вызов передаёт ThingDef.
        /// </summary>
        private static void InjectButtonTranslations(LoadedLanguage language)
        {
            if (language == null || !IsRussianLanguage(language))
            {
                return;
            }

            LoadedLanguage english = LanguageDatabase.defaultLanguage;
            if (english == null)
            {
                return;
            }

            TryInject(language, english, "CSP_BillableDrillLimit", "Задать лимит");
            TryInject(language, english, "CSP_DrillResourceMode", "Ресурс буровой установки: {0}");
            TryInject(language, english, "CSP_DrillResourceMinerals", "Сканированные минералы");
            TryInject(
                language,
                english,
                "CSP_DrillResourceModeDesc",
                "Выберите, добывает ли буровая установка сканированные минералы или обычный сыпучий материал.");
            TryInject(language, english, "CSP_DrillResourceInspect", "Выбранный ресурс: {0_label}");
            TryInject(language, english, "CSP_CurrentLimit", "Текущий лимит: {0}");
        }

        private static void TryInject(LoadedLanguage language, LoadedLanguage english, string key, string value)
        {
            if (language.keyedReplacements == null
                || !english.HaveTextForKey(key)
                || language.HaveTextForKey(key))
            {
                return;
            }

            language.keyedReplacements[key] = new LoadedLanguage.KeyedReplacement
            {
                key = key,
                value = value,
                isPlaceholder = false
            };
        }

        private static bool IsRussianLanguage(LoadedLanguage language)
        {
            if (language.folderName != null
                && language.folderName.StartsWith("Russian", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string friendly = language.FriendlyNameEnglish;
            return friendly != null && friendly.StartsWith("Russian", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// While this patch is on, returning false skips HSK's CanDrillNow postfix so it does not
        /// also apply its count.
        ///
        /// Пока патч включен, false пропускает postfix CanDrillNow из HSK, чтобы тот не считал лимит ещё раз.
        /// </summary>
        public static bool SkipHskLimitPrefix()
        {
            return !KebabTweaksSettings.EnableDrillLimitPatch;
        }

        /// <summary>
        /// Stops the drill once the map has enough of the counted def. The sum is the same one
        /// HSK uses (spawned stacks of that def on the drill map, not the resource readout).
        ///
        /// Останавливает буровую установку, когда на карте достаточно считаемого def. Сумма та же,
        /// что у HSK (появившиеся стаки этого def на карте буровой установки, не сводка ресурсов).
        /// </summary>
        public static void CanDrillNowPostfix(CompDeepDrill __instance, ref bool __result)
        {
            if (!KebabTweaksSettings.EnableDrillLimitPatch || __instance?.parent == null)
            {
                return;
            }

            Thing drill = __instance.parent;
            object billable = DrillLimitHsk.GetComp(drill);
            if (billable != null && DrillLimitHsk.SelectedAlternative(billable) != null)
            {
                CompPowerTrader power = drill.TryGetComp<CompPowerTrader>();
                __result = power == null || power.PowerOn;
            }

            int limit = billable != null ? DrillLimitHsk.LimitOf(billable) : DrillLimitState.LimitOf(drill);
            if (!__result || limit < 0)
            {
                return;
            }

            ThingDef counted = DrillLimitState.EffectiveResource(drill, billable);
            if (counted == null || drill.Map == null)
            {
                return;
            }

            if (CountOnMap(drill.Map, counted) >= limit)
            {
                __result = false;
            }
        }

        /// <summary>
        /// True when this drill's stored limit is zero or more and the map already holds that many
        /// of the counted def. Missing power or an empty deposit is not this case.
        ///
        /// Истина, когда сохранённый лимит этой буровой установки не ниже нуля и на карте уже есть
        /// столько считаемого def. Нет энергии или пустое месторождение - не этот случай.
        /// </summary>
        private static bool LimitReached(Thing drill, out int limit, out ThingDef counted)
        {
            limit = -1;
            counted = null;
            if (!KebabTweaksSettings.EnableDrillLimitPatch || drill?.Map == null)
            {
                return false;
            }

            object billable = DrillLimitHsk.GetComp(drill);
            limit = billable != null ? DrillLimitHsk.LimitOf(billable) : DrillLimitState.LimitOf(drill);
            if (limit < 0)
            {
                return false;
            }

            counted = DrillLimitState.EffectiveResource(drill, billable);
            return counted != null && CountOnMap(drill.Map, counted) >= limit;
        }

        public static void WorkGiverOptionPostfix(Pawn pawn, WorkGiverDef workGiver, LocalTargetInfo target, ref FloatMenuOption __result)
        {
            if (!IsDrillWorkOrder(pawn, workGiver, pawn.Drafted) || !target.HasThing)
            {
                return;
            }

            Thing drill = target.Thing;
            if (!IsPlayerDeepDrill(pawn, drill) || !LimitReached(drill, out int limit, out ThingDef counted))
            {
                return;
            }

            __result = LimitReachedOption(drill, limit, counted);
        }

        public static void AddJobGiverWorkOrdersPostfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts, bool drafted)
        {
            if (!KebabTweaksSettings.EnableDrillLimitPatch || pawn?.Map == null || opts == null)
            {
                return;
            }

            if (pawn.thinker == null || pawn.thinker.TryGetMainTreeThinkNode<JobGiver_Work>() == null)
            {
                return;
            }

            WorkGiverDef giver = DrillWorkGiver();
            if (!IsDrillWorkOrder(pawn, giver, drafted))
            {
                return;
            }

            IntVec3 cell = IntVec3.FromVector3(clickPos);
            if (!cell.InBounds(pawn.Map))
            {
                return;
            }

            int addedFor = 0;
            foreach (Thing candidate in cell.GetThingList(pawn.Map))
            {
                if (candidate == null || !candidate.Spawned)
                {
                    continue;
                }

                Thing drill = candidate;
                CompSelectProxy proxy = candidate.TryGetComp<CompSelectProxy>();
                if (proxy != null && proxy.thingToSelect != null)
                {
                    drill = proxy.thingToSelect;
                }

                if (!IsPlayerDeepDrill(pawn, drill) || addedFor == drill.thingIDNumber)
                {
                    continue;
                }

                if (!LimitReached(drill, out int limit, out ThingDef counted))
                {
                    continue;
                }

                RemoveDrillWorkRows(opts, drill, giver);
                opts.Add(LimitReachedOption(drill, limit, counted));
                addedFor = drill.thingIDNumber;
            }
        }

        private static bool IsDrillWorkOrder(Pawn pawn, WorkGiverDef workGiver, bool drafted)
        {
            if (!KebabTweaksSettings.EnableDrillLimitPatch || pawn == null || workGiver == null)
            {
                return false;
            }

            if (workGiver.defName != "Drill" && workGiver.giverClass != typeof(WorkGiver_DeepDrill))
            {
                return false;
            }

            if (!workGiver.directOrderable)
            {
                return false;
            }

            return !drafted || workGiver.canBeDoneWhileDrafted;
        }

        private static bool IsPlayerDeepDrill(Pawn pawn, Thing thing)
        {
            return thing != null && thing.def == ThingDefOf.DeepDrill && thing.Faction == pawn.Faction;
        }

        private static WorkGiverDef DrillWorkGiver()
        {
            return DefDatabase<WorkGiverDef>.GetNamedSilentFail("Drill");
        }

        private static void RemoveDrillWorkRows(List<FloatMenuOption> opts, Thing drill, WorkGiverDef giver)
        {
            string gerund = giver.gerund;
            string prioritize = "PrioritizeGeneric".Translate(gerund, drill.Label).CapitalizeFirst();
            string already = "CannotGenericAlreadyAm".Translate(gerund, drill.LabelShort, drill);
            for (int i = opts.Count - 1; i >= 0; i--)
            {
                string label = opts[i].Label;
                if (label == null)
                {
                    continue;
                }

                label = label.TrimEnd();
                if (label == prioritize || label == already)
                {
                    opts.RemoveAt(i);
                }
            }
        }

        private static FloatMenuOption LimitReachedOption(Thing drill, int limit, ThingDef counted)
        {
            return new FloatMenuOption(DrillLimitText.CannotDrill(drill, limit, counted), null, drill, Color.white);
        }

        public static void DeepDrillGizmosPostfix(CompDeepDrill __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!KebabTweaksSettings.EnableDrillLimitPatch || __instance?.parent == null)
            {
                return;
            }

            if (DrillLimitHsk.GetComp(__instance.parent) != null)
            {
                return;
            }

            List<Gizmo> gizmos = __result == null ? new List<Gizmo>() : new List<Gizmo>(__result);
            Thing drill = __instance.parent;
            ThingDef counted = DrillLimitState.EffectiveResource(drill, null);
            if (counted != null)
            {
                gizmos.Add(MakeSetLimitCommand(drill, counted));
            }

            __result = gizmos;
        }

        public static void BillableGizmosPostfix(ThingComp __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!KebabTweaksSettings.EnableDrillLimitPatch || __instance?.parent == null || __result == null)
            {
                return;
            }

            string setLimitLabel = "CSP_BillableDrillLimit".Translate();
            List<Gizmo> gizmos = new List<Gizmo>(__result);
            for (int i = 0; i < gizmos.Count; i++)
            {
                Command_Action command = gizmos[i] as Command_Action;
                if (command == null || command.defaultLabel != setLimitLabel)
                {
                    continue;
                }

                ThingDef counted = DrillLimitState.EffectiveResource(__instance.parent, __instance);
                if (counted == null)
                {
                    gizmos.RemoveAt(i);
                }
                else
                {
                    gizmos[i] = MakeSetLimitCommand(__instance.parent, counted);
                }

                break;
            }

            __result = gizmos;
        }

        public static void InspectPostfix(CompDeepDrill __instance, ref string __result)
        {
            if (!KebabTweaksSettings.EnableDrillLimitPatch || __instance?.parent == null)
            {
                return;
            }

            if (DrillLimitHsk.GetComp(__instance.parent) != null)
            {
                return;
            }

            int limit = DrillLimitState.LimitOf(__instance.parent);
            if (limit == -1)
            {
                return;
            }

            string line = DrillLimitText.CurrentLimitRaw().Replace("{0}", limit.ToString());
            if (!__result.NullOrEmpty())
            {
                __result = __result + "\n" + line;
            }
            else
            {
                __result = line;
            }
        }

        private static Command_Action MakeSetLimitCommand(Thing drill, ThingDef counted)
        {
            int limit = DrillLimitHsk.GetComp(drill) is object billable
                ? DrillLimitHsk.LimitOf(billable)
                : DrillLimitState.LimitOf(drill);
            return new Command_Action
            {
                defaultLabel = DrillLimitText.SetLimitLabel(),
                defaultDesc = DrillLimitText.LimitButtonDesc(drill, counted),
                icon = counted.uiIcon,
                action = delegate
                {
                    List<Thing> group = DrillLimitState.DrillsSharing(drill, counted);
                    Find.WindowStack.Add(new DrillLimitWindow(group, limit, counted));
                },
                groupable = true,
                groupKey = drill.def.shortHash * 397 ^ counted.shortHash
            };
        }

        /// <summary>
        /// Several pawns ask CanDrillNow in the same second, so the map sum is cached briefly.
        ///
        /// Несколько пешек спрашивают CanDrillNow в ту же секунду, поэтому сумма по карте ненадолго кэшируется.
        /// </summary>
        private static int CountOnMap(Map map, ThingDef def)
        {
            int key = (map.uniqueID * 397) ^ def.index;
            int now = Find.TickManager.TicksGame;
            if (CountCacheTicksByKey.TryGetValue(key, out int cachedTick)
                && now - cachedTick < CountCacheTicks
                && now >= cachedTick
                && CountCacheValueByKey.TryGetValue(key, out int cached))
            {
                return cached;
            }

            int sum = 0;
            List<Thing> things = map.listerThings.ThingsOfDef(def);
            for (int i = 0; i < things.Count; i++)
            {
                sum += things[i].stackCount;
            }

            CountCacheTicksByKey[key] = now;
            CountCacheValueByKey[key] = sum;
            return sum;
        }
    }

    /// <summary>
    /// Reads CompBillableDrill when the 1.6 assembly is present. 1.5 has no such type.
    ///
    /// Читает CompBillableDrill, если сборка 1.6 на месте. В 1.5 такого типа нет.
    /// </summary>
    public static class DrillLimitHsk
    {
        private static bool resolved;
        private static Type billableType;
        private static PropertyInfo limitProperty;
        private static PropertyInfo currentProperty;
        private static PropertyInfo selectedProperty;
        private static MethodInfo notifyMethod;

        public static Type BillableType
        {
            get
            {
                Resolve();
                return billableType;
            }
        }

        public static object GetComp(Thing thing)
        {
            Resolve();
            if (billableType == null || !(thing is ThingWithComps withComps))
            {
                return null;
            }

            List<ThingComp> comps = withComps.AllComps;
            for (int i = 0; i < comps.Count; i++)
            {
                if (billableType.IsInstanceOfType(comps[i]))
                {
                    return comps[i];
                }
            }

            return null;
        }

        public static int LimitOf(object comp)
        {
            Resolve();
            if (comp == null || limitProperty == null)
            {
                return -1;
            }

            object value = limitProperty.GetValue(comp, null);
            return value is int number ? number : -1;
        }

        public static void SetLimit(object comp, int value)
        {
            Resolve();
            if (comp == null || notifyMethod == null)
            {
                return;
            }

            notifyMethod.Invoke(comp, new object[] { value });
        }

        public static ThingDef CurrentResource(object comp)
        {
            Resolve();
            if (comp == null || currentProperty == null)
            {
                return null;
            }

            return currentProperty.GetValue(comp, null) as ThingDef;
        }

        public static ThingDef SelectedAlternative(object comp)
        {
            Resolve();
            if (comp == null || selectedProperty == null)
            {
                return null;
            }

            return selectedProperty.GetValue(comp, null) as ThingDef;
        }

        private static void Resolve()
        {
            if (resolved)
            {
                return;
            }

            resolved = true;
            billableType = AccessTools.TypeByName("SK.CompBillableDrill");
            if (billableType == null)
            {
                return;
            }

            limitProperty = AccessTools.Property(billableType, "BillableResourceLimit");
            currentProperty = AccessTools.Property(billableType, "CurrentResource");
            selectedProperty = AccessTools.Property(billableType, "SelectedAlternativeResource");
            notifyMethod = AccessTools.Method(billableType, "Nofity_LimitSelected");
        }
    }

    /// <summary>
    /// Per-drill limit memory. 1.6 keeps the number on CompBillableDrill and only the pinned
    /// count def here. 1.5 keeps the number and the pin here.
    /// A pin equal to the drilled resource is dropped so the two stay in sync.
    ///
    /// Память лимита каждой буровой установки. В 1.6 число лежит в CompBillableDrill, здесь
    /// только закреплённый def счётчика. В 1.5 здесь число и закрепление.
    /// Закрепление, совпавшее с ресурсом буровой установки, снимается, и они снова синхронны.
    /// </summary>
    public class DrillLimitGameComponent : GameComponent
    {
        private List<DrillLimitRecord> records = new List<DrillLimitRecord>();
        private Dictionary<int, DrillLimitRecord> byId = new Dictionary<int, DrillLimitRecord>();

        public DrillLimitGameComponent(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref records, "drillLimitRecords", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (records == null)
                {
                    records = new List<DrillLimitRecord>();
                }

                Rebuild();
                if (!KebabTweaksSettings.EnableDrillLimitAnyResource)
                {
                    ClearPins();
                }
            }
        }

        public int LimitOf(int thingId)
        {
            return byId.TryGetValue(thingId, out DrillLimitRecord record) ? record.limit : -1;
        }

        public void SetLimit(int thingId, int limit)
        {
            DrillLimitRecord record = Record(thingId);
            record.limit = limit;
            DropEmpty(record);
        }

        public ThingDef PinOf(int thingId)
        {
            if (!byId.TryGetValue(thingId, out DrillLimitRecord record) || record.pinDefName.NullOrEmpty())
            {
                return null;
            }

            return DefDatabase<ThingDef>.GetNamedSilentFail(record.pinDefName);
        }

        public void SetPin(int thingId, ThingDef def)
        {
            DrillLimitRecord record = Record(thingId);
            record.pinDefName = def?.defName;
            DropEmpty(record);
        }

        public void ClearPins()
        {
            for (int i = records.Count - 1; i >= 0; i--)
            {
                records[i].pinDefName = null;
                DropEmpty(records[i]);
            }
        }

        private DrillLimitRecord Record(int thingId)
        {
            if (!byId.TryGetValue(thingId, out DrillLimitRecord record))
            {
                record = new DrillLimitRecord { thingId = thingId, limit = -1 };
                records.Add(record);
                byId[thingId] = record;
            }

            return record;
        }

        private void DropEmpty(DrillLimitRecord record)
        {
            if (record.limit != -1 || !record.pinDefName.NullOrEmpty() || !record.altDefName.NullOrEmpty())
            {
                return;
            }

            records.Remove(record);
            byId.Remove(record.thingId);
        }

        private void Rebuild()
        {
            byId.Clear();
            for (int i = 0; i < records.Count; i++)
            {
                DrillLimitRecord record = records[i];
                if (record != null)
                {
                    byId[record.thingId] = record;
                }
            }
        }
    }

    public class DrillLimitRecord : IExposable
    {
        public int thingId;
        public int limit = -1;
        public string pinDefName;
        public string altDefName;

        public void ExposeData()
        {
            Scribe_Values.Look(ref thingId, "thingId");
            Scribe_Values.Look(ref limit, "limit", -1);
            Scribe_Values.Look(ref pinDefName, "pinDefName");
            Scribe_Values.Look(ref altDefName, "altDefName");
        }
    }

    /// <summary>
    /// Resolves what a drill is extracting and what Set Limit counts.
    ///
    /// Определяет, что добывает буровая установка и что считает Set Limit.
    /// </summary>
    public static class DrillLimitState
    {
        public static int LimitOf(Thing drill)
        {
            DrillLimitGameComponent data = Data();
            return data == null || drill == null ? -1 : data.LimitOf(drill.thingIDNumber);
        }

        public static void SetLimit(Thing drill, int limit)
        {
            object billable = DrillLimitHsk.GetComp(drill);
            if (billable != null)
            {
                DrillLimitHsk.SetLimit(billable, limit);
                return;
            }

            Data()?.SetLimit(drill.thingIDNumber, limit);
        }

        public static void SetPin(Thing drill, ThingDef def)
        {
            Data()?.SetPin(drill.thingIDNumber, def);
        }

        public static void ClearPins()
        {
            Data()?.ClearPins();
        }

        /// <summary>
        /// Resource the deep drill extracts: HSK's current resource, or the scanned lump under
        /// the building.
        ///
        /// Ресурс, который добывает буровая установка: текущий ресурс HSK либо сканированная
        /// жила под постройкой.
        /// </summary>
        public static ThingDef DrilledResource(Thing drill, object billable)
        {
            if (billable != null)
            {
                return DrillLimitHsk.CurrentResource(billable);
            }

            if (drill?.Map == null)
            {
                return null;
            }

            return DeepDrillUtility.GetNextResource(drill.Position, drill.Map);
        }

        /// <summary>
        /// Def whose stacks the limit compares. A saved pin is kept only while it differs from
        /// the drilled resource and the any-resource option is on.
        ///
        /// Def, чьи стаки сравнивает лимит. Сохранённое закрепление держится только пока оно
        /// отличается от ресурса буровой установки и включён выбор любого ресурса.
        /// </summary>
        public static ThingDef EffectiveResource(Thing drill, object billable)
        {
            ThingDef drilled = DrilledResource(drill, billable);
            if (!KebabTweaksSettings.EnableDrillLimitAnyResource || drill == null)
            {
                return drilled;
            }

            DrillLimitGameComponent data = Data();
            ThingDef pin = data?.PinOf(drill.thingIDNumber);
            if (pin == null)
            {
                return drilled;
            }

            if (pin == drilled)
            {
                data.SetPin(drill.thingIDNumber, null);
                return drilled;
            }

            return pin;
        }

        public static List<Thing> DrillsSharing(Thing source, ThingDef counted)
        {
            List<Thing> group = new List<Thing>();
            if (source?.Map == null || counted == null)
            {
                if (source != null)
                {
                    group.Add(source);
                }

                return group;
            }

            foreach (Building building in source.Map.listerBuildings.AllBuildingsColonistOfDef(source.def))
            {
                object billable = DrillLimitHsk.GetComp(building);
                if (EffectiveResource(building, billable) == counted)
                {
                    group.Add(building);
                }
            }

            if (group.Count == 0)
            {
                group.Add(source);
            }

            return group;
        }

        private static DrillLimitGameComponent Data()
        {
            return Current.Game?.GetComponent<DrillLimitGameComponent>();
        }
    }

    /// <summary>
    /// CanTranslate is only the active language's own text, so a missing Russian key uses this
    /// mod's string instead of the English one.
    ///
    /// CanTranslate - только свой текст активного языка, поэтому отсутствующий русский ключ
    /// берётся из этого мода, а не из английской строки.
    /// </summary>
    public static class DrillLimitText
    {
        public static string SetLimitLabel()
        {
            if ("CSP_BillableDrillLimit".CanTranslate())
            {
                return "CSP_BillableDrillLimit".Translate();
            }

            return "KebabTweaks.DrillLimit.SetLimit".Translate();
        }

        public static string DrillResourceTitle()
        {
            string raw = "CSP_DrillResourceMode".CanTranslate()
                ? "CSP_DrillResourceMode".Translate()
                : "KebabTweaks.DrillLimit.DrillResource".Translate();
            return StripPlaceholder(raw);
        }

        public static string LimitButtonDesc(Thing drill, ThingDef counted)
        {
            return "KebabTweaks.DrillLimit.SetLimitDesc".Translate(
                (drill != null ? drill.LabelCap.ToString() : string.Empty).Named("0"),
                (counted != null ? counted.LabelCap.ToString() : string.Empty).Named("1"));
        }

        /// <summary>
        /// Inactive right-click line. The drill label is {0}, the limit number is {1}, and the
        /// counted item is {2}.
        ///
        /// Неактивная строка по ПКМ. {0} - подпись буровой установки, {1} - число лимита,
        /// {2} - считаемый предмет.
        /// </summary>
        public static string CannotDrill(Thing drill, int limit, ThingDef counted)
        {
            return "KebabTweaks.DrillLimit.CannotDrill".Translate(
                (drill != null ? drill.Label : string.Empty).Named("0"),
                limit.ToString().Named("1"),
                (counted != null ? counted.LabelCap.ToString() : string.Empty).Named("2"));
        }

        public static string PatchTooltip()
        {
            return "KebabTweaks.Patch.DrillLimit.Tooltip".Translate(
                SetLimitLabel().Named("0"),
                DrillResourceTitle().Named("1"));
        }

        private static string StripPlaceholder(string raw)
        {
            if (raw.NullOrEmpty())
            {
                return raw;
            }

            int token = raw.IndexOf("{0}", StringComparison.Ordinal);
            if (token >= 0)
            {
                raw = raw.Substring(0, token);
            }

            return raw.Trim().TrimEnd(':').Trim();
        }

        public static string CurrentLimitRaw()
        {
            string key = "CSP_CurrentLimit".CanTranslate()
                ? "CSP_CurrentLimit"
                : "KebabTweaks.DrillLimit.CurrentLimit";
            string raw = RawKeyed(LanguageDatabase.activeLanguage, key);
            if (raw != null)
            {
                return raw;
            }

            raw = RawKeyed(EnglishLanguage(), key);
            return raw ?? "Current Limit: {0}";
        }

        public static string LimitTooltip()
        {
            return "KebabTweaks.DrillLimit.CurrentLimitTooltip".Translate(
                "KebabTweaks.Patch.DrillLimit".Translate());
        }

        private static string RawKeyed(LoadedLanguage language, string key)
        {
            if (language == null || !language.TryGetTextFromKey(key, out TaggedString tagged))
            {
                return null;
            }

            string raw = tagged.RawText;
            if (raw.NullOrEmpty())
            {
                raw = tagged;
            }

            if (raw.NullOrEmpty() || string.Equals(raw, key, StringComparison.Ordinal))
            {
                return null;
            }

            return raw;
        }

        private static LoadedLanguage EnglishLanguage()
        {
            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (language?.folderName != null
                    && language.folderName.StartsWith("English", StringComparison.OrdinalIgnoreCase))
                {
                    return language;
                }
            }

            return LanguageDatabase.defaultLanguage;
        }
    }

    /// <summary>
    /// Things a ground stockpile filter can list: storable items, minified buildings, corpses.
    /// Labels are matched in the active language, English, and Russian.
    ///
    /// Вещи, которые может показать фильтр наземного склада: хранимые предметы, свёрнутые
    /// постройки, трупы. Подписи ищутся на активном языке, английском и русском.
    /// </summary>
    public static class DrillLimitCatalog
    {
        private static List<ThingDef> candidates;
        private static readonly Dictionary<int, string> Haystacks = new Dictionary<int, string>();
        private static string haystackLanguage;

        public static List<ThingDef> Matches(string query)
        {
            List<ThingDef> found = new List<ThingDef>();
            if (query.NullOrEmpty() || query.Trim().Length == 0)
            {
                return found;
            }

            string needle = query.Trim();
            EnsureCandidates();
            string languageName = LanguageDatabase.activeLanguage?.folderName ?? string.Empty;
            if (!string.Equals(haystackLanguage, languageName, StringComparison.Ordinal))
            {
                Haystacks.Clear();
                haystackLanguage = languageName;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                ThingDef def = candidates[i];
                if (Haystack(def).IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    found.Add(def);
                }
            }

            found.Sort((a, b) => string.Compare(a.LabelCap, b.LabelCap, StringComparison.CurrentCultureIgnoreCase));
            return found;
        }

        private static void EnsureCandidates()
        {
            if (candidates != null)
            {
                return;
            }

            candidates = new List<ThingDef>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (CanStoreInStockpile(def))
                {
                    candidates.Add(def);
                }
            }

            foreach (LoadedLanguage language in IndexedLanguages())
            {
                EnsureLanguageLoaded(language);
            }
        }

        /// <summary>
        /// Same test the stockpile filter uses for a row: EverStorable, including minified
        /// buildings, plus corpse defs which the filter lists through a special category.
        ///
        /// Та же проверка, что даёт строку фильтра склада: EverStorable, включая свёрнутые
        /// постройки, плюс def трупов, которые фильтр показывает отдельной категорией.
        /// </summary>
        private static bool CanStoreInStockpile(ThingDef def)
        {
            if (def == null || def.IsBlueprint || def.IsFrame || def.destroyOnDrop)
            {
                return false;
            }

            if (def.IsCorpse)
            {
                return true;
            }

            switch (def.category)
            {
                case ThingCategory.Item:
                case ThingCategory.Building:
                case ThingCategory.Plant:
                    return def.EverStorable(true);
                default:
                    return false;
            }
        }

        private static string Haystack(ThingDef def)
        {
            if (Haystacks.TryGetValue(def.index, out string cached))
            {
                return cached;
            }

            StringBuilder text = new StringBuilder();
            Append(text, def.label);
            Append(text, def.LabelCap);
            Append(text, ReplacedEnglish(def));
            foreach (LoadedLanguage language in IndexedLanguages())
            {
                Append(text, LabelIn(language, def));
            }
            string built = text.ToString();
            Haystacks[def.index] = built;
            return built;
        }

        private static void Append(StringBuilder text, string line)
        {
            if (line.NullOrEmpty())
            {
                return;
            }

            if (text.Length > 0)
            {
                text.Append('\n');
            }

            text.Append(line);
        }

        private static string LabelIn(LoadedLanguage language, ThingDef def)
        {
            if (language == null)
            {
                return null;
            }

            EnsureLanguageLoaded(language);
            DefInjectionPackage package = Package(language);
            if (package?.injections == null)
            {
                return null;
            }

            if (!package.injections.TryGetValue(def.defName + ".label", out DefInjectionPackage.DefInjection injection))
            {
                return null;
            }

            if (injection == null || injection.isPlaceholder || injection.injection.NullOrEmpty())
            {
                return null;
            }

            return injection.injection;
        }

        /// <summary>
        /// English source label kept on the active language's injection when that language
        /// replaced it. Used when the English language has no DefInjected row of its own.
        ///
        /// Исходная английская подпись, которую активный язык сохранил в injection при замене.
        /// Нужна, когда у английского языка нет своей строки DefInjected.
        /// </summary>
        private static string ReplacedEnglish(ThingDef def)
        {
            LoadedLanguage active = LanguageDatabase.activeLanguage;
            if (active == null || IsEnglish(active))
            {
                return null;
            }

            DefInjectionPackage package = Package(active);
            if (package?.injections == null
                || !package.injections.TryGetValue(def.defName + ".label", out DefInjectionPackage.DefInjection injection)
                || injection == null)
            {
                return null;
            }

            return injection.replacedString;
        }

        private static DefInjectionPackage Package(LoadedLanguage language)
        {
            List<DefInjectionPackage> packages = language.defInjections;
            if (packages == null)
            {
                return null;
            }

            for (int i = 0; i < packages.Count; i++)
            {
                if (packages[i] != null && packages[i].defType == typeof(ThingDef))
                {
                    return packages[i];
                }
            }

            return null;
        }

        private static void EnsureLanguageLoaded(LoadedLanguage language)
        {
            if (language == null)
            {
                return;
            }

            language.TryGetTextFromKey("CSP_CurrentLimit", out _);
        }

        private static List<LoadedLanguage> IndexedLanguages()
        {
            List<LoadedLanguage> languages = new List<LoadedLanguage>();
            LoadedLanguage active = LanguageDatabase.activeLanguage;
            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (language == null)
                {
                    continue;
                }

                if (ReferenceEquals(language, active) || IsEnglish(language) || IsRussian(language))
                {
                    languages.Add(language);
                }
            }

            return languages;
        }

        private static bool IsEnglish(LoadedLanguage language)
        {
            return StartsWith(language, "English");
        }

        private static bool IsRussian(LoadedLanguage language)
        {
            return StartsWith(language, "Russian");
        }

        private static bool StartsWith(LoadedLanguage language, string prefix)
        {
            if (language.folderName != null
                && language.folderName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string friendly = language.FriendlyNameEnglish;
            return friendly != null && friendly.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Set Limit window. The slider runs from -1 to the configured maximum.
    ///
    /// Окно Set Limit. Ползунок идёт от -1 до настроенного максимума.
    /// </summary>
    public class DrillLimitWindow : Window
    {
        private const float PanelWidth = 340f;
        private const float LabelRowHeight = 32f;
        private const float SliderHeight = 30f;
        private const float AfterLabelGap = 10f;
        private const float AfterSliderGap = 16f;
        private const float BetweenPickerGap = 8f;
        private const float ButtonHeight = 30f;
        private const float ButtonGap = 10f;
        private const float FieldHeight = 24f;
        private const float FieldWidth = 88f;
        private const float PickerRowHeight = 30f;
        private const float IconSize = 24f;
        private const string SearchControl = "KebabDrillLimitSearch";

        private readonly List<Thing> drills;
        private readonly ThingDef openedResource;
        private int curValue;
        private string limitBuffer;
        private string searchText = string.Empty;
        private string lastSearch = string.Empty;
        private ThingDef pendingPick;
        private bool listOpen;
        private Vector2 listScroll;
        private List<ThingDef> matches = new List<ThingDef>();
        private Rect panelRect;
        private Rect searchRect;
        private Rect pickerRect;
        private Rect listRect;

        public DrillLimitWindow(List<Thing> drills, int startingValue, ThingDef counted)
        {
            this.drills = drills ?? new List<Thing>();
            openedResource = counted;
            int max = SliderMax();
            curValue = Mathf.Clamp(startingValue, -1, max);
            limitBuffer = curValue.ToString();
            doWindowBackground = false;
            doCloseButton = false;
            doCloseX = false;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = false;
            closeOnCancel = true;
            closeOnAccept = false;
            forcePause = true;
            preventCameraMotion = true;
            draggable = false;
            resizeable = false;
            drawShadow = false;
        }

        public override Vector2 InitialSize => new Vector2(
            Mathf.Max(UI.screenWidth, PanelWidth),
            Mathf.Max(UI.screenHeight, 200f));

        public override void DoWindowContents(Rect inRect)
        {
            windowRect = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
            int max = SliderMax();
            curValue = Mathf.Clamp(curValue, -1, max);
            bool anyResource = KebabTweaksSettings.EnableDrillLimitAnyResource;
            bool valueField = KebabTweaksSettings.EnableDrillLimitValueField;
            float contentHeight = LabelRowHeight + AfterLabelGap + SliderHeight + AfterSliderGap + ButtonHeight;
            if (anyResource)
            {
                contentHeight += PickerRowHeight + BetweenPickerGap + FieldHeight + AfterSliderGap;
            }

            float panelHeight = contentHeight + 24f;

            panelRect = new Rect(
                inRect.x + (inRect.width - PanelWidth) / 2f,
                inRect.y + (inRect.height - panelHeight) / 2f,
                PanelWidth,
                panelHeight);
            Widgets.DrawWindowBackground(panelRect);

            Rect inner = panelRect.ContractedBy(12f);
            Rect labelRow = new Rect(inner.x, inner.y, inner.width, LabelRowHeight);
            bool coverList = MouseOverOpenList();
            EventType coveredType = EventType.Ignore;
            if (coverList)
            {
                coveredType = Event.current.type;
                Event.current.type = EventType.Ignore;
            }

            DrawLimitRow(labelRow, valueField);
            if (coverList)
            {
                Event.current.type = coveredType;
            }

            Rect sliderRect = new Rect(inner.x, labelRow.yMax + AfterLabelGap, inner.width, SliderHeight);
            int before = curValue;
            curValue = ReadSlider(sliderRect, max);
            if (curValue != before)
            {
                limitBuffer = curValue.ToString();
            }

            DrawEndLabels(sliderRect, max);

            float y = sliderRect.yMax + AfterSliderGap;
            if (anyResource)
            {
                pickerRect = new Rect(inner.x, y, inner.width, PickerRowHeight);
                DrawPicker(pickerRect);
                y = pickerRect.yMax + BetweenPickerGap;
                searchRect = new Rect(inner.x, y, inner.width, FieldHeight);
                DrawSearch(searchRect);
                y = searchRect.yMax + AfterSliderGap;
            }
            else
            {
                pickerRect = default;
                searchRect = default;
                listOpen = false;
            }

            DrawButtons(inner, y);
            HandleDismissInput();
            if (listOpen)
            {
                DrawMatchList();
            }
        }

        private void DrawLimitRow(Rect row, bool valueField)
        {
            string template = DrillLimitText.CurrentLimitRaw();
            int token = template.IndexOf("{0}", StringComparison.Ordinal);
            string before = token >= 0 ? template.Substring(0, token) : template;
            string after = token >= 0 ? template.Substring(token + 3) : string.Empty;
            if (valueField)
            {
                float beforeWidth = Text.CalcSize(before).x;
                float afterWidth = Text.CalcSize(after).x;
                float total = beforeWidth + FieldWidth + afterWidth;
                float x = row.x + Mathf.Max(0f, (row.width - total) / 2f);
                Rect beforeRect = new Rect(x, row.y, beforeWidth, row.height);
                Rect fieldRect = new Rect(beforeRect.xMax, row.y + (row.height - FieldHeight) / 2f, FieldWidth, FieldHeight);
                Rect afterRect = new Rect(fieldRect.xMax, row.y, afterWidth, row.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(beforeRect, before);
                Widgets.TextFieldNumeric(fieldRect, ref curValue, ref limitBuffer, -1f, SliderMax());
                Widgets.Label(afterRect, after);
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(row, template.Replace("{0}", curValue.ToString()));
                Text.Anchor = TextAnchor.UpperLeft;
            }

            TooltipHandler.TipRegion(row, DrillLimitText.LimitTooltip());
        }

        /// <summary>
        /// While the match list is open, mouse events are hidden from the slider, and from the
        /// current-limit field when a row covers it, so the row click selects the row.
        ///
        /// Пока список совпадений открыт, события мыши скрыты от ползунка и от поля текущего
        /// лимита, если строка его накрывает, чтобы клик выбрал строку.
        /// </summary>
        private int ReadSlider(Rect sliderRect, int max)
        {
            Event current = Event.current;
            bool block = listOpen
                && current != null
                && (current.type == EventType.MouseDown
                    || current.type == EventType.MouseUp
                    || current.type == EventType.MouseDrag);
            EventType saved = EventType.Ignore;
            if (block)
            {
                saved = current.type;
                current.type = EventType.Ignore;
            }

            int value = (int)Widgets.HorizontalSlider(sliderRect, curValue, -1f, max, false, null, null, null, 1f);
            if (block)
            {
                current.type = saved;
            }

            return value;
        }

        private bool MouseOverOpenList()
        {
            Event current = Event.current;
            if (!listOpen || current == null || listRect.width <= 0f)
            {
                return false;
            }

            if (current.type != EventType.MouseDown
                && current.type != EventType.MouseUp
                && current.type != EventType.MouseDrag)
            {
                return false;
            }

            return listRect.Contains(current.mousePosition);
        }

        private void DrawEndLabels(Rect sliderRect, int max)
        {
            Color previous = GUI.color;
            GUI.color = ColoredText.SubtleGrayColor;
            Text.Font = GameFont.Tiny;
            float labelY = sliderRect.yMax - 10f;
            float height = Text.LineHeight;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(new Rect(sliderRect.x, labelY, sliderRect.width / 2f, height), "-1");
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(
                new Rect(sliderRect.x + sliderRect.width / 2f, labelY, sliderRect.width / 2f, height),
                max.ToString());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = previous;
        }

        private void DrawPicker(Rect rect)
        {
            ThingDef shown = pendingPick ?? openedResource;
            Widgets.DrawMenuSection(rect);
            if (shown != null)
            {
                Rect iconRect = new Rect(rect.x + 4f, rect.y + (rect.height - IconSize) / 2f, IconSize, IconSize);
                Widgets.DefIcon(iconRect, shown);
                Rect labelRect = new Rect(iconRect.xMax + 6f, rect.y, rect.width - IconSize - 16f, rect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, shown.LabelCap);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            if (Widgets.ButtonInvisible(rect) && !searchText.NullOrEmpty() && matches.Count > 0)
            {
                listOpen = !listOpen;
            }
        }

        private void DrawSearch(Rect rect)
        {
            GUI.SetNextControlName(SearchControl);
            string typed = Widgets.TextField(rect, searchText);
            if (!string.Equals(typed, searchText, StringComparison.Ordinal))
            {
                searchText = typed;
            }

            if (!string.Equals(searchText, lastSearch, StringComparison.Ordinal))
            {
                lastSearch = searchText;
                matches = DrillLimitCatalog.Matches(searchText);
                listOpen = matches.Count > 0;
            }
        }

        private void DrawButtons(Rect inner, float y)
        {
            float width = (inner.width - ButtonGap) / 2f;
            Rect cancel = new Rect(inner.x, y, width, ButtonHeight);
            Rect ok = new Rect(cancel.xMax + ButtonGap, y, width, ButtonHeight);
            if (Widgets.ButtonText(cancel, "CancelButton".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(ok, "OK".Translate()))
            {
                ApplyAndClose();
            }
        }

        /// <summary>
        /// Match list grows upward from the picker and may extend past the right of the screen.
        /// A click outside the list and the search field, or a key while that field is not
        /// focused, hides it.
        ///
        /// Список совпадений растёт вверх от кнопки и может выйти за правый край экрана.
        /// Клик вне списка и поля поиска или клавиша, когда поле не в фокусе, скрывает его.
        /// </summary>
        private void DrawMatchList()
        {
            if (matches.Count == 0 || searchText.NullOrEmpty())
            {
                listOpen = false;
                return;
            }

            float width = pickerRect.width;
            for (int i = 0; i < matches.Count; i++)
            {
                float labelWidth = Text.CalcSize(matches[i].LabelCap).x + IconSize + 20f;
                if (labelWidth > width)
                {
                    width = labelWidth;
                }
            }

            float contentHeight = matches.Count * PickerRowHeight;
            float available = Mathf.Max(PickerRowHeight, pickerRect.y - 4f);
            float viewHeight = Mathf.Min(contentHeight, available);
            listRect = new Rect(pickerRect.x, pickerRect.y - viewHeight, width, viewHeight);
            Widgets.DrawWindowBackground(listRect);
            Rect view = listRect.ContractedBy(2f);
            Rect content = new Rect(0f, 0f, view.width - 16f, contentHeight);
            Widgets.BeginScrollView(view, ref listScroll, content);
            float visibleTop = listScroll.y - PickerRowHeight;
            float visibleBottom = listScroll.y + view.height + PickerRowHeight;
            for (int i = 0; i < matches.Count; i++)
            {
                Rect row = new Rect(0f, i * PickerRowHeight, content.width, PickerRowHeight);
                if (row.yMax < visibleTop || row.y > visibleBottom)
                {
                    continue;
                }

                if (Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                ThingDef def = matches[i];
                Rect iconRect = new Rect(row.x + 4f, row.y + (row.height - IconSize) / 2f, IconSize, IconSize);
                Widgets.DefIcon(iconRect, def);
                Rect labelRect = new Rect(iconRect.xMax + 6f, row.y, row.width - IconSize - 14f, row.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, def.LabelCap);
                Text.Anchor = TextAnchor.UpperLeft;
                if (Widgets.ButtonInvisible(row))
                {
                    pendingPick = def;
                    listOpen = false;
                    SoundDefOf.Click.PlayOneShotOnCamera();
                }
            }

            Widgets.EndScrollView();
        }

        private void HandleDismissInput()
        {
            Event current = Event.current;
            if (current == null)
            {
                return;
            }

            if (current.type == EventType.KeyDown && listOpen)
            {
                if (GUI.GetNameOfFocusedControl() != SearchControl)
                {
                    listOpen = false;
                }
            }

            if (current.type != EventType.MouseDown)
            {
                return;
            }

            Vector2 mouse = current.mousePosition;
            bool onList = listOpen && listRect.Contains(mouse);
            bool onSearch = searchRect.width > 0f && searchRect.Contains(mouse);
            bool onPicker = pickerRect.width > 0f && pickerRect.Contains(mouse);
            if (listOpen && !onList && !onSearch && !onPicker)
            {
                listOpen = false;
            }

            if (!panelRect.Contains(mouse) && !onList)
            {
                Close();
                current.Use();
            }
        }

        private void ApplyAndClose()
        {
            int max = SliderMax();
            int value = Mathf.Clamp(curValue, -1, max);
            for (int i = 0; i < drills.Count; i++)
            {
                Thing drill = drills[i];
                if (drill == null || drill.Destroyed)
                {
                    continue;
                }

                DrillLimitState.SetLimit(drill, value);
                if (!KebabTweaksSettings.EnableDrillLimitAnyResource || pendingPick == null)
                {
                    continue;
                }

                object billable = DrillLimitHsk.GetComp(drill);
                ThingDef drilled = DrillLimitState.DrilledResource(drill, billable);
                if (pendingPick == drilled)
                {
                    DrillLimitState.SetPin(drill, null);
                }
                else
                {
                    DrillLimitState.SetPin(drill, pendingPick);
                }
            }

            Close();
        }

        private static int SliderMax()
        {
            return Mathf.Clamp(KebabTweaksSettings.DrillLimitMax, 0, KebabTweaksSettings.MaxDrillLimitMax);
        }
    }
}
