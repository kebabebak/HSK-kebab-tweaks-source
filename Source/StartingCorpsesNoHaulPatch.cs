using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: A new colony map can already have corpses (HSK StartwithCorpses, plus ruins).
    /// Colonists haul them from distant tiles into the starting stockpile. The default mark
    /// asked Keyz Allow Utilities for KAU_NoHaulDesignation. That mod is not in the HSK 1.6
    /// Alpha active list, so the mark did nothing and the Don't haul button stayed available.
    /// On that list the button is Haul Explicitly (likeafox.haulexplicitly). Designator_Unhaul
    /// calls Thing_Extensions.SetDontMoved. That sets HaulExplicitly_dontMoved and adds
    /// designation HaulExplicitly_Unhaul, which draws the anchor. Their passthrough on
    /// StoreUtility.TryFindBestBetterStorageFor then returns false, and the Pick Up And Haul
    /// OkThingToHaul postfix does the same. Adding only the designation would draw the anchor
    /// and still let stockpile haul run, because that postfix reads the flag, not the designation.
    ///
    /// Fix: Once per new player-home map, mark every corpse. Default uses Keyz
    /// KAU_NoHaulDesignation when that def is loaded and the designation sticks. Otherwise it
    /// calls Haul Explicitly SetDontMoved, resolved by name, with no assembly reference.
    /// Mode 1 applies vanilla SetForbidden. Mode 2 applies the Don't haul mark and forbid.
    /// Does not run when loading a save. If neither Don't haul target is available, default
    /// mode skips the mark and logs once. The feature stays on so mode 1 still works.
    /// Missing corpse generators only skip those scatter hooks. The Keyz def is read when
    /// features apply, after defs are loaded, and again on each mark.
    ///
    /// Проблема: на карте новой колонии уже могут лежать трупы (HSK StartwithCorpses и руины).
    /// Колонисты тащат их с дальних клеток на стартовый склад. Метка по умолчанию просила у
    /// Keyz Allow Utilities def KAU_NoHaulDesignation. Этого мода нет в активном списке HSK 1.6
    /// Alpha, поэтому метка не ставилась, а кнопка Don't haul оставалась доступной. В этом
    /// списке кнопка принадлежит Haul Explicitly (likeafox.haulexplicitly). Designator_Unhaul
    /// вызывает Thing_Extensions.SetDontMoved. Тот ставит HaulExplicitly_dontMoved и designation
    /// HaulExplicitly_Unhaul, по которому рисуется якорь. Их passthrough на
    /// StoreUtility.TryFindBestBetterStorageFor после этого возвращает false, и то же делает
    /// postfix OkThingToHaul у Pick Up And Haul. Одна designation нарисует якорь, но склад всё
    /// равно заберёт труп: postfix читает флаг, а не designation.
    ///
    /// Исправление: один раз на карте нового дома игрока помечаются все трупы. По умолчанию
    /// ставится KAU_NoHaulDesignation, если этот def загружен и designation остаётся на вещи.
    /// Иначе вызывается SetDontMoved Haul Explicitly, тип ищется по имени, без ссылки на сборку.
    /// Режим 1 ставит ванильный SetForbidden. Режим 2 ставит метку Don't haul и запрет.
    /// При загрузке сохранения не срабатывает. Если нет ни одной цели Don't haul, режим по
    /// умолчанию пропускает метку и пишет это один раз. Фича остаётся включённой, режим 1
    /// работает. Отсутствие генераторов трупов пропускает только их scatter-хуки. Def Keyz
    /// читается при применении фич, когда дефы уже загружены, и снова на каждой пометке.
    /// </summary>
    public static class StartingCorpsesNoHaulFeatures
    {
        public const string NoHaulDesignationDefName = "KAU_NoHaulDesignation";

        private const string HaulExplicitlyExtensionsTypeName = "HaulExplicitly.Extension.Thing_Extensions";

        private static bool haulExplicitlyResolved;

        private static MethodInfo haulExplicitlySetDontMoved;

        private static MethodInfo haulExplicitlyGetDontMoved;

        public static void Apply(Harmony harmony)
        {
            try
            {
                Type animalGeneratorType = AccessTools.TypeByName("SK.Events.AnimalCorpsesGenerator");
                Type villagerGeneratorType = AccessTools.TypeByName("SK.Events.VillagerCorpsesGenerator");
                MethodInfo generateAnimals = animalGeneratorType == null
                    ? null
                    : AccessTools.Method(animalGeneratorType, "GenerateAnimalCorpses");
                MethodInfo generateVillagers = villagerGeneratorType == null
                    ? null
                    : AccessTools.Method(villagerGeneratorType, "GenerateVillagerCorpses");
                bool animalGenerator = generateAnimals != null;
                bool villagerGenerator = generateVillagers != null;
                MethodInfo haulFastEarly = AccessTools.Method(
                    typeof(HaulAIUtility),
                    "PawnCanAutomaticallyHaulFast");
                if (!animalGenerator && !villagerGenerator && haulFastEarly == null)
                {
                    FeatureAvailability.SkipHandled(
                        FeatureAvailability.StartingCorpses,
                        "Starting Corpses Patch",
                        "corpse generators and HaulAIUtility.PawnCanAutomaticallyHaulFast were not found.");
                    return;
                }

                if (animalGenerator)
                {
                    harmony.Patch(
                        generateAnimals,
                        postfix: new HarmonyMethod(
                            typeof(AnimalCorpsesGenerator_GenerateAnimalCorpses_Patch),
                            nameof(AnimalCorpsesGenerator_GenerateAnimalCorpses_Patch.Postfix)));
                }

                if (villagerGenerator)
                {
                    harmony.Patch(
                        generateVillagers,
                        postfix: new HarmonyMethod(
                            typeof(VillagerCorpsesGenerator_GenerateVillagerCorpses_Patch),
                            nameof(VillagerCorpsesGenerator_GenerateVillagerCorpses_Patch.Postfix)));
                }

                ResolveHaulExplicitly();
                bool keyzNoHaul = DefDatabase<DesignationDef>.GetNamedSilentFail(NoHaulDesignationDefName) != null;
                bool haulExplicitlyNoHaul = haulExplicitlySetDontMoved != null;
                if (!animalGenerator || !villagerGenerator)
                {
                    string marking = keyzNoHaul || haulExplicitlyNoHaul
                        ? "Haul marking still applies."
                        : "Default mode has no Don't haul target.";
                    FeatureAvailability.WarnOnce(
                        "StartingCorpsesGenerators",
                        "[HSK kebab tweaks] Starting Corpses No Haul: Core SK (skyarkhangel.HSK) corpse generators are not loaded; scatter hooks skipped. " + marking);
                }

                if (!keyzNoHaul && haulExplicitlyNoHaul)
                {
                    Log.Message("[HSK kebab tweaks] Starting Corpses No Haul: KAU_NoHaulDesignation is not loaded. Don't haul uses Haul Explicitly.");
                }
                else if (!keyzNoHaul && !haulExplicitlyNoHaul)
                {
                    FeatureAvailability.WarnOnce(
                        "StartingCorpsesNoHaulDef",
                        "[HSK kebab tweaks] Starting Corpses No Haul: KAU_NoHaulDesignation is not loaded and Haul Explicitly Don't haul is not available. Default mode will not mark corpses.");
                }

                if (haulFastEarly == null)
                {
                    FeatureAvailability.WarnOnce(
                        "StartingCorpsesHaul",
                        "[HSK kebab tweaks] Starting Corpses Patch: HaulAIUtility.PawnCanAutomaticallyHaulFast was not found; the Keyz haul check was not installed.");
                }

                MethodInfo haulFast = haulFastEarly;
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

        /// <summary>
        /// Applies the default Don't haul mark. Keyz is used when its designation stays on the
        /// thing. Otherwise Haul Explicitly SetDontMoved is used. A miss of both logs once.
        ///
        /// Ставит метку Don't haul по умолчанию. Keyz используется, если его designation
        /// остаётся на вещи. Иначе вызывается SetDontMoved Haul Explicitly. Если нет обоих,
        /// об этом пишется один раз.
        /// </summary>
        private static void TryApplyNoHaul(Thing thing)
        {
            if (TryApplyKeyzNoHaul(thing))
            {
                return;
            }

            ResolveHaulExplicitly();
            if (haulExplicitlySetDontMoved == null)
            {
                FeatureAvailability.WarnOnce(
                    "StartingCorpsesNoHaulDef",
                    "[HSK kebab tweaks] Starting Corpses No Haul: KAU_NoHaulDesignation is not loaded and Haul Explicitly Don't haul is not available. Default mode will not mark corpses.");
                return;
            }

            TryApplyHaulExplicitlyNoHaul(thing);
        }

        /// <summary>
        /// Puts KAU_NoHaulDesignation on the thing. Returns false when the def is missing, the
        /// map cannot hold a designation, or the designation does not stick.
        ///
        /// Ставит на вещь KAU_NoHaulDesignation. Возвращает false, если def нет, карта не
        /// принимает designation или designation на вещи не остаётся.
        /// </summary>
        private static bool TryApplyKeyzNoHaul(Thing thing)
        {
            DesignationDef def = DefDatabase<DesignationDef>.GetNamedSilentFail(NoHaulDesignationDefName);
            if (def == null)
            {
                return false;
            }

            Map map = thing.MapHeld;
            if (map == null || map.designationManager == null)
            {
                return false;
            }

            if (map.designationManager.DesignationOn(thing, def) != null)
            {
                return true;
            }

            try
            {
                map.designationManager.AddDesignation(new Designation(thing, def));
            }
            catch (Exception e)
            {
                FeatureAvailability.WarnOnce(
                    "StartingCorpsesKeyzApply",
                    "[HSK kebab tweaks] Starting Corpses No Haul: KAU_NoHaulDesignation could not be applied (" + e.GetType().Name + ": " + e.Message + ").");
                return false;
            }

            return map.designationManager.DesignationOn(thing, def) != null;
        }

        /// <summary>
        /// Calls Haul Explicitly SetDontMoved(thing, true). That is the Don't haul button:
        /// the extra-data flag blocks stockpile search, and the same call adds the anchor
        /// designation. Returns false when the method is missing, the thing has no map, the
        /// call throws, or the flag does not stick (Haul Explicitly ignores a thing whose def
        /// is not alwaysHaulable).
        ///
        /// Вызывает SetDontMoved(thing, true) у Haul Explicitly. Это кнопка Don't haul:
        /// флаг в дополнительных данных закрывает поиск склада, и тот же вызов ставит
        /// designation-якорь. Возвращает false, если метода нет, у вещи нет карты, вызов
        /// бросает исключение или флаг не остаётся (Haul Explicitly пропускает вещь, у
        /// которой def не alwaysHaulable).
        /// </summary>
        private static bool TryApplyHaulExplicitlyNoHaul(Thing thing)
        {
            ResolveHaulExplicitly();
            if (haulExplicitlySetDontMoved == null || thing.MapHeld == null || Current.Game == null)
            {
                return false;
            }

            if (thing.def != null && !thing.def.alwaysHaulable)
            {
                FeatureAvailability.WarnOnce(
                    "StartingCorpsesHaulExplicitlyHaulable",
                    "[HSK kebab tweaks] Starting Corpses No Haul: Haul Explicitly Don't haul does not apply to " + thing.def.defName + " because it is not always haulable.");
                return false;
            }

            try
            {
                if (HaulExplicitlyDontMoved(thing))
                {
                    return true;
                }

                haulExplicitlySetDontMoved.Invoke(null, new object[] { thing, true });
                if (haulExplicitlyGetDontMoved == null || HaulExplicitlyDontMoved(thing))
                {
                    return true;
                }

                FeatureAvailability.WarnOnce(
                    "StartingCorpsesHaulExplicitlyStick",
                    "[HSK kebab tweaks] Starting Corpses No Haul: Haul Explicitly SetDontMoved did not mark " + (thing.def == null ? thing.ThingID : thing.def.defName) + ".");
                return false;
            }
            catch (Exception e)
            {
                Exception reported = e.InnerException ?? e;
                FeatureAvailability.WarnOnce(
                    "StartingCorpsesHaulExplicitlyApply",
                    "[HSK kebab tweaks] Starting Corpses No Haul: Haul Explicitly Don't haul failed (" + reported.GetType().Name + ": " + reported.Message + ").");
                return false;
            }
        }

        /// <summary>
        /// Resolves Haul Explicitly SetDontMoved and GetDontMoved once. A missing type leaves
        /// both methods null so the caller can fall through.
        ///
        /// Один раз ищет SetDontMoved и GetDontMoved у Haul Explicitly. Если типа нет, оба
        /// метода остаются null, и вызывающий код идёт дальше.
        /// </summary>
        private static void ResolveHaulExplicitly()
        {
            if (haulExplicitlyResolved)
            {
                return;
            }

            haulExplicitlyResolved = true;
            Type extensions = AccessTools.TypeByName(HaulExplicitlyExtensionsTypeName);
            if (extensions == null)
            {
                return;
            }

            haulExplicitlySetDontMoved = AccessTools.Method(extensions, "SetDontMoved");
            haulExplicitlyGetDontMoved = AccessTools.Method(extensions, "GetDontMoved");
        }

        private static bool HaulExplicitlyDontMoved(Thing thing)
        {
            if (haulExplicitlyGetDontMoved == null)
            {
                return false;
            }

            object marked = haulExplicitlyGetDontMoved.Invoke(null, new object[] { thing });
            return marked is bool && (bool)marked;
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
    /// The generator is patched by name: a Core SK build without AnimalCorpsesGenerator still compiles.
    ///
    /// Помечает трупы после scatter животных HSK на тике 1.
    /// Генератор патчится по имени: сборка Core SK без AnimalCorpsesGenerator всё равно компилируется.
    /// </summary>
    public static class AnimalCorpsesGenerator_GenerateAnimalCorpses_Patch
    {
        public static void Postfix(Building __instance)
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
    /// The generator is patched by name: a Core SK build without VillagerCorpsesGenerator still compiles.
    ///
    /// Помечает трупы после scatter племенных тел HSK на тике 1.
    /// Генератор патчится по имени: сборка Core SK без VillagerCorpsesGenerator всё равно компилируется.
    /// </summary>
    public static class VillagerCorpsesGenerator_GenerateVillagerCorpses_Patch
    {
        public static void Postfix(Building __instance)
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
    /// including when Keyz hid its own gizmos. Haul Explicitly is not handled here: SetDontMoved
    /// is what their StoreUtility and Pick Up And Haul postfixes already read, and forcing this
    /// result false would also block haul to a bill, which that Don't haul button does not do.
    ///
    /// Пока фича включена, KAU_NoHaulDesignation блокирует автопереноску на склады, в том числе
    /// если Keyz скрыл свои gizmo. Haul Explicitly здесь не обрабатывается: SetDontMoved читают
    /// их собственные postfix на StoreUtility и Pick Up And Haul, а принудительный false
    /// закрыл бы и доставку к верстаку, чего кнопка Don't haul не делает.
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
