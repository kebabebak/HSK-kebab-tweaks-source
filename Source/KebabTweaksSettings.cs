using System;
using System.Collections.Generic;
using HSK.KebabTweaks.KebabSwitches;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Combined mod settings: kebab switches (no master enable) plus per-patch enable headers
    /// and each patch's own options / logging.
    ///
    /// Общие настройки мода: kebab switches (без master enable) и заголовки-enable патчей
    /// плюс собственные опции / логирование каждого патча.
    /// </summary>
    [StaticConstructorOnStartup]
    public class KebabTweaksSettings : ModSettings
    {
        public const float DefaultLowTpsThreshold = 100f;
        public const float DefaultLowTpsLogCooldownSeconds = 1f;
        public const int DefaultIdleWorkSearchCooldownTicks = 1000;
        public const int MaxIdleWorkSearchCooldownTicks = 100000;

#if RIMWORLD_1_6
        public static bool EnableCatCrazyTime;
#else
        public static bool EnableCatCrazyTime = true;
#endif
        public static bool EnableCatFloorSleep = true;
        public static bool EnableCeRunForCoverDestFix = true;
        public static bool EnableCeProjectileNullSoundFix = true;
        public static bool EnableCeExtendedLoadoutMedicineLabelFix = true;
        public static bool EnableArmorRacksAssignFix = true;
        public static bool EnableStorageSettingsAllowedToAcceptFix = true;
        public static bool EnableFishTableTypeListFix = true;
        public static bool EnableGetActiveRitualsFix = true;
        public static bool EnableGrowerCutTrees = true;
        public static bool EnableIdleErrorWanderFix = true;
        public static bool EnableHospitalityGuestApparelOptimizeFix = true;
        public static bool EnableLowTpsPawnDump = true;
        public static bool EnableIdleWorkSearchCooldown = true;
        public static bool EnableParanoidPerfectionist;
        public static bool ParanoidCatchCritical = true;
        public static bool ParanoidCatchAnyError;
        public static bool EnableDrillLimitPatch = true;
        public static bool EnableDrillLimitValueField = true;
        public static bool EnableDrillLimitAnyResource = true;
        public static bool EnableStartingCorpsesNoHaul = true;
        public static int StartingCorpseMarkMode;
        public static bool EnablePtgMedicalCare = true;
        public static bool EnableSeedsPleaseSowFix = true;
        public static bool EnableTakeFromMending = true;
        public static bool EnableTradeCaravanLordFix = true;
        public static bool EnableTraderCaravanStayFix = true;
        public static bool EnableRatkinMercenaryAgeFix = true;
        public static bool EnableCompressibleOverlapFix = true;
        public static bool EnableApparelPolicyLoadFix = true;
        public static bool EnableBillRenamePrefill = true;
        public static bool EnableRecipeProductDropLostLog = true;
        public static bool EnableNumbersLoadDefaultFallback = true;
        public static bool EnableAllowToolHaulUrgentlyNreFix = true;
        public static bool EnableDubsAnalyzerBeginUpdateFix = true;
        public static bool EnableDebugLogSplitterDragFix = true;
        public static bool EnableNeanderthalChiefLeaderFix = true;
        public static bool EnableDominantIngredientStuffFix = true;
        public static bool EnableStartingPawnChildAgeFix = true;
        public static bool EnableStartingPawnNameFix = true;
        public static bool EnableAndroidEnlightenFix = true;
        public static bool EnableArchitectSearchResearchPatch = true;
        public static bool EnableMonumentMaterialOnMapPatch = true;
        public static bool EnableObsoletePatches;
        public static bool DoNotResetObsoletePatchesOnReload;
        public static bool EnableWikiRimCompat = true;
        public static bool EnableWikiRimMultilingualSearch;
        public static bool EnableSaveSettingsLoadFix = true;
        public static bool EnableObsoleteFixes;
        public static bool DoNotResetObsoleteFixesOnReload;
        public static bool SymptomBucketDefaultsApplied;
        public static bool EnableMapPreviewRngBaselineFix = true;
        public static bool EnableUnifiedXmlPathFix = true;
        public static bool EnableMainMenuBgFitFix = true;
        public static bool EnableToggleableReadoutsGuiFix = true;
        public static bool EnableRimatomicsGuidancePanelFix = true;
        public static bool EnableUfFillExtraIngredientsFix = true;
        public static bool EnableUfFillCapPickupToBillCount = true;
        public static bool UfFillExtraIngredientsFixEnableLogging;
        public static bool EnableBurnWeaponBillFix;
        public static bool EnableBreachAxeWorkAmountFix;
        public static bool EnableRawFungusBillFix = true;
        public static bool EnableAnomalyEspFleeFix = true;
        public static bool EnableStratumSolarRoofEnergyTabFix = true;
        public static bool EnableArchitectInfoBoxFpsFix = true;
        public static bool EnableRimatomicsHelFpsFix = true;
        public static bool EnableDiseaseComplicationFix = true;
        public static bool EnableTraitGainFix = true;
        public static bool EnableSoldPawnExitGoodwillFix = true;

#if RIMWORLD_1_6
        public static bool AppliedCatCrazyTime;
#else
        public static bool AppliedCatCrazyTime = true;
#endif
        public static bool AppliedCatFloorSleep = true;
        public static bool AppliedCeRunForCoverDestFix = true;
        public static bool AppliedCeProjectileNullSoundFix = true;
        public static bool AppliedCeExtendedLoadoutMedicineLabelFix = true;
        public static bool AppliedArmorRacksAssignFix = true;
        public static bool AppliedStorageSettingsAllowedToAcceptFix = true;
        public static bool AppliedFishTableTypeListFix = true;
        public static bool AppliedGetActiveRitualsFix = true;
        public static bool AppliedGrowerCutTrees = true;
        public static bool AppliedIdleErrorWanderFix = true;
        public static bool AppliedHospitalityGuestApparelOptimizeFix = true;
        public static bool AppliedLowTpsPawnDump = true;
        public static bool AppliedIdleWorkSearchCooldown = true;
        public static bool AppliedParanoidPerfectionist;
        public static bool AppliedDrillLimitPatch = true;
        public static bool AppliedStartingCorpsesNoHaul = true;
        public static bool AppliedPtgMedicalCare = true;
        public static bool AppliedSeedsPleaseSowFix = true;
        public static bool AppliedTakeFromMending = true;
        public static bool AppliedTradeCaravanLordFix = true;
        public static bool AppliedTraderCaravanStayFix = true;
        public static bool AppliedRatkinMercenaryAgeFix = true;
        public static bool AppliedCompressibleOverlapFix = true;
        public static bool AppliedApparelPolicyLoadFix = true;
        public static bool AppliedBillRenamePrefill = true;
        public static bool AppliedRecipeProductDropLostLog = true;
        public static bool AppliedNumbersLoadDefaultFallback = true;
        public static bool AppliedAllowToolHaulUrgentlyNreFix = true;
        public static bool AppliedDubsAnalyzerBeginUpdateFix = true;
        public static bool AppliedDebugLogSplitterDragFix = true;
        public static bool AppliedNeanderthalChiefLeaderFix = true;
        public static bool AppliedDominantIngredientStuffFix = true;
        public static bool AppliedStartingPawnChildAgeFix = true;
        public static bool AppliedStartingPawnNameFix = true;
        public static bool AppliedAndroidEnlightenFix = true;
        public static bool AppliedArchitectSearchResearchPatch = true;
        public static bool AppliedMonumentMaterialOnMapPatch = true;
        public static bool AppliedWikiRimCompat = true;
        public static bool AppliedSaveSettingsLoadFix = true;
        public static bool AppliedMapPreviewRngBaselineFix = true;
        public static bool AppliedUnifiedXmlPathFix = true;
        public static bool AppliedMainMenuBgFitFix = true;
        public static bool AppliedToggleableReadoutsGuiFix = true;
        public static bool AppliedRimatomicsGuidancePanelFix = true;
        public static bool AppliedUfFillExtraIngredientsFix = true;
        public static bool AppliedBurnWeaponBillFix;
        public static bool AppliedBreachAxeWorkAmountFix;
        public static bool AppliedRawFungusBillFix;
        public static bool AppliedAnomalyEspFleeFix = true;
        public static bool AppliedStratumSolarRoofEnergyTabFix = true;
        public static bool AppliedArchitectInfoBoxFpsFix = true;
        public static bool AppliedRimatomicsHelFpsFix = true;
        public static bool AppliedDiseaseComplicationFix = true;
        public static bool AppliedTraitGainFix = true;
        public static bool AppliedSoldPawnExitGoodwillFix = true;

        public static bool CatCrazyTimeEnableLogging;
        public static bool CatFloorSleepEnableLogging;
        public static bool GrowerCutTreesEnableLogging;
        public static bool TakeFromMendingEnableLogging;
        public static bool TakeFromMendingShowMaintenanceMessages;
        public static bool TradeCaravanLordEnableLogging;
        public static bool CompressibleOverlapFixEnableLogging;

        public static bool LowTpsEnableLogging;
        public static bool LowTpsColonistsOnly;
        public static bool LowTpsSkipWhenPaused;
        public static bool LowTpsLogStartupMessage;
        public static float LowTpsThreshold = DefaultLowTpsThreshold;
        public static float LowTpsLogCooldownSeconds = DefaultLowTpsLogCooldownSeconds;
        public static string LowTpsThreshold_Buffer;
        public static string LowTpsLogCooldownSeconds_Buffer;

        public static bool IdleWorkSearchCooldownEnableLogging;
        public static int IdleWorkSearchCooldownTicks = DefaultIdleWorkSearchCooldownTicks;
        public static string IdleWorkSearchCooldownTicks_Buffer;

        public const int DefaultDrillLimitMax = 5000;
        public const int MaxDrillLimitMax = 10000000;
        public static int DrillLimitMax = DefaultDrillLimitMax;
        public static string DrillLimitMax_Buffer;

        public const int DefaultStartingCorpseMarkMode = 0;

        public static bool KebabSwitchesEnableLogging;
        public static bool SuppressFilledMapMessage;
        public static bool SuppressGaveBirthMessage;
        public static bool SuppressAnimalIsPregnantMessage;
        public static bool SuppressMiscarriedStarvationMessage;
        public static bool SuppressMiscarriedPoorHealthMessage;
        public static bool SuppressSeasonBegunMessage;
        public static bool SuppressBillCompleteMessage;
        public static bool SuppressFullyHealedMessage;
        public static bool SuppressSocialFightMessage;
        public static bool SuppressShipChunkDropMessage;
        public static bool SuppressStruckMineableMessage;
        public static bool SuppressBedLostAssignmentMessage;
        public static bool SuppressBedDestroyedMessage;
        public static bool SuppressSleepingPawnsWokenUpMessage;
        public static bool SuppressNoLongerSocialFightingMessage;
        public static bool SuppressNoLongerTargetedInsultMessage;
        public static bool SuppressInsultingSpreeEndedMessage;
        public static bool SuppressTargetedInsultStreamEndedMessage;
        public static bool SuppressFoodBingeEndedMessage;
        public static bool SuppressNoLongerBingingOnDrugMessage;
        public static bool SuppressBerserkEndedMessage;
        public static bool SuppressFireStartingEndedMessage;
        public static bool SuppressJailbreakerEndedMessage;
        public static bool SuppressSlaughtererEndedMessage;
        public static bool SuppressMurderousRageEndedMessage;
        public static bool SuppressGiveUpReturnedMessage;
        public static bool SuppressTantrumEndedMessage;
        public static bool SuppressRedirectedAngerMessage;
        public static bool SuppressSadisticRageEndedMessage;
        public static bool SuppressCorpseObsessionEndedMessage;
        public static bool SuppressWanderSadEndedMessage;
        public static bool SuppressHideInRoomEndedMessage;
        public static bool SuppressPsychoticWanderEndedMessage;
        public static bool SuppressConfusedEndedMessage;
        public static bool SuppressPanicFleeEndedMessage;
        public static bool SuppressManhunterEndedMessage;
        public static bool SuppressNewBondRelationMessage;
        public static bool SuppressNewBondRelationNewNameMessage;
        public static bool SuppressFoodPoisoningMessage;
        public static bool SuppressRoamerLeavingMessage;
        public static bool SuppressHiveReproducedMessage;
        public static bool SuppressTraderCaravanLeavingMessage;
        public static bool SuppressTraderCaravanDismissedMessage;
        public static bool SuppressCompSpawnerSpawnedItemMessage;
        public static bool SuppressFishCaughtInTrap;
        public static bool SuppressPlantDiedOfCold;
        public static bool SuppressPlantDiedOfRotUnharvested;
        public static bool SuppressPlantDiedOfRotLight;
        public static bool SuppressPlantDiedOfRot;
        public static bool SuppressPlantDiedOfPoison;
        public static bool SuppressPlantDiedOfBlight;
        public static bool SuppressPlantDiedOfPollution;
        public static bool SuppressPlantDiedOfNoPollution;
        public static bool SuppressPlantDiedOfRotPollutedTerrain;
#if RIMWORLD_1_6
        public static bool SuppressPlantDiedOfVacuum;
        public static bool SuppressEspInjuredMessage;
#endif
        public static bool SuppressMinifiedTreeDied;
        public static bool SuppressRottedAwayInStorage;
        public static bool SuppressDeterioratedAway;
        public static bool SuppressWornApparelDeterioratedAway;
        public static HashSet<string> SuppressLetterIds = new HashSet<string>(StringComparer.Ordinal);
        public static HashSet<string> SuppressAlertIds = new HashSet<string>(StringComparer.Ordinal);

        private static void EnsureSuppressLetterIds()
        {
            if (SuppressLetterIds == null)
            {
                SuppressLetterIds = new HashSet<string>(StringComparer.Ordinal);
            }
        }

        public static bool IsLetterSuppressed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            EnsureSuppressLetterIds();
            return SuppressLetterIds.Contains(id);
        }

        public static void SetLetterSuppressed(string id, bool suppress)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            EnsureSuppressLetterIds();
            if (suppress)
            {
                SuppressLetterIds.Add(id);
            }
            else
            {
                SuppressLetterIds.Remove(id);
            }
        }

        public static int SuppressedLetterCount
        {
            get
            {
                EnsureSuppressLetterIds();
                return SuppressLetterIds.Count;
            }
        }

        private static void EnsureSuppressAlertIds()
        {
            if (SuppressAlertIds == null)
            {
                SuppressAlertIds = new HashSet<string>(StringComparer.Ordinal);
            }
        }

        public static bool IsAlertSuppressed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            EnsureSuppressAlertIds();
            return SuppressAlertIds.Contains(id);
        }

        public static void SetAlertSuppressed(string id, bool suppress)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            EnsureSuppressAlertIds();
            if (suppress)
            {
                SuppressAlertIds.Add(id);
            }
            else
            {
                SuppressAlertIds.Remove(id);
            }
        }

        public static int SuppressedAlertCount
        {
            get
            {
                EnsureSuppressAlertIds();
                return SuppressAlertIds.Count;
            }
        }

        public static bool IsObsoleteFixEnabled(bool enableFlag)
        {
            return EnableObsoleteFixes && enableFlag;
        }

        /// <summary>
        /// True when the fix may change the game. A missing dependency blocks it even when
        /// the Obsolete section is on and the leftover checkbox is on. The row stays under
        /// Obsolete.
        ///
        /// True, когда фикс может менять игру. Недостающая зависимость блокирует его, даже
        /// если раздел «Устаревшие» включён и leftover-чекбокс тоже. Строка остаётся в
        /// «Устаревших».
        /// </summary>
        static bool IsFixRunning(bool enableFlag, bool leftover, string availabilityId)
        {
            if (FeatureAvailability.IsBlocked(availabilityId))
            {
                return false;
            }

            if (leftover)
            {
                return IsObsoleteFixEnabled(enableFlag);
            }

            return enableFlag;
        }

        /// <summary>
        /// True when a Patches-tab hook should run. Leftover patches use the Patches obsolete
        /// section, separate from the Fixes section.
        ///
        /// True, когда хук вкладки «Патчи» должен работать. Leftover-патчи используют раздел
        /// «Устаревшие» на вкладке патчей, отдельно от раздела фиксов.
        /// </summary>
        static bool IsPatchRunning(bool enableFlag, bool leftover, string availabilityId)
        {
            if (FeatureAvailability.IsBlocked(availabilityId))
            {
                return false;
            }

            if (leftover)
            {
                return EnableObsoletePatches && enableFlag;
            }

            return enableFlag;
        }

        /// <summary>
        /// True when architect search should ignore projects the player cannot see.
        ///
        /// True, когда поиск архитектора должен пропускать проекты, которых игрок не видит.
        /// </summary>
        public static bool IsArchitectSearchResearchPatchEnabled()
        {
            return IsPatchRunning(
                EnableArchitectSearchResearchPatch,
                FixSymptomProbe.IsArchitectSearchResearchPatchLeftover(),
                null);
        }

        /// <summary>
        /// True when growing-zone cuts should run as farmer sub-work before sowing.
        ///
        /// True, когда рубка зоны выращивания должна идти подработой фермера до посева.
        /// </summary>
        public static bool IsGrowerCutTreesPatchEnabled()
        {
            return IsPatchRunning(
                EnableGrowerCutTrees,
                FixSymptomProbe.IsGrowerCutTreesPatchLeftover(),
                FeatureAvailability.Grower);
        }

        /// <summary>
        /// True when monument material choices should hide options with nothing on the map.
        ///
        /// True, когда выбор материалов монумента должен скрывать варианты, которых нет на карте.
        /// </summary>
        public static bool IsMonumentMaterialOnMapPatchEnabled()
        {
            return IsFixRunning(EnableMonumentMaterialOnMapPatch, false, FeatureAvailability.MonumentMaterials);
        }

        /// <summary>
        /// True when Cat Crazy Time hooks should run: current enable when the load-reroll
        /// symptom is still present; Obsolete section plus leftover enable when SK already
        /// keeps saved num.
        ///
        /// True, когда хуки Cat Crazy Time должны работать: обычный enable, если реролл при
        /// load ещё есть; раздел «Устаревшие» плюс leftover enable, если SK уже хранит num.
        /// </summary>
        public static bool IsCatCrazyTimeEnabled()
        {
            return IsFixRunning(
                EnableCatCrazyTime,
                FixSymptomProbe.IsCatCrazyTimeLeftover(),
                FeatureAvailability.CatCrazyTime);
        }

        /// <summary>
        /// True when SeedsPlease Sow hooks should run: current enable while hop or TargetB NRE
        /// is still present; Obsolete section plus leftover enable when Seeds.dll already
        /// closed both.
        ///
        /// True, когда хуки SeedsPlease Sow должны работать: обычный enable, пока hop или
        /// TargetB NRE ещё есть; раздел «Устаревшие» плюс leftover enable, если Seeds.dll уже
        /// закрыл оба.
        /// </summary>
        public static bool IsSeedsPleaseSowFixEnabled()
        {
            return IsFixRunning(
                EnableSeedsPleaseSowFix,
                FixSymptomProbe.IsSeedsPleaseSowFixLeftover(),
                FeatureAvailability.SeedsPlease);
        }

        /// <summary>
        /// True when Craft Stuff hooks should run: current enable while SK still lacks a full
        /// CanMake max-stack pick; Obsolete section plus leftover enable when SK already closed
        /// that remainder.
        ///
        /// True, когда хуки Craft Stuff должны работать: обычный enable, пока SK ещё не берёт
        /// CanMake по max stack; раздел «Устаревшие» плюс leftover enable, если SK уже закрыл
        /// этот остаток.
        /// </summary>
        public static bool IsDominantIngredientStuffFixEnabled()
        {
            return IsFixRunning(
                EnableDominantIngredientStuffFix,
                FixSymptomProbe.IsCraftStuffFixLeftover(),
                FeatureAvailability.CraftStuff);
        }

        /// <summary>
        /// True when Starting Pawn Name hooks should run: current enable while
        /// NamerPersonCorunanEN still has a single-token r_name/name rule; Obsolete section
        /// plus leftover enable when every such rule already has a space or quotes.
        ///
        /// True, когда хуки Starting Pawn Name должны работать: обычный enable, пока у
        /// NamerPersonCorunanEN ещё есть однословное r_name/name; раздел «Устаревшие» плюс
        /// leftover enable, если у всех таких правил уже есть пробел или кавычки.
        /// </summary>
        public static bool IsStartingPawnNameFixEnabled()
        {
            return IsFixRunning(
                EnableStartingPawnNameFix,
                FixSymptomProbe.IsStartingPawnNameFixLeftover(),
                FeatureAvailability.StartingPawnName);
        }

        /// <summary>
        /// True when Starting Pawn Child Age hooks should run: current enable while
        /// SK.PawnGeneratorPatches does not yet store FixedBiologicalAge; Obsolete section
        /// plus leftover enable when that prefix already does.
        ///
        /// True, когда хуки Starting Pawn Child Age должны работать: обычный enable, пока
        /// SK.PawnGeneratorPatches ещё не пишет FixedBiologicalAge; раздел «Устаревшие» плюс
        /// leftover enable, если этот prefix уже пишет.
        /// </summary>
        public static bool IsStartingPawnChildAgeFixEnabled()
        {
            return IsFixRunning(
                EnableStartingPawnChildAgeFix,
                FixSymptomProbe.IsStartingPawnChildAgeFixLeftover(),
                FeatureAvailability.StartingPawnChildAge);
        }

        /// <summary>
        /// True when Tribal Leader hooks should run: current enable while
        /// SK.FactionLeaderGenerationPatches is absent; Obsolete section plus leftover
        /// enable when that Core_SK guard is loaded. XML patches are not this flag.
        ///
        /// True, когда хуки Tribal Leader должны работать: обычный enable, пока нет
        /// SK.FactionLeaderGenerationPatches; раздел «Устаревшие» плюс leftover enable,
        /// если этот guard Core_SK загружен. XML-патчи этим флагом не управляются.
        /// </summary>
        public static bool IsNeanderthalChiefLeaderFixEnabled()
        {
            return IsFixRunning(
                EnableNeanderthalChiefLeaderFix,
                FixSymptomProbe.IsNeanderthalChiefLeaderFixLeftover(),
                FeatureAvailability.TribalLeader);
        }

        /// <summary>
        /// True when Architect Info Box hooks should run: current enable while
        /// SK.Patch_ArchitectCategoryTab_DoInfoBox is absent; Obsolete section plus leftover
        /// enable when that Core_SK cache is loaded.
        ///
        /// True, когда хуки Architect Info Box должны работать: обычный enable, пока нет
        /// SK.Patch_ArchitectCategoryTab_DoInfoBox; раздел «Устаревшие» плюс leftover enable,
        /// если этот кеш Core_SK загружен.
        /// </summary>
        public static bool IsArchitectInfoBoxFpsFixEnabled()
        {
            return IsFixRunning(
                EnableArchitectInfoBoxFpsFix,
                FixSymptomProbe.IsArchitectInfoBoxFpsFixLeftover(),
                null);
        }

        /// <summary>
        /// True when the high-energy laser hooks should run: current enable while
        /// SK.Rimatomic_BetterHEL.TryFindNewTarget still walks every map thing; Obsolete
        /// section plus leftover enable when that walk is already gone.
        ///
        /// True, когда хуки высокоэнергетического лазера должны работать: обычный enable, пока
        /// SK.Rimatomic_BetterHEL.TryFindNewTarget ещё обходит все вещи карты; раздел «Устаревшие»
        /// плюс leftover enable, если этого обхода уже нет.
        /// </summary>
        public static bool IsRimatomicsHelFpsFixEnabled()
        {
            return IsFixRunning(
                EnableRimatomicsHelFpsFix,
                FixSymptomProbe.IsRimatomicsHelFpsFixLeftover(),
                FeatureAvailability.RimatomicsHel);
        }

        /// <summary>
        /// True when the disease complication hook should run: current enable while IsReady still
        /// passes the per-tick argument; Obsolete section plus leftover enable when it already
        /// passes the current severity.
        ///
        /// True, когда хук осложнений болезни должен работать: обычный enable, пока IsReady ещё
        /// передаёт аргумент за тик; раздел «Устаревшие» плюс leftover enable, если уже передаётся
        /// текущая тяжесть.
        /// </summary>
        public static bool IsDiseaseComplicationFixEnabled()
        {
            return IsFixRunning(
                EnableDiseaseComplicationFix,
                FixSymptomProbe.IsDiseaseComplicationFixLeftover(),
                FeatureAvailability.DiseaseComplication);
        }

        /// <summary>
        /// True when trait gain should run: current enable while both upstream counters are
        /// absent; Obsolete section plus leftover enable when SK.TraitChangerMapComponent or
        /// HSK.TraitGrowth.TraitGrowthMapComponent already counts cannibalism every 1200 ticks
        /// to 2200. The tick still does nothing while either counter is loaded.
        ///
        /// True, когда выдача черт должна работать: обычный enable, пока нет обоих счётчиков;
        /// раздел «Устаревшие» плюс leftover enable, если SK.TraitChangerMapComponent или
        /// HSK.TraitGrowth.TraitGrowthMapComponent уже считает каннибализм каждые 1200 тиков
        /// до 2200. Тик всё равно ничего не делает, пока загружен любой из них.
        /// </summary>
        public static bool IsTraitGainFixEnabled()
        {
            return IsFixRunning(
                EnableTraitGainFix,
                FixSymptomProbe.IsTraitGainFixLeftover(),
                FeatureAvailability.TraitGain);
        }

        /// <summary>
        /// True when the sold-pawn relations hook should run: current enable while
        /// SK.Patch_SoldPawnExitGoodwill does not yet clear the tend count; Obsolete section
        /// plus leftover enable when that Core_SK postfix already does.
        ///
        /// True, когда хук отношений за проданную пешку должен работать: обычный enable, пока
        /// SK.Patch_SoldPawnExitGoodwill ещё не обнуляет счётчик лечений; раздел «Устаревшие»
        /// плюс leftover enable, если этот postfix Core_SK уже обнуляет.
        /// </summary>
        public static bool IsSoldPawnExitGoodwillFixEnabled()
        {
            return IsFixRunning(
                EnableSoldPawnExitGoodwillFix,
                FixSymptomProbe.IsSoldPawnExitGoodwillFixLeftover(),
                null);
        }

        public static bool IsBurnWeaponBillFixEnabled()
        {
            return IsFixRunning(
                EnableBurnWeaponBillFix,
                FixSymptomProbe.IsBurnWeaponBillFixLeftover(),
                FeatureAvailability.BurnWeapon);
        }

        public static bool IsBreachAxeWorkAmountFixEnabled()
        {
            return IsFixRunning(
                EnableBreachAxeWorkAmountFix,
                FixSymptomProbe.IsBreachAxeWorkAmountFixLeftover(),
                FeatureAvailability.BreachAxe);
        }

        public static bool IsRawFungusBillFixEnabled()
        {
            return IsFixRunning(
                EnableRawFungusBillFix,
                FixSymptomProbe.IsRawFungusBillFixLeftover(),
                FeatureAvailability.RawFungus);
        }

        /// <summary>
        /// True when Anomaly Injured Fleeing hooks should run: current enable while ESP still
        /// starts SelfPreservation on mutants that prevent mental breaks; Obsolete section plus
        /// leftover enable when Core_SK or ESP already skip those pawns.
        ///
        /// True, когда хуки Anomaly Injured Fleeing должны работать: обычный enable, пока ESP ещё
        /// стартует SelfPreservation на мутантах без mental breaks; раздел «Устаревшие» плюс
        /// leftover enable, если Core_SK или ESP уже пропускают этих пешек.
        /// </summary>
        public static bool IsAnomalyEspFleeFixEnabled()
        {
            return IsFixRunning(
                EnableAnomalyEspFleeFix,
                FixSymptomProbe.IsAnomalyEspFleeFixLeftover(),
                FeatureAvailability.AnomalyEsp);
        }

        /// <summary>
        /// True when the main-menu background fit transpiler should run: current enable while
        /// RimThemesLite Prefix still calls Screen width or height, or that Prefix is absent.
        /// Obsolete section plus leftover enable when the Prefix exists and no longer calls them.
        ///
        /// True, когда транспайлер фона главного меню должен ставиться: обычный enable, пока
        /// Prefix RimThemesLite ещё вызывает ширину или высоту Screen, или этого Prefix нет.
        /// Раздел «Устаревшие» плюс leftover enable, если Prefix есть и этого больше не делает.
        /// </summary>
        public static bool IsMainMenuBgFitFixEnabled()
        {
            return IsFixRunning(
                EnableMainMenuBgFitFix,
                FixSymptomProbe.IsMainMenuBgFitFixLeftover(),
                FeatureAvailability.MainMenuBg);
        }

        /// <summary>
        /// True when the resource-list copies should run: current enable while GUIContent has no
        /// m_Text field. Obsolete section plus leftover enable when that field is present.
        ///
        /// True, когда копии списка ресурсов должны ставиться: обычный enable, пока у GUIContent
        /// нет поля m_Text. Раздел «Устаревшие» плюс leftover enable, если это поле есть.
        /// </summary>
        public static bool IsToggleableReadoutsGuiFixEnabled()
        {
            return IsFixRunning(
                EnableToggleableReadoutsGuiFix,
                FixSymptomProbe.IsToggleableReadoutsGuiFixLeftover(),
                FeatureAvailability.ToggleableReadouts);
        }

        /// <summary>
        /// True when Fermenter Fill hooks should run: current enable while CommitIngredients still
        /// aborts extra stacks; Obsolete section plus leftover enable when that abort is gone.
        ///
        /// True, когда хуки Fermenter Fill должны работать: обычный enable, пока CommitIngredients
        /// ещё рвёт лишние стаки; раздел «Устаревшие» плюс leftover enable, если этого abort уже нет.
        /// </summary>
        public static bool IsUfFillExtraIngredientsFixEnabled()
        {
            return IsFixRunning(
                EnableUfFillExtraIngredientsFix,
                FixSymptomProbe.IsUfFillFixLeftover(),
                FeatureAvailability.UfFill);
        }

        public static bool HasAnyObsoleteLeftover()
        {
            return FixSymptomProbe.HasAnyLeftover();
        }

        private const float SettingsCheckboxRowHeight = 32f;
        private const float FeatureHeaderLeadingSpacerHeight = SettingsCheckboxRowHeight * 0.5f;
        private const float ObsoleteHeaderLeadingSpacerHeight = FeatureHeaderLeadingSpacerHeight * 3f;
        private const float SettingsCheckboxRowGap = 2f;
        private const float SettingsSectionHeaderHeight = 28f;
        private const float SettingsScrollBarWidth = 16f;
        private const float CheckboxControlSize = 24f;
        private const float FeatureResetButtonWidth = 80f;
        private const float FeatureResetButtonGap = 4f;
        private const float FullResetButtonHeight = 30f;
        private const float FullResetButtonWidth = 280f;
        private const float FeatureBodyContentInset = 4f;
        private const float HeaderControlsRightInset = 4f;
        private const float UnwrappedLabelWidthPad = 2f;
        private const float SettingsTabsHeight = 32f;
        private const float NonDefaultUnderlineThickness = 1f;

        private enum SettingsTabKind
        {
            Notifications,
            Patches,
            Fixes,
        }

        private static readonly Color SettingsRowSeparatorColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);
        private static readonly Color FeatureBodyPanelFillColor = new Color(0f, 0f, 0f, 0.22f);
        private static readonly Color SettingsTabFillColor = new Color(0f, 0f, 0f, 0.8f);
        private static readonly Color SettingsTabSelectedFillColor = new Color(0.32f, 0.32f, 0.36f, 0.88f);
        private static readonly Color SettingsTabSelectedGlowColor = new Color(0.85f, 0.88f, 1f, 0.95f);
        private static readonly Color RestartPendingOutlineColor = new Color(0.95f, 0.85f, 0.15f);
        private static readonly Color ModeDigitActiveColor = new Color(0.25f, 0.9f, 0.25f);
        private static readonly Color ModeDigitInactiveColor = Color.white;
        private static readonly Color ModeDigitDisabledColor = Color.white;
        private static readonly Color SupersededHeaderColor = new Color(0.95f, 0.25f, 0.25f);
        private static readonly Color NonDefaultUnderlineColor = new Color(0.45f, 0.78f, 1f);
        private static readonly Color SettingsDisabledVeilColor = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color RelevancePercentPositiveColor = new Color(1f, 0.85f, 0.15f);
        private static readonly Color RelevancePercentZeroColor = new Color(0.95f, 0.22f, 0.22f);

        private const int CatCrazyTimeFixRelevancePercent = 0;
        private const int ObsoleteFixRelevancePercent = 0;

        private static Texture2D featureHeaderAlphaGradientTex;

        private readonly List<float> featureBodyPanelHeightCachesNotifications = new List<float>();
        private readonly List<float> featureBodyPanelHeightCachesPatches = new List<float>();
        private readonly List<float> featureBodyPanelHeightCachesFixes = new List<float>();
        private List<float> featureBodyPanelHeightCaches;
        private int featureBodyPanelDrawIndex;
        private static float bodyContentInset;
        private static bool bodyContentInteractive = true;

        private SettingsTabKind selectedSettingsTab = SettingsTabKind.Notifications;
        private Vector2 settingsScrollPositionNotifications;
        private Vector2 settingsScrollPositionPatches;
        private Vector2 settingsScrollPositionFixes;
        private static readonly QuickSearchWidget NotificationsSearchWidget = new QuickSearchWidget();
        private const float SettingsListingCanvasHeightMin = 8000f;

        private float scrollContentHeightNotifications;
        private float scrollContentHeightPatches = 500f;
        private float scrollContentHeightFixes = 1100f;
        private float obsoleteFixesSectionHeightCache;

        /// <summary>
        /// Same preset gradient as kebab limits modified-limit highlight colors.
        ///
        /// Тот же градиент, что у kebab limits для цвета изменённых лимитов.
        /// </summary>
        private static readonly Color[] BlockSeparatorGradientColors =
        {
            new Color(1f, 1f, 0.2f),
            new Color(1f, 0.55f, 0.1f),
            new Color(0.35f, 1f, 0.45f),
            new Color(0.45f, 0.75f, 1f),
            new Color(1f, 0.45f, 0.75f),
            new Color(0.9f, 0.9f, 0.9f),
        };

        public void DrawSettings(Rect inRect)
        {
            ClearAlwaysInteractiveRects();
            FixErrorTraceUi.BeginHoverFrame();
            FixErrorTraceUi.SetDrawBounds(inRect);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            Rect resetRow = new Rect(inRect.x, inRect.y, inRect.width, FullResetButtonHeight);
            DrawFullResetButton(resetRow, inRect.width);

            Rect tabBarRect = new Rect(
                inRect.x,
                inRect.y + FullResetButtonHeight + SettingsCheckboxRowGap,
                inRect.width,
                SettingsTabsHeight);
            DrawSettingsTabBar(tabBarRect);

            Rect tabContentRect = new Rect(
                inRect.x,
                tabBarRect.yMax,
                inRect.width,
                Mathf.Max(0f, inRect.yMax - tabBarRect.yMax));

            switch (selectedSettingsTab)
            {
                case SettingsTabKind.Patches:
                    DrawTabScrollContent(
                        tabContentRect,
                        ref settingsScrollPositionPatches,
                        ref scrollContentHeightPatches,
                        featureBodyPanelHeightCachesPatches,
                        DrawPatchesTabContents);
                    break;
                case SettingsTabKind.Fixes:
                    DrawTabScrollContent(
                        tabContentRect,
                        ref settingsScrollPositionFixes,
                        ref scrollContentHeightFixes,
                        featureBodyPanelHeightCachesFixes,
                        DrawFixesTabContents);
                    break;
                default:
                    DrawTabScrollContent(
                        tabContentRect,
                        ref settingsScrollPositionNotifications,
                        ref scrollContentHeightNotifications,
                        featureBodyPanelHeightCachesNotifications,
                        DrawNotificationsTabContents,
                        EstimateNotificationsListingCanvasHeight());
                    break;
            }

            FixErrorTraceUi.DrawHoverPanelIfNeeded();
        }

        /// <summary>
        /// Draws the three settings tabs.
        ///
        /// Рисует три вкладки настроек.
        /// </summary>
        private void DrawSettingsTabBar(Rect tabBarRect)
        {
            float tabWidth = tabBarRect.width / 3f;
            DrawOneSettingsTab(
                new Rect(tabBarRect.x, tabBarRect.y, tabWidth, tabBarRect.height),
                "KebabTweaks.Tab.Notifications".Translate(),
                SettingsTabKind.Notifications);
            DrawOneSettingsTab(
                new Rect(tabBarRect.x + tabWidth, tabBarRect.y, tabWidth, tabBarRect.height),
                "KebabTweaks.Tab.Patches".Translate(),
                SettingsTabKind.Patches);
            DrawOneSettingsTab(
                new Rect(tabBarRect.x + tabWidth * 2f, tabBarRect.y, tabWidth, tabBarRect.height),
                "KebabTweaks.Tab.Fixes".Translate(),
                SettingsTabKind.Fixes);
        }

        private void DrawOneSettingsTab(Rect tabRect, string label, SettingsTabKind kind)
        {
            bool selected = selectedSettingsTab == kind;
            if (Event.current.type == EventType.Repaint)
            {
                Color previous = GUI.color;
                GUI.color = selected ? SettingsTabSelectedFillColor : SettingsTabFillColor;
                GUI.DrawTexture(tabRect, BaseContent.WhiteTex);
                if (selected)
                {
                    DrawInnerBoxBorder(tabRect, 2f, SettingsTabSelectedGlowColor);
                    GUI.color = SettingsTabSelectedGlowColor;
                    GUI.DrawTexture(new Rect(tabRect.x, tabRect.yMax - 2f, tabRect.width, 2f), BaseContent.WhiteTex);
                }
                else
                {
                    GUI.color = SettingsRowSeparatorColor;
                    GUI.DrawTexture(new Rect(tabRect.x, tabRect.yMax - 1f, tabRect.width, 1f), BaseContent.WhiteTex);
                    GUI.DrawTexture(new Rect(tabRect.xMax - 1f, tabRect.y, 1f, tabRect.height), BaseContent.WhiteTex);
                }

                GUI.color = previous;
            }

            if (Widgets.ButtonInvisible(tabRect))
            {
                selectedSettingsTab = kind;
            }

            TextAnchor previousAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            DrawUnwrappedLabel(tabRect, label);
            Text.Anchor = previousAnchor;
        }

        private static float EstimateNotificationsListingCanvasHeight()
        {
            int checkboxRows =
                SuppressibleScreenMessages.All.Length
                + SuppressibleLetters.All.Length
                + SuppressibleAlerts.All.Length;
            const float overhead = 480f;
            float rowSlot = SettingsCheckboxRowHeight + SettingsCheckboxRowGap + 2f;
            return Mathf.Max(
                SettingsListingCanvasHeightMin,
                overhead + checkboxRows * rowSlot * 1.35f);
        }

        private void DrawTabScrollContent(
            Rect tabContentRect,
            ref Vector2 scrollPosition,
            ref float contentHeight,
            List<float> panelHeightCaches,
            Action<Listing_Standard, float> drawContents,
            float listingCanvasMin = SettingsListingCanvasHeightMin)
        {
            featureBodyPanelHeightCaches = panelHeightCaches;
            featureBodyPanelDrawIndex = 0;

            float viewWidth = tabContentRect.width - SettingsScrollBarWidth;
            float scrollHeight = Mathf.Max(contentHeight, tabContentRect.height);
            Rect viewRect = new Rect(0f, 0f, viewWidth, scrollHeight);
            Widgets.BeginScrollView(tabContentRect, ref scrollPosition, viewRect);
            FixErrorTraceUi.SetScrollContext(tabContentRect, scrollPosition);

            Listing_Standard listing = new Listing_Standard();
            listing.ColumnWidth = viewWidth;
            // Listing_Standard stops advancing CurHeight past the Begin() height - size canvas to catalog.
            float canvasHeight = Mathf.Max(listingCanvasMin, scrollHeight);
            listing.Begin(new Rect(0f, 0f, viewWidth, canvasHeight));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            drawContents(listing, viewWidth);
            float measured = listing.CurHeight + SettingsCheckboxRowGap;
            listing.End();
            Widgets.EndScrollView();

            if (measured > 1f)
            {
                contentHeight = Mathf.Max(measured, tabContentRect.height);
            }
        }

        private void DrawNotificationsTabContents(Listing_Standard listing, float fullWidth)
        {
            DrawKebabSwitchesBlock(listing, fullWidth);
        }

        private void DrawPatchesTabContents(Listing_Standard listing, float fullWidth)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.LowTpsPawnDump".Translate(),
                "KebabTweaks.Patch.LowTpsPawnDump.Tooltip".Translate(),
                SupersededStandaloneMods.LowTpsPawnDump,
                ref EnableLowTpsPawnDump, AppliedLowTpsPawnDump, true, false, ResetLowTpsPawnDump,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "LowTpsPawnDump.EnableLogging".Translate(),
                        () => LowTpsEnableLogging, v => LowTpsEnableLogging = v,
                        "LowTpsPawnDump.EnableLoggingTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing, "LowTpsPawnDump.ColonistsOnly".Translate(),
                        ref LowTpsColonistsOnly, false,
                        "LowTpsPawnDump.ColonistsOnlyTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing, "LowTpsPawnDump.SkipWhenPaused".Translate(),
                        ref LowTpsSkipWhenPaused, false,
                        "LowTpsPawnDump.SkipWhenPausedTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing, "LowTpsPawnDump.LogStartupMessage".Translate(),
                        ref LowTpsLogStartupMessage, false,
                        "LowTpsPawnDump.LogStartupMessageTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawBodyContentNumericFloatRow(
                        listing,
                        fullWidth,
                        "LowTpsPawnDump.TpsThreshold".Translate(),
                        ref LowTpsThreshold,
                        ref LowTpsThreshold_Buffer,
                        10f,
                        900f,
                        DefaultLowTpsThreshold);
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawBodyContentNumericFloatRow(
                        listing,
                        fullWidth,
                        "LowTpsPawnDump.LogCooldownSeconds".Translate(),
                        ref LowTpsLogCooldownSeconds,
                        ref LowTpsLogCooldownSeconds_Buffer,
                        0.25f,
                        10f,
                        DefaultLowTpsLogCooldownSeconds);
                    listing.Gap(SettingsCheckboxRowGap);
                },
                leadingSpacer: false,
                availabilityId: FeatureAvailability.LowTps);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.DrillLimit".Translate(),
                DrillLimitText.PatchTooltip(),
                null,
                ref EnableDrillLimitPatch, AppliedDrillLimitPatch, true, false,
                ResetDrillLimitPatch,
                () =>
                {
                    DrawSettingsCheckboxRow(
                        listing,
                        "DrillLimit.ValueField".Translate(),
                        ref EnableDrillLimitValueField,
                        true,
                        "DrillLimit.ValueFieldTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    bool anyResourceBefore = EnableDrillLimitAnyResource;
                    DrawSettingsCheckboxRow(
                        listing,
                        "DrillLimit.AnyResource".Translate(),
                        ref EnableDrillLimitAnyResource,
                        true,
                        "DrillLimit.AnyResourceTooltip".Translate());
                    if (anyResourceBefore && !EnableDrillLimitAnyResource)
                    {
                        DrillLimitState.ClearPins();
                    }

                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawBodyContentNumericIntRow(
                        listing,
                        fullWidth,
                        "DrillLimit.Max".Translate(),
                        ref DrillLimitMax,
                        ref DrillLimitMax_Buffer,
                        0,
                        MaxDrillLimitMax,
                        DefaultDrillLimitMax);

                    listing.Gap(SettingsCheckboxRowGap);
                },
                availabilityId: FeatureAvailability.DrillLimit);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.IdleWorkSearchCooldown".Translate(),
                "KebabTweaks.Patch.IdleWorkSearchCooldown.Tooltip".Translate(),
                null,
                ref EnableIdleWorkSearchCooldown, AppliedIdleWorkSearchCooldown, true, false,
                ResetIdleWorkSearchCooldown,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing,
                        "IdleWorkSearchCooldown.EnableLogging".Translate(),
                        () => IdleWorkSearchCooldownEnableLogging,
                        v => IdleWorkSearchCooldownEnableLogging = v,
                        "IdleWorkSearchCooldown.EnableLoggingTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawBodyContentNumericIntRow(
                        listing,
                        fullWidth,
                        "IdleWorkSearchCooldown.CooldownTicks".Translate(),
                        ref IdleWorkSearchCooldownTicks,
                        ref IdleWorkSearchCooldownTicks_Buffer,
                        0,
                        MaxIdleWorkSearchCooldownTicks,
                        DefaultIdleWorkSearchCooldownTicks);
                    listing.Gap(SettingsCheckboxRowGap);
                },
                availabilityId: FeatureAvailability.IdleWorkSearch);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.ParanoidPerfectionist".Translate(),
                "KebabTweaks.Patch.ParanoidPerfectionist.Tooltip".Translate(),
                null,
                ref EnableParanoidPerfectionist, AppliedParanoidPerfectionist, false, false,
                ResetParanoidPerfectionist,
                () =>
                {
                    DrawSettingsCheckboxRow(listing,
                        "ParanoidPerfectionist.CatchCritical".Translate(),
                        ref ParanoidCatchCritical,
                        true,
                        "ParanoidPerfectionist.CatchCriticalTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing,
                        "ParanoidPerfectionist.CatchAny".Translate(),
                        ref ParanoidCatchAnyError,
                        false,
                        "ParanoidPerfectionist.CatchAnyTooltip".Translate());
                });

            if (!FixSymptomProbe.IsGrowerCutTreesPatchLeftover())
            {
                DrawGrowerCutTreesPatchBlock(listing, fullWidth, leftover: false);
            }

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.StartingCorpsesNoHaul".Translate(),
                "KebabTweaks.Patch.StartingCorpsesNoHaul.Tooltip".Translate(),
                null,
                ref EnableStartingCorpsesNoHaul, AppliedStartingCorpsesNoHaul, true, false,
                ResetStartingCorpsesNoHaul,
                () =>
                {
                    DrawStartingCorpseMarkModeRow(listing);
                },
                researchLink: FeatureResearchLinkCatalog.StartingCorpsesNoHaul,
                availabilityId: FeatureAvailability.StartingCorpses);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.ApparelPolicyLoad".Translate(),
                "KebabTweaks.Patch.ApparelPolicyLoad.Tooltip".Translate(),
                null,
                ref EnableApparelPolicyLoadFix, AppliedApparelPolicyLoadFix, true, false, ResetApparelPolicyLoadFix,
                null,
                researchLink: FeatureResearchLinkCatalog.ApparelPolicyLoadFix,
                availabilityId: FeatureAvailability.ApparelPolicy);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.BillRenamePrefill".Translate(),
                "KebabTweaks.Patch.BillRenamePrefill.Tooltip".Translate(),
                null,
                ref EnableBillRenamePrefill, AppliedBillRenamePrefill, true, false, ResetBillRenamePrefill,
                null,
                availabilityId: FeatureAvailability.BillRename);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.RecipeProductDropLostLog".Translate(),
                "KebabTweaks.Patch.RecipeProductDropLostLog.Tooltip".Translate(),
                null,
                ref EnableRecipeProductDropLostLog, AppliedRecipeProductDropLostLog, true, false,
                ResetRecipeProductDropLostLog,
                null,
                availabilityId: FeatureAvailability.RecipeDrop);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.NumbersLoadDefaultFallback".Translate(),
                "KebabTweaks.Patch.NumbersLoadDefaultFallback.Tooltip".Translate(),
                null,
                ref EnableNumbersLoadDefaultFallback, AppliedNumbersLoadDefaultFallback, true, false,
                ResetNumbersLoadDefaultFallback,
                null,
                researchLink: FeatureResearchLinkCatalog.NumbersLoadDefaultFallback,
                availabilityId: FeatureAvailability.Numbers);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.AndroidEnlightenFix".Translate(),
                "KebabTweaks.Patch.AndroidEnlightenFix.Tooltip".Translate(),
                null,
                ref EnableAndroidEnlightenFix, AppliedAndroidEnlightenFix,
                true, false, ResetAndroidEnlightenFix,
                null,
                researchLink: FeatureResearchLinkCatalog.AndroidEnlightenFix,
                availabilityId: FeatureAvailability.AndroidEnlighten);
            AndroidEnlightenFixFeatures.SyncGivers();
            if (!FixSymptomProbe.IsArchitectSearchResearchPatchLeftover())
            {
                DrawArchitectSearchResearchPatchBlock(listing, fullWidth, leftover: false);
            }

            DrawMonumentMaterialOnMapPatchBlock(listing, fullWidth);
            DrawCompatibilityPatchesSection(listing, fullWidth);
            if (HasAnyObsoletePatchLeftover())
            {
                DrawObsoletePatchesSection(listing, fullWidth);
            }
        }

        static bool HasAnyObsoletePatchLeftover()
        {
            return FixSymptomProbe.IsArchitectSearchResearchPatchLeftover()
                || FixSymptomProbe.IsGrowerCutTreesPatchLeftover();
        }

        void DrawGrowerCutTreesPatchBlock(Listing_Standard listing, float fullWidth, bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.GrowerCutTrees".Translate(),
                "KebabTweaks.Patch.GrowerCutTrees.Tooltip".Translate(),
                SupersededStandaloneMods.GrowerCutTrees,
                ref EnableGrowerCutTrees, AppliedGrowerCutTrees,
                !leftover, false, ResetGrowerCutTrees,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "GrowerCutTreesPatch.EnableLogging".Translate(),
                        () => GrowerCutTreesEnableLogging, v => GrowerCutTreesEnableLogging = v);
                },
                leadingSpacer: true,
                interactive: leftover ? EnableObsoletePatches : true,
                relevancePercent: leftover ? 0 : (int?)null,
                availabilityId: FeatureAvailability.Grower);
        }

        void DrawArchitectSearchResearchPatchBlock(Listing_Standard listing, float fullWidth, bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.ArchitectSearchResearch".Translate(),
                "KebabTweaks.Patch.ArchitectSearchResearch.Tooltip".Translate(),
                null,
                ref EnableArchitectSearchResearchPatch, AppliedArchitectSearchResearchPatch,
                !leftover, false, ResetArchitectSearchResearchPatch,
                null,
                leadingSpacer: true,
                interactive: leftover ? EnableObsoletePatches : true,
                relevancePercent: leftover ? 0 : (int?)null);
        }

        void DrawMonumentMaterialOnMapPatchBlock(Listing_Standard listing, float fullWidth)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.MonumentMaterialOnMap".Translate(),
                "KebabTweaks.Patch.MonumentMaterialOnMap.Tooltip".Translate(),
                null,
                ref EnableMonumentMaterialOnMapPatch, AppliedMonumentMaterialOnMapPatch,
                true, false, ResetMonumentMaterialOnMapPatch,
                null,
                availabilityId: FeatureAvailability.MonumentMaterials);
        }

        void DrawObsoletePatchesSection(Listing_Standard listing, float fullWidth)
        {
            listing.GetRect(ObsoleteHeaderLeadingSpacerHeight);
            DrawObsoletePatchesEnableHeaderRow(listing);
            DrawSettingsRowSeparator(listing, fullWidth);
            DrawFeatureBodyPanel(
                listing,
                fullWidth,
                () =>
                {
                    DrawSettingsCheckboxRow(
                        listing,
                        "KebabTweaks.DoNotResetObsoleteFixesOnReload".Translate(),
                        ref DoNotResetObsoletePatchesOnReload,
                        false,
                        "KebabTweaks.DoNotResetObsoleteFixesOnReload.Tooltip".Translate());
                },
                interactive: true,
                drawRainbowTop: false);

            if (FixSymptomProbe.IsArchitectSearchResearchPatchLeftover())
            {
                DrawArchitectSearchResearchPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsGrowerCutTreesPatchLeftover())
            {
                DrawGrowerCutTreesPatchBlock(listing, fullWidth, leftover: true);
            }
        }

        void DrawObsoletePatchesEnableHeaderRow(Listing_Standard listing)
        {
            DrawPatchEnableHeaderRow(
                listing,
                "KebabTweaks.ObsoleteFixes".Translate(),
                null,
                null,
                ref EnableObsoletePatches,
                EnableObsoletePatches,
                false,
                false,
                ResetObsoletePatches,
                confirmEnable: true,
                confirmEnableAction: () => EnableObsoletePatches = true);
        }

        /// <summary>
        /// Compatibility section under the other patches. It has no enable of its own.
        ///
        /// Раздел совместимости под остальными патчами. Своего включения у раздела нет.
        /// </summary>
        private void DrawCompatibilityPatchesSection(Listing_Standard listing, float fullWidth)
        {
            listing.GetRect(ObsoleteHeaderLeadingSpacerHeight);
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            DrawFeatureHeaderBackground(row);
            DrawFeatureHeaderLabel(row, "KebabTweaks.CompatibilityPatches".Translate(), false, false);
            listing.Gap(SettingsCheckboxRowGap);
            DrawSettingsRowSeparator(listing, fullWidth);
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.WikiRim".Translate(),
                null,
                null,
                ref EnableWikiRimCompat, AppliedWikiRimCompat, true, false, ResetWikiRimCompat,
                () =>
                {
                    DrawSettingsCheckboxRow(
                        listing,
                        "KebabTweaks.WikiRim.MultilingualSearch".Translate(),
                        ref EnableWikiRimMultilingualSearch,
                        false,
                        "KebabTweaks.WikiRim.MultilingualSearch.Tooltip".Translate());
                },
                headerSuffix: " " + "KebabTweaks.Patch.WikiRim.PackageSuffix".Translate(),
                headerSuffixTooltip: "KebabTweaks.Patch.WikiRim.PackageSuffixTooltip".Translate(),
                availabilityId: FeatureAvailability.WikiRim);
        }

        private void DrawFixesTabContents(Listing_Standard listing, float fullWidth)
        {
            FixSymptomProbe.Ensure();
            bool firstHeaderOnTab = true;
            if (!FixSymptomProbe.IsUfFillFixLeftover())
            {
                DrawUfFillExtraIngredientsFixPatchBlock(listing, fullWidth, leftover: false,
                    leadingSpacer: !firstHeaderOnTab);
                firstHeaderOnTab = false;
            }

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.TakeFromMending".Translate(),
                "KebabTweaks.Patch.TakeFromMending.Tooltip".Translate(),
                SupersededStandaloneMods.TakeFromMending,
                ref EnableTakeFromMending, AppliedTakeFromMending, true, false, ResetTakeFromMending,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "TakeFromMendingPatch.EnableLogging".Translate(),
                        () => TakeFromMendingEnableLogging, v => TakeFromMendingEnableLogging = v,
                        "TakeFromMendingPatch.EnableLoggingTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing, "TakeFromMendingPatch.ShowMaintenanceMessages".Translate(),
                        ref TakeFromMendingShowMaintenanceMessages, false,
                        "TakeFromMendingPatch.ShowMaintenanceMessagesTooltip".Translate());
                },
                FixErrorTraceCatalog.TakeFromMending, FixErrorTraceCatalog.TakeFromMendingTipId,
                leadingSpacer: !firstHeaderOnTab,
                researchLink: FeatureResearchLinkCatalog.TakeFromMending,
                availabilityId: FeatureAvailability.TakeFromMending);
            firstHeaderOnTab = false;
            if (!FixSymptomProbe.IsCatCrazyTimeLeftover())
            {
                DrawCatCrazyTimePatchBlock(listing, fullWidth, leftover: false,
                    leadingSpacer: !firstHeaderOnTab);
                firstHeaderOnTab = false;
            }

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CatFloorSleep".Translate(),
                "KebabTweaks.Patch.CatFloorSleep.Tooltip".Translate(),
                SupersededStandaloneMods.CatFloorSleep,
                ref EnableCatFloorSleep, AppliedCatFloorSleep, true, false, ResetCatFloorSleep,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "CatFloorSleepPatch.EnableLogging".Translate(),
                        () => CatFloorSleepEnableLogging, v => CatFloorSleepEnableLogging = v,
                        "CatFloorSleepPatch.EnableLoggingTooltip".Translate());
                },
                FixErrorTraceCatalog.CatFloorSleep, FixErrorTraceCatalog.CatFloorSleepTipId,
                availabilityId: FeatureAvailability.CatFloorSleep);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.TradeCaravanLordFix".Translate(),
                "KebabTweaks.Patch.TradeCaravanLordFix.Tooltip".Translate(),
                SupersededStandaloneMods.TradeCaravanLordFix,
                ref EnableTradeCaravanLordFix, AppliedTradeCaravanLordFix, true, false, ResetTradeCaravanLordFix,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "TradeCaravanLordFixPatch.EnableLogging".Translate(),
                        () => TradeCaravanLordEnableLogging, v => TradeCaravanLordEnableLogging = v,
                        "TradeCaravanLordFixPatch.EnableLoggingTooltip".Translate());
                },
                FixErrorTraceCatalog.TradeCaravanLordFix, FixErrorTraceCatalog.TradeCaravanLordFixTipId,
                availabilityId: FeatureAvailability.TradeCaravan);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CompressibleOverlapFix".Translate(),
                "KebabTweaks.Patch.CompressibleOverlapFix.Tooltip".Translate(),
                null,
                ref EnableCompressibleOverlapFix, AppliedCompressibleOverlapFix, true, false,
                ResetCompressibleOverlapFix,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing,
                        "KebabTweaks.Patch.CompressibleOverlapFix.EnableLogging".Translate(),
                        () => CompressibleOverlapFixEnableLogging, v => CompressibleOverlapFixEnableLogging = v,
                        "KebabTweaks.Patch.CompressibleOverlapFix.EnableLoggingTooltip".Translate());
                },
                FixErrorTraceCatalog.CompressibleOverlapFix, FixErrorTraceCatalog.CompressibleOverlapFixTipId,
                researchLink: FeatureResearchLinkCatalog.CompressibleOverlapFix,
                availabilityId: FeatureAvailability.Compressible);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.TraderCaravanStayFix".Translate(),
                "KebabTweaks.Patch.TraderCaravanStayFix.Tooltip".Translate(),
                null,
                ref EnableTraderCaravanStayFix, AppliedTraderCaravanStayFix, true, false,
                ResetTraderCaravanStayFix,
                null,
                availabilityId: FeatureAvailability.TraderStay);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.RatkinMercenaryAgeFix".Translate(),
                "KebabTweaks.Patch.RatkinMercenaryAgeFix.Tooltip".Translate(),
                null,
                ref EnableRatkinMercenaryAgeFix, AppliedRatkinMercenaryAgeFix, true, false,
                ResetRatkinMercenaryAgeFix,
                null,
                FixErrorTraceCatalog.RatkinMercenaryAgeFix,
                FixErrorTraceCatalog.RatkinMercenaryAgeFixTipId,
                availabilityId: FeatureAvailability.Ratkin);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.ArmorRacksAssignFix".Translate(),
                "KebabTweaks.Patch.ArmorRacksAssignFix.Tooltip".Translate(),
                SupersededStandaloneMods.ArmorRacksAssignFix,
                ref EnableArmorRacksAssignFix, AppliedArmorRacksAssignFix, true, false, ResetArmorRacksAssignFix,
                null,
                FixErrorTraceCatalog.ArmorRacksAssignFix, FixErrorTraceCatalog.ArmorRacksAssignFixTipId,
                researchLink: FeatureResearchLinkCatalog.ArmorRacksAssignFix,
                availabilityId: FeatureAvailability.ArmorRacks);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.SaveSettingsLoadFix".Translate(),
                "KebabTweaks.Patch.SaveSettingsLoadFix.Tooltip".Translate(),
                null,
                ref EnableSaveSettingsLoadFix, AppliedSaveSettingsLoadFix,
                true, false, ResetSaveSettingsLoadFix,
                null,
                FixErrorTraceCatalog.SaveSettingsLoadFix,
                FixErrorTraceCatalog.SaveSettingsLoadFixTipId,
                researchLink: FeatureResearchLinkCatalog.SaveSettingsLoadFix,
                availabilityId: FeatureAvailability.SaveSettings);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.StorageSettingsAllowedToAcceptFix".Translate(),
                "KebabTweaks.Patch.StorageSettingsAllowedToAcceptFix.Tooltip".Translate(),
                null,
                ref EnableStorageSettingsAllowedToAcceptFix, AppliedStorageSettingsAllowedToAcceptFix,
                true, false, ResetStorageSettingsAllowedToAcceptFix,
                null,
                FixErrorTraceCatalog.StorageSettingsAllowedToAcceptFix,
                FixErrorTraceCatalog.StorageSettingsAllowedToAcceptFixTipId,
                availabilityId: FeatureAvailability.StorageSettings);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CeRunForCoverDestFix".Translate(),
                "KebabTweaks.Patch.CeRunForCoverDestFix.Tooltip".Translate(),
                SupersededStandaloneMods.CeRunForCoverDestFix,
                ref EnableCeRunForCoverDestFix, AppliedCeRunForCoverDestFix, true, false, ResetCeRunForCoverDestFix,
                null,
                FixErrorTraceCatalog.CeRunForCoverDestFix, FixErrorTraceCatalog.CeRunForCoverDestFixTipId,
                availabilityId: FeatureAvailability.CeRunForCover);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CeProjectileNullSoundFix".Translate(),
                "KebabTweaks.Patch.CeProjectileNullSoundFix.Tooltip".Translate(),
                null,
                ref EnableCeProjectileNullSoundFix, AppliedCeProjectileNullSoundFix, true, false,
                ResetCeProjectileNullSoundFix,
                null,
                FixErrorTraceCatalog.CeProjectileNullSoundFix,
                FixErrorTraceCatalog.CeProjectileNullSoundFixTipId,
                availabilityId: FeatureAvailability.CeProjectile);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CeExtendedLoadoutMedicineLabelFix".Translate(),
                "KebabTweaks.Patch.CeExtendedLoadoutMedicineLabelFix.Tooltip".Translate(),
                null,
                ref EnableCeExtendedLoadoutMedicineLabelFix, AppliedCeExtendedLoadoutMedicineLabelFix,
                true, true, ResetCeExtendedLoadoutMedicineLabelFix,
                null,
                FixErrorTraceCatalog.CeExtendedLoadoutMedicineLabelFix,
                FixErrorTraceCatalog.CeExtendedLoadoutMedicineLabelFixTipId,
                researchLink: FeatureResearchLinkCatalog.CeExtendedLoadoutMedicineLabelFix,
                availabilityId: FeatureAvailability.CeExtendedLoadout);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.FishTableTypeListFix".Translate(),
                "KebabTweaks.Patch.FishTableTypeListFix.Tooltip".Translate(),
                SupersededStandaloneMods.FishTableTypeListFix,
                ref EnableFishTableTypeListFix, AppliedFishTableTypeListFix, true, false, ResetFishTableTypeListFix,
                null,
                FixErrorTraceCatalog.FishTableTypeListFix, FixErrorTraceCatalog.FishTableTypeListFixTipId,
                availabilityId: FeatureAvailability.FishTable);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.GetActiveRitualsFix".Translate(),
                "KebabTweaks.Patch.GetActiveRitualsFix.Tooltip".Translate(),
                SupersededStandaloneMods.GetActiveRitualsFix,
                ref EnableGetActiveRitualsFix, AppliedGetActiveRitualsFix, true, false, ResetGetActiveRitualsFix,
                null,
                FixErrorTraceCatalog.GetActiveRitualsFix, FixErrorTraceCatalog.GetActiveRitualsFixTipId,
                availabilityId: FeatureAvailability.GetActiveRituals);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.IdleErrorWanderFix".Translate(),
                "KebabTweaks.Patch.IdleErrorWanderFix.Tooltip".Translate(),
                SupersededStandaloneMods.IdleErrorWanderFix,
                ref EnableIdleErrorWanderFix, AppliedIdleErrorWanderFix, true, false, ResetIdleErrorWanderFix,
                null,
                FixErrorTraceCatalog.IdleErrorWanderFix, FixErrorTraceCatalog.IdleErrorWanderFixTipId,
                availabilityId: FeatureAvailability.IdleErrorWander);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.AllowToolHaulUrgentlyNreFix".Translate(),
                "KebabTweaks.Patch.AllowToolHaulUrgentlyNreFix.Tooltip".Translate(),
                null,
                ref EnableAllowToolHaulUrgentlyNreFix, AppliedAllowToolHaulUrgentlyNreFix,
                true, false, ResetAllowToolHaulUrgentlyNreFix,
                null,
                FixErrorTraceCatalog.AllowToolHaulUrgentlyNreFix,
                FixErrorTraceCatalog.AllowToolHaulUrgentlyNreFixTipId,
                availabilityId: FeatureAvailability.AllowTool);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.DubsAnalyzerBeginUpdateFix".Translate(),
                "KebabTweaks.Patch.DubsAnalyzerBeginUpdateFix.Tooltip".Translate(),
                null,
                ref EnableDubsAnalyzerBeginUpdateFix, AppliedDubsAnalyzerBeginUpdateFix,
                true, false, ResetDubsAnalyzerBeginUpdateFix,
                null,
                FixErrorTraceCatalog.DubsAnalyzerBeginUpdateFix,
                FixErrorTraceCatalog.DubsAnalyzerBeginUpdateFixTipId,
                availabilityId: FeatureAvailability.DubsAnalyzer);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.DebugLogSplitterDragFix".Translate(),
                "KebabTweaks.Patch.DebugLogSplitterDragFix.Tooltip".Translate(),
                null,
                ref EnableDebugLogSplitterDragFix, AppliedDebugLogSplitterDragFix,
                true, false, ResetDebugLogSplitterDragFix,
                null,
                FixErrorTraceCatalog.DebugLogSplitterDragFix,
                FixErrorTraceCatalog.DebugLogSplitterDragFixTipId,
                availabilityId: FeatureAvailability.DebugLog);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.HospitalityGuestApparelOptimizeFix".Translate(),
                "KebabTweaks.Patch.HospitalityGuestApparelOptimizeFix.Tooltip".Translate(),
                null,
                ref EnableHospitalityGuestApparelOptimizeFix, AppliedHospitalityGuestApparelOptimizeFix,
                true, false, ResetHospitalityGuestApparelOptimizeFix,
                null,
                FixErrorTraceCatalog.HospitalityGuestApparelOptimizeFix,
                FixErrorTraceCatalog.HospitalityGuestApparelOptimizeFixTipId,
                availabilityId: FeatureAvailability.Hospitality);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.PtgMedicalCare".Translate(),
                "KebabTweaks.Patch.PtgMedicalCare.Tooltip".Translate(),
                SupersededStandaloneMods.PtgMedicalCare,
                ref EnablePtgMedicalCare, AppliedPtgMedicalCare, true, true, ResetPtgMedicalCare,
                null,
                FixErrorTraceCatalog.PtgMedicalCare, FixErrorTraceCatalog.PtgMedicalCareTipId,
                availabilityId: FeatureAvailability.PtgMedical);

            if (!FixSymptomProbe.IsSeedsPleaseSowFixLeftover())
            {
                DrawSeedsPleaseSowFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsNeanderthalChiefLeaderFixLeftover())
            {
                DrawNeanderthalChiefLeaderFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsCraftStuffFixLeftover())
            {
                DrawCraftStuffFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsStartingPawnChildAgeFixLeftover())
            {
                DrawStartingPawnChildAgeFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsStartingPawnNameFixLeftover())
            {
                DrawStartingPawnNameFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsAnomalyEspFleeFixLeftover())
            {
                DrawAnomalyEspFleeFixPatchBlock(listing, fullWidth, leftover: false);
            }

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.StratumSolarRoofEnergyTabFix".Translate(),
                "KebabTweaks.Patch.StratumSolarRoofEnergyTabFix.Tooltip".Translate(),
                null,
                ref EnableStratumSolarRoofEnergyTabFix, AppliedStratumSolarRoofEnergyTabFix,
                true, false, ResetStratumSolarRoofEnergyTabFix,
                null,
                researchLink: FeatureResearchLinkCatalog.StratumSolarRoofEnergyTabFix,
                availabilityId: FeatureAvailability.StratumSolar);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.MapPreviewRngBaselineFix".Translate(),
                "KebabTweaks.Patch.MapPreviewRngBaselineFix.Tooltip".Translate(),
                SupersededStandaloneMods.MapPreviewRngBaselineFix,
                ref EnableMapPreviewRngBaselineFix, AppliedMapPreviewRngBaselineFix,
                true, true, ResetMapPreviewRngBaselineFix,
                null,
                FixErrorTraceCatalog.MapPreviewRngBaselineFix,
                FixErrorTraceCatalog.MapPreviewRngBaselineFixTipId,
                availabilityId: FeatureAvailability.MapPreview);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.UnifiedXmlPathFix".Translate(),
                "KebabTweaks.Patch.UnifiedXmlPathFix.Tooltip".Translate(),
                SupersededStandaloneMods.UnifiedXmlPathFix,
                ref EnableUnifiedXmlPathFix, AppliedUnifiedXmlPathFix,
                true, true, ResetUnifiedXmlPathFix,
                null,
                FixErrorTraceCatalog.UnifiedXmlPathFix,
                FixErrorTraceCatalog.UnifiedXmlPathFixTipId,
                availabilityId: FeatureAvailability.UnifiedXml);

            if (!FixSymptomProbe.IsMainMenuBgFitFixLeftover())
            {
                DrawMainMenuBgFitFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsToggleableReadoutsGuiFixLeftover())
            {
                DrawToggleableReadoutsGuiFixPatchBlock(listing, fullWidth, leftover: false);
            }

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.RimatomicsGuidancePanelFix".Translate(),
                "KebabTweaks.Patch.RimatomicsGuidancePanelFix.Tooltip".Translate(),
                SupersededStandaloneMods.RimatomicsGuidancePanelFix,
                ref EnableRimatomicsGuidancePanelFix, AppliedRimatomicsGuidancePanelFix,
                true, true, ResetRimatomicsGuidancePanelFix,
                null,
                FixErrorTraceCatalog.RimatomicsGuidancePanelFix,
                FixErrorTraceCatalog.RimatomicsGuidancePanelFixTipId,
                availabilityId: FeatureAvailability.Rimatomics);

            if (!FixSymptomProbe.IsBurnWeaponBillFixLeftover())
            {
                DrawBurnWeaponBillFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsBreachAxeWorkAmountFixLeftover())
            {
                DrawBreachAxeWorkAmountFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsRawFungusBillFixLeftover())
            {
                DrawRawFungusBillFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsArchitectInfoBoxFpsFixLeftover())
            {
                DrawArchitectInfoBoxFpsFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsRimatomicsHelFpsFixLeftover())
            {
                DrawRimatomicsHelFpsFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsDiseaseComplicationFixLeftover())
            {
                DrawDiseaseComplicationFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsTraitGainFixLeftover())
            {
                DrawTraitGainFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (!FixSymptomProbe.IsSoldPawnExitGoodwillFixLeftover())
            {
                DrawSoldPawnExitGoodwillFixPatchBlock(listing, fullWidth, leftover: false);
            }

            if (HasAnyObsoleteLeftover())
            {
                DrawObsoleteFixesSection(listing, fullWidth);
            }
        }

        private void DrawFullResetButton(Rect row, float fullWidth)
        {
            Rect buttonRect = new Rect(
                row.x + (fullWidth - FullResetButtonWidth) / 2f,
                row.y,
                FullResetButtonWidth,
                FullResetButtonHeight);
            if (Widgets.ButtonText(buttonRect, "KebabTweaks.ResetAll".Translate()))
            {
                Find.WindowStack.Add(new Dialog_MessageBox(
                    "KebabTweaks.ResetAllConfirm".Translate(),
                    "Confirm".Translate(),
                    ResetAllToDefaults,
                    "Cancel".Translate(),
                    null,
                    "KebabTweaks.ResetAll".Translate(),
                    buttonADestructive: true));
            }
        }

        private void DrawKebabSwitchesBlock(Listing_Standard listing, float fullWidth)
        {
            bool notificationsBlocked = FeatureAvailability.IsBlocked(FeatureAvailability.KebabSwitches);
            DrawFeatureHeaderWithResetOnly(
                listing,
                "KebabTweaks.KebabSwitches.Header".Translate(),
                ResetKebabSwitches,
                SupersededStandaloneMods.KebabSwitches);

            DrawFeatureBodyPanel(listing, fullWidth, () =>
            {
                bool searchActive = NotificationsSearchQueryActive();
                bool showLogging = !searchActive || NotificationsSearchKeyedMatches("KebabSwitches.EnableLogging");
                bool showIgnoreAll = !searchActive || NotificationsSearchKeyedMatches("KebabSwitches.IgnoreAll");
                bool screenHeaderMatch = searchActive && NotificationsSearchKeyedMatches("KebabSwitches.ScreenMessagesHeader");
                bool letterHeaderMatch = searchActive && NotificationsSearchKeyedMatches("KebabSwitches.LettersHeader");
                bool alertHeaderMatch = searchActive && NotificationsSearchKeyedMatches("KebabSwitches.AlertsHeader");
                bool showScreen = !searchActive || screenHeaderMatch || CountMatchingScreenMessages() > 0;
                bool showLetters = !searchActive || letterHeaderMatch || CountMatchingLetters() > 0;
                bool showAlerts = !searchActive || alertHeaderMatch || CountMatchingAlerts() > 0;

                if (showLogging)
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "KebabSwitches.EnableLogging".Translate(),
                        () => KebabSwitchesEnableLogging, v => KebabSwitchesEnableLogging = v);
                }

                if (showLogging && showIgnoreAll)
                {
                    DrawSettingsRowSeparator(listing, fullWidth);
                }

                if (showIgnoreAll)
                {
                    bool ignoreAll = AllNotificationIgnoresEnabled();
                    bool ignoreAllBefore = ignoreAll;
                    DrawSettingsCheckboxRow(
                        listing,
                        "KebabSwitches.IgnoreAll".Translate(),
                        ref ignoreAll,
                        false);
                    if (ignoreAll != ignoreAllBefore)
                    {
                        bool turnOn = ignoreAll;
                        Find.WindowStack.Add(new Dialog_MessageBox(
                            (turnOn
                                ? "KebabSwitches.IgnoreAll.ConfirmOn"
                                : "KebabSwitches.IgnoreAll.ConfirmOff").Translate(),
                            "Confirm".Translate(),
                            () => SetAllNotificationIgnores(turnOn),
                            "Cancel".Translate(),
                            null,
                            "KebabSwitches.IgnoreAll".Translate(),
                            buttonADestructive: true));
                    }
                }

                if (showLogging || showIgnoreAll)
                {
                    DrawSettingsRowSeparator(listing, fullWidth);
                }

                DrawNotificationsSearchRow(listing);
                DrawSettingsRowSeparator(listing, fullWidth);

                bool drewCatalogSection = false;
                if (showScreen)
                {
                    DrawSettingsCheckboxHeightSpacer(listing);
                    DrawNotificationSection(
                        listing,
                        fullWidth,
                        "KebabSwitches.ScreenMessagesHeader".Translate(),
                        AllScreenMessagesSuppressed(),
                        SetAllScreenMessages,
                        () =>
                        {
                            foreach (SuppressibleScreenMessageEntry entry in SuppressibleScreenMessages.All)
                            {
                                if (!screenHeaderMatch && !NotificationsSearchMatches(entry))
                                {
                                    continue;
                                }

                                bool value = entry.IsSuppressEnabled();
                                DrawSettingsCheckboxRow(listing, entry.CheckboxLabel, ref value, false, entry.SourceTooltip);
                                entry.SetSuppressEnabled(value);
                                DrawSettingsRowSeparator(listing, fullWidth);
                            }
                        });
                    drewCatalogSection = true;
                }

                if (showLetters)
                {
                    if (drewCatalogSection)
                    {
                        DrawSettingsCheckboxHeightSpacer(listing);
                    }

                    DrawNotificationSection(
                        listing,
                        fullWidth,
                        "KebabSwitches.LettersHeader".Translate(),
                        AllLettersSuppressed(),
                        SetAllLetters,
                        () =>
                        {
                            foreach (SuppressibleLetterEntry entry in SuppressibleLetters.All)
                            {
                                if (!letterHeaderMatch && !NotificationsSearchMatches(entry))
                                {
                                    continue;
                                }

                                bool value = entry.IsSuppressEnabled;
                                DrawSettingsCheckboxRow(listing, entry.CheckboxLabel, ref value, false, entry.SourceTooltip);
                                entry.SetSuppressEnabled(value);
                                DrawSettingsRowSeparator(listing, fullWidth);
                            }
                        });
                    drewCatalogSection = true;
                }

                if (showAlerts)
                {
                    if (drewCatalogSection)
                    {
                        DrawSettingsCheckboxHeightSpacer(listing);
                    }

                    DrawNotificationSection(
                        listing,
                        fullWidth,
                        "KebabSwitches.AlertsHeader".Translate(),
                        AllAlertsSuppressed(),
                        SetAllAlerts,
                        () =>
                        {
                            foreach (SuppressibleAlertEntry entry in SuppressibleAlerts.All)
                            {
                                if (!alertHeaderMatch && !NotificationsSearchMatches(entry))
                                {
                                    continue;
                                }

                                bool value = entry.IsSuppressEnabled;
                                DrawSettingsCheckboxRow(listing, entry.CheckboxLabel, ref value, false, entry.SourceTooltip);
                                entry.SetSuppressEnabled(value);
                                DrawSettingsRowSeparator(listing, fullWidth);
                            }
                        });
                }

                bool anyHit = showLogging || showIgnoreAll || showScreen || showLetters || showAlerts;
                NotificationsSearchWidget.noResultsMatched = searchActive && !anyHit;
                if (searchActive && !anyHit)
                {
                    DrawNotificationsSearchNoMatchesRow(listing);
                }
            }, interactive: !notificationsBlocked);
        }

        /// <summary>
        /// One feed header and its ignore rows. The header checkbox sets every row in this feed.
        ///
        /// Заголовок одной ленты и её строки игнора. Чекбокс заголовка ставит каждую строку этой ленты.
        /// </summary>
        private void DrawNotificationSection(
            Listing_Standard listing,
            float fullWidth,
            string headerLabel,
            bool allOn,
            Action<bool> setAll,
            Action drawRows)
        {
            bool master = allOn;
            DrawSettingsSectionHeader(listing, headerLabel, ref master, "KebabSwitches.SectionMaster.Tooltip".Translate());
            if (master != allOn)
            {
                setAll(master);
            }

            DrawSettingsRowSeparator(listing, fullWidth);
            drawRows();
        }

        static bool AllScreenMessagesSuppressed()
        {
            if (SuppressibleScreenMessages.All.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < SuppressibleScreenMessages.All.Length; i++)
            {
                if (!SuppressibleScreenMessages.All[i].IsSuppressEnabled())
                {
                    return false;
                }
            }

            return true;
        }

        static void SetAllScreenMessages(bool value)
        {
            for (int i = 0; i < SuppressibleScreenMessages.All.Length; i++)
            {
                SuppressibleScreenMessages.All[i].SetSuppressEnabled(value);
            }
        }

        static bool AllLettersSuppressed()
        {
            if (SuppressibleLetters.All.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < SuppressibleLetters.All.Length; i++)
            {
                if (!SuppressibleLetters.All[i].IsSuppressEnabled)
                {
                    return false;
                }
            }

            return true;
        }

        static void SetAllLetters(bool value)
        {
            for (int i = 0; i < SuppressibleLetters.All.Length; i++)
            {
                SuppressibleLetters.All[i].SetSuppressEnabled(value);
            }
        }

        static bool AllAlertsSuppressed()
        {
            if (SuppressibleAlerts.All.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < SuppressibleAlerts.All.Length; i++)
            {
                if (!SuppressibleAlerts.All[i].IsSuppressEnabled)
                {
                    return false;
                }
            }

            return true;
        }

        static void SetAllAlerts(bool value)
        {
            for (int i = 0; i < SuppressibleAlerts.All.Length; i++)
            {
                SuppressibleAlerts.All[i].SetSuppressEnabled(value);
            }
        }

        static bool AllNotificationIgnoresEnabled()
        {
            return AllScreenMessagesSuppressed() && AllLettersSuppressed() && AllAlertsSuppressed();
        }

        static void SetAllNotificationIgnores(bool value)
        {
            SetAllScreenMessages(value);
            SetAllLetters(value);
            SetAllAlerts(value);
        }

        static bool NotificationsSearchKeyedMatches(string key)
        {
            return NotificationsSearchHaystackMatches(NotificationSearchTexts.ForKeyed(key));
        }

        /// <summary>
        /// Obsolete leftover fixes for this launch. Hidden when the probe found none.
        ///
        /// Устаревшие leftover-фиксы этого запуска. Скрыт, если зонд ничего не нашёл.
        /// </summary>
        private void DrawObsoleteFixesSection(Listing_Standard listing, float fullWidth)
        {
            listing.GetRect(ObsoleteHeaderLeadingSpacerHeight);
            DrawObsoleteFixesEnableHeaderRow(listing);
            DrawSettingsRowSeparator(listing, fullWidth);
            DrawFeatureBodyPanel(
                listing,
                fullWidth,
                () =>
                {
                    DrawSettingsCheckboxRow(
                        listing,
                        "KebabTweaks.DoNotResetObsoleteFixesOnReload".Translate(),
                        ref DoNotResetObsoleteFixesOnReload,
                        false,
                        "KebabTweaks.DoNotResetObsoleteFixesOnReload.Tooltip".Translate());
                },
                interactive: true,
                drawRainbowTop: false);

            float startY = listing.CurHeight;
            float cachedHeight = obsoleteFixesSectionHeightCache;
            Rect cachedRect = new Rect(0f, startY, fullWidth, cachedHeight);
            BlockDisabledSettingsRowInput(cachedRect, !EnableObsoleteFixes && cachedHeight > 0.5f);

            if (FixSymptomProbe.IsUfFillFixLeftover())
            {
                DrawUfFillExtraIngredientsFixPatchBlock(listing, fullWidth, leftover: true,
                    leadingSpacer: true);
            }

            if (FixSymptomProbe.IsCatCrazyTimeLeftover())
            {
                DrawCatCrazyTimePatchBlock(listing, fullWidth, leftover: true, leadingSpacer: true);
            }

            if (FixSymptomProbe.IsCraftStuffFixLeftover())
            {
                DrawCraftStuffFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsStartingPawnChildAgeFixLeftover())
            {
                DrawStartingPawnChildAgeFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsNeanderthalChiefLeaderFixLeftover())
            {
                DrawNeanderthalChiefLeaderFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsStartingPawnNameFixLeftover())
            {
                DrawStartingPawnNameFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsSeedsPleaseSowFixLeftover())
            {
                DrawSeedsPleaseSowFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsBurnWeaponBillFixLeftover())
            {
                DrawBurnWeaponBillFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsBreachAxeWorkAmountFixLeftover())
            {
                DrawBreachAxeWorkAmountFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsRawFungusBillFixLeftover())
            {
                DrawRawFungusBillFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsAnomalyEspFleeFixLeftover())
            {
                DrawAnomalyEspFleeFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsMainMenuBgFitFixLeftover())
            {
                DrawMainMenuBgFitFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsToggleableReadoutsGuiFixLeftover())
            {
                DrawToggleableReadoutsGuiFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsArchitectInfoBoxFpsFixLeftover())
            {
                DrawArchitectInfoBoxFpsFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsRimatomicsHelFpsFixLeftover())
            {
                DrawRimatomicsHelFpsFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsDiseaseComplicationFixLeftover())
            {
                DrawDiseaseComplicationFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsTraitGainFixLeftover())
            {
                DrawTraitGainFixPatchBlock(listing, fullWidth, leftover: true);
            }

            if (FixSymptomProbe.IsSoldPawnExitGoodwillFixLeftover())
            {
                DrawSoldPawnExitGoodwillFixPatchBlock(listing, fullWidth, leftover: true);
            }

            float height = listing.CurHeight - startY;
            obsoleteFixesSectionHeightCache = height;
            DrawDisabledSettingsRowVeil(new Rect(0f, startY, fullWidth, height),
                !EnableObsoleteFixes && height > 0.5f);
        }

        private void DrawObsoleteFixesEnableHeaderRow(Listing_Standard listing)
        {
            DrawPatchEnableHeaderRow(
                listing,
                "KebabTweaks.ObsoleteFixes".Translate(),
                null,
                null,
                ref EnableObsoleteFixes,
                EnableObsoleteFixes,
                false,
                false,
                ResetObsoleteFixes,
                confirmEnable: true);
        }

        private void DrawCatCrazyTimePatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover,
            bool leadingSpacer)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CatCrazyTime".Translate(),
                "KebabTweaks.Patch.CatCrazyTime.Tooltip".Translate(),
                SupersededStandaloneMods.CatCrazyTime,
                ref EnableCatCrazyTime, AppliedCatCrazyTime,
                !leftover, false, ResetCatCrazyTime,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "CatCrazyTimePatch.EnableLogging".Translate(),
                        () => CatCrazyTimeEnableLogging, v => CatCrazyTimeEnableLogging = v,
                        "CatCrazyTimePatch.EnableLoggingTooltip".Translate());
                },
                FixErrorTraceCatalog.CatCrazyTime, FixErrorTraceCatalog.CatCrazyTimeTipId,
                leadingSpacer: leadingSpacer,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? CatCrazyTimeFixRelevancePercent : (int?)null,
                availabilityId: FeatureAvailability.CatCrazyTime);
        }

        private void DrawCraftStuffFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CraftStuffFix".Translate(),
                "KebabTweaks.Patch.CraftStuffFix.Tooltip".Translate(),
                null,
                ref EnableDominantIngredientStuffFix, AppliedDominantIngredientStuffFix,
                !leftover, false, ResetDominantIngredientStuffFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: FixSymptomProbe.GetCraftStuffRelevancePercent(),
                researchLink: FeatureResearchLinkCatalog.DominantIngredientStuffFix,
                availabilityId: FeatureAvailability.CraftStuff);
        }

        private void DrawNeanderthalChiefLeaderFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.TribalLeaderFix".Translate(),
                "KebabTweaks.Patch.TribalLeaderFix.Tooltip".Translate(),
                SupersededStandaloneMods.NeanderthalChiefLeaderFix,
                ref EnableNeanderthalChiefLeaderFix, AppliedNeanderthalChiefLeaderFix,
                !leftover, false, ResetNeanderthalChiefLeaderFix,
                null,
                FixErrorTraceCatalog.NeanderthalChiefLeaderFix,
                FixErrorTraceCatalog.NeanderthalChiefLeaderFixTipId,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? 0 : (int?)null,
                researchLink: FeatureResearchLinkCatalog.NeanderthalChiefLeaderFix,
                availabilityId: FeatureAvailability.TribalLeader);
        }

        private void DrawStartingPawnChildAgeFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.StartingPawnChildAgeFix".Translate(),
                "KebabTweaks.Patch.StartingPawnChildAgeFix.Tooltip".Translate(),
                null,
                ref EnableStartingPawnChildAgeFix, AppliedStartingPawnChildAgeFix,
                !leftover, false, ResetStartingPawnChildAgeFix,
                null,
                FixErrorTraceCatalog.StartingPawnChildAgeFix,
                FixErrorTraceCatalog.StartingPawnChildAgeFixTipId,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? 0 : (int?)null,
                researchLink: FeatureResearchLinkCatalog.StartingPawnChildAgeFix,
                availabilityId: FeatureAvailability.StartingPawnChildAge);
        }

        private void DrawStartingPawnNameFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.StartingPawnNameFix".Translate(),
                "KebabTweaks.Patch.StartingPawnNameFix.Tooltip".Translate(),
                null,
                ref EnableStartingPawnNameFix, AppliedStartingPawnNameFix,
                !leftover, false, ResetStartingPawnNameFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? 0 : (int?)null,
                researchLink: FeatureResearchLinkCatalog.StartingPawnNameFix,
                availabilityId: FeatureAvailability.StartingPawnName);
        }

        private void DrawSeedsPleaseSowFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.SeedsPleaseSowFix".Translate(),
                "KebabTweaks.Patch.SeedsPleaseSowFix.Tooltip".Translate(),
                SupersededStandaloneMods.SeedsPleaseSowFix,
                ref EnableSeedsPleaseSowFix, AppliedSeedsPleaseSowFix,
                !leftover, true, ResetSeedsPleaseSowFix,
                null,
                FixErrorTraceCatalog.SeedsPleaseSowFix, FixErrorTraceCatalog.SeedsPleaseSowFixTipId,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: FixSymptomProbe.GetSeedsPleaseSowRelevancePercent(),
                researchLink: FeatureResearchLinkCatalog.SeedsPleaseSowFix,
                availabilityId: FeatureAvailability.SeedsPlease);
        }

        private void DrawUfFillExtraIngredientsFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover,
            bool leadingSpacer)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.UfFillExtraIngredientsFix".Translate(),
                "KebabTweaks.Patch.UfFillExtraIngredientsFix.Tooltip".Translate(),
                null,
                ref EnableUfFillExtraIngredientsFix, AppliedUfFillExtraIngredientsFix,
                !leftover, false, ResetUfFillExtraIngredientsFix,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing,
                        "KebabTweaks.Patch.UfFillExtraIngredientsFix.EnableLogging".Translate(),
                        () => UfFillExtraIngredientsFixEnableLogging,
                        v => UfFillExtraIngredientsFixEnableLogging = v,
                        "KebabTweaks.Patch.UfFillExtraIngredientsFix.EnableLoggingTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing,
                        "KebabTweaks.Patch.UfFillExtraIngredientsFix.CapPickup".Translate(),
                        ref EnableUfFillCapPickupToBillCount, true,
                        "KebabTweaks.Patch.UfFillExtraIngredientsFix.CapPickupTooltip".Translate());
                },
                FixErrorTraceCatalog.UfFillExtraIngredientsFix,
                FixErrorTraceCatalog.UfFillExtraIngredientsFixTipId,
                leadingSpacer: leadingSpacer,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: FixSymptomProbe.GetUfFillRelevancePercent(),
                researchLink: FeatureResearchLinkCatalog.UfFillExtraIngredientsFix,
                availabilityId: FeatureAvailability.UfFill);
        }

        private void DrawMainMenuBgFitFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.MainMenuBgFitFix".Translate(),
                "KebabTweaks.Patch.MainMenuBgFitFix.Tooltip".Translate(),
                SupersededStandaloneMods.MainMenuBgFitFix,
                ref EnableMainMenuBgFitFix, AppliedMainMenuBgFitFix,
                !leftover, true, ResetMainMenuBgFitFix,
                null,
                FixErrorTraceCatalog.MainMenuBgFitFix,
                FixErrorTraceCatalog.MainMenuBgFitFixTipId,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? 0 : (int?)null,
                availabilityId: FeatureAvailability.MainMenuBg);
        }

        private void DrawToggleableReadoutsGuiFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.ToggleableReadoutsGuiFix".Translate(),
                "KebabTweaks.Patch.ToggleableReadoutsGuiFix.Tooltip".Translate(),
                null,
                ref EnableToggleableReadoutsGuiFix, AppliedToggleableReadoutsGuiFix,
                !leftover, true, ResetToggleableReadoutsGuiFix,
                null,
                FixErrorTraceCatalog.ToggleableReadoutsGuiFix,
                FixErrorTraceCatalog.ToggleableReadoutsGuiFixTipId,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? 0 : (int?)null,
                availabilityId: FeatureAvailability.ToggleableReadouts);
        }

        private void DrawBurnWeaponBillFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.BurnWeaponBillFix".Translate(),
                "KebabTweaks.Patch.BurnWeaponBillFix.Tooltip".Translate(),
                null,
                ref EnableBurnWeaponBillFix, AppliedBurnWeaponBillFix,
                !leftover, false, ResetBurnWeaponBillFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? ObsoleteFixRelevancePercent : (int?)null,
                researchLink: FeatureResearchLinkCatalog.BurnWeaponBillFix,
                availabilityId: FeatureAvailability.BurnWeapon);
            BurnWeaponBillFixFeatures.SyncRecipeWorkTypes();
        }

        private void DrawBreachAxeWorkAmountFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.BreachAxeWorkAmountFix".Translate(),
                "KebabTweaks.Patch.BreachAxeWorkAmountFix.Tooltip".Translate(),
                null,
                ref EnableBreachAxeWorkAmountFix, AppliedBreachAxeWorkAmountFix,
                !leftover, false, ResetBreachAxeWorkAmountFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? ObsoleteFixRelevancePercent : (int?)null,
                researchLink: FeatureResearchLinkCatalog.BreachAxeWorkAmountFix,
                availabilityId: FeatureAvailability.BreachAxe);
            BreachAxeWorkAmountFixFeatures.SyncWorkAmount();
        }

        private void DrawRawFungusBillFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.RawFungusBillFix".Translate(),
                "KebabTweaks.Patch.RawFungusBillFix.Tooltip".Translate(),
                null,
                ref EnableRawFungusBillFix, AppliedRawFungusBillFix,
                !leftover, false, ResetRawFungusBillFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? ObsoleteFixRelevancePercent : (int?)null,
                researchLink: FeatureResearchLinkCatalog.RawFungusBillFix,
                availabilityId: FeatureAvailability.RawFungus);
            RawFungusBillFixFeatures.SyncCategory();
        }

        private void DrawArchitectInfoBoxFpsFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.ArchitectInfoBoxFpsFix".Translate(),
                "KebabTweaks.Patch.ArchitectInfoBoxFpsFix.Tooltip".Translate(),
                null,
                ref EnableArchitectInfoBoxFpsFix, AppliedArchitectInfoBoxFpsFix,
                !leftover, false, ResetArchitectInfoBoxFpsFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? ObsoleteFixRelevancePercent : (int?)null);
        }

        private void DrawRimatomicsHelFpsFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.RimatomicsHelFpsFix".Translate(),
                "KebabTweaks.Patch.RimatomicsHelFpsFix.Tooltip".Translate(),
                null,
                ref EnableRimatomicsHelFpsFix, AppliedRimatomicsHelFpsFix,
                !leftover, false, ResetRimatomicsHelFpsFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? ObsoleteFixRelevancePercent : (int?)null,
                availabilityId: FeatureAvailability.RimatomicsHel);
        }

        private void DrawDiseaseComplicationFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.DiseaseComplicationFix".Translate(),
                "KebabTweaks.Patch.DiseaseComplicationFix.Tooltip".Translate(),
                null,
                ref EnableDiseaseComplicationFix, AppliedDiseaseComplicationFix,
                !leftover, true, ResetDiseaseComplicationFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? ObsoleteFixRelevancePercent : (int?)null,
                availabilityId: FeatureAvailability.DiseaseComplication);
        }

        private void DrawTraitGainFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.TraitGainFix".Translate(),
                "KebabTweaks.Patch.TraitGainFix.Tooltip".Translate(),
                null,
                ref EnableTraitGainFix, AppliedTraitGainFix,
                !leftover, false, ResetTraitGainFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? ObsoleteFixRelevancePercent : (int?)null,
                availabilityId: FeatureAvailability.TraitGain);
        }

        private void DrawSoldPawnExitGoodwillFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.SoldPawnExitGoodwillFix".Translate(),
                "KebabTweaks.Patch.SoldPawnExitGoodwillFix.Tooltip".Translate(),
                null,
                ref EnableSoldPawnExitGoodwillFix, AppliedSoldPawnExitGoodwillFix,
                !leftover, false, ResetSoldPawnExitGoodwillFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? ObsoleteFixRelevancePercent : (int?)null);
        }

        private void DrawAnomalyEspFleeFixPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            bool leftover)
        {
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.AnomalyEspFleeFix".Translate(),
                "KebabTweaks.Patch.AnomalyEspFleeFix.Tooltip".Translate(),
                null,
                ref EnableAnomalyEspFleeFix, AppliedAnomalyEspFleeFix,
                !leftover, false, ResetAnomalyEspFleeFix,
                null,
                interactive: leftover ? EnableObsoleteFixes : true,
                relevancePercent: leftover ? ObsoleteFixRelevancePercent : (int?)null,
                researchLink: FeatureResearchLinkCatalog.AnomalyEspFleeFix,
                availabilityId: FeatureAvailability.AnomalyEsp);
        }

        private void DrawPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            string headerLabel,
            string headerTooltip,
            string supersededPackageId,
            ref bool enabled,
            bool appliedAtLoad,
            bool defaultEnabled,
            bool requiresRestart,
            Action resetAction,
            Action drawBody,
            string errorTrace = null,
            int errorTraceTipId = 0,
            bool leadingSpacer = true,
            bool interactive = true,
            int? relevancePercent = null,
            string researchLink = null,
            string headerSuffix = null,
            string headerSuffixTooltip = null,
            string availabilityId = null,
            string trailingSuffix = null)
        {
            bool dependencyBlocked = FeatureAvailability.IsBlocked(availabilityId);
            if (dependencyBlocked)
            {
                interactive = false;
            }

            if (leadingSpacer)
            {
                Rect spacer = listing.GetRect(FeatureHeaderLeadingSpacerHeight);
                spacer.x = 0f;
                spacer.width = fullWidth;
                BlockDisabledSettingsRowInput(spacer, dependencyBlocked);
                DrawDisabledSettingsRowVeil(spacer, dependencyBlocked);
            }

            DrawPatchEnableHeaderRow(
                listing,
                headerLabel,
                headerTooltip,
                supersededPackageId,
                ref enabled,
                appliedAtLoad,
                defaultEnabled,
                requiresRestart,
                resetAction,
                errorTrace,
                errorTraceTipId,
                interactive,
                confirmEnable: false,
                relevancePercent,
                researchLink,
                showCopyButton: selectedSettingsTab == SettingsTabKind.Fixes,
                showSearchButton: selectedSettingsTab == SettingsTabKind.Patches
                    || selectedSettingsTab == SettingsTabKind.Fixes,
                headerSuffix,
                headerSuffixTooltip,
                dependencyBlocked,
                trailingSuffix);
            if (drawBody != null)
            {
                DrawFeatureBodyPanel(listing, fullWidth, drawBody, enabled && !dependencyBlocked);
            }
            else
            {
                DrawFeatureBlockRainbowTop(listing, fullWidth);
            }
        }

        /// <summary>
        /// Draws feature options inside a dimmed panel.
        ///
        /// Рисует опции фичи в затемнённой панели.
        /// </summary>
        private void DrawFeatureBodyPanel(Listing_Standard listing, float fullWidth, Action drawBody,
            bool interactive = true, bool drawRainbowTop = true)
        {
            int index = featureBodyPanelDrawIndex++;
            while (featureBodyPanelHeightCaches.Count <= index)
            {
                featureBodyPanelHeightCaches.Add(0f);
            }

            float startY = listing.CurHeight;
            float cachedHeight = featureBodyPanelHeightCaches[index];
            Rect cachedRect = new Rect(0f, startY, fullWidth, cachedHeight);
            if (Event.current.type == EventType.Repaint && cachedHeight > 0.5f)
            {
                DrawFeatureBodyPanelFill(cachedRect);
            }

            BlockDisabledSettingsRowInput(cachedRect, !interactive && cachedHeight > 0.5f);

            float previousInset = bodyContentInset;
            bool previousInteractive = bodyContentInteractive;
            bodyContentInset = FeatureBodyContentInset;
            bodyContentInteractive = interactive;
            try
            {
                drawBody();
            }
            finally
            {
                bodyContentInset = previousInset;
                bodyContentInteractive = previousInteractive;
            }

            float height = listing.CurHeight - startY;
            featureBodyPanelHeightCaches[index] = height;
            Rect panelRect = new Rect(0f, startY, fullWidth, height);
            if (Event.current.type == EventType.Repaint && height > 0.5f)
            {
                DrawFeatureBodyPanelChrome(panelRect, drawRainbowTop);
            }

            DrawDisabledSettingsRowVeil(panelRect, !interactive && height > 0.5f);
        }

        private static void DrawFeatureBodyPanelFill(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = FeatureBodyPanelFillColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = previousColor;
        }

        private static void DrawFeatureBodyPanelChrome(Rect panel, bool drawRainbowTop = true)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            const float thickness = 1f;
            Color previousColor = GUI.color;
            GUI.color = SettingsRowSeparatorColor;
            Texture2D tex = BaseContent.WhiteTex;
            GUI.DrawTexture(new Rect(panel.x, panel.yMax - thickness, panel.width, thickness), tex);
            float topSkip = drawRainbowTop ? thickness : 0f;
            float sideHeight = Mathf.Max(0f, panel.height - thickness - topSkip);
            GUI.DrawTexture(new Rect(panel.x, panel.y + topSkip, thickness, sideHeight), tex);
            GUI.DrawTexture(new Rect(panel.xMax - thickness, panel.y + topSkip, thickness, sideHeight), tex);
            GUI.color = previousColor;

            if (drawRainbowTop)
            {
                DrawGradientLine(new Rect(panel.x, panel.y, panel.width, thickness));
            }
        }

        /// <summary>
        /// Draws the rainbow line under a feature header.
        ///
        /// Рисует радужную линию под заголовком фичи.
        /// </summary>
        private static void DrawFeatureBlockRainbowTop(Listing_Standard listing, float fullWidth)
        {
            Rect lineRect = listing.GetRect(1f);
            lineRect.x = 0f;
            lineRect.width = fullWidth;
            DrawGradientLine(lineRect);
            listing.Gap(SettingsCheckboxRowGap);
        }

        private static void ApplyBodyContentInset(ref Rect row, float fullWidth)
        {
            if (bodyContentInset <= 0f)
            {
                return;
            }

            row.x = bodyContentInset;
            row.width = fullWidth - bodyContentInset * 2f;
        }

        private const float BodyNumericFieldWidthFraction = 0.25f;

        private static Rect CalcBodyNumericFieldRect(Rect row, Rect rightHalf, float fieldHeight)
        {
            float fieldWidth = rightHalf.width * BodyNumericFieldWidthFraction;
            return new Rect(
                rightHalf.xMax - fieldWidth,
                row.y + (row.height - fieldHeight) / 2f,
                fieldWidth,
                fieldHeight);
        }

        /// <summary>
        /// Body-row numeric float field (label left, TextFieldNumeric right) like idle work-search cooldown ticks.
        ///
        /// Числовое поле float в body: лейбл слева, TextFieldNumeric справа - как задержка idle work search.
        /// </summary>
        private static void DrawBodyContentNumericFloatRow(
            Listing_Standard listing,
            float fullWidth,
            string label,
            ref float value,
            ref string buffer,
            float min,
            float max,
            float defaultValue)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            ApplyBodyContentInset(ref row, fullWidth);
            Rect left = row.LeftHalf();
            Rect right = row.RightHalf();
            const float numericFieldHeight = 24f;
            Rect fieldRect = CalcBodyNumericFieldRect(row, right, numericFieldHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            DrawUnwrappedLabel(left, label);
            Text.Anchor = TextAnchor.UpperLeft;
            bool previousGuiEnabled = GUI.enabled;
            GUI.enabled = previousGuiEnabled && bodyContentInteractive;
            Widgets.TextFieldNumeric(fieldRect, ref value, ref buffer, min, max);
            GUI.enabled = previousGuiEnabled;
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            if (!Mathf.Approximately(value, defaultValue))
            {
                DrawNonDefaultTextUnderline(row.LeftHalf(), label, TextAnchor.MiddleLeft);
            }
        }

        /// <summary>
        /// Body-row numeric int field (label left, TextFieldNumeric right) like kebab limits slider max.
        ///
        /// Числовое поле в body: лейбл слева, TextFieldNumeric справа - как max ползунка в kebab limits.
        /// </summary>
        private static void DrawBodyContentNumericIntRow(
            Listing_Standard listing,
            float fullWidth,
            string label,
            ref int value,
            ref string buffer,
            int min,
            int max,
            int defaultValue)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            ApplyBodyContentInset(ref row, fullWidth);
            Rect left = row.LeftHalf();
            Rect right = row.RightHalf();
            const float numericFieldHeight = 24f;
            Rect fieldRect = CalcBodyNumericFieldRect(row, right, numericFieldHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            DrawUnwrappedLabel(left, label);
            Text.Anchor = TextAnchor.UpperLeft;
            bool previousGuiEnabled = GUI.enabled;
            GUI.enabled = previousGuiEnabled && bodyContentInteractive;
            Widgets.TextFieldNumeric(fieldRect, ref value, ref buffer, min, max);
            GUI.enabled = previousGuiEnabled;
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            if (value != defaultValue)
            {
                DrawNonDefaultTextUnderline(row.LeftHalf(), label, TextAnchor.MiddleLeft);
            }
        }

        /// <summary>
        /// Centered feature title + reset on the right (no enable checkbox) - kebab switches.
        /// Red bold title while the superseded standalone package is still active.
        ///
        /// Центрированный заголовок + сброс справа (без enable) - kebab switches.
        /// Красный жирный заголовок, пока активен устаревший отдельный package.
        /// </summary>
        private void DrawFeatureHeaderWithResetOnly(
            Listing_Standard listing,
            string label,
            Action resetAction,
            string supersededPackageId)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            DrawFeatureHeaderBackground(row);
            float controlY = row.y + (row.height - CheckboxControlSize) / 2f;
            Rect resetRect = new Rect(
                row.xMax - HeaderControlsRightInset - FeatureResetButtonWidth,
                controlY,
                FeatureResetButtonWidth,
                CheckboxControlSize);

            bool dependencyBlocked = FeatureAvailability.IsBlocked(FeatureAvailability.KebabSwitches);
            bool supersededActive = SupersededStandaloneMods.IsActive(supersededPackageId);
            if (supersededActive)
            {
                if (!dependencyBlocked && Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                TooltipHandler.TipRegion(row, "KebabTweaks.SupersededStandaloneTooltip".Translate());
            }

            BlockDisabledSettingsRowInput(row, dependencyBlocked);
            DrawFeatureHeaderLabel(row, label, supersededActive, differsFromDefault: false);
            DrawFeatureResetButton(resetRect, label, resetAction, interactive: !dependencyBlocked);
            DrawDisabledSettingsRowVeil(row, dependencyBlocked);
            listing.Gap(SettingsCheckboxRowGap);
        }

        /// <summary>
        /// Centered patch title with reset, enable, optional magnifier, and optional copy.
        /// Yellow outline when a restart-required toggle is pending.
        ///
        /// Центрированный заголовок патча со сбросом, enable, необязательной лупой и копированием.
        /// Жёлтая обводка, пока рестарт-переключатель не применён.
        /// </summary>
        private void DrawPatchEnableHeaderRow(
            Listing_Standard listing,
            string label,
            string tooltip,
            string supersededPackageId,
            ref bool value,
            bool appliedAtLoad,
            bool defaultEnabled,
            bool requiresRestart,
            Action resetAction,
            string errorTrace = null,
            int errorTraceTipId = 0,
            bool interactive = true,
            bool confirmEnable = false,
            int? relevancePercent = null,
            string researchLink = null,
            bool showCopyButton = false,
            bool showSearchButton = false,
            string headerSuffix = null,
            string headerSuffixTooltip = null,
            bool dependencyVeil = false,
            string trailingSuffix = null,
            Action confirmEnableAction = null)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            DrawFeatureHeaderBackground(row);
            float controlY = row.y + (row.height - CheckboxControlSize) / 2f;
            Rect checkRect = new Rect(
                row.xMax - HeaderControlsRightInset - CheckboxControlSize,
                controlY,
                CheckboxControlSize,
                CheckboxControlSize);
            Rect resetRect = default(Rect);
            if (resetAction != null)
            {
                resetRect = new Rect(
                    checkRect.x - FeatureResetButtonGap - FeatureResetButtonWidth,
                    controlY,
                    FeatureResetButtonWidth,
                    CheckboxControlSize);
            }

            bool hasErrorTrace = !errorTrace.NullOrEmpty();
            int headerIconCount = 0;
            if (showCopyButton)
            {
                headerIconCount++;
            }

            if (showSearchButton)
            {
                headerIconCount++;
            }

            float iconCursorX = resetAction != null ? resetRect.x : checkRect.x;
            if (showCopyButton)
            {
                Rect copyRect = new Rect(
                    iconCursorX - FixErrorTraceUi.CopyButtonGap - FixErrorTraceUi.CopyButtonSize,
                    controlY,
                    FixErrorTraceUi.CopyButtonSize,
                    FixErrorTraceUi.CopyButtonSize);
                if (hasErrorTrace)
                {
                    NoteAlwaysInteractiveRect(copyRect);
                }

                FixErrorTraceUi.DrawHeaderIconButton(
                    copyRect,
                    TexButton.Copy,
                    errorTrace,
                    clickDisabled: !hasErrorTrace);
                iconCursorX = copyRect.x;
            }

            if (showSearchButton)
            {
                Rect searchRect = new Rect(
                    iconCursorX - FixErrorTraceUi.CopyButtonGap - FixErrorTraceUi.CopyButtonSize,
                    controlY,
                    FixErrorTraceUi.CopyButtonSize,
                    FixErrorTraceUi.CopyButtonSize);
                if (!researchLink.NullOrEmpty())
                {
                    NoteAlwaysInteractiveRect(searchRect);
                }

                FixErrorTraceUi.DrawHeaderIconButton(
                    searchRect,
                    FixErrorTraceUi.SearchIcon,
                    researchLink,
                    clickDisabled: researchLink.NullOrEmpty());
            }

            BlockDisabledSettingsRowInput(row, !interactive);

            bool supersededActive = SupersededStandaloneMods.IsActive(supersededPackageId);

            string tip = tooltip ?? string.Empty;
            if (dependencyVeil)
            {
                string missingNote = "KebabTweaks.MissingDependenciesTooltip".Translate();
                tip = tip.NullOrEmpty() ? missingNote : missingNote + "\n\n" + tip;
            }

            if (supersededActive)
            {
                if (!tip.NullOrEmpty())
                {
                    tip += "\n\n";
                }

                tip += "KebabTweaks.SupersededStandaloneTooltip".Translate();
            }
            else if (requiresRestart)
            {
                if (!tip.NullOrEmpty())
                {
                    tip += "\n\n";
                }

                tip += "KebabTweaks.RequiresRestartTooltip".Translate();
            }

            Rect labelRect;
            Rect suffixRect;
            DrawFeatureHeaderLabel(row, label, supersededActive, differsFromDefault: false,
                relevancePercent, out labelRect, out suffixRect, headerSuffix, trailingSuffix);
            bool overSuffix = suffixRect.width > 0.5f && Mouse.IsOver(suffixRect);

            Rect headerTipRect = row;
            float rightControlsWidth = HeaderControlsRightInset + CheckboxControlSize;
            if (resetAction != null)
            {
                rightControlsWidth += FeatureResetButtonGap + FeatureResetButtonWidth;
            }

            if (headerIconCount > 0)
            {
                rightControlsWidth += FixErrorTraceUi.HeaderIconsReservedWidth(headerIconCount);
            }

            headerTipRect.width -= rightControlsWidth;

            if (overSuffix)
            {
                Widgets.DrawHighlight(suffixRect);
                string suffixTip = headerSuffixTooltip.NullOrEmpty()
                    ? "KebabTweaks.RelevancePercentTooltip".Translate()
                    : headerSuffixTooltip;
                TooltipHandler.TipRegion(suffixRect, suffixTip);
            }
            else if (!tip.NullOrEmpty())
            {
                if (interactive && Mouse.IsOver(headerTipRect))
                {
                    Widgets.DrawHighlight(headerTipRect);
                }

                TooltipHandler.TipRegion(headerTipRect, tip);
            }

            DrawFeatureResetButton(resetRect, label, resetAction, interactive);

            bool displayed = value;
            Widgets.Checkbox(new Vector2(checkRect.x, checkRect.y), ref displayed, CheckboxControlSize,
                disabled: !interactive);
            if (interactive && displayed != value)
            {
                if (displayed && confirmEnable)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox(
                        "KebabTweaks.EnableObsoleteFixesConfirm".Translate(),
                        "Confirm".Translate(),
                        () =>
                        {
                            if (confirmEnableAction != null)
                            {
                                confirmEnableAction();
                            }
                            else
                            {
                                EnableObsoleteFixes = true;
                            }

                            Write();
                        },
                        "Cancel".Translate(),
                        null,
                        "KebabTweaks.EnableObsoleteFixesConfirmTitle".Translate(),
                        buttonADestructive: true));
                }
                else
                {
                    value = displayed;
                    if (confirmEnable)
                    {
                        Write();
                    }
                }
            }

            if (value != defaultEnabled)
            {
                DrawNonDefaultTextUnderline(labelRect, label, TextAnchor.MiddleLeft);
            }

            // After restart Applied* matches Enable* - outline stays off until the player toggles again.
            // A superseded skip keeps Applied* aligned with Enable*, so no yellow outline then.
            if (!supersededActive && requiresRestart && value != appliedAtLoad)
            {
                DrawInnerBoxBorder(checkRect, 2f, RestartPendingOutlineColor);
            }

            DrawDisabledSettingsRowVeil(row, dependencyVeil);
            listing.Gap(SettingsCheckboxRowGap);
        }

        private static void DrawFeatureHeaderLabel(
            Rect row,
            string label,
            bool supersededActive,
            bool differsFromDefault)
        {
            Rect unusedLabel;
            Rect unusedSuffix;
            DrawFeatureHeaderLabel(row, label, supersededActive, differsFromDefault, null,
                out unusedLabel, out unusedSuffix);
        }

        /// <summary>
        /// Draws the centered patch title, then the relevance or package suffix, then an optional
        /// trailing suffix such as the 1.5-only mark. The trailing piece has no suffix tooltip.
        ///
        /// Рисует центрированный заголовок, затем суффикс актуальности или package id, затем
        /// необязательный хвостовой суффикс, например метку «только 1.5». У хвоста нет своего тултипа.
        /// </summary>
        private static void DrawFeatureHeaderLabel(
            Rect row,
            string label,
            bool supersededActive,
            bool differsFromDefault,
            int? relevancePercent,
            out Rect labelRect,
            out Rect suffixRect,
            string headerSuffix = null,
            string trailingSuffix = null)
        {
            string suffix = !headerSuffix.NullOrEmpty()
                ? headerSuffix
                : relevancePercent.HasValue
                    ? " (" + relevancePercent.Value.ToString() + "%)"
                    : string.Empty;
            bool tintRelevance = headerSuffix.NullOrEmpty() && relevancePercent.HasValue;
            string trailing = trailingSuffix ?? string.Empty;

            TextAnchor previousAnchor = Text.Anchor;
            Color previousColor = GUI.color;
            FontStyle previousStyle = Text.CurFontStyle.fontStyle;
            bool previousWrap = Text.WordWrap;
            Text.WordWrap = false;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (supersededActive)
            {
                GUI.color = SupersededHeaderColor;
                Text.CurFontStyle.fontStyle = FontStyle.Bold;
            }

            Vector2 labelSize = CalcUnwrappedLabelSize(label);
            Vector2 suffixSize = suffix.NullOrEmpty() ? Vector2.zero : CalcUnwrappedLabelSize(suffix);
            Vector2 trailingSize = trailing.NullOrEmpty() ? Vector2.zero : CalcUnwrappedLabelSize(trailing);
            float startX = row.x + (row.width - labelSize.x - suffixSize.x - trailingSize.x) / 2f;
            labelRect = new Rect(startX, row.y, labelSize.x, row.height);
            suffixRect = suffix.NullOrEmpty()
                ? new Rect(startX + labelSize.x, row.y, 0f, row.height)
                : new Rect(startX + labelSize.x, row.y, suffixSize.x, row.height);
            Rect trailingRect = new Rect(startX + labelSize.x + suffixSize.x, row.y, trailingSize.x, row.height);

            Widgets.Label(labelRect, label);
            Text.CurFontStyle.fontStyle = previousStyle;
            GUI.color = previousColor;

            if (!suffix.NullOrEmpty())
            {
                if (tintRelevance)
                {
                    GUI.color = relevancePercent.GetValueOrDefault() > 0
                        ? RelevancePercentPositiveColor
                        : RelevancePercentZeroColor;
                }

                Widgets.Label(suffixRect, suffix);
                GUI.color = previousColor;
            }

            if (!trailing.NullOrEmpty())
            {
                Widgets.Label(trailingRect, trailing);
            }

            Text.WordWrap = previousWrap;
            Text.Anchor = previousAnchor;

            if (differsFromDefault)
            {
                DrawNonDefaultTextUnderline(labelRect, label, TextAnchor.MiddleLeft);
            }
        }

        /// <summary>
        /// Label size with word wrap off.
        ///
        /// Размер лейбла без переноса слов.
        /// </summary>
        private static Vector2 CalcUnwrappedLabelSize(string text)
        {
            bool previousWrap = Text.WordWrap;
            Text.WordWrap = false;
            Vector2 size = Text.CalcSize(text ?? string.Empty);
            Text.WordWrap = previousWrap;
            size.x = Mathf.Ceil(size.x) + UnwrappedLabelWidthPad;
            return size;
        }

        private static void DrawUnwrappedLabel(Rect rect, string text)
        {
            bool previousWrap = Text.WordWrap;
            Text.WordWrap = false;
            Widgets.Label(rect, text);
            Text.WordWrap = previousWrap;
        }

        private void DrawFeatureResetButton(Rect rect, string featureLabel, Action resetAction, bool interactive)
        {
            if (resetAction == null)
            {
                return;
            }

            bool previousGuiEnabled = GUI.enabled;
            GUI.enabled = previousGuiEnabled && interactive;
            bool clicked = Widgets.ButtonText(rect, "KebabTweaks.ResetFeature".Translate());
            GUI.enabled = previousGuiEnabled;
            if (!interactive || !clicked)
            {
                return;
            }

            Find.WindowStack.Add(new Dialog_MessageBox(
                "KebabTweaks.ResetFeatureConfirm".Translate(featureLabel.Named("FEATURE")),
                "Confirm".Translate(),
                () =>
                {
                    resetAction();
                    Write();
                },
                "Cancel".Translate(),
                null,
                "KebabTweaks.ResetFeature".Translate(),
                buttonADestructive: true));
        }

        private static void DrawSettingsCheckboxHeightSpacer(Listing_Standard listing)
        {
            listing.GetRect(SettingsCheckboxRowHeight);
        }

        /// <summary>
        /// Draws the Notifications search box. Ignore rows match four lists at once: the
        /// English checkbox, the Russian checkbox, the English game sentence, and the Russian
        /// game sentence. Enable logging, Ignore all, and the three feed headers use ForKeyed.
        ///
        /// Рисует поле поиска на вкладке Уведомления. Строки игнора сверяются сразу по четырём
        /// спискам: английский чекбокс, русский чекбокс, английская фраза игры и русская фраза
        /// игры. Включить логирование, Игнорировать все и три заголовка лент ищутся через ForKeyed.
        /// </summary>
        private void DrawNotificationsSearchRow(Listing_Standard listing)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            ApplyBodyContentInset(ref row, listing.ColumnWidth);

            const float widgetHeight = 24f;
            float widgetWidth = Mathf.Max(widgetHeight, row.width / 3f);
            Rect widgetRect = new Rect(
                row.x,
                row.y + (row.height - widgetHeight) / 2f,
                widgetWidth,
                widgetHeight);

            string previous = NotificationsSearchWidget.filter.Text ?? string.Empty;
            NotificationsSearchWidget.OnGUI(widgetRect);
            string current = NotificationsSearchWidget.filter.Text ?? string.Empty;
            if (!string.Equals(previous, current, StringComparison.Ordinal))
            {
                settingsScrollPositionNotifications = Vector2.zero;
            }

            listing.Gap(SettingsCheckboxRowGap);
        }

        private static void DrawNotificationsSearchNoMatchesRow(Listing_Standard listing)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            ApplyBodyContentInset(ref row, listing.ColumnWidth);
            Text.Anchor = TextAnchor.MiddleLeft;
            DrawUnwrappedLabel(row, "KebabSwitches.Search.NoMatches".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            listing.Gap(SettingsCheckboxRowGap);
        }

        private static bool NotificationsSearchMatches(SuppressibleScreenMessageEntry entry)
        {
            if (!NotificationsSearchQueryActive())
            {
                return true;
            }

            return NotificationSearchTexts.Matches(entry, NotificationsSearchWidget.filter.Text);
        }

        private static bool NotificationsSearchMatches(SuppressibleLetterEntry entry)
        {
            if (!NotificationsSearchQueryActive())
            {
                return true;
            }

            return NotificationSearchTexts.Matches(entry, NotificationsSearchWidget.filter.Text);
        }

        private static bool NotificationsSearchMatches(SuppressibleAlertEntry entry)
        {
            if (!NotificationsSearchQueryActive())
            {
                return true;
            }

            return NotificationSearchTexts.Matches(entry, NotificationsSearchWidget.filter.Text);
        }

        private static bool NotificationsSearchQueryActive()
        {
            QuickSearchFilter filter = NotificationsSearchWidget.filter;
            return filter != null && filter.Active;
        }

        private static bool NotificationsSearchHaystackMatches(string haystack)
        {
            return !haystack.NullOrEmpty() && NotificationsSearchWidget.filter.Matches(haystack);
        }

        private static int CountMatchingScreenMessages()
        {
            int count = 0;
            for (int i = 0; i < SuppressibleScreenMessages.All.Length; i++)
            {
                if (NotificationsSearchMatches(SuppressibleScreenMessages.All[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountMatchingLetters()
        {
            int count = 0;
            for (int i = 0; i < SuppressibleLetters.All.Length; i++)
            {
                if (NotificationsSearchMatches(SuppressibleLetters.All[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountMatchingAlerts()
        {
            int count = 0;
            for (int i = 0; i < SuppressibleAlerts.All.Length; i++)
            {
                if (NotificationsSearchMatches(SuppressibleAlerts.All[i]))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Centered feed title with a checkbox on the right. The checkbox does not move the title.
        /// Tooltip stays active when the row cannot be toggled.
        ///
        /// Центрированный заголовок ленты с чекбоксом справа. Чекбокс не сдвигает заголовок.
        /// Тултип работает, когда строку нельзя переключить.
        /// </summary>
        private static void DrawSettingsSectionHeader(
            Listing_Standard listing,
            string label,
            ref bool value,
            string tooltip)
        {
            Rect row = listing.GetRect(SettingsSectionHeaderHeight);
            float controlY = row.y + (row.height - CheckboxControlSize) / 2f;
            Rect checkRect = new Rect(
                row.xMax - HeaderControlsRightInset - CheckboxControlSize,
                controlY,
                CheckboxControlSize,
                CheckboxControlSize);
            Text.Anchor = TextAnchor.MiddleCenter;
            DrawUnwrappedLabel(row, label);
            Text.Anchor = TextAnchor.UpperLeft;
            if (!tooltip.NullOrEmpty())
            {
                if (bodyContentInteractive && Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                TooltipHandler.TipRegion(row, tooltip);
            }

            BlockDisabledSettingsRowInput(row, !bodyContentInteractive);
            bool displayed = value;
            Widgets.Checkbox(new Vector2(checkRect.x, checkRect.y), ref displayed, CheckboxControlSize,
                disabled: !bodyContentInteractive);
            if (bodyContentInteractive && displayed != value)
            {
                value = displayed;
            }

            listing.Gap(SettingsCheckboxRowGap);
        }

        /// <summary>
        /// Draws the feature header background.
        ///
        /// Рисует фон заголовка фичи.
        /// </summary>
        private static void DrawFeatureHeaderBackground(Rect row)
        {
            if (Event.current.type != EventType.Repaint || row.height <= 0.5f)
            {
                return;
            }

            Color previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(row, GetFeatureHeaderAlphaGradientTex());
            GUI.color = previous;
        }

        private static Texture2D GetFeatureHeaderAlphaGradientTex()
        {
            if (featureHeaderAlphaGradientTex != null)
            {
                return featureHeaderAlphaGradientTex;
            }

            const int height = 128;
            Texture2D tex = new Texture2D(1, height, TextureFormat.ARGB32, mipChain: false)
            {
                name = "KebabTweaks.FeatureHeaderAlphaGradient",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            // Unity Texture2D (0,0) is bottom-left; GUI.DrawTexture maps that to the bottom of the rect.
            Color bottom = SettingsTabFillColor;
            Color[] pixels = new Color[height];
            for (int y = 0; y < height; y++)
            {
                float tFromBottom = height <= 1 ? 0f : (float)y / (height - 1);
                float alpha = Mathf.Lerp(bottom.a, 0f, tFromBottom);
                pixels[y] = new Color(bottom.r, bottom.g, bottom.b, alpha);
            }

            tex.SetPixels(pixels);
            tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            featureHeaderAlphaGradientTex = tex;
            return featureHeaderAlphaGradientTex;
        }

        private static void DrawSettingsRowSeparator(Listing_Standard listing, float fullWidth)
        {
            Rect lineRect = listing.GetRect(1f);
            lineRect.x = 0f;
            lineRect.width = fullWidth;
            if (Event.current.type == EventType.Repaint)
            {
                Color previousColor = GUI.color;
                GUI.color = SettingsRowSeparatorColor;
                GUI.DrawTexture(lineRect, BaseContent.WhiteTex);
                GUI.color = previousColor;
            }
        }

        private static void DrawGradientLine(Rect rect)
        {
            if (Event.current.type != EventType.Repaint || BlockSeparatorGradientColors.Length == 0)
            {
                return;
            }

            const float step = 2f;
            Color previous = GUI.color;
            for (float x = rect.xMin; x < rect.xMax; x += step)
            {
                float t = Mathf.InverseLerp(rect.xMin, rect.xMax, x);
                GUI.color = SampleGradientColor(t);
                GUI.DrawTexture(new Rect(x, rect.yMin, Mathf.Min(step, rect.xMax - x), rect.height),
                    BaseContent.WhiteTex);
            }

            GUI.color = previous;
        }

        private static Color SampleGradientColor(float position)
        {
            position = Mathf.Clamp01(position);
            if (BlockSeparatorGradientColors.Length <= 1)
            {
                return BlockSeparatorGradientColors[0];
            }

            float scaled = position * (BlockSeparatorGradientColors.Length - 1);
            int left = Mathf.FloorToInt(scaled);
            int right = Mathf.Min(left + 1, BlockSeparatorGradientColors.Length - 1);
            return Color.Lerp(BlockSeparatorGradientColors[left], BlockSeparatorGradientColors[right], scaled - left);
        }

        private static float CalcCheckboxRowHeight(Listing_Standard listing, string label)
        {
            float rowWidth = listing.ColumnWidth;
            if (bodyContentInset > 0f)
            {
                rowWidth -= bodyContentInset * 2f;
            }

            float labelWidth = Mathf.Max(10f, rowWidth - CheckboxControlSize - 6f);
            return Mathf.Max(SettingsCheckboxRowHeight, Text.CalcHeight(label, labelWidth));
        }

        private static void DrawSettingsCheckboxRow(
            Listing_Standard listing,
            string label,
            ref bool value,
            bool defaultValue,
            string tooltip = null)
        {
            Rect row = listing.GetRect(CalcCheckboxRowHeight(listing, label));
            ApplyBodyContentInset(ref row, listing.ColumnWidth);
            if (!tooltip.NullOrEmpty())
            {
                if (bodyContentInteractive && Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                TooltipHandler.TipRegion(row, tooltip);
            }

            Widgets.CheckboxLabeled(row, label, ref value, disabled: !bodyContentInteractive);
            if (value != defaultValue)
            {
                DrawNonDefaultTextUnderline(row, label, TextAnchor.MiddleLeft);
            }

            listing.Gap(SettingsCheckboxRowGap);
        }

        private void DrawStartingCorpseMarkModeRow(Listing_Standard listing)
        {
            const float controlSize = 24f;
            const float digitGap = 4f;
            const int controlCount = 3;
            float controlsWidth = controlCount * controlSize + (controlCount - 1) * digitGap;
            bool interactive = bodyContentInteractive;

            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            ApplyBodyContentInset(ref row, listing.ColumnWidth);
            BlockDisabledSettingsRowInput(row, !interactive);

            Rect labelRect = row;
            labelRect.width = Mathf.Max(0f, row.width - controlsWidth);
            TextAnchor previousAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, "KebabTweaks.Patch.StartingCorpsesNoHaul.Mode".Translate());
            Text.Anchor = previousAnchor;
            if (StartingCorpseMarkMode != DefaultStartingCorpseMarkMode)
            {
                DrawNonDefaultTextUnderline(labelRect,
                    "KebabTweaks.Patch.StartingCorpsesNoHaul.Mode".Translate(),
                    TextAnchor.MiddleLeft);
            }

            float controlY = row.y + (row.height - controlSize) / 2f;
            float x = row.xMax - controlSize;
            Rect crossRect = new Rect(x, controlY, controlSize, controlSize);
            TooltipHandler.TipRegion(crossRect,
                "KebabTweaks.Patch.StartingCorpsesNoHaul.ModeX.Tooltip".Translate());
            if (interactive && Widgets.ButtonImage(crossRect, Widgets.CheckboxOffTex))
            {
                StartingCorpseMarkMode = 0;
            }
            else if (!interactive)
            {
                GUI.DrawTexture(crossRect, Widgets.CheckboxOffTex);
            }

            x -= digitGap + controlSize;
            DrawStartingCorpseMarkModeDigit(new Rect(x, controlY, controlSize, controlSize), 2, interactive);
            x -= digitGap + controlSize;
            DrawStartingCorpseMarkModeDigit(new Rect(x, controlY, controlSize, controlSize), 1, interactive);

            listing.Gap(SettingsCheckboxRowGap);
        }

        private static void DrawStartingCorpseMarkModeDigit(Rect rect, int mode, bool interactive)
        {
            string tooltipKey = mode == 1
                ? "KebabTweaks.Patch.StartingCorpsesNoHaul.Mode1.Tooltip"
                : "KebabTweaks.Patch.StartingCorpsesNoHaul.Mode2.Tooltip";
            TooltipHandler.TipRegion(rect, tooltipKey.Translate());
            if (interactive && Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }

            if (interactive && Widgets.ButtonInvisible(rect))
            {
                StartingCorpseMarkMode = mode;
            }

            bool active = StartingCorpseMarkMode == mode;
            if (active)
            {
                DrawInnerBoxBorder(rect, 2f, ModeDigitActiveColor);
            }

            Color digitColor = StartingCorpseMarkMode == 0
                ? ModeDigitDisabledColor
                : active ? ModeDigitActiveColor : ModeDigitInactiveColor;

            GameFont previousFont = Text.Font;
            TextAnchor previousAnchor = Text.Anchor;
            Color previousColor = GUI.color;
            Text.Font = GameFont.Medium;
            GUIStyle labelStyle = new GUIStyle(Text.CurFontStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            GUI.color = digitColor;
            GUI.Label(rect, mode.ToString(), labelStyle);
            GUI.color = previousColor;
            Text.Anchor = previousAnchor;
            Text.Font = previousFont;
        }

        /// <summary>
        /// Enable-logging checkbox: turning ON opens a confirm dialog (like feature reset); OFF is immediate.
        ///
        /// Чекбокс «Включить логирование»: включение - через confirm; выключение - без диалога.
        /// </summary>
        private void DrawSettingsEnableLoggingCheckboxRow(
            Listing_Standard listing,
            string label,
            Func<bool> getValue,
            Action<bool> setValue,
            string featureTooltip = null)
        {
            Rect row = listing.GetRect(CalcCheckboxRowHeight(listing, label));
            ApplyBodyContentInset(ref row, listing.ColumnWidth);
            if (!featureTooltip.NullOrEmpty())
            {
                if (bodyContentInteractive && Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                TooltipHandler.TipRegion(row, featureTooltip);
            }

            bool value = getValue();
            bool previous = value;
            Widgets.CheckboxLabeled(row, label, ref value, disabled: !bodyContentInteractive);
            if (!bodyContentInteractive)
            {
                if (getValue())
                {
                    DrawNonDefaultTextUnderline(row, label, TextAnchor.MiddleLeft);
                }

                listing.Gap(SettingsCheckboxRowGap);
                return;
            }

            if (!previous && value)
            {
                value = false;
                string body = "KebabTweaks.EnableLoggingConfirm".Translate();
                if (!featureTooltip.NullOrEmpty())
                {
                    body += "\n\n" + featureTooltip;
                }

                Find.WindowStack.Add(new Dialog_MessageBox(
                    body,
                    "Confirm".Translate(),
                    () =>
                    {
                        setValue(true);
                        Write();
                    },
                    "Cancel".Translate(),
                    null,
                    "KebabTweaks.EnableLoggingConfirmTitle".Translate()));
            }
            else if (value != previous)
            {
                setValue(value);
            }

            if (getValue())
            {
                DrawNonDefaultTextUnderline(row, label, TextAnchor.MiddleLeft);
            }

            listing.Gap(SettingsCheckboxRowGap);
        }

        static readonly List<Rect> alwaysInteractiveRects = new List<Rect>();

        /// <summary>
        /// Drops magnifier and copy rects from the previous Repaint before this one records new ones.
        ///
        /// Сбрасывает прямоугольники лупы и копирования с прошлого Repaint перед записью новых.
        /// </summary>
        private static void ClearAlwaysInteractiveRects()
        {
            if (Event.current.type == EventType.Repaint)
            {
                alwaysInteractiveRects.Clear();
            }
        }

        /// <summary>
        /// Remembers a magnifier or copy rect. Those buttons stay clickable under a veil.
        ///
        /// Запоминает прямоугольник лупы или копирования. Эти кнопки остаются кликабельными под пеленой.
        /// </summary>
        private static void NoteAlwaysInteractiveRect(Rect rect)
        {
            if (Event.current.type != EventType.Repaint || rect.width <= 0.5f || rect.height <= 0.5f)
            {
                return;
            }

            alwaysInteractiveRects.Add(rect);
        }

        static bool MouseOverAlwaysInteractiveRect()
        {
            for (int i = 0; i < alwaysInteractiveRects.Count; i++)
            {
                if (Mouse.IsOver(alwaysInteractiveRects[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Veil and input block are one contract: a veiled rect does not take clicks, and a
        /// non-interactive rect is veiled. Only MouseDown, MouseUp, and MouseDrag are consumed,
        /// so TipRegion tooltips still work. Magnifier and copy rects noted on Repaint stay clickable.
        /// CheckboxLabeled still toggles after the event is used, so a veiled checkbox is also drawn disabled.
        ///
        /// Затемнение и блок ввода - одна связка: затемнённый прямоугольник не принимает клики,
        /// а невосприимчивый прямоугольник затемнён. Поглощаются только MouseDown, MouseUp и
        /// MouseDrag, поэтому тултипы TipRegion работают. Прямоугольники лупы и копирования,
        /// отмеченные на Repaint, остаются кликабельными.
        /// CheckboxLabeled после поглощения события всё равно переключается, поэтому затемнённый чекбокс рисуется недоступным.
        /// </summary>
        private static void BlockDisabledSettingsRowInput(Rect row, bool disabled)
        {
            if (!disabled || !Mouse.IsOver(row) || MouseOverAlwaysInteractiveRect())
            {
                return;
            }

            Event current = Event.current;
            if (current.type == EventType.MouseDown || current.type == EventType.MouseUp ||
                current.type == EventType.MouseDrag)
            {
                current.Use();
            }
        }

        /// <summary>
        /// Visual half of the veil and input-block contract. Does not consume mouse events.
        ///
        /// Визуальная половина связки затемнения и блока ввода. События мыши не поглощает.
        /// </summary>
        private static void DrawDisabledSettingsRowVeil(Rect row, bool disabled)
        {
            if (!disabled || Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color previous = GUI.color;
            GUI.color = SettingsDisabledVeilColor;
            GUI.DrawTexture(row, BaseContent.WhiteTex);
            GUI.color = previous;
        }

        /// <summary>
        /// Light-blue underline under a setting label when the value differs from default.
        ///
        /// Голубая линия под текстом настройки, если значение отличается от дефолта.
        /// </summary>
        private static void DrawNonDefaultTextUnderline(Rect row, string text, TextAnchor anchor)
        {
            if (Event.current.type != EventType.Repaint || text.NullOrEmpty())
            {
                return;
            }

            Vector2 size = CalcUnwrappedLabelSize(text);
            float x;
            if (anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.UpperCenter ||
                anchor == TextAnchor.LowerCenter)
            {
                x = row.x + (row.width - size.x) / 2f;
            }
            else
            {
                x = row.x;
            }

            float y = row.yMax - NonDefaultUnderlineThickness - 3f;
            Color previous = GUI.color;
            GUI.color = NonDefaultUnderlineColor;
            GUI.DrawTexture(new Rect(x, y, size.x, NonDefaultUnderlineThickness), BaseContent.WhiteTex);
            GUI.color = previous;
        }

        private static void DrawInnerBoxBorder(Rect rect, float thickness, Color color)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = color;
            Texture2D tex = BaseContent.WhiteTex;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), tex);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), tex);
            float innerHeight = rect.height - thickness * 2f;
            GUI.DrawTexture(new Rect(rect.x, rect.y + thickness, thickness, innerHeight), tex);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y + thickness, thickness, innerHeight), tex);
            GUI.color = previousColor;
        }

        public void ResetAllToDefaults()
        {
            ResetKebabSwitches();
            ResetCatCrazyTime();
            ResetCatFloorSleep();
            ResetCeRunForCoverDestFix();
            ResetCeProjectileNullSoundFix();
            ResetCeExtendedLoadoutMedicineLabelFix();
            ResetArmorRacksAssignFix();
            ResetStorageSettingsAllowedToAcceptFix();
            ResetSaveSettingsLoadFix();
            ResetFishTableTypeListFix();
            ResetGetActiveRitualsFix();
            ResetGrowerCutTrees();
            ResetIdleErrorWanderFix();
            ResetHospitalityGuestApparelOptimizeFix();
            ResetLowTpsPawnDump();
            ResetIdleWorkSearchCooldown();
            ResetParanoidPerfectionist();
            ResetDrillLimitPatch();
            ResetStartingCorpsesNoHaul();
            ResetPtgMedicalCare();
            ResetSeedsPleaseSowFix();
            ResetTakeFromMending();
            ResetTradeCaravanLordFix();
            ResetTraderCaravanStayFix();
            ResetRatkinMercenaryAgeFix();
            ResetCompressibleOverlapFix();
            ResetApparelPolicyLoadFix();
            ResetBillRenamePrefill();
            ResetRecipeProductDropLostLog();
            ResetNumbersLoadDefaultFallback();
            ResetAllowToolHaulUrgentlyNreFix();
            ResetDubsAnalyzerBeginUpdateFix();
            ResetDebugLogSplitterDragFix();
            ResetNeanderthalChiefLeaderFix();
            ResetDominantIngredientStuffFix();
            ResetStartingPawnChildAgeFix();
            ResetStartingPawnNameFix();
            ResetAndroidEnlightenFix();
            ResetArchitectSearchResearchPatch();
            ResetMonumentMaterialOnMapPatch();
            ResetObsoletePatches();
            ResetWikiRimCompat();
            ResetObsoleteFixes();
            ResetMapPreviewRngBaselineFix();
            ResetUnifiedXmlPathFix();
            ResetMainMenuBgFitFix();
            ResetToggleableReadoutsGuiFix();
            ResetRimatomicsGuidancePanelFix();
            ResetUfFillExtraIngredientsFix();
            ResetBurnWeaponBillFix();
            ResetBreachAxeWorkAmountFix();
            ResetRawFungusBillFix();
            ResetAnomalyEspFleeFix();
            ResetStratumSolarRoofEnergyTabFix();
            ResetArchitectInfoBoxFpsFix();
            ResetRimatomicsHelFpsFix();
            ResetDiseaseComplicationFix();
            ResetTraitGainFix();
            ResetSoldPawnExitGoodwillFix();
            Write();
        }

        private static void ResetKebabSwitches()
        {
            KebabSwitchesEnableLogging = false;
            NotificationsSearchWidget.Reset();
            SuppressFilledMapMessage = false;
            SuppressGaveBirthMessage = false;
            SuppressAnimalIsPregnantMessage = false;
            SuppressMiscarriedStarvationMessage = false;
            SuppressMiscarriedPoorHealthMessage = false;
            SuppressSeasonBegunMessage = false;
            SuppressBillCompleteMessage = false;
            SuppressFullyHealedMessage = false;
            SuppressSocialFightMessage = false;
            SuppressShipChunkDropMessage = false;
            SuppressStruckMineableMessage = false;
            SuppressBedLostAssignmentMessage = false;
            SuppressBedDestroyedMessage = false;
            SuppressSleepingPawnsWokenUpMessage = false;
            SuppressNoLongerSocialFightingMessage = false;
            SuppressNoLongerTargetedInsultMessage = false;
            SuppressInsultingSpreeEndedMessage = false;
            SuppressTargetedInsultStreamEndedMessage = false;
            SuppressFoodBingeEndedMessage = false;
            SuppressNoLongerBingingOnDrugMessage = false;
            SuppressBerserkEndedMessage = false;
            SuppressFireStartingEndedMessage = false;
            SuppressJailbreakerEndedMessage = false;
            SuppressSlaughtererEndedMessage = false;
            SuppressMurderousRageEndedMessage = false;
            SuppressGiveUpReturnedMessage = false;
            SuppressTantrumEndedMessage = false;
            SuppressRedirectedAngerMessage = false;
            SuppressSadisticRageEndedMessage = false;
            SuppressCorpseObsessionEndedMessage = false;
            SuppressWanderSadEndedMessage = false;
            SuppressHideInRoomEndedMessage = false;
            SuppressPsychoticWanderEndedMessage = false;
            SuppressConfusedEndedMessage = false;
            SuppressPanicFleeEndedMessage = false;
            SuppressManhunterEndedMessage = false;
            SuppressNewBondRelationMessage = false;
            SuppressNewBondRelationNewNameMessage = false;
            SuppressFoodPoisoningMessage = false;
            SuppressRoamerLeavingMessage = false;
            SuppressHiveReproducedMessage = false;
            SuppressTraderCaravanLeavingMessage = false;
            SuppressTraderCaravanDismissedMessage = false;
            SuppressCompSpawnerSpawnedItemMessage = false;
            SuppressFishCaughtInTrap = false;
            SuppressPlantDiedOfCold = false;
            SuppressPlantDiedOfRotUnharvested = false;
            SuppressPlantDiedOfRotLight = false;
            SuppressPlantDiedOfRot = false;
            SuppressPlantDiedOfPoison = false;
            SuppressPlantDiedOfBlight = false;
            SuppressPlantDiedOfPollution = false;
            SuppressPlantDiedOfNoPollution = false;
            SuppressPlantDiedOfRotPollutedTerrain = false;
#if RIMWORLD_1_6
            SuppressPlantDiedOfVacuum = false;
            SuppressEspInjuredMessage = false;
#endif
            SuppressMinifiedTreeDied = false;
            SuppressRottedAwayInStorage = false;
            SuppressDeterioratedAway = false;
            SuppressWornApparelDeterioratedAway = false;
            EnsureSuppressLetterIds();
            SuppressLetterIds.Clear();
            EnsureSuppressAlertIds();
            SuppressAlertIds.Clear();
        }

        private static void ResetCatCrazyTime()
        {
            EnableCatCrazyTime = !FixSymptomProbe.IsCatCrazyTimeLeftover();
            CatCrazyTimeEnableLogging = false;
        }

        private static void ResetCatFloorSleep()
        {
            EnableCatFloorSleep = true;
            CatFloorSleepEnableLogging = false;
        }

        private static void ResetCeRunForCoverDestFix()
        {
            EnableCeRunForCoverDestFix = true;
        }

        private static void ResetCeProjectileNullSoundFix()
        {
            EnableCeProjectileNullSoundFix = true;
        }

        private static void ResetCeExtendedLoadoutMedicineLabelFix()
        {
            EnableCeExtendedLoadoutMedicineLabelFix = true;
        }

        private static void ResetArmorRacksAssignFix()
        {
            EnableArmorRacksAssignFix = true;
        }

        private static void ResetStorageSettingsAllowedToAcceptFix()
        {
            EnableStorageSettingsAllowedToAcceptFix = true;
        }

        private static void ResetFishTableTypeListFix()
        {
            EnableFishTableTypeListFix = true;
        }

        private static void ResetGetActiveRitualsFix()
        {
            EnableGetActiveRitualsFix = true;
        }

        private static void ResetGrowerCutTrees()
        {
            EnableGrowerCutTrees = !FixSymptomProbe.IsGrowerCutTreesPatchLeftover();
            GrowerCutTreesEnableLogging = false;
        }

        private static void ResetIdleErrorWanderFix()
        {
            EnableIdleErrorWanderFix = true;
        }

        private static void ResetHospitalityGuestApparelOptimizeFix()
        {
            EnableHospitalityGuestApparelOptimizeFix = true;
        }

        private static void ResetLowTpsPawnDump()
        {
            EnableLowTpsPawnDump = true;
            LowTpsEnableLogging = false;
            LowTpsColonistsOnly = false;
            LowTpsSkipWhenPaused = false;
            LowTpsLogStartupMessage = false;
            LowTpsThreshold = DefaultLowTpsThreshold;
            LowTpsLogCooldownSeconds = DefaultLowTpsLogCooldownSeconds;
            LowTpsThreshold_Buffer = null;
            LowTpsLogCooldownSeconds_Buffer = null;
        }

        private static void ResetParanoidPerfectionist()
        {
            EnableParanoidPerfectionist = false;
            ParanoidCatchCritical = true;
            ParanoidCatchAnyError = false;
        }

        private static void ResetIdleWorkSearchCooldown()
        {
            EnableIdleWorkSearchCooldown = true;
            IdleWorkSearchCooldownEnableLogging = false;
            IdleWorkSearchCooldownTicks = DefaultIdleWorkSearchCooldownTicks;
            IdleWorkSearchCooldownTicks_Buffer = null;
        }

        private static void ResetDrillLimitPatch()
        {
            EnableDrillLimitPatch = true;
            EnableDrillLimitValueField = true;
            EnableDrillLimitAnyResource = true;
            DrillLimitMax = DefaultDrillLimitMax;
            DrillLimitMax_Buffer = null;
        }

        private static void ResetStartingCorpsesNoHaul()
        {
            EnableStartingCorpsesNoHaul = true;
            StartingCorpseMarkMode = DefaultStartingCorpseMarkMode;
        }

        private static void ResetPtgMedicalCare()
        {
            EnablePtgMedicalCare = true;
        }

        private static void ResetSeedsPleaseSowFix()
        {
            EnableSeedsPleaseSowFix = !FixSymptomProbe.IsSeedsPleaseSowFixLeftover();
        }

        private static void ResetTakeFromMending()
        {
            EnableTakeFromMending = true;
            TakeFromMendingEnableLogging = false;
            TakeFromMendingShowMaintenanceMessages = false;
        }

        private static void ResetTradeCaravanLordFix()
        {
            EnableTradeCaravanLordFix = true;
            TradeCaravanLordEnableLogging = false;
        }

        private static void ResetTraderCaravanStayFix()
        {
            EnableTraderCaravanStayFix = true;
        }

        private static void ResetRatkinMercenaryAgeFix()
        {
            EnableRatkinMercenaryAgeFix = true;
        }

        private static void ResetCompressibleOverlapFix()
        {
            EnableCompressibleOverlapFix = true;
            CompressibleOverlapFixEnableLogging = false;
        }

        private static void ResetApparelPolicyLoadFix()
        {
            EnableApparelPolicyLoadFix = true;
        }

        private static void ResetBillRenamePrefill()
        {
            EnableBillRenamePrefill = true;
        }

        private static void ResetRecipeProductDropLostLog()
        {
            EnableRecipeProductDropLostLog = true;
        }

        private static void ResetNumbersLoadDefaultFallback()
        {
            EnableNumbersLoadDefaultFallback = true;
        }

        private static void ResetAllowToolHaulUrgentlyNreFix()
        {
            EnableAllowToolHaulUrgentlyNreFix = true;
        }

        private static void ResetDubsAnalyzerBeginUpdateFix()
        {
            EnableDubsAnalyzerBeginUpdateFix = true;
        }

        private static void ResetDebugLogSplitterDragFix()
        {
            EnableDebugLogSplitterDragFix = true;
        }

        private static void ResetNeanderthalChiefLeaderFix()
        {
            EnableNeanderthalChiefLeaderFix = !FixSymptomProbe.IsNeanderthalChiefLeaderFixLeftover();
        }

        private static void ResetDominantIngredientStuffFix()
        {
            EnableDominantIngredientStuffFix = !FixSymptomProbe.IsCraftStuffFixLeftover();
        }

        private static void ResetStartingPawnChildAgeFix()
        {
            EnableStartingPawnChildAgeFix = !FixSymptomProbe.IsStartingPawnChildAgeFixLeftover();
        }

        private static void ResetStartingPawnNameFix()
        {
            EnableStartingPawnNameFix = !FixSymptomProbe.IsStartingPawnNameFixLeftover();
        }

        private static void ResetAndroidEnlightenFix()
        {
            EnableAndroidEnlightenFix = true;
            AndroidEnlightenFixFeatures.SyncGivers(force: true);
        }

        private static void ResetArchitectSearchResearchPatch()
        {
            EnableArchitectSearchResearchPatch = !FixSymptomProbe.IsArchitectSearchResearchPatchLeftover();
        }

        private static void ResetMonumentMaterialOnMapPatch()
        {
            EnableMonumentMaterialOnMapPatch = true;
        }

        /// <summary>
        /// Restores the Patches obsolete section, its leftover children, and Do not reset on reload.
        ///
        /// Возвращает раздел «Устаревшие» на вкладке патчей, патчи под ним и «не сбрасывать».
        /// </summary>
        private static void ResetObsoletePatches()
        {
            DoNotResetObsoletePatchesOnReload = false;
            EnableObsoletePatches = false;
            if (FixSymptomProbe.IsArchitectSearchResearchPatchLeftover())
            {
                EnableArchitectSearchResearchPatch = false;
            }

            if (FixSymptomProbe.IsGrowerCutTreesPatchLeftover())
            {
                EnableGrowerCutTrees = false;
                GrowerCutTreesEnableLogging = false;
            }
        }

        /// <summary>
        /// Turns the Patches obsolete section and this launch's leftover patches off when
        /// Do not reset on reload is off.
        ///
        /// Выключает раздел «Устаревшие» на вкладке патчей и leftover-патчи этого запуска,
        /// если «не сбрасывать» выключен.
        /// </summary>
        public static void ApplyObsoletePatchesResetOnGameStart()
        {
            if (DoNotResetObsoletePatchesOnReload)
            {
                return;
            }

            FixSymptomProbe.Ensure();
            EnableObsoletePatches = false;
            if (FixSymptomProbe.IsArchitectSearchResearchPatchLeftover())
            {
                EnableArchitectSearchResearchPatch = false;
            }

            if (FixSymptomProbe.IsGrowerCutTreesPatchLeftover())
            {
                EnableGrowerCutTrees = false;
                GrowerCutTreesEnableLogging = false;
            }
        }

        private static void ResetWikiRimCompat()
        {
            EnableWikiRimCompat = true;
            EnableWikiRimMultilingualSearch = false;
        }

        private static void ResetSaveSettingsLoadFix()
        {
            EnableSaveSettingsLoadFix = true;
        }

        /// <summary>
        /// Defaults the Obsolete section enable, leftover children, and the Do not reset on reload checkbox.
        ///
        /// Возвращает включение раздела «Устаревшие», фиксы под ним и чекбокс «не сбрасывать» к умолчанию.
        /// </summary>
        private static void ResetObsoleteFixes()
        {
            DoNotResetObsoleteFixesOnReload = false;
            ResetObsoleteFixToggles();
            if (FixSymptomProbe.IsCatCrazyTimeLeftover())
            {
                CatCrazyTimeEnableLogging = false;
            }

            if (FixSymptomProbe.IsCraftStuffFixLeftover())
            {
                ResetDominantIngredientStuffFix();
            }

            if (FixSymptomProbe.IsStartingPawnChildAgeFixLeftover())
            {
                ResetStartingPawnChildAgeFix();
            }

            if (FixSymptomProbe.IsNeanderthalChiefLeaderFixLeftover())
            {
                ResetNeanderthalChiefLeaderFix();
            }

            if (FixSymptomProbe.IsStartingPawnNameFixLeftover())
            {
                ResetStartingPawnNameFix();
            }

            if (FixSymptomProbe.IsSeedsPleaseSowFixLeftover())
            {
                ResetSeedsPleaseSowFix();
            }

            if (FixSymptomProbe.IsUfFillFixLeftover())
            {
                ResetUfFillExtraIngredientsFix();
            }

            if (FixSymptomProbe.IsBurnWeaponBillFixLeftover())
            {
                ResetBurnWeaponBillFix();
            }

            if (FixSymptomProbe.IsBreachAxeWorkAmountFixLeftover())
            {
                ResetBreachAxeWorkAmountFix();
            }

            if (FixSymptomProbe.IsRawFungusBillFixLeftover())
            {
                ResetRawFungusBillFix();
            }

            if (FixSymptomProbe.IsAnomalyEspFleeFixLeftover())
            {
                ResetAnomalyEspFleeFix();
            }

            if (FixSymptomProbe.IsMainMenuBgFitFixLeftover())
            {
                ResetMainMenuBgFitFix();
            }

            if (FixSymptomProbe.IsToggleableReadoutsGuiFixLeftover())
            {
                ResetToggleableReadoutsGuiFix();
            }

            if (FixSymptomProbe.IsArchitectInfoBoxFpsFixLeftover())
            {
                ResetArchitectInfoBoxFpsFix();
            }

            if (FixSymptomProbe.IsRimatomicsHelFpsFixLeftover())
            {
                ResetRimatomicsHelFpsFix();
            }

            if (FixSymptomProbe.IsDiseaseComplicationFixLeftover())
            {
                ResetDiseaseComplicationFix();
            }

            if (FixSymptomProbe.IsTraitGainFixLeftover())
            {
                ResetTraitGainFix();
            }

            if (FixSymptomProbe.IsSoldPawnExitGoodwillFixLeftover())
            {
                ResetSoldPawnExitGoodwillFix();
            }
        }

        /// <summary>
        /// Turns Obsolete and leftover children off after settings load when Do not reset on reload is off.
        ///
        /// После загрузки настроек выключает «Устаревшие» и фиксы под ним, если «не сбрасывать» выключен.
        /// </summary>
        public static void ApplyObsoleteFixesResetOnGameStart()
        {
            if (DoNotResetObsoleteFixesOnReload)
            {
                return;
            }

            FixSymptomProbe.Ensure();
            ResetObsoleteFixToggles();
        }

        /// <summary>
        /// Once per settings file, turn still-needed candidates on if they were left off.
        /// Waits until def-backed probes have run.
        ///
        /// Один раз на файл настроек включает ещё нужные фиксы, если они остались выкл.
        /// Ждёт зонды по дефам.
        /// </summary>
        public static void ApplySymptomBucketCurrentDefaultsOnce()
        {
            if (SymptomBucketDefaultsApplied)
            {
                return;
            }

            FixSymptomProbe.Ensure();
            if (!FixSymptomProbe.DefBackedProbesReady)
            {
                return;
            }

            SymptomBucketDefaultsApplied = true;
            if (!FixSymptomProbe.IsCatCrazyTimeLeftover())
            {
                EnableCatCrazyTime = true;
            }

            if (!FixSymptomProbe.IsCraftStuffFixLeftover())
            {
                EnableDominantIngredientStuffFix = true;
            }

            if (!FixSymptomProbe.IsStartingPawnChildAgeFixLeftover())
            {
                EnableStartingPawnChildAgeFix = true;
            }

            if (!FixSymptomProbe.IsNeanderthalChiefLeaderFixLeftover())
            {
                EnableNeanderthalChiefLeaderFix = true;
            }

            if (!FixSymptomProbe.IsStartingPawnNameFixLeftover())
            {
                EnableStartingPawnNameFix = true;
            }

            if (!FixSymptomProbe.IsSeedsPleaseSowFixLeftover())
            {
                EnableSeedsPleaseSowFix = true;
            }

            if (!FixSymptomProbe.IsUfFillFixLeftover())
            {
                EnableUfFillExtraIngredientsFix = true;
            }

            if (!FixSymptomProbe.IsBurnWeaponBillFixLeftover())
            {
                EnableBurnWeaponBillFix = true;
            }

            if (!FixSymptomProbe.IsBreachAxeWorkAmountFixLeftover())
            {
                EnableBreachAxeWorkAmountFix = true;
            }

            if (!FixSymptomProbe.IsRawFungusBillFixLeftover())
            {
                EnableRawFungusBillFix = true;
            }

            if (!FixSymptomProbe.IsAnomalyEspFleeFixLeftover())
            {
                EnableAnomalyEspFleeFix = true;
            }

            if (!FixSymptomProbe.IsMainMenuBgFitFixLeftover())
            {
                EnableMainMenuBgFitFix = true;
            }

            if (!FixSymptomProbe.IsToggleableReadoutsGuiFixLeftover())
            {
                EnableToggleableReadoutsGuiFix = true;
            }

            if (!FixSymptomProbe.IsDiseaseComplicationFixLeftover())
            {
                EnableDiseaseComplicationFix = true;
            }

            if (!FixSymptomProbe.IsTraitGainFixLeftover())
            {
                EnableTraitGainFix = true;
            }

            if (!FixSymptomProbe.IsSoldPawnExitGoodwillFixLeftover())
            {
                EnableSoldPawnExitGoodwillFix = true;
            }

            if (!FixSymptomProbe.IsArchitectSearchResearchPatchLeftover())
            {
                EnableArchitectSearchResearchPatch = true;
            }

            Mod mod = LoadedModManager.GetMod<KebabTweaksMod>();
            if (mod != null)
            {
                mod.WriteSettings();
            }
        }

        private static void ResetObsoleteFixToggles()
        {
            EnableObsoleteFixes = false;
            if (FixSymptomProbe.IsCatCrazyTimeLeftover())
            {
                EnableCatCrazyTime = false;
            }

            if (FixSymptomProbe.IsCraftStuffFixLeftover())
            {
                EnableDominantIngredientStuffFix = false;
            }

            if (FixSymptomProbe.IsStartingPawnChildAgeFixLeftover())
            {
                EnableStartingPawnChildAgeFix = false;
            }

            if (FixSymptomProbe.IsNeanderthalChiefLeaderFixLeftover())
            {
                EnableNeanderthalChiefLeaderFix = false;
            }

            if (FixSymptomProbe.IsStartingPawnNameFixLeftover())
            {
                EnableStartingPawnNameFix = false;
            }

            if (FixSymptomProbe.IsSeedsPleaseSowFixLeftover())
            {
                EnableSeedsPleaseSowFix = false;
            }

            if (FixSymptomProbe.IsUfFillFixLeftover())
            {
                EnableUfFillExtraIngredientsFix = false;
            }

            if (FixSymptomProbe.IsBurnWeaponBillFixLeftover())
            {
                EnableBurnWeaponBillFix = false;
            }

            if (FixSymptomProbe.IsBreachAxeWorkAmountFixLeftover())
            {
                EnableBreachAxeWorkAmountFix = false;
            }

            if (FixSymptomProbe.IsRawFungusBillFixLeftover())
            {
                EnableRawFungusBillFix = false;
            }

            if (FixSymptomProbe.IsAnomalyEspFleeFixLeftover())
            {
                EnableAnomalyEspFleeFix = false;
            }

            if (FixSymptomProbe.IsMainMenuBgFitFixLeftover())
            {
                EnableMainMenuBgFitFix = false;
            }

            if (FixSymptomProbe.IsToggleableReadoutsGuiFixLeftover())
            {
                EnableToggleableReadoutsGuiFix = false;
            }

            if (FixSymptomProbe.IsArchitectInfoBoxFpsFixLeftover())
            {
                EnableArchitectInfoBoxFpsFix = false;
            }

            if (FixSymptomProbe.IsRimatomicsHelFpsFixLeftover())
            {
                EnableRimatomicsHelFpsFix = false;
            }

            if (FixSymptomProbe.IsDiseaseComplicationFixLeftover())
            {
                EnableDiseaseComplicationFix = false;
            }

            if (FixSymptomProbe.IsTraitGainFixLeftover())
            {
                EnableTraitGainFix = false;
            }

            if (FixSymptomProbe.IsSoldPawnExitGoodwillFixLeftover())
            {
                EnableSoldPawnExitGoodwillFix = false;
            }
        }

        private static void ResetMapPreviewRngBaselineFix()
        {
            EnableMapPreviewRngBaselineFix = true;
        }

        private static void ResetUnifiedXmlPathFix()
        {
            EnableUnifiedXmlPathFix = true;
        }

        private static void ResetMainMenuBgFitFix()
        {
            EnableMainMenuBgFitFix = !FixSymptomProbe.IsMainMenuBgFitFixLeftover();
        }

        private static void ResetToggleableReadoutsGuiFix()
        {
            EnableToggleableReadoutsGuiFix = !FixSymptomProbe.IsToggleableReadoutsGuiFixLeftover();
        }

        private static void ResetRimatomicsGuidancePanelFix()
        {
            EnableRimatomicsGuidancePanelFix = true;
        }

        private static void ResetUfFillExtraIngredientsFix()
        {
            EnableUfFillExtraIngredientsFix = !FixSymptomProbe.IsUfFillFixLeftover();
            EnableUfFillCapPickupToBillCount = true;
            UfFillExtraIngredientsFixEnableLogging = false;
        }

        private static void ResetBurnWeaponBillFix()
        {
            EnableBurnWeaponBillFix = !FixSymptomProbe.IsBurnWeaponBillFixLeftover();
            BurnWeaponBillFixFeatures.SyncRecipeWorkTypes(force: true);
        }

        private static void ResetBreachAxeWorkAmountFix()
        {
            EnableBreachAxeWorkAmountFix = !FixSymptomProbe.IsBreachAxeWorkAmountFixLeftover();
            BreachAxeWorkAmountFixFeatures.SyncWorkAmount(force: true);
        }

        private static void ResetRawFungusBillFix()
        {
            EnableRawFungusBillFix = !FixSymptomProbe.IsRawFungusBillFixLeftover();
            RawFungusBillFixFeatures.SyncCategory(force: true);
        }

        private static void ResetAnomalyEspFleeFix()
        {
            EnableAnomalyEspFleeFix = !FixSymptomProbe.IsAnomalyEspFleeFixLeftover();
        }

        private static void ResetStratumSolarRoofEnergyTabFix()
        {
            EnableStratumSolarRoofEnergyTabFix = true;
        }

        private static void ResetArchitectInfoBoxFpsFix()
        {
            EnableArchitectInfoBoxFpsFix = !FixSymptomProbe.IsArchitectInfoBoxFpsFixLeftover();
        }

        private static void ResetRimatomicsHelFpsFix()
        {
            EnableRimatomicsHelFpsFix = !FixSymptomProbe.IsRimatomicsHelFpsFixLeftover();
        }

        private static void ResetDiseaseComplicationFix()
        {
            EnableDiseaseComplicationFix = !FixSymptomProbe.IsDiseaseComplicationFixLeftover();
        }

        private static void ResetTraitGainFix()
        {
            EnableTraitGainFix = !FixSymptomProbe.IsTraitGainFixLeftover();
        }

        private static void ResetSoldPawnExitGoodwillFix()
        {
            EnableSoldPawnExitGoodwillFix = !FixSymptomProbe.IsSoldPawnExitGoodwillFixLeftover();
        }

        public override void ExposeData()
        {
            base.ExposeData();

#if RIMWORLD_1_6
            Scribe_Values.Look(ref EnableCatCrazyTime, "EnableCatCrazyTime", defaultValue: false);
#else
            Scribe_Values.Look(ref EnableCatCrazyTime, "EnableCatCrazyTime", defaultValue: true);
#endif
            Scribe_Values.Look(ref EnableCatFloorSleep, "EnableCatFloorSleep", defaultValue: true);
            Scribe_Values.Look(ref EnableCeRunForCoverDestFix, "EnableCeRunForCoverDestFix", defaultValue: true);
            Scribe_Values.Look(ref EnableCeProjectileNullSoundFix, "EnableCeProjectileNullSoundFix", defaultValue: true);
            Scribe_Values.Look(ref EnableCeExtendedLoadoutMedicineLabelFix,
                "EnableCeExtendedLoadoutMedicineLabelFix", defaultValue: true);
            Scribe_Values.Look(ref EnableArmorRacksAssignFix, "EnableArmorRacksAssignFix", defaultValue: true);
            Scribe_Values.Look(ref EnableStorageSettingsAllowedToAcceptFix,
                "EnableStorageSettingsAllowedToAcceptFix", defaultValue: true);
            Scribe_Values.Look(ref EnableFishTableTypeListFix, "EnableFishTableTypeListFix", defaultValue: true);
            Scribe_Values.Look(ref EnableGetActiveRitualsFix, "EnableGetActiveRitualsFix", defaultValue: true);
            Scribe_Values.Look(ref EnableGrowerCutTrees, "EnableGrowerCutTrees", defaultValue: true);
            Scribe_Values.Look(ref EnableIdleErrorWanderFix, "EnableIdleErrorWanderFix", defaultValue: true);
            Scribe_Values.Look(ref EnableHospitalityGuestApparelOptimizeFix,
                "EnableHospitalityGuestApparelOptimizeFix", defaultValue: true);
            Scribe_Values.Look(ref EnableLowTpsPawnDump, "EnableLowTpsPawnDump", defaultValue: true);
            Scribe_Values.Look(ref EnableIdleWorkSearchCooldown, "EnableIdleWorkSearchCooldown",
                defaultValue: true);
            Scribe_Values.Look(ref EnableParanoidPerfectionist, "EnableParanoidPerfectionist", defaultValue: false);
            Scribe_Values.Look(ref ParanoidCatchCritical, "ParanoidCatchCritical", defaultValue: true);
            Scribe_Values.Look(ref ParanoidCatchAnyError, "ParanoidCatchAnyError", defaultValue: false);
            Scribe_Values.Look(ref EnableDrillLimitPatch, "EnableDrillLimitPatch", defaultValue: true);
            Scribe_Values.Look(ref EnableDrillLimitValueField, "EnableDrillLimitValueField", defaultValue: true);
            Scribe_Values.Look(ref EnableDrillLimitAnyResource, "EnableDrillLimitAnyResource", defaultValue: true);
            Scribe_Values.Look(ref DrillLimitMax, "DrillLimitMax", DefaultDrillLimitMax);
            if (DrillLimitMax < 0)
            {
                DrillLimitMax = 0;
            }
            else if (DrillLimitMax > MaxDrillLimitMax)
            {
                DrillLimitMax = MaxDrillLimitMax;
            }
            Scribe_Values.Look(ref EnableStartingCorpsesNoHaul, "EnableStartingCorpsesNoHaul", defaultValue: true);
            Scribe_Values.Look(ref StartingCorpseMarkMode, "StartingCorpseMarkMode",
                defaultValue: DefaultStartingCorpseMarkMode);
            if (StartingCorpseMarkMode < 0 || StartingCorpseMarkMode > 2)
            {
                StartingCorpseMarkMode = DefaultStartingCorpseMarkMode;
            }
            Scribe_Values.Look(ref EnablePtgMedicalCare, "EnablePtgMedicalCare", defaultValue: true);
            Scribe_Values.Look(ref EnableSeedsPleaseSowFix, "EnableSeedsPleaseSowFix", defaultValue: true);
            Scribe_Values.Look(ref EnableTakeFromMending, "EnableTakeFromMending", defaultValue: true);
            Scribe_Values.Look(ref EnableTradeCaravanLordFix, "EnableTradeCaravanLordFix", defaultValue: true);
            Scribe_Values.Look(ref EnableTraderCaravanStayFix, "EnableTraderCaravanStayFix", defaultValue: true);
            Scribe_Values.Look(ref EnableRatkinMercenaryAgeFix, "EnableRatkinMercenaryAgeFix", defaultValue: true);
            Scribe_Values.Look(ref EnableCompressibleOverlapFix, "EnableCompressibleOverlapFix", defaultValue: true);
            Scribe_Values.Look(ref EnableApparelPolicyLoadFix, "EnableApparelPolicyLoadFix", defaultValue: true);
            Scribe_Values.Look(ref EnableBillRenamePrefill, "EnableBillRenamePrefill", defaultValue: true);
            Scribe_Values.Look(ref EnableRecipeProductDropLostLog, "EnableRecipeProductDropLostLog", defaultValue: true);
            Scribe_Values.Look(ref EnableNumbersLoadDefaultFallback, "EnableNumbersLoadDefaultFallback",
                defaultValue: true);
            Scribe_Values.Look(ref EnableAllowToolHaulUrgentlyNreFix, "EnableAllowToolHaulUrgentlyNreFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableDubsAnalyzerBeginUpdateFix, "EnableDubsAnalyzerBeginUpdateFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableDebugLogSplitterDragFix, "EnableDebugLogSplitterDragFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableNeanderthalChiefLeaderFix, "EnableNeanderthalChiefLeaderFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableDominantIngredientStuffFix, "EnableDominantIngredientStuffFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableStartingPawnChildAgeFix, "EnableStartingPawnChildAgeFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableStartingPawnNameFix, "EnableStartingPawnNameFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableAndroidEnlightenFix, "EnableAndroidEnlightenFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableArchitectSearchResearchPatch, "EnableArchitectSearchResearchPatch",
                defaultValue: true);
            Scribe_Values.Look(ref EnableMonumentMaterialOnMapPatch, "EnableMonumentMaterialOnMapPatch",
                defaultValue: true);
            Scribe_Values.Look(ref EnableObsoletePatches, "EnableObsoletePatches", defaultValue: false);
            Scribe_Values.Look(ref DoNotResetObsoletePatchesOnReload, "DoNotResetObsoletePatchesOnReload",
                defaultValue: false);
            Scribe_Values.Look(ref EnableWikiRimCompat, "EnableWikiRimCompat", defaultValue: true);
            Scribe_Values.Look(ref EnableWikiRimMultilingualSearch, "EnableWikiRimMultilingualSearch",
                defaultValue: false);
            Scribe_Values.Look(ref EnableSaveSettingsLoadFix, "EnableSaveSettingsLoadFix", defaultValue: true);
            Scribe_Values.Look(ref EnableObsoleteFixes, "EnableObsoleteFixes", defaultValue: false);
            Scribe_Values.Look(ref DoNotResetObsoleteFixesOnReload, "DoNotResetObsoleteFixesOnReload",
                defaultValue: false);
            Scribe_Values.Look(ref SymptomBucketDefaultsApplied, "SymptomBucketDefaultsApplied",
                defaultValue: false);
            Scribe_Values.Look(ref EnableMapPreviewRngBaselineFix, "EnableMapPreviewRngBaselineFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableUnifiedXmlPathFix, "EnableUnifiedXmlPathFix", defaultValue: true);
            Scribe_Values.Look(ref EnableMainMenuBgFitFix, "EnableMainMenuBgFitFix", defaultValue: true);
            Scribe_Values.Look(ref EnableToggleableReadoutsGuiFix, "EnableToggleableReadoutsGuiFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableRimatomicsGuidancePanelFix, "EnableRimatomicsGuidancePanelFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableUfFillExtraIngredientsFix, "EnableUfFillExtraIngredientsFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableUfFillCapPickupToBillCount, "EnableUfFillCapPickupToBillCount",
                defaultValue: true);
            Scribe_Values.Look(ref UfFillExtraIngredientsFixEnableLogging,
                "UfFillExtraIngredientsFixEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref EnableBurnWeaponBillFix, "EnableBurnWeaponBillFix",
                defaultValue: false);
            Scribe_Values.Look(ref EnableBreachAxeWorkAmountFix, "EnableBreachAxeWorkAmountFix",
                defaultValue: false);
            Scribe_Values.Look(ref EnableRawFungusBillFix, "EnableRawFungusBillFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableAnomalyEspFleeFix, "EnableAnomalyEspFleeFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableStratumSolarRoofEnergyTabFix, "EnableStratumSolarRoofEnergyTabFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableArchitectInfoBoxFpsFix, "EnableArchitectInfoBoxFpsFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableRimatomicsHelFpsFix, "EnableRimatomicsHelFpsFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableDiseaseComplicationFix, "EnableDiseaseComplicationFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableTraitGainFix, "EnableTraitGainFix",
                !FixSymptomProbe.IsTraitGainFixLeftover());
            Scribe_Values.Look(ref EnableSoldPawnExitGoodwillFix, "EnableSoldPawnExitGoodwillFix",
                !FixSymptomProbe.IsSoldPawnExitGoodwillFixLeftover());

            Scribe_Values.Look(ref CatCrazyTimeEnableLogging, "CatCrazyTimeEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref CatFloorSleepEnableLogging, "CatFloorSleepEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref GrowerCutTreesEnableLogging, "GrowerCutTreesEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref TakeFromMendingEnableLogging, "TakeFromMendingEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref TakeFromMendingShowMaintenanceMessages, "TakeFromMendingShowMaintenanceMessages",
                defaultValue: false);
            Scribe_Values.Look(ref TradeCaravanLordEnableLogging, "TradeCaravanLordEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref CompressibleOverlapFixEnableLogging, "CompressibleOverlapFixEnableLogging",
                defaultValue: false);

            Scribe_Values.Look(ref LowTpsEnableLogging, "LowTpsEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref LowTpsColonistsOnly, "LowTpsColonistsOnly", defaultValue: false);
            Scribe_Values.Look(ref LowTpsSkipWhenPaused, "LowTpsSkipWhenPaused", defaultValue: false);
            Scribe_Values.Look(ref LowTpsLogStartupMessage, "LowTpsLogStartupMessage", defaultValue: false);
            Scribe_Values.Look(ref LowTpsThreshold, "LowTpsThreshold", defaultValue: DefaultLowTpsThreshold);
            Scribe_Values.Look(ref LowTpsLogCooldownSeconds, "LowTpsLogCooldownSeconds",
                defaultValue: DefaultLowTpsLogCooldownSeconds);

            Scribe_Values.Look(ref IdleWorkSearchCooldownEnableLogging,
                "IdleWorkSearchCooldownEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref IdleWorkSearchCooldownTicks, "IdleWorkSearchCooldownTicks",
                defaultValue: DefaultIdleWorkSearchCooldownTicks);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                IdleWorkSearchCooldownTicks = Mathf.Clamp(
                    IdleWorkSearchCooldownTicks, 0, MaxIdleWorkSearchCooldownTicks);
            }

            Scribe_Values.Look(ref KebabSwitchesEnableLogging, "KebabSwitchesEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref SuppressFilledMapMessage, "SuppressFilledMapMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressGaveBirthMessage, "SuppressGaveBirthMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressAnimalIsPregnantMessage, "SuppressAnimalIsPregnantMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressMiscarriedStarvationMessage, "SuppressMiscarriedStarvationMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressMiscarriedPoorHealthMessage, "SuppressMiscarriedPoorHealthMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressSeasonBegunMessage, "SuppressSeasonBegunMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressBillCompleteMessage, "SuppressBillCompleteMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressFullyHealedMessage, "SuppressFullyHealedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressSocialFightMessage, "SuppressSocialFightMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressShipChunkDropMessage, "SuppressShipChunkDropMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressStruckMineableMessage, "SuppressStruckMineableMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressBedLostAssignmentMessage, "SuppressBedLostAssignmentMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressBedDestroyedMessage, "SuppressBedDestroyedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressSleepingPawnsWokenUpMessage, "SuppressSleepingPawnsWokenUpMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressNoLongerSocialFightingMessage, "SuppressNoLongerSocialFightingMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressNoLongerTargetedInsultMessage, "SuppressNoLongerTargetedInsultMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressInsultingSpreeEndedMessage, "SuppressInsultingSpreeEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressTargetedInsultStreamEndedMessage, "SuppressTargetedInsultStreamEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressFoodBingeEndedMessage, "SuppressFoodBingeEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressNoLongerBingingOnDrugMessage, "SuppressNoLongerBingingOnDrugMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressBerserkEndedMessage, "SuppressBerserkEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressFireStartingEndedMessage, "SuppressFireStartingEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressJailbreakerEndedMessage, "SuppressJailbreakerEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressSlaughtererEndedMessage, "SuppressSlaughtererEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressMurderousRageEndedMessage, "SuppressMurderousRageEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressGiveUpReturnedMessage, "SuppressGiveUpReturnedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressTantrumEndedMessage, "SuppressTantrumEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressRedirectedAngerMessage, "SuppressRedirectedAngerMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressSadisticRageEndedMessage, "SuppressSadisticRageEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressCorpseObsessionEndedMessage, "SuppressCorpseObsessionEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressWanderSadEndedMessage, "SuppressWanderSadEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressHideInRoomEndedMessage, "SuppressHideInRoomEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressPsychoticWanderEndedMessage, "SuppressPsychoticWanderEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressConfusedEndedMessage, "SuppressConfusedEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressPanicFleeEndedMessage, "SuppressPanicFleeEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressManhunterEndedMessage, "SuppressManhunterEndedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressNewBondRelationMessage, "SuppressNewBondRelationMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressNewBondRelationNewNameMessage, "SuppressNewBondRelationNewNameMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressFoodPoisoningMessage, "SuppressFoodPoisoningMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressRoamerLeavingMessage, "SuppressRoamerLeavingMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressHiveReproducedMessage, "SuppressHiveReproducedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressTraderCaravanLeavingMessage, "SuppressTraderCaravanLeavingMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressTraderCaravanDismissedMessage, "SuppressTraderCaravanDismissedMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressCompSpawnerSpawnedItemMessage, "SuppressCompSpawnerSpawnedItemMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressFishCaughtInTrap, "SuppressFishCaughtInTrap", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfCold, "SuppressPlantDiedOfCold", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfRotUnharvested, "SuppressPlantDiedOfRotUnharvested",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfRotLight, "SuppressPlantDiedOfRotLight", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfRot, "SuppressPlantDiedOfRot", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfPoison, "SuppressPlantDiedOfPoison", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfBlight, "SuppressPlantDiedOfBlight", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfPollution, "SuppressPlantDiedOfPollution", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfNoPollution, "SuppressPlantDiedOfNoPollution",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfRotPollutedTerrain, "SuppressPlantDiedOfRotPollutedTerrain",
                defaultValue: false);
#if RIMWORLD_1_6
            Scribe_Values.Look(ref SuppressPlantDiedOfVacuum, "SuppressPlantDiedOfVacuum", defaultValue: false);
            Scribe_Values.Look(ref SuppressEspInjuredMessage, "SuppressEspInjuredMessage", defaultValue: false);
#endif
            Scribe_Values.Look(ref SuppressMinifiedTreeDied, "SuppressMinifiedTreeDied", defaultValue: false);
            Scribe_Values.Look(ref SuppressRottedAwayInStorage, "SuppressRottedAwayInStorage", defaultValue: false);
            Scribe_Values.Look(ref SuppressDeterioratedAway, "SuppressDeterioratedAway", defaultValue: false);
            Scribe_Values.Look(ref SuppressWornApparelDeterioratedAway, "SuppressWornApparelDeterioratedAway",
                defaultValue: false);
            Scribe_Collections.Look(ref SuppressLetterIds, "SuppressLetterIds", LookMode.Value);
            EnsureSuppressLetterIds();
            bool legacySurgeryFailedLetter = false;
            Scribe_Values.Look(ref legacySurgeryFailedLetter, "SuppressSurgeryFailedLetter", defaultValue: false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && legacySurgeryFailedLetter)
            {
                SuppressLetterIds.Add("SurgeryFailed");
            }

            Scribe_Collections.Look(ref SuppressAlertIds, "SuppressAlertIds", LookMode.Value);
            EnsureSuppressAlertIds();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                ApplyObsoleteFixesResetOnGameStart();
                ApplyObsoletePatchesResetOnGameStart();
            }
        }
    }
}
