using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Without SK.TraitChangerMapComponent, colonists never gain Cannibal from the
    /// memories AteHumanlikeMeatDirect, AteHumanlikeMeatAsIngredient, and
    /// AteHumanlikeMeatAsIngredientCannibal, a long mood never changes NaturalMood or grants
    /// Hardened, and the storyteller never runs TraitChange. Those incidents are absent, or
    /// they name SK.Worker_ChangeTrait when that type is not loaded.
    ///
    /// Fix: While that component is absent, TraitGainMapComponent counts the same memories and
    /// mood every 1200 ticks and grants the same traits and letters. TraitChange and TraitRemove
    /// are created or repaired in code when Misc, Special, and the target tags Map_PlayerHome,
    /// Map_TempIncident, Map_Misc, and Map_RaidBeacon exist. A def that already has a resolving
    /// worker and the required category, base chance, and tags is left as it is. A def whose
    /// worker type does not resolve is pointed at TraitGainChangeWorker or TraitGainRemoveWorker
    /// and filled to that shape. The row is leftover when SK.TraitChangerMapComponent is loaded,
    /// subclasses MapComponent, has MapComponentTick and checkColonistCannibal, and that tick IL
    /// contains 1200 and 2200. The tick does nothing in that case. Core SK absent veils the row.
    ///
    /// Проблема: без SK.TraitChangerMapComponent поселенцы не получают Cannibal от воспоминаний
    /// AteHumanlikeMeatDirect, AteHumanlikeMeatAsIngredient и AteHumanlikeMeatAsIngredientCannibal,
    /// долгое настроение не меняет NaturalMood и не даёт Hardened, а рассказчик не запускает
    /// TraitChange. Этих инцидентов нет, либо они ссылаются на SK.Worker_ChangeTrait, когда этого
    /// типа нет в сборке.
    ///
    /// Исправление: пока этого компонента нет, TraitGainMapComponent считает те же воспоминания и
    /// настроение каждые 1200 тиков и выдаёт те же черты и письма. TraitChange и TraitRemove
    /// создаются или чинятся в коде, когда есть Misc, Special и метки Map_PlayerHome,
    /// Map_TempIncident, Map_Misc и Map_RaidBeacon. Деф с рабочим воркером и нужными категорией,
    /// шансом и метками не трогается. Деф, чей тип воркера не находится, переводится на
    /// TraitGainChangeWorker или TraitGainRemoveWorker и доводится до этой формы. Строка
    /// становится leftover, когда SK.TraitChangerMapComponent загружен, наследует MapComponent,
    /// имеет MapComponentTick и checkColonistCannibal, а IL тика содержит 1200 и 2200. В этом
    /// случае тик ничего не делает. Без Core SK строка под вуалью.
    /// </summary>
    public static class TraitGainFixFeatures
    {
        const string UpstreamTypeName = "SK.TraitChangerMapComponent";
        public const int CheckInterval = 1200;
        public const int CannibalThreshold = 2200;

        const string ChangeDefName = "TraitChange";
        const string RemoveDefName = "TraitRemove";
        const float ChangeBaseChance = 1f;
        const float RemoveBaseChance = 0f;

        static readonly string[] RequiredTargetTags =
        {
            "Map_PlayerHome",
            "Map_TempIncident",
            "Map_Misc",
            "Map_RaidBeacon",
        };

        static readonly string[] CannibalThoughtNames =
        {
            "AteHumanlikeMeatDirect",
            "AteHumanlikeMeatAsIngredient",
            "AteHumanlikeMeatAsIngredientCannibal",
        };

        static readonly string[] RandomTraitExclusions =
        {
            "Brawler",
            "Gay",
            "AnnoyingVoice",
            "CreepyBreathing",
            "Pyromaniac",
            "SpeedOffset",
            "Neurotic",
            "Beauty",
            "AIPawn_BaseTrait",
            "AIRobot_BaseTrait",
        };

        static bool upstreamResolved;
        static bool upstreamPresent;
        static bool incidentsEnsured;
        static List<ThoughtDef> cannibalThoughts;
        static bool thoughtsResolved;
        static HediffDef comaHediff;
        static bool comaResolved;
        static PropertyInfo mutantProperty;
        static PropertyInfo hasTurnedProperty;
        static bool mutantResolved;
        static MethodInfo isMechanicalMethod;
        static bool mechanicalResolved;
        static FieldInfo passionUpdateField;
        static FieldInfo debugPassionField;
        static object passionSettings;
        static bool passionResolved;

        /// <summary>
        /// Veils the row when Core SK is not active. Called even if the checkbox is off.
        ///
        /// Вешает вуаль, если Core SK не активен. Вызывается и при выключенном чекбоксе.
        /// </summary>
        public static void NoteIfMissing()
        {
            if (ModLister.GetActiveModWithIdentifier("skyarkhangel.HSK") != null)
            {
                return;
            }

            FeatureAvailability.SkipMissingMods(
                FeatureAvailability.TraitGain,
                "Trait Gain Fix",
                FeatureAvailability.CoreSk);
        }

        /// <summary>
        /// Creates or repairs the trait incidents when the fix is on and the HSK component is absent.
        ///
        /// Создаёт или чинит инциденты черт, когда фикс включён и компонента HSK нет.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            if (harmony == null ||
                FeatureAvailability.IsBlocked(FeatureAvailability.TraitGain))
            {
                return;
            }

            if (UpstreamTraitChangerPresent() || !KebabTweaksSettings.IsTraitGainFixEnabled())
            {
                return;
            }

            EnsureIncidents();
            Log.Message("[HSK kebab tweaks] Trait Gain Fix applied.");
        }

        /// <summary>
        /// True when the loaded SK.TraitChangerMapComponent is the counter this fix replaces:
        /// a MapComponent with MapComponentTick, field checkColonistCannibal, and tick IL
        /// constants 1200 and 2200.
        ///
        /// True, если загруженный SK.TraitChangerMapComponent - тот счётчик, который этот фикс
        /// заменяет: MapComponent с MapComponentTick, полем checkColonistCannibal и константами
        /// 1200 и 2200 в IL тика.
        /// </summary>
        public static bool UpstreamTraitChangerPresent()
        {
            if (upstreamResolved)
            {
                return upstreamPresent;
            }

            upstreamResolved = true;
            Type type = AccessTools.TypeByName(UpstreamTypeName);
            if (type == null || type.IsAbstract || !typeof(MapComponent).IsAssignableFrom(type))
            {
                return false;
            }

            MethodInfo tick = AccessTools.DeclaredMethod(type, "MapComponentTick");
            FieldInfo counter = AccessTools.Field(type, "checkColonistCannibal");
            if (tick == null || counter == null)
            {
                return false;
            }

            upstreamPresent = IlHasInt(tick, CheckInterval) && IlHasInt(tick, CannibalThreshold);
            return upstreamPresent;
        }

        /// <summary>
        /// Adds TraitChange and TraitRemove, or repairs a def whose worker type does not resolve,
        /// after Misc, Special, and the four map tags are confirmed. A def that already matches
        /// is not rewritten.
        ///
        /// Добавляет TraitChange и TraitRemove или чинит деф, чей тип воркера не находится,
        /// после проверки Misc, Special и четырёх меток карты. Уже подходящий деф не переписывается.
        /// </summary>
        public static void EnsureIncidents()
        {
            if (incidentsEnsured ||
                UpstreamTraitChangerPresent() ||
                !KebabTweaksSettings.IsTraitGainFixEnabled() ||
                FeatureAvailability.IsBlocked(FeatureAvailability.TraitGain))
            {
                return;
            }

            incidentsEnsured = true;
            IncidentCategoryDef misc = DefDatabase<IncidentCategoryDef>.GetNamedSilentFail("Misc");
            IncidentCategoryDef special = DefDatabase<IncidentCategoryDef>.GetNamedSilentFail("Special");
            if (misc == null || special == null || !TryGetRequiredTags(out List<IncidentTargetTagDef> tags))
            {
                FeatureAvailability.WarnOnce(
                    FeatureAvailability.TraitGain + ":incidents",
                    "[HSK kebab tweaks] Trait Gain Fix skipped incident defs: Misc, Special, or a map target tag is missing.");
                return;
            }

            EnsureOne(
                ChangeDefName,
                "trait change",
                misc,
                ChangeBaseChance,
                typeof(TraitGainChangeWorker),
                tags);
            EnsureOne(
                RemoveDefName,
                "trait removed",
                special,
                RemoveBaseChance,
                typeof(TraitGainRemoveWorker),
                tags);
        }

        /// <summary>
        /// Human-meat memories that last at least part of a day. Empty when none of the three
        /// thoughts exist in that form.
        ///
        /// Воспоминания о человеческом мясе, которые длятся хотя бы часть дня. Пусто, если ни
        /// одной из трёх мыслей в таком виде нет.
        /// </summary>
        public static List<ThoughtDef> CannibalThoughts()
        {
            if (thoughtsResolved)
            {
                return cannibalThoughts;
            }

            thoughtsResolved = true;
            cannibalThoughts = new List<ThoughtDef>();
            for (int i = 0; i < CannibalThoughtNames.Length; i++)
            {
                ThoughtDef thought = DefDatabase<ThoughtDef>.GetNamedSilentFail(CannibalThoughtNames[i]);
                if (thought != null && thought.durationDays > 0f)
                {
                    cannibalThoughts.Add(thought);
                }
            }

            if (cannibalThoughts.Count == 0)
            {
                FeatureAvailability.WarnOnce(
                    FeatureAvailability.TraitGain + ":thoughts",
                    "[HSK kebab tweaks] Trait Gain Fix: human-meat thoughts are missing or do not last, so cannibalism is not counted.");
            }

            return cannibalThoughts;
        }

        /// <summary>
        /// Highest humanlike trait count in the world, and at least 3.
        ///
        /// Наибольшее число черт у человекоподобного в мире, и не меньше 3.
        /// </summary>
        public static int MaxTraitCount()
        {
            int count = 3;
            if (Find.WorldPawns == null)
            {
                return count;
            }

            foreach (Pawn pawn in Find.WorldPawns.AllPawnsAlive)
            {
                if (pawn?.story?.traits?.allTraits == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike)
                {
                    continue;
                }

                if (pawn.story.traits.allTraits.Count > count)
                {
                    count = pawn.story.traits.allTraits.Count;
                }
            }

            return count;
        }

        /// <summary>
        /// Colonist, prisoner, or slave who is not a mechanoid, for the trait letters.
        ///
        /// Поселенец, пленник или раб, не механоид, для писем о черте.
        /// </summary>
        public static bool IsPlayerPawn(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            if (!pawn.IsColonist && !pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony)
            {
                return false;
            }

            return !IsMechanical(pawn);
        }

        /// <summary>
        /// True when the pawn does not already have the trait, none of the current traits list
        /// it as a conflict, and the alien race does not disallow it.
        ///
        /// True, если у пешки ещё нет этой черты, текущие черты не числят её конфликтом и раса
        /// пришельцев её не запрещает.
        /// </summary>
        public static bool SuitableTrait(Pawn pawn, TraitDef trait)
        {
            if (pawn?.story?.traits == null || trait == null || pawn.story.traits.HasTrait(trait))
            {
                return false;
            }

            if (Conflicts(pawn, trait) || AlienDisallows(pawn, trait))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// True when the trait def has this degree, or has no degree list and the degree is 0.
        ///
        /// True, если у черты есть эта степень, либо списка степеней нет и степень равна 0.
        /// </summary>
        public static bool DegreeExists(TraitDef trait, int degree)
        {
            if (trait == null)
            {
                return false;
            }

            if (trait.degreeDatas.NullOrEmpty())
            {
                return degree == 0;
            }

            for (int i = 0; i < trait.degreeDatas.Count; i++)
            {
                if (trait.degreeDatas[i] != null && trait.degreeDatas[i].degree == degree)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool GrantTrait(Pawn pawn, TraitDef trait, int degree, List<TraitDef> changed)
        {
            if (!SuitableTrait(pawn, trait) || !DegreeExists(trait, degree))
            {
                return false;
            }

            pawn.story.traits.GainTrait(new Trait(trait, degree));
            Trait gained = pawn.story.traits.GetTrait(trait);
            if (gained == null || gained.Degree != degree)
            {
                return false;
            }

            changed?.Add(trait);
            RefreshPawn(pawn);
            return true;
        }

        /// <summary>
        /// Refreshes work types and mood after a trait change that did not go through GrantTrait.
        ///
        /// Обновляет типы работы и настроение после смены черты, которая прошла не через GrantTrait.
        /// </summary>
        public static void RefreshAfterGrant(Pawn pawn)
        {
            if (pawn != null)
            {
                RefreshPawn(pawn);
            }
        }

        public static void RemoveTrait(Pawn pawn, TraitDef trait, List<TraitDef> changed)
        {
            if (pawn?.story?.traits?.allTraits == null || trait == null || !pawn.story.traits.HasTrait(trait))
            {
                return;
            }

            List<Trait> traits = pawn.story.traits.allTraits;
            for (int i = traits.Count - 1; i >= 0; i--)
            {
                if (traits[i] != null && traits[i].def == trait)
                {
                    traits.RemoveAt(i);
                }
            }

            changed?.Add(trait);
            RefreshPawn(pawn);
        }

        /// <summary>
        /// Rewrites passions for the listed skills while Core SK PassionUpdate is on.
        ///
        /// Переписывает увлечения по перечисленным навыкам, пока в Core SK включён PassionUpdate.
        /// </summary>
        public static void PassionUpdate(Pawn pawn, List<SkillDef> skillsToEffect)
        {
            if (pawn?.story?.traits?.allTraits == null || pawn.skills == null ||
                skillsToEffect == null || skillsToEffect.Count == 0 || !PassionUpdateOn())
            {
                return;
            }

            Dictionary<SkillDef, float> values = new Dictionary<SkillDef, float>();
            List<Trait> traits = pawn.story.traits.allTraits;
            for (int i = 0; i < traits.Count; i++)
            {
                TraitDegreeData data = traits[i]?.CurrentData;
                if (data == null)
                {
                    continue;
                }

                if (data.skillGains != null)
                {
                    for (int s = 0; s < data.skillGains.Count; s++)
                    {
                        SkillGain gain = data.skillGains[s];
                        if (gain?.skill != null)
                        {
                            AddPassion(values, gain.skill, gain.amount, skillsToEffect);
                        }
                    }
                }

                AddStatPassions(values, data.statOffsets, skillsToEffect);
                AddStatPassions(values, data.statFactors, skillsToEffect);
            }

            if (DebugPassionOn() && values.Count > 0)
            {
                Log.Warning("[Debug Passions Enabled] Pawn: " + pawn.LabelShort + " traits changed.");
            }

            foreach (KeyValuePair<SkillDef, float> entry in values)
            {
                SkillRecord record = pawn.skills.GetSkill(entry.Key);
                if (record == null || record.TotallyDisabled)
                {
                    continue;
                }

                record.passion = entry.Value == 1f
                    ? Passion.Minor
                    : entry.Value == 2f ? Passion.Major : Passion.None;
            }
        }

        public static void CollectChangedSkills(List<TraitDef> changed, List<SkillDef> skills)
        {
            if (changed == null || skills == null)
            {
                return;
            }

            for (int i = 0; i < changed.Count; i++)
            {
                TraitDef trait = changed[i];
                if (trait?.degreeDatas == null)
                {
                    continue;
                }

                for (int d = 0; d < trait.degreeDatas.Count; d++)
                {
                    TraitDegreeData data = trait.degreeDatas[d];
                    if (data == null)
                    {
                        continue;
                    }

                    if (data.skillGains != null)
                    {
                        for (int s = 0; s < data.skillGains.Count; s++)
                        {
                            if (data.skillGains[s]?.skill != null)
                            {
                                skills.Add(data.skillGains[s].skill);
                            }
                        }
                    }

                    CollectStatSkills(data.statOffsets, skills);
                    CollectStatSkills(data.statFactors, skills);
                }
            }
        }

        /// <summary>
        /// True when Coma is a hediff and this pawn has it. Mood stays at zero in a coma.
        ///
        /// True, если Coma есть среди болезней и она есть у пешки. В коме настроение остаётся нулём.
        /// </summary>
        public static bool InComa(Pawn pawn)
        {
            if (!comaResolved)
            {
                comaResolved = true;
                comaHediff = DefDatabase<HediffDef>.GetNamedSilentFail("Coma");
            }

            return comaHediff != null &&
                   pawn?.health?.hediffSet != null &&
                   pawn.health.hediffSet.HasHediff(comaHediff);
        }

        /// <summary>
        /// True when the pawn is a turned Anomaly mutant. Absent mutant data is not turned.
        ///
        /// True, если пешка - обращённый мутант Anomaly. Нет данных мутанта - не обращён.
        /// </summary>
        public static bool TurnedMutant(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            if (!mutantResolved)
            {
                mutantResolved = true;
                mutantProperty = AccessTools.Property(typeof(Pawn), "mutant");
            }

            if (mutantProperty == null)
            {
                return false;
            }

            object tracker;
            try
            {
                tracker = mutantProperty.GetValue(pawn);
            }
            catch (Exception)
            {
                return false;
            }

            if (tracker == null)
            {
                return false;
            }

            if (hasTurnedProperty == null)
            {
                hasTurnedProperty = AccessTools.Property(tracker.GetType(), "HasTurned");
            }

            if (hasTurnedProperty == null)
            {
                return false;
            }

            try
            {
                return hasTurnedProperty.GetValue(tracker) is bool turned && turned;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// A random free colonist on the map who is not a mechanoid or a wild man.
        ///
        /// Случайный свободный поселенец на карте, не механоид и не дикарь.
        /// </summary>
        public static Pawn RandomColonist(IIncidentTarget target)
        {
            Map map = target as Map;
            if (map?.mapPawns == null)
            {
                return null;
            }

            List<Pawn> colonists = new List<Pawn>();
            foreach (Pawn pawn in map.mapPawns.FreeColonistsAndPrisoners)
            {
                if (pawn != null && pawn.IsColonist && pawn.story?.traits != null &&
                    !pawn.NonHumanlikeOrWildMan() && !IsMechanical(pawn))
                {
                    colonists.Add(pawn);
                }
            }

            if (colonists.Count == 0)
            {
                return null;
            }

            return colonists[new IntRange(0, colonists.Count - 1).RandomInRange];
        }

        /// <summary>
        /// Traits this pawn can be given by TraitChange. Exclusion defs that are not loaded are skipped.
        ///
        /// Черты, которые TraitChange может выдать этой пешке. Незагруженные исключения пропускаются.
        /// </summary>
        public static List<TraitDef> ValidRandomTraits(Pawn pawn)
        {
            List<TraitDef> excluded = new List<TraitDef>();
            for (int i = 0; i < RandomTraitExclusions.Length; i++)
            {
                TraitDef excludedDef = DefDatabase<TraitDef>.GetNamedSilentFail(RandomTraitExclusions[i]);
                if (excludedDef != null)
                {
                    excluded.Add(excludedDef);
                }
            }

            List<TraitDef> valid = new List<TraitDef>();
            List<TraitDef> all = DefDatabase<TraitDef>.AllDefsListForReading;
            for (int i = 0; i < all.Count; i++)
            {
                TraitDef trait = all[i];
                if (trait == null || excluded.Contains(trait))
                {
                    continue;
                }

                if (pawn != null && trait.GetGenderSpecificCommonality(pawn.gender) > 0.01f)
                {
                    valid.Add(trait);
                }
            }

            return valid;
        }

        public static void SendTraitLetter(Pawn pawn, string text, LetterDef letter)
        {
            if (!IsPlayerPawn(pawn) || text.NullOrEmpty() || letter == null || Find.LetterStack == null)
            {
                return;
            }

            Find.LetterStack.ReceiveLetter("TraitChangedLabel".Translate(), text, letter, pawn);
        }

        static void EnsureOne(
            string defName,
            string label,
            IncidentCategoryDef category,
            float baseChance,
            Type worker,
            List<IncidentTargetTagDef> tags)
        {
            IncidentDef existing = DefDatabase<IncidentDef>.GetNamedSilentFail(defName);
            if (existing == null)
            {
                AddIncident(defName, label, category, baseChance, worker, tags);
                return;
            }

            if (MatchesRequiredForm(existing, category, baseChance, tags))
            {
                return;
            }

            if (WorkerResolves(existing))
            {
                FeatureAvailability.WarnOnce(
                    FeatureAvailability.TraitGain + ":" + defName,
                    "[HSK kebab tweaks] Trait Gain Fix left " + defName +
                    " unchanged: its worker resolves, but the category, chance, or target tags are not the required form.");
                return;
            }

            existing.label = label;
            existing.workerClass = worker;
            existing.category = category;
            existing.baseChance = baseChance;
            existing.targetTags = tags;
            AccessTools.Field(typeof(IncidentDef), "worker")?.SetValue(existing, null);
            Log.Message("[HSK kebab tweaks] Trait Gain Fix repaired " + defName + ".");
        }

        static void AddIncident(
            string defName,
            string label,
            IncidentCategoryDef category,
            float baseChance,
            Type worker,
            List<IncidentTargetTagDef> tags)
        {
            IncidentDef def = new IncidentDef
            {
                defName = defName,
                label = label,
                workerClass = worker,
                category = category,
                baseChance = baseChance,
                targetTags = new List<IncidentTargetTagDef>(tags),
                modContentPack = LoadedModManager.GetMod<KebabTweaksMod>()?.Content,
            };

            try
            {
                DefDatabase<IncidentDef>.Add(def);
            }
            catch (Exception ex)
            {
                FeatureAvailability.WarnOnce(
                    FeatureAvailability.TraitGain + ":add:" + defName,
                    "[HSK kebab tweaks] Trait Gain Fix could not add " + defName + ": " + ex.Message);
                return;
            }

            Log.Message("[HSK kebab tweaks] Trait Gain Fix added " + defName + ".");
        }

        static bool MatchesRequiredForm(
            IncidentDef def,
            IncidentCategoryDef category,
            float baseChance,
            List<IncidentTargetTagDef> tags)
        {
            if (!WorkerResolves(def) || def.category != category || !Mathf.Approximately(def.baseChance, baseChance))
            {
                return false;
            }

            if (def.targetTags == null)
            {
                return false;
            }

            for (int i = 0; i < tags.Count; i++)
            {
                if (!def.targetTags.Contains(tags[i]))
                {
                    return false;
                }
            }

            return true;
        }

        static bool WorkerResolves(IncidentDef def)
        {
            return def?.workerClass != null && typeof(IncidentWorker).IsAssignableFrom(def.workerClass);
        }

        static bool TryGetRequiredTags(out List<IncidentTargetTagDef> tags)
        {
            tags = new List<IncidentTargetTagDef>();
            for (int i = 0; i < RequiredTargetTags.Length; i++)
            {
                IncidentTargetTagDef tag = DefDatabase<IncidentTargetTagDef>.GetNamedSilentFail(RequiredTargetTags[i]);
                if (tag == null)
                {
                    tags = null;
                    return false;
                }

                tags.Add(tag);
            }

            return true;
        }

        static bool IlHasInt(MethodInfo method, int value)
        {
            byte[] il;
            try
            {
                il = method?.GetMethodBody()?.GetILAsByteArray();
            }
            catch (Exception)
            {
                return false;
            }

            if (il == null || il.Length < 5)
            {
                return false;
            }

            byte[] bytes = BitConverter.GetBytes(value);
            for (int i = 0; i <= il.Length - 5; i++)
            {
                if (il[i] != 0x20)
                {
                    continue;
                }

                if (il[i + 1] == bytes[0] && il[i + 2] == bytes[1] &&
                    il[i + 3] == bytes[2] && il[i + 4] == bytes[3])
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsMechanical(Pawn pawn)
        {
            if (pawn?.RaceProps == null)
            {
                return false;
            }

            if (!mechanicalResolved)
            {
                mechanicalResolved = true;
                Type utility = AccessTools.TypeByName("SK.SK_Utility");
                isMechanicalMethod = utility == null ? null : AccessTools.Method(utility, "isMechanical");
            }

            if (isMechanicalMethod != null)
            {
                try
                {
                    if (isMechanicalMethod.Invoke(null, new object[] { pawn }) is bool mechanical)
                    {
                        return mechanical;
                    }
                }
                catch (Exception)
                {
                    return pawn.RaceProps.IsMechanoid;
                }
            }

            return pawn.RaceProps.IsMechanoid;
        }

        static bool Conflicts(Pawn pawn, TraitDef trait)
        {
            List<Trait> traits = pawn.story.traits.allTraits;
            for (int i = 0; i < traits.Count; i++)
            {
                TraitDef current = traits[i]?.def;
                if (current?.conflictingTraits != null && current.conflictingTraits.Contains(trait))
                {
                    return true;
                }
            }

            return false;
        }

        static bool AlienDisallows(Pawn pawn, TraitDef trait)
        {
            if (pawn?.def == null || trait == null)
            {
                return false;
            }

            try
            {
                object alienRace = AccessTools.Field(pawn.def.GetType(), "alienRace")?.GetValue(pawn.def);
                if (alienRace == null)
                {
                    return false;
                }

                object general = AccessTools.Field(alienRace.GetType(), "generalSettings")?.GetValue(alienRace);
                if (general == null)
                {
                    return false;
                }

                if (!(AccessTools.Field(general.GetType(), "disallowedTraits")?.GetValue(general) is System.Collections.IEnumerable list))
                {
                    return false;
                }

                foreach (object entry in list)
                {
                    if (entry == null)
                    {
                        continue;
                    }

                    object traitWithDegree = AccessTools.Field(entry.GetType(), "entry")?.GetValue(entry);
                    if (traitWithDegree == null)
                    {
                        continue;
                    }

                    if (AccessTools.Field(traitWithDegree.GetType(), "def")?.GetValue(traitWithDegree) == trait)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        static void RefreshPawn(Pawn pawn)
        {
            pawn.Notify_DisabledWorkTypesChanged();
            if (pawn.skills != null)
            {
                pawn.skills.Notify_SkillDisablesChanged();
            }

            if (!pawn.Dead && pawn.RaceProps != null && pawn.RaceProps.Humanlike &&
                pawn.needs?.mood?.thoughts?.situational != null)
            {
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
        }

        static bool PassionUpdateOn()
        {
            ResolvePassionSettings();
            return passionUpdateField?.GetValue(passionSettings) is bool on && on;
        }

        static bool DebugPassionOn()
        {
            ResolvePassionSettings();
            return debugPassionField?.GetValue(passionSettings) is bool on && on;
        }

        static void ResolvePassionSettings()
        {
            if (passionResolved)
            {
                return;
            }

            passionResolved = true;
            Type controller = AccessTools.TypeByName("SK.Controller");
            FieldInfo settingsField = controller?.GetField("Settings", BindingFlags.Public | BindingFlags.Static);
            passionSettings = settingsField?.GetValue(null);
            if (passionSettings == null)
            {
                return;
            }

            passionUpdateField = passionSettings.GetType().GetField("PassionUpdate");
            debugPassionField = passionSettings.GetType().GetField("DebugPassionUpdate");
        }

        static void AddStatPassions(
            Dictionary<SkillDef, float> values,
            List<StatModifier> modifiers,
            List<SkillDef> skillsToEffect)
        {
            if (modifiers == null)
            {
                return;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier modifier = modifiers[i];
                if (modifier?.stat == null)
                {
                    continue;
                }

                AddNeedPassions(values, modifier.stat.skillNeedFactors, modifier.value, skillsToEffect);
                AddNeedPassions(values, modifier.stat.skillNeedOffsets, modifier.value, skillsToEffect);
            }
        }

        static void AddNeedPassions(
            Dictionary<SkillDef, float> values,
            List<SkillNeed> needs,
            float statValue,
            List<SkillDef> skillsToEffect)
        {
            if (needs == null)
            {
                return;
            }

            for (int i = 0; i < needs.Count; i++)
            {
                if (needs[i]?.skill != null)
                {
                    AddPassion(values, needs[i].skill, statValue, skillsToEffect);
                }
            }
        }

        static void AddPassion(
            Dictionary<SkillDef, float> values,
            SkillDef skill,
            float statValue,
            List<SkillDef> skillsToEffect)
        {
            if (skill == null || skillsToEffect == null || !skillsToEffect.Contains(skill))
            {
                return;
            }

            float delta = statValue > 0f ? 1f : statValue < 0f ? -1f : 0f;
            if (!values.TryGetValue(skill, out float current))
            {
                values[skill] = Mathf.Clamp(delta, 0f, 2f);
                return;
            }

            if (current < 2f && current >= 0f)
            {
                values[skill] = Mathf.Clamp(current + delta, 0f, 2f);
            }
        }

        static void CollectStatSkills(List<StatModifier> modifiers, List<SkillDef> skills)
        {
            if (modifiers == null)
            {
                return;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatDef stat = modifiers[i]?.stat;
                if (stat == null)
                {
                    continue;
                }

                CollectNeedSkills(stat.skillNeedFactors, skills);
                CollectNeedSkills(stat.skillNeedOffsets, skills);
            }
        }

        static void CollectNeedSkills(List<SkillNeed> needs, List<SkillDef> skills)
        {
            if (needs == null)
            {
                return;
            }

            for (int i = 0; i < needs.Count; i++)
            {
                if (needs[i]?.skill != null)
                {
                    skills.Add(needs[i].skill);
                }
            }
        }
    }

    /// <summary>
    /// Counts human-meat memories, a long mood, and a long negative stretch on each map.
    /// Coma is skipped because mood stays at zero. A newborn still gets the cannibal count;
    /// mood and Hardened wait, and later colonists on that pass are still checked. Each
    /// Hardened bonus is a new Trait. The tick does nothing while SK.TraitChangerMapComponent
    /// with the 1200 and 2200 counter is loaded.
    ///
    /// Считает воспоминания о человеческом мясе, долгое настроение и долгий негатив на каждой
    /// карте. Кома пропускается, потому что настроение остаётся нулём. Новорождённый всё ещё
    /// получает счёт каннибализма; настроение и Hardened ждут, а следующие поселенцы этого
    /// прохода всё ещё проверяются. Каждый бонус Hardened - новый Trait. Тик ничего не делает,
    /// пока загружен SK.TraitChangerMapComponent со счётчиком 1200 и 2200.
    /// </summary>
    public class TraitGainMapComponent : MapComponent
    {
        const int MoodLow = -800;
        const int MoodHigh = 1400;
        const int NegativeTime = 2600;
        const float ExtraTraitChance = 0.3f;

        static readonly (string defName, int degree)[] HardenedBonus =
        {
            ("Nerves", 2),
            ("Numb", 0),
            ("Tough", 0),
            ("Confident", 0),
        };

        int cachedMaxTraitCount;
        List<int> idList = new List<int>();
        Dictionary<int, int> moods = new Dictionary<int, int>();
        Dictionary<int, int> cannibal = new Dictionary<int, int>();
        Dictionary<int, int> negativeTime = new Dictionary<int, int>();

        public TraitGainMapComponent(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cachedMaxTraitCount, "traitGainMaxTraits", 0);
            Scribe_Collections.Look(ref idList, "traitGainIds", LookMode.Value);
            Scribe_Collections.Look(ref moods, "traitGainMoods", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref cannibal, "traitGainCannibal", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref negativeTime, "traitGainTime", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                EnsureCollections();
            }
        }

        /// <summary>
        /// Every 1200 ticks, counts lasting human-meat memories and mood for spawned colonists
        /// and prisoners. Cannibal passes 2200. Mood uses -800 and 1400. Hardened follows a
        /// long negative stretch.
        ///
        /// Каждые 1200 тиков считает длящиеся воспоминания о человеческом мясе и настроение
        /// появившихся поселенцев и пленников. Каннибализм проходит 2200. Настроение использует
        /// -800 и 1400. Hardened приходит после долгого негатива.
        /// </summary>
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager == null || Find.TickManager.TicksGame % TraitGainFixFeatures.CheckInterval != 0)
            {
                return;
            }

            if (FixSymptomProbe.IsTraitGainFixLeftover() || !KebabTweaksSettings.IsTraitGainFixEnabled())
            {
                return;
            }

            TraitGainFixFeatures.EnsureIncidents();
            if (map?.mapPawns == null)
            {
                return;
            }

            EnsureCollections();
            if (cachedMaxTraitCount <= 0)
            {
                cachedMaxTraitCount = TraitGainFixFeatures.MaxTraitCount();
            }

            foreach (Pawn colonist in map.mapPawns.FreeColonistsAndPrisoners)
            {
                if (colonist != null && !idList.Contains(colonist.thingIDNumber))
                {
                    idList.Add(colonist.thingIDNumber);
                }
            }

            Dictionary<int, Pawn> byId = new Dictionary<int, Pawn>();
            foreach (Pawn pawn in map.mapPawns.AllPawns)
            {
                if (pawn != null)
                {
                    byId[pawn.thingIDNumber] = pawn;
                }
            }

            for (int i = idList.Count - 1; i >= 0; i--)
            {
                int id = idList[i];
                if (!byId.TryGetValue(id, out Pawn pawn) || pawn == null || !pawn.Spawned ||
                    TraitGainFixFeatures.TurnedMutant(pawn))
                {
                    idList.RemoveAt(i);
                    moods.Remove(id);
                    cannibal.Remove(id);
                    negativeTime.Remove(id);
                }
            }

            List<ThoughtDef> thoughts = TraitGainFixFeatures.CannibalThoughts();
            TraitDef cannibalTrait = DefDatabase<TraitDef>.GetNamedSilentFail("Cannibal");
            TraitDef naturalMood = DefDatabase<TraitDef>.GetNamedSilentFail("NaturalMood");
            TraitDef hardened = DefDatabase<TraitDef>.GetNamedSilentFail("Hardened");
            for (int i = 0; i < idList.Count; i++)
            {
                int id = idList[i];
                if (!byId.TryGetValue(id, out Pawn pawn) || pawn?.story?.traits == null || !pawn.Spawned)
                {
                    continue;
                }

                if (TraitGainFixFeatures.InComa(pawn))
                {
                    continue;
                }

                EnsureCounter(moods, id);
                EnsureCounter(cannibal, id);
                EnsureCounter(negativeTime, id);
                List<TraitDef> changed = new List<TraitDef>();
                if (pawn.RaceProps?.FleshType == FleshTypeDefOf.Normal)
                {
                    TraitGainFixFeatures.RemoveTrait(pawn, DefDatabase<TraitDef>.GetNamedSilentFail("AIPawn_BaseTrait"), changed);
                    TraitGainFixFeatures.RemoveTrait(pawn, DefDatabase<TraitDef>.GetNamedSilentFail("AIRobot_BaseTrait"), changed);
                }

                if (cannibalTrait != null && !pawn.story.traits.HasTrait(cannibalTrait) &&
                    pawn.needs?.mood?.thoughts?.memories?.Memories != null)
                {
                    List<Thought_Memory> memories = pawn.needs.mood.thoughts.memories.Memories;
                    for (int m = 0; m < memories.Count; m++)
                    {
                        if (memories[m] != null && thoughts.Contains(memories[m].def))
                        {
                            cannibal[id] += 1;
                        }
                    }

                    if (cannibal[id] > TraitGainFixFeatures.CannibalThreshold &&
                        TraitGainFixFeatures.GrantTrait(pawn, cannibalTrait, 0, changed))
                    {
                        TraitGainFixFeatures.SendTraitLetter(
                            pawn,
                            pawn.LabelShort + " " + "TraitCannibal".Translate(),
                            LetterDefOf.NegativeEvent);
                    }
                }

                if (pawn.DevelopmentalStage == DevelopmentalStage.Newborn)
                {
                    ApplyPassions(pawn, changed);
                    continue;
                }

                if ((hardened == null || !pawn.story.traits.HasTrait(hardened)) && pawn.needs?.mood != null)
                {
                    ScoreMood(pawn, id, changed);
                    if (hardened != null && !pawn.story.traits.HasTrait(hardened))
                    {
                        if (HasDegree(pawn, naturalMood, -2) && negativeTime[id] >= NegativeTime)
                        {
                            MakeHarder(pawn, hardened, changed);
                        }
                        else if (negativeTime[id] >= NegativeTime * 3)
                        {
                            MakeHarder(pawn, hardened, changed);
                        }
                    }

                    if (moods[id] < 0)
                    {
                        negativeTime[id] += 1;
                    }
                }

                ApplyPassions(pawn, changed);
            }
        }

        void ScoreMood(Pawn pawn, int id, List<TraitDef> changed)
        {
            TraitDef naturalMood = DefDatabase<TraitDef>.GetNamedSilentFail("NaturalMood");
            MentalBreaker breaker = new MentalBreaker(pawn);
            if (breaker.BreakMinorIsImminent)
            {
                moods[id] -= 1;
            }
            else if (breaker.BreakMajorIsImminent)
            {
                moods[id] -= 2;
            }
            else if (breaker.BreakExtremeIsImminent)
            {
                moods[id] -= 4;
            }
            else if (pawn.InAggroMentalState)
            {
                moods[id] -= 5;
            }
            else if (pawn.needs.mood.CurLevelPercentage > 0.9f)
            {
                moods[id] += 3;
            }
            else if (pawn.needs.mood.CurLevelPercentage > 0.7f)
            {
                moods[id] += 1;
            }
            else
            {
                if (moods[id] < 0)
                {
                    moods[id] += 1;
                }

                if (moods[id] > 0)
                {
                    moods[id] -= 1;
                }
            }

            if (moods[id] < MoodLow)
            {
                if (new IntRange(1, 5).RandomInRange == 1)
                {
                    int degree = HasDegree(pawn, naturalMood, -1) ? -2 : -1;
                    if (TraitGainFixFeatures.GrantTrait(pawn, naturalMood, degree, changed))
                    {
                        string body = degree == 1
                            ? "TraitGoodMoodA".Translate() + ", " + pawn.LabelShort + " " + "TraitGoodMoodB".Translate()
                            : "TraitBadMoodA".Translate() + ", " + pawn.LabelShort + " " + "TraitBadMoodB".Translate();
                        TraitGainFixFeatures.SendTraitLetter(
                            pawn,
                            body,
                            degree == 1 ? LetterDefOf.PositiveEvent : LetterDefOf.NegativeEvent);
                    }
                }
                else
                {
                    moods[id] -= MoodLow / new IntRange(2, 4).RandomInRange;
                }
            }

            if (moods[id] > MoodHigh)
            {
                if (new IntRange(1, 5).RandomInRange == 1)
                {
                    if (TraitGainFixFeatures.GrantTrait(pawn, naturalMood, 1, changed))
                    {
                        TraitGainFixFeatures.SendTraitLetter(
                            pawn,
                            "TraitGoodMoodA".Translate() + ", " + pawn.LabelShort + " " + "TraitGoodMoodB".Translate(),
                            LetterDefOf.PositiveEvent);
                    }
                }
                else
                {
                    moods[id] = 0;
                }
            }
        }

        void MakeHarder(Pawn pawn, TraitDef hardened, List<TraitDef> changed)
        {
            TraitDef naturalMood = DefDatabase<TraitDef>.GetNamedSilentFail("NaturalMood");
            TraitGainFixFeatures.RemoveTrait(pawn, naturalMood, changed);
            List<TraitDef> conflicts = new List<TraitDef>();
            List<Trait> traits = pawn.story.traits.allTraits;
            for (int i = 0; i < traits.Count; i++)
            {
                TraitDef current = traits[i]?.def;
                if (current == null)
                {
                    continue;
                }

                bool conflict = (hardened.conflictingTraits != null && hardened.conflictingTraits.Contains(current)) ||
                                (current.conflictingTraits != null && current.conflictingTraits.Contains(hardened));
                if (conflict)
                {
                    conflicts.Add(current);
                }
            }

            for (int i = 0; i < conflicts.Count; i++)
            {
                TraitGainFixFeatures.RemoveTrait(pawn, conflicts[i], changed);
                GrantHardenedBonus(pawn, changed);
            }

            if (pawn.story.traits.allTraits.Count < cachedMaxTraitCount - 1)
            {
                GrantHardenedBonus(pawn, changed);
            }

            if (TraitGainFixFeatures.DegreeExists(hardened, 0) && !pawn.story.traits.HasTrait(hardened))
            {
                pawn.story.traits.GainTrait(new Trait(hardened));
                if (pawn.story.traits.HasTrait(hardened))
                {
                    changed.Add(hardened);
                    TraitGainFixFeatures.RefreshAfterGrant(pawn);
                }
            }

            TraitDef extra = ExtraPersonality(pawn);
            if (extra != null)
            {
                TraitGainFixFeatures.GrantTrait(pawn, extra, 0, changed);
            }

            if (pawn.story.traits.HasTrait(hardened))
            {
                TraitGainFixFeatures.SendTraitLetter(
                    pawn,
                    pawn.LabelShort + " " + "TraitHardened".Translate(),
                    LetterDefOf.PositiveEvent);
            }
        }

        void GrantHardenedBonus(Pawn pawn, List<TraitDef> changed)
        {
            foreach ((string defName, int degree) bonus in HardenedBonus.InRandomOrder())
            {
                TraitDef trait = DefDatabase<TraitDef>.GetNamedSilentFail(bonus.defName);
                if (trait != null && TraitGainFixFeatures.GrantTrait(pawn, trait, bonus.degree, changed))
                {
                    break;
                }
            }
        }

        TraitDef ExtraPersonality(Pawn pawn)
        {
            if (pawn?.story?.traits?.allTraits == null || pawn.story.traits.allTraits.Count > cachedMaxTraitCount)
            {
                return null;
            }

            float roll = Rand.Value;
            TraitDef leader = DefDatabase<TraitDef>.GetNamedSilentFail("Leader");
            TraitDef psychopath = DefDatabase<TraitDef>.GetNamedSilentFail("Psychopath");
            TraitDef villian = DefDatabase<TraitDef>.GetNamedSilentFail("Villian");
            if (roll <= ExtraTraitChance && TraitGainFixFeatures.SuitableTrait(pawn, leader))
            {
                return leader;
            }

            if (roll + 0.05f <= ExtraTraitChance && TraitGainFixFeatures.SuitableTrait(pawn, psychopath))
            {
                return psychopath;
            }

            if (roll + 0.1f <= ExtraTraitChance && TraitGainFixFeatures.SuitableTrait(pawn, villian))
            {
                return villian;
            }

            return null;
        }

        static void ApplyPassions(Pawn pawn, List<TraitDef> changed)
        {
            if (changed == null || changed.Count == 0)
            {
                return;
            }

            List<SkillDef> skills = new List<SkillDef>();
            TraitGainFixFeatures.CollectChangedSkills(changed, skills);
            TraitGainFixFeatures.PassionUpdate(pawn, skills);
        }

        static bool HasDegree(Pawn pawn, TraitDef trait, int degree)
        {
            if (pawn?.story?.traits == null || trait == null || !pawn.story.traits.HasTrait(trait))
            {
                return false;
            }

            Trait current = pawn.story.traits.GetTrait(trait);
            return current != null && current.Degree == degree;
        }

        void EnsureCollections()
        {
            if (idList == null)
            {
                idList = new List<int>();
            }

            if (moods == null)
            {
                moods = new Dictionary<int, int>();
            }

            if (cannibal == null)
            {
                cannibal = new Dictionary<int, int>();
            }

            if (negativeTime == null)
            {
                negativeTime = new Dictionary<int, int>();
            }
        }

        static void EnsureCounter(Dictionary<int, int> counters, int id)
        {
            if (!counters.ContainsKey(id))
            {
                counters.Add(id, 0);
            }
        }
    }

    /// <summary>
    /// Storyteller incident TraitChange. Adds one allowed trait to a random colonist who is
    /// still under the trait cap.
    ///
    /// Инцидент рассказчика TraitChange. Добавляет одну допустимую черту случайному поселенцу,
    /// который ещё ниже предела черт.
    /// </summary>
    public class TraitGainChangeWorker : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return KebabTweaksSettings.IsTraitGainFixEnabled() && !FixSymptomProbe.IsTraitGainFixLeftover();
        }

        /// <summary>
        /// Picks one valid trait and degree for a random colonist. An empty colony still counts
        /// as a finished incident.
        ///
        /// Выбирает одну допустимую черту и степень случайному поселенцу. Пустая колония всё
        /// равно считается завершённым инцидентом.
        /// </summary>
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Pawn pawn = TraitGainFixFeatures.RandomColonist(parms?.target);
            if (pawn == null)
            {
                return true;
            }

            if (pawn.story.traits.allTraits.Count >= TraitGainFixFeatures.MaxTraitCount())
            {
                return true;
            }

            List<TraitDef> valid = TraitGainFixFeatures.ValidRandomTraits(pawn);
            if (valid.Count == 0)
            {
                return true;
            }

            TraitDef trait = valid[new IntRange(0, valid.Count - 1).RandomInRange];
            if (trait.defName == "DislikesMen" && pawn.gender == Gender.Male)
            {
                TraitDef women = DefDatabase<TraitDef>.GetNamedSilentFail("DislikesWomen");
                if (women != null)
                {
                    trait = women;
                }
            }

            int degree = 0;
            string degreeLabel = trait.label;
            if (!trait.degreeDatas.NullOrEmpty())
            {
                TraitDegreeData data = trait.degreeDatas.RandomElement();
                if (data != null)
                {
                    degree = data.degree;
                    if (!data.label.NullOrEmpty())
                    {
                        degreeLabel = data.label;
                    }
                }
            }

            if (!TraitGainFixFeatures.DegreeExists(trait, degree))
            {
                return true;
            }

            TraitDef shooting = DefDatabase<TraitDef>.GetNamedSilentFail("ShootingAccuracy");
            TraitDef brawler = DefDatabase<TraitDef>.GetNamedSilentFail("Brawler");
            if (trait == shooting && brawler != null && pawn.story.traits.HasTrait(brawler))
            {
                return true;
            }

            if (pawn.story.traits.HasTrait(trait))
            {
                Trait current = pawn.story.traits.GetTrait(trait);
                if (current != null && current.Degree == degree)
                {
                    return true;
                }

                TraitGainFixFeatures.RemoveTrait(pawn, trait, null);
            }

            if (trait.defName == "Jealous" || trait.defName == "Greedy" || trait.defName == "Ascetic")
            {
                RemoveWealthTrait(pawn);
            }

            if (!TraitGainFixFeatures.GrantTrait(pawn, trait, degree, null))
            {
                return true;
            }

            TraitGainFixFeatures.SendTraitLetter(
                pawn,
                pawn.LabelShort + " " + "GainedTrait".Translate() + " " + degreeLabel + ".",
                LetterDefOf.NeutralEvent);
            return true;
        }

        static void RemoveWealthTrait(Pawn pawn)
        {
            List<Trait> traits = pawn.story.traits.allTraits;
            for (int i = 0; i < traits.Count; i++)
            {
                string name = traits[i]?.def?.defName;
                if (name == "Ascetic" || name == "Jealous" || name == "Greedy")
                {
                    TraitGainFixFeatures.RemoveTrait(pawn, traits[i].def, null);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Incident TraitRemove. The def keeps base chance 0, so the storyteller does not pick it.
    /// The worker has to exist because the def names a remove worker.
    ///
    /// Инцидент TraitRemove. У дефа шанс 0, поэтому рассказчик его не выбирает. Воркер должен
    /// существовать, потому что деф ссылается на воркер снятия черты.
    /// </summary>
    public class TraitGainRemoveWorker : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return KebabTweaksSettings.IsTraitGainFixEnabled() && !FixSymptomProbe.IsTraitGainFixLeftover();
        }

        /// <summary>
        /// Removes one valid trait from a random colonist when something fires TraitRemove directly.
        ///
        /// Снимает одну допустимую черту со случайного поселенца, если TraitRemove запущен напрямую.
        /// </summary>
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Pawn pawn = TraitGainFixFeatures.RandomColonist(parms?.target);
            if (pawn?.story?.traits?.allTraits == null || pawn.story.traits.allTraits.Count == 0)
            {
                return true;
            }

            int index = new IntRange(0, pawn.story.traits.allTraits.Count - 1).RandomInRange;
            Trait trait = pawn.story.traits.allTraits[index];
            if (trait?.def == null || !TraitGainFixFeatures.ValidRandomTraits(pawn).Contains(trait.def))
            {
                return true;
            }

            string label = trait.LabelCap;
            TraitGainFixFeatures.RemoveTrait(pawn, trait.def, null);
            TraitGainFixFeatures.SendTraitLetter(
                pawn,
                pawn.LabelShort + " " + "LostTrait".Translate() + " " + label + ".",
                LetterDefOf.NeutralEvent);
            return true;
        }
    }
}
