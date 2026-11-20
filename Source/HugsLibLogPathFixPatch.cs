using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: HugsLib.Utils.HugsLibUtility.TryGetLogFilePath on Windows returns
    /// Application.persistentDataPath plus Player.log. Copying the log then throws
    /// FileNotFoundException from HugsLib.Logs.LogPublisher.GetLogFileContents when the
    /// game is writing that file under a Logs folder instead. Unity 1.5 and 1.6 still
    /// write Player.log at the path this method returns, so the copy succeeds there.
    ///
    /// Fix: A postfix on that method returns the file the game is writing when the
    /// returned path is a different file. The game file is Application.consoleLogPath
    /// when that file exists, otherwise Logs\Player.log under persistentDataPath when
    /// that file exists. The probe is Closed when the unpatched method already returns
    /// that same file, which covers Unity and a HugsLib build that already reads
    /// consoleLogPath. The row is veiled when HugsLib is not loaded. The hook is not
    /// installed while the probe is Closed and the leftover checkbox is off.
    ///
    /// Проблема: HugsLib.Utils.HugsLibUtility.TryGetLogFilePath на Windows возвращает
    /// Application.persistentDataPath плюс Player.log. Копирование лога тогда бросает
    /// FileNotFoundException из HugsLib.Logs.LogPublisher.GetLogFileContents, если игра
    /// пишет этот файл в папку Logs. Unity 1.5 и 1.6 по-прежнему пишут Player.log по
    /// пути этого метода, и копирование там проходит.
    ///
    /// Исправление: постфикс этого метода возвращает файл, который пишет игра, если
    /// возвращённый путь - другой файл. Файл игры - это Application.consoleLogPath,
    /// когда этот файл есть, иначе Logs\Player.log под persistentDataPath, когда есть
    /// он. Зонд Closed, когда незапатченный метод уже возвращает тот же файл: это
    /// Unity и сборка HugsLib, которая уже читает consoleLogPath. Без HugsLib строка
    /// под вуалью. Хук не ставится, пока зонд Closed и leftover-чекбокс выключен.
    /// </summary>
    public static class HugsLibLogPathFixFeatures
    {
        const string UtilityTypeName = "HugsLib.Utils.HugsLibUtility";
        const string PathMethodName = "TryGetLogFilePath";
        const string HugsLibPackageId = "UnlimitedHugs.HugsLib";

        static bool loadFinished;

        /// <summary>
        /// After this, a missing HugsLib type is a finished answer. Before it, the probe retries.
        ///
        /// После этого отсутствие типа HugsLib - законченный ответ. До этого зонд повторяет проверку.
        /// </summary>
        public static void NoteLoadFinished()
        {
            loadFinished = true;
        }

        /// <summary>
        /// Veils the row when HugsLib is not loaded. Called even if the checkbox is off.
        ///
        /// Вешает вуаль, если HugsLib не загружен. Вызывается и при выключенном чекбоксе.
        /// </summary>
        public static void NoteIfMissing()
        {
            if (AccessTools.TypeByName(UtilityTypeName) != null)
            {
                return;
            }

            FeatureAvailability.SkipMissingMods(
                FeatureAvailability.HugsLibLogPath,
                "Log Copy Fix",
                "HugsLib (UnlimitedHugs.HugsLib)");
        }

        /// <summary>
        /// Points TryGetLogFilePath at the file the game is writing.
        ///
        /// Направляет TryGetLogFilePath на файл, который пишет игра.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            if (harmony == null ||
                FeatureAvailability.IsBlocked(FeatureAvailability.HugsLibLogPath))
            {
                return;
            }

            MethodInfo method = PathMethod();
            if (method == null)
            {
                NoteIfMissing();
                return;
            }

            harmony.Patch(method, postfix: new HarmonyMethod(typeof(HugsLibLogPathFixFeatures), nameof(TryGetLogFilePath_Postfix)));
            Log.Message("[HSK kebab tweaks] Log Copy Fix applied.");
        }

        /// <summary>
        /// Null when the log file is not visible yet, or HugsLib is in the list but its
        /// type is not loaded yet, so the probe retries. False when HugsLib is absent after
        /// load, or its path is not the file the game is writing. True when the unpatched
        /// method already returns that file, or its IL calls Application.get_consoleLogPath.
        ///
        /// Null, если файл лога ещё не виден, или HugsLib есть в списке, но тип ещё не
        /// загружен, и зонд повторит проверку. False, если после загрузки HugsLib нет или
        /// его путь - не файл, который пишет игра. True, если незапатченный метод уже
        /// возвращает этот файл или его IL вызывает Application.get_consoleLogPath.
        /// </summary>
        public static bool? ReadClosed()
        {
            if (AccessTools.TypeByName(UtilityTypeName) == null)
            {
                if (!loadFinished && ModsConfig.IsActive(HugsLibPackageId))
                {
                    return null;
                }

                return false;
            }

            MethodInfo method = PathMethod();
            if (method == null)
            {
                return false;
            }

            if (CallsConsoleLogPath(method))
            {
                return true;
            }

            string actual = GamePlayerLogPath();
            if (string.IsNullOrEmpty(actual))
            {
                return null;
            }

            string reported;
            try
            {
                reported = method.Invoke(null, null) as string;
            }
            catch (Exception)
            {
                return false;
            }

            return SamePath(reported, actual);
        }

        /// <summary>
        /// Replaces a path that is not the file the game is writing.
        ///
        /// Подменяет путь, который не является файлом, который пишет игра.
        /// </summary>
        public static void TryGetLogFilePath_Postfix(ref string __result)
        {
            if (!KebabTweaksSettings.IsHugsLibLogPathFixEnabled())
            {
                return;
            }

            string actual = GamePlayerLogPath();
            if (string.IsNullOrEmpty(actual) || SamePath(__result, actual))
            {
                return;
            }

            __result = actual;
        }

        static MethodInfo PathMethod()
        {
            Type type = AccessTools.TypeByName(UtilityTypeName);
            if (type == null)
            {
                return null;
            }

            MethodInfo method = AccessTools.Method(type, PathMethodName, Type.EmptyTypes);
            if (method == null || method.ReturnType != typeof(string))
            {
                return null;
            }

            return method;
        }

        static string GamePlayerLogPath()
        {
            string console = ConsoleLogPath();
            if (!string.IsNullOrEmpty(console) && File.Exists(console))
            {
                return console;
            }

            string root = Application.persistentDataPath;
            if (string.IsNullOrEmpty(root))
            {
                return null;
            }

            string underLogs = Path.Combine(root, "Logs", "Player.log");
            if (File.Exists(underLogs))
            {
                return underLogs;
            }

            string plain = Path.Combine(root, "Player.log");
            if (File.Exists(plain))
            {
                return plain;
            }

            return null;
        }

        static string ConsoleLogPath()
        {
            PropertyInfo property = AccessTools.Property(typeof(Application), "consoleLogPath");
            if (property == null || !property.CanRead)
            {
                return null;
            }

            try
            {
                return property.GetValue(null, null) as string;
            }
            catch (Exception)
            {
                return null;
            }
        }

        static bool CallsConsoleLogPath(MethodInfo method)
        {
            byte[] il = method.GetMethodBody()?.GetILAsByteArray();
            if (il == null)
            {
                return false;
            }

            Module module = method.Module;
            for (int i = 0; i < il.Length - 4; i++)
            {
                byte op = il[i];
                if (op != 0x28 && op != 0x6F)
                {
                    continue;
                }

                int token = BitConverter.ToInt32(il, i + 1);
                try
                {
                    MethodBase resolved = module.ResolveMethod(token);
                    if (resolved != null &&
                        resolved.Name == "get_consoleLogPath" &&
                        resolved.DeclaringType == typeof(Application))
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

        static bool SamePath(string left, string right)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
            {
                return false;
            }

            try
            {
                return string.Equals(
                    Path.GetFullPath(left),
                    Path.GetFullPath(right),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
