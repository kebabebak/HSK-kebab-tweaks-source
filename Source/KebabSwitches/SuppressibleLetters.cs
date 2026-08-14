using System;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// One suppressible right-side Letter (LetterStack.ReceiveLetter envelope button label).
    ///
    /// Одно отключаемое Letter справа (label кнопки-конверта LetterStack.ReceiveLetter).
    /// </summary>
    public sealed class SuppressibleLetterEntry
    {
        public string Id { get; }
        public string TranslationKey { get; }
        public bool IsSurgeryFailed { get; }

        public SuppressibleLetterEntry(string id, string translationKey, bool isSurgeryFailed = false)
        {
            Id = id;
            TranslationKey = translationKey;
            IsSurgeryFailed = isSurgeryFailed;
        }

        public string QuoteKey =>
            IsSurgeryFailed ? "KebabSwitches.SurgeryFailedLetterQuote" : "KebabSwitches.LetterQuote." + Id;

        public string CheckboxLabel
        {
            get
            {
                string quote = LetterLabelPreview.SanitizeQuote(QuoteKey.Translate());
                return "KebabSwitches.IgnoreLetter".Translate(quote.Named("QUOTE"));
            }
        }

        /// <summary>
        /// RimWorld tooltip: vanilla keyed label template (English) or surgery-failure pattern.
        ///
        /// RimWorld-тултип: vanilla keyed label (EN) или шаблон провала операции.
        /// </summary>
        public string SourceTooltip => NotificationSourceTooltip.ForLetter(this);

        public bool IsSuppressEnabled => KebabTweaksSettings.IsLetterSuppressed(Id);

        public void SetSuppressEnabled(bool value) => KebabTweaksSettings.SetLetterSuppressed(Id, value);
    }

    /// <summary>
    /// Catalog of vanilla Core + DLC Letter labels (Letters.xml, Incidents.xml, Messages.xml,
    /// Anomaly/Royalty/Ideology/Biotech) plus HSK mod letter labels; surgery-failure regex.
    ///
    /// Каталог label писем vanilla Core + DLC (Letters.xml, Incidents.xml, Messages.xml,
    /// Anomaly/Royalty/Ideology/Biotech) и модов HSK; regex провала хирургии.
    /// </summary>
    public static class SuppressibleLetters
    {
        public static readonly SuppressibleLetterEntry[] All =
        {
            new SuppressibleLetterEntry("SurgeryFailed", translationKey: null, isSurgeryFailed: true),

            new SuppressibleLetterEntry("BladelinkWeaponBondedLabel", "LetterBladelinkWeaponBondedLabel"),
            new SuppressibleLetterEntry("CraftedLegendaryLabel", "LetterCraftedLegendaryLabel"),
            new SuppressibleLetterEntry("CraftedMasterworkLabel", "LetterCraftedMasterworkLabel"),
            new SuppressibleLetterEntry("FriendlyTrapSprungLabel", "LetterFriendlyTrapSprungLabel"),
            new SuppressibleLetterEntry("HealthComplicationsLabel", "LetterHealthComplicationsLabel"),
            new SuppressibleLetterEntry("HediffFromRandomHediffGiverLabel", "LetterHediffFromRandomHediffGiverLabel"),
            new SuppressibleLetterEntry("JoinOfferLabel", "LetterJoinOfferLabel"),
            new SuppressibleLetterEntry("AcceptedProposal", "LetterLabelAcceptedProposal"),
            new SuppressibleLetterEntry("Affair", "LetterLabelAffair"),
            new SuppressibleLetterEntry("AgentRevealed", "LetterLabelAgentRevealed"),
            new SuppressibleLetterEntry("AICoreOffer", "LetterLabelAICoreOffer"),
            new SuppressibleLetterEntry("AllCaravanColonistsDied", "LetterLabelAllCaravanColonistsDied"),
            new SuppressibleLetterEntry("AmbushInExistingMap", "LetterLabelAmbushInExistingMap"),
            new SuppressibleLetterEntry("AncientShrineWarning", "LetterLabelAncientShrineWarning"),
            new SuppressibleLetterEntry("AnimalInsanityMultiple", "LetterLabelAnimalInsanityMultiple"),
            new SuppressibleLetterEntry("AnimalInsanitySingle", "LetterLabelAnimalInsanitySingle"),
            new SuppressibleLetterEntry("AnimalManhunterRevenge", "LetterLabelAnimalManhunterRevenge"),
            new SuppressibleLetterEntry("AnimalSelfTame", "LetterLabelAnimalSelfTame"),
            new SuppressibleLetterEntry("AreaRevealed", "LetterLabelAreaRevealed"),
            new SuppressibleLetterEntry("BeaversArrived", "LetterLabelBeaversArrived"),
            new SuppressibleLetterEntry("Birthday", "LetterLabelBirthday"),
            new SuppressibleLetterEntry("Breakup", "LetterLabelBreakup"),
            new SuppressibleLetterEntry("CaravanEnteredEnemyBase", "LetterLabelCaravanEnteredEnemyBase"),
            new SuppressibleLetterEntry("CaravanEnteredMap", "LetterLabelCaravanEnteredMap"),
            new SuppressibleLetterEntry("CaravanRequest", "LetterLabelCaravanRequest"),
            new SuppressibleLetterEntry("CaravansBattlefieldVictory", "LetterLabelCaravansBattlefieldVictory"),
            new SuppressibleLetterEntry("CargoPodCrash", "LetterLabelCargoPodCrash"),
            new SuppressibleLetterEntry("CropBlight", "LetterLabelCropBlight"),
            new SuppressibleLetterEntry("DeepScannerFoundLump", "LetterLabelDeepScannerFoundLump"),
            new SuppressibleLetterEntry("DefeatAllEnemiesQuestCompleted", "LetterLabelDefeatAllEnemiesQuestCompleted"),
            new SuppressibleLetterEntry("DrugBinge", "LetterLabelDrugBinge"),
            new SuppressibleLetterEntry("FactionBaseDefeated", "LetterLabelFactionBaseDefeated"),
            new SuppressibleLetterEntry("FactionBaseProximity", "LetterLabelFactionBaseProximity"),
            new SuppressibleLetterEntry("FarmAnimalsWanderIn", "LetterLabelFarmAnimalsWanderIn"),
            new SuppressibleLetterEntry("FirstSummerWarning", "LetterLabelFirstSummerWarning"),
            new SuppressibleLetterEntry("FoundPreciousLump", "LetterLabelFoundPreciousLump"),
            new SuppressibleLetterEntry("GroupVisitorsArrive", "LetterLabelGroupVisitorsArrive"),
            new SuppressibleLetterEntry("HibernateComplete", "LetterLabelHibernateComplete"),
            new SuppressibleLetterEntry("ManhunterPackArrived", "LetterLabelManhunterPackArrived"),
            new SuppressibleLetterEntry("MechClusterArrived", "LetterLabelMechClusterArrived"),
            new SuppressibleLetterEntry("RecruitSuccess", "LetterLabelMessageRecruitSuccess"),
            new SuppressibleLetterEntry("MiracleHeal", "LetterLabelMiracleHeal"),
            new SuppressibleLetterEntry("NewDisease", "LetterLabelNewDisease"),
            new SuppressibleLetterEntry("NewLovers", "LetterLabelNewLovers"),
            new SuppressibleLetterEntry("NewlyAddicted", "LetterLabelNewlyAddicted"),
            new SuppressibleLetterEntry("NoticedRelatedPawns", "LetterLabelNoticedRelatedPawns"),
            new SuppressibleLetterEntry("PawnLeaving", "LetterLabelPawnLeaving"),
            new SuppressibleLetterEntry("PawnsArrive", "LetterLabelPawnsArrive"),
            new SuppressibleLetterEntry("PawnsArriveAndJoin", "LetterLabelPawnsArriveAndJoin"),
            new SuppressibleLetterEntry("PawnsKidnapped", "LetterLabelPawnsKidnapped"),
            new SuppressibleLetterEntry("PawnsLeaving", "LetterLabelPawnsLeaving"),
            new SuppressibleLetterEntry("PawnsLostBecauseMapClosed_Caravan", "LetterLabelPawnsLostBecauseMapClosed_Caravan"),
            new SuppressibleLetterEntry("PawnsLostBecauseMapClosed_Home", "LetterLabelPawnsLostBecauseMapClosed_Home"),
            new SuppressibleLetterEntry("PeaceTalks_Backfire", "LetterLabelPeaceTalks_Backfire"),
            new SuppressibleLetterEntry("PeaceTalks_Disaster", "LetterLabelPeaceTalks_Disaster"),
            new SuppressibleLetterEntry("PeaceTalks_Success", "LetterLabelPeaceTalks_Success"),
            new SuppressibleLetterEntry("PeaceTalks_TalksFlounder", "LetterLabelPeaceTalks_TalksFlounder"),
            new SuppressibleLetterEntry("PeaceTalks_Triumph", "LetterLabelPeaceTalks_Triumph"),
            new SuppressibleLetterEntry("PredatorHuntingColonist", "LetterLabelPredatorHuntingColonist"),
            new SuppressibleLetterEntry("PrisonBreak", "LetterLabelPrisonBreak"),
            new SuppressibleLetterEntry("PsychicDroneLevelIncreased", "LetterLabelPsychicDroneLevelIncreased"),
            new SuppressibleLetterEntry("QuestAskerCaptured", "LetterLabelQuestAskerCaptured"),
            new SuppressibleLetterEntry("QuestAskerDied", "LetterLabelQuestAskerDied"),
            new SuppressibleLetterEntry("QuestAskerFactionHostile", "LetterLabelQuestAskerFactionHostile"),
            new SuppressibleLetterEntry("QuestDropPodsArrived", "LetterLabelQuestDropPodsArrived"),
            new SuppressibleLetterEntry("QuestItemsAddedToCaravanInventory", "LetterLabelQuestItemsAddedToCaravanInventory"),
            new SuppressibleLetterEntry("RefugeeJoins", "LetterLabelRefugeeJoins"),
            new SuppressibleLetterEntry("RefugeePodCrash", "LetterLabelRefugeePodCrash"),
            new SuppressibleLetterEntry("RejectedProposal", "LetterLabelRejectedProposal"),
            new SuppressibleLetterEntry("RelationsChange_Ally", "LetterLabelRelationsChange_Ally"),
            new SuppressibleLetterEntry("RelationsChange_Hostile", "LetterLabelRelationsChange_Hostile"),
            new SuppressibleLetterEntry("RelationsChange_NeutralFromAlly", "LetterLabelRelationsChange_NeutralFromAlly"),
            new SuppressibleLetterEntry("RelationsChange_NeutralFromHostile", "LetterLabelRelationsChange_NeutralFromHostile"),
            new SuppressibleLetterEntry("RescueeJoins", "LetterLabelRescueeJoins"),
            new SuppressibleLetterEntry("RescueQuestFinished", "LetterLabelRescueQuestFinished"),
            new SuppressibleLetterEntry("RoofCollapsed", "LetterLabelRoofCollapsed"),
            new SuppressibleLetterEntry("ShortCircuit", "LetterLabelShortCircuit"),
            new SuppressibleLetterEntry("SingleVisitorArrives", "LetterLabelSingleVisitorArrives"),
            new SuppressibleLetterEntry("SiteCountdownStarted", "LetterLabelSiteCountdownStarted"),
            new SuppressibleLetterEntry("SiteNoLongerHostile", "LetterLabelSiteNoLongerHostile"),
            new SuppressibleLetterEntry("SiteNoLongerHostileMulti", "LetterLabelSiteNoLongerHostileMulti"),
            new SuppressibleLetterEntry("SpeechCancelled", "LetterLabelSpeechCancelled"),
            new SuppressibleLetterEntry("SuffixBondedAnimalDied", "LetterLabelSuffixBondedAnimalDied"),
            new SuppressibleLetterEntry("ThrumboPasses", "LetterLabelThrumboPasses"),
            new SuppressibleLetterEntry("TraderCaravanArrival", "LetterLabelTraderCaravanArrival"),
            new SuppressibleLetterEntry("TraitDisease", "LetterLabelTraitDisease"),
            new SuppressibleLetterEntry("TransportPodsLandedInEnemyBase", "LetterLabelTransportPodsLandedInEnemyBase"),
            new SuppressibleLetterEntry("UnrecruitablePawnCaptured", "LetterLabelUnrecruitablePawnCaptured"),
            new SuppressibleLetterEntry("VisitorsGaveGift", "LetterLabelVisitorsGaveGift"),
            new SuppressibleLetterEntry("WandererJoins", "LetterLabelWandererJoins"),
            new SuppressibleLetterEntry("LeaderChangedLabel", "LetterLeaderChangedLabel"),
            new SuppressibleLetterEntry("LeadersDeathLabel", "LetterLeadersDeathLabel"),
            new SuppressibleLetterEntry("QuestCompletedLabel", "LetterQuestCompletedLabel"),
            new SuppressibleLetterEntry("QuestConcludedLabel", "LetterQuestConcludedLabel"),
            new SuppressibleLetterEntry("QuestFailedLabel", "LetterQuestFailedLabel"),
            new SuppressibleLetterEntry("TechprintAppliedLabel", "LetterTechprintAppliedLabel"),
            new SuppressibleLetterEntry("ColonistPregnancyLaborLabel", "LetterColonistPregnancyLaborLabel"),
            new SuppressibleLetterEntry("Adopted", "LetterLabelAdopted"),
            new SuppressibleLetterEntry("ArchonexusStructureResearched", "LetterLabelArchonexusStructureResearched"),
            new SuppressibleLetterEntry("ArchonexusWealthReached", "LetterLabelArchonexusWealthReached"),
            new SuppressibleLetterEntry("ArchotechStructuresAbandoned", "LetterLabelArchotechStructuresAbandoned"),
            new SuppressibleLetterEntry("BecameAdult", "LetterLabelBecameAdult"),
            new SuppressibleLetterEntry("BecameChild", "LetterLabelBecameChild"),
            new SuppressibleLetterEntry("BestowingCeremonyExpired", "LetterLabelBestowingCeremonyExpired"),
            new SuppressibleLetterEntry("BestowingCeremonyTitleUpdated", "LetterLabelBestowingCeremonyTitleUpdated"),
            new SuppressibleLetterEntry("BossgroupArrived", "LetterLabelBossgroupArrived"),
            new SuppressibleLetterEntry("BossgroupCallerUnlocked", "LetterLabelBossgroupCallerUnlocked"),
            new SuppressibleLetterEntry("BossgroupSummoned", "LetterLabelBossgroupSummoned"),
            new SuppressibleLetterEntry("CommsConsoleSpawned", "LetterLabelCommsConsoleSpawned"),
            new SuppressibleLetterEntry("ConnectedTreeDestroyed", "LetterLabelConnectedTreeDestroyed"),
            new SuppressibleLetterEntry("ConvertIdeoAttempt_Success", "LetterLabelConvertIdeoAttempt_Success"),
            new SuppressibleLetterEntry("EnslavementSuccess", "LetterLabelEnslavementSuccess"),
            new SuppressibleLetterEntry("EntityDiscovery", "LetterLabelEntityDiscovery"),
            new SuppressibleLetterEntry("GainedRoyalTitle", "LetterLabelGainedRoyalTitle"),
            new SuppressibleLetterEntry("GenesImplanted", "LetterLabelGenesImplanted"),
            new SuppressibleLetterEntry("GrandSlaveEscape", "LetterLabelGrandSlaveEscape"),
            new SuppressibleLetterEntry("GrandSlaveRebellion", "LetterLabelGrandSlaveRebellion"),
            new SuppressibleLetterEntry("InvoluntaryDeathrest", "LetterLabelInvoluntaryDeathrest"),
            new SuppressibleLetterEntry("LastHackerLost", "LetterLabelLastHackerLost"),
            new SuppressibleLetterEntry("LocalSlaveEscape", "LetterLabelLocalSlaveEscape"),
            new SuppressibleLetterEntry("LocalSlaveRebellion", "LetterLabelLocalSlaveRebellion"),
            new SuppressibleLetterEntry("LostRoyalTitle", "LetterLabelLostRoyalTitle"),
            new SuppressibleLetterEntry("MechanitorCasketFound", "LetterLabelMechanitorCasketFound"),
            new SuppressibleLetterEntry("MechanitorCasketOpened", "LetterLabelMechanitorCasketOpened"),
            new SuppressibleLetterEntry("MechlinkAvailable", "LetterLabelMechlinkAvailable"),
            new SuppressibleLetterEntry("MechlinkInstalled", "LetterLabelMechlinkInstalled"),
            new SuppressibleLetterEntry("MechsFeral", "LetterLabelMechsFeral"),
            new SuppressibleLetterEntry("NewPrimaryIdeo", "LetterLabelNewPrimaryIdeo"),
            new SuppressibleLetterEntry("PawnConnected", "LetterLabelPawnConnected"),
            new SuppressibleLetterEntry("PsylinkLevelGained", "LetterLabelPsylinkLevelGained"),
            new SuppressibleLetterEntry("RandomDecree", "LetterLabelRandomDecree"),
            new SuppressibleLetterEntry("ReformIdeo", "LetterLabelReformIdeo"),
            new SuppressibleLetterEntry("RegenerationComa", "LetterLabelRegenerationComa"),
            new SuppressibleLetterEntry("RelicDestroyed", "LetterLabelRelicDestroyed"),
            new SuppressibleLetterEntry("RelicFound", "LetterLabelRelicFound"),
            new SuppressibleLetterEntry("RelicLost", "LetterLabelRelicLost"),
            new SuppressibleLetterEntry("RelicsCollected", "LetterLabelRelicsCollected"),
            new SuppressibleLetterEntry("RewardsForNewTitle", "LetterLabelRewardsForNewTitle"),
            new SuppressibleLetterEntry("RoleActive", "LetterLabelRoleActive"),
            new SuppressibleLetterEntry("RoleActiveDesc", "LetterLabelRoleActiveDesc"),
            new SuppressibleLetterEntry("RoleInactive", "LetterLabelRoleInactive"),
            new SuppressibleLetterEntry("RoleInactiveDesc", "LetterLabelRoleInactiveDesc"),
            new SuppressibleLetterEntry("RoleLost", "LetterLabelRoleLost"),
            new SuppressibleLetterEntry("SanguophageWaitingToReimplant", "LetterLabelSanguophageWaitingToReimplant"),
            new SuppressibleLetterEntry("ShamblerAnimalsArrived", "LetterLabelShamblerAnimalsArrived"),
            new SuppressibleLetterEntry("ShamblerArrived", "LetterLabelShamblerArrived"),
            new SuppressibleLetterEntry("ShamblerSwarmArrived", "LetterLabelShamblerSwarmArrived"),
            new SuppressibleLetterEntry("ShuttleCrashed", "LetterLabelShuttleCrashed"),
            new SuppressibleLetterEntry("SingleSlaveEscape", "LetterLabelSingleSlaveEscape"),
            new SuppressibleLetterEntry("SingleSlaveRebellion", "LetterLabelSingleSlaveRebellion"),
            new SuppressibleLetterEntry("SpacedroneIncoming", "LetterLabelSpacedroneIncoming"),
            new SuppressibleLetterEntry("SurpriseReinforcements", "LetterLabelSurpriseReinforcements"),
            new SuppressibleLetterEntry("ThirdTrimester", "LetterLabelThirdTrimester"),
            new SuppressibleLetterEntry("TributeCollectorArrival", "LetterLabelTributeCollectorArrival"),
            new SuppressibleLetterEntry("WandererJoinsAbasia", "LetterLabelWandererJoinsAbasia"),
            new SuppressibleLetterEntry("XenogermOrderedImplanted", "LetterLabelXenogermOrderedImplanted"),
            new SuppressibleLetterEntry("LawViolationDetectedLabel", "LetterLawViolationDetectedLabel"),
            new SuppressibleLetterEntry("PsychicBondCreatedLovinLabel", "LetterPsychicBondCreatedLovinLabel"),
            new SuppressibleLetterEntry("TitleHeirLostLabel", "LetterTitleHeirLostLabel"),
            new SuppressibleLetterEntry("ChimerasAttackingLabel", "LetterChimerasAttackingLabel"),
            new SuppressibleLetterEntry("GrayFleshDiscoveredLabel", "LetterGrayFleshDiscoveredLabel"),
            new SuppressibleLetterEntry("CorpseDisappeared", "LetterLabelCorpseDisappeared"),
            new SuppressibleLetterEntry("CorpseReappeared", "LetterLabelCorpseReappeared"),
            new SuppressibleLetterEntry("DeathPallEnded", "LetterLabelDeathPallEnded"),
            new SuppressibleLetterEntry("DreadmeldWarning", "LetterLabelDreadmeldWarning"),
            new SuppressibleLetterEntry("EscapingFromHoldingPlatform", "LetterLabelEscapingFromHoldingPlatform"),
            new SuppressibleLetterEntry("FleshmassHeartDestroyed", "LetterLabelFleshmassHeartDestroyed"),
            new SuppressibleLetterEntry("FleshTentacleAttack", "LetterLabelFleshTentacleAttack"),
            new SuppressibleLetterEntry("FloorEtchingRamblings", "LetterLabelFloorEtchingRamblings"),
            new SuppressibleLetterEntry("FloorEtchings", "LetterLabelFloorEtchings"),
            new SuppressibleLetterEntry("GateClosing", "LetterLabelGateClosing"),
            new SuppressibleLetterEntry("GateEntered", "LetterLabelGateEntered"),
            new SuppressibleLetterEntry("GoldenCubeComa", "LetterLabelGoldenCubeComa"),
            new SuppressibleLetterEntry("GrayPallDescending", "LetterLabelGrayPallDescending"),
            new SuppressibleLetterEntry("LabyrinthExit", "LetterLabelLabyrinthExit"),
            new SuppressibleLetterEntry("LinkingRitualCompleted", "LetterLabelLinkingRitualCompleted"),
            new SuppressibleLetterEntry("ObeliskDiscovered", "LetterLabelObeliskDiscovered"),
            new SuppressibleLetterEntry("PawnHypnotized", "LetterLabelPawnHypnotized"),
            new SuppressibleLetterEntry("RevenantEmergence", "LetterLabelRevenantEmergence"),
            new SuppressibleLetterEntry("RevenantKilled", "LetterLabelRevenantKilled"),
            new SuppressibleLetterEntry("RevenantRevealed", "LetterLabelRevenantRevealed"),
            new SuppressibleLetterEntry("RevenantSmearDiscovered", "LetterLabelRevenantSmearDiscovered"),
            new SuppressibleLetterEntry("SightstealerHowl", "LetterLabelSightstealerHowl"),
            new SuppressibleLetterEntry("SightstealerHowlBig", "LetterLabelSightstealerHowlBig"),
            new SuppressibleLetterEntry("SightstealerRevealed", "LetterLabelSightstealerRevealed"),
            new SuppressibleLetterEntry("UndercaveCollapsing", "LetterLabelUndercaveCollapsing"),
            new SuppressibleLetterEntry("MetalhorrorReawakeningLabel", "LetterMetalhorrorReawakeningLabel"),
            new SuppressibleLetterEntry("RevenantFleshChunkLabel", "LetterRevenantFleshChunkLabel"),
            new SuppressibleLetterEntry("RevenantSeenLabel", "LetterRevenantSeenLabel"),
            new SuppressibleLetterEntry("SurgicallyInspectedLabel", "LetterSurgicallyInspectedLabel"),
            new SuppressibleLetterEntry("UnnaturalHealingLabel", "LetterUnnaturalHealingLabel"),
#if RIMWORLD_1_6
            new SuppressibleLetterEntry("AncientGravEngineDiscoveredLabel", "LetterAncientGravEngineDiscoveredLabel"),
            new SuppressibleLetterEntry("OdysseyTurretDisabled", "LetterLabelOdysseyTurretDisabled"),
            new SuppressibleLetterEntry("OdysseyBlastDoorOpened", "LetterLabelOdysseyBlastDoorOpened"),
            new SuppressibleLetterEntry("SkipAbduction", "LetterLabelSkipAbduction"),
            new SuppressibleLetterEntry("FleshbeastsEmerging", "LetterLabelFleshbeastsEmerging"),
#endif
        };
    }
}
