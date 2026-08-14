using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: when GenPlace.TryPlaceThing fails after crafting, vanilla logs
    /// "could not drop recipe product" and the item is not placed; players have no persistent tally.
    ///
    /// Fix: when enabled, append each such failure to RecipeProductDropLost.log under
    /// GenFilePaths.SaveDataFolderPath/KebabTweaks/ (same folder RimWorld uses for saves/config).
    ///
    /// Проблема: при неудачном GenPlace.TryPlaceThing после крафта vanilla пишет
    /// «could not drop recipe product», предмет не кладётся; у игрока нет накопительного учёта.
    ///
    /// Исправление: при включении дописывает каждый такой случай в RecipeProductDropLost.log
    /// в GenFilePaths.SaveDataFolderPath/KebabTweaks/ (та же папка данных, что у сейвов).
    /// </summary>
    public static class RecipeProductDropLostLogFeatures
    {
        private const string LogFileName = "RecipeProductDropLost.log";
        private const string LogSubfolder = "KebabTweaks";

        private static readonly Regex DropFailedRegex = new Regex(
            @"^(?<pawn>.+?) could not drop recipe product (?<thing>.+?) near \((?<x>-?\d+), (?<y>-?\d+), (?<z>-?\d+)\)$",
            RegexOptions.Compiled);

        public static void Apply(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    AccessTools.Method(typeof(Log), nameof(Log.Error), new[] { typeof(string) }),
                    prefix: new HarmonyMethod(typeof(RecipeProductDropLostLogFeatures), nameof(Log_Error_Prefix)));

                Log.Message("[RecipeProductDropLostLogPatch] Patches applied (recipe drop failure file log).");
            }
            catch (Exception ex)
            {
                Log.Error("[RecipeProductDropLostLogPatch] Failed to apply patches: " + ex);
            }
        }

        /// <summary>
        /// Records matching vanilla recipe-drop errors to the KebabTweaks log file.
        ///
        /// Записывает совпадающие ошибки vanilla о drop recipe product в лог KebabTweaks.
        /// </summary>
        public static void Log_Error_Prefix(string text)
        {
            if (!KebabTweaksSettings.EnableRecipeProductDropLostLog || text.NullOrEmpty())
            {
                return;
            }

            Match match = DropFailedRegex.Match(text.Trim());
            if (!match.Success)
            {
                return;
            }

            try
            {
                AppendRecord(
                    match.Groups["pawn"].Value,
                    match.Groups["thing"].Value,
                    match.Groups["x"].Value,
                    match.Groups["y"].Value,
                    match.Groups["z"].Value);
            }
            catch (Exception ex)
            {
                Log.Warning("[RecipeProductDropLostLogPatch] Failed to write log entry: " + ex.Message);
            }
        }

        private static void AppendRecord(string pawnLabel, string thingLabel, string x, string y, string z)
        {
            string folder = Path.Combine(GenFilePaths.SaveDataFolderPath, LogSubfolder);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, LogFileName);

            StringBuilder line = new StringBuilder();
            line.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            line.Append('\t');
            line.Append("tick=").Append(Find.TickManager.TicksGame);
            if (Find.CurrentMap != null)
            {
                line.Append('\t').Append("map=").Append(Find.CurrentMap.Parent?.LabelCap ?? Find.CurrentMap.uniqueID.ToString());
            }

            line.Append('\t').Append("pawn=").Append(pawnLabel);
            line.Append('\t').Append("thing=").Append(thingLabel);
            line.Append('\t').Append("cell=(").Append(x).Append(", ").Append(y).Append(", ").Append(z).Append(')');

            File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
        }
    }
}
