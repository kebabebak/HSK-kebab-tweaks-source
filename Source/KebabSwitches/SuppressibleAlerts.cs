using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// One suppressible right-side HUD Alert (AlertsReadout active list).
    ///
    /// Одно отключаемое Alert справа (активный список AlertsReadout).
    /// </summary>
    public sealed class SuppressibleAlertEntry
    {
        public string Id { get; }
        public string TranslationKey { get; }
        public string HardcodedEnglish { get; }

        public SuppressibleAlertEntry(string id, string translationKey = null, string hardcodedEnglish = null)
        {
            Id = id;
            TranslationKey = translationKey;
            HardcodedEnglish = hardcodedEnglish;
        }

        public string CheckboxLabel
        {
            get
            {
                string quote = AlertLabelPreview.GetQuote(this);
                return "KebabSwitches.IgnoreAlert".Translate(quote.Named("QUOTE"));
            }
        }

        /// <summary>
        /// RimWorld tooltip: vanilla keyed label template (English) or hardcoded literal.
        ///
        /// RimWorld-тултип: vanilla keyed label (EN) или литерал.
        /// </summary>
        public string SourceTooltip => NotificationSourceTooltip.ForAlert(this);

        public bool IsSuppressEnabled => KebabTweaksSettings.IsAlertSuppressed(Id);

        public void SetSuppressEnabled(bool value) => KebabTweaksSettings.SetAlertSuppressed(Id, value);
    }

    /// <summary>
    /// Catalog of vanilla Core + DLC Alert types (RimWorld.Alert subclasses) for the right-side HUD list.
    ///
    /// Каталог vanilla Core + DLC Alert (подклассы RimWorld.Alert) для списка справа на HUD.
    /// </summary>
    public static class SuppressibleAlerts
    {
        public static readonly SuppressibleAlertEntry[] All =
        {            new SuppressibleAlertEntry("AbandonedBaby", "AlertAbandonedBaby"),
            new SuppressibleAlertEntry("ActivatorCountdown", "ActivatorCountdown"),
            new SuppressibleAlertEntry("AgeReversalDemandNear", "AlertAgeReversalDemandNear"),
            new SuppressibleAlertEntry("Analyzable_GrayFlesh", "AlertGrayFleshSample"),
            new SuppressibleAlertEntry("Analyzable_NeuralLump", "AlertNeuralLump"),
            new SuppressibleAlertEntry("Analyzable_RevenantFlesh", "AlertRevenantFlesh"),
            new SuppressibleAlertEntry("AnimalFilth", "AlertAnimalFilth"),
            new SuppressibleAlertEntry("AnimaLinkingReady", "AnimaLinkingReadyLabel"),
            new SuppressibleAlertEntry("AnimalPenNeeded", "AlertAnimalPenNeeded"),
            new SuppressibleAlertEntry("AnimalPenNotEnclosed", "AlertAnimalPenNotEnclosed"),
            new SuppressibleAlertEntry("AnimalRoaming", "AlertAnimalIsRoaming"),
            new SuppressibleAlertEntry("AwaitingMedicalOperation", "PatientsAwaitingMedicalOperation"),
            new SuppressibleAlertEntry("BestowerWaiting", "BestowerWaitingAlert"),
            new SuppressibleAlertEntry("BilliardsTableOnWall", "BilliardsNeedsSpace"),
            new SuppressibleAlertEntry("Biostarvation", "Biostarvation"),
            new SuppressibleAlertEntry("Boredom", "Boredom"),
            new SuppressibleAlertEntry("BrawlerHasRangedWeapon", "BrawlerHasRangedWeapon"),
            new SuppressibleAlertEntry("CannotBeUsedRoofed", "BuildingCantBeUsedRoofed"),
            new SuppressibleAlertEntry("CaravanIdle", "CaravanIdle"),
            new SuppressibleAlertEntry("CasketOpening", "AlertCasketOpening"),
            new SuppressibleAlertEntry("ChessTableNoChairs", "ChessTablesNeedChairs"),
            new SuppressibleAlertEntry("ColonistLeftUnburied", "AlertColonistLeftUnburied"),
            new SuppressibleAlertEntry("ColonistNeedsRescuing", "ColonistsNeedRescue"),
            new SuppressibleAlertEntry("ColonistNeedsTend", "ColonistNeedsTreatment"),
            new SuppressibleAlertEntry("ColonistsIdle", "ColonistsIdle"),
            new SuppressibleAlertEntry("ConnectedPawnNotAssignedToPlantCutting", "AlertConnectedPawnNotAssignedToPlantCuttingLabel"),
            new SuppressibleAlertEntry("CreepJoinerTimeout", "CreepJoinerTimeout"),
            new SuppressibleAlertEntry("CubeWithdrawal", "AlertCubeWithdrawal"),
            new SuppressibleAlertEntry("CultistPsychicRitual", "Alert_CultistPsychicRitual"),
            new SuppressibleAlertEntry("Custom", translationKey: null, hardcodedEnglish: "Custom alert (dynamic label)"),
            new SuppressibleAlertEntry("CustomCritical", translationKey: null, hardcodedEnglish: "Custom critical alert (dynamic label)"),
            new SuppressibleAlertEntry("DangerousActivity", "ActivityMultipleDangerous"),
            new SuppressibleAlertEntry("DateRitualComing", "AlertRitualComing"),
            new SuppressibleAlertEntry("DeathrestComplete", "AlertDeathrestComplete"),
            new SuppressibleAlertEntry("Digesting", "AlertDigestion"),
            new SuppressibleAlertEntry("DisallowedBuildingInsideMonument", "DisallowedBuildingInsideMonument"),
            new SuppressibleAlertEntry("DormanyWakeUpDelay", "AlertHostilesWakingUp"),
            new SuppressibleAlertEntry("EmergingPitGate", "AlertEmergingPitGate"),
            new SuppressibleAlertEntry("EntityNeedsTend", "EntityNeedsTreatment"),
            new SuppressibleAlertEntry("Exhaustion", "Exhaustion"),
            new SuppressibleAlertEntry("FireInHomeArea", "FireInHomeArea"),
            new SuppressibleAlertEntry("FuelNodeIgnition", "AlertFuelNodeIgniting"),
            new SuppressibleAlertEntry("GauranlenTreeWithoutProductionMode", "AlertGauranlenTreeWithoutDryadTypeLabel"),
            new SuppressibleAlertEntry("GenebankUnpowered", "AlertGenebankUnpowered"),
            new SuppressibleAlertEntry("GhoulHypothermia", "AlertGhoulHypothermia"),
            new SuppressibleAlertEntry("Heatstroke", "AlertHeatstroke"),
            new SuppressibleAlertEntry("HitchedAnimalHungryNoFood", "AlertHitchedAnimalHungryNoFood"),
            new SuppressibleAlertEntry("HunterHasShieldAndRangedWeapon", "HunterHasShieldAndRangedWeapon"),
            new SuppressibleAlertEntry("HunterLacksRangedWeapon", "HunterLacksWeapon"),
            new SuppressibleAlertEntry("Hypothermia", "AlertHypothermia"),
            new SuppressibleAlertEntry("HypothermicAnimals", translationKey: null, hardcodedEnglish: "Hypothermic wild animals (debug)"),
            new SuppressibleAlertEntry("ImmobileCaravan", "ImmobileCaravan"),
            new SuppressibleAlertEntry("InfestationDelay", "AlertInfestationArriving"),
            new SuppressibleAlertEntry("InhibitorBlocked", "AlertInhibitorBlocked"),
            new SuppressibleAlertEntry("IdeoBuildingDisrespected", "IdeoBuildingDisrespected"),
            new SuppressibleAlertEntry("IdeoBuildingMissing", "IdeoBuildingMissing"),
            new SuppressibleAlertEntry("InsufficientContainmentStrength", "Alert_InsufficientContainment"),
            new SuppressibleAlertEntry("LifeThreateningHediff", "PawnsWithLifeThreateningDisease"),
            new SuppressibleAlertEntry("LowBabyFood", "AlertLowBabyFood"),
            new SuppressibleAlertEntry("LowDeathrest", "AlertLowDeathrestPawns"),
            new SuppressibleAlertEntry("LowFood", "LowFood"),
            new SuppressibleAlertEntry("LowHemogen", "AlertLowHemogen"),
            new SuppressibleAlertEntry("LowMedicine", "LowMedicine"),
            new SuppressibleAlertEntry("MajorOrExtremeBreakRisk", "BreakRiskMajor"),
            new SuppressibleAlertEntry("MeatHunger", "AlertMeatHunger"),
            new SuppressibleAlertEntry("MechChargerFull", "AlertRechargerFull"),
            new SuppressibleAlertEntry("MechDamaged", "AlertMechNeedsRepair"),
            new SuppressibleAlertEntry("MechMissingBodyPart", "AlertMechMissingBodyPart"),
            new SuppressibleAlertEntry("MinifiedTreeAboutToDie", "AlertMinifiedTreeAboutToDie"),
            new SuppressibleAlertEntry("MinorBreakRisk", "BreakRiskMinor"),
            new SuppressibleAlertEntry("MonolithAutoActivating", "MonolithTwistingAlert"),
            new SuppressibleAlertEntry("MonumentMarkerMissingBlueprints", "MonumentMarkerMissingBlueprints"),
            new SuppressibleAlertEntry("NeedAnomalyProject", "NeedAnomalyProject"),
            new SuppressibleAlertEntry("NeedBabyCribs", "AlertNeedBabyCribs"),
            new SuppressibleAlertEntry("NeedBatteries", "NeedBatteries"),
            new SuppressibleAlertEntry("NeedColonistBeds", "NeedColonistBeds"),
            new SuppressibleAlertEntry("NeedDefenses", "NeedDefenses"),
            new SuppressibleAlertEntry("NeedDoctor", "NeedDoctor"),
            new SuppressibleAlertEntry("NeedHoldingPlatform", "AlertHoldingPlatform"),
            new SuppressibleAlertEntry("NeedJoySources", "NeedJoySource"),
            new SuppressibleAlertEntry("NeedMealSource", "NeedMealSource"),
            new SuppressibleAlertEntry("NeedMechChargers", "AlertNeedMechChargers"),
            new SuppressibleAlertEntry("NeedMeditationSpot", "NeedMeditationSpotAlert"),
            new SuppressibleAlertEntry("NeedMiner", "NeedMiner"),
            new SuppressibleAlertEntry("NeedResearchBench", "NeedResearchBench"),
            new SuppressibleAlertEntry("NeedResearchProject", "NeedResearchProject"),
            new SuppressibleAlertEntry("NeedSlaveBeds", "NeedSlaveBeds"),
            new SuppressibleAlertEntry("NeedSlaveCribs", "AlertNeedSlaveCribs"),
            new SuppressibleAlertEntry("NeedWarden", "NeedWarden"),
            new SuppressibleAlertEntry("NeedWarmClothes", "NeedWarmClothes"),
            new SuppressibleAlertEntry("NoBabyFeeders", "AlertNoBabyFeeder"),
            new SuppressibleAlertEntry("NoBabyFoodCaravan", "AlertNoBabyFoodCaravan"),
            new SuppressibleAlertEntry("PasteDispenserNeedsHopper", "NeedFoodHopper"),
            new SuppressibleAlertEntry("PennedAnimalHungry", "AlertPennedAnimalHungry"),
            new SuppressibleAlertEntry("PermitAvailable", "PermitChoiceReadyAlert"),
            new SuppressibleAlertEntry("PokerTableNoChairs", "PokerTablesNeedChairs"),
            new SuppressibleAlertEntry("PollutedTerrain", "AlertPollutedTerrain"),
            new SuppressibleAlertEntry("PredatorInPen", "AlertPredatorInAnimalPen"),
            new SuppressibleAlertEntry("PsychicBondedSeparated", "AlertPsychicBondedPawnsSeparated"),
            new SuppressibleAlertEntry("QuestExpiresSoon", "QuestExpiresSoon"),
            new SuppressibleAlertEntry("ReimplantationAvailable", "AlertReimplantationAvailable"),
            new SuppressibleAlertEntry("RolesEmpty", "IdeoRolesEmpty"),
            new SuppressibleAlertEntry("RoyalNoAcceptableFood", "RoyalNoAcceptableFood"),
            new SuppressibleAlertEntry("RoyalNoThroneAssigned", "NeedThroneAssigned"),
            new SuppressibleAlertEntry("ShieldUserHasRangedWeapon", "ShieldUserHasRangedWeapon"),
            new SuppressibleAlertEntry("ShuttleLandingBeaconUnusable", "ShipLandingBeaconUnusable"),
            new SuppressibleAlertEntry("SlaveRebellionLikely", "AlertSlaveRebellionLikely"),
            new SuppressibleAlertEntry("SlavesUnattended", "SlaveUnattendedLabel"),
            new SuppressibleAlertEntry("SlavesUnsuppressed", "SlavesUnsuppressedLabel"),
            new SuppressibleAlertEntry("StarvationAnimals", "StarvationAnimals"),
            new SuppressibleAlertEntry("StarvationColonists", "Starvation"),
            new SuppressibleAlertEntry("SubjectHasNowOverseer", "AlertMechLacksOverseer"),
            new SuppressibleAlertEntry("TatteredApparel", "AlertTatteredApparel"),
            new SuppressibleAlertEntry("ThroneroomInvalidConfiguration", "ThroneroomInvalidConfiguration"),
            new SuppressibleAlertEntry("TimedMakeFactionHostile", "FactionWillBecomeHostileIfNotLeavingWithin"),
            new SuppressibleAlertEntry("TimedRaidsArriving", "AlertTimedRaidsArrivingIn"),
            new SuppressibleAlertEntry("TitleRequiresBedroom", "NeedBedroomAssigned"),
            new SuppressibleAlertEntry("ToxicBuildup", "AlertToxicBuildup"),
            new SuppressibleAlertEntry("ToxifierGeneratorStopped", "AlertToxifierGeneratorStopped"),
            new SuppressibleAlertEntry("UndercaveUnstable", "Alert_UndercaveUnstable"),
            new SuppressibleAlertEntry("UndignifiedBedroom", "UndignifiedBedroom"),
            new SuppressibleAlertEntry("UndignifiedThroneroom", "UndignifiedThroneroom"),
            new SuppressibleAlertEntry("UnhappyNudity", "AlertUnhappyNudity"),
            new SuppressibleAlertEntry("UnusableMeditationFocus", "UnusableMeditationFocusAlert"),
            new SuppressibleAlertEntry("WarqueenHasLowResources", "AlertWarqueenHasLowResources"),
        };
    }
}
