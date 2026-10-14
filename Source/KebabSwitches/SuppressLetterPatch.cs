using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Suppresses selected letters before LetterStack adds them, and drops letters already on the
    /// stack (a save restores them without ReceiveLetter). Match uses the label and the body.
    ///
    /// Подавляет выбранные письма до добавления в LetterStack и снимает письма, уже лежащие в стопке
    /// (сохранение восстанавливает их без ReceiveLetter). Совпадение идёт по label и по тексту.
    /// </summary>
    public static class SuppressLetterPatch
    {
        private static bool purging;

        private static readonly FieldInfo LetterLabelField = AccessTools.Field(typeof(Letter), "label");

        private static readonly FieldInfo ChoiceLetterTextField = AccessTools.Field(typeof(ChoiceLetter), "text");

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
                else if (firstParam == typeof(Letter))
                {
                    prefix = new HarmonyMethod(typeof(SuppressLetterPatch), nameof(PrefixLetter));
                }
                else
                {
                    continue;
                }

                harmony.Patch(method, prefix);
            }

            MethodInfo lettersOnGui = AccessTools.Method(typeof(LetterStack), "LettersOnGUI");
            if (lettersOnGui != null)
            {
                harmony.Patch(
                    lettersOnGui,
                    prefix: new HarmonyMethod(typeof(SuppressLetterPatch), nameof(LettersOnGUI_Prefix)));
            }
        }

        public static bool PrefixString(string label)
        {
            return !ShouldSuppress(label);
        }

        public static bool PrefixTaggedString(TaggedString label, TaggedString text)
        {
            return !ShouldSuppress(label) && !ShouldSuppress(text);
        }

        public static bool PrefixLetter(Letter let)
        {
            return !LetterSuppressed(let);
        }

        /// <summary>
        /// Letters restored from a save are drawn by LettersOnGUI and never pass ReceiveLetter again.
        ///
        /// Письма из сохранения рисует LettersOnGUI, повторного ReceiveLetter у них нет.
        /// </summary>
        public static void LettersOnGUI_Prefix(LetterStack __instance)
        {
            if (purging || __instance == null)
            {
                return;
            }

            List<Letter> list = __instance.LettersListForReading;
            if (list == null || list.Count == 0)
            {
                return;
            }

            purging = true;
            try
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    Letter let = list[i];
                    if (LetterSuppressed(let))
                    {
                        __instance.RemoveLetter(let);
                    }
                }
            }
            finally
            {
                purging = false;
            }
        }

        private static bool LetterSuppressed(Letter let)
        {
            if (let == null)
            {
                return false;
            }

            if (ShouldSuppress(ReadField(let, LetterLabelField)))
            {
                return true;
            }

            ChoiceLetter choice = let as ChoiceLetter;
            if (choice != null && ShouldSuppress(ReadField(choice, ChoiceLetterTextField)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Letter.label and ChoiceLetter.text are on the game type. The reference assembly does not expose them.
        ///
        /// Letter.label и ChoiceLetter.text есть на игровом типе. Референс-сборка их не показывает.
        /// </summary>
        private static string ReadField(object obj, FieldInfo field)
        {
            if (obj == null || field == null)
            {
                return null;
            }

            object value = field.GetValue(obj);
            if (value == null)
            {
                return null;
            }

            if (value is TaggedString tagged)
            {
                return tagged.RawText ?? tagged.ToString();
            }

            return value.ToString();
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
