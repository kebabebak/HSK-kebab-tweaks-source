using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// One startup pass that records which notification hooks and ignore targets are present.
    /// A missing Messages.Message, ReceiveLetter, or CheckAddOrRemoveAlert veils that feed.
    /// A keyed row also needs its translation key. An alert row needs Alert_ plus its id
    /// (AquaponicBasinNotBreeding is the loaded class Alert_AquacultureBasinNotBreeding).
    /// Surgery failure needs Recipe_Surgery, the fallen monolith needs ThingDef VoidMonolith,
    /// and storyteller frenzy needs GameConditionDef StorytellerFrenzy or StorytellerFrenzyRandom.
    /// A tool-need row needs SurvivalToolsLite.Alert_ColonistNeedsSurvivalTool and that StatDef.
    /// A hardcoded screen message stays available while Messages.Message itself exists.
    ///
    /// Один проход при запуске: какие хуки уведомлений и цели игнора есть.
    /// Нет Messages.Message, ReceiveLetter или CheckAddOrRemoveAlert - затемняется эта лента.
    /// Строке с ключом нужен этот ключ перевода. Строке alert нужен тип Alert_ и её id
    /// (AquaponicBasinNotBreeding - это загруженный класс Alert_AquacultureBasinNotBreeding).
    /// Провалу операции нужен Recipe_Surgery, упавшему монолиту - ThingDef VoidMonolith,
    /// ярости рассказчика - GameConditionDef StorytellerFrenzy или StorytellerFrenzyRandom.
    /// Строке инструмента нужны SurvivalToolsLite.Alert_ColonistNeedsSurvivalTool и этот StatDef.
    /// Жёсткое screen message остаётся доступным, пока есть сам Messages.Message.
    /// </summary>
    public static class NotificationDependency
    {
        static readonly HashSet<string> alertTypeNames = new HashSet<string>(StringComparer.Ordinal);
        static readonly List<string> missingScreenIds = new List<string>();
        static readonly List<string> missingLetterIds = new List<string>();
        static readonly List<string> missingAlertIds = new List<string>();
        static readonly List<string> missingToolIds = new List<string>();

        public static bool Probed { get; private set; }

        public static bool ScreenHook { get; private set; }

        public static bool LetterHook { get; private set; }

        public static bool AlertHook { get; private set; }

        public static bool ToolAlert { get; private set; }

        public static bool AnyHook => ScreenHook || LetterHook || AlertHook;

        /// <summary>
        /// Records hooks and catalog targets once. Warns once per feed that cannot run.
        ///
        /// Один раз записывает хуки и цели каталога. Одно предупреждение на ленту, которая не работает.
        /// </summary>
        public static void Probe()
        {
            if (Probed)
            {
                return;
            }

            Probed = true;
            ScreenHook = HasMessageMethod();
            LetterHook = HasReceiveLetter();
            AlertHook = AccessTools.Method(typeof(AlertsReadout), "CheckAddOrRemoveAlert") != null;
            ToolAlert = AccessTools.TypeByName("SurvivalToolsLite.Alert_ColonistNeedsSurvivalTool") != null;
            IndexAlertTypes();

            bool languageReady = LanguageDatabase.activeLanguage != null;
            if (!languageReady)
            {
                FeatureAvailability.WarnOnce(
                    "notifications:language",
                    "[HSK kebab tweaks] Notifications: language data was not loaded; keyed ignore rows were left available.");
            }

            if (!ScreenHook)
            {
                FeatureAvailability.WarnOnce(
                    "notifications:screen-hook",
                    "[HSK kebab tweaks] Notifications screen messages skipped: Messages.Message was not found.");
            }
            else if (languageReady)
            {
                CollectMissingScreen(missingScreenIds);
                WarnIds(
                    "notifications:screen-rows",
                    "Notifications screen messages skipped: text was not found: ",
                    missingScreenIds);
            }

            if (!LetterHook)
            {
                FeatureAvailability.WarnOnce(
                    "notifications:letter-hook",
                    "[HSK kebab tweaks] Notifications letters skipped: LetterStack.ReceiveLetter was not found.");
            }
            else
            {
                CollectMissingLetters(missingLetterIds, languageReady);
                WarnIds(
                    "notifications:letter-rows",
                    "Notifications letters skipped: text was not found: ",
                    missingLetterIds);
            }

            if (!AlertHook)
            {
                FeatureAvailability.WarnOnce(
                    "notifications:alert-hook",
                    "[HSK kebab tweaks] Notifications alerts skipped: AlertsReadout.CheckAddOrRemoveAlert was not found.");
            }
            else
            {
                CollectMissingAlerts(missingAlertIds);
                WarnIds(
                    "notifications:alert-rows",
                    "Notifications alerts skipped: alert type was not found: ",
                    missingAlertIds);
            }

            if (!ToolAlert)
            {
                FeatureAvailability.WarnOnce(
                    "notifications:tool-alert",
                    "[HSK kebab tweaks] Notifications tool needs skipped: SurvivalToolsLite.Alert_ColonistNeedsSurvivalTool was not found.");
            }
            else
            {
                CollectMissingTools(missingToolIds);
                WarnIds(
                    "notifications:tool-rows",
                    "Notifications tool needs skipped: stat was not found: ",
                    missingToolIds);
            }
        }

        public static bool IsScreenRowAvailable(SuppressibleScreenMessageEntry entry)
        {
            if (!Probed || entry == null)
            {
                return Probed ? false : true;
            }

            if (!ScreenHook)
            {
                return false;
            }

            if (entry.TranslationKey.NullOrEmpty())
            {
                return !entry.HardcodedEnglish.NullOrEmpty();
            }

            if (LanguageDatabase.activeLanguage == null)
            {
                return true;
            }

            return entry.TranslationKey.CanTranslate() || !entry.HardcodedEnglish.NullOrEmpty();
        }

        public static bool IsLetterRowAvailable(SuppressibleLetterEntry entry)
        {
            if (!Probed || entry == null)
            {
                return Probed ? false : true;
            }

            if (!LetterHook)
            {
                return false;
            }

            if (entry.IsSurgeryFailed)
            {
                return AccessTools.TypeByName("RimWorld.Recipe_Surgery") != null;
            }

            if (string.Equals(entry.Id, "FallenMonolith", StringComparison.Ordinal))
            {
                return DefDatabase<ThingDef>.GetNamedSilentFail("VoidMonolith") != null;
            }

            if (string.Equals(entry.Id, "StorytellerFrenzy", StringComparison.Ordinal))
            {
                return DefDatabase<GameConditionDef>.GetNamedSilentFail("StorytellerFrenzy") != null
                    || DefDatabase<GameConditionDef>.GetNamedSilentFail("StorytellerFrenzyRandom") != null;
            }

            if (entry.TranslationKey.NullOrEmpty())
            {
                return false;
            }

            if (LanguageDatabase.activeLanguage == null)
            {
                return true;
            }

            return entry.TranslationKey.CanTranslate();
        }

        public static bool IsAlertRowAvailable(SuppressibleAlertEntry entry)
        {
            if (!Probed || entry == null)
            {
                return Probed ? false : true;
            }

            return AlertHook && AlertPresent(entry.Id);
        }

        public static bool IsToolRowAvailable(ToolNeedEntry entry)
        {
            if (!Probed || entry == null)
            {
                return Probed ? false : true;
            }

            if (!AlertHook || !ToolAlert)
            {
                return false;
            }

            return DefDatabase<StatDef>.GetNamedSilentFail(entry.StatDefName) != null;
        }

        public static bool AnyScreenRow()
        {
            return Any(SuppressibleScreenMessages.All, IsScreenRowAvailable);
        }

        public static bool AnyLetterRow()
        {
            return Any(SuppressibleLetters.All, IsLetterRowAvailable);
        }

        public static bool AnyAlertIgnore()
        {
            if (Any(SuppressibleAlerts.All, IsAlertRowAvailable))
            {
                return true;
            }

            return Any(ToolNeedIgnore.All, IsToolRowAvailable);
        }

        public static bool AnyIgnoreRow()
        {
            return AnyScreenRow() || AnyLetterRow() || AnyAlertIgnore();
        }

        static bool Any<T>(T[] entries, Func<T, bool> available)
        {
            if (entries == null)
            {
                return false;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (available(entries[i]))
                {
                    return true;
                }
            }

            return false;
        }

        static bool HasMessageMethod()
        {
            MethodInfo[] methods = typeof(Messages).GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                if (!string.Equals(methods[i].Name, "Message", StringComparison.Ordinal))
                {
                    continue;
                }

                ParameterInfo[] parameters = methods[i].GetParameters();
                if (parameters.Length < 1)
                {
                    continue;
                }

                Type first = parameters[0].ParameterType;
                if (first == typeof(string) || first == typeof(TaggedString) || first == typeof(Message))
                {
                    return true;
                }
            }

            return false;
        }

        static bool HasReceiveLetter()
        {
            MethodInfo[] methods = typeof(LetterStack).GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                if (!string.Equals(methods[i].Name, "ReceiveLetter", StringComparison.Ordinal))
                {
                    continue;
                }

                ParameterInfo[] parameters = methods[i].GetParameters();
                if (parameters.Length < 1)
                {
                    continue;
                }

                Type first = parameters[0].ParameterType;
                if (first == typeof(string) || first == typeof(TaggedString) || first == typeof(Letter))
                {
                    return true;
                }
            }

            return false;
        }

        static void IndexAlertTypes()
        {
            alertTypeNames.Clear();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type[] types;
                try
                {
                    types = assemblies[i].GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                catch (Exception)
                {
                    continue;
                }

                if (types == null)
                {
                    continue;
                }

                for (int t = 0; t < types.Length; t++)
                {
                    Type type = types[t];
                    if (type != null && type.Name.StartsWith("Alert_", StringComparison.Ordinal))
                    {
                        alertTypeNames.Add(type.Name);
                    }
                }
            }
        }

        static bool AlertPresent(string id)
        {
            if (id.NullOrEmpty())
            {
                return false;
            }

            if (alertTypeNames.Contains("Alert_" + id))
            {
                return true;
            }

            return string.Equals(id, "AquaponicBasinNotBreeding", StringComparison.Ordinal)
                && alertTypeNames.Contains("Alert_AquacultureBasinNotBreeding");
        }

        static void CollectMissingScreen(List<string> ids)
        {
            SuppressibleScreenMessageEntry[] all = SuppressibleScreenMessages.All;
            for (int i = 0; i < all.Length; i++)
            {
                if (!IsScreenRowAvailable(all[i]))
                {
                    ids.Add(all[i].Id);
                }
            }
        }

        static void CollectMissingLetters(List<string> ids, bool languageReady)
        {
            SuppressibleLetterEntry[] all = SuppressibleLetters.All;
            for (int i = 0; i < all.Length; i++)
            {
                if (!languageReady && !all[i].TranslationKey.NullOrEmpty()
                    && !all[i].IsSurgeryFailed
                    && !string.Equals(all[i].Id, "FallenMonolith", StringComparison.Ordinal)
                    && !string.Equals(all[i].Id, "StorytellerFrenzy", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!IsLetterRowAvailable(all[i]))
                {
                    ids.Add(all[i].Id);
                }
            }
        }

        static void CollectMissingAlerts(List<string> ids)
        {
            SuppressibleAlertEntry[] all = SuppressibleAlerts.All;
            for (int i = 0; i < all.Length; i++)
            {
                if (!IsAlertRowAvailable(all[i]))
                {
                    ids.Add(all[i].Id);
                }
            }
        }

        static void CollectMissingTools(List<string> ids)
        {
            ToolNeedEntry[] all = ToolNeedIgnore.All;
            for (int i = 0; i < all.Length; i++)
            {
                if (DefDatabase<StatDef>.GetNamedSilentFail(all[i].StatDefName) == null)
                {
                    ids.Add(all[i].StatDefName);
                }
            }
        }

        static void WarnIds(string key, string lead, List<string> ids)
        {
            if (ids.Count == 0)
            {
                return;
            }

            var text = new StringBuilder();
            text.Append("[HSK kebab tweaks] ");
            text.Append(lead);
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0)
                {
                    text.Append(", ");
                }

                text.Append(ids[i]);
            }

            text.Append('.');
            FeatureAvailability.WarnOnce(key, text.ToString());
        }
    }
}
