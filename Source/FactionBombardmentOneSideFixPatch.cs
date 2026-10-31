using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Factional War shelling and contention sites spawn a combat group and still
    /// create a lord when that group is empty. TryGetRandomPawnGroupMaker keeps a group whose
    /// cheapest kind fits the point total. MaxPawnCost is not part of that test, so
    /// ChoosePawnGenOptionsByPoints can reject every kind and GeneratePawns returns none.
    /// IncidentWorkerFactionWar.ResolvePawnList drops that empty list.
    /// SiteFactionWarShelling.PostMapGenerate and SiteFactionWarContention.PostMapGenerate do
    /// not, so the empty side still gets LordJobShellFactionFirst or LordJobFactionContention.
    /// TriggerAllHostileFactionMembersDead is true when that faction has no living spawned
    /// pawn, and the side that did spawn takes SrClearBattlefiled and leaves.
    ///
    /// Fix: Postfix on the mod GeneratePawns. Harmony binds that argument by name:
    /// pawnGroupMakerParms. A parameter named parms throws while the patch is applied and
    /// neither hook runs. An empty list is replaced from another group of the same kind that
    /// CanGenerateFrom and has a kind at or under MaxPawnCost and the point total.
    /// generateFightersOnly still requires a fighter. The replacement calls
    /// PawnGroupMaker.GeneratePawns with errorOnZeroResults false. The weighted payable group
    /// is tried first, then the others. The list stays empty when none return a pawn. The
    /// first vanilla roll can still log its own empty-group error. Postfix on both sites'
    /// PostMapGenerate keeps the lord pair only when both lords own pawns. Otherwise both
    /// lords are removed and their living pawns are despawned and passed to the world, before
    /// the lord ticks, so SrClearBattlefiled does not fire. A player pawn in one of these
    /// lords skips the removal. The player caravan is a different lord. The one-faction
    /// defense site is not patched. Soft skip when Factional War is absent. Leftover when
    /// Factional War is loaded and SK.FactionalWarPawnGroupPatches.FillEmptyGroup takes
    /// pawnGroupMakerParms and calls PawnGroupMakerUtility.MaxPawnCost, and a method on that
    /// type calls LordManager.RemoveLord. Factional War absent stays current.
    ///
    /// Проблема: точки обстрела и схватки Factional War создают боевую группу и всё равно
    /// дают ей лорда, когда группа пустая. TryGetRandomPawnGroupMaker оставляет группу, чей
    /// самый дешёвый вид влезает в очки. MaxPawnCost в этой проверке нет, поэтому
    /// ChoosePawnGenOptionsByPoints может отвергнуть все виды, и GeneratePawns вернёт пусто.
    /// IncidentWorkerFactionWar.ResolvePawnList такой список отбрасывает.
    /// SiteFactionWarShelling.PostMapGenerate и SiteFactionWarContention.PostMapGenerate нет,
    /// поэтому пустая сторона всё равно получает LordJobShellFactionFirst или
    /// LordJobFactionContention. TriggerAllHostileFactionMembersDead истинен, когда у фракции
    /// нет живой пешки на карте, и появившаяся сторона берёт SrClearBattlefiled и уходит.
    ///
    /// Исправление: Postfix на GeneratePawns мода. Harmony связывает аргумент по имени:
    /// pawnGroupMakerParms. Параметр parms бросает исключение при наложении патча, и оба
    /// хука не встают. Пустой список заменяется другой группой того же вида, которая
    /// CanGenerateFrom и имеет вид не дороже MaxPawnCost и суммы очков. generateFightersOnly
    /// по-прежнему требует бойца. Замена зовёт PawnGroupMaker.GeneratePawns с
    /// errorOnZeroResults false. Сначала группа с весом, потом остальные. Список остаётся
    /// пустым, если никто не вернул пешку. Первый ванильный бросок всё ещё может записать
    /// свою ошибку пустой группы. Postfix на PostMapGenerate обеих точек оставляет пару
    /// лордов только когда у обоих есть пешки. Иначе оба лорда снимаются, их живые пешки
    /// деспавнятся и уходят в мир до тика лорда, поэтому SrClearBattlefiled не срабатывает.
    /// Пешка игрока в одном из этих лордов отменяет снятие. Караван игрока - другой лорд.
    /// Точка обороны одной фракции не патчится. Soft skip, если Factional War нет. Leftover,
    /// если Factional War загружен и SK.FactionalWarPawnGroupPatches.FillEmptyGroup принимает
    /// pawnGroupMakerParms и зовёт PawnGroupMakerUtility.MaxPawnCost, а метод этого типа зовёт
    /// LordManager.RemoveLord. Без Factional War фикс остаётся актуальным.
    /// </summary>
    public static class FactionBombardmentOneSideFixFeatures
    {
        const string UtilityTypeName = "SR.ModRimWorld.FactionalWar.PawnGroupMakerUtility";
        const string ShellingSite = "SR.ModRimWorld.FactionalWar.SiteFactionWarShelling";
        const string ContentionSite = "SR.ModRimWorld.FactionalWar.SiteFactionWarContention";
        const string ShellingLordJob = "SR.ModRimWorld.FactionalWar.LordJobShellFactionFirst";
        const string ContentionLordJob = "SR.ModRimWorld.FactionalWar.LordJobFactionContention";

        public static void Apply(Harmony harmony)
        {
            try
            {
                Type utility = AccessTools.TypeByName(UtilityTypeName);
                if (utility == null)
                {
                    FeatureAvailability.SkipMissingMods(
                        FeatureAvailability.FactionBombardment,
                        "Faction Bombardment One Side",
                        "Factional War (SR.ModRimworld.FactionalWarContinued)");
                    return;
                }

                int patched = 0;
                MethodInfo generatePawns = AccessTools.Method(
                    utility,
                    "GeneratePawns",
                    new[] { typeof(PawnGroupMakerParms) });
                if (generatePawns != null && generatePawns.ReturnType == typeof(List<Pawn>))
                {
                    harmony.Patch(
                        generatePawns,
                        postfix: new HarmonyMethod(
                            typeof(FactionBombardmentOneSideFixFeatures),
                            nameof(FillEmptyGroup)));
                    patched++;
                }

                patched += PatchPostMapGenerate(harmony, ShellingSite);
                patched += PatchPostMapGenerate(harmony, ContentionSite);
                if (patched == 0)
                {
                    FeatureAvailability.SkipHandled(
                        FeatureAvailability.FactionBombardment,
                        "Faction Bombardment One Side",
                        "Factional War GeneratePawns and site PostMapGenerate were not found.");
                    return;
                }

                Log.Message("[FactionBombardmentOneSideFix] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[FactionBombardmentOneSideFix] Failed to apply patches: " + e);
            }
        }

        /// <summary>
        /// Replaces an empty mod GeneratePawns result from another payable group of the same kind.
        /// The argument name is pawnGroupMakerParms because Harmony binds it to that parameter.
        ///
        /// Заменяет пустой результат GeneratePawns мода другой оплачиваемой группой того же вида.
        /// Имя аргумента pawnGroupMakerParms, потому что Harmony связывает его с этим параметром.
        /// </summary>
        public static void FillEmptyGroup(PawnGroupMakerParms pawnGroupMakerParms, ref List<Pawn> __result)
        {
            if (!KebabTweaksSettings.IsFactionBombardmentOneSideFixEnabled())
            {
                return;
            }

            PawnGroupMakerParms parms = pawnGroupMakerParms;
            if (parms?.faction?.def?.pawnGroupMakers == null || parms.groupKind == null)
            {
                return;
            }

            if (__result != null && __result.Count > 0)
            {
                return;
            }

            float maxCost = PawnGroupMakerUtility.MaxPawnCost(
                parms.faction, parms.points, parms.raidStrategy, parms.groupKind);
            List<PawnGroupMaker> makers = parms.faction.def.pawnGroupMakers;
            List<PawnGroupMaker> payable = new List<PawnGroupMaker>();
            for (int i = 0; i < makers.Count; i++)
            {
                PawnGroupMaker maker = makers[i];
                if (maker == null || !maker.CanGenerateFrom(parms) || !HasPayableKind(maker, parms, maxCost))
                {
                    continue;
                }

                payable.Add(maker);
            }

            if (payable.Count == 0)
            {
                return;
            }

            PawnGroupMaker chosen = payable.RandomElementByWeight(maker => GroupWeight(parms, maker));
            if (TryTakeGroup(chosen, parms, ref __result))
            {
                return;
            }

            for (int i = 0; i < payable.Count; i++)
            {
                if (payable[i] == chosen)
                {
                    continue;
                }

                if (TryTakeGroup(payable[i], parms, ref __result))
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Removes a shelling or contention lord pair that is not two lords which both own pawns.
        ///
        /// Снимает пару лордов обстрела или схватки, если это не два лорда и у обоих есть пешки.
        /// </summary>
        public static void PostMapGenerate(MapParent __instance)
        {
            if (!KebabTweaksSettings.IsFactionBombardmentOneSideFixEnabled())
            {
                return;
            }

            if (__instance == null)
            {
                return;
            }

            string name = __instance.GetType().FullName;
            if (name == ShellingSite)
            {
                RemoveBrokenPair(__instance, ShellingLordJob);
            }
            else if (name == ContentionSite)
            {
                RemoveBrokenPair(__instance, ContentionLordJob);
            }
        }

        static int PatchPostMapGenerate(Harmony harmony, string typeName)
        {
            MethodInfo method = AccessTools.Method(AccessTools.TypeByName(typeName), "PostMapGenerate", Type.EmptyTypes);
            if (method == null)
            {
                return 0;
            }

            harmony.Patch(
                method,
                postfix: new HarmonyMethod(
                    typeof(FactionBombardmentOneSideFixFeatures),
                    nameof(PostMapGenerate)));
            return 1;
        }

        /// <summary>
        /// Weight of a replacement group. 1.5 PawnGroupMakerParms has no ignoreGroupCommonality.
        ///
        /// Вес заменяющей группы. У PawnGroupMakerParms в 1.5 нет ignoreGroupCommonality.
        /// </summary>
        static float GroupWeight(PawnGroupMakerParms parms, PawnGroupMaker maker)
        {
#if RIMWORLD_1_6
            if (parms.ignoreGroupCommonality)
            {
                return 1f;
            }
#endif
            return maker.commonality > 0f ? maker.commonality : 0.001f;
        }

        /// <summary>
        /// True when the maker has a kind this request can pay for.
        ///
        /// True, если у группы есть вид, который этот запрос может оплатить.
        /// </summary>
        static bool HasPayableKind(PawnGroupMaker maker, PawnGroupMakerParms parms, float maxCost)
        {
            List<PawnGenOption> options = maker.options;
            if (options == null)
            {
                return false;
            }

            for (int i = 0; i < options.Count; i++)
            {
                PawnGenOption option = options[i];
                PawnKindDef kind = option?.kind;
                if (kind == null)
                {
                    continue;
                }

                if (parms.generateFightersOnly && !kind.isFighter)
                {
                    continue;
                }

                float cost = option.Cost;
                if (cost > maxCost || cost > parms.points)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Puts a non-empty GeneratePawns result into the postfix list. errorOnZeroResults stays false.
        ///
        /// Кладёт непустой результат GeneratePawns в список postfix. errorOnZeroResults остаётся false.
        /// </summary>
        static bool TryTakeGroup(PawnGroupMaker maker, PawnGroupMakerParms parms, ref List<Pawn> __result)
        {
            if (maker == null)
            {
                return false;
            }

            IEnumerable<Pawn> generated = maker.GeneratePawns(parms, false);
            if (generated == null)
            {
                return false;
            }

            List<Pawn> pawns = generated.ToList();
            if (pawns.Count == 0)
            {
                return false;
            }

            __result = pawns;
            return true;
        }

        /// <summary>
        /// Removes the matched lords and passes their living non-player pawns to the world.
        /// RemoveLord clears ownedPawns, so the copy is what gets despawned.
        ///
        /// Снимает совпавшие лорды и отправляет их живых пешек не игрока в мир.
        /// RemoveLord очищает ownedPawns, поэтому деспавнится копия.
        /// </summary>
        static void RemoveBrokenPair(MapParent site, string lordJobTypeName)
        {
            Map map = site.Map;
            if (map?.lordManager?.lords == null)
            {
                return;
            }

            List<Lord> lords = map.lordManager.lords;
            List<Lord> matched = new List<Lord>();
            for (int i = 0; i < lords.Count; i++)
            {
                Lord lord = lords[i];
                if (lord?.LordJob != null && lord.LordJob.GetType().FullName == lordJobTypeName)
                {
                    matched.Add(lord);
                }
            }

            if (!PairIsBroken(matched))
            {
                return;
            }

            for (int i = 0; i < matched.Count; i++)
            {
                Lord lord = matched[i];
                List<Pawn> pawns = lord.ownedPawns == null ? new List<Pawn>() : new List<Pawn>(lord.ownedPawns);
                map.lordManager.RemoveLord(lord);
                for (int j = 0; j < pawns.Count; j++)
                {
                    PassLivingPawnToWorld(pawns[j]);
                }
            }
        }

        /// <summary>
        /// True unless the list is exactly two lords that both own pawns and none are the player.
        /// An empty list is not a broken pair.
        ///
        /// True, если список - не ровно два лорда, у обоих есть пешки и среди них нет игрока.
        /// Пустой список - не сломанная пара.
        /// </summary>
        static bool PairIsBroken(List<Lord> lords)
        {
            if (lords.Count == 0)
            {
                return false;
            }

            int withPawns = 0;
            for (int i = 0; i < lords.Count; i++)
            {
                Lord lord = lords[i];
                if (lord.faction == Faction.OfPlayer)
                {
                    return false;
                }

                List<Pawn> pawns = lord.ownedPawns;
                if (pawns == null || pawns.Count == 0)
                {
                    continue;
                }

                for (int j = 0; j < pawns.Count; j++)
                {
                    Pawn pawn = pawns[j];
                    if (pawn != null && pawn.Faction == Faction.OfPlayer)
                    {
                        return false;
                    }
                }

                withPawns++;
            }

            return lords.Count != 2 || withPawns != 2;
        }

        /// <summary>
        /// Despawns a living non-player pawn and passes it to the world.
        ///
        /// Деспавнит живую пешку не игрока и отправляет её в мир.
        /// </summary>
        static void PassLivingPawnToWorld(Pawn pawn)
        {
            if (pawn == null || pawn.Dead || pawn.Destroyed || pawn.Faction == Faction.OfPlayer)
            {
                return;
            }

            if (pawn.Spawned)
            {
                pawn.DeSpawn();
            }

            if (pawn.Spawned || Find.WorldPawns.Contains(pawn))
            {
                return;
            }

            Find.WorldPawns.PassToWorld(pawn);
        }
    }
}
