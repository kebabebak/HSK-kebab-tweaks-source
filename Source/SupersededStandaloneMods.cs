using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Former standalone kebabebak mods/patches superseded by this combined mod. Used for
    /// About incompatibleWith, silent skip of bundled Harmony, and settings header styling.
    ///
    /// Бывшие отдельные моды/патчи kebabebak, заменённые этим комбинированным модом. Для
    /// incompatibleWith в About, тихого skip Harmony и оформления заголовков в настройках.
    /// </summary>
    public static class SupersededStandaloneMods
    {
        public const string KebabSwitches = "kebabebak.HSKKebabSwitches";
        public const string CatCrazyTime = "kebabebak.cat.crazytime.load.patch";
        public const string CatFloorSleep = "kebabebak.cat.floor.sleep.patch";
        public const string ArmorRacksAssignFix = "kebabebak.armorracks.assign.fix.patch";
        public const string CeRunForCoverDestFix = "kebabebak.ce.runforcover.dest.fix.patch";
        public const string FishTableTypeListFix = "kebabebak.fishtable.typelist.fix.patch";
        public const string GetActiveRitualsFix = "kebabebak.get.active.rituals.fix.patch";
        public const string GrowerCutTrees = "kebabebak.grower.cut.trees.patch";
        public const string IdleErrorWanderFix = "kebabebak.idle.error.wander.fix.patch";
        public const string LowTpsPawnDump = "kebabebak.low.tps.pawn.dump";
        public const string PtgMedicalCare = "kebabebak.ptg.medical.care.patch";
        public const string SeedsPleaseSowFix = "kebabebak.seedsplease.sow.fix.patch";
        public const string TakeFromMending = "kebabebak.takefrom.mending.patch";
        public const string TradeCaravanLordFix = "kebabebak.trade.caravan.lord.fix.patch";
        public const string NeanderthalChiefLeaderFix = "kebabebak.neanderthal.chief.leader.fix.patch";
        public const string MapPreviewRngBaselineFix = "kebabebak.map.preview.rng.baseline.patch";
        public const string UnifiedXmlPathFix = "kebabebak.unified.xml.path.fix.patch";
        public const string MainMenuBgFitFix = "kebabebak.main.menu.bg.fit.patch";
        public const string RimatomicsGuidancePanelFix = "kebabebak.rimatomics.guidance.panel.fix";
        public const string OdysseyRuTradersGuildNamerFix = "kebabebak.odyssey.ru.traders.guild.namer.fix.patch";

        /// <summary>
        /// True when the standalone package is active in the current mod list.
        ///
        /// True, если отдельный package активен в текущем списке модов.
        /// </summary>
        public static bool IsActive(string packageId)
        {
            return !packageId.NullOrEmpty() &&
                   ModLister.GetActiveModWithIdentifier(packageId) != null;
        }
    }
}
