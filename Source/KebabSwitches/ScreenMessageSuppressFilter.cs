using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Matches live Verse.Messages text against suppressible catalog entries (keyed templates and
    /// hardcoded English). Rebuilds regex/exact caches after language load.
    ///
    /// Сопоставляет живой текст Verse.Messages с каталогом (keyed-шаблоны и жёсткий English).
    /// Кэш regex/exact пересобирается после загрузки языка.
    /// </summary>
    public static class ScreenMessageSuppressFilter
    {
        private static readonly Regex FilledMapEnglishPattern = new Regex(
            @"you\s+have\s+filled\s+the\s+map",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private static readonly Dictionary<string, List<Regex>> PatternsById =
            new Dictionary<string, List<Regex>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, HashSet<string>> ExactById =
            new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        public static int TrackedVariantCount
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<string, HashSet<string>> pair in ExactById)
                {
                    count += pair.Value.Count;
                }

                foreach (KeyValuePair<string, List<Regex>> pair in PatternsById)
                {
                    count += pair.Value.Count;
                }

                return count;
            }
        }

        public static void RebuildCache()
        {
            PatternsById.Clear();
            ExactById.Clear();

            foreach (SuppressibleScreenMessageEntry entry in SuppressibleScreenMessages.All)
            {
                EnsureBuckets(entry.Id);

                if (!string.IsNullOrEmpty(entry.HardcodedEnglish))
                {
                    AddExact(entry.Id, entry.HardcodedEnglish);
                    if (entry.Id == "FilledTheMap")
                    {
                        AddPattern(entry.Id, FilledMapEnglishPattern);
                    }
                }

                if (!string.IsNullOrEmpty(entry.TranslationKey))
                {
                    AddFromTranslationKey(entry.Id, entry.TranslationKey);
                }
            }

            AddExact("GaveBirth", "{0} has given birth.");
            AddExact("GaveBirth", "{0} родила.");

            AddExact("AnimalIsPregnant", "{PAWN_nameDef} is pregnant!");
            AddExact("AnimalIsPregnant", "{PAWN_nameDef} беременна!");

            AddExact("MiscarriedStarvation", "{0} has miscarried due to starvation.");
            AddExact("MiscarriedStarvation", "У {lookup: {0}; Case; 1} из-за голода произошёл выкидыш.");

            AddExact("MiscarriedPoorHealth", "{0} has miscarried due to poor health.");
            AddExact("MiscarriedPoorHealth", "У {lookup: {0}; Case; 1} из-за плохого здоровья произошёл выкидыш.");

            AddExact("SeasonBegun", "{0} has begun.");
            AddExact("SeasonBegun", "{0_gender ? начался : началась : началось} {0}.");

            AddExact("BillComplete", "Bill complete: {0}.");
            AddExact("BillComplete", "Задача завершена: {0}.");

            AddExact("FullyHealed", "{1_label} is fully healed.");
            AddExact("FullyHealed", "{1_label} полностью {1_gender ? здоров : здорова}.");

            AddExact("SocialFight", "{PAWN1_labelShort} started a social fight with {PAWN2_labelShort}.");
            AddExact("SocialFight", "{PAWN1_labelShort} и {PAWN2_labelShort} повздорили.");

            AddExact("ShipChunkDrop", "Chunks of a spacecraft have impacted nearby. You can deconstruct them to yield resources.");
            AddExact("ShipChunkDrop", "Несколько обломков космического корабля упали с неба неподалёку. Их можно разобрать для получения ценных ресурсов.");

            AddExact("StruckMineable", "We have struck {0}!");
            AddExact("StruckMineable", "Мы обнаружили {lookup: {0}; Case; 3}!");

            AddExact("BedLostAssignment", "{0} is no longer assigned to {1_nameDef}.");
            AddExact("BedLostAssignment", "{0} более не назначена {1_nameDef}");

            AddExact("BedDestroyed", "{0} assigned to {1_nameDef} was destroyed.");
            AddExact("BedDestroyed", "{0}, назначенная {1_nameDef}, уничтожена.");

            AddExact("SleepingPawnsWokenUp", "{0} have woken up.");
            AddExact("SleepingPawnsWokenUp", "{0} пробудились.");

            AddExact("NoLongerSocialFighting", "{PAWN1_labelShort} and {PAWN2_labelShort} are no longer social fighting.");
            AddExact("NoLongerSocialFighting", "{PAWN1_labelShort} и {PAWN2_labelShort} прекратили драку.");

            AddExact("NoLongerTargetedInsult", "{PAWN_labelShort} is no longer on an insulting spree against {TARGET_label}.");
            AddExact("NoLongerTargetedInsult", "{PAWN_labelShort} больше не оскорбляет {TARGET_label}.");

            AddExact("InsultingSpreeEnded", "{0} больше не оскорбляет всех подряд.");
            AddExact("TargetedInsultStreamEnded", "{0} прекращает свой поток оскорблений.");
            AddExact("FoodBingeEnded", "{0} перестаёт объедаться.");

            AddExact("NoLongerBingingOnDrug", "{2_labelShort} is no longer binging on {1}.");
            AddExact("NoLongerBingingOnDrug", "{2_labelShort} {2_gender ? прекратил : прекратила} бесконтрольно употреблять {lookup: {1}; Case; 3}.");

            AddExact("BerserkEnded", "{0} успокаивается.");
            AddExact("FireStartingEnded", "{0} больше не поджигает всё подряд.");
            AddExact("JailbreakerEnded", "{0} больше не подстрекает пленников к побегу.");
            AddExact("SlaughtererEnded", "{0} больше не убивает животных.");
            AddExact("MurderousRageEnded", "{0} больше не испытывает гнев.");
            AddExact("GiveUpReturned", "{0} меняет своё мнение и возвращается в поселение.");
            AddExact("TantrumEnded", "{0} больше не бьётся в истерике.");
            AddExact("RedirectedAnger",
                "{PAWN_labelShort} couldn't reach {OLDTARGET_label} so {PAWN_pronoun} decided to redirect {PAWN_possessive} anger at {TARGET_label}.");
            AddExact("RedirectedAnger",
                "{PAWN_labelShort} не может добраться до {lookup: {OLDTARGET_label}; Case; 1} и направляет свою ярость на {lookup: {TARGET_label}; Case; 3}.");
            AddExact("SadisticRageEnded", "{0} прекращает издеваться над пленниками.");
            AddExact("CorpseObsessionEnded", "{0} больше не разоряет могилы.");
            AddExact("WanderSadEnded", "{0} больше не грустит.");
            AddExact("HideInRoomEnded", "{0} больше не прячется в своей комнате.");
            AddExact("PsychoticWanderEnded", "{0} больше не испытывает растерянность.");
            AddExact("ConfusedEnded", "{0} оправляется и возвращается.");
            AddExact("PanicFleeEnded", "{0} больше не паникует.");
            AddExact("ManhunterEnded", "{0} успокаивается, агрессия проходит.");

            AddExact("NewBondRelation", "{HUMAN_labelShort} and {ANIMAL_labelShort} have formed a bond.");
            AddExact("NewBondRelation", "{HUMAN_labelShort} и {ANIMAL_labelShort} привязались друг к другу.");

            AddExact("NewBondRelationNewName",
                "{HUMAN_labelShort} and {1} have formed a bond. {HUMAN_labelShort} has named {ANIMAL_objective} {ANIMAL_nameFull}.");
            AddExact("NewBondRelationNewName",
                "{HUMAN_labelShort} и {1} привязались друг к другу. {HUMAN_labelShort} называет {ANIMAL_possessive} {ANIMAL_nameFull}.");

            AddExact("FoodPoisoning", "{PAWN_labelShort} has gotten food poisoning from: {FOOD_labelShort}. Cause: {2}.");
            AddExact("FoodPoisoning",
                "{PAWN_labelShort} {PAWN_gender ? отравился : отравилась} {lookup: {FOOD_labelShort}; Case; 4}. Причина — {2}.");

            AddExact("RoamerLeaving",
                "{PAWN_labelShort} has started to roam away! {PAWN_pronoun} will leave the map unless an animal handler ropes {PAWN_objective} back to a pen.");
            AddExact("RoamerLeaving",
                "{PAWN_labelShort} убредает! {PAWN_pronoun} покинет это место, если животновод не приведёт {PAWN_possessive} обратно в загон.");

            AddExact("HiveReproduced", "A bug hive has reproduced itself.");
            AddExact("HiveReproduced", "Улей насекомых расплодился.");

            AddExact("TraderCaravanLeaving", "The trade caravan from {0} is leaving.");
            AddExact("TraderCaravanLeaving", "Торговый караван из фракции {0} уходит.");

            AddExact("TraderCaravanDismissed", "The trade caravan from {0} has been dismissed.");
            AddExact("TraderCaravanDismissed", "Вы отказались от торговли с караваном из фракции {0} — он уходит.");

            AddExact("CompSpawnerSpawnedItem", "Item produced: {0}.");
            AddExact("CompSpawnerSpawnedItem", "Воспроизводит: {0}.");

            AddExact("FishCaughtInTrap", "Fish Caught in Trap");

            AddExact("PlantDiedOfCold", "{0} has died because of cold.");
            AddExact("PlantDiedOfRotUnharvested", "{0} has died from rotting due to being left unharvested.");
            AddExact("PlantDiedOfRotLight", "{0} has died due to being exposed to light.");
            AddExact("PlantDiedOfRot", "{0} has died from rotting.");
            AddExact("PlantDiedOfPoison", "{0} has died because of poison.");
            AddExact("PlantDiedOfBlight", "{1_label} has died because of blight.");
            AddExact("PlantDiedOfPollution", "{0} has died because of pollution.");
            AddExact("PlantDiedOfNoPollution", "{0} has died because of a lack of pollution.");
            AddExact("PlantDiedOfRotPollutedTerrain", "{0} has rotted due to polluted terrain.");
#if RIMWORLD_1_6
            AddExact("PlantDiedOfVacuum", "{0} has died from vacuum exposure.");
#endif
            AddExact("MinifiedTreeDied", "An extracted tree just died.");
            AddExact("RottedAwayInStorage", "{1_label} has rotted away in storage.");
            AddExact("DeterioratedAway", "{0} has deteriorated away in storage.");
            AddExact("WornApparelDeterioratedAway", "{0} worn by {1_nameFull} deteriorated away to nothing.");

            AddExact("PlantDiedOfCold", "{0} {0_gender ? погиб : погибла : погибло} от холода.");
            AddExact("PlantDiedOfRotUnharvested", "{0} {0_gender ? погиб : погибла : погибло} от старости.");
            AddExact("PlantDiedOfRotLight",
                "{0} {0_gender ? погиб : погибла : погибло}, так как слишком много времени {0_gender ? провёл : провела : провело} на свету.");
            AddExact("PlantDiedOfRot", "{0} {0_gender ? погиб : погибла : погибло}.");
            AddExact("PlantDiedOfPoison", "{0} {0_gender ? погиб : погибла : погибло} из-за отравления.");
            AddExact("PlantDiedOfBlight", "{0} {0_gender ? погиб : погибла : погибло} из-за болезни.");
            AddExact("PlantDiedOfPollution", "{0} {0_gender ? погиб : погибла : погибло} от загрязнения.");
            AddExact("PlantDiedOfNoPollution",
                "{0} {0_gender ? погиб : погибла : погибло} из-за недостатка загрязнения.");
            AddExact("PlantDiedOfRotPollutedTerrain",
                "{0} {0_gender ? сгнил : сгнила : сгнило} из-за загрязнённой почвы.");
#if RIMWORLD_1_6
            AddExact("PlantDiedOfVacuum",
                "{0} {0_gender ? погиб : погибла : погибло} от вакуума.");
#endif
            AddExact("MinifiedTreeDied", "Извлечённое дерево погибло.");
            AddExact("RottedAwayInStorage", "{1_label} {1_gender ? сгнил : сгнила : сгнило} на складе.");
            AddExact("DeterioratedAway", "{0} {0_gender ? пришёл : пришла : пришло} в негодность на складе.");
            AddExact("WornApparelDeterioratedAway",
                "{0}, {0_gender ? который : которую : которое} {1_gender ? носил : носила} {1_nameFull}, {0_gender ? износился : износилась : износилось} окончательно и {0_gender ? рассыпался : рассыпалась : рассыпалось} в прах.");
#if RIMWORLD_1_6
            AddExact("EspInjured", "{0} is injured and fleeing!");
#endif
            AddFromTranslationKey("RedirectedAnger", "MessageTargetedInsultingSpreeChangedTarget");

            foreach (string id in new List<string>(ExactById.Keys))
            {
                foreach (string template in ExactById[id])
                {
                    Regex regex = KeyedSuppressMatcher.TemplateToRegex(template);
                    if (regex != null)
                    {
                        AddPattern(id, regex);
                    }
                }
            }
        }

        public static string MatchEntryId(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string normalized = SuppressibleScreenMessages.Normalize(text);

            foreach (SuppressibleScreenMessageEntry entry in SuppressibleScreenMessages.All)
            {
                if (ExactById.TryGetValue(entry.Id, out HashSet<string> exact)
                    && exact.Contains(normalized))
                {
                    return entry.Id;
                }

                if (PatternsById.TryGetValue(entry.Id, out List<Regex> patterns))
                {
                    for (int i = 0; i < patterns.Count; i++)
                    {
                        if (patterns[i].IsMatch(normalized))
                        {
                            return entry.Id;
                        }
                    }
                }
            }

            if (FilledMapEnglishPattern.IsMatch(normalized))
            {
                return "FilledTheMap";
            }

            return null;
        }

        public static SuppressibleScreenMessageEntry FindEntry(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            for (int i = 0; i < SuppressibleScreenMessages.All.Length; i++)
            {
                if (string.Equals(SuppressibleScreenMessages.All[i].Id, id, StringComparison.Ordinal))
                {
                    return SuppressibleScreenMessages.All[i];
                }
            }

            return null;
        }

        private static void AddFromTranslationKey(string entryId, string key)
        {
            try
            {
                if (key.CanTranslate())
                {
                    string translated = key.Translate().ToString();
                    if (!string.IsNullOrWhiteSpace(translated)
                        && !string.Equals(translated, key, StringComparison.Ordinal))
                    {
                        AddExact(entryId, translated);
                    }
                }
            }
            catch (Exception)
            {
                // Format-only keys still covered by hardcoded EN/RU templates.
            }
        }

        public static void CollectExact(string id, List<string> target)
        {
            if (target == null || string.IsNullOrEmpty(id))
            {
                return;
            }

            if (ExactById.TryGetValue(id, out HashSet<string> exact))
            {
                foreach (string text in exact)
                {
                    target.Add(text);
                }
            }
        }

        private static void EnsureBuckets(string id)
        {
            if (!PatternsById.ContainsKey(id))
            {
                PatternsById[id] = new List<Regex>();
            }

            if (!ExactById.ContainsKey(id))
            {
                ExactById[id] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void AddExact(string id, string text)
        {
            EnsureBuckets(id);
            string normalized = SuppressibleScreenMessages.Normalize(text);
            if (!string.IsNullOrEmpty(normalized))
            {
                ExactById[id].Add(normalized);
            }
        }

        private static void AddPattern(string id, Regex regex)
        {
            if (regex == null)
            {
                return;
            }

            EnsureBuckets(id);
            PatternsById[id].Add(regex);
        }
    }
}
