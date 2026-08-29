using System;
using System.Text.RegularExpressions;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// One suppressible upper-left screen Message (Verse.Messages feed).
    /// Either a hardcoded English body (no HSK translation) or a vanilla keyed template (EN + RU).
    ///
    /// Одно отключаемое screen Message (лента слева сверху). Либо жёсткий английский текст
    /// (без перевода HSK), либо vanilla keyed-шаблон (EN + RU).
    /// </summary>
    public sealed class SuppressibleScreenMessageEntry
    {
        public string Id { get; }
        public string CheckboxLabelKey { get; }
        public string TranslationKey { get; }
        public string HardcodedEnglish { get; }
        public Func<bool> IsSuppressEnabled { get; }
        public Action<bool> SetSuppressEnabled { get; }

        public SuppressibleScreenMessageEntry(
            string id,
            string checkboxLabelKey,
            Func<bool> isSuppressEnabled,
            Action<bool> setSuppressEnabled,
            string translationKey = null,
            string hardcodedEnglish = null)
        {
            Id = id;
            CheckboxLabelKey = checkboxLabelKey;
            IsSuppressEnabled = isSuppressEnabled;
            SetSuppressEnabled = setSuppressEnabled;
            TranslationKey = translationKey;
            HardcodedEnglish = hardcodedEnglish;
        }

        public string CheckboxLabel => CheckboxLabelKey.Translate();

        /// <summary>
        /// RimWorld tooltip: keyed English template or hardcoded literal shown in-game.
        ///
        /// RimWorld-тултип: английский keyed-шаблон или литерал, как в игре.
        /// </summary>
        public string SourceTooltip => NotificationSourceTooltip.ForScreenMessage(this);
    }

    /// <summary>
    /// Catalog of repeating upper-left screen Messages the player can mute. Includes Core plant-death
    /// and storage-rot lines, the HSK fish-trap catch, and Odyssey vacuum plant-death.
    ///
    /// Каталог повторяющихся screen Messages слева сверху, которые игрок может отключить.
    /// Включает гибель растений и гниение на складе из Core, поимку рыбы в ловушке HSK и гибель
    /// растений от вакуума Odyssey.
    /// </summary>
    public static class SuppressibleScreenMessages
    {
        public const string FilledTheMapEnglish = "You have filled the map.";

        public static readonly SuppressibleScreenMessageEntry[] All =
        {
            new SuppressibleScreenMessageEntry(
                "FilledTheMap",
                "KebabSwitches.Ignore.FilledTheMap",
                () => KebabTweaksSettings.SuppressFilledMapMessage,
                v => KebabTweaksSettings.SuppressFilledMapMessage = v,
                hardcodedEnglish: FilledTheMapEnglish),

            new SuppressibleScreenMessageEntry(
                "GaveBirth",
                "KebabSwitches.Ignore.GaveBirth",
                () => KebabTweaksSettings.SuppressGaveBirthMessage,
                v => KebabTweaksSettings.SuppressGaveBirthMessage = v,
                translationKey: "MessageGaveBirth"),

            new SuppressibleScreenMessageEntry(
                "AnimalIsPregnant",
                "KebabSwitches.Ignore.AnimalIsPregnant",
                () => KebabTweaksSettings.SuppressAnimalIsPregnantMessage,
                v => KebabTweaksSettings.SuppressAnimalIsPregnantMessage = v,
                translationKey: "MessageAnimalIsPregnant"),

            new SuppressibleScreenMessageEntry(
                "MiscarriedStarvation",
                "KebabSwitches.Ignore.MiscarriedStarvation",
                () => KebabTweaksSettings.SuppressMiscarriedStarvationMessage,
                v => KebabTweaksSettings.SuppressMiscarriedStarvationMessage = v,
                translationKey: "MessageMiscarriedStarvation"),

            new SuppressibleScreenMessageEntry(
                "MiscarriedPoorHealth",
                "KebabSwitches.Ignore.MiscarriedPoorHealth",
                () => KebabTweaksSettings.SuppressMiscarriedPoorHealthMessage,
                v => KebabTweaksSettings.SuppressMiscarriedPoorHealthMessage = v,
                translationKey: "MessageMiscarriedPoorHealth"),

            new SuppressibleScreenMessageEntry(
                "SeasonBegun",
                "KebabSwitches.Ignore.SeasonBegun",
                () => KebabTweaksSettings.SuppressSeasonBegunMessage,
                v => KebabTweaksSettings.SuppressSeasonBegunMessage = v,
                translationKey: "MessageSeasonBegun"),

            new SuppressibleScreenMessageEntry(
                "BillComplete",
                "KebabSwitches.Ignore.BillComplete",
                () => KebabTweaksSettings.SuppressBillCompleteMessage,
                v => KebabTweaksSettings.SuppressBillCompleteMessage = v,
                translationKey: "MessageBillComplete"),

            new SuppressibleScreenMessageEntry(
                "FullyHealed",
                "KebabSwitches.Ignore.FullyHealed",
                () => KebabTweaksSettings.SuppressFullyHealedMessage,
                v => KebabTweaksSettings.SuppressFullyHealedMessage = v,
                translationKey: "MessageFullyHealed"),

            new SuppressibleScreenMessageEntry(
                "SocialFight",
                "KebabSwitches.Ignore.SocialFight",
                () => KebabTweaksSettings.SuppressSocialFightMessage,
                v => KebabTweaksSettings.SuppressSocialFightMessage = v,
                translationKey: "MessageSocialFight"),

            new SuppressibleScreenMessageEntry(
                "NewBondRelation",
                "KebabSwitches.Ignore.NewBondRelation",
                () => KebabTweaksSettings.SuppressNewBondRelationMessage,
                v => KebabTweaksSettings.SuppressNewBondRelationMessage = v,
                translationKey: "MessageNewBondRelation"),

            new SuppressibleScreenMessageEntry(
                "NewBondRelationNewName",
                "KebabSwitches.Ignore.NewBondRelationNewName",
                () => KebabTweaksSettings.SuppressNewBondRelationNewNameMessage,
                v => KebabTweaksSettings.SuppressNewBondRelationNewNameMessage = v,
                translationKey: "MessageNewBondRelationNewName"),

            new SuppressibleScreenMessageEntry(
                "FoodPoisoning",
                "KebabSwitches.Ignore.FoodPoisoning",
                () => KebabTweaksSettings.SuppressFoodPoisoningMessage,
                v => KebabTweaksSettings.SuppressFoodPoisoningMessage = v,
                translationKey: "MessageFoodPoisoning"),

            new SuppressibleScreenMessageEntry(
                "RoamerLeaving",
                "KebabSwitches.Ignore.RoamerLeaving",
                () => KebabTweaksSettings.SuppressRoamerLeavingMessage,
                v => KebabTweaksSettings.SuppressRoamerLeavingMessage = v,
                translationKey: "MessageRoamerLeaving"),

            new SuppressibleScreenMessageEntry(
                "HiveReproduced",
                "KebabSwitches.Ignore.HiveReproduced",
                () => KebabTweaksSettings.SuppressHiveReproducedMessage,
                v => KebabTweaksSettings.SuppressHiveReproducedMessage = v,
                translationKey: "MessageHiveReproduced"),

            new SuppressibleScreenMessageEntry(
                "TraderCaravanLeaving",
                "KebabSwitches.Ignore.TraderCaravanLeaving",
                () => KebabTweaksSettings.SuppressTraderCaravanLeavingMessage,
                v => KebabTweaksSettings.SuppressTraderCaravanLeavingMessage = v,
                translationKey: "MessageTraderCaravanLeaving"),

            new SuppressibleScreenMessageEntry(
                "TraderCaravanDismissed",
                "KebabSwitches.Ignore.TraderCaravanDismissed",
                () => KebabTweaksSettings.SuppressTraderCaravanDismissedMessage,
                v => KebabTweaksSettings.SuppressTraderCaravanDismissedMessage = v,
                translationKey: "MessageTraderCaravanDismissed"),

            new SuppressibleScreenMessageEntry(
                "CompSpawnerSpawnedItem",
                "KebabSwitches.Ignore.CompSpawnerSpawnedItem",
                () => KebabTweaksSettings.SuppressCompSpawnerSpawnedItemMessage,
                v => KebabTweaksSettings.SuppressCompSpawnerSpawnedItemMessage = v,
                translationKey: "MessageCompSpawnerSpawnedItem"),

            new SuppressibleScreenMessageEntry(
                "FishCaughtInTrap",
                "KebabSwitches.Ignore.FishCaughtInTrap",
                () => KebabTweaksSettings.SuppressFishCaughtInTrap,
                v => KebabTweaksSettings.SuppressFishCaughtInTrap = v,
                translationKey: "HSK.TrapSuccessTitle"),

            new SuppressibleScreenMessageEntry(
                "PlantDiedOfCold",
                "KebabSwitches.Ignore.PlantDiedOfCold",
                () => KebabTweaksSettings.SuppressPlantDiedOfCold,
                v => KebabTweaksSettings.SuppressPlantDiedOfCold = v,
                translationKey: "MessagePlantDiedOfCold"),

            new SuppressibleScreenMessageEntry(
                "PlantDiedOfRotUnharvested",
                "KebabSwitches.Ignore.PlantDiedOfRotUnharvested",
                () => KebabTweaksSettings.SuppressPlantDiedOfRotUnharvested,
                v => KebabTweaksSettings.SuppressPlantDiedOfRotUnharvested = v,
                translationKey: "MessagePlantDiedOfRot_LeftUnharvested"),

            new SuppressibleScreenMessageEntry(
                "PlantDiedOfRotLight",
                "KebabSwitches.Ignore.PlantDiedOfRotLight",
                () => KebabTweaksSettings.SuppressPlantDiedOfRotLight,
                v => KebabTweaksSettings.SuppressPlantDiedOfRotLight = v,
                translationKey: "MessagePlantDiedOfRot_ExposedToLight"),

            new SuppressibleScreenMessageEntry(
                "PlantDiedOfRot",
                "KebabSwitches.Ignore.PlantDiedOfRot",
                () => KebabTweaksSettings.SuppressPlantDiedOfRot,
                v => KebabTweaksSettings.SuppressPlantDiedOfRot = v,
                translationKey: "MessagePlantDiedOfRot"),

            new SuppressibleScreenMessageEntry(
                "PlantDiedOfPoison",
                "KebabSwitches.Ignore.PlantDiedOfPoison",
                () => KebabTweaksSettings.SuppressPlantDiedOfPoison,
                v => KebabTweaksSettings.SuppressPlantDiedOfPoison = v,
                translationKey: "MessagePlantDiedOfPoison"),

            new SuppressibleScreenMessageEntry(
                "PlantDiedOfBlight",
                "KebabSwitches.Ignore.PlantDiedOfBlight",
                () => KebabTweaksSettings.SuppressPlantDiedOfBlight,
                v => KebabTweaksSettings.SuppressPlantDiedOfBlight = v,
                translationKey: "MessagePlantDiedOfBlight"),

            new SuppressibleScreenMessageEntry(
                "PlantDiedOfPollution",
                "KebabSwitches.Ignore.PlantDiedOfPollution",
                () => KebabTweaksSettings.SuppressPlantDiedOfPollution,
                v => KebabTweaksSettings.SuppressPlantDiedOfPollution = v,
                translationKey: "MessagePlantDiedOfPollution"),

            new SuppressibleScreenMessageEntry(
                "PlantDiedOfNoPollution",
                "KebabSwitches.Ignore.PlantDiedOfNoPollution",
                () => KebabTweaksSettings.SuppressPlantDiedOfNoPollution,
                v => KebabTweaksSettings.SuppressPlantDiedOfNoPollution = v,
                translationKey: "MessagePlantDiedOfNoPollution"),

            new SuppressibleScreenMessageEntry(
                "PlantDiedOfRotPollutedTerrain",
                "KebabSwitches.Ignore.PlantDiedOfRotPollutedTerrain",
                () => KebabTweaksSettings.SuppressPlantDiedOfRotPollutedTerrain,
                v => KebabTweaksSettings.SuppressPlantDiedOfRotPollutedTerrain = v,
                translationKey: "MessagePlantDiedOfRot_PollutedTerrain"),
#if RIMWORLD_1_6
            new SuppressibleScreenMessageEntry(
                "PlantDiedOfVacuum",
                "KebabSwitches.Ignore.PlantDiedOfVacuum",
                () => KebabTweaksSettings.SuppressPlantDiedOfVacuum,
                v => KebabTweaksSettings.SuppressPlantDiedOfVacuum = v,
                translationKey: "MessagePlantDiedOfRot_ExposedToVacuum"),
#endif

            new SuppressibleScreenMessageEntry(
                "MinifiedTreeDied",
                "KebabSwitches.Ignore.MinifiedTreeDied",
                () => KebabTweaksSettings.SuppressMinifiedTreeDied,
                v => KebabTweaksSettings.SuppressMinifiedTreeDied = v,
                translationKey: "MessageMinifiedTreeDied"),

            new SuppressibleScreenMessageEntry(
                "RottedAwayInStorage",
                "KebabSwitches.Ignore.RottedAwayInStorage",
                () => KebabTweaksSettings.SuppressRottedAwayInStorage,
                v => KebabTweaksSettings.SuppressRottedAwayInStorage = v,
                translationKey: "MessageRottedAwayInStorage"),

            new SuppressibleScreenMessageEntry(
                "DeterioratedAway",
                "KebabSwitches.Ignore.DeterioratedAway",
                () => KebabTweaksSettings.SuppressDeterioratedAway,
                v => KebabTweaksSettings.SuppressDeterioratedAway = v,
                translationKey: "MessageDeterioratedAway"),

            new SuppressibleScreenMessageEntry(
                "WornApparelDeterioratedAway",
                "KebabSwitches.Ignore.WornApparelDeterioratedAway",
                () => KebabTweaksSettings.SuppressWornApparelDeterioratedAway,
                v => KebabTweaksSettings.SuppressWornApparelDeterioratedAway = v,
                translationKey: "MessageWornApparelDeterioratedAway"),
        };

        /// <summary>
        /// Builds a short quote prefix from free text (first three words + ellipsis).
        ///
        /// Короткий префикс-цитата из текста (первые три слова + многоточие).
        /// </summary>
        public static string BuildQuotePrefix(string message)
        {
            string[] words = Normalize(message).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return string.Empty;
            }

            if (words.Length <= 3)
            {
                return string.Join(" ", words) + "...";
            }

            return string.Join(" ", words, 0, 3) + "...";
        }

        internal static string Normalize(string text) => KeyedSuppressMatcher.Normalize(text);
    }
}
