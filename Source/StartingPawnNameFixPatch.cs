using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Page_ConfigureStartingPawns.CanDoNext rejects founding when any pawn in
    /// startingAndOptionalPawns (selected and left behind) has Name.IsValid false.
    /// NameTriple.IsValid requires a non-empty First and Last. Culture rule packs such as
    /// NamerPersonCorunanEN can emit a single token (Russian-SK DefInjected uses mostly
    /// r_name-&gt;[WordTribal_EN]). NameTriple.FromString stores that token in Nick only;
    /// ResolveMissingPieces does not copy Nick into First or Last.
    ///
    /// Fix: After name generation, copy the non-empty piece into empty First and Last
    /// (and wrap a NameSingle as a triple). Hooks: FromString and GeneratePawnName while
    /// GameInitData exists, GiveAppropriateBioAndNameTo, NewGeneratedStartingPawn,
    /// CanDoNext Prefix on the full list. Prefix does not skip CanDoNext.
    ///
    /// Проблема: Page_ConfigureStartingPawns.CanDoNext не даёт основать колонию, если у любой
    /// пешки в startingAndOptionalPawns (выбранные и оставленные) Name.IsValid ложно.
    /// NameTriple.IsValid требует непустые First и Last. Rule pack вроде
    /// NamerPersonCorunanEN может выдать одно слово (русский DefInjected в основном
    /// r_name-&gt;[WordTribal_EN]). NameTriple.FromString кладёт его только в Nick;
    /// ResolveMissingPieces не копирует Nick в First и Last.
    ///
    /// Исправление: после генерации имени копирует непустую часть в пустые First и Last
    /// (NameSingle превращает в тройку). Хуки: FromString и GeneratePawnName пока есть
    /// GameInitData, GiveAppropriateBioAndNameTo, NewGeneratedStartingPawn, Prefix CanDoNext
    /// по всему списку. Prefix не пропускает CanDoNext.
    /// </summary>
    public static class StartingPawnNameFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                if (AccessTools.Method(typeof(NameTriple), "FromString") == null
                    || AccessTools.Method(typeof(PawnBioAndNameGenerator), "GiveAppropriateBioAndNameTo") == null
                    || AccessTools.Method(typeof(PawnBioAndNameGenerator), "GeneratePawnName") == null
                    || AccessTools.Method(typeof(StartingPawnUtility), "NewGeneratedStartingPawn") == null
                    || AccessTools.Method(typeof(Page_ConfigureStartingPawns), "CanDoNext") == null)
                {
                    Log.Message(
                        "[StartingPawnNameFixPatch] Name or starting-pawn APIs not found; patch skipped.");
                    return;
                }

                harmony.CreateClassProcessor(typeof(NameTriple_FromString_StartingPawnName_Patch)).Patch();
                harmony.CreateClassProcessor(typeof(PawnBioAndNameGenerator_GeneratePawnName_StartingPawnName_Patch))
                    .Patch();
                harmony.CreateClassProcessor(
                    typeof(PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo_StartingPawnName_Patch)).Patch();
                harmony.CreateClassProcessor(typeof(StartingPawnUtility_NewGeneratedStartingPawn_StartingPawnName_Patch))
                    .Patch();
                harmony.CreateClassProcessor(typeof(Page_ConfigureStartingPawns_CanDoNext_StartingPawnName_Patch))
                    .Patch();
                Log.Message("[StartingPawnNameFixPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[StartingPawnNameFixPatch] Failed to apply patches: " + e);
            }
        }

        internal static bool InNewGameNameGen()
        {
            return KebabTweaksSettings.EnableStartingPawnNameFix && Find.GameInitData != null;
        }

        /// <summary>
        /// Fills empty First/Last on an invalid NameTriple from the remaining piece, or wraps
        /// an invalid NameSingle as First/Nick/Last.
        ///
        /// Для невалидного NameTriple заполняет пустые First/Last из оставшейся части; невалидный
        /// NameSingle оборачивает как First/Nick/Last.
        /// </summary>
        internal static Name CoerceName(Name name)
        {
            if (name == null || name.IsValid)
            {
                return name;
            }

            NameTriple triple = name as NameTriple;
            if (triple != null)
            {
                return CoerceTriple(triple);
            }

            NameSingle single = name as NameSingle;
            if (single != null && !single.Name.NullOrEmpty())
            {
                return new NameTriple(single.Name, single.Name, single.Name);
            }

            return name;
        }

        internal static void EnsureValidName(Pawn pawn)
        {
            if (!KebabTweaksSettings.EnableStartingPawnNameFix || pawn == null)
            {
                return;
            }

            Name coerced = CoerceName(pawn.Name);
            if (!ReferenceEquals(coerced, pawn.Name))
            {
                pawn.Name = coerced;
            }
        }

        internal static void EnsureStartingAndOptionalPawnNames()
        {
            if (!KebabTweaksSettings.EnableStartingPawnNameFix)
            {
                return;
            }

            var data = Find.GameInitData;
            if (data == null || data.startingAndOptionalPawns == null)
            {
                return;
            }

            for (int i = 0; i < data.startingAndOptionalPawns.Count; i++)
            {
                EnsureValidName(data.startingAndOptionalPawns[i]);
            }
        }

        private static Name CoerceTriple(NameTriple triple)
        {
            string first = triple.First;
            string nick = triple.Nick;
            string last = triple.Last;
            string seed = null;
            if (!first.NullOrEmpty())
            {
                seed = first;
            }
            else if (!nick.NullOrEmpty())
            {
                seed = nick;
            }
            else if (!last.NullOrEmpty())
            {
                seed = last;
            }

            if (seed.NullOrEmpty())
            {
                return triple;
            }

            if (first.NullOrEmpty())
            {
                first = seed;
            }

            if (last.NullOrEmpty())
            {
                last = seed;
            }

            if (nick.NullOrEmpty())
            {
                nick = seed;
            }

            return new NameTriple(first, nick, last);
        }
    }

    /// <summary>
    /// Coerces NameTriple.FromString during new-game init so single-token namers become valid.
    ///
    /// Приводит NameTriple.FromString во время нового сценария, чтобы однословные имена стали валидными.
    /// </summary>
    [HarmonyPatch(typeof(NameTriple), "FromString")]
    public static class NameTriple_FromString_StartingPawnName_Patch
    {
        public static void Postfix(ref NameTriple __result)
        {
            if (!StartingPawnNameFixFeatures.InNewGameNameGen() || __result == null)
            {
                return;
            }

            __result = StartingPawnNameFixFeatures.CoerceName(__result) as NameTriple ?? __result;
        }
    }

    /// <summary>
    /// Coerces GeneratePawnName results during new-game init.
    ///
    /// Приводит результат GeneratePawnName во время нового сценария.
    /// </summary>
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "GeneratePawnName")]
    public static class PawnBioAndNameGenerator_GeneratePawnName_StartingPawnName_Patch
    {
        public static void Postfix(ref Name __result)
        {
            if (!StartingPawnNameFixFeatures.InNewGameNameGen())
            {
                return;
            }

            __result = StartingPawnNameFixFeatures.CoerceName(__result);
        }
    }

    /// <summary>
    /// Coerces the name GiveAppropriateBioAndNameTo assigned.
    ///
    /// Приводит имя, которое выставил GiveAppropriateBioAndNameTo.
    /// </summary>
    [HarmonyPatch(typeof(PawnBioAndNameGenerator), "GiveAppropriateBioAndNameTo")]
    public static class PawnBioAndNameGenerator_GiveAppropriateBioAndNameTo_StartingPawnName_Patch
    {
        public static void Postfix(Pawn pawn)
        {
            if (!StartingPawnNameFixFeatures.InNewGameNameGen())
            {
                return;
            }

            StartingPawnNameFixFeatures.EnsureValidName(pawn);
        }
    }

    /// <summary>
    /// Coerces names on the first starting-pawn fill and on RandomizePawn / RandomizeInPlace rerolls.
    ///
    /// Приводит имена при первом наборе стартовых пешек и при перегенерации RandomizePawn / RandomizeInPlace.
    /// </summary>
    [HarmonyPatch(typeof(StartingPawnUtility), "NewGeneratedStartingPawn")]
    public static class StartingPawnUtility_NewGeneratedStartingPawn_StartingPawnName_Patch
    {
        public static void Postfix(Pawn __result)
        {
            StartingPawnNameFixFeatures.EnsureValidName(__result);
        }
    }

    /// <summary>
    /// Repairs selected and left-behind starting pawns before EveryoneNeedsValidName.
    ///
    /// Чинит выбранных и оставленных стартовых пешек до проверки EveryoneNeedsValidName.
    /// </summary>
    [HarmonyPatch(typeof(Page_ConfigureStartingPawns), "CanDoNext")]
    public static class Page_ConfigureStartingPawns_CanDoNext_StartingPawnName_Patch
    {
        public static void Prefix()
        {
            StartingPawnNameFixFeatures.EnsureStartingAndOptionalPawnNames();
        }
    }
}
