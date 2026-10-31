using System;
using HarmonyLib;
using Verse;
using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Problem: HSK / vanilla spam screen Messages, routine Letters, and HUD Alerts cannot be toggled off individually.
    ///
    /// Fix: Harmony prefixes on Messages.Message, LetterStack.ReceiveLetter (label and body, including a
    /// letter object), and AlertsReadout.CheckAddOrRemoveAlert suppress selected catalog entries.
    /// A message that repeats a letter body follows that letter checkbox. LettersOnGUI drops letters
    /// the save already put on the stack. Storyteller frenzy also hides that condition row.
    /// Colonists lack tools has no whole-alert ignore. Each work requirement is its own checkbox:
    /// a checked requirement does not count, and the alert stays while any other requirement is still missing.
    ///
    /// Проблема: spam screen Messages HSK/vanilla, рутинные Letters и HUD Alerts нельзя отключать по отдельности.
    ///
    /// Исправление: Harmony Prefix на Messages.Message, LetterStack.ReceiveLetter (label и текст, в том
    /// числе объект письма) и AlertsReadout.CheckAddOrRemoveAlert подавляет выбранные записи каталога.
    /// Сообщение, повторяющее текст письма, слушает чекбокс этого письма. LettersOnGUI снимает письма,
    /// которые сохранение уже положило в стопку. Ярость рассказчика также прячет строку этого условия.
    /// «Поселенцы нуждаются в инструменте» не отключается целиком. Каждое требование к работе - свой
    /// чекбокс: отмеченное не считается, оповещение остаётся, пока не хватает любого неотмеченного.
    /// </summary>
    public static class KebabSwitchesFeatures
    {
        public static void Apply(Harmony harmony)
        {
            try
            {
                NotificationDependency.Probe();
                if (!NotificationDependency.AnyHook)
                {
                    FeatureAvailability.SkipHandled(
                        FeatureAvailability.KebabSwitches,
                        "Notifications",
                        "Messages.Message, LetterStack.ReceiveLetter, and AlertsReadout.CheckAddOrRemoveAlert were not found.");
                    return;
                }

                ScreenMessageSuppressFilter.RebuildCache();
                LetterSuppressFilter.RebuildCache();
                SuppressScreenMessagePatch.Apply(harmony);
                SuppressLetterPatch.Apply(harmony);
                SuppressAlertPatch.Apply(harmony);
                StorytellerFrenzyConditionPatch.Apply(harmony);
                SurvivalToolNeedPatch.Apply(harmony);

                int enabledScreenSuppress = 0;
                foreach (SuppressibleScreenMessageEntry entry in SuppressibleScreenMessages.All)
                {
                    if (entry.IsSuppressEnabled())
                    {
                        enabledScreenSuppress++;
                    }
                }

                Log.Message(
                    $"[HSK kebab tweaks][kebab switches] Loaded (logging {(KebabTweaksSettings.KebabSwitchesEnableLogging ? "ON" : "OFF")}, " +
                    $"screen Message suppress checkboxes ON={enabledScreenSuppress}/{SuppressibleScreenMessages.All.Length}, " +
                    $"Letter suppress checkboxes ON={KebabTweaksSettings.SuppressedLetterCount}/{SuppressibleLetters.All.Length}, " +
                    $"Alert suppress checkboxes ON={KebabTweaksSettings.SuppressedAlertCount}/{SuppressibleAlerts.All.Length}, " +
                    $"tool-need ON={ToolNeedIgnore.SuppressedCount()}/{ToolNeedIgnore.All.Length}, " +
                    $"tracked message variants={ScreenMessageSuppressFilter.TrackedVariantCount}, " +
                    $"tracked letter variants={LetterSuppressFilter.TrackedVariantCount}).");
            }
            catch (Exception ex)
            {
                Log.Error("[HSK kebab tweaks][kebab switches] Failed to apply patches: " + ex);
            }
        }
    }

    public static class SwitchLog
    {
        public static void Message(string text)
        {
            if (KebabTweaksSettings.KebabSwitchesEnableLogging)
            {
                Log.Message(text);
            }
        }
    }
}
