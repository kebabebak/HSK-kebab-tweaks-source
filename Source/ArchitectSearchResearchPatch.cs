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
    /// Problem: architect search highlights a category when a project label matches, including
    /// projects Designator_Build.Visible still hides. Research leaves Visible false until
    /// researchPrerequisites are finished. DebugSettings.godMode makes Visible true.
    /// Prefs.DevMode does not, but those projects stay searchable while developer mode is on.
    /// Architect Sense matches every subcategory child, so a hidden sibling lights a visible group.
    /// A dropdown label is the active element, so a hidden first child lights a visible dropdown.
    ///
    /// Fix: prefix ArchitectCategoryTab.Matches at Priority.First, before skyarkhangel.ArchitectSense.
    /// While this patch is on, a subcategory result is decided here so the older prefix cannot match
    /// hidden children. Hidden commands do not match. A dropdown matches only elements the player
    /// can open. Developer mode and god mode still match hidden projects. The Architect Sense type
    /// filter still rejects a subcategory when that type is selected.
    ///
    /// Проблема: поиск архитектора подсвечивает вкладку по названию проекта, который
    /// Designator_Build.Visible ещё прячет. Пока researchPrerequisites не закрыты, Visible ложно.
    /// DebugSettings.godMode делает Visible истинным. Prefs.DevMode этого не делает, но в режиме
    /// разработчика такие проекты всё равно ищутся. Architect Sense сравнивает каждого ребёнка
    /// подкатегории, поэтому скрытый сосед подсвечивает видимую группу. Подпись выпадающего
    /// списка - это активный элемент, поэтому скрытый первый ребёнок подсвечивает видимый список.
    ///
    /// Исправление: префикс ArchitectCategoryTab.Matches с Priority.First, раньше
    /// skyarkhangel.ArchitectSense. Пока патч включён, результат подкатегории решается здесь,
    /// и старый префикс не сравнивает скрытых детей. Скрытые команды не совпадают. Выпадающий
    /// список совпадает только с элементами, которые игрок может открыть. Режим разработчика и
    /// режим бога по-прежнему находят скрытые проекты. Фильтр типа Architect Sense по-прежнему
    /// отсекает подкатегорию, когда выбран этот тип.
    /// </summary>
    public static class ArchitectSearchResearchPatchFeatures
    {
        const string ArchitectSenseId = "skyarkhangel.ArchitectSense";
        const string SubCategoryTypeName = "ArchitectSense.Designator_SubCategory";
        const string FilterTypeName = "ArchitectSense.ArchitectSense_Filter";

        public static void Apply(Harmony harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(ArchitectCategoryTab), "Matches");
            if (method == null)
            {
                return;
            }

            HarmonyMethod prefix = new HarmonyMethod(typeof(ArchitectSearchResearchPatchFeatures), nameof(Prefix));
            prefix.priority = Priority.First;
            prefix.before = new[] { ArchitectSenseId };
            harmony.Patch(method, prefix: prefix);
        }

        /// <summary>
        /// Decides the search hit from what the player can see. Returns true only when a later
        /// prefix must still run: the patch is off, or developer mode / god mode leaves the
        /// subcategory to the loaded Architect Sense prefix.
        ///
        /// Решает попадание поиска по тому, что игрок видит. Возвращает true только когда
        /// следующий префикс ещё должен выполниться: патч выключен, либо режим разработчика /
        /// режим бога оставляет подкатегорию префиксу загруженного Architect Sense.
        /// </summary>
        public static bool Prefix(QuickSearchFilter ___quickSearchFilter, Command c, ref bool __result)
        {
            if (!KebabTweaksSettings.IsArchitectSearchResearchPatchEnabled() || ___quickSearchFilter == null)
            {
                return true;
            }

            bool includeHidden = Prefs.DevMode || DebugSettings.godMode;
            if (IsSubCategory(c))
            {
                if (includeHidden)
                {
                    return true;
                }

                if (TypeFilterRejects(c))
                {
                    __result = false;
                    return false;
                }

                if (ChildMatches(c, ___quickSearchFilter, visibleOnly: true))
                {
                    __result = true;
                    return false;
                }

                if (!c.Visible)
                {
                    __result = false;
                    return false;
                }

                if (ChildMatches(c, ___quickSearchFilter, visibleOnly: false))
                {
                    __result = ___quickSearchFilter.Matches(c.Label);
                    return false;
                }

                return true;
            }

            if (c is Designator_Dropdown dropdown)
            {
                if (!includeHidden && !dropdown.Visible)
                {
                    __result = false;
                    return false;
                }

                __result = DropdownMatches(dropdown, ___quickSearchFilter, includeHidden);
                return false;
            }

            if (!includeHidden && c != null && !c.Visible)
            {
                __result = false;
                return false;
            }

            return true;
        }

        static bool IsSubCategory(Command command)
        {
            return command != null && command.GetType().FullName == SubCategoryTypeName;
        }

        /// <summary>
        /// True when the Architect Sense type filter is on and this subcategory carries that type.
        /// Missing filter types mean Architect Sense is absent, so nothing is rejected here.
        ///
        /// True, когда фильтр типа Architect Sense включён и эта подкатегория несёт этот тип.
        /// Нет типов фильтра - нет Architect Sense, здесь никто не отсекается.
        /// </summary>
        static bool TypeFilterRejects(Command category)
        {
            Type filterType = AccessTools.TypeByName(FilterTypeName);
            if (filterType == null)
            {
                return false;
            }

            FieldInfo enabledField = AccessTools.Field(filterType, "Enabled");
            if (!(enabledField?.GetValue(null) is bool enabled) || !enabled)
            {
                return false;
            }

            object def = AccessTools.Field(category.GetType(), "def")?.GetValue(category);
            if (def == null)
            {
                return false;
            }

            DesignationCategoryDef designation = AccessTools.Field(def.GetType(), "designationCategory")
                ?.GetValue(def) as DesignationCategoryDef;
            MethodInfo getSelected = AccessTools.Method(filterType, "GetSelectedType");
            string selected = getSelected?.Invoke(null, new object[] { designation }) as string;
            if (selected == null || selected == "__ALL__")
            {
                return false;
            }

            if (!(AccessTools.Field(def.GetType(), "types")?.GetValue(def) is IList types))
            {
                return false;
            }

            for (int i = 0; i < types.Count; i++)
            {
                if (selected.Equals(types[i] as string))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Child labels of one subcategory. visibleOnly skips children the player cannot open.
        /// A false visibleOnly reports a hidden child, which must not be left to the older prefix.
        ///
        /// Подписи детей одной подкатегории. visibleOnly пропускает детей, которых игрок не откроет.
        /// visibleOnly false сообщает о скрытом ребёнке: его нельзя оставлять старому префиксу.
        /// </summary>
        static bool ChildMatches(Command category, QuickSearchFilter filter, bool visibleOnly)
        {
            if (!(AccessTools.Field(category.GetType(), "SubDesignators")?.GetValue(category) is IList children))
            {
                return false;
            }

            for (int i = 0; i < children.Count; i++)
            {
                Command child = children[i] as Command;
                if (child == null)
                {
                    continue;
                }

                if (visibleOnly)
                {
                    if (child.Visible && filter.Matches(child.Label))
                    {
                        return true;
                    }
                }
                else if (!child.Visible && filter.Matches(child.Label))
                {
                    return true;
                }
            }

            return false;
        }

        static bool DropdownMatches(Designator_Dropdown dropdown, QuickSearchFilter filter, bool includeHidden)
        {
            List<Designator> elements = dropdown?.Elements;
            if (elements == null)
            {
                return false;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                Designator element = elements[i];
                if (element == null || (!includeHidden && !element.Visible))
                {
                    continue;
                }

                if (filter.Matches(element.Label))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
