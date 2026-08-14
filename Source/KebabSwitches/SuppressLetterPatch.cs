using System;
using System.Reflection;
using HarmonyLib;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Suppresses selected Letters before LetterStack adds them (no envelope button, sound, or card).
    ///
    /// Подавляет выбранные Letters до добавления в LetterStack (без кнопки-конверта, звука и карточки).
    /// </summary>
    public static class SuppressLetterPatch
    {
        public static void Apply(Harmony harmony)
        {
            foreach (MethodInfo method in typeof(LetterStack).GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!string.Equals(method.Name, "ReceiveLetter", StringComparison.Ordinal))
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length < 1)
                {
                    continue;
                }

                Type firstParam = parameters[0].ParameterType;
                HarmonyMethod prefix;
                if (firstParam == typeof(string))
                {
                    prefix = new HarmonyMethod(typeof(SuppressLetterPatch), nameof(PrefixString));
                }
                else if (firstParam == typeof(TaggedString))
                {
                    prefix = new HarmonyMethod(typeof(SuppressLetterPatch), nameof(PrefixTaggedString));
                }
                else
                {
                    continue;
                }

                harmony.Patch(method, prefix);
            }
        }

        public static bool PrefixString(string label)
        {
            return !ShouldSuppress(label);
        }

        public static bool PrefixTaggedString(TaggedString label)
        {
            return !ShouldSuppress(label);
        }

        private static bool ShouldSuppress(string label)
        {
            string entryId = LetterSuppressFilter.MatchEntryId(label);
            if (string.IsNullOrEmpty(entryId))
            {
                return false;
            }

            SuppressibleLetterEntry entry = LetterSuppressFilter.FindEntry(entryId);
            if (entry == null || !entry.IsSuppressEnabled)
            {
                return false;
            }

            SwitchLog.Message(
                $"[HSK kebab tweaks][kebab switches] Suppressed letter ({entryId}): \"{label}\"");
            return true;
        }

        private static bool ShouldSuppress(TaggedString label)
        {
            return ShouldSuppress(label.RawText ?? label.ToString());
        }
    }
}
