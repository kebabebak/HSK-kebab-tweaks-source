using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: One mod can report the same exception on every interface frame, so the rest of
    /// that frame never finishes. The log often names a wrapper method, not the mod that holds
    /// the missing member. Turning that mod off is enough for the colony to start.
    ///
    /// Fix: While the patch is on, identical errors are counted in a row. Twenty of the same
    /// exception, or eight of the same error when the wider checkbox is on, switches that mod
    /// off in the mod list and saves it. The current session keeps running; the mod stays off
    /// on the next launch. A quoted missing member is matched to the one mod assembly that
    /// contains that name. Otherwise the top of the stack is used, and only when it points at
    /// one mod. Core, official expansions, Core SK, Combat Extended, Harmony, HugsLib, and this
    /// mod are never switched off. Each decision writes one yellow warning.
    ///
    /// Проблема: Один мод может сообщать одно и то же исключение на каждом кадре интерфейса,
    /// и остаток этого кадра не выполняется. Лог часто называет метод-обёртку, а не мод, в
    /// котором отсутствует член. Выключения этого мода достаточно, чтобы колония запускалась.
    ///
    /// Исправление: Пока патч включён, одинаковые ошибки считаются подряд. Двадцать одинаковых
    /// исключений, или восемь одинаковых ошибок при широком чекбоксе, выключает этот мод в
    /// списке модов и сохраняет список. Текущая сессия продолжается; на следующем запуске мод
    /// остаётся выключенным. Имя отсутствующего члена в кавычках сопоставляется с единственной
    /// сборкой мода, где это имя есть. Иначе берётся вершина стека, и только если она указывает
    /// на один мод. Core, официальные дополнения, Core SK, Combat Extended, Harmony, HugsLib и
    /// этот мод не выключаются. Каждое решение пишет одно жёлтое предупреждение.
    /// </summary>
    public static class ParanoidPerfectionistFeatures
    {
        public const int CriticalRun = 20;
        public const int AnyRun = 8;

        static readonly object gate = new object();
        static readonly HashSet<string> decided = new HashSet<string>();
        static readonly Dictionary<string, string> memberOwner = new Dictionary<string, string>();
        static Dictionary<Assembly, ModContentPack> assemblyOwner;
        static string lastFingerprint;
        static string pendingPackage;
        static string pendingFingerprint;
        static int run;
        static int pendingSeen;
        static bool installed;
        static bool busy;

        /// <summary>
        /// Installs the error watcher once. The hook stays installed when the patch checkbox is
        /// off and returns immediately, so turning the patch on mid-session counts later errors.
        ///
        /// Ставит наблюдатель ошибок один раз. Хук остаётся, когда чекбокс патча выключен, и
        /// сразу выходит, поэтому включение патча посреди сессии считает уже следующие ошибки.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            if (harmony == null || installed)
            {
                return;
            }

            MethodInfo error = AccessTools.Method(typeof(Log), nameof(Log.Error), new[] { typeof(string) });
            if (error == null)
            {
                Log.Warning("[HSK kebab tweaks] Paranoid perfectionist found no Log.Error, so repeated errors are not watched.");
                return;
            }

            harmony.Patch(error, postfix: new HarmonyMethod(typeof(ParanoidPerfectionistFeatures), nameof(AfterError)));
            installed = true;
        }

        /// <summary>
        /// Counts one logged error. A finished run switches off the single mod tied to it.
        ///
        /// Считает одну записанную ошибку. Законченная серия выключает единственный мод,
        /// который к ней привязан.
        /// </summary>
        public static void AfterError(string text)
        {
            if (busy || string.IsNullOrEmpty(text))
            {
                return;
            }

            if (UnityData.IsInMainThread)
            {
                busy = true;
                try
                {
                    FlushPending();
                }
                catch (Exception ex)
                {
                    Log.Warning("[HSK kebab tweaks] Paranoid perfectionist could not switch a mod off: " + ex.Message);
                }
                finally
                {
                    busy = false;
                }
            }

            if (!KebabTweaksSettings.EnableParanoidPerfectionist)
            {
                return;
            }

            string fingerprint = Fingerprint(text);
            if (fingerprint.Length == 0 || fingerprint.StartsWith("[HSK kebab tweaks]", StringComparison.Ordinal))
            {
                return;
            }

            int seen;
            bool act;
            lock (gate)
            {
                if (fingerprint == lastFingerprint)
                {
                    run++;
                }
                else
                {
                    lastFingerprint = fingerprint;
                    run = 1;
                }

                int need = NeededRun(fingerprint);
                act = need > 0 && run >= need && decided.Add(fingerprint);
                seen = run;
            }

            if (!act)
            {
                return;
            }

            busy = true;
            try
            {
                string packageId = ChoosePackage(text);
                if (packageId == null)
                {
                    Log.Warning("[HSK kebab tweaks] Paranoid perfectionist left every mod on. The same error was reported "
                        + seen + " times in a row, and no single mod could be tied to it: " + fingerprint);
                    return;
                }

                TrySwitchOff(packageId, seen, fingerprint);
            }
            catch (Exception ex)
            {
                Log.Warning("[HSK kebab tweaks] Paranoid perfectionist could not switch a mod off: " + ex.Message);
            }
            finally
            {
                busy = false;
            }
        }

        static int NeededRun(string fingerprint)
        {
            bool exception = fingerprint.IndexOf("Exception", StringComparison.Ordinal) >= 0;
            if (KebabTweaksSettings.ParanoidCatchAnyError)
            {
                return AnyRun;
            }

            if (KebabTweaksSettings.ParanoidCatchCritical && exception)
            {
                return CriticalRun;
            }

            return 0;
        }

        static string Fingerprint(string text)
        {
            int end = text.IndexOf('\n');
            string line = end >= 0 ? text.Substring(0, end) : text;
            int reference = line.IndexOf("[Ref ", StringComparison.Ordinal);
            if (reference >= 0)
            {
                line = line.Substring(0, reference);
            }

            line = line.Trim();
            if (line.Length > 220)
            {
                line = line.Substring(0, 220);
            }

            return line;
        }

        /// <summary>
        /// Picks the mod that owns a repeated error. A missing member named in the message wins
        /// when exactly one mod assembly contains it. Several candidates, or none, switch nothing off.
        ///
        /// Выбирает мод, которому принадлежит повторяющаяся ошибка. Отсутствующий член, названный
        /// в сообщении, побеждает, когда его содержит ровно одна сборка мода. Несколько кандидатов
        /// или ни одного - ничего не выключается.
        /// </summary>
        static string ChoosePackage(string text)
        {
            if (TryQuotedMember(text, out string typeSimple, out string member))
            {
                string owner = PackageFromMember(typeSimple, member, out bool ambiguous);
                if (ambiguous)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(owner))
                {
                    return owner;
                }
            }

            List<string> fromText = PackagesFromFrames(text);
            if (fromText.Count == 1)
            {
                return fromText[0];
            }

            if (fromText.Count > 1)
            {
                return null;
            }

            List<string> fromCaller = PackagesFromFrames(Environment.StackTrace);
            if (fromCaller.Count == 1)
            {
                return fromCaller[0];
            }

            return null;
        }

        static void TrySwitchOff(string packageId, int seen, string fingerprint)
        {
            ModContentPack pack = FindPack(packageId);
            string label = pack != null && !string.IsNullOrEmpty(pack.Name) ? pack.Name : packageId;
            if (pack == null || IsProtected(pack))
            {
                Log.Warning("[HSK kebab tweaks] Paranoid perfectionist left " + label
                    + " on. The same error was reported " + seen
                    + " times in a row, and this mod stays enabled: " + fingerprint);
                return;
            }

            if (!UnityData.IsInMainThread)
            {
                lock (gate)
                {
                    pendingPackage = packageId;
                    pendingSeen = seen;
                    pendingFingerprint = fingerprint;
                }

                return;
            }

            if (!ModsConfig.IsActive(packageId))
            {
                return;
            }

            ModsConfig.SetActive(packageId, false);
            ModsConfig.Save();
            Log.Warning("[HSK kebab tweaks] Paranoid perfectionist will switch off " + label
                + " (" + packageId + ") on the next launch. The same error was reported " + seen
                + " times in a row: " + fingerprint);
        }

        static void FlushPending()
        {
            string packageId;
            string fingerprint;
            int seen;
            lock (gate)
            {
                packageId = pendingPackage;
                if (string.IsNullOrEmpty(packageId))
                {
                    return;
                }

                seen = pendingSeen;
                fingerprint = pendingFingerprint;
                pendingPackage = null;
                pendingFingerprint = null;
                pendingSeen = 0;
            }

            TrySwitchOff(packageId, seen, fingerprint);
        }

        static bool IsProtected(ModContentPack pack)
        {
            if (pack.IsCoreMod || pack.IsOfficialMod)
            {
                return true;
            }

            string id = pack.PackageId ?? "";
            if (id.StartsWith("ludeon.rimworld", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return id.Equals("skyarkhangel.hsk", StringComparison.OrdinalIgnoreCase)
                || id.Equals("ceteam.combatextended", StringComparison.OrdinalIgnoreCase)
                || id.Equals("brrainz.harmony", StringComparison.OrdinalIgnoreCase)
                || id.Equals("unlimitedhugs.hugslib", StringComparison.OrdinalIgnoreCase)
                || id.Equals("kebabebak.hsk.kebab.tweaks", StringComparison.OrdinalIgnoreCase);
        }

        static ModContentPack FindPack(string packageId)
        {
            if (LoadedModManager.RunningMods == null)
            {
                return null;
            }

            foreach (ModContentPack pack in LoadedModManager.RunningMods)
            {
                if (pack != null && string.Equals(pack.PackageId, packageId, StringComparison.OrdinalIgnoreCase))
                {
                    return pack;
                }
            }

            return null;
        }

        static List<string> PackagesFromFrames(string text)
        {
            List<string> found = new List<string>();
            if (string.IsNullOrEmpty(text))
            {
                return found;
            }

            EnsureAssemblyMap();
            int seen = 0;
            int start = 0;
            while (start < text.Length && seen < 12)
            {
                int end = text.IndexOf('\n', start);
                if (end < 0)
                {
                    end = text.Length;
                }

                string typeName = TypeNameFromFrame(text.Substring(start, end - start));
                start = end + 1;
                if (typeName == null)
                {
                    continue;
                }

                seen++;
                Type type = AccessTools.TypeByName(typeName);
                if (type == null || assemblyOwner == null || !assemblyOwner.TryGetValue(type.Assembly, out ModContentPack pack))
                {
                    continue;
                }

                if (IsProtected(pack))
                {
                    continue;
                }

                string id = pack.PackageId;
                if (!found.Contains(id))
                {
                    found.Add(id);
                }
            }

            return found;
        }

        static string TypeNameFromFrame(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return null;
            }

            string trimmed = line.Trim();
            if (trimmed.StartsWith("at ", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(3);
            }
            else
            {
                int at = trimmed.IndexOf(" at ", StringComparison.Ordinal);
                if (at < 0)
                {
                    return null;
                }

                trimmed = trimmed.Substring(at + 4);
            }

            int paren = trimmed.IndexOf('(');
            if (paren > 0)
            {
                trimmed = trimmed.Substring(0, paren);
            }

            int lastDot = trimmed.LastIndexOf('.');
            if (lastDot <= 0)
            {
                return null;
            }

            return trimmed.Substring(0, lastDot);
        }

        static string PackageFromMember(string typeSimple, string member, out bool ambiguous)
        {
            ambiguous = false;
            string key = typeSimple + "." + member;
            lock (gate)
            {
                if (memberOwner.TryGetValue(key, out string cached))
                {
                    ambiguous = cached == "?";
                    return ambiguous ? null : cached;
                }
            }

            string found = null;
            int hits = 0;
            if (LoadedModManager.RunningMods != null)
            {
                byte[] typeBytes = Encoding.ASCII.GetBytes(typeSimple);
                byte[] memberBytes = Encoding.ASCII.GetBytes(member);
                foreach (ModContentPack pack in LoadedModManager.RunningMods)
                {
                    if (pack?.assemblies?.loadedAssemblies == null || IsProtected(pack))
                    {
                        continue;
                    }

                    bool packHit = false;
                    foreach (Assembly assembly in pack.assemblies.loadedAssemblies)
                    {
                        string path = assembly?.Location;
                        if (string.IsNullOrEmpty(path) || !File.Exists(path))
                        {
                            continue;
                        }

                        byte[] file;
                        try
                        {
                            file = File.ReadAllBytes(path);
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (IndexOfBytes(file, typeBytes) >= 0 && IndexOfBytes(file, memberBytes) >= 0)
                        {
                            packHit = true;
                            break;
                        }
                    }

                    if (!packHit)
                    {
                        continue;
                    }

                    hits++;
                    found = pack.PackageId;
                    if (hits > 1)
                    {
                        break;
                    }
                }
            }

            ambiguous = hits > 1;
            lock (gate)
            {
                memberOwner[key] = ambiguous ? "?" : (found ?? "");
            }

            return ambiguous ? null : found;
        }

        static bool TryQuotedMember(string text, out string typeSimple, out string member)
        {
            typeSimple = null;
            member = null;
            int search = 0;
            while (search < text.Length)
            {
                int open = text.IndexOf('\'', search);
                if (open < 0)
                {
                    return false;
                }

                int close = text.IndexOf('\'', open + 1);
                if (close < 0)
                {
                    return false;
                }

                string inner = text.Substring(open + 1, close - open - 1);
                search = close + 1;
                int dot = inner.LastIndexOf('.');
                if (dot <= 0 || dot >= inner.Length - 1)
                {
                    continue;
                }

                string memberName = inner.Substring(dot + 1);
                string typeName = inner.Substring(0, dot);
                int typeDot = typeName.LastIndexOf('.');
                string simple = typeDot >= 0 ? typeName.Substring(typeDot + 1) : typeName;
                if (memberName.Length < 3 || simple.Length < 3)
                {
                    continue;
                }

                typeSimple = simple;
                member = memberName;
                return true;
            }

            return false;
        }

        static void EnsureAssemblyMap()
        {
            if (assemblyOwner != null || LoadedModManager.RunningMods == null)
            {
                return;
            }

            Dictionary<Assembly, ModContentPack> map = new Dictionary<Assembly, ModContentPack>();
            foreach (ModContentPack pack in LoadedModManager.RunningMods)
            {
                if (pack?.assemblies?.loadedAssemblies == null)
                {
                    continue;
                }

                foreach (Assembly assembly in pack.assemblies.loadedAssemblies)
                {
                    if (assembly != null && !map.ContainsKey(assembly))
                    {
                        map.Add(assembly, pack);
                    }
                }
            }

            assemblyOwner = map;
        }

        static int IndexOfBytes(byte[] haystack, byte[] needle)
        {
            if (needle.Length == 0 || haystack.Length < needle.Length)
            {
                return -1;
            }

            int last = haystack.Length - needle.Length;
            for (int i = 0; i <= last; i++)
            {
                int n = 0;
                while (n < needle.Length && haystack[i + n] == needle[n])
                {
                    n++;
                }

                if (n == needle.Length)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
