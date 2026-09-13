using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using SK.Events;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: A new colony map can already have corpses (HSK StartwithCorpses scatter of
    /// animal and tribal bodies, plus ruins / Anomaly room contents). Colonists haul them from
    /// distant tiles into the starting stockpile.
    ///
    /// Fix: Once per new player-home map, mark every corpse. Default applies Keyz Allow
    /// Utilities KAU_NoHaulDesignation so stockpile haul skips them. Mode 1 applies vanilla
    /// SetForbidden. Mode 2 applies both. Does not run when loading a save. Soft-skips
    /// KAU_NoHaulDesignation if that def is missing.
    ///
    /// Проблема: на карте новой колонии уже могут лежать трупы (HSK StartwithCorpses - животные
    /// и племенные тела, плюс руины / комнаты Anomaly). Колонисты тащат их с дальних клеток на
    /// стартовый склад.
    ///
    /// Исправление: один раз на карте нового дома игрока помечаются все трупы. По умолчанию
    /// ставится KAU_NoHaulDesignation Keyz Allow Utilities, склады их не берут.
    /// Режим 1 ставит ванильный SetForbidden. Режим 2 - оба. При загрузке сохранения не
    /// срабатывает. Если KAU_NoHaulDesignation нет, эта часть тихо пропускается.
    /// </summary>
    public static class StartingCorpsesNoHaulFeatures
    {
        public const string NoHaulDesignationDefName = "KAU_NoHaulDesignation";

        private static bool loggedMissingNoHaulDef;

        public static void Apply(Harmony harmony)
        {
            try
            {
                harmony.CreateClassProcessor(typeof(AnimalCorpsesGenerator_GenerateAnimalCorpses_Patch)).Patch();
                harmony.CreateClassProcessor(typeof(VillagerCorpsesGenerator_GenerateVillagerCorpses_Patch)).Patch();

                MethodInfo haulFast = AccessTools.Method(
                    typeof(HaulAIUtility),
                    "PawnCanAutomaticallyHaulFast");
                if (haulFast != null)
                {
                    harmony.Patch(
                        haulFast,
                        postfix: new HarmonyMethod(
                            typeof(HaulAIUtility_PawnCanAutomaticallyHaulFast_StartingCorpses_Patch),
                            nameof(HaulAIUtility_PawnCanAutomaticallyHaulFast_StartingCorpses_Patch.Postfix)));
                }

                Log.Message("[StartingCorpsesNoHaulPatch] Patches applied.");
            }
            catch (Exception e)
            {
                Log.Error("[StartingCorpsesNoHaulPatch] Failed to apply patches: " + e);
            }
        }

        public static void MarkAllCorpsesOnMap(Map map)
        {
            if (!KebabTweaksSettings.EnableStartingCorpsesNoHaul)
            {
                return;
            }

            if (map == null || map.listerThings == null)
            {
                return;
            }

            List<Thing> corpses = map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
            if (corpses == null || corpses.Count == 0)
            {
                return;
            }

            List<Thing> snapshot = new List<Thing>(corpses);
            for (int i = 0; i < snapshot.Count; i++)
            {
                Corpse corpse = snapshot[i] as Corpse;
                if (corpse != null)
                {
                    MarkCorpse(corpse);
                }
            }
        }

        public static bool IsPlayerHomeMap(Map map)
        {
            if (map == null)
            {
                return false;
            }

            if (map.IsPlayerHome)
            {
                return true;
            }

            return map.ParentFaction != null && map.ParentFaction.IsPlayer;
        }

        private static void MarkCorpse(Corpse corpse)
        {
            if (corpse == null || !corpse.Spawned)
            {
                return;
            }

            int mode = KebabTweaksSettings.StartingCorpseMarkMode;
            bool applyNoHaul = mode == 0 || mode == 2;
            bool applyForbid = mode == 1 || mode == 2;

            if (applyNoHaul)
            {
                TryApplyNoHaul(corpse);
            }

            if (applyForbid)
            {
                corpse.SetForbidden(true, false);
            }

            CancelHaulJobsTargeting(corpse);
        }

        private static void TryApplyNoHaul(Thing thing)
        {
            DesignationDef def = DefDatabase<DesignationDef>.GetNamedSilentFail(NoHaulDesignationDefName);
            if (def == null)
            {
                if (!loggedMissingNoHaulDef)
                {
                    loggedMissingNoHaulDef = true;
                    Log.Message(
                        "[StartingCorpsesNoHaulPatch] " + NoHaulDesignationDefName +
                        " not found; designation skipped (Keyz Allow Utilities missing).");
                }

                return;
            }

            Map map = thing.MapHeld;
            if (map == null || map.designationManager == null)
            {
                return;
            }

            if (map.designationManager.DesignationOn(thing, def) != null)
            {
                return;
            }

            map.designationManager.AddDesignation(new Designation(thing, def));
        }

        private static void CancelHaulJobsTargeting(Thing thing)
        {
            Map map = thing.MapHeld;
            if (map == null || map.mapPawns == null)
            {
                return;
            }

            List<Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
            if (colonists == null)
            {
                return;
            }

            for (int i = 0; i < colonists.Count; i++)
            {
                Pawn pawn = colonists[i];
                if (pawn == null || pawn.jobs == null)
                {
                    continue;
                }

                Job job = pawn.CurJob;
                if (job == null)
                {
                    continue;
                }

                if (!JobTargetsThing(job, thing))
                {
                    continue;
                }

                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob: false);
            }
        }

        private static bool JobTargetsThing(Job job, Thing thing)
        {
            if (job.targetA.Thing == thing || job.targetB.Thing == thing)
            {
                return true;
            }

            if (job.targetQueueA == null)
            {
                return false;
            }

            for (int i = 0; i < job.targetQueueA.Count; i++)
            {
                if (job.targetQueueA[i].Thing == thing)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Schedules a one-shot corpse mark on a newly generated player-home map (not on save load).
    ///
    /// Один раз помечает трупы на только что сгенерированной карте дома игрока (не при загрузке).
    /// </summary>
    public class StartingCorpsesMapComponent : MapComponent
    {
        private bool pendingPass;

        public StartingCorpsesMapComponent(Map map)
            : base(map)
        {
        }

        public override void MapGenerated()
        {
            if (!StartingCorpsesNoHaulFeatures.IsPlayerHomeMap(map))
            {
                return;
            }

            pendingPass = true;
            StartingCorpsesNoHaulFeatures.MarkAllCorpsesOnMap(map);
        }

        /// <summary>
        /// Called from StartedNewGame so the starting map is marked even if IsPlayerHome was
        /// not set yet during MapGenerated.
        ///
        /// Вызывается из StartedNewGame, чтобы стартовая карта помечалась, даже если во время
        /// MapGenerated IsPlayerHome ещё не был установлен.
        /// </summary>
        public void NotifyStartedNewGame()
        {
            pendingPass = true;
            StartingCorpsesNoHaulFeatures.MarkAllCorpsesOnMap(map);
        }

        public override void MapComponentTick()
        {
            if (!pendingPass)
            {
                return;
            }

            if (Find.TickManager == null || Find.TickManager.TicksGame < 1)
            {
                return;
            }

            StartingCorpsesNoHaulFeatures.MarkAllCorpsesOnMap(map);
            pendingPass = false;
        }
    }

    /// <summary>
    /// Flags the starting maps when a new game begins so corpses are marked after HSK scatter.
    ///
    /// Помечает стартовые карты при начале новой игры, чтобы трупы получили метку после scatter HSK.
    /// </summary>
    public class StartingCorpsesGameComponent : GameComponent
    {
        public StartingCorpsesGameComponent(Game game)
        {
        }

        public override void StartedNewGame()
        {
            if (Current.Game == null || Current.Game.Maps == null)
            {
                return;
            }

            List<Map> maps = Current.Game.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (map == null)
                {
                    continue;
                }

                StartingCorpsesMapComponent component = map.GetComponent<StartingCorpsesMapComponent>();
                if (component != null)
                {
                    component.NotifyStartedNewGame();
                }
                else
                {
                    StartingCorpsesNoHaulFeatures.MarkAllCorpsesOnMap(map);
                }
            }
        }
    }

    /// <summary>
    /// Marks corpses after HSK animal-corpse scatter on tick 1.
    ///
    /// Помечает трупы после scatter животных HSK на тике 1.
    /// </summary>
    [HarmonyPatch(typeof(AnimalCorpsesGenerator), "GenerateAnimalCorpses")]
    public static class AnimalCorpsesGenerator_GenerateAnimalCorpses_Patch
    {
        public static void Postfix(AnimalCorpsesGenerator __instance)
        {
            if (__instance == null)
            {
                return;
            }

            StartingCorpsesNoHaulFeatures.MarkAllCorpsesOnMap(__instance.Map);
        }
    }

    /// <summary>
    /// Marks corpses after HSK tribal-corpse scatter on tick 1.
    ///
    /// Помечает трупы после scatter племенных тел HSK на тике 1.
    /// </summary>
    [HarmonyPatch(typeof(VillagerCorpsesGenerator), "GenerateVillagerCorpses")]
    public static class VillagerCorpsesGenerator_GenerateVillagerCorpses_Patch
    {
        public static void Postfix(VillagerCorpsesGenerator __instance)
        {
            if (__instance == null)
            {
                return;
            }

            StartingCorpsesNoHaulFeatures.MarkAllCorpsesOnMap(__instance.Map);
        }
    }

    /// <summary>
    /// Treats KAU_NoHaulDesignation as blocking automatic stockpile haul while this feature is on,
    /// including when Keyz hid its own gizmos.
    ///
    /// Пока фича включена, KAU_NoHaulDesignation блокирует автопереноску на склады, в том числе
    /// если Keyz скрыл свои gizmo.
    /// </summary>
    public static class HaulAIUtility_PawnCanAutomaticallyHaulFast_StartingCorpses_Patch
    {
        public static void Postfix(Thing t, ref bool __result)
        {
            if (!__result || !KebabTweaksSettings.EnableStartingCorpsesNoHaul || t == null)
            {
                return;
            }

            Map map = t.MapHeld;
            if (map == null || map.designationManager == null)
            {
                return;
            }

            DesignationDef def = DefDatabase<DesignationDef>.GetNamedSilentFail(
                StartingCorpsesNoHaulFeatures.NoHaulDesignationDefName);
            if (def == null)
            {
                return;
            }

            if (map.designationManager.DesignationOn(t, def) != null)
            {
                __result = false;
            }
        }
    }
}
