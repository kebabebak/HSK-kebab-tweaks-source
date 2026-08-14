using System.Collections.Generic;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Fallback English keyed templates when the English LoadedLanguage is unavailable at tooltip time.
    ///
    /// Запасные английские keyed-шаблоны, если LoadedLanguage English недоступен при показе тултипа.
    /// </summary>
    internal static class NotificationEnglishCatalog
    {
        private static readonly Dictionary<string, string> ByKey = Build();

        public static string TryGet(string translationKey)
        {
            if (translationKey == null)
            {
                return null;
            }

            return ByKey.TryGetValue(translationKey, out string text) ? text : null;
        }

        private static Dictionary<string, string> Build()
        {
            var map = new Dictionary<string, string>();

            AddScreenMessages(map);
            AddLetterLabels(map);
            return map;
        }

        private static void AddScreenMessages(Dictionary<string, string> map)
        {
            map["MessageGaveBirth"] = "{0} has given birth.";
            map["MessageAnimalIsPregnant"] = "{PAWN_nameDef} is pregnant!";
            map["MessageMiscarriedStarvation"] = "{0} has miscarried due to starvation.";
            map["MessageMiscarriedPoorHealth"] = "{0} has miscarried due to poor health.";
            map["MessageSeasonBegun"] = "{0} has begun.";
            map["MessageBillComplete"] = "Bill complete: {0}.";
            map["MessageFullyHealed"] = "{1_label} is fully healed.";
            map["MessageSocialFight"] = "{PAWN1_labelShort} started a social fight with {PAWN2_labelShort}.";
            map["MessageNewBondRelation"] = "{HUMAN_labelShort} and {ANIMAL_labelShort} have formed a bond.";
            map["MessageNewBondRelationNewName"] =
                "{HUMAN_labelShort} and {1} have formed a bond. {HUMAN_labelShort} has named {ANIMAL_objective} {ANIMAL_nameFull}.";
            map["MessageFoodPoisoning"] =
                "{PAWN_labelShort} has gotten food poisoning from: {FOOD_labelShort}. Cause: {2}.";
            map["MessageRoamerLeaving"] =
                "{PAWN_labelShort} has started to roam away! {PAWN_pronoun} will leave the map unless an animal handler ropes {PAWN_objective} back to a pen.";
            map["MessageHiveReproduced"] = "A bug hive has reproduced itself.";
            map["MessageTraderCaravanLeaving"] = "The trade caravan from {0} is leaving.";
            map["MessageTraderCaravanDismissed"] = "The trade caravan from {0} has been dismissed.";
            map["MessageCompSpawnerSpawnedItem"] = "Item produced: {0}.";
            map["MessagePlantDiedOfCold"] = "{0} has died because of cold.";
            map["MessagePlantDiedOfRot_LeftUnharvested"] =
                "{0} has died from rotting due to being left unharvested.";
            map["MessagePlantDiedOfRot_ExposedToLight"] = "{0} has died due to being exposed to light.";
            map["MessagePlantDiedOfRot"] = "{0} has died from rotting.";
            map["MessagePlantDiedOfPoison"] = "{0} has died because of poison.";
            map["MessagePlantDiedOfBlight"] = "{1_label} has died because of blight.";
            map["MessagePlantDiedOfPollution"] = "{0} has died because of pollution.";
            map["MessagePlantDiedOfNoPollution"] = "{0} has died because of a lack of pollution.";
            map["MessagePlantDiedOfRot_PollutedTerrain"] = "{0} has rotted due to polluted terrain.";
            map["MessageMinifiedTreeDied"] = "An extracted tree just died.";
            map["MessageRottedAwayInStorage"] = "{1_label} has rotted away in storage.";
            map["MessageDeterioratedAway"] = "{0} has deteriorated away in storage.";
            map["MessageWornApparelDeterioratedAway"] =
                "{0} worn by {1_nameFull} deteriorated away to nothing.";
        }

        private static void AddLetterLabels(Dictionary<string, string> map)
        {
            map["LetterBladelinkWeaponBondedLabel"] = "Persona bond: {WEAPON_labelShort}";
            map["LetterCraftedLegendaryLabel"] = "Legendary Work";
            map["LetterCraftedMasterworkLabel"] = "Masterwork";
            map["LetterFriendlyTrapSprungLabel"] = "{1_labelShort} hit trap";
            map["LetterHealthComplicationsLabel"] = "{PAWN_labelShort}: {1}";
            map["LetterHediffFromRandomHediffGiverLabel"] = "{PAWN_labelShort}: {1}";
            map["LetterJoinOfferLabel"] = "Join offer: {PAWN_nameDef}";
            map["LetterLabelAcceptedProposal"] = "Marriage is on!";
            map["LetterLabelAffair"] = "Affair";
            map["LetterLabelAgentRevealed"] = "Agent";
            map["LetterLabelAICoreOffer"] = "Persona core offer";
            map["LetterLabelAllCaravanColonistsDied"] = "Caravan destroyed";
            map["LetterLabelAmbushInExistingMap"] = "Ambush";
            map["LetterLabelAncientShrineWarning"] = "Ancient danger";
            map["LetterLabelAnimalInsanityMultiple"] = "Mad animals";
            map["LetterLabelAnimalInsanitySingle"] = "Mad animal";
            map["LetterLabelAnimalManhunterRevenge"] = "{0} revenge";
            map["LetterLabelAnimalSelfTame"] = "Self-tamed animal";
            map["LetterLabelAreaRevealed"] = "Area revealed";
            map["LetterLabelBeaversArrived"] = "Beavers!";
            map["LetterLabelBirthday"] = "Birthday";
            map["LetterLabelBreakup"] = "Breakup";
            map["LetterLabelCaravanEnteredEnemyBase"] = "Attack begun";
            map["LetterLabelCaravanEnteredMap"] = "Caravan arrived at {0_label}";
            map["LetterLabelCaravanRequest"] = "Caravan request";
            map["LetterLabelCaravansBattlefieldVictory"] = "Caravan battle won";
            map["LetterLabelCargoPodCrash"] = "Cargo pods";
            map["LetterLabelCropBlight"] = "Blight: {0}";
            map["LetterLabelDeepScannerFoundLump"] = "Scanned underground";
            map["LetterLabelDefeatAllEnemiesQuestCompleted"] = "Quest completed";
            map["LetterLabelDrugBinge"] = "{0} binge";
            map["LetterLabelFactionBaseDefeated"] = "Base destroyed";
            map["LetterLabelFactionBaseProximity"] = "Faction base proximity";
            map["LetterLabelFarmAnimalsWanderIn"] = "Farm animals join";
            map["LetterLabelFirstSummerWarning"] = "Summer";
            map["LetterLabelFoundPreciousLump"] = "Distant resource scanned";
            map["LetterLabelGroupVisitorsArrive"] = "Visitors";
            map["LetterLabelHibernateComplete"] = "Reactor ready";
            map["LetterLabelManhunterPackArrived"] = "Manhunter pack";
            map["LetterLabelMechClusterArrived"] = "Mechanoids arrived";
            map["LetterLabelMessageRecruitSuccess"] = "New recruit";
            map["LetterLabelMiracleHeal"] = "Miracle";
            map["LetterLabelNewDisease"] = "Disease";
            map["LetterLabelNewLovers"] = "New lovers";
            map["LetterLabelNewlyAddicted"] = "{0} addiction";
            map["LetterLabelNoticedRelatedPawns"] = "Relationship";
            map["LetterLabelPawnLeaving"] = "{0} leaving";
            map["LetterLabelPawnsArrive"] = "People arrived";
            map["LetterLabelPawnsArriveAndJoin"] = "People join";
            map["LetterLabelPawnsKidnapped"] = "{0} kidnapped";
            map["LetterLabelPawnsLeaving"] = "{0} leaving";
            map["LetterLabelPawnsLostBecauseMapClosed_Caravan"] = "Caravan lost";
            map["LetterLabelPawnsLostBecauseMapClosed_Home"] = "Inhabitants abandoned";
            map["LetterLabelPeaceTalks_Backfire"] = "Peace talks backfire";
            map["LetterLabelPeaceTalks_Disaster"] = "Peace talks disaster";
            map["LetterLabelPeaceTalks_Success"] = "Peace talks success";
            map["LetterLabelPeaceTalks_TalksFlounder"] = "Peace talks flounder";
            map["LetterLabelPeaceTalks_Triumph"] = "Peace talks triumph";
            map["LetterLabelPredatorHuntingColonist"] = "{PREDATOR} hunting {PREY_definite}";
            map["LetterLabelPrisonBreak"] = "Prison break";
            map["LetterLabelPsychicDroneLevelIncreased"] = "Drone intensifies";
            map["LetterLabelQuestAskerCaptured"] = "Quest failed";
            map["LetterLabelQuestAskerDied"] = "Quest failed";
            map["LetterLabelQuestAskerFactionHostile"] = "Quest failed";
            map["LetterLabelQuestDropPodsArrived"] = "Pods arrived";
            map["LetterLabelQuestItemsAddedToCaravanInventory"] = "{0} received items";
            map["LetterLabelRefugeeJoins"] = "{PAWN_nameDef} joins";
            map["LetterLabelRefugeePodCrash"] = "Transport pod crash";
            map["LetterLabelRejectedProposal"] = "Rejected proposal";
            map["LetterLabelRelationsChange_Ally"] = "Allied: {0}";
            map["LetterLabelRelationsChange_Hostile"] = "Hostile: {0}";
            map["LetterLabelRelationsChange_NeutralFromAlly"] = "Ally now neutral: {0}";
            map["LetterLabelRelationsChange_NeutralFromHostile"] = "Enemy now neutral: {0}";
            map["LetterLabelRescueeJoins"] = "{PAWN_nameDef} joins";
            map["LetterLabelRescueQuestFinished"] = "Rescuee joins";
            map["LetterLabelRoofCollapsed"] = "Roof collapse";
            map["LetterLabelShortCircuit"] = "Zzztt...";
            map["LetterLabelSingleVisitorArrives"] = "Visitor";
            map["LetterLabelSiteCountdownStarted"] = "Caravan detected";
            map["LetterLabelSiteNoLongerHostile"] = "Quest no longer available";
            map["LetterLabelSiteNoLongerHostileMulti"] = "Quests no longer available";
            map["LetterLabelSpeechCancelled"] = "Speech cancelled";
            map["LetterLabelSuffixBondedAnimalDied"] = "bonded";
            map["LetterLabelThrumboPasses"] = "Rare thrumbos";
            map["LetterLabelTraderCaravanArrival"] = "{0} from {1}";
            map["LetterLabelTraitDisease"] = "Disease: {0}";
            map["LetterLabelTransportPodsLandedInEnemyBase"] = "Attack begun";
            map["LetterLabelUnrecruitablePawnCaptured"] = "Unwavering prisoner";
            map["LetterLabelVisitorsGaveGift"] = "Gift from {0}";
            map["LetterLabelWandererJoins"] = "Wanderer joins: {0}";
            map["LetterLeaderChangedLabel"] = "New {1}: {0}";
            map["LetterLeadersDeathLabel"] = "{1} died: {0}";
            map["LetterQuestCompletedLabel"] = "Quest completed";
            map["LetterQuestConcludedLabel"] = "Quest concluded";
            map["LetterQuestFailedLabel"] = "Quest failed";
            map["LetterTechprintAppliedLabel"] = "Techprint applied: {0}";
            map["LetterColonistPregnancyLaborLabel"] = "{0_labelShort} in labor!";
            map["LetterLabelAdopted"] = "{BABY_nameDef} adopted";
            map["LetterLabelArchonexusStructureResearched"] = "{0_labelShort} studied";
            map["LetterLabelArchonexusWealthReached"] = "Quest '{0}' available";
            map["LetterLabelArchotechStructuresAbandoned"] = "Archotech structures abandoned";
            map["LetterLabelBecameAdult"] = "{0_nameDef} became an adult";
            map["LetterLabelBecameChild"] = "{0_nameDef} became a child";
            map["LetterLabelBestowingCeremonyExpired"] = "Bestowing ceremony expired";
            map["LetterLabelBestowingCeremonyTitleUpdated"] = "{TARGET_definite}'s title changed";
            map["LetterLabelBossgroupArrived"] = "{0} arrived";
            map["LetterLabelBossgroupCallerUnlocked"] = "Summon {LEADER_label} possible";
            map["LetterLabelBossgroupSummoned"] = "{0} summoned";
            map["LetterLabelCommsConsoleSpawned"] = "Summon {LEADER_label} available";
            map["LetterLabelConnectedTreeDestroyed"] = "Connected {TREE_label} destroyed";
            map["LetterLabelConvertIdeoAttempt_Success"] = "Conversion";
            map["LetterLabelEnslavementSuccess"] = "Enslaved";
            map["LetterLabelEntityDiscovery"] = "New research available";
            map["LetterLabelGainedRoyalTitle"] = "{TITLE} title gained: {PAWN_labelShort}";
            map["LetterLabelGenesImplanted"] = "Genes reimplanted";
            map["LetterLabelGrandSlaveEscape"] = "Grand slave escape";
            map["LetterLabelGrandSlaveRebellion"] = "Grand slave rebellion";
            map["LetterLabelInvoluntaryDeathrest"] = "Involuntary deathrest";
            map["LetterLabelLastHackerLost"] = "Last hacker lost";
            map["LetterLabelLocalSlaveEscape"] = "Local slave escape";
            map["LetterLabelLocalSlaveRebellion"] = "Local slave rebellion";
            map["LetterLabelLostRoyalTitle"] = "{TITLE} title lost: {PAWN_labelShort}";
            map["LetterLabelMechanitorCasketFound"] = "Mechanitor casket found";
            map["LetterLabelMechanitorCasketOpened"] = "{PAWN_nameDef}'s mechlink available";
            map["LetterLabelMechlinkAvailable"] = "Mechlink available";
            map["LetterLabelMechlinkInstalled"] = "Mechlink installed";
            map["LetterLabelMechsFeral"] = "Mech(s) gone feral";
            map["LetterLabelNewPrimaryIdeo"] = "Primary ideoligion changed";
            map["LetterLabelPawnConnected"] = "{TREE_label} connection";
            map["LetterLabelPsylinkLevelGained"] = "Psylink gained";
            map["LetterLabelRandomDecree"] = "{0_nameIndef} issues decree";
            map["LetterLabelReformIdeo"] = "Reform ideoligion";
            map["LetterLabelRegenerationComa"] = "Regeneration coma";
            map["LetterLabelRelicDestroyed"] = "Relic destroyed";
            map["LetterLabelRelicFound"] = "Relic found";
            map["LetterLabelRelicLost"] = "Relic lost";
            map["LetterLabelRelicsCollected"] = "Relics collected";
            map["LetterLabelRewardsForNewTitle"] = "Title rewards";
            map["LetterLabelRoleActive"] = "{0} role activated";
            map["LetterLabelRoleActiveDesc"] = "The number of {0}s in your colony has reached {1}. You can now assign the role of {ROLE_labelDef} to one of your colonists.\\n\\nTo assign a role, use the person's Social tab, or select a ritual spot or altar and press the 'Begin role change' button.";
            map["LetterLabelRoleInactive"] = "{0} role deactivated";
            map["LetterLabelRoleInactiveDesc"] = "Number of {0}s in your colony fell to {1}. {ROLE_labelDef} role is no longer active. This role will become available once again when the number of {0}s reaches {2}.";
            map["LetterLabelRoleLost"] = "{PAWN_labelShort}'s {ROLE_labelIndef} role lost";
            map["LetterLabelSanguophageWaitingToReimplant"] = "Xenogerm implantation";
            map["LetterLabelShamblerAnimalsArrived"] = "{ANIMALKIND_label} shamblers";
            map["LetterLabelShamblerArrived"] = "Shambler approaches";
            map["LetterLabelShamblerSwarmArrived"] = "Shamblers approach";
            map["LetterLabelShuttleCrashed"] = "Shuttle crashed";
            map["LetterLabelSingleSlaveEscape"] = "Slave escape";
            map["LetterLabelSingleSlaveRebellion"] = "Slave rebellion";
            map["LetterLabelSpacedroneIncoming"] = "Spacedrone incoming";
            map["LetterLabelSurpriseReinforcements"] = "Surprise reinforcements!";
            map["LetterLabelThirdTrimester"] = "{0_nameDef}'s baby prep";
            map["LetterLabelTributeCollectorArrival"] = "Royal tribute collector";
            map["LetterLabelWandererJoinsAbasia"] = "Transport pod crash: {0_nameDef}";
            map["LetterLabelXenogermOrderedImplanted"] = "Xenogerm implantation ordered";
            map["LetterLawViolationDetectedLabel"] = "Lawbreaker: {PAWN_labelShort}";
            map["LetterPsychicBondCreatedLovinLabel"] = "Psychic bond";
            map["LetterTitleHeirLostLabel"] = "Heir lost";
            map["LetterChimerasAttackingLabel"] = "Chimeras attacking";
            map["LetterGrayFleshDiscoveredLabel"] = "Gray flesh";
            map["LetterLabelCorpseDisappeared"] = "Corpse disappearance";
            map["LetterLabelCorpseReappeared"] = "Corpse appearance";
            map["LetterLabelDeathPallEnded"] = "Death pall clearing";
            map["LetterLabelDreadmeldWarning"] = "Squirming sounds";
            map["LetterLabelEscapingFromHoldingPlatform"] = "Entity escape";
            map["LetterLabelFleshmassHeartDestroyed"] = "Fleshmass heart defeated";
            map["LetterLabelFleshTentacleAttack"] = "Flesh tentacle attack";
            map["LetterLabelFloorEtchingRamblings"] = "Floor etchings";
            map["LetterLabelFloorEtchings"] = "Floor etchings";
            map["LetterLabelGateClosing"] = "Pit gate closing";
            map["LetterLabelGateEntered"] = "Fleshbeast lair";
            map["LetterLabelGoldenCubeComa"] = "Cube coma: {PAWN_labelShort}";
            map["LetterLabelGrayPallDescending"] = "Gray pall";
            map["LetterLabelLabyrinthExit"] = "Colonists returned";
            map["LetterLabelLinkingRitualCompleted"] = "Linking ritual completed";
            map["LetterLabelObeliskDiscovered"] = "Warped obelisk";
            map["LetterLabelPawnHypnotized"] = "{PAWN_nameDef} hypnotized";
            map["LetterLabelRevenantEmergence"] = "Revenant emergence";
            map["LetterLabelRevenantKilled"] = "Revenant killed";
            map["LetterLabelRevenantRevealed"] = "Revenant attack";
            map["LetterLabelRevenantSmearDiscovered"] = "Revenant smear discovered";
            map["LetterLabelSightstealerHowl"] = "Sightstealer shrieks";
            map["LetterLabelSightstealerHowlBig"] = "Sightstealer howling";
            map["LetterLabelSightstealerRevealed"] = "Sightstealer revealed";
            map["LetterLabelUndercaveCollapsing"] = "Undercave unstable";
            map["LetterMetalhorrorReawakeningLabel"] = "Metalhorrors awakening";
            map["LetterRevenantFleshChunkLabel"] = "Revenant flesh chunk";
            map["LetterRevenantSeenLabel"] = "Revenant seen";
            map["LetterSurgicallyInspectedLabel"] = "Surgical inspection results";
            map["LetterUnnaturalHealingLabel"] = "Unnatural healing";
#if RIMWORLD_1_6
            map["LetterAncientGravEngineDiscoveredLabel"] = "Ancient grav engine";
            map["LetterLabelOdysseyTurretDisabled"] = "Turret disabled";
            map["LetterLabelOdysseyBlastDoorOpened"] = "Blast door opened";
            map["LetterLabelSkipAbduction"] = "Skip abduction";
            map["LetterLabelFleshbeastsEmerging"] = "Fleshbeasts emerging";
#endif
        }
    }
}
