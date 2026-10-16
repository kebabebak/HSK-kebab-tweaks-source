using System.Collections.Generic;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// One yellow warning per feature that did not apply this launch, and a flag the settings
    /// row uses to veil that feature. A second report for the same feature does not log again.
    ///
    /// Одно жёлтое предупреждение на фичу, которая в этом запуске не встала, и флаг, по которому
    /// строка настроек затемняется. Повторный отчёт по той же фиче в лог не пишется.
    /// </summary>
    public static class FeatureAvailability
    {
        public const string KebabSwitches = "KebabSwitchesFeatures";
        public const string LowTps = "LowTpsPawnDumpFeatures";
        public const string DrillLimit = "DrillLimitFeatures";
        public const string IdleWorkSearch = "IdleWorkSearchCooldownFeatures";
        public const string Grower = "GrowerCutTreesFeatures";
        public const string StartingCorpses = "StartingCorpsesNoHaulFeatures";
        public const string ApparelPolicy = "ApparelPolicyLoadFixFeatures";
        public const string BillRename = "BillRenamePrefillFeatures";
        public const string RecipeDrop = "RecipeProductDropLostLogFeatures";
        public const string Numbers = "NumbersLoadDefaultFallbackFeatures";
        public const string AndroidEnlighten = "AndroidEnlightenFixFeatures";
        public const string WikiRim = "WikiRimCompatFeatures";
        public const string TakeFromMending = "TakeFromMendingFeatures";
        public const string CatFloorSleep = "CatFloorSleepFeatures";
        public const string TradeCaravan = "TradeCaravanLordFixFeatures";
        public const string Compressible = "CompressibleOverlapFixFeatures";
        public const string TraderStay = "TraderCaravanStayFixFeatures";
        public const string Ratkin = "RatkinMercenaryAgeFixFeatures";
        public const string ArmorRacks = "ArmorRacksAssignFixFeatures";
        public const string SaveSettings = "SaveSettingsLoadFixFeatures";
        public const string StorageSettings = "StorageSettingsAllowedToAcceptFixFeatures";
        public const string CeRunForCover = "CeRunForCoverDestFixFeatures";
        public const string CeProjectile = "CeProjectileNullSoundFixFeatures";
        public const string CeExtendedLoadout = "CeExtendedLoadoutMedicineLabelFixFeatures";
        public const string FishTable = "FishTableTypeListFixFeatures";
        public const string GetActiveRituals = "GetActiveRitualsFixFeatures";
        public const string IdleErrorWander = "IdleErrorWanderFixFeatures";
        public const string AllowTool = "AllowToolHaulUrgentlyNreFixFeatures";
        public const string DubsAnalyzer = "DubsAnalyzerBeginUpdateFixFeatures";
        public const string DebugLog = "DebugLogSplitterDragFixFeatures";
        public const string Hospitality = "HospitalityGuestApparelOptimizeFixFeatures";
        public const string PtgMedical = "PtgMedicalCareFeatures";
        public const string TribalLeader = "NeanderthalChiefLeaderFixFeatures";
        public const string StratumSolar = "StratumSolarRoofEnergyTabFixFeatures";
        public const string MapPreview = "MapPreviewRngBaselineFixFeatures";
        public const string UnifiedXml = "UnifiedXmlPathFixFeatures";
        public const string Rimatomics = "RimatomicsGuidancePanelFixFeatures";
        public const string CatCrazyTime = "CatCrazyTimeFeatures";
        public const string CraftStuff = "DominantIngredientStuffFixFeatures";
        public const string StartingPawnChildAge = "StartingPawnChildAgeFixFeatures";
        public const string StartingPawnName = "StartingPawnNameFixFeatures";
        public const string SeedsPlease = "SeedsPleaseSowFixFeatures";
        public const string UfFill = "UfFillExtraIngredientsFixFeatures";
        public const string MainMenuBg = "MainMenuBgFitFixFeatures";
        public const string BurnWeapon = "BurnWeaponBillFixFeatures";
        public const string BreachAxe = "BreachAxeWorkAmountFixFeatures";
        public const string RawFungus = "RawFungusBillFixFeatures";
        public const string AnomalyEsp = "AnomalyEspFleeFixFeatures";
        public const string ToggleableReadouts = "ToggleableReadoutsGuiFixFeatures";

        public const string CoreSk = "Core SK (skyarkhangel.HSK)";

        static readonly HashSet<string> blocked = new HashSet<string>();

        static readonly HashSet<string> warned = new HashSet<string>();

        public static bool IsBlocked(string featureId)
        {
            return !featureId.NullOrEmpty() && blocked.Contains(featureId);
        }

        /// <summary>
        /// Blocks the feature and logs one warning naming every required mod that is absent.
        ///
        /// Блокирует фичу и пишет одно предупреждение со всеми отсутствующими требуемыми модами.
        /// </summary>
        public static void SkipMissingMods(string featureId, string featureName, params string[] missingMods)
        {
            if (featureId.NullOrEmpty() || !blocked.Add(featureId))
            {
                return;
            }

            string listed = missingMods == null || missingMods.Length == 0
                ? "required mod"
                : string.Join(", ", missingMods);
            Log.Warning("[HSK kebab tweaks] " + featureName + " skipped: required mod not active: " + listed + ".");
        }

        /// <summary>
        /// Blocks the feature and logs one warning for a skip kebab tweaks already handles.
        ///
        /// Блокирует фичу и пишет одно предупреждение о пропуске, который kebab tweaks уже обрабатывает.
        /// </summary>
        public static void SkipHandled(string featureId, string featureName, string reason)
        {
            if (featureId.NullOrEmpty() || !blocked.Add(featureId))
            {
                return;
            }

            Log.Warning("[HSK kebab tweaks] " + featureName + " skipped: " + reason);
        }

        /// <summary>
        /// Logs one warning without blocking the feature. Used when only an optional hook is skipped.
        ///
        /// Одно предупреждение без блокировки фичи. Для случая, когда пропущен только необязательный хук.
        /// </summary>
        public static void WarnOnce(string key, string message)
        {
            if (key.NullOrEmpty() || !warned.Add(key))
            {
                return;
            }

            Log.Warning(message);
        }
    }
}
