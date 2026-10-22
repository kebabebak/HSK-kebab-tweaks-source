using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: the monument material window lists every floor and stuff the sketch accepts.
    /// A floor whose cost items are all absent from the map is still offered, and so is a stuff
    /// when neither that stuff nor any other item that building needs is on the map.
    ///
    /// Fix: while Dialog_SimpleMonumentMaterials draws, terrain candidate lists and AllowedStuffs
    /// keep an option when any required ThingDef has a count above zero on the marker map.
    /// One of three is enough. The option is dropped only when every required item is absent.
    /// The saved candidate lists are put back when the window method ends. Simple Monument Quest
    /// is optional: missing types skip the hooks.
    ///
    /// Проблема: окно материалов монумента показывает каждый пол и материал, который принимает чертёж.
    /// Пол, всех предметов стоимости которого нет на карте, всё равно предлагается. Так же и материал,
    /// если на карте нет ни его, ни другого предмета, нужного этой постройке.
    ///
    /// Исправление: пока рисуется Dialog_SimpleMonumentMaterials, списки полов и AllowedStuffs
    /// оставляют вариант, если любой нужный ThingDef есть на карте маркера в количестве больше нуля.
    /// Одного из трёх достаточно. Вариант убирается только когда нет ни одного нужного предмета.
    /// Сохранённые списки возвращаются, когда метод окна заканчивается. Simple Monument Quest
    /// необязателен: без типов хуки не ставятся.
    /// </summary>
    public static class MonumentMaterialOnMapPatchFeatures
    {
        const string DialogTypeName = "ZuoYao.SimpleMonumentQuest.Dialog_SimpleMonumentMaterials";
        const string GroupTypeName = "ZuoYao.SimpleMonumentQuest.MonumentConstructionGroup";

        static readonly List<SavedCandidates> savedCandidates = new List<SavedCandidates>();
        static int depth;
        static Map currentMap;
        static Sketch currentSketch;

        /// <summary>
        /// Installs the dialog filter when Simple Monument Quest is loaded.
        ///
        /// Ставит фильтр окна, когда Simple Monument Quest загружен.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            Type dialogType = AccessTools.TypeByName(DialogTypeName);
            Type groupType = AccessTools.TypeByName(GroupTypeName);
            MethodInfo draw = AccessTools.Method(dialogType, "DoWindowContents");
            MethodInfo allowed = AccessTools.Method(groupType, "AllowedStuffs");
            if (dialogType == null || groupType == null || draw == null || allowed == null)
            {
                FeatureAvailability.SkipMissingMods(
                    FeatureAvailability.MonumentMaterials,
                    "Monument Material Patch",
                    "Simple Monument Quest (ZuoYao.SimpleMonumentQuest)");
                return;
            }

            harmony.Patch(
                draw,
                prefix: new HarmonyMethod(typeof(MonumentMaterialOnMapPatchFeatures), nameof(Prefix)),
                finalizer: new HarmonyMethod(typeof(MonumentMaterialOnMapPatchFeatures), nameof(Finalizer)));
            harmony.Patch(
                allowed,
                postfix: new HarmonyMethod(typeof(MonumentMaterialOnMapPatchFeatures), nameof(AllowedStuffsPostfix)));
        }

        /// <summary>
        /// Swaps in filtered candidate lists for this draw, including the click that accepts them.
        ///
        /// На время этой отрисовки подставляет отфильтрованные списки, включая щелчок, который их принимает.
        /// </summary>
        public static void Prefix(object __instance)
        {
            if (!KebabTweaksSettings.IsMonumentMaterialOnMapPatchEnabled())
            {
                return;
            }

            depth++;
            if (depth == 1)
            {
                Begin(__instance);
            }
        }

        /// <summary>
        /// Puts the sketch candidate lists back. An exception from the window is left unchanged.
        ///
        /// Возвращает списки кандидатов чертежа. Исключение окна не подменяется.
        /// </summary>
        public static Exception Finalizer(Exception __exception)
        {
            if (depth > 0)
            {
                depth--;
            }

            if (depth == 0)
            {
                Restore();
            }

            return __exception;
        }

        /// <summary>
        /// Drops stuffs whose own def and every other cost of the matching buildings are absent.
        ///
        /// Убирает материалы, чьего дефа и любого другого предмета стоимости подходящих построек нет.
        /// </summary>
        public static void AllowedStuffsPostfix(object __instance, ref List<ThingDef> __result)
        {
            if (depth <= 0 || __result == null || !KebabTweaksSettings.IsMonumentMaterialOnMapPatchEnabled())
            {
                return;
            }

            object categories = AccessTools.Field(__instance.GetType(), "categories")?.GetValue(__instance);
            List<ThingDef> filtered = new List<ThingDef>();
            for (int i = 0; i < __result.Count; i++)
            {
                ThingDef stuff = __result[i];
                if (StuffShown(currentMap, currentSketch, categories as IList, stuff))
                {
                    filtered.Add(stuff);
                }
            }

            __result = filtered;
        }

        static void Begin(object dialog)
        {
            object marker = AccessTools.Field(dialog.GetType(), "marker")?.GetValue(dialog);
            currentMap = (marker as Thing)?.Map;
            currentSketch = (marker as MonumentMarker)?.sketch;
            if (!(AccessTools.Field(marker?.GetType(), "constructionGroups")?.GetValue(marker) is IList groups))
            {
                return;
            }

            for (int i = 0; i < groups.Count; i++)
            {
                object group = groups[i];
                if (!IsTerrain(group))
                {
                    continue;
                }

                FieldInfo field = AccessTools.Field(group.GetType(), "terrainCandidates");
                if (!(field?.GetValue(group) is List<TerrainDef> candidates))
                {
                    continue;
                }

                savedCandidates.Add(new SavedCandidates
                {
                    Group = group,
                    Field = field,
                    Candidates = candidates
                });
                List<TerrainDef> filtered = new List<TerrainDef>();
                for (int j = 0; j < candidates.Count; j++)
                {
                    if (TerrainShown(currentMap, candidates[j]))
                    {
                        filtered.Add(candidates[j]);
                    }
                }

                field.SetValue(group, filtered);
            }

            ClearHiddenSelections(dialog, groups);
        }

        static void ClearHiddenSelections(object dialog, IList groups)
        {
            IList selections = AccessTools.Field(dialog.GetType(), "selections")?.GetValue(dialog) as IList;
            IList terrainSelections = AccessTools.Field(dialog.GetType(), "terrainSelections")?.GetValue(dialog) as IList;
            for (int i = 0; i < groups.Count; i++)
            {
                object group = groups[i];
                if (group == null)
                {
                    continue;
                }

                if (IsTerrain(group))
                {
                    if (terrainSelections == null || i >= terrainSelections.Count)
                    {
                        continue;
                    }

                    List<TerrainDef> candidates = AccessTools.Field(group.GetType(), "terrainCandidates")
                        ?.GetValue(group) as List<TerrainDef>;
                    TerrainDef selected = terrainSelections[i] as TerrainDef;
                    if (selected != null && (candidates == null || !candidates.Contains(selected)))
                    {
                        terrainSelections[i] = null;
                    }
                }
                else if (selections != null && i < selections.Count)
                {
                    MethodInfo allowed = AccessTools.Method(group.GetType(), "AllowedStuffs");
                    List<ThingDef> stuffs = allowed?.Invoke(group, null) as List<ThingDef>;
                    ThingDef selected = selections[i] as ThingDef;
                    if (selected != null && (stuffs == null || !stuffs.Contains(selected)))
                    {
                        selections[i] = null;
                    }
                }
            }
        }

        static void Restore()
        {
            for (int i = 0; i < savedCandidates.Count; i++)
            {
                SavedCandidates saved = savedCandidates[i];
                saved.Field.SetValue(saved.Group, saved.Candidates);
            }

            savedCandidates.Clear();
            currentMap = null;
            currentSketch = null;
        }

        static bool IsTerrain(object group)
        {
            object kind = AccessTools.Field(group?.GetType(), "kind")?.GetValue(group);
            return kind != null && kind.ToString() == "Terrain";
        }

        static bool TerrainShown(Map map, TerrainDef terrain)
        {
            if (terrain == null)
            {
                return false;
            }

            if (map == null)
            {
                return true;
            }

            List<ThingDefCountClass> costs = terrain.CostList;
            if (costs == null || costs.Count == 0)
            {
                return true;
            }

            return AnyCostOnMap(map, costs);
        }

        static bool StuffShown(Map map, Sketch sketch, IList categories, ThingDef stuff)
        {
            if (stuff == null)
            {
                return false;
            }

            if (map == null)
            {
                return true;
            }

            if (AnyOnMap(map, stuff))
            {
                return true;
            }

            if (sketch?.Buildables == null)
            {
                return false;
            }

            for (int i = 0; i < sketch.Buildables.Count; i++)
            {
                ThingDef thing = sketch.Buildables[i]?.Buildable as ThingDef;
                if (thing == null || !thing.MadeFromStuff || !CategoriesOverlap(thing.stuffCategories, categories))
                {
                    continue;
                }

                if (AnyCostOnMap(map, thing.costList))
                {
                    return true;
                }

                try
                {
                    if (AnyCostOnMap(map, thing.CostListAdjusted(stuff, false)))
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                }
            }

            return false;
        }

        static bool CategoriesOverlap(List<StuffCategoryDef> thingCategories, IList wanted)
        {
            if (thingCategories == null || wanted == null)
            {
                return false;
            }

            for (int i = 0; i < thingCategories.Count; i++)
            {
                StuffCategoryDef category = thingCategories[i];
                for (int j = 0; j < wanted.Count; j++)
                {
                    if (category == wanted[j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool AnyCostOnMap(Map map, List<ThingDefCountClass> costs)
        {
            if (costs == null)
            {
                return false;
            }

            for (int i = 0; i < costs.Count; i++)
            {
                ThingDef def = costs[i]?.thingDef;
                if (def != null && AnyOnMap(map, def))
                {
                    return true;
                }
            }

            return false;
        }

        static bool AnyOnMap(Map map, ThingDef def)
        {
            if (map == null || def == null)
            {
                return false;
            }

            try
            {
                if (map.resourceCounter != null && map.resourceCounter.GetCount(def) > 0)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }

            List<Thing> things = map.listerThings?.ThingsOfDef(def);
            if (things == null)
            {
                return false;
            }

            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing != null && thing.stackCount > 0)
                {
                    return true;
                }
            }

            return false;
        }

        sealed class SavedCandidates
        {
            public object Group;
            public FieldInfo Field;
            public List<TerrainDef> Candidates;
        }
    }
}
