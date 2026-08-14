using System.Collections.Generic;

namespace HSK.KebabTweaks.KebabSwitches
{
    internal static class AlertEnglishCatalog
    {
        private static readonly Dictionary<string, string> ByKey = Build();

        public static string TryGet(string translationKey)
        {
            if (translationKey == null) return null;
            return ByKey.TryGetValue(translationKey, out string text) ? text : null;
        }

        private static Dictionary<string, string> Build()
        {
            var map = new Dictionary<string, string>();
            map["ActivatorCountdown"] = "Countdown activator {0}";
            map["ActivityMultipleDangerous"] = "Dangerous activity levels";
            map["Alert_CultistPsychicRitual"] = "Cultist psychic ritual";
            map["Alert_InsufficientContainment"] = "Insufficient containment";
            map["Alert_UndercaveUnstable"] = "Undercave unstable";
            map["AlertAbandonedBaby"] = "Abandoned baby";
            map["AlertAgeReversalDemandNear"] = "Age reversal demanded";
            map["AlertAnimalFilth"] = "Animal filth";
            map["AlertAnimalIsRoaming"] = "Animal wandering away";
            map["AlertAnimalPenNeeded"] = "Pen needed";
            map["AlertAnimalPenNotEnclosed"] = "Pen not enclosed";
            map["AlertCasketOpening"] = "Cryptosleep caskets opening in {0}";
            map["AlertColonistLeftUnburied"] = "Colonist left unburied";
            map["AlertConnectedPawnNotAssignedToPlantCuttingLabel"] = "Connector not assigned to plant cutting";
            map["AlertCubeWithdrawal"] = "Cube withdrawal";
            map["AlertDeathrestComplete"] = "Deathrest complete";
            map["AlertDigestion"] = "Devourer digestion";
            map["AlertEmergingPitGate"] = "Pit gate opening";
            map["AlertFuelNodeIgniting"] = "Fuel node explodes in {0}";
            map["AlertGauranlenTreeWithoutDryadTypeLabel"] = "Dryad caste not set";
            map["AlertGenebankUnpowered"] = "Gene bank needs power";
            map["AlertGhoulHypothermia"] = "Ghoul hypothermia";
            map["AlertGrayFleshSample"] = "Gray flesh";
            map["AlertHeatstroke"] = "Heatstroke";
            map["AlertHitchedAnimalHungryNoFood"] = "Hungry hitched animal";
            map["AlertHoldingPlatform"] = "Holding platforms needed";
            map["AlertHostilesWakingUp"] = "{0_pawnsPlural} waking in {1}";
            map["AlertHypothermia"] = "Hypothermia";
            map["AlertInfestationArriving"] = "Insects arriving in {0}";
            map["AlertInhibitorBlocked"] = "Inhibitor blocked";
            map["AlertLowBabyFood"] = "Low baby food";
            map["AlertLowDeathrestPawns"] = "{NUMCULPRITS} colonists need deathrest";
            map["AlertLowHemogen"] = "Low hemogen";
            map["AlertMeatHunger"] = "Ghoul starvation";
            map["AlertMechLacksOverseer"] = "Uncontrolled mechs";
            map["AlertMechMissingBodyPart"] = "Mechanoid missing part";
            map["AlertMechNeedsRepair"] = "Mechanoid repair needed";
            map["AlertMinifiedTreeAboutToDie"] = "{0} trees dying";
            map["AlertNeedBabyCribs"] = "Need baby cribs";
            map["AlertNeedMechChargers"] = "Need mech rechargers";
            map["AlertNeedSlaveCribs"] = "Need slave cribs";
            map["AlertNeuralLump"] = "Neural lump";
            map["AlertNoBabyFeeder"] = "No baby feeders";
            map["AlertNoBabyFoodCaravan"] = "No baby food in caravan";
            map["AlertPennedAnimalHungry"] = "Hungry pen animals";
            map["AlertPollutedTerrain"] = "Polluted terrain";
            map["AlertPredatorInAnimalPen"] = "{ANIMAL_labelShort} in animal pen";
            map["AlertPsychicBondedPawnsSeparated"] = "Psychic bond distance";
            map["AlertRechargerFull"] = "Recharger full";
            map["AlertReimplantationAvailable"] = "Reimplantation available";
            map["AlertRevenantFlesh"] = "Revenant flesh";
            map["AlertRitualComing"] = "Ritual opportunity soon";
            map["AlertSlaveRebellionLikely"] = "Slave rebellion likely";
            map["AlertTatteredApparel"] = "Tattered apparel";
            map["AlertTimedRaidsArrivingIn"] = "Raids arriving in {0}";
            map["AlertToxicBuildup"] = "Toxic buildup";
            map["AlertToxifierGeneratorStopped"] = "{0} stopped";
            map["AlertUnhappyNudity"] = "Unhappy nudity";
            map["AlertWarqueenHasLowResources"] = "{0} is low on resources";
            map["AnimaLinkingReadyLabel"] = "Anima linking ceremony ready";
            map["BestowerWaitingAlert"] = "Bestower waiting";
            map["BilliardsNeedsSpace"] = "Billiards needs space";
            map["Biostarvation"] = "Biostarvation";
            map["Boredom"] = "Boredom";
            map["BrawlerHasRangedWeapon"] = "Brawler has ranged weapon";
            map["BreakRiskMajor"] = "Major break risk";
            map["BreakRiskMinor"] = "Minor break risk";
            map["BuildingCantBeUsedRoofed"] = "Building unusable due to roof";
            map["CaravanIdle"] = "Caravan idle";
            map["ChessTablesNeedChairs"] = "Chess table needs chairs";
            map["ColonistNeedsTreatment"] = "Medical treatment needed";
            map["ColonistsIdle"] = "{0} colonists idle";
            map["ColonistsNeedRescue"] = "Colonists need rescue";
            map["CreepJoinerTimeout"] = "Visitor ignored";
            map["DisallowedBuildingInsideMonument"] = "Monument will be destroyed";
            map["EntityNeedsTreatment"] = "Entity treatment needed";
            map["Exhaustion"] = "Exhaustion";
            map["FactionWillBecomeHostileIfNotLeavingWithin"] = "Faction will become hostile if you stay";
            map["FireInHomeArea"] = "Fire!";
            map["HunterHasShieldAndRangedWeapon"] = "Hunter has shield and ranged weapon";
            map["HunterLacksWeapon"] = "Hunter lacks suitable weapon";
            map["IdeoBuildingDisrespected"] = "{0} disrespected";
            map["IdeoBuildingMissing"] = "{0} desired";
            map["IdeoRolesEmpty"] = "{0} roles unfilled";
            map["ImmobileCaravan"] = "Immobile caravan";
            map["LowFood"] = "Low food";
            map["LowMedicine"] = "Low medicine";
            map["MonolithTwistingAlert"] = "Monolith twisting";
            map["MonumentMarkerMissingBlueprints"] = "Monument missing blueprints";
            map["NeedAnomalyProject"] = "Need anomaly project";
            map["NeedBatteries"] = "Need batteries";
            map["NeedBedroomAssigned"] = "Title requires bedroom";
            map["NeedColonistBeds"] = "Need colonist beds";
            map["NeedDefenses"] = "Need defenses";
            map["NeedDoctor"] = "Need doctor";
            map["NeedFoodHopper"] = "Need food hopper";
            map["NeedJoySource"] = "Need recreation variety";
            map["NeedMealSource"] = "Need meal source";
            map["NeedMeditationSpotAlert"] = "Need meditation spot";
            map["NeedMiner"] = "Need miner";
            map["NeedResearchBench"] = "Need research bench";
            map["NeedResearchProject"] = "Need research project";
            map["NeedSlaveBeds"] = "Need slave beds";
            map["NeedThroneAssigned"] = "Need throneroom";
            map["NeedWarden"] = "Need warden";
            map["NeedWarmClothes"] = "Need warm clothes";
            map["PatientsAwaitingMedicalOperation"] = "{0} patients await medical operation";
            map["PawnsWithLifeThreateningDisease"] = "Medical emergency";
            map["PermitChoiceReadyAlert"] = "Permit choice ready";
            map["PokerTablesNeedChairs"] = "Poker table needs chairs";
            map["QuestExpiresSoon"] = "Quest expires in {0}";
            map["RoyalNoAcceptableFood"] = "No acceptable food";
            map["ShieldUserHasRangedWeapon"] = "Shield user has ranged weapon";
            map["ShipLandingBeaconUnusable"] = "Beacon unusable";
            map["SlavesUnsuppressedLabel"] = "Slave unsuppressed";
            map["SlaveUnattendedLabel"] = "Slave unattended";
            map["Starvation"] = "Starvation";
            map["StarvationAnimals"] = "Animal starvation";
            map["ThroneroomInvalidConfiguration"] = "Throne not usable";
            map["UndignifiedBedroom"] = "Undignified bedroom";
            map["UndignifiedThroneroom"] = "Undignified throneroom";
            map["UnusableMeditationFocusAlert"] = "Unusable meditation focus";
            return map;
        }
    }
}
