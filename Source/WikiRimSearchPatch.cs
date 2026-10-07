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
    /// Problem: WikiRim HelpDef.MatchesFilter compares only the active-language label, so a
    /// query in English misses a Russian page and the other way around. The header enable
    /// alone does not change that.
    ///
    /// Fix: when multilingual search is on, a postfix also checks the source def label in the
    /// active language, English, and Russian. The hook is installed only when ModsConfig lists
    /// mlie.wikirim and HelpTab.HelpDef is loaded from that mod folder.
    ///
    /// Проблема: WikiRim HelpDef.MatchesFilter сравнивает только подпись активного языка, поэтому
    /// запрос на английском не находит русскую страницу и наоборот. Одно включение заголовка
    /// это не меняет.
    ///
    /// Исправление: при включённом мультиязыковом поиске postfix дополнительно проверяет подпись
    /// исходного def на активном языке, английском и русском. Хук ставится только если в
    /// ModsConfig есть mlie.wikirim и HelpTab.HelpDef загружен из папки этого мода.
    /// </summary>
    public static class WikiRimCompatFeatures
    {
        public const string PackageId = "mlie.wikirim";

        private static FieldInfo keyDefField;
        private static readonly Dictionary<object, string> Haystacks = new Dictionary<object, string>();
        private static readonly HashSet<LoadedLanguage> LanguagesLoaded = new HashSet<LoadedLanguage>();
        private static string haystackLanguage = string.Empty;

        public static void Apply(Harmony harmony)
        {
            if (!IsPackageActive())
            {
                Log.Message("[HSK kebab tweaks] WikiRimCompat skipped (mlie.wikirim is not active).");
                return;
            }

            Type type = AccessTools.TypeByName("HelpTab.HelpDef");
            MethodInfo method = type == null ? null : AccessTools.Method(type, "MatchesFilter");
            if (method == null || !AssemblyIsThisMod(type))
            {
                Log.Message("[HSK kebab tweaks] WikiRimCompat skipped (HelpTab.HelpDef is not from mlie.wikirim).");
                return;
            }

            keyDefField = AccessTools.Field(type, "keyDef");
            harmony.Patch(method, postfix: new HarmonyMethod(
                typeof(WikiRimCompatFeatures),
                nameof(HelpDef_MatchesFilter)));
            Log.Message("[HSK kebab tweaks] WikiRimCompat loaded.");
        }

        /// <summary>
        /// ModsConfig matches package ids in lowercase, so this checks mlie.wikirim.
        ///
        /// ModsConfig сравнивает package id в нижнем регистре, поэтому здесь проверяется mlie.wikirim.
        /// </summary>
        public static bool IsPackageActive()
        {
            return ModsConfig.IsActive(PackageId);
        }

        /// <summary>
        /// True when this HelpTab type was loaded from the mlie.wikirim mod folder.
        ///
        /// Истина, если этот тип HelpTab загружен из папки мода mlie.wikirim.
        /// </summary>
        private static bool AssemblyIsThisMod(Type type)
        {
            if (type == null)
            {
                return false;
            }

            string location = type.Assembly.Location;
            if (location.NullOrEmpty())
            {
                return false;
            }

            ModMetaData meta = ModLister.GetActiveModWithIdentifier(PackageId);
            DirectoryInfo rootDir = meta?.RootDir;
            if (rootDir == null)
            {
                return false;
            }

            string root = Path.GetFullPath(rootDir.FullName)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string file = Path.GetFullPath(location);
            if (file.Length < root.Length
                || !file.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return file.Length == root.Length
                || file[root.Length] == Path.DirectorySeparatorChar
                || file[root.Length] == Path.AltDirectorySeparatorChar;
        }

        public static void HelpDef_MatchesFilter(object __instance, string filter, ref bool __result)
        {
            if (__result || filter.NullOrEmpty() || __instance == null)
            {
                return;
            }

            if (!KebabTweaksSettings.EnableWikiRimCompat
                || !KebabTweaksSettings.EnableWikiRimMultilingualSearch
                || !IsPackageActive())
            {
                return;
            }

            if (Haystack(__instance).IndexOf(filter.Trim(), StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                __result = true;
            }
        }

        private static string Haystack(object helpDef)
        {
            string languageName = LanguageDatabase.activeLanguage?.folderName ?? string.Empty;
            if (!string.Equals(haystackLanguage, languageName, StringComparison.Ordinal))
            {
                Haystacks.Clear();
                haystackLanguage = languageName;
            }

            if (Haystacks.TryGetValue(helpDef, out string cached))
            {
                return cached;
            }

            Def source = keyDefField == null ? null : keyDefField.GetValue(helpDef) as Def;
            StringBuilder text = new StringBuilder();
            Append(text, ReplacedEnglish(source));
            foreach (LoadedLanguage language in IndexedLanguages())
            {
                Append(text, LabelIn(language, source));
            }

            string built = text.ToString();
            Haystacks[helpDef] = built;
            return built;
        }

        private static void Append(StringBuilder text, string line)
        {
            if (line.NullOrEmpty())
            {
                return;
            }

            if (text.Length > 0)
            {
                text.Append('\n');
            }

            text.Append(line);
        }

        private static string LabelIn(LoadedLanguage language, Def def)
        {
            if (language == null || def == null || def.defName.NullOrEmpty())
            {
                return null;
            }

            EnsureLanguageLoaded(language);
            for (Type type = def.GetType(); type != null && type != typeof(Def); type = type.BaseType)
            {
                DefInjectionPackage package = Package(language, type);
                if (package?.injections == null)
                {
                    continue;
                }

                if (!package.injections.TryGetValue(def.defName + ".label", out DefInjectionPackage.DefInjection injection))
                {
                    continue;
                }

                if (injection == null || injection.isPlaceholder || injection.injection.NullOrEmpty())
                {
                    continue;
                }

                return injection.injection;
            }

            return null;
        }

        /// <summary>
        /// English source label kept on the active language injection when that language replaced it.
        ///
        /// Исходная английская подпись, которую активный язык сохранил в injection при замене.
        /// </summary>
        private static string ReplacedEnglish(Def def)
        {
            LoadedLanguage active = LanguageDatabase.activeLanguage;
            if (def == null || active == null || IsEnglish(active) || def.defName.NullOrEmpty())
            {
                return null;
            }

            EnsureLanguageLoaded(active);
            for (Type type = def.GetType(); type != null && type != typeof(Def); type = type.BaseType)
            {
                DefInjectionPackage package = Package(active, type);
                if (package?.injections == null
                    || !package.injections.TryGetValue(def.defName + ".label", out DefInjectionPackage.DefInjection injection)
                    || injection == null)
                {
                    continue;
                }

                return injection.replacedString;
            }

            return null;
        }

        private static DefInjectionPackage Package(LoadedLanguage language, Type defType)
        {
            List<DefInjectionPackage> packages = language.defInjections;
            if (packages == null || defType == null)
            {
                return null;
            }

            for (int i = 0; i < packages.Count; i++)
            {
                if (packages[i] != null && packages[i].defType == defType)
                {
                    return packages[i];
                }
            }

            return null;
        }

        private static void EnsureLanguageLoaded(LoadedLanguage language)
        {
            if (language == null || !LanguagesLoaded.Add(language))
            {
                return;
            }

            language.TryGetTextFromKey("OK", out _);
        }

        private static List<LoadedLanguage> IndexedLanguages()
        {
            List<LoadedLanguage> languages = new List<LoadedLanguage>();
            LoadedLanguage active = LanguageDatabase.activeLanguage;
            if (LanguageDatabase.AllLoadedLanguages == null)
            {
                return languages;
            }

            foreach (LoadedLanguage language in LanguageDatabase.AllLoadedLanguages)
            {
                if (language == null)
                {
                    continue;
                }

                if (ReferenceEquals(language, active) || IsEnglish(language) || IsRussian(language))
                {
                    languages.Add(language);
                }
            }

            return languages;
        }

        private static bool IsEnglish(LoadedLanguage language)
        {
            return NameStartsWith(language, "English");
        }

        private static bool IsRussian(LoadedLanguage language)
        {
            return NameStartsWith(language, "Russian");
        }

        private static bool NameStartsWith(LoadedLanguage language, string prefix)
        {
            string folder = language.folderName;
            if (!folder.NullOrEmpty()
                && folder.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string friendly = language.FriendlyNameEnglish;
            return !friendly.NullOrEmpty()
                && friendly.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
