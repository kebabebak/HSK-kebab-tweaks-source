using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using SK;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Runtime leftover vs current for probe-capable Obsolete candidates, plus partial
    /// relevance for Craft Stuff / Fermenter Fill / SeedsPlease Sow. Reads known defs or
    /// original method IL once per launch. Closed → Obsolete default off. Partial → current
    /// with a suffix. Alive or Unknown → current, no suffix, do not force-disable.
    ///
    /// Рантайм leftover / current для фиксов, которые могут устареть, плюс частичная
    /// актуальность Craft Stuff / Fermenter Fill / SeedsPlease Sow. Читает известные дефы
    /// или IL оригинала один раз за запуск. Closed → Устаревшие, default off. Partial →
    /// актуальные с суффиксом. Alive или Unknown → актуальные без суффикса, принудительно
    /// не гасить.
    /// </summary>
    public static class FixSymptomProbe
    {
        const float BreachAxeMissingWorkToMakeMax = 1.01f;
        const int CraftStuffPartialRelevancePercent = 25;
        const int UfFillPartialRelevancePercent = 65;
        const int SeedsPleaseSowPartialRelevancePercent = 40;
        const string SeedsPleaseDriverTypeName =
            "SeedsPlease.JobDriver_PlantSowWithSeeds";
        const string SkDominantIngredientPrefixTypeName =
            "SK.Patch_Toils_Recipe_CalculateDominantIngredient";
        const string UfFillJobDriverTypeName = "UniversalFermenterSK.JobDriver_FillUF";
        const string UfFillJobDriverTypeNameAlt = "UniversalFermenter.JobDriver_FillUF";
        const string UfFillAbortWithoutConsumingMarker =
            "Aborting job without consuming them";
        const string SkEnemySelfPreservationPatchTypeName =
            "SK.EnemySelfPreservationPatch";
        const string EspPawnPostApplyDamageTypeName =
            "MUR_ESP.Pawn_PostApplyDamage";
        const string CorunanPersonNamerDefName = "NamerPersonCorunanEN";
        const string SkPawnGeneratorPatchesTypeName = "SK.PawnGeneratorPatches";
        const string SkPawnGeneratorChildAgeMethodName = "PawnGenerator_GenerateRandomAge";
        const string SkFactionLeaderGenerationTypeName = "SK.FactionLeaderGenerationPatches";
        const string SkFactionLeaderGeneratePawnMethodName = "PawnGenerator_GeneratePawn";
        const string SkFactionLeaderFinalizerMethodName = "Faction_TryGenerateNewLeader";
        const string RimThemesBackgroundPatchTypeName = "RimThemesLite.UI_BackgroundMain_Patch";
        const string ArchitectInfoBoxPatchTypeName = "SK.Patch_ArchitectCategoryTab_DoInfoBox";
        const string ArchitectInfoBoxCacheFieldName = "_cache";
        const string ArchitectInfoBoxTargetMethodName = "TargetMethod";
        const string RimatomicsHelTypeName = "Rimatomics.Building_HEL";
        const string RimatomicsBetterHelTypeName = "SK.Rimatomic_BetterHEL";
        const string RimatomicsBetterHelMethodName = "TryFindNewTarget";
        const string SkSoldPawnExitGoodwillTypeName = "SK.Patch_SoldPawnExitGoodwill";
        const string SkSoldPawnExitGoodwillMethodName = "Pawn_PreTraded";
        const string SoldPawnTendFieldName = "timesGuestTendedToByPlayer";

        static bool catCrazyTimeProbed;
        static bool catCrazyTimeLeftover;
        static bool startingPawnNameProbed;
        static bool startingPawnNameLeftover;
        static bool startingPawnChildAgeProbed;
        static bool startingPawnChildAgeLeftover;
        static bool tribalLeaderProbed;
        static bool tribalLeaderLeftover;
        static bool craftStuffProbed;
        static bool craftStuffLeftover;
        static bool craftStuffPartial;
        static bool seedsPleaseSowProbed;
        static bool seedsPleaseSowLeftover;
        static bool seedsPleaseSowPartial;
        static bool defsProbed;
        static bool burnWeaponLeftover;
        static bool breachAxeLeftover;
        static bool rawFungusLeftover;
        static bool ufFillProbed;
        static bool ufFillLeftover;
        static bool ufFillPartial;
        static bool anomalyEspProbed;
        static bool anomalyEspLeftover;
        static bool mainMenuBgFitProbed;
        static bool mainMenuBgFitLeftover;
        static bool readoutGuiProbed;
        static bool readoutGuiLeftover;
        static bool architectInfoBoxProbed;
        static bool architectInfoBoxLeftover;
        static bool rimatomicsHelProbed;
        static bool rimatomicsHelLeftover;
        static bool diseaseComplicationProbed;
        static bool diseaseComplicationLeftover;
        static bool traitGainProbed;
        static bool traitGainLeftover;
        static bool soldPawnExitProbed;
        static bool soldPawnExitLeftover;
        static bool architectSearchResearchProbed;
        static bool architectSearchResearchLeftover;
        static bool logged;

        /// <summary>
        /// Refreshes leftover flags when defs exist. Idempotent per launch.
        ///
        /// Обновляет leftover-флаги, когда дефы есть. Повторные вызовы за запуск безвредны.
        /// </summary>
        public static void Ensure()
        {
            ProbeCatCrazyTimeIfNeeded();
            ProbeCraftStuffIfNeeded();
            ProbeSeedsPleaseSowIfNeeded();
            ProbeStartingPawnNameIfNeeded();
            ProbeStartingPawnChildAgeIfNeeded();
            ProbeTribalLeaderIfNeeded();
            ProbeArchitectInfoBoxIfNeeded();
            ProbeRimatomicsHelIfNeeded();
            ProbeDiseaseComplicationIfNeeded();
            ProbeTraitGainIfNeeded();
            ProbeSoldPawnExitGoodwillIfNeeded();
            ProbeArchitectSearchResearchIfNeeded();
            ProbeUfFillIfNeeded();
            ProbeAnomalyEspFleeIfNeeded();
            ProbeMainMenuBgFitIfNeeded();
            ProbeReadoutGuiIfNeeded();
            ProbeDefBackedFixesIfNeeded();
            LogOnce();
        }

        public static bool IsCatCrazyTimeLeftover()
        {
            Ensure();
            return catCrazyTimeLeftover;
        }

        public static bool IsCraftStuffFixLeftover()
        {
            Ensure();
            return craftStuffLeftover;
        }

        /// <summary>
        /// Header suffix for Craft Stuff: 0 leftover, 25 when SK already filters CanMake but
        /// still picks random or ingredients[0], null when the SK Prefix is absent or has no
        /// CanMake (omit = 100%).
        ///
        /// Суффикс Craft Stuff: 0 leftover, 25 если SK уже фильтрует CanMake, но ещё берёт
        /// random или ingredients[0], null если Prefix SK нет или в нём нет CanMake (без
        /// суффикса = 100%).
        /// </summary>
        public static int? GetCraftStuffRelevancePercent()
        {
            Ensure();
            if (craftStuffLeftover)
            {
                return 0;
            }

            if (craftStuffPartial)
            {
                return CraftStuffPartialRelevancePercent;
            }

            return null;
        }

        public static bool IsSeedsPleaseSowFixLeftover()
        {
            Ensure();
            return seedsPleaseSowLeftover;
        }

        public static bool IsStartingPawnNameFixLeftover()
        {
            Ensure();
            return startingPawnNameLeftover;
        }

        public static bool IsStartingPawnChildAgeFixLeftover()
        {
            Ensure();
            return startingPawnChildAgeLeftover;
        }

        public static bool IsNeanderthalChiefLeaderFixLeftover()
        {
            Ensure();
            return tribalLeaderLeftover;
        }

        public static bool IsArchitectInfoBoxFpsFixLeftover()
        {
            Ensure();
            return architectInfoBoxLeftover;
        }

        public static bool IsArchitectSearchResearchPatchLeftover()
        {
            Ensure();
            return architectSearchResearchLeftover;
        }

        public static bool IsRimatomicsHelFpsFixLeftover()
        {
            Ensure();
            return rimatomicsHelLeftover;
        }

        public static bool IsDiseaseComplicationFixLeftover()
        {
            Ensure();
            return diseaseComplicationLeftover;
        }

        public static bool IsSoldPawnExitGoodwillFixLeftover()
        {
            Ensure();
            return soldPawnExitLeftover;
        }

        public static bool IsTraitGainFixLeftover()
        {
            Ensure();
            return traitGainLeftover;
        }

        /// <summary>
        /// True when SeedsPlease IsCellOpenForSowingPlantOfType already loads Zone_Growing.allowSow
        /// (hop Closed or leftover). allowSow postfix should no-op; TargetB transpilers still run
        /// while leftover is false.
        ///
        /// True, если IsCellOpenForSowingPlantOfType уже грузит Zone_Growing.allowSow (hop Closed
        /// или leftover). Postfix allowSow должен молчать; транспайлеры TargetB ещё работают,
        /// пока leftover false.
        /// </summary>
        public static bool IsSeedsPleaseSowHopClosed()
        {
            Ensure();
            return seedsPleaseSowLeftover || seedsPleaseSowPartial;
        }

        /// <summary>
        /// Header suffix for SeedsPlease Sow: 0 leftover, 40 when hop already reads allowSow but
        /// MakeNewToils still loads Job.targetB, null when hop is still open (omit = 100%).
        ///
        /// Суффикс SeedsPlease Sow: 0 leftover, 40 если hop уже читает allowSow, но MakeNewToils
        /// ещё грузит Job.targetB, null если hop ещё открыт (без суффикса = 100%).
        /// </summary>
        public static int? GetSeedsPleaseSowRelevancePercent()
        {
            Ensure();
            if (seedsPleaseSowLeftover)
            {
                return 0;
            }

            if (seedsPleaseSowPartial)
            {
                return SeedsPleaseSowPartialRelevancePercent;
            }

            return null;
        }

        public static bool IsBurnWeaponBillFixLeftover()
        {
            Ensure();
            return burnWeaponLeftover;
        }

        public static bool IsBreachAxeWorkAmountFixLeftover()
        {
            Ensure();
            return breachAxeLeftover;
        }

        public static bool IsRawFungusBillFixLeftover()
        {
            Ensure();
            return rawFungusLeftover;
        }

        public static bool IsUfFillFixLeftover()
        {
            Ensure();
            return ufFillLeftover;
        }

        public static bool IsAnomalyEspFleeFixLeftover()
        {
            Ensure();
            return anomalyEspLeftover;
        }

        public static bool IsMainMenuBgFitFixLeftover()
        {
            Ensure();
            return mainMenuBgFitLeftover;
        }

        public static bool IsToggleableReadoutsGuiFixLeftover()
        {
            Ensure();
            return readoutGuiLeftover;
        }

        /// <summary>
        /// Header suffix for Fermenter Fill: 0 leftover, 65 when CommitIngredients still aborts
        /// extra stacks after IngredientPortion RemainingCount or get_RemainingCount, null when
        /// that abort exists without that call (omit = 100%) or commit/driver is missing.
        ///
        /// Суффикс Fermenter Fill: 0 leftover, 65 если CommitIngredients всё ещё рвёт лишние
        /// стаки после RemainingCount или get_RemainingCount у IngredientPortion, null если abort
        /// есть без этого вызова (без суффикса = 100%) или нет commit/драйвера.
        /// </summary>
        public static int? GetUfFillRelevancePercent()
        {
            Ensure();
            if (ufFillLeftover)
            {
                return 0;
            }

            if (ufFillPartial)
            {
                return UfFillPartialRelevancePercent;
            }

            return null;
        }

        public static bool DefBackedProbesReady
        {
            get
            {
                return defsProbed;
            }
        }

        public static bool HasAnyLeftover()
        {
            if (IsCatCrazyTimeLeftover() || IsCraftStuffFixLeftover() ||
                IsSeedsPleaseSowFixLeftover() || IsStartingPawnNameFixLeftover() ||
                IsStartingPawnChildAgeFixLeftover() || IsNeanderthalChiefLeaderFixLeftover())
            {
                return true;
            }

            if (IsUfFillFixLeftover() || IsBurnWeaponBillFixLeftover() ||
                IsBreachAxeWorkAmountFixLeftover() || IsRawFungusBillFixLeftover() ||
                IsAnomalyEspFleeFixLeftover() || IsMainMenuBgFitFixLeftover() ||
                IsToggleableReadoutsGuiFixLeftover() || IsArchitectInfoBoxFpsFixLeftover() ||
                IsRimatomicsHelFpsFixLeftover() || IsDiseaseComplicationFixLeftover() ||
                IsTraitGainFixLeftover() ||
                IsSoldPawnExitGoodwillFixLeftover())
            {
                return true;
            }

            return false;
        }

        static void ProbeCatCrazyTimeIfNeeded()
        {
            if (catCrazyTimeProbed)
            {
                return;
            }

            catCrazyTimeProbed = true;
            // typeof(JobDriver_CrazyTime) is only in ProbeCatCrazyTimeClosed. Calling it when
            // Core SK is absent JIT-fails the whole apply pass.
            if (AccessTools.TypeByName("SK.JobDriver_CrazyTime") == null)
            {
                catCrazyTimeLeftover = false;
                return;
            }

            catCrazyTimeLeftover = ProbeCatCrazyTimeClosed();
        }

        static void ProbeCraftStuffIfNeeded()
        {
            if (craftStuffProbed)
            {
                return;
            }

            craftStuffProbed = true;
            craftStuffLeftover = false;
            craftStuffPartial = false;
            ProbeCraftStuffBucket();
        }

        static void ProbeSeedsPleaseSowIfNeeded()
        {
            if (seedsPleaseSowProbed)
            {
                return;
            }

            seedsPleaseSowProbed = true;
            seedsPleaseSowLeftover = false;
            seedsPleaseSowPartial = false;
            ProbeSeedsPleaseSowBucket();
        }

        static void ProbeStartingPawnNameIfNeeded()
        {
            if (startingPawnNameProbed)
            {
                return;
            }

            if (DefDatabase<RulePackDef>.DefCount == 0)
            {
                return;
            }

            startingPawnNameProbed = true;
            startingPawnNameLeftover = ProbeStartingPawnNameClosed();
        }

        static void ProbeStartingPawnChildAgeIfNeeded()
        {
            if (startingPawnChildAgeProbed)
            {
                return;
            }

            startingPawnChildAgeProbed = true;
            startingPawnChildAgeLeftover = ProbeStartingPawnChildAgeClosed();
        }

        static void ProbeTribalLeaderIfNeeded()
        {
            if (tribalLeaderProbed)
            {
                return;
            }

            tribalLeaderProbed = true;
            tribalLeaderLeftover = ProbeTribalLeaderClosed();
        }

        static void ProbeArchitectInfoBoxIfNeeded()
        {
            if (architectInfoBoxProbed)
            {
                return;
            }

            architectInfoBoxProbed = true;
            architectInfoBoxLeftover = ProbeArchitectInfoBoxClosed();
        }

        static void ProbeRimatomicsHelIfNeeded()
        {
            if (rimatomicsHelProbed)
            {
                return;
            }

            rimatomicsHelProbed = true;
            rimatomicsHelLeftover = ProbeRimatomicsHelClosed();
        }

        static void ProbeArchitectSearchResearchIfNeeded()
        {
            if (architectSearchResearchProbed)
            {
                return;
            }

            architectSearchResearchProbed = true;
            architectSearchResearchLeftover = ProbeArchitectSearchResearchClosed();
        }

        /// <summary>
        /// Closed when the loaded Architect Sense search prefix calls Prefs.get_DevMode and get_Visible.
        /// That is the visibility cut: hidden projects stay out of search, and developer mode still
        /// searches them. Architect Sense absent stays current, because vanilla Matches still ignores Visible.
        ///
        /// Closed, когда загруженный префикс поиска Architect Sense вызывает Prefs.get_DevMode и get_Visible.
        /// Это отсечение по видимости: скрытые проекты не ищутся, а режим разработчика их всё ещё ищет.
        /// Без Architect Sense строка остаётся актуальной: ванильный Matches по-прежнему не смотрит Visible.
        /// </summary>
        static bool ProbeArchitectSearchResearchClosed()
        {
            Type patch = AccessTools.TypeByName(
                "ArchitectSense.HarmonyPatches+ArchitectCategoryTab_Matches_Patch");
            if (patch == null)
            {
                return false;
            }

            bool callsDevMode = false;
            bool callsVisible = false;
            MethodInfo[] methods = patch.GetMethods(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methods.Length; i++)
            {
                if (MethodCalls(methods[i], typeof(Prefs), "get_DevMode"))
                {
                    callsDevMode = true;
                }

                if (MethodCallsName(methods[i], "get_Visible"))
                {
                    callsVisible = true;
                }
            }

            return callsDevMode && callsVisible;
        }

        /// <summary>
        /// Closed when Diseases+ IsReady already passes get_Severity into FloatRange.Includes.
        /// That is the 1.7 mod and a later 1.6 mod with the same comparison. Alive when Includes
        /// still reads the per-tick argument. The mod being absent stays current so the row is
        /// veiled, not Obsolete.
        ///
        /// Closed, если IsReady в Diseases+ уже передаёт get_Severity в FloatRange.Includes.
        /// Это мод 1.7 и поздний мод 1.6 с тем же сравнением. Alive, если Includes всё ещё читает
        /// аргумент за тик. Без мода строка остаётся актуальной и затемняется, а не уходит в «Устаревшие».
        /// </summary>
        static void ProbeDiseaseComplicationIfNeeded()
        {
            if (diseaseComplicationProbed)
            {
                return;
            }

            diseaseComplicationProbed = true;
            diseaseComplicationLeftover =
                DiseaseComplicationFixFeatures.ReadOriginalCompare() ==
                DiseaseSeverityCompare.CurrentSeverity;
        }

        /// <summary>
        /// Closed when loaded SK.TraitChangerMapComponent subclasses MapComponent, ticks, stores
        /// checkColonistCannibal, and that tick IL contains 1200 and 2200. Absent type stays current.
        ///
        /// Closed, если загруженный SK.TraitChangerMapComponent наследует MapComponent, тикает,
        /// хранит checkColonistCannibal, а IL тика содержит 1200 и 2200. Нет типа - строка актуальна.
        /// </summary>
        static void ProbeTraitGainIfNeeded()
        {
            if (traitGainProbed)
            {
                return;
            }

            traitGainProbed = true;
            traitGainLeftover = TraitGainFixFeatures.UpstreamTraitChangerPresent();
        }

        /// <summary>
        /// Closed when Rimatomics Building_HEL is loaded and SK.Rimatomic_BetterHEL.TryFindNewTarget
        /// is missing or its IL does not call ListerThings.get_AllThings. Alive when that call is
        /// still there. Rimatomics absent stays current so the row is veiled, not Obsolete.
        ///
        /// Closed, если Rimatomics Building_HEL загружен, а SK.Rimatomic_BetterHEL.TryFindNewTarget
        /// отсутствует или его IL не вызывает ListerThings.get_AllThings. Alive, если этот вызов
        /// ещё есть. Без Rimatomics строка остаётся актуальной и затемняется, а не уходит в «Устаревшие».
        /// </summary>
        static bool ProbeRimatomicsHelClosed()
        {
            if (AccessTools.TypeByName(RimatomicsHelTypeName) == null)
            {
                return false;
            }

            Type better = AccessTools.TypeByName(RimatomicsBetterHelTypeName);
            MethodInfo method = better == null
                ? null
                : AccessTools.Method(better, RimatomicsBetterHelMethodName);
            if (method == null)
            {
                return true;
            }

            if (method.GetMethodBody() == null)
            {
                return false;
            }

            return !MethodCalls(method, typeof(ListerThings), "get_AllThings");
        }

        /// <summary>
        /// Closed when SK.Patch_ArchitectCategoryTab_DoInfoBox has _cache and TargetMethod.
        /// Alive when that type or either member is absent (the loaded Core_SK still calls
        /// TruncateHeight without a dictionary).
        ///
        /// Closed, если у SK.Patch_ArchitectCategoryTab_DoInfoBox есть _cache и TargetMethod.
        /// Alive, если типа или любого из членов нет (загруженный Core_SK всё ещё зовёт
        /// TruncateHeight без словаря).
        /// </summary>
        static bool ProbeArchitectInfoBoxClosed()
        {
            Type sk = AccessTools.TypeByName(ArchitectInfoBoxPatchTypeName);
            if (sk == null)
            {
                return false;
            }

            return AccessTools.Field(sk, ArchitectInfoBoxCacheFieldName) != null
                && AccessTools.Method(sk, ArchitectInfoBoxTargetMethodName) != null;
        }

        /// <summary>
        /// Closed when SK.FactionLeaderGenerationPatches has the GeneratePawn prefix and the
        /// TryGenerateNewLeader finalizer. Alive when that type or either method is absent
        /// (the loaded Core_SK does not yet force violence on those leader requests).
        ///
        /// Closed, если у SK.FactionLeaderGenerationPatches есть prefix GeneratePawn и
        /// finalizer TryGenerateNewLeader. Alive, если типа или любого из методов нет
        /// (загруженный Core_SK ещё не требует violence у этих запросов лидера).
        /// </summary>
        static bool ProbeTribalLeaderClosed()
        {
            Type sk = AccessTools.TypeByName(SkFactionLeaderGenerationTypeName);
            if (sk == null)
            {
                return false;
            }

            return AccessTools.Method(sk, SkFactionLeaderGeneratePawnMethodName) != null
                && AccessTools.Method(sk, SkFactionLeaderFinalizerMethodName) != null;
        }

        /// <summary>
        /// Closed when SK.PawnGeneratorPatches.PawnGenerator_GenerateRandomAge stores
        /// PawnGenerationRequest.FixedBiologicalAge. Alive or Unknown when that type or
        /// setter call is absent (1.5, or an Alpha that still leaves Child age to the kind floor).
        ///
        /// Closed, если SK.PawnGeneratorPatches.PawnGenerator_GenerateRandomAge пишет
        /// PawnGenerationRequest.FixedBiologicalAge. Alive или Unknown, если типа или вызова
        /// сеттера нет (1.5 или Alpha, где возраст Child всё ещё упирается в пол kind).
        /// </summary>
        static bool ProbeStartingPawnChildAgeClosed()
        {
            Type sk = AccessTools.TypeByName(SkPawnGeneratorPatchesTypeName);
            if (sk == null)
            {
                return false;
            }

            MethodInfo method = AccessTools.Method(sk, SkPawnGeneratorChildAgeMethodName);
            return MethodCallsNamed(method, "set_FixedBiologicalAge");
        }

        /// <summary>
        /// Closed when the loaded Core_SK sale hook stores 0 in the tend counter and passes
        /// false to guest.Released. Alive when that type or either write is absent.
        ///
        /// Closed, если загруженный хук продажи Core_SK пишет 0 в счётчик лечений и передаёт
        /// false в guest.Released. Alive, если этого типа или любой из записей нет.
        /// </summary>
        static void ProbeSoldPawnExitGoodwillIfNeeded()
        {
            if (soldPawnExitProbed)
            {
                return;
            }

            soldPawnExitProbed = true;
            soldPawnExitLeftover = ProbeSoldPawnExitGoodwillClosed();
        }

        static bool ProbeSoldPawnExitGoodwillClosed()
        {
            Type sk = AccessTools.TypeByName(SkSoldPawnExitGoodwillTypeName);
            if (sk == null)
            {
                return false;
            }

            MethodInfo method = AccessTools.Method(sk, SkSoldPawnExitGoodwillMethodName);
            if (method == null || method.GetMethodBody() == null)
            {
                return false;
            }

            return MethodStoresZeroToField(method, SoldPawnTendFieldName)
                && MethodPassesFalseTo(method, "set_Released");
        }

        /// <summary>
        /// Closed when NamerPersonCorunanEN has person-name rules and none of them is a
        /// single token (space or quotes in the body). Alive when any r_name/name rule is
        /// one token. Unknown (not leftover) if the def or those rules are missing.
        ///
        /// Closed, если у NamerPersonCorunanEN есть правила имени и ни одно не однословное
        /// (в теле пробел или кавычки). Alive, если любое r_name/name - один токен. Unknown
        /// (не leftover), если дефа или этих правил нет.
        /// </summary>
        static bool ProbeStartingPawnNameClosed()
        {
            RulePackDef def = DefDatabase<RulePackDef>.GetNamedSilentFail(CorunanPersonNamerDefName);
            if (def == null)
            {
                return false;
            }

            List<string> rules = GetRulePackStrings(def);
            if (rules == null)
            {
                return false;
            }

            bool sawNameRule = false;
            for (int i = 0; i < rules.Count; i++)
            {
                string rule = rules[i];
                if (!IsPersonNameMakerRule(rule))
                {
                    continue;
                }

                sawNameRule = true;
                if (IsSingleTokenNameRuleBody(rule))
                {
                    return false;
                }
            }

            return sawNameRule;
        }

        static List<string> GetRulePackStrings(RulePackDef def)
        {
            FieldInfo packField = AccessTools.Field(typeof(RulePackDef), "rulePack");
            object pack = packField == null ? null : packField.GetValue(def);
            if (pack == null)
            {
                return null;
            }

            FieldInfo stringsField = AccessTools.Field(pack.GetType(), "rulesStrings");
            return stringsField == null ? null : stringsField.GetValue(pack) as List<string>;
        }

        static bool IsPersonNameMakerRule(string rule)
        {
            if (string.IsNullOrEmpty(rule))
            {
                return false;
            }

            int arrow = rule.IndexOf("->", StringComparison.Ordinal);
            if (arrow <= 0)
            {
                return false;
            }

            string key = rule.Substring(0, arrow);
            return key.StartsWith("r_name", StringComparison.Ordinal) ||
                string.Equals(key, "name", StringComparison.Ordinal);
        }

        static bool IsSingleTokenNameRuleBody(string rule)
        {
            int arrow = rule.IndexOf("->", StringComparison.Ordinal);
            if (arrow < 0)
            {
                return false;
            }

            string body = rule.Substring(arrow + 2).Trim();
            if (body.Length == 0)
            {
                return true;
            }

            return body.IndexOf(' ') < 0 && body.IndexOf('\'') < 0;
        }

        /// <summary>
        /// Closed when SK no longer re-rolls CrazyTime num on toil rebuild. Alive when
        /// MakeNewToils still calls Rand.RangeInclusive.
        ///
        /// Closed, если SK больше не бросает num при пересборке toils. Alive, если
        /// MakeNewToils всё ещё зовёт Rand.RangeInclusive.
        /// </summary>
        static bool ProbeCatCrazyTimeClosed()
        {
            Type driver = typeof(JobDriver_CrazyTime);
            if (IteratorCallsRangeInclusive(driver))
            {
                return false;
            }

            MethodInfo tryMake = AccessTools.Method(driver, "TryMakePreToilReservations");
            if (tryMake == null)
            {
                return false;
            }

            if (!MethodCalls(tryMake, typeof(Rand), "RangeInclusive"))
            {
                return true;
            }

            FieldInfo numField = AccessTools.Field(driver, "num");
            return numField != null && MethodLoadsField(tryMake, numField);
        }

        /// <summary>
        /// Leftover when IsCellOpen already loads Zone_Growing.allowSow and MakeNewToils no
        /// longer loads Job.targetB. Partial when hop is closed but targetB is still loaded.
        /// Alive when hop is still open or SeedsPlease types are missing.
        ///
        /// Leftover, если IsCellOpen уже грузит Zone_Growing.allowSow и MakeNewToils больше
        /// не грузит Job.targetB. Partial, если hop закрыт, но targetB ещё грузится. Alive,
        /// если hop ещё открыт или типов SeedsPlease нет.
        /// </summary>
        static void ProbeSeedsPleaseSowBucket()
        {
            Type driver = AccessTools.TypeByName(SeedsPleaseDriverTypeName);
            if (driver == null)
            {
                return;
            }

            MethodInfo isCellOpen = AccessTools.Method(
                driver,
                "IsCellOpenForSowingPlantOfType",
                new[] { typeof(IntVec3), typeof(Map), typeof(ThingDef) });
            FieldInfo allowSow = AccessTools.Field(typeof(Zone_Growing), "allowSow");
            if (isCellOpen == null || allowSow == null)
            {
                return;
            }

            if (!MethodLoadsField(isCellOpen, allowSow))
            {
                return;
            }

            FieldInfo targetB = AccessTools.Field(typeof(Job), nameof(Job.targetB));
            bool sawIterator = false;
            bool nreStillAlive = targetB != null &&
                IteratorLoadsField(driver, targetB, out sawIterator);
            if (targetB == null || !sawIterator || nreStillAlive)
            {
                seedsPleaseSowPartial = true;
                return;
            }

            seedsPleaseSowLeftover = true;
        }

        static bool IteratorLoadsField(Type driver, FieldInfo field, out bool sawIterator)
        {
            sawIterator = false;
            if (driver == null || field == null)
            {
                return false;
            }

            bool loaded = false;
            foreach (Type nested in driver.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (nested == null || nested.Name == null ||
                    nested.Name.IndexOf("MakeNewToils", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                MethodInfo moveNext = AccessTools.Method(nested, "MoveNext");
                if (moveNext == null)
                {
                    continue;
                }

                sawIterator = true;
                if (MethodLoadsField(moveNext, field))
                {
                    loaded = true;
                }
            }

            return loaded;
        }

        static bool IteratorCallsRangeInclusive(Type driver)
        {
            foreach (Type nested in driver.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (nested == null || nested.Name == null ||
                    nested.Name.IndexOf("MakeNewToils", StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                MethodInfo moveNext = AccessTools.Method(nested, "MoveNext");
                if (MethodCalls(moveNext, typeof(Rand), "RangeInclusive"))
                {
                    return true;
                }
            }

            MethodInfo makeNewToils = AccessTools.Method(driver, "MakeNewToils");
            return MethodCalls(makeNewToils, typeof(Rand), "RangeInclusive");
        }

        /// <summary>
        /// Closed when SK already keeps stuffed material to CanMake stuff without random or
        /// ingredients[0]. Partial when that Prefix exists and still uses those picks. Alive
        /// when the Prefix type is missing or has no CanMake.
        ///
        /// Closed, если SK уже оставляет stuffed-материал CanMake stuff без random и
        /// ingredients[0]. Partial, если Prefix есть и всё ещё так выбирает. Alive, если типа
        /// Prefix нет или в нём нет CanMake.
        /// </summary>
        static void ProbeCraftStuffBucket()
        {
            Type prefixType = AccessTools.TypeByName(SkDominantIngredientPrefixTypeName);
            if (prefixType == null)
            {
                return;
            }

            bool canMake = TypeTreeCalls(prefixType, typeof(StuffProperties), "CanMake");
            if (!canMake)
            {
                return;
            }

            bool randomWeight = TypeTreeCalls(prefixType, typeof(GenCollection), "RandomElementByWeight")
                || TypeTreeCallsNamed(prefixType, "RandomElementByWeight");
            bool listIndexer = TypeTreeCallsListIndexer(prefixType);
            if (randomWeight || listIndexer)
            {
                craftStuffPartial = true;
                return;
            }

            craftStuffLeftover = true;
        }

        static void ProbeUfFillIfNeeded()
        {
            if (ufFillProbed)
            {
                return;
            }

            ufFillProbed = true;
            ufFillLeftover = false;
            ufFillPartial = false;
            ProbeUfFillBucket();
        }

        /// <summary>
        /// Closed when FillUF.CommitIngredients exists and no longer aborts with the extra-stack
        /// warning. Partial when that abort remains and the type tree calls RemainingCount or
        /// get_RemainingCount (the IngredientPortion property getter). Alive when the abort is
        /// present without that accounting, or commit/driver is missing (Unknown stays current).
        ///
        /// Closed, если FillUF.CommitIngredients есть и больше не рвёт работу предупреждением
        /// про лишние стаки. Partial, если abort остался и дерево типа зовёт RemainingCount или
        /// get_RemainingCount (геттер свойства IngredientPortion). Alive, если abort есть без
        /// этого учёта, либо нет commit/драйвера (Unknown остаётся актуальным).
        /// </summary>
        static void ProbeUfFillBucket()
        {
            Type fill = AccessTools.TypeByName(UfFillJobDriverTypeName)
                ?? AccessTools.TypeByName(UfFillJobDriverTypeNameAlt);
            if (fill == null)
            {
                return;
            }

            MethodInfo commit = AccessTools.Method(fill, "CommitIngredients");
            if (commit == null)
            {
                return;
            }

            if (!MethodContainsString(commit, UfFillAbortWithoutConsumingMarker))
            {
                ufFillLeftover = true;
                return;
            }

            if (TypeTreeCallsNamed(fill, "RemainingCount") ||
                TypeTreeCallsNamed(fill, "get_RemainingCount"))
            {
                ufFillPartial = true;
            }
        }

        static void ProbeAnomalyEspFleeIfNeeded()
        {
            if (anomalyEspProbed)
            {
                return;
            }

            anomalyEspProbed = true;
            anomalyEspLeftover = ProbeAnomalyEspFleeClosed();
        }

        static void ProbeMainMenuBgFitIfNeeded()
        {
            if (mainMenuBgFitProbed)
            {
                return;
            }

            mainMenuBgFitProbed = true;
            mainMenuBgFitLeftover = ProbeMainMenuBgFitClosed();
        }

        /// <summary>
        /// Closed when RimThemesLite UI_BackgroundMain_Patch.Prefix exists and no longer calls
        /// UnityEngine.Screen width or height. Alive when that Prefix still calls them. Unknown
        /// (not leftover) when RimThemesLite or the Prefix is absent.
        ///
        /// Closed, если Prefix RimThemesLite UI_BackgroundMain_Patch есть и больше не вызывает
        /// ширину или высоту UnityEngine.Screen. Alive, если Prefix всё ещё их вызывает.
        /// Unknown (не leftover), если RimThemesLite или Prefix нет.
        /// </summary>
        static bool ProbeMainMenuBgFitClosed()
        {
            Type patchType = AccessTools.TypeByName(RimThemesBackgroundPatchTypeName);
            MethodInfo prefix = patchType == null
                ? null
                : AccessTools.Method(patchType, "Prefix");
            if (prefix == null)
            {
                return false;
            }

            return !MainMenuBgFit_RimThemesVideoBgRect_Transpiler.PrefixUsesScreenDimensions(prefix);
        }

        /// <summary>
        /// Closed when MutantDef has no preventsMentalBreaks, or when Core_SK
        /// EnemySelfPreservationPatch or the ESP PostApplyDamage postfix already loads that field.
        /// Alive when the field exists and neither body loads it.
        ///
        /// Closed, если у MutantDef нет preventsMentalBreaks, либо Core_SK
        /// EnemySelfPreservationPatch или postfix ESP PostApplyDamage уже грузит это поле.
        /// Alive, если поле есть и его нет ни в одном из этих тел.
        /// </summary>
        static bool ProbeAnomalyEspFleeClosed()
        {
            FieldInfo preventsMentalBreaks = AccessTools.Field(typeof(MutantDef), "preventsMentalBreaks");
            if (preventsMentalBreaks == null)
            {
                return true;
            }

            Type sk = AccessTools.TypeByName(SkEnemySelfPreservationPatchTypeName);
            if (sk != null && TypeTreeLoadsField(sk, preventsMentalBreaks))
            {
                return true;
            }

            Type esp = AccessTools.TypeByName(EspPawnPostApplyDamageTypeName);
            if (esp == null)
            {
                return false;
            }

            MethodInfo postfix = AccessTools.Method(esp, "Postfix");
            return MethodLoadsField(postfix, preventsMentalBreaks);
        }

        static void ProbeDefBackedFixesIfNeeded()
        {
            if (defsProbed)
            {
                return;
            }

            if (DefDatabase<ThingDef>.DefCount == 0 || DefDatabase<RecipeDef>.DefCount == 0)
            {
                return;
            }

            defsProbed = true;
            BurnWeaponBillFixFeatures.EnsureOriginalsCaptured();
            burnWeaponLeftover = ProbeBurnWeaponClosed();
            BreachAxeWorkAmountFixFeatures.EnsureOriginalsCaptured();
            breachAxeLeftover = ProbeBreachAxeClosed();
            RawFungusBillFixFeatures.EnsureOriginalsCaptured();
            rawFungusLeftover = ProbeRawFungusClosed();
        }

        /// <summary>
        /// Closed when each burn recipe either has no requiredGiverWorkType or a DoBill WorkGiver
        /// with that work type covers a recipe user (haulers can start the bill). Alive when a
        /// required type is set and no matching DoBill remains.
        ///
        /// Closed, если у каждого рецепта сжигания нет requiredGiverWorkType либо есть DoBill с
        /// этим work type на пользователе рецепта (грузчик может взять задание). Alive, если тип
        /// задан и подходящего DoBill нет.
        /// </summary>
        static bool ProbeBurnWeaponClosed()
        {
            string[] names =
            {
                "BurnWeapon",
                "BurnApparel",
                "BurnDrugs",
            };
            bool sawRecipe = false;
            foreach (string defName in names)
            {
                RecipeDef recipe = DefDatabase<RecipeDef>.GetNamedSilentFail(defName);
                if (recipe == null)
                {
                    continue;
                }

                sawRecipe = true;
                WorkTypeDef required = BurnWeaponBillFixFeatures.GetCapturedRequiredGiver(defName);
                if (required == null)
                {
                    continue;
                }

                if (!HasMatchingDoBill(recipe, required))
                {
                    return false;
                }
            }

            return sawRecipe;
        }

        static bool HasMatchingDoBill(RecipeDef recipe, WorkTypeDef required)
        {
            foreach (ThingDef user in recipe.AllRecipeUsers)
            {
                if (user == null)
                {
                    continue;
                }

                foreach (WorkGiverDef wg in DefDatabase<WorkGiverDef>.AllDefsListForReading)
                {
                    if (wg == null || wg.giverClass == null || wg.workType != required)
                    {
                        continue;
                    }

                    if (!typeof(WorkGiver_DoBill).IsAssignableFrom(wg.giverClass))
                    {
                        continue;
                    }

                    if (WorkGiverCoversBuilding(wg, user))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool WorkGiverCoversBuilding(WorkGiverDef wg, ThingDef building)
        {
            if (wg.fixedBillGiverDefs == null || wg.fixedBillGiverDefs.Count == 0)
            {
                return false;
            }

            return wg.fixedBillGiverDefs.Contains(building);
        }

        /// <summary>
        /// Closed when captured WorkToMake is above the vanilla missing-stat default of 1.
        ///
        /// Closed, если сохранённый WorkToMake выше ванильного default 1 при отсутствии стата.
        /// </summary>
        static bool ProbeBreachAxeClosed()
        {
            float thingWork;
            if (!BreachAxeWorkAmountFixFeatures.TryGetCapturedThingWorkToMake(out thingWork))
            {
                return false;
            }

            return thingWork > BreachAxeMissingWorkToMakeMax;
        }

        /// <summary>
        /// Closed when captured RawFungus already belongs to FungusPlantRaw (HSK 1.5 overwrite
        /// or a later Unified fix). Alive when it is still only PlantFoodRaw.
        ///
        /// Closed, если сохранённый RawFungus уже в FungusPlantRaw (overwrite HSK 1.5 или
        /// поздний фикс Unified). Alive, если он всё ещё только в PlantFoodRaw.
        /// </summary>
        static bool ProbeRawFungusClosed()
        {
            return RawFungusBillFixFeatures.CapturedInFungusCategory();
        }

        /// <summary>
        /// Closed when GUIContent still has the instance field m_Text (Unity 1.5 and 1.6).
        /// Alive when that field is absent (RimWorld 1.7). The other missing readout calls
        /// are the same stub split, so this one field is the fingerprint.
        ///
        /// Closed, если у GUIContent ещё есть поле экземпляра m_Text (Unity 1.5 и 1.6).
        /// Alive, если поля нет (RimWorld 1.7). Остальные отсутствующие вызовы списка
        /// ресурсов из того же разрыва, поэтому отпечаток один.
        /// </summary>
        static void ProbeReadoutGuiIfNeeded()
        {
            if (readoutGuiProbed)
            {
                return;
            }

            readoutGuiProbed = true;
            readoutGuiLeftover = typeof(UnityEngine.GUIContent).GetField(
                "m_Text",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        static void LogOnce()
        {
            if (logged)
            {
                return;
            }

            if (!catCrazyTimeProbed || !craftStuffProbed || !seedsPleaseSowProbed ||
                !startingPawnNameProbed || !startingPawnChildAgeProbed ||
                !tribalLeaderProbed ||
                !ufFillProbed || !defsProbed || !anomalyEspProbed || !mainMenuBgFitProbed ||
                !readoutGuiProbed || !architectInfoBoxProbed || !rimatomicsHelProbed ||
                !diseaseComplicationProbed || !traitGainProbed ||
                !soldPawnExitProbed)
            {
                return;
            }

            logged = true;
            Log.Message(
                "[HSK kebab tweaks] Symptom leftover: CatCrazyTime=" + catCrazyTimeLeftover +
                " CraftStuff=" + craftStuffLeftover +
                " CraftStuffPartial=" + craftStuffPartial +
                " SeedsPleaseSow=" + seedsPleaseSowLeftover +
                " SeedsPleaseSowPartial=" + seedsPleaseSowPartial +
                " StartingPawnName=" + startingPawnNameLeftover +
                " StartingPawnChildAge=" + startingPawnChildAgeLeftover +
                " TribalLeader=" + tribalLeaderLeftover +
                " UfFill=" + ufFillLeftover +
                " UfFillPartial=" + ufFillPartial +
                " BurnWeapon=" + burnWeaponLeftover +
                " BreachAxe=" + breachAxeLeftover +
                " RawFungus=" + rawFungusLeftover +
                " AnomalyEspFlee=" + anomalyEspLeftover +
                " MainMenuBgFit=" + mainMenuBgFitLeftover +
                " ReadoutGui=" + readoutGuiLeftover +
                " ArchitectInfoBox=" + architectInfoBoxLeftover +
                " RimatomicsHel=" + rimatomicsHelLeftover +
                " DiseaseComplication=" + diseaseComplicationLeftover +
                " TraitGain=" + traitGainLeftover +
                " SoldPawnExit=" + soldPawnExitLeftover +
                " ArchitectSearch=" + architectSearchResearchLeftover + ".");
        }

        static bool MethodCallsName(MethodInfo method, string methodName)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || methodName == null)
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                byte op = il[i];
                if (op != 0x28 && op != 0x6F)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    MethodBase resolved = module.ResolveMethod(token);
                    if (resolved != null && resolved.Name == methodName)
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                }
            }

            return false;
        }

        static bool MethodCalls(MethodInfo method, Type declaringType, string methodName)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || declaringType == null || methodName == null)
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                byte op = il[i];
                if (op != 0x28 && op != 0x6F)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    MethodBase resolved = module.ResolveMethod(token);
                    if (resolved != null && resolved.Name == methodName &&
                        resolved.DeclaringType == declaringType)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        static bool MethodLoadsField(MethodInfo method, FieldInfo field)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || field == null)
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                byte op = il[i];
                if (op != 0x7B && op != 0x7C)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    FieldInfo resolved = module.ResolveField(token);
                    if (FieldMatches(resolved, field))
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        static bool FieldMatches(FieldInfo resolved, FieldInfo field)
        {
            if (resolved == null || field == null)
            {
                return false;
            }

            if (resolved == field)
            {
                return true;
            }

            return resolved.Name == field.Name && resolved.DeclaringType == field.DeclaringType;
        }

        static bool TypeTreeCalls(Type root, Type declaringType, string methodName)
        {
            bool found = false;
            ForEachDeclaredMethod(root, method =>
            {
                if (!found && MethodCalls(method, declaringType, methodName))
                {
                    found = true;
                }
            });
            return found;
        }

        static bool TypeTreeLoadsField(Type root, FieldInfo field)
        {
            bool found = false;
            ForEachDeclaredMethod(root, method =>
            {
                if (!found && MethodLoadsField(method, field))
                {
                    found = true;
                }
            });
            return found;
        }

        static bool TypeTreeCallsNamed(Type root, string methodName)
        {
            bool found = false;
            ForEachDeclaredMethod(root, method =>
            {
                if (!found && MethodCallsNamed(method, methodName))
                {
                    found = true;
                }
            });
            return found;
        }

        static bool TypeTreeCallsListIndexer(Type root)
        {
            bool found = false;
            ForEachDeclaredMethod(root, method =>
            {
                if (!found && MethodCallsListIndexer(method))
                {
                    found = true;
                }
            });
            return found;
        }

        static void ForEachDeclaredMethod(Type type, Action<MethodInfo> visit)
        {
            if (type == null || visit == null)
            {
                return;
            }

            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                BindingFlags.Static | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methods.Length; i++)
            {
                visit(methods[i]);
            }

            Type[] nested = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < nested.Length; i++)
            {
                ForEachDeclaredMethod(nested[i], visit);
            }
        }

        static bool MethodStoresZeroToField(MethodInfo method, string fieldName)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || fieldName == null)
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 1; i < il.Length - 4; i++)
            {
                if (il[i] != 0x7D || il[i - 1] != 0x16)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    FieldInfo field = module.ResolveField(token);
                    if (field != null && field.Name == fieldName)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        static bool MethodPassesFalseTo(MethodInfo method, string methodName)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || methodName == null)
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 1; i < il.Length - 4; i++)
            {
                byte op = il[i];
                if ((op != 0x28 && op != 0x6F) || il[i - 1] != 0x16)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    MethodBase resolved = module.ResolveMethod(token);
                    if (resolved != null && resolved.Name == methodName)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        static bool MethodCallsNamed(MethodInfo method, string methodName)
        {
            return MethodCallsWhere(method, resolved =>
                resolved != null && resolved.Name == methodName);
        }

        static bool MethodCallsListIndexer(MethodInfo method)
        {
            return MethodCallsWhere(method, resolved =>
            {
                if (resolved == null || resolved.Name != "get_Item")
                {
                    return false;
                }

                Type declaring = resolved.DeclaringType;
                return declaring != null && declaring.IsGenericType &&
                    declaring.GetGenericTypeDefinition() == typeof(List<>);
            });
        }

        static bool MethodCallsWhere(MethodInfo method, Func<MethodBase, bool> match)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || match == null)
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                byte op = il[i];
                if (op != 0x28 && op != 0x6F)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    MethodBase resolved = module.ResolveMethod(token);
                    if (match(resolved))
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        static bool MethodContainsString(MethodInfo method, string marker)
        {
            byte[] il = method?.GetMethodBody()?.GetILAsByteArray();
            if (il == null || string.IsNullOrEmpty(marker))
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                if (il[i] != 0x72)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    string value = module.ResolveString(token);
                    if (value != null &&
                        value.IndexOf(marker, StringComparison.Ordinal) >= 0)
                    {
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }
    }
}
