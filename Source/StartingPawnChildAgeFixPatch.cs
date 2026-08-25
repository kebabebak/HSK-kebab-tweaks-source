using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Biotech starting-pawn Child or Baby sets AllowedDevelopmentalStages to that
    /// stage and regenerates the pawn. HSK player kinds inherit minGenerationAge 18
    /// (SK_BasePlayerPawnKind / Core_SK_CoreModule BasePlayerPawnKind). Racial kinds often
    /// use 13+. Child life stages are about ages 2-13, so PawnGenerator.GenerateRandomAge
    /// AgeAllowed never succeeds. After 300 rolls it logs Tried 300 times to generate age for
    /// and leaves a mismatched age.
    ///
    /// Fix: When the request is Baby/Child without Adult and no age can satisfy both the
    /// requested stages and the kind min/max age, Prefix sets FixedBiologicalAge from the
    /// race lifeStageAges table. The Prefix does not skip, so the original GenerateRandomAge
    /// still runs (chronological age, cryptosleep, AgeReversalDemand).
    ///
    /// Проблема: кнопка Child или Baby на стартовых пешках Biotech ставит
    /// AllowedDevelopmentalStages и перегенерирует пешку. Игровые kind HSK наследуют
    /// minGenerationAge 18 (SK_BasePlayerPawnKind / Core_SK_CoreModule BasePlayerPawnKind).
    /// У расовых kind часто 13+. Детские life stages это примерно 2-13 лет, поэтому
    /// AgeAllowed в PawnGenerator.GenerateRandomAge никогда не проходит. После 300 бросков
    /// пишется Tried 300 times to generate age for, возраст остаётся неверным.
    ///
    /// Исправление: если запрос Baby/Child без Adult и ни один возраст не проходит и стадии,
    /// и min/max kind, Prefix ставит FixedBiologicalAge из таблицы lifeStageAges расы.
    /// Prefix не пропускает оригинал, поэтому исходный GenerateRandomAge всё равно выполняется
    /// (хронологический возраст, криптосон, AgeReversalDemand).
    /// </summary>
    public static class StartingPawnChildAgeFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                if (AccessTools.Method(
                        typeof(PawnGenerator),
                        "GenerateRandomAge",
                        new[] { typeof(Pawn), typeof(PawnGenerationRequest) }) == null)
                {
                    Log.Message(
                        "[StartingPawnChildAgeFixPatch] PawnGenerator.GenerateRandomAge not found; patch skipped.");
                    return;
                }

                harmony.CreateClassProcessor(typeof(PawnGenerator_GenerateRandomAge_ChildAge_Patch)).Patch();
                Log.Message("[StartingPawnChildAgeFixPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[StartingPawnChildAgeFixPatch] Failed to apply patches: " + e);
            }
        }
    }

    /// <summary>
    /// Sets FixedBiologicalAge for Baby/Child-only requests whose kind minGenerationAge has no
    /// overlap with the race child/baby life stages. Prefix does not skip the original so
    /// chronological age, cryptosleep, and AgeReversalDemand still run.
    ///
    /// Ставит FixedBiologicalAge для запросов только Baby/Child, если minGenerationAge kind
    /// не пересекается с детскими life stages расы. Prefix не пропускает оригинал: хронологический
    /// возраст, криптосон и AgeReversalDemand по-прежнему выполняются.
    /// </summary>
    [HarmonyPatch(typeof(PawnGenerator), "GenerateRandomAge")]
    [HarmonyAfter("erdelf.HumanoidAlienRaces")]
    [HarmonyPriority(Priority.Last)]
    public static class PawnGenerator_GenerateRandomAge_ChildAge_Patch
    {
        public static void Prefix(Pawn pawn, ref PawnGenerationRequest request)
        {
            if (!KebabTweaksSettings.EnableStartingPawnChildAgeFix)
            {
                return;
            }

            if (pawn == null || pawn.RaceProps == null || pawn.RaceProps.lifeStageAges == null)
            {
                return;
            }

            if (request.FixedBiologicalAge.HasValue)
            {
                return;
            }

            DevelopmentalStage stages = request.AllowedDevelopmentalStages;
            if (stages == DevelopmentalStage.None || stages.Newborn() || stages.Adult())
            {
                return;
            }

            if (!stages.Baby() && !stages.Child())
            {
                return;
            }

            if (KindAgeLimitsAllowRequestedStages(pawn, request))
            {
                return;
            }

            if (!TryChooseAgeYears(pawn, request, out float years))
            {
                return;
            }

            request.FixedBiologicalAge = years;
        }

        /// <summary>
        /// True when at least one requested life-stage age also sits inside the kind
        /// minGenerationAge / maxGenerationAge window (vanilla AgeAllowed can succeed).
        ///
        /// True, если хоть один возраст запрошенной стадии попадает в окно minGenerationAge /
        /// maxGenerationAge kind (ванильный AgeAllowed может пройти).
        /// </summary>
        private static bool KindAgeLimitsAllowRequestedStages(Pawn pawn, PawnGenerationRequest request)
        {
            List<LifeStageAge> ages = pawn.RaceProps.lifeStageAges;
            for (int i = 0; i < ages.Count; i++)
            {
                if (TryGetStageAgeRange(pawn, request, i, ignoreKindAgeLimits: false, out _, out _))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Picks a random age inside a requested Baby/Child life stage, ignoring kind
        /// minGenerationAge / maxGenerationAge. Respects BiologicalAgeRange and
        /// ExcludeBiologicalAgeRange when those are set.
        ///
        /// Берёт случайный возраст внутри запрошенной стадии Baby/Child, игнорируя
        /// minGenerationAge / maxGenerationAge kind. Учитывает BiologicalAgeRange и
        /// ExcludeBiologicalAgeRange, если они заданы.
        /// </summary>
        private static bool TryChooseAgeYears(Pawn pawn, PawnGenerationRequest request, out float years)
        {
            years = 0f;
            List<LifeStageAge> ages = pawn.RaceProps.lifeStageAges;
            int matchCount = 0;
            for (int i = 0; i < ages.Count; i++)
            {
                if (TryGetStageAgeRange(pawn, request, i, ignoreKindAgeLimits: true, out _, out _))
                {
                    matchCount++;
                }
            }

            if (matchCount == 0)
            {
                return false;
            }

            int pick = Rand.Range(0, matchCount);
            for (int i = 0; i < ages.Count; i++)
            {
                if (!TryGetStageAgeRange(pawn, request, i, ignoreKindAgeLimits: true, out float min, out float max))
                {
                    continue;
                }

                if (pick != 0)
                {
                    pick--;
                    continue;
                }

                float spanMax = Math.Max(min + 0.05f, max - 0.05f);
                for (int attempt = 0; attempt < 8; attempt++)
                {
                    years = Rand.Range(min, spanMax);
                    if (!request.ExcludeBiologicalAgeRange.HasValue
                        || !request.ExcludeBiologicalAgeRange.Value.Includes(years))
                    {
                        return true;
                    }
                }

                years = min;
                return true;
            }

            return false;
        }

        private static bool TryGetStageAgeRange(
            Pawn pawn,
            PawnGenerationRequest request,
            int index,
            bool ignoreKindAgeLimits,
            out float min,
            out float max)
        {
            min = 0f;
            max = 0f;
            List<LifeStageAge> ages = pawn.RaceProps.lifeStageAges;
            LifeStageAge stageAge = ages[index];
            if (stageAge == null || stageAge.def == null)
            {
                return false;
            }

            if (!request.AllowedDevelopmentalStages.Has(stageAge.def.developmentalStage))
            {
                return false;
            }

            min = stageAge.minAge;
            max = (index + 1 < ages.Count) ? ages[index + 1].minAge : pawn.RaceProps.lifeExpectancy;
            if (max <= min)
            {
                max = min + 1f;
            }

            if (!ignoreKindAgeLimits)
            {
                PawnKindDef kind = pawn.kindDef ?? request.KindDef;
                if (kind != null)
                {
                    min = Math.Max(min, kind.minGenerationAge);
                    max = Math.Min(max, kind.maxGenerationAge);
                }
            }

            if (request.BiologicalAgeRange.HasValue)
            {
                FloatRange range = request.BiologicalAgeRange.Value;
                min = Math.Max(min, range.min);
                max = Math.Min(max, range.max);
            }

            if (request.ExcludeBiologicalAgeRange.HasValue)
            {
                FloatRange exclude = request.ExcludeBiologicalAgeRange.Value;
                if (exclude.min <= min && exclude.max >= max)
                {
                    return false;
                }
            }

            return min < max;
        }
    }
}
