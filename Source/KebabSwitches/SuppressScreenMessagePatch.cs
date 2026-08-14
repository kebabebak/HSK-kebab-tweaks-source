using System;
using System.Reflection;
using HarmonyLib;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Harmony prefixes on Messages.Message that skip catalog entries whose suppress checkbox is on.
    ///
    /// Harmony Prefix на Messages.Message: пропускает записи каталога с включённым suppress.
    /// </summary>
    public static class SuppressScreenMessagePatch
    {
        public static void Apply(Harmony harmony)
        {
            foreach (MethodInfo method in typeof(Messages).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (!string.Equals(method.Name, "Message", StringComparison.Ordinal))
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
                    prefix = new HarmonyMethod(typeof(SuppressScreenMessagePatch), nameof(PrefixString));
                }
                else if (firstParam == typeof(TaggedString))
                {
                    prefix = new HarmonyMethod(typeof(SuppressScreenMessagePatch), nameof(PrefixTaggedString));
                }
                else
                {
                    continue;
                }

                harmony.Patch(method, prefix);
            }
        }

        public static bool PrefixString(string text)
        {
            return !ShouldSuppress(text);
        }

        public static bool PrefixTaggedString(TaggedString text)
        {
            return !ShouldSuppress(text.RawText ?? text.ToString());
        }

        private static bool ShouldSuppress(string text)
        {
            string entryId = ScreenMessageSuppressFilter.MatchEntryId(text);
            if (entryId == null)
            {
                return false;
            }

            SuppressibleScreenMessageEntry entry = ScreenMessageSuppressFilter.FindEntry(entryId);
            if (entry == null || !entry.IsSuppressEnabled())
            {
                return false;
            }

            SwitchLog.Message(
                $"[HSK kebab tweaks][kebab switches] Suppressed screen Message [{entry.Id}]: \"{text}\"");
            return true;
        }
    }
}
