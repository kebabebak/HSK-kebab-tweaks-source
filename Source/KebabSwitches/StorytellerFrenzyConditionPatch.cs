using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// DoConditionsUI draws a row unless HiddenByOtherCondition is true. That call is also used
    /// by gameplay, so the method itself stays. The draw call is swapped for a wrapper that
    /// hides StorytellerFrenzy and StorytellerFrenzyRandom when that letter ignore is on.
    /// The condition keeps running.
    ///
    /// DoConditionsUI рисует строку, пока HiddenByOtherCondition ложен. Этот метод нужен и геймплею,
    /// поэтому сам он не меняется. Вызов в отрисовке заменён обёрткой, которая прячет
    /// StorytellerFrenzy и StorytellerFrenzyRandom, когда включён игнор этого письма.
    /// Условие при этом продолжает действовать.
    /// </summary>
    public static class StorytellerFrenzyConditionPatch
    {
        public const string LetterId = "StorytellerFrenzy";

        public static void Apply(Harmony harmony)
        {
            MethodInfo method = AccessTools.Method(typeof(GameConditionManager), "DoConditionsUI");
            if (method == null)
            {
                FeatureAvailability.WarnOnce(
                    "notifications:frenzy-ui",
                    "[HSK kebab tweaks] Notifications: GameConditionManager.DoConditionsUI was not found; storyteller frenzy row hook skipped.");
                return;
            }

            harmony.Patch(method, transpiler: new HarmonyMethod(typeof(StorytellerFrenzyConditionPatch), nameof(Transpiler)));
        }

        public static bool HiddenOrIgnored(GameCondition condition, Map map)
        {
            if (condition != null
                && KebabTweaksSettings.IsLetterSuppressed(LetterId)
                && IsFrenzy(condition))
            {
                return true;
            }

            if (condition == null)
            {
                return false;
            }

            return condition.HiddenByOtherCondition(map);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo hidden = AccessTools.Method(typeof(GameCondition), nameof(GameCondition.HiddenByOtherCondition));
            MethodInfo replacement = AccessTools.Method(typeof(StorytellerFrenzyConditionPatch), nameof(HiddenOrIgnored));
            var list = new List<CodeInstruction>(instructions);
            int replaced = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Calls(hidden))
                {
                    list[i].opcode = OpCodes.Call;
                    list[i].operand = replacement;
                    replaced++;
                }
            }

            if (replaced == 0)
            {
                FeatureAvailability.WarnOnce(
                    "notifications:frenzy-call",
                    "[HSK kebab tweaks] Notifications: storyteller frenzy row hook did not find HiddenByOtherCondition.");
            }

            return list;
        }

        private static bool IsFrenzy(GameCondition condition)
        {
            string name = condition.GetType().Name;
            return name != null && name.StartsWith("GameCondition_StorytellerFrenzy", System.StringComparison.Ordinal);
        }
    }
}
