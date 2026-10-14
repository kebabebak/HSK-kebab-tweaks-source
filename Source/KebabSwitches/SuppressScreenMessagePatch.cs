using System;
using System.Reflection;
using HarmonyLib;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Harmony prefixes on Messages.Message that skip catalog entries whose suppress checkbox is on.
    /// Roof collapse also sends the letter body as a message, so that toast follows the letter checkbox.
    /// Struck ore follows its own checkbox and either scanned-lump letter checkbox.
    ///
    /// Harmony Prefix на Messages.Message: пропускает записи каталога с включённым suppress.
    /// Обрушение крыши ещё и шлёт текст письма как сообщение, поэтому этот тост слушает чекбокс письма.
    /// Удар по руде слушает свой чекбокс и чекбокс любого письма о найденных залежах.
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
                else if (firstParam == typeof(Message))
                {
                    prefix = new HarmonyMethod(typeof(SuppressScreenMessagePatch), nameof(PrefixMessage));
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

        public static bool PrefixMessage(Message msg)
        {
            if (msg == null)
            {
                return true;
            }

            return !ShouldSuppress(msg.text);
        }

        private static bool ShouldSuppress(string text)
        {
            string entryId = ScreenMessageSuppressFilter.MatchEntryId(text);
            if (entryId != null)
            {
                SuppressibleScreenMessageEntry entry = ScreenMessageSuppressFilter.FindEntry(entryId);
                if (entry != null && entry.IsSuppressEnabled())
                {
                    LogSuppressed(entry.Id, text);
                    return true;
                }

                if (string.Equals(entryId, "StruckMineable", StringComparison.Ordinal)
                    && (LetterOn("FoundPreciousLump") || LetterOn("DeepScannerFoundLump")))
                {
                    LogSuppressed(entryId, text);
                    return true;
                }
            }

            string letterId = LetterSuppressFilter.MatchEntryId(text);
            if (!string.IsNullOrEmpty(letterId))
            {
                SuppressibleLetterEntry letter = LetterSuppressFilter.FindEntry(letterId);
                if (letter != null && letter.IsSuppressEnabled)
                {
                    LogSuppressed(letter.Id, text);
                    return true;
                }
            }

            return false;
        }

        private static bool LetterOn(string letterId)
        {
            SuppressibleLetterEntry letter = LetterSuppressFilter.FindEntry(letterId);
            return letter != null && letter.IsSuppressEnabled;
        }

        private static void LogSuppressed(string entryId, string text)
        {
            SwitchLog.Message(
                $"[HSK kebab tweaks][kebab switches] Suppressed screen Message [{entryId}]: \"{text}\"");
        }
    }
}
