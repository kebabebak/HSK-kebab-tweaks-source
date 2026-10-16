using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Reads HaulToBuilding ExtraBillData by name so this assembly does not reference that DLL.
    /// Instance and TakeFrom are fields. TakeFromText is HaulToBuilding.Utils.TakeFromText.
    ///
    /// Читает ExtraBillData из HaulToBuilding по имени, без ссылки на ту DLL.
    /// Instance и TakeFrom там поля. TakeFromText - это HaulToBuilding.Utils.TakeFromText.
    /// </summary>
    static class ExtraBillDataAccess
    {
        const string ComponentTypeName = "HaulToBuilding.GameComponent_ExtraBillData";
        const string DataTypeName = "HaulToBuilding.ExtraBillData";

        static bool resolved;
        static Type componentType;
        static Type dataType;
        static MemberInfo instanceMember;
        static MethodInfo getDataMethod;
        static FieldInfo dataField;
        static MemberInfo takeFromMember;
        static MethodInfo takeFromTextMethod;

        public static bool IsLoaded
        {
            get
            {
                Ensure();
                return componentType != null && dataType != null;
            }
        }

        public static Type ComponentType
        {
            get
            {
                Ensure();
                return componentType;
            }
        }

        public static object Instance
        {
            get
            {
                Ensure();
                if (instanceMember == null)
                {
                    return null;
                }

                return instanceMember is PropertyInfo property
                    ? property.GetValue(null)
                    : ((FieldInfo)instanceMember).GetValue(null);
            }
        }

        public static object GetData(object component, Bill bill)
        {
            Ensure();
            if (component == null || getDataMethod == null)
            {
                return null;
            }

            return getDataMethod.Invoke(component, new object[] { bill, false });
        }

        public static IDictionary BillDictionary(object component)
        {
            Ensure();
            if (component == null || dataField == null)
            {
                return null;
            }

            return dataField.GetValue(component) as IDictionary;
        }

        public static bool TakeFromHasAny(object data)
        {
            IEnumerable items = ReadTakeFrom(data);
            if (items == null)
            {
                return false;
            }

            foreach (object unused in items)
            {
                return true;
            }

            return false;
        }

        public static int TakeFromCount(object data)
        {
            IEnumerable items = ReadTakeFrom(data);
            if (items is ICollection collection)
            {
                return collection.Count;
            }

            if (items == null)
            {
                return 0;
            }

            int count = 0;
            foreach (object unused in items)
            {
                count++;
            }

            return count;
        }

        public static string TakeFromText(object data)
        {
            Ensure();
            if (data == null || takeFromTextMethod == null)
            {
                return null;
            }

            object value = takeFromTextMethod.IsStatic
                ? takeFromTextMethod.Invoke(null, new[] { data })
                : takeFromTextMethod.Invoke(data, null);
            return value as string;
        }

        public static List<ISlotGroupParent> CopyTakeFromParents(object data)
        {
            IEnumerable items = ReadTakeFrom(data);
            var parents = new List<ISlotGroupParent>();
            if (items == null)
            {
                return parents;
            }

            foreach (object item in items)
            {
                if (item is ISlotGroupParent parent)
                {
                    parents.Add(parent);
                }
            }

            return parents;
        }

        static IEnumerable ReadTakeFrom(object data)
        {
            Ensure();
            if (data == null || takeFromMember == null)
            {
                return null;
            }

            object value = takeFromMember is PropertyInfo property
                ? property.GetValue(data)
                : ((FieldInfo)takeFromMember).GetValue(data);
            return value as IEnumerable;
        }

        static void Ensure()
        {
            if (resolved)
            {
                return;
            }

            resolved = true;
            componentType = AccessTools.TypeByName(ComponentTypeName);
            dataType = AccessTools.TypeByName(DataTypeName);
            if (componentType == null)
            {
                return;
            }

            instanceMember = AccessTools.Property(componentType, "Instance")
                ?? (MemberInfo)AccessTools.Field(componentType, "Instance");
            getDataMethod = AccessTools.Method(componentType, "GetData", new[] { typeof(Bill), typeof(bool) });
            dataField = AccessTools.Field(componentType, "data");
            if (dataType == null)
            {
                return;
            }

            takeFromMember = AccessTools.Property(dataType, "TakeFrom")
                ?? (MemberInfo)AccessTools.Field(dataType, "TakeFrom");
            takeFromTextMethod = AccessTools.Method(dataType, "TakeFromText");
            if (takeFromTextMethod == null)
            {
                Type utilsType = AccessTools.TypeByName("HaulToBuilding.Utils");
                if (utilsType != null)
                {
                    takeFromTextMethod = AccessTools.Method(utilsType, "TakeFromText", new[] { dataType });
                }
            }
        }
    }

    /// <summary>
    /// Reads Mending mod settings by name so this assembly does not reference Mending.dll.
    ///
    /// Читает настройки Mending по имени, без ссылки на Mending.dll.
    /// </summary>
    static class MendingSettingsAccess
    {
        static bool resolved;
        static MemberInfo progressiveMending;
        static MemberInfo pawnFreeSkillBonus;

        public static bool ProgressiveMending
        {
            get
            {
                object value = Read(progressiveMending);
                return value is bool flag && flag;
            }
        }

        public static int PawnFreeSkillBonus
        {
            get
            {
                object value = Read(pawnFreeSkillBonus);
                return value is int bonus ? bonus : 0;
            }
        }

        static object Read(MemberInfo member)
        {
            Ensure();
            if (member == null)
            {
                return null;
            }

            return member is PropertyInfo property
                ? property.GetValue(null)
                : ((FieldInfo)member).GetValue(null);
        }

        static void Ensure()
        {
            if (resolved)
            {
                return;
            }

            resolved = true;
            Type settingsType = AccessTools.TypeByName("Mending.Mending_Settings");
            if (settingsType == null)
            {
                return;
            }

            progressiveMending = AccessTools.Property(settingsType, "progressiveMending")
                ?? (MemberInfo)AccessTools.Field(settingsType, "progressiveMending");
            pawnFreeSkillBonus = AccessTools.Property(settingsType, "pawnFreeSkillBonus")
                ?? (MemberInfo)AccessTools.Field(settingsType, "pawnFreeSkillBonus");
        }
    }

    public static class TakeFromMendingModCompatibility
    {
        private const string MendAndRecyclePackageId = "notfood.MendAndRecycle";

        public static bool IsMendAndRecycleLoaded()
        {
            return IsPackageActive(MendAndRecyclePackageId);
        }

        private static bool IsPackageActive(string packageId)
        {
            if (ModsConfig.IsActive(packageId))
            {
                return true;
            }

            return LoadedModManager.RunningModsListForReading.Exists(
                mod => string.Equals(mod.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Problem: the "Take It To Storage!" mod (HaulToBuilding) adds a per-bill "Take from"
    /// stockpile list (ExtraBillData.TakeFrom). It patches vanilla
    /// WorkGiver_DoBill.TryFindBestBillIngredients for crafting, but Core SK MendAndRecycle
    /// replaces mend/recycle with Mending.WorkGiver_DoBill, which searches the whole map via
    /// ClosestThingReachable and never reads TakeFrom. The same symptom can hit ordinary bills
    /// when ExtraBillData is keyed to a different Bill instance than the job (e.g. Better
    /// Workbench Management linked/mirrored bills): UI shows TakeFrom, pawns still haul from the
    /// nearest stockpile.
    ///
    /// Fix: Harmony prefixes (plus a vanilla postfix safety net) intercept ingredient search
    /// before the unconstrained path. When TakeFrom is non-empty, ExtraBillData is resolved for
    /// the active bill (direct lookup, BillStack siblings, same workbench, or same recipe on the
    /// map), candidates come only from selected storages, validated like vanilla. Mend bills use
    /// Mending-specific ingredient rules. Postfix re-runs constrained search if another mod
    /// already returned disallowed storages. Prefix skip is required to intercept before
    /// HaulToBuilding / MendAndRecycle run unconstrained search when TakeFrom is set.
    /// ExtraBillData cleanup: postfix on GameComponent_ExtraBillData.ExposeData prunes null/dead
    /// bill keys that otherwise spam load errors after deleted bills deserialize.
    /// HaulToBuilding and Mending types are resolved by name, so this assembly does not
    /// reference those DLLs and skips the feature when Take It To Storage! is absent.
    ///
    /// Проблема: мод "Take It To Storage!" (HaulToBuilding) добавляет per-bill список "Take from"
    /// (ExtraBillData.TakeFrom). Патчит vanilla TryFindBestBillIngredients для крафта, но Core SK
    /// MendAndRecycle подменяет mend/recycle на Mending.WorkGiver_DoBill, который ищет по всей
    /// карте через ClosestThingReachable и не читает TakeFrom. Тот же симптом бывает на обычных
    /// биллях, если ExtraBillData привязан к другому Bill, чем job (например linked/mirrored bills
    /// Better Workbench Management): в UI TakeFrom настроен, пешки всё равно берут с ближайшего
    /// склада.
    ///
    /// Исправление: Harmony Prefix (плюс Postfix-страховка на vanilla) перехватывают поиск
    /// ингредиентов до неограниченного пути. При непустом TakeFrom ExtraBillData резолвится для
    /// активного bill (прямой lookup, сиблинги BillStack, тот же верстак или тот же recipe на
    /// карте); кандидаты только из выбранных складов. Mend использует правила Mending. Postfix
    /// повторяет ограниченный поиск, если другой мод уже вернул запрещённые склады. Prefix skip
    /// нужен, чтобы перехватить до HaulToBuilding / MendAndRecycle при установленном TakeFrom.
    /// Очистка ExtraBillData: Postfix GameComponent_ExtraBillData.ExposeData удаляет null/мёртвые
    /// ключи bill, иначе после удалённых билл спамят ошибки load.
    /// Типы HaulToBuilding и Mending ищутся по имени, сборка на эти DLL не ссылается
    /// и пропускает фикс, если нет Take It To Storage!.
    /// </summary>
    public static class TakeFromMendingFeatures
    {
        public static void Apply(Harmony harmony)
        {
            if (!ExtraBillDataAccess.IsLoaded)
            {
                FeatureAvailability.SkipMissingMods(
                    FeatureAvailability.TakeFromMending,
                    "Take From Mending",
                    "Take It To Storage! (legodude17.htsb)");
                return;
            }

            try
            {
                // Explicit types only - never PatchAll(GetExecutingAssembly()) in the combined mod.
                harmony.CreateClassProcessor(typeof(VanillaTryFindBestBillIngredientsPatch)).Patch();
                harmony.CreateClassProcessor(typeof(MendingTryFindBestBillIngredientsPatch)).Patch();
                harmony.CreateClassProcessor(typeof(ExtraBillDataExposeDataPatch)).Patch();
                Log.Message(
                    $"[TakeFromMendingPatch] Loaded (verbose logging {(KebabTweaksSettings.TakeFromMendingEnableLogging ? "ON" : "OFF")}). " +
                    "Enable logging in mod settings for ingredient-search details.");
            }
            catch (Exception ex)
            {
                Log.Error("[TakeFromMendingPatch] Failed to apply patches: " + ex);
            }
        }
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.First)]
    [HarmonyBefore("legodude17.HaulToBuilding")]
    internal static class VanillaTryFindBestBillIngredientsPatch
    {
        private static MethodBase targetMethod;

        private static readonly Type[] TargetParameterTypes =
        {
            typeof(Bill),
            typeof(Pawn),
            typeof(Thing),
            typeof(List<ThingCount>),
            typeof(List<IngredientCount>)
        };

        private static bool Prepare()
        {
            targetMethod = AccessTools.Method(
                typeof(RimWorld.WorkGiver_DoBill),
                "TryFindBestBillIngredients",
                TargetParameterTypes);
            if (targetMethod != null)
            {
                return true;
            }

            Log.ErrorOnce(
                "[TakeFromMendingPatch] Could not find RimWorld.WorkGiver_DoBill.TryFindBestBillIngredients.",
                0x5a2b7c11);
            return false;
        }

        private static MethodBase TargetMethod()
        {
            return targetMethod;
        }

        public static bool Prefix(
            Bill bill,
            Pawn pawn,
            Thing billGiver,
            List<ThingCount> chosen,
            List<IngredientCount> missingIngredients,
            ref bool __result)
        {
            if (!KebabTweaksSettings.EnableTakeFromMending)
            {
                return true;
            }

            return TakeFromMendingPatchLogic.VanillaPrefix(
                bill,
                pawn,
                billGiver,
                chosen,
                missingIngredients,
                ref __result);
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(
            Bill bill,
            Pawn pawn,
            Thing billGiver,
            List<ThingCount> chosen,
            List<IngredientCount> missingIngredients,
            ref bool __result)
        {
            if (!KebabTweaksSettings.EnableTakeFromMending)
            {
                return;
            }

            TakeFromMendingPatchLogic.VanillaPostfix(
                bill,
                pawn,
                billGiver,
                chosen,
                missingIngredients,
                ref __result);
        }
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.First)]
    internal static class MendingTryFindBestBillIngredientsPatch
    {
        private static MethodBase targetMethod;

        private static bool Prepare()
        {
            if (!TakeFromMendingModCompatibility.IsMendAndRecycleLoaded())
            {
                return false;
            }

            Type mendingGiver = AccessTools.TypeByName("Mending.WorkGiver_DoBill");
            if (mendingGiver == null)
            {
                return false;
            }

            targetMethod = AccessTools.Method(
                mendingGiver,
                "TryFindBestBillIngredients",
                new[]
                {
                    typeof(Bill),
                    typeof(Pawn),
                    typeof(Thing),
                    typeof(bool),
                    typeof(Thing).MakeByRefType()
                });

            if (targetMethod != null)
            {
                return true;
            }

            Log.WarningOnce(
                "[TakeFromMendingPatch] MendAndRecycle is loaded but Mending.WorkGiver_DoBill.TryFindBestBillIngredients was not found.",
                unchecked((int)0x8c41f903));
            return false;
        }

        private static MethodBase TargetMethod()
        {
            return targetMethod;
        }

        public static bool Prefix(
            Bill bill,
            Pawn pawn,
            Thing billGiver,
            bool ignoreHitPoints,
            ref Thing chosen,
            ref bool __result)
        {
            if (!KebabTweaksSettings.EnableTakeFromMending)
            {
                return true;
            }

            return TakeFromMendingPatchLogic.MendingPrefix(
                bill,
                pawn,
                billGiver,
                ignoreHitPoints,
                ref chosen,
                ref __result);
        }
    }

    [HarmonyPatch]
    internal static class ExtraBillDataExposeDataPatch
    {
        static MethodBase targetMethod;

        static bool Prepare()
        {
            Type componentType = ExtraBillDataAccess.ComponentType;
            if (componentType == null)
            {
                return false;
            }

            targetMethod = AccessTools.Method(componentType, "ExposeData");
            return targetMethod != null;
        }

        static MethodBase TargetMethod()
        {
            return targetMethod;
        }

        public static void Postfix(object __instance)
        {
            if (!KebabTweaksSettings.EnableTakeFromMending)
            {
                return;
            }

            if (__instance == null)
            {
                return;
            }

            string route = Scribe.mode == LoadSaveMode.LoadingVars ? "load" : "save";
            ExtraBillDataMaintenance.PruneStaleBillDataKeys(__instance, route);
        }
    }

    internal static class ExtraBillDataMaintenance
    {
        internal static IDictionary GetBillDataDictionary(object component)
        {
            return ExtraBillDataAccess.BillDictionary(component);
        }

        internal static bool IsBillDataKeyAlive(Bill bill)
        {
            if (bill == null)
            {
                return false;
            }

            try
            {
                return !bill.DeletedOrDereferenced;
            }
            catch (Exception ex)
            {
                TakeFromMendingPatchLog.Warning($"[TakeFromMendingPatch] Stale ExtraBillData bill key skipped: {ex.Message}");
                return false;
            }
        }

        internal static bool BillRecipesMatch(Bill left, Bill right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            try
            {
                return left.recipe == right.recipe;
            }
            catch (Exception ex)
            {
                TakeFromMendingPatchLog.Warning($"[TakeFromMendingPatch] Stale bill recipe compare skipped: {ex.Message}");
                return false;
            }
        }

        internal static int PruneStaleBillDataKeys(object component, string route)
        {
            IDictionary dict = ExtraBillDataMaintenance.GetBillDataDictionary(component);
            if (dict == null || dict.Count == 0)
            {
                return 0;
            }

            List<Bill> staleKeys = null;
            int nullKeys = 0;
            var keys = new List<Bill>();
            foreach (DictionaryEntry entry in dict)
            {
                keys.Add(entry.Key as Bill);
            }

            foreach (Bill key in keys)
            {
                if (key == null)
                {
                    nullKeys++;
                    staleKeys ??= new List<Bill>();
                    staleKeys.Add(null);
                    continue;
                }

                if (IsBillDataKeyAlive(key))
                {
                    continue;
                }

                staleKeys ??= new List<Bill>();
                staleKeys.Add(key);
            }

            if (staleKeys == null)
            {
                return 0;
            }

            for (int i = 0; i < staleKeys.Count; i++)
            {
                dict.Remove(staleKeys[i]);
            }

            string nullSuffix = nullKeys > 0 ? $", nullKeys={nullKeys}" : string.Empty;
            string message =
                $"[TakeFromMendingPatch] Pruned {staleKeys.Count} stale ExtraBillData bill key(s) during {route} " +
                $"(dictionary now {dict.Count} entries{nullSuffix}).";
            TakeFromMendingPatchLog.Maintenance(message);
            return staleKeys.Count;
        }
    }

    internal static class TakeFromMendingPatchLogic
    {

        // When TakeFrom is set, search only selected storages; otherwise defer to vanilla + HaulToBuilding.
        internal static bool VanillaPrefix(
            Bill bill,
            Pawn pawn,
            Thing billGiver,
            List<ThingCount> chosen,
            List<IngredientCount> missingIngredients,
            ref bool __result)
        {
            TakeFromMendingPatchLog.Message(
                $"[TakeFromMendingPatch] Vanilla TryFindBestBillIngredients enter: {DescribeBill(bill, pawn, billGiver)}");
            if (!TryResolveTakeFromParents("Vanilla", bill, pawn, billGiver, out var parents, out var resolution))
            {
                return true;
            }
            return RunVanillaConstrainedSearch(
                bill,
                pawn,
                billGiver,
                chosen,
                missingIngredients,
                parents,
                resolution,
                ref __result);
        }
        // Safety net: if another mod picked ingredients from the wrong storage, rerun a constrained search.
        internal static void VanillaPostfix(
            Bill bill,
            Pawn pawn,
            Thing billGiver,
            List<ThingCount> chosen,
            List<IngredientCount> missingIngredients,
            ref bool __result)
        {
            if (!TryResolveTakeFromParents("Vanilla-postfix", bill, pawn, billGiver, out var parents, out var resolution))
            {
                if (KebabTweaksSettings.TakeFromMendingEnableLogging)
                {
                    TakeFromMendingPatchLog.Message(
                        $"[TakeFromMendingPatch] Vanilla postfix: no TakeFrom constraint. result={__result}, chosen={DescribeThingCounts(chosen)}");
                }
                return;
            }
            if (__result && chosen != null && chosen.Count > 0 && !ChosenViolatesTakeFrom(chosen, parents))
            {
                TakeFromMendingPatchLog.Message(
                    $"[TakeFromMendingPatch] Vanilla postfix OK ({resolution}): chosen={DescribeThingCounts(chosen)}");
                return;
            }
            if (__result && chosen != null && chosen.Count > 0)
            {
                TakeFromMendingPatchLog.Warning(
                    $"[TakeFromMendingPatch] Vanilla postfix: ingredients from disallowed storage ({resolution}). " +
                    $"chosen={DescribeThingCounts(chosen)}, allowed={DescribeParents(parents)}. Re-searching constrained.");
            }
            else if (!__result)
            {
                TakeFromMendingPatchLog.Message(
                    $"[TakeFromMendingPatch] Vanilla postfix: original search failed ({resolution}), trying constrained search.");
            }
            RunVanillaConstrainedSearch(
                bill,
                pawn,
                billGiver,
                chosen,
                missingIngredients,
                parents,
                resolution + "/postfix",
                ref __result);
        }
        // Gather candidates only from TakeFrom storages, then call vanilla TryFindBestBillIngredientsInSet.
        private static bool RunVanillaConstrainedSearch(
            Bill bill,
            Pawn pawn,
            Thing billGiver,
            List<ThingCount> chosen,
            List<IngredientCount> missingIngredients,
            List<ISlotGroupParent> parents,
            string resolution,
            ref bool __result)
        {
            var rootCell = GetBillGiverRootCell(billGiver, pawn);
            if (!IsRootRegionValid("Vanilla", pawn, bill, billGiver, rootCell))
            {
                chosen?.Clear();
                missingIngredients?.Clear();
                __result = false;
                return false;
            }
            LogStockpileContext(pawn, parents, resolution);
            var candidates = GatherCandidateThings(parents, pawn.Map).Distinct().ToList();
            TakeFromMendingPatchLog.Message(
                $"[TakeFromMendingPatch] Vanilla constrained ({resolution}): {DescribeBill(bill, pawn, billGiver)}, " +
                $"candidates={candidates.Count}, selected={DescribeParents(parents)}");
            var filtered = new List<Thing>();
            foreach (var thing in candidates)
            {
                if (IsValidVanillaIngredient(thing, bill, pawn, billGiver, out var reason))
                {
                    filtered.Add(thing);
                    TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] Vanilla candidate OK: {DescribeThing(thing)}");
                }
                else
                {
                    TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] Vanilla candidate rejected: {DescribeThing(thing)}; reason={reason}");
                }
            }
            chosen?.Clear();
            missingIngredients?.Clear();
            __result = VanillaTryFindBestBillIngredientsInSet(
                filtered,
                bill,
                chosen,
                rootCell,
                alreadySorted: false,
                missingIngredients);
            TakeFromMendingPatchLog.Message(
                $"[TakeFromMendingPatch] Vanilla result ({resolution})={__result}, chosen={DescribeThingCounts(chosen)}, " +
                $"missing={DescribeMissingIngredients(missingIngredients)}");
            return false;
        }
        internal static bool MendingPrefix(
            Bill bill,
            Pawn pawn,
            Thing billGiver,
            bool ignoreHitPoints,
            ref Thing chosen,
            ref bool __result)
        {
            TakeFromMendingPatchLog.Message(
                $"[TakeFromMendingPatch] Mending TryFindBestBillIngredients enter: {DescribeBill(bill, pawn, billGiver)}, ignoreHitPoints={ignoreHitPoints}");
            if (!TryResolveTakeFromParents("Mending", bill, pawn, billGiver, out var parents, out var resolution))
            {
                return true;
            }
            var rootCell = GetBillGiverRootCell(billGiver, pawn);
            if (!IsRootRegionValid("Mending", pawn, bill, billGiver, rootCell))
            {
                chosen = null;
                __result = false;
                return false;
            }
            LogStockpileContext(pawn, parents, resolution);
            var candidates = GatherCandidateThings(parents, pawn.Map).Distinct().ToList();
            TakeFromMendingPatchLog.Message(
                $"[TakeFromMendingPatch] Mending constrained ({resolution}): {DescribeBill(bill, pawn, billGiver)}, " +
                $"candidates={candidates.Count}, selected={DescribeParents(parents)}");
            if (candidates.Count == 0)
            {
                TakeFromMendingPatchLog.Message("[TakeFromMendingPatch] Mending result=false because selected storages have no things.");
                chosen = null;
                __result = false;
                return false;
            }
            float skillNeeded = -1f;
            Thing repairable = null;
            float repairLimit = GetRepairLimit(bill, pawn);
            bool Validator(Thing thing)
            {
                bool valid = IsValidMendingIngredient(
                    thing,
                    bill,
                    pawn,
                    ignoreHitPoints,
                    repairLimit,
                    ref skillNeeded,
                    ref repairable,
                    out var reason);
                TakeFromMendingPatchLog.Message(
                    valid
                        ? $"[TakeFromMendingPatch] Mending candidate OK: {DescribeThing(thing)}"
                        : $"[TakeFromMendingPatch] Mending candidate rejected: {DescribeThing(thing)}; reason={reason}");
                return valid;
            }
            chosen = GenClosest.ClosestThingReachable(
                rootCell,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
                PathEndMode.ClosestTouch,
                TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, canBashDoors: false, alwaysUseAvoidGrid: false, canBashFences: false),
                bill.ingredientSearchRadius,
                Validator,
                candidates,
                searchRegionsMin: 0,
                searchRegionsMax: -1,
                forceAllowGlobalSearch: false,
                traversableRegionTypes: RegionType.Set_Passable,
                ignoreEntirelyForbiddenRegions: false);
            if (chosen == null && (int)(skillNeeded * 20f) != -1)
            {
                var reason = repairable == null
                    ? "MissingMaterials".Translate("UsableIngredients".Translate())
                    : "UnderRequiredSkill".Translate((int)(skillNeeded * 20f) - MendingSettingsAccess.PawnFreeSkillBonus) + " : " + repairable.LabelNoCount;
                JobFailReason.Is(reason);
            }
            __result = chosen != null;
            TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] Mending result ({resolution})={__result}, chosen={DescribeThing(chosen)}");
            return false;
        }
        // ExtraBillData is keyed by Bill instance; with BWM/copies, fall back to sibling bills and recipe match.
        private static bool TryResolveTakeFromParents(
            string route,
            Bill bill,
            Pawn pawn,
            Thing billGiver,
            out List<ISlotGroupParent> parents,
            out string resolution)
        {
            parents = null;
            resolution = null;
            if (bill == null)
            {
                TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] {route} skipped: bill is null.");
                return false;
            }
            object component = ExtraBillDataAccess.Instance;
            if (component == null)
            {
                TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] {route} skipped: GameComponent_ExtraBillData.Instance is null for {DescribeBill(bill, pawn, billGiver)}.");
                return false;
            }

            try
            {
                if (TryGetTakeFromFromData(ExtraBillDataAccess.GetData(component, bill), out parents))
                {
                    resolution = "direct";
                    TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] {route} resolved TakeFrom via direct bill key: {DescribeParents(parents)}");
                    return true;
                }
                if (bill.billStack != null)
                {
                    foreach (var other in bill.billStack.Bills)
                    {
                        if (other == null || ReferenceEquals(other, bill))
                        {
                            continue;
                        }
                        if (!ExtraBillDataMaintenance.IsBillDataKeyAlive(other))
                        {
                            TakeFromMendingPatchLog.Warning(
                                $"[TakeFromMendingPatch] {route} skipped dead bill stack sibling for {DescribeBill(bill, pawn, billGiver)}.");
                            continue;
                        }
                        if (TryGetTakeFromFromData(ExtraBillDataAccess.GetData(component, other), out parents))
                        {
                            resolution = $"billStack:{other.Label}";
                            TakeFromMendingPatchLog.Message(
                                $"[TakeFromMendingPatch] {route} resolved TakeFrom via bill stack sibling «{other.Label}»: {DescribeParents(parents)}");
                            return true;
                        }
                    }
                }

                int pruned = ExtraBillDataMaintenance.PruneStaleBillDataKeys(component, route);
                if (pruned > 0)
                {
                    TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] {route} continuing TakeFrom fallback after pruning {pruned} stale bill key(s).");
                }

                foreach (var kv in EnumerateBillDataWithTakeFrom(component))
                {
                    var other = kv.Key;
                    if (!ExtraBillDataMaintenance.IsBillDataKeyAlive(other))
                    {
                        continue;
                    }

                    if (ReferenceEquals(other, bill) || !ExtraBillDataMaintenance.BillRecipesMatch(other, bill))
                    {
                        continue;
                    }
                    if (!TryGetTakeFromFromData(kv.Value, out var otherParents))
                    {
                        continue;
                    }
                    if (other.billStack?.billGiver == bill.billStack?.billGiver)
                    {
                        parents = otherParents;
                        resolution = $"sameWorkbench:{other.Label}";
                        TakeFromMendingPatchLog.Message(
                            $"[TakeFromMendingPatch] {route} resolved TakeFrom via same workbench bill «{other.Label}»: {DescribeParents(parents)}");
                        return true;
                    }
                }
                Map map = pawn?.Map ?? billGiver?.Map;
                Bill bestMatch = null;
                object bestData = null;
                foreach (var kv in EnumerateBillDataWithTakeFrom(component))
                {
                    var other = kv.Key;
                    if (!ExtraBillDataMaintenance.IsBillDataKeyAlive(other))
                    {
                        continue;
                    }

                    if (ReferenceEquals(other, bill) || !ExtraBillDataMaintenance.BillRecipesMatch(other, bill))
                    {
                        continue;
                    }
                    if (map != null && other.billStack?.billGiver?.Map != map)
                    {
                        continue;
                    }
                    if (bestMatch == null || string.Equals(other.Label, bill.Label, StringComparison.Ordinal))
                    {
                        bestMatch = other;
                        bestData = kv.Value;
                    }
                }
                if (bestMatch != null && TryGetTakeFromFromData(bestData, out parents))
                {
                    resolution = $"recipeMatch:{bestMatch.Label}";
                    TakeFromMendingPatchLog.Message(
                        $"[TakeFromMendingPatch] {route} resolved TakeFrom via recipe match on «{bestMatch.Label}» " +
                        $"(linked/mirrored bill): {DescribeParents(parents)}");
                    return true;
                }
                object emptyDirect = ExtraBillDataAccess.GetData(component, bill);
                if (emptyDirect != null && !ExtraBillDataAccess.TakeFromHasAny(emptyDirect))
                {
                    TakeFromMendingPatchLog.Message(
                        $"[TakeFromMendingPatch] {route} skipped: TakeFrom is empty for {DescribeBill(bill, pawn, billGiver)} " +
                        $"(TakeFromText={ExtraBillDataAccess.TakeFromText(emptyDirect)}).");
                }
                else
                {
                    TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] {route} skipped: no ExtraBillData with TakeFrom for {DescribeBill(bill, pawn, billGiver)}.");
                    LogBillDataDiagnostics(bill, component);
                }
                return false;
            }
            catch (Exception ex)
            {
                TakeFromMendingPatchLog.Warning(
                    $"[TakeFromMendingPatch] {route} TakeFrom resolve failed for {DescribeBill(bill, pawn, billGiver)}: {ex}");
                Log.WarningOnce(
                    $"[TakeFromMendingPatch] TakeFrom resolve fail-open to vanilla ({route}): {ex.GetType().Name}: {ex.Message}",
                    unchecked((int)0xb7e54d91));
                parents = null;
                resolution = null;
                return false;
            }
        }
        private static bool TryGetTakeFromFromData(object extraData, out List<ISlotGroupParent> parents)
        {
            parents = null;
            if (!ExtraBillDataAccess.TakeFromHasAny(extraData))
            {
                return false;
            }

            parents = ExtraBillDataAccess.CopyTakeFromParents(extraData);
            return parents != null && parents.Count > 0;
        }

        private static IEnumerable<KeyValuePair<Bill, object>> EnumerateBillDataWithTakeFrom(object component)
        {
            IDictionary dict = ExtraBillDataMaintenance.GetBillDataDictionary(component);
            if (dict == null)
            {
                yield break;
            }

            foreach (DictionaryEntry entry in dict)
            {
                Bill key = entry.Key as Bill;
                if (!ExtraBillDataMaintenance.IsBillDataKeyAlive(key) || !TryGetTakeFromFromData(entry.Value, out _))
                {
                    continue;
                }

                yield return new KeyValuePair<Bill, object>(key, entry.Value);
            }
        }

        private static void LogBillDataDiagnostics(Bill bill, object component)
        {
            if (!KebabTweaksSettings.TakeFromMendingEnableLogging)
            {
                return;
            }

            IDictionary dict = ExtraBillDataMaintenance.GetBillDataDictionary(component);
            int count = dict?.Count ?? 0;
            TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] ExtraBillData dictionary entries: {count}");
            if (dict == null)
            {
                return;
            }

            foreach (DictionaryEntry entry in dict)
            {
                Bill key = entry.Key as Bill;
                if (!ExtraBillDataMaintenance.IsBillDataKeyAlive(key))
                {
                    TakeFromMendingPatchLog.Message("[TakeFromMendingPatch]   bill entry: <stale/dead key skipped>");
                    continue;
                }

                int takeFromCount = ExtraBillDataAccess.TakeFromCount(entry.Value);
                string recipe = key?.recipe?.defName ?? "<no recipe>";
                string label = key?.Label ?? "<no label>";
                string giver = key?.billStack?.billGiver?.LabelShort ?? "<no giver>";
                TakeFromMendingPatchLog.Message(
                    $"[TakeFromMendingPatch]   bill entry: recipe={recipe}, label={label}, giver={giver}, TakeFrom.Count={takeFromCount}, " +
                    $"sameStack={ReferenceEquals(key?.billStack, bill?.billStack)}");
            }
        }
        private static void LogStockpileContext(Pawn pawn, List<ISlotGroupParent> parents, string resolution)
        {
            if (!KebabTweaksSettings.TakeFromMendingEnableLogging || pawn?.Map == null)
            {
                return;
            }
            var allStockpiles = pawn.Map.zoneManager.AllZones
                .OfType<Zone_Stockpile>()
                .Select(z => $"{z.label} held={z.GetSlotGroup()?.HeldThings?.Count() ?? 0}")
                .ToList();
            TakeFromMendingPatchLog.Message(
                $"[TakeFromMendingPatch] Map stockpiles ({resolution}): {string.Join("; ", allStockpiles)}");
            TakeFromMendingPatchLog.Message($"[TakeFromMendingPatch] Selected TakeFrom ({resolution}): {DescribeParents(parents)}");
        }
        private static bool ChosenViolatesTakeFrom(IEnumerable<ThingCount> chosen, List<ISlotGroupParent> parents)
        {
            foreach (var thingCount in chosen)
            {
                if (thingCount.Thing != null && !IsThingFromAllowedStorage(thingCount.Thing, parents))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsThingFromAllowedStorage(Thing thing, List<ISlotGroupParent> parents)
        {
            if (thing == null || parents == null)
            {
                return false;
            }
            foreach (var parent in parents)
            {
                if (parent == null)
                {
                    continue;
                }
                if (parent is Zone_Stockpile zone && zone.ContainsCell(thing.Position))
                {
                    return true;
                }
                if (parent is Building_Storage storage && storage.GetSlotGroup()?.HeldThings?.Contains(thing) == true)
                {
                    return true;
                }
                var slotGroup = parent.GetSlotGroup();
                if (slotGroup?.HeldThings != null && slotGroup.HeldThings.Contains(thing))
                {
                    return true;
                }
            }
            return false;
        }
        // Things from slot-group HeldThings and from stockpile zone cells (on the floor).
        private static IEnumerable<Thing> GatherCandidateThings(IEnumerable<ISlotGroupParent> parents, Map map)
        {
            if (parents == null || map == null)
            {
                yield break;
            }
            var seen = new HashSet<Thing>();
            foreach (var parent in parents)
            {
                if (parent == null)
                {
                    continue;
                }
                foreach (var thing in ThingsFromParent(parent, map))
                {
                    if (thing != null && seen.Add(thing))
                    {
                        yield return thing;
                    }
                }
            }
        }
        private static IEnumerable<Thing> ThingsFromParent(ISlotGroupParent parent, Map map)
        {
            var slotGroup = parent.GetSlotGroup();
            if (slotGroup?.HeldThings != null)
            {
                foreach (var thing in slotGroup.HeldThings)
                {
                    if (thing != null)
                    {
                        yield return thing;
                    }
                }
            }
            if (parent is Zone_Stockpile zone)
            {
                foreach (var thing in zone.AllContainedThings)
                {
                    if (thing != null)
                    {
                        yield return thing;
                    }
                }
            }
        }
        private static bool IsRootRegionValid(string route, Pawn pawn, Bill bill, Thing billGiver, IntVec3 rootCell)
        {
            if (pawn?.Map?.regionGrid.GetValidRegionAt(rootCell) != null)
            {
                return true;
            }
            TakeFromMendingPatchLog.Message(
                $"[TakeFromMendingPatch] {route} result=false: invalid root region. root={rootCell}, {DescribeBill(bill, pawn, billGiver)}");
            return false;
        }
        private static bool IsValidMendingIngredient(
            Thing thing,
            Bill bill,
            Pawn pawn,
            bool ignoreHitPoints,
            float repairLimit,
            ref float skillNeeded,
            ref Thing repairable,
            out string reason)
        {
            if (thing == null || !thing.Spawned || thing.Destroyed || ForbidUtility.IsForbidden(thing, pawn))
            {
                reason = "null, unspawned, destroyed, or forbidden";
                return false;
            }
            if (!bill.recipe.fixedIngredientFilter.Allows(thing) ||
                !bill.ingredientFilter.Allows(thing) ||
                !bill.recipe.ingredients.Any(ingNeed => ingNeed.filter.Allows(thing)))
            {
                reason = "recipe or bill ingredient filter rejected thing";
                return false;
            }
            if (!ignoreHitPoints &&
                (thing.HitPoints <= 0 ||
                 thing.HitPoints >= thing.MaxHitPoints ||
                 !EnoughSkill(thing, repairLimit, ref skillNeeded, ref repairable)))
            {
                reason = $"hitpoints/skill rejected thing: hp={thing.HitPoints}/{thing.MaxHitPoints}, repairLimit={repairLimit:F3}, skillNeeded={skillNeeded:F3}";
                return false;
            }
            if (!ReservationUtility.CanReserve(
                    pawn,
                    thing,
                    maxPawns: 1,
                    stackCount: -1,
                    layer: null,
                    ignoreOtherReservations: false))
            {
                reason = "pawn cannot reserve thing";
                return false;
            }
            if (bill.CheckIngredientsIfSociallyProper && !SocialProperness.IsSociallyProper(thing, pawn))
            {
                reason = "thing is not socially proper";
                return false;
            }
            reason = "ok";
            return true;
        }
        private static bool IsValidVanillaIngredient(Thing thing, Bill bill, Pawn pawn, Thing billGiver, out string reason)
        {
            if (thing == null || !thing.Spawned || thing.Destroyed)
            {
                reason = "null, unspawned, or destroyed";
                return false;
            }
            if (ForbidUtility.IsForbidden(thing, pawn))
            {
                reason = "forbidden";
                return false;
            }
            var offset = thing.Position - billGiver.Position;
            float radiusSquared = bill.ingredientSearchRadius * bill.ingredientSearchRadius;
            if (offset.LengthHorizontalSquared >= radiusSquared)
            {
                reason = $"outside ingredientSearchRadius: distSq={offset.LengthHorizontalSquared}, radiusSq={radiusSquared}";
                return false;
            }
            if (!bill.IsFixedOrAllowedIngredient(thing))
            {
                reason = "bill fixed/allowed ingredient filter rejected thing";
                return false;
            }
            if (!bill.recipe.ingredients.Any(ingNeed => ingNeed.filter.Allows(thing)))
            {
                reason = "recipe ingredient filters rejected thing";
                return false;
            }
            if (!ReservationUtility.CanReserve(pawn, thing, maxPawns: 1, stackCount: -1, layer: null, ignoreOtherReservations: false))
            {
                reason = "pawn cannot reserve thing";
                return false;
            }
            if (!ReachabilityUtility.CanReach(pawn, thing, PathEndMode.ClosestTouch, Danger.Deadly, canBashDoors: false, canBashFences: false, TraverseMode.ByPawn))
            {
                reason = "pawn cannot reach thing";
                return false;
            }
            if (!thing.def.EverHaulable)
            {
                reason = "thing def is not EverHaulable";
                return false;
            }
            reason = "ok";
            return true;
        }
        private static bool VanillaTryFindBestBillIngredientsInSet(
            List<Thing> availableThings,
            Bill bill,
            List<ThingCount> chosen,
            IntVec3 rootCell,
            bool alreadySorted,
            List<IngredientCount> missingIngredients)
        {
            var method = AccessTools.Method(
                typeof(RimWorld.WorkGiver_DoBill),
                "TryFindBestBillIngredientsInSet",
                new[]
                {
                    typeof(List<Thing>),
                    typeof(Bill),
                    typeof(List<ThingCount>),
                    typeof(IntVec3),
                    typeof(bool),
                    typeof(List<IngredientCount>)
                });
            if (method == null)
            {
                Log.Error("[TakeFromMendingPatch] Could not find RimWorld.WorkGiver_DoBill.TryFindBestBillIngredientsInSet.");
                return false;
            }
            return (bool)method.Invoke(null, new object[]
            {
                availableThings,
                bill,
                chosen,
                rootCell,
                alreadySorted,
                missingIngredients
            });
        }
        private static float GetRepairLimit(Bill bill, Pawn pawn)
        {
            if (!MendingSettingsAccess.ProgressiveMending)
            {
                return 1f;
            }
            if (bill.recipe.workSkill == null)
            {
                return 0.001f;
            }
            var skillRecord = pawn.skills?.GetSkill(bill.recipe.workSkill);
            int level = skillRecord != null
                ? skillRecord.GetLevel(includeAptitudes: true)
                : pawn.RaceProps.mechFixedSkillLevel;
            return 0.05f * Mathf.Clamp(level + MendingSettingsAccess.PawnFreeSkillBonus, 0, 20);
        }
        private static bool EnoughSkill(Thing thing, float repairLimit, ref float skillNeeded, ref Thing repairable)
        {
            if (thing.HitPoints < (int)(thing.MaxHitPoints * repairLimit))
            {
                return true;
            }
            const float step = 0.05f;
            var required = repairLimit + step;
            while (required < 1f && thing.HitPoints >= (int)(thing.MaxHitPoints * Mathf.Clamp(required, 0f, 1f)))
            {
                required += step;
            }
            skillNeeded = required;
            repairable = thing;
            return false;
        }
        private static IntVec3 GetBillGiverRootCell(Thing billGiver, Pawn pawn)
        {
            if (billGiver is Building building)
            {
                if (building.def.hasInteractionCell)
                {
                    return building.InteractionCell;
                }
                Log.Error("[TakeFromMendingPatch] Tried to find bill ingredients for a building with no interaction cell: " + billGiver);
                return pawn.Position;
            }
            return billGiver.Position;
        }
        private static string DescribeBill(Bill bill, Pawn pawn, Thing billGiver)
        {
            string recipe = bill?.recipe?.defName ?? "<no recipe>";
            string billLabel = bill?.Label ?? "<no bill label>";
            string pawnLabel = pawn?.LabelShort ?? "<no pawn>";
            string giverLabel = billGiver == null ? "<no billGiver>" : $"{billGiver.LabelShort} at {billGiver.Position}";
            int billId = bill?.GetHashCode() ?? 0;
            return $"recipe={recipe}, bill={billLabel}, billId={billId}, pawn={pawnLabel}, billGiver={giverLabel}, radius={bill?.ingredientSearchRadius}";
        }
        private static string DescribeParents(IEnumerable<ISlotGroupParent> parents)
        {
            return string.Join("; ", parents.Select(parent =>
            {
                var slotGroup = parent.GetSlotGroup();
                int heldCount = slotGroup?.HeldThings?.Count() ?? 0;
                string label = parent.SlotYielderLabel();
                if (parent is Zone_Stockpile zone)
                {
                    int contained = zone.AllContainedThings?.Count() ?? 0;
                    return $"Zone:{label} held={heldCount} contained={contained}";
                }
                return $"{parent.GetType().Name}:{label} held={heldCount}";
            }));
        }
        private static string DescribeThing(Thing thing)
        {
            if (thing == null)
            {
                return "<null>";
            }
            string slotGroup = thing.MapHeld?.haulDestinationManager?.SlotGroupAt(thing.Position)?.parent?.SlotYielderLabel() ?? "<no slotGroup>";
            return $"{thing.LabelShort} def={thing.def?.defName} stack={thing.stackCount} hp={thing.HitPoints}/{thing.MaxHitPoints} pos={thing.Position} slot={slotGroup}";
        }
        private static string DescribeThingCounts(IEnumerable<ThingCount> things)
        {
            if (things == null)
            {
                return "<null>";
            }
            return string.Join("; ", things.Select(thingCount => $"{DescribeThing(thingCount.Thing)} count={thingCount.Count}"));
        }
        private static string DescribeMissingIngredients(IEnumerable<IngredientCount> ingredients)
        {
            if (ingredients == null)
            {
                return "<null>";
            }
            return string.Join("; ", ingredients.Select(ingredient => ingredient.Summary));
        }
    }
    // Verbose ingredient-search logs in Player.log (prefix [TakeFromMendingPatch]).
    public static class TakeFromMendingPatchLog
    {
        public static void Message(string text)
        {
            if (KebabTweaksSettings.TakeFromMendingEnableLogging)
            {
                Log.Message(text);
            }
        }

        public static void Warning(string text)
        {
            if (KebabTweaksSettings.TakeFromMendingEnableLogging)
            {
                Log.Warning(text);
            }
        }

        public static void Maintenance(string text)
        {
            if (KebabTweaksSettings.TakeFromMendingShowMaintenanceMessages)
            {
                Log.Message(text);
            }
        }
    }
}
