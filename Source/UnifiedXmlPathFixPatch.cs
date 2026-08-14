#if RIMWORLD_1_6
using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Unified Xml Export default path ./Mods/Unified.xml resolves against launcher CWD
    /// when RimWorld starts via HSK Launcher, causing DirectoryNotFoundException on every mod load.
    ///
    /// Fix: Prefix on ParseAndProcessXML_postfix rewrites relative ./Mods paths onto
    /// GenFilePaths.ModsFolderPath and creates the parent directory. Must apply in Mod ctor before
    /// LoadAllActiveMods. Soft-skips when Unified Xml Export is absent. Restart to toggle.
    ///
    /// Проблема: UXE с путём ./Mods/Unified.xml относительно CWD лаунчера даёт
    /// DirectoryNotFoundException при каждой загрузке модов.
    ///
    /// Исправление: Prefix на ParseAndProcessXML_postfix переписывает ./Mods на
    /// GenFilePaths.ModsFolderPath и создаёт каталог. Применение в ctor Mod до LoadAllActiveMods.
    /// Без UXE — пропуск. Переключение — после рестарта.
    /// </summary>
    public static class UnifiedXmlPathFixFeatures
    {
        public static void ApplyEarly(Harmony harmony)
        {
            Type patchesType = AccessTools.TypeByName("UnifiedXmlExport.LoadedModManagerPatches");
            if (patchesType == null)
            {
                Log.Message("[HSK kebab tweaks] Unified Xml Export not loaded; UXE path fix skipped.");
                return;
            }

            MethodInfo target = AccessTools.Method(patchesType, "ParseAndProcessXML_postfix");
            if (target == null)
            {
                Log.Warning("[HSK kebab tweaks] UXE ParseAndProcessXML_postfix not found; path fix skipped.");
                return;
            }

            harmony.Patch(
                target,
                prefix: new HarmonyMethod(typeof(UnifiedXmlExportPath_Patch), nameof(UnifiedXmlExportPath_Patch.Prefix)));

            Log.Message(
                "[HSK kebab tweaks] Unified Xml Export path fix loaded (relative paths via GenFilePaths.ModsFolderPath).");
        }
    }

    internal static class UnifiedXmlExportPath_Patch
    {
        private const string DefaultRelativePath = "./Mods/Unified.xml";
        private const string DefaultFileName = "Unified.xml";

        public static void Prefix()
        {
            try
            {
                Type modType = AccessTools.TypeByName("UnifiedXmlExport.Mod");
                if (modType == null)
                {
                    return;
                }

                PropertyInfo settingsProp = AccessTools.Property(modType, "Settings");
                object settings = settingsProp?.GetValue(null, null);
                if (settings == null)
                {
                    return;
                }

                FieldInfo pathField = AccessTools.Field(settings.GetType(), "xmlExportPath");
                if (pathField == null)
                {
                    return;
                }

                string path = pathField.GetValue(settings) as string;
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = DefaultRelativePath;
                }

                string resolved = ResolveExportPath(path);
                if (!string.Equals(path, resolved, StringComparison.OrdinalIgnoreCase))
                {
                    pathField.SetValue(settings, resolved);
                    Log.Message("[HSK kebab tweaks] UXE export path: " + path + " -> " + resolved);
                }

                string directory = Path.GetDirectoryName(resolved);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("[HSK kebab tweaks] Could not prepare UXE export path: " + ex.Message);
            }
        }

        internal static string ResolveExportPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            string normalized = path.Replace('\\', '/').Trim();
            while (normalized.StartsWith("./", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(2);
            }

            if (normalized.StartsWith("Mods/", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "Mods", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileName(normalized.Replace('/', Path.DirectorySeparatorChar));
                if (string.IsNullOrEmpty(fileName)
                    || string.Equals(fileName, "Mods", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = DefaultFileName;
                }

                return Path.Combine(GenFilePaths.ModsFolderPath, fileName);
            }

            string gameRoot = Path.GetDirectoryName(GenFilePaths.ModsFolderPath);
            if (string.IsNullOrEmpty(gameRoot))
            {
                return Path.GetFullPath(path);
            }

            return Path.GetFullPath(Path.Combine(gameRoot, path));
        }
    }
}
#endif
