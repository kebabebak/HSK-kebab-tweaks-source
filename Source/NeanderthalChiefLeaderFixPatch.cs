using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Tribal faction leaders (often Tribal_ChiefMelee_Neanderthal) fail PawnGenerator
    /// when a new world is created. Combat Extended injects a Melee skill floor onto
    /// Tribal_ChiefMelee; abstract TribalChiefBase also uses that defName, so Neanderthal chiefs
    /// inherit the floor. Pacifist Tribal backstories (vanilla SoleSurvivor21 / ShamanOfShadows47
    /// and extra Cybranian Backstories+ entries) disable Violent while chiefs require that work
    /// tag. After 120 tries GeneratePawn returns null and Faction.TryGenerateNewLeader throws
    /// NullReferenceException on leader.RaceProps, which aborts WorldGenStep_Factions.
    ///
    /// Fix: XML removes inherited Combat Extended skills on Neanderthal chief kinds and moves
    /// Violent-disabled Tribal backstories to TribalPeaceful (PlayerTribe still uses those
    /// stories). Postfix retries leader generation with mustBeCapableOfViolence when leader is
    /// still null. Finalizer swallows NullReferenceException so world creation can continue.
    /// Prefix skip is not used: the original TryGenerateNewLeader must still run.
    ///
    /// Проблема: Лидеры племенных фракций (часто Tribal_ChiefMelee_Neanderthal) падают в
    /// PawnGenerator при создании мира. Combat Extended вешает порог навыка Melee на
    /// Tribal_ChiefMelee; абстрактный TribalChiefBase сам несёт этот defName, поэтому
    /// неандертальские вожди наследуют порог. Пацифистские Tribal-бэкстори (vanilla
    /// SoleSurvivor21 / ShamanOfShadows47 и записи Cybranian Backstories+) отключают Violent,
    /// хотя вождю тег нужен. После 120 попыток GeneratePawn даёт null, TryGenerateNewLeader
    /// бросает NullReferenceException на leader.RaceProps и обрывает WorldGenStep_Factions.
    ///
    /// Исправление: XML снимает унаследованные skills Combat Extended у неандертальских вождей
    /// и переносит Tribal-бэкстори с отключённым Violent в TribalPeaceful (у PlayerTribe
    /// истории остаются). Postfix повторяет генерацию лидера с mustBeCapableOfViolence, если
    /// leader всё ещё null. Finalizer глотает NullReferenceException, чтобы создание мира
    /// продолжилось. Prefix skip не используется: исходный TryGenerateNewLeader должен выполняться.
    /// </summary>
    public static class NeanderthalChiefLeaderFixFeatures
    {
        /// <summary>
        /// Hooks Faction.TryGenerateNewLeader postfix retry and NullReferenceException finalizer.
        ///
        /// Вешает postfix-повтор и finalizer NullReferenceException на Faction.TryGenerateNewLeader.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Faction), nameof(Faction.TryGenerateNewLeader)),
                postfix: new HarmonyMethod(
                    typeof(NeanderthalChiefLeaderFixFeatures),
                    nameof(TryGenerateNewLeader_Postfix)),
                finalizer: new HarmonyMethod(
                    typeof(NeanderthalChiefLeaderFixFeatures),
                    nameof(TryGenerateNewLeader_Finalizer)));
        }

        private static bool _loggedNre;
        private static bool _loggedRetry;

        /// <summary>
        /// If vanilla left the faction without a leader, generate one that must be capable of violence.
        /// Postfix cannot recover a NullReferenceException by itself; the finalizer must swallow it first.
        ///
        /// Если vanilla оставила фракцию без лидера, создать пешку, способную к насилию.
        /// Postfix сам не перехватывает NullReferenceException; сначала finalizer должен её проглотить.
        /// </summary>
        private static void TryGenerateNewLeader_Postfix(Faction __instance, ref bool __result)
        {
            if (!KebabTweaksSettings.EnableNeanderthalChiefLeaderFix)
            {
                return;
            }

            if (__instance == null || __instance.leader != null || !FactionShouldHaveLeader(__instance))
            {
                return;
            }

            Pawn pawn = TryGenerateLeaderPawn(__instance);
            if (pawn == null)
            {
                return;
            }

            AttachLeader(__instance, pawn);
            __result = true;
            if (!_loggedRetry)
            {
                _loggedRetry = true;
                Log.Message(
                    "[HSK kebab tweaks] Tribal leader generation retry succeeded after "
                    + "TryGenerateNewLeader left the faction without a leader.");
            }
        }

        /// <summary>
        /// Swallows NullReferenceException when GeneratePawn returned null and the original method
        /// read leader.RaceProps. Without this, postfix retry never runs and WorldGenStep_Factions aborts.
        ///
        /// Глотает NullReferenceException, когда GeneratePawn вернул null и исходный метод читает
        /// leader.RaceProps. Без этого postfix-повтор не выполняется и WorldGenStep_Factions обрывается.
        /// </summary>
        private static Exception TryGenerateNewLeader_Finalizer(Exception __exception)
        {
            if (!KebabTweaksSettings.EnableNeanderthalChiefLeaderFix)
            {
                return __exception;
            }

            if (__exception is NullReferenceException)
            {
                if (!_loggedNre)
                {
                    _loggedNre = true;
                    Log.Warning(
                        "[HSK kebab tweaks] Tribal leader fix swallowed NullReferenceException "
                        + "in Faction.TryGenerateNewLeader (leader pawn generation returned null).");
                }

                return null;
            }

            return __exception;
        }

        /// <summary>
        /// Reads Faction.ShouldHaveLeader by name so compile refs that omit the property still build.
        /// Fallback: humanlike non-player factions.
        ///
        /// Читает Faction.ShouldHaveLeader по имени, чтобы сборка шла, если свойства нет в compile refs.
        /// Запасной путь: человекоподобные неигровые фракции.
        /// </summary>
        private static bool FactionShouldHaveLeader(Faction faction)
        {
            var getter = AccessTools.PropertyGetter(typeof(Faction), "ShouldHaveLeader");
            if (getter != null)
            {
                return (bool)getter.Invoke(faction, null);
            }

            return faction.def != null && faction.def.humanlikeFaction && !faction.IsPlayer;
        }

        /// <summary>
        /// Tries factionLeader kinds from pawn group makers and fixedLeaderKinds, then RandomPawnKind.
        ///
        /// Сначала kinds с factionLeader из pawn group makers и fixedLeaderKinds, затем RandomPawnKind.
        /// </summary>
        private static Pawn TryGenerateLeaderPawn(Faction faction)
        {
            List<PawnKindDef> kinds = CollectLeaderKinds(faction);
            for (int i = 0; i < kinds.Count; i++)
            {
                Pawn pawn = TryGenerate(faction, kinds[i]);
                if (pawn != null)
                {
                    return pawn;
                }
            }

            PawnKindDef fallback = faction.RandomPawnKind();
            if (fallback == null || kinds.Contains(fallback))
            {
                return null;
            }

            return TryGenerate(faction, fallback);
        }

        /// <summary>
        /// Collects unique leader PawnKindDef entries from the faction def.
        ///
        /// Собирает уникальные лидерские PawnKindDef из def фракции.
        /// </summary>
        private static List<PawnKindDef> CollectLeaderKinds(Faction faction)
        {
            List<PawnKindDef> list = new List<PawnKindDef>();
            if (faction?.def == null)
            {
                return list;
            }

            if (faction.def.pawnGroupMakers != null)
            {
                for (int i = 0; i < faction.def.pawnGroupMakers.Count; i++)
                {
                    PawnGroupMaker maker = faction.def.pawnGroupMakers[i];
                    if (maker?.options == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < maker.options.Count; j++)
                    {
                        PawnKindDef kind = maker.options[j]?.kind;
                        if (kind != null && kind.factionLeader && !list.Contains(kind))
                        {
                            list.Add(kind);
                        }
                    }
                }
            }

            if (faction.def.fixedLeaderKinds != null)
            {
                for (int i = 0; i < faction.def.fixedLeaderKinds.Count; i++)
                {
                    PawnKindDef kind = faction.def.fixedLeaderKinds[i];
                    if (kind != null && !list.Contains(kind))
                    {
                        list.Add(kind);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Generates a leader pawn that must be capable of violence. Failures return null.
        ///
        /// Генерирует лидера, способного к насилию. При сбое возвращает null.
        /// </summary>
        private static Pawn TryGenerate(Faction faction, PawnKindDef kind)
        {
            if (kind == null)
            {
                return null;
            }

            try
            {
#if RIMWORLD_1_6
                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind,
                    faction,
                    PawnGenerationContext.NonPlayer,
                    forceGenerateNewPawn: true,
                    canGeneratePawnRelations: true,
                    mustBeCapableOfViolence: true);
#else
                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind,
                    faction,
                    PawnGenerationContext.NonPlayer,
                    -1,
                    true,
                    false,
                    false,
                    true,
                    true);
#endif
                if (faction.ideos?.PrimaryIdeo != null)
                {
                    Gender supreme = faction.ideos.PrimaryIdeo.SupremeGender;
                    if (supreme != Gender.None)
                    {
                        request.FixedGender = supreme;
                    }
                }

                return PawnGenerator.GeneratePawn(request);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Assigns the pawn as faction leader and passes it to WorldPawns when needed.
        ///
        /// Назначает пешку лидером фракции и при необходимости передаёт её в WorldPawns.
        /// </summary>
        private static void AttachLeader(Faction faction, Pawn pawn)
        {
            faction.leader = pawn;
            if (pawn.RaceProps != null && pawn.RaceProps.IsFlesh && pawn.relations != null)
            {
                pawn.relations.everSeenByPlayer = true;
            }

            if (Find.WorldPawns != null && !Find.WorldPawns.Contains(pawn))
            {
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
            }
        }
    }
}
