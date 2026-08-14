using System;
using System.Collections.Generic;
using HSK.KebabTweaks.KebabSwitches;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Combined mod settings: kebab switches (no master enable) plus per-patch enable headers
    /// and each patch's own options / logging.
    ///
    /// Общие настройки мода: kebab switches (без master enable) и заголовки-enable патчей
    /// плюс собственные опции / логирование каждого патча.
    /// </summary>
    [StaticConstructorOnStartup]
    public class KebabTweaksSettings : ModSettings
    {
        public const float DefaultLowTpsThreshold = 100f;
        public const float DefaultLowTpsLogCooldownSeconds = 1f;
        public const int DefaultIdleWorkSearchCooldownTicks = 1000;
        public const int MaxIdleWorkSearchCooldownTicks = 100000;

        public static bool EnableCatCrazyTime = true;
        public static bool EnableCatFloorSleep = true;
        public static bool EnableCeRunForCoverDestFix = true;
        public static bool EnableCeProjectileNullSoundFix = true;
        public static bool EnableArmorRacksAssignFix = true;
        public static bool EnableStorageSettingsAllowedToAcceptFix = true;
        public static bool EnableFishTableTypeListFix = true;
        public static bool EnableGetActiveRitualsFix = true;
        public static bool EnableGrowerCutTrees = true;
        public static bool EnableIdleErrorWanderFix = true;
        public static bool EnableHospitalityGuestApparelOptimizeFix = true;
        public static bool EnableLowTpsPawnDump = true;
        public static bool EnableIdleWorkSearchCooldown = true;
        public static bool EnablePtgMedicalCare = true;
        public static bool EnableSeedsPleaseSowFix = true;
        public static bool EnableTakeFromMending = true;
        public static bool EnableTradeCaravanLordFix = true;
        public static bool EnableApparelPolicyLoadFix = true;
        public static bool EnableBillRenamePrefill = true;
        public static bool EnableRecipeProductDropLostLog = true;
        public static bool EnableAllowToolHaulUrgentlyNreFix = true;
        public static bool EnableDubsAnalyzerBeginUpdateFix = true;
        public static bool EnableDebugLogSplitterDragFix = true;
#if RIMWORLD_1_6
        public static bool EnableNeanderthalChiefLeaderFix = true;
        public static bool EnableMapPreviewRngBaselineFix = true;
        public static bool EnableUnifiedXmlPathFix = true;
        public static bool EnableMainMenuBgFitFix = true;
        public static bool EnableRimatomicsGuidancePanelFix = true;
#endif

        public static bool AppliedCatCrazyTime = true;
        public static bool AppliedCatFloorSleep = true;
        public static bool AppliedCeRunForCoverDestFix = true;
        public static bool AppliedCeProjectileNullSoundFix = true;
        public static bool AppliedArmorRacksAssignFix = true;
        public static bool AppliedStorageSettingsAllowedToAcceptFix = true;
        public static bool AppliedFishTableTypeListFix = true;
        public static bool AppliedGetActiveRitualsFix = true;
        public static bool AppliedGrowerCutTrees = true;
        public static bool AppliedIdleErrorWanderFix = true;
        public static bool AppliedHospitalityGuestApparelOptimizeFix = true;
        public static bool AppliedLowTpsPawnDump = true;
        public static bool AppliedIdleWorkSearchCooldown = true;
        public static bool AppliedPtgMedicalCare = true;
        public static bool AppliedSeedsPleaseSowFix = true;
        public static bool AppliedTakeFromMending = true;
        public static bool AppliedTradeCaravanLordFix = true;
        public static bool AppliedApparelPolicyLoadFix = true;
        public static bool AppliedBillRenamePrefill = true;
        public static bool AppliedRecipeProductDropLostLog = true;
        public static bool AppliedAllowToolHaulUrgentlyNreFix = true;
        public static bool AppliedDubsAnalyzerBeginUpdateFix = true;
        public static bool AppliedDebugLogSplitterDragFix = true;
#if RIMWORLD_1_6
        public static bool AppliedNeanderthalChiefLeaderFix = true;
        public static bool AppliedMapPreviewRngBaselineFix = true;
        public static bool AppliedUnifiedXmlPathFix = true;
        public static bool AppliedMainMenuBgFitFix = true;
        public static bool AppliedRimatomicsGuidancePanelFix = true;
#endif

        public static bool CatCrazyTimeEnableLogging;
        public static bool CatFloorSleepEnableLogging;
        public static bool GrowerCutTreesEnableLogging;
        public static bool TakeFromMendingEnableLogging;
        public static bool TakeFromMendingShowMaintenanceMessages;
        public static bool TradeCaravanLordEnableLogging;

        public static bool LowTpsEnableLogging;
        public static bool LowTpsColonistsOnly;
        public static bool LowTpsSkipWhenPaused;
        public static bool LowTpsLogStartupMessage;
        public static float LowTpsThreshold = DefaultLowTpsThreshold;
        public static float LowTpsLogCooldownSeconds = DefaultLowTpsLogCooldownSeconds;
        public static string LowTpsThreshold_Buffer;
        public static string LowTpsLogCooldownSeconds_Buffer;

        public static bool IdleWorkSearchCooldownEnableLogging;
        public static int IdleWorkSearchCooldownTicks = DefaultIdleWorkSearchCooldownTicks;
        public static string IdleWorkSearchCooldownTicks_Buffer;

        public static bool KebabSwitchesEnableLogging;
        public static bool SuppressFilledMapMessage;
        public static bool SuppressGaveBirthMessage;
        public static bool SuppressAnimalIsPregnantMessage;
        public static bool SuppressMiscarriedStarvationMessage;
        public static bool SuppressMiscarriedPoorHealthMessage;
        public static bool SuppressSeasonBegunMessage;
        public static bool SuppressBillCompleteMessage;
        public static bool SuppressFullyHealedMessage;
        public static bool SuppressSocialFightMessage;
        public static bool SuppressNewBondRelationMessage;
        public static bool SuppressNewBondRelationNewNameMessage;
        public static bool SuppressFoodPoisoningMessage;
        public static bool SuppressRoamerLeavingMessage;
        public static bool SuppressHiveReproducedMessage;
        public static bool SuppressTraderCaravanLeavingMessage;
        public static bool SuppressTraderCaravanDismissedMessage;
        public static bool SuppressCompSpawnerSpawnedItemMessage;
        public static bool SuppressPlantDiedOfCold;
        public static bool SuppressPlantDiedOfRotUnharvested;
        public static bool SuppressPlantDiedOfRotLight;
        public static bool SuppressPlantDiedOfRot;
        public static bool SuppressPlantDiedOfPoison;
        public static bool SuppressPlantDiedOfBlight;
        public static bool SuppressPlantDiedOfPollution;
        public static bool SuppressPlantDiedOfNoPollution;
        public static bool SuppressPlantDiedOfRotPollutedTerrain;
        public static bool SuppressMinifiedTreeDied;
        public static bool SuppressRottedAwayInStorage;
        public static bool SuppressDeterioratedAway;
        public static bool SuppressWornApparelDeterioratedAway;
        public static HashSet<string> SuppressLetterIds = new HashSet<string>(StringComparer.Ordinal);
        public static HashSet<string> SuppressAlertIds = new HashSet<string>(StringComparer.Ordinal);

        private static void EnsureSuppressLetterIds()
        {
            if (SuppressLetterIds == null)
            {
                SuppressLetterIds = new HashSet<string>(StringComparer.Ordinal);
            }
        }

        public static bool IsLetterSuppressed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            EnsureSuppressLetterIds();
            return SuppressLetterIds.Contains(id);
        }

        public static void SetLetterSuppressed(string id, bool suppress)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            EnsureSuppressLetterIds();
            if (suppress)
            {
                SuppressLetterIds.Add(id);
            }
            else
            {
                SuppressLetterIds.Remove(id);
            }
        }

        public static int SuppressedLetterCount
        {
            get
            {
                EnsureSuppressLetterIds();
                return SuppressLetterIds.Count;
            }
        }

        private static void EnsureSuppressAlertIds()
        {
            if (SuppressAlertIds == null)
            {
                SuppressAlertIds = new HashSet<string>(StringComparer.Ordinal);
            }
        }

        public static bool IsAlertSuppressed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            EnsureSuppressAlertIds();
            return SuppressAlertIds.Contains(id);
        }

        public static void SetAlertSuppressed(string id, bool suppress)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            EnsureSuppressAlertIds();
            if (suppress)
            {
                SuppressAlertIds.Add(id);
            }
            else
            {
                SuppressAlertIds.Remove(id);
            }
        }

        public static int SuppressedAlertCount
        {
            get
            {
                EnsureSuppressAlertIds();
                return SuppressAlertIds.Count;
            }
        }

        private const float SettingsCheckboxRowHeight = 32f;
        private const float SettingsCheckboxRowGap = 2f;
        private const float SettingsSectionHeaderHeight = 28f;
        private const float SettingsScrollBarWidth = 16f;
        private const float CheckboxControlSize = 24f;
        private const float FeatureResetButtonWidth = 80f;
        private const float FeatureResetButtonGap = 4f;
        private const float FullResetButtonHeight = 30f;
        private const float FullResetButtonWidth = 280f;
        private const float FeatureBodyContentInset = 4f;
        private const float HeaderControlsRightInset = 4f;
        private const float SettingsTabsHeight = 32f;
        private const float NonDefaultUnderlineThickness = 1f;

        private enum SettingsTabKind
        {
            Notifications,
            Patches,
            Fixes,
        }

        private static readonly Color SettingsRowSeparatorColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);
        private static readonly Color FeatureBodyPanelFillColor = new Color(0f, 0f, 0f, 0.22f);
        /// <summary>Unselected tab fill ~80% opaque.</summary>
        private static readonly Color SettingsTabFillColor = new Color(0f, 0f, 0f, 0.8f);
        /// <summary>Selected tab: lighter fill + stronger edge glow vs idle tabs.</summary>
        private static readonly Color SettingsTabSelectedFillColor = new Color(0.32f, 0.32f, 0.36f, 0.88f);
        private static readonly Color SettingsTabSelectedGlowColor = new Color(0.85f, 0.88f, 1f, 0.95f);
        private static readonly Color RestartPendingOutlineColor = new Color(0.95f, 0.85f, 0.15f);
        private static readonly Color SupersededHeaderColor = new Color(0.95f, 0.25f, 0.25f);
        private static readonly Color NonDefaultUnderlineColor = new Color(0.45f, 0.78f, 1f);

        /// <summary>Cached vertical alpha gradient for feature headers (bilinear, no strip banding).</summary>
        private static Texture2D featureHeaderAlphaGradientTex;

        private readonly List<float> featureBodyPanelHeightCachesNotifications = new List<float>();
        private readonly List<float> featureBodyPanelHeightCachesPatches = new List<float>();
        private readonly List<float> featureBodyPanelHeightCachesFixes = new List<float>();
        private List<float> featureBodyPanelHeightCaches;
        private int featureBodyPanelDrawIndex;
        private static float bodyContentInset;

        private SettingsTabKind selectedSettingsTab = SettingsTabKind.Notifications;
        private Vector2 settingsScrollPositionNotifications;
        private Vector2 settingsScrollPositionPatches;
        private Vector2 settingsScrollPositionFixes;
        private const float SettingsListingCanvasHeightMin = 8000f;

        private float scrollContentHeightNotifications;
        private float scrollContentHeightPatches = 500f;
        private float scrollContentHeightFixes = 1100f;

        /// <summary>
        /// Same preset gradient as kebab limits modified-limit highlight colors.
        ///
        /// Тот же градиент, что у kebab limits для цвета изменённых лимитов.
        /// </summary>
        private static readonly Color[] BlockSeparatorGradientColors =
        {
            new Color(1f, 1f, 0.2f),
            new Color(1f, 0.55f, 0.1f),
            new Color(0.35f, 1f, 0.45f),
            new Color(0.45f, 0.75f, 1f),
            new Color(1f, 0.45f, 0.75f),
            new Color(0.9f, 0.9f, 0.9f),
        };

        public void DrawSettings(Rect inRect)
        {
            FixErrorTraceUi.BeginHoverFrame();
            FixErrorTraceUi.SetDrawBounds(inRect);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            Rect resetRow = new Rect(inRect.x, inRect.y, inRect.width, FullResetButtonHeight);
            DrawFullResetButton(resetRow, inRect.width);

            Rect tabBarRect = new Rect(
                inRect.x,
                inRect.y + FullResetButtonHeight + SettingsCheckboxRowGap,
                inRect.width,
                SettingsTabsHeight);
            DrawSettingsTabBar(tabBarRect);

            Rect tabContentRect = new Rect(
                inRect.x,
                tabBarRect.yMax,
                inRect.width,
                Mathf.Max(0f, inRect.yMax - tabBarRect.yMax));

            switch (selectedSettingsTab)
            {
                case SettingsTabKind.Patches:
                    DrawTabScrollContent(
                        tabContentRect,
                        ref settingsScrollPositionPatches,
                        ref scrollContentHeightPatches,
                        featureBodyPanelHeightCachesPatches,
                        DrawPatchesTabContents);
                    break;
                case SettingsTabKind.Fixes:
                    DrawTabScrollContent(
                        tabContentRect,
                        ref settingsScrollPositionFixes,
                        ref scrollContentHeightFixes,
                        featureBodyPanelHeightCachesFixes,
                        DrawFixesTabContents);
                    break;
                default:
                    DrawTabScrollContent(
                        tabContentRect,
                        ref settingsScrollPositionNotifications,
                        ref scrollContentHeightNotifications,
                        featureBodyPanelHeightCachesNotifications,
                        DrawNotificationsTabContents,
                        EstimateNotificationsListingCanvasHeight());
                    break;
            }

            FixErrorTraceUi.DrawHoverPanelIfNeeded();
        }

        /// <summary>
        /// Dark ~80%-opaque tabs; selected tab uses a lighter fill and bright edge glow.
        ///
        /// Тёмные вкладки ~80% непрозрачности; выбранная — светлее и с ярким свечением края.
        /// </summary>
        private void DrawSettingsTabBar(Rect tabBarRect)
        {
            float tabWidth = tabBarRect.width / 3f;
            DrawOneSettingsTab(
                new Rect(tabBarRect.x, tabBarRect.y, tabWidth, tabBarRect.height),
                "KebabTweaks.Tab.Notifications".Translate(),
                SettingsTabKind.Notifications);
            DrawOneSettingsTab(
                new Rect(tabBarRect.x + tabWidth, tabBarRect.y, tabWidth, tabBarRect.height),
                "KebabTweaks.Tab.Patches".Translate(),
                SettingsTabKind.Patches);
            DrawOneSettingsTab(
                new Rect(tabBarRect.x + tabWidth * 2f, tabBarRect.y, tabWidth, tabBarRect.height),
                "KebabTweaks.Tab.Fixes".Translate(),
                SettingsTabKind.Fixes);
        }

        private void DrawOneSettingsTab(Rect tabRect, string label, SettingsTabKind kind)
        {
            bool selected = selectedSettingsTab == kind;
            if (Event.current.type == EventType.Repaint)
            {
                Color previous = GUI.color;
                GUI.color = selected ? SettingsTabSelectedFillColor : SettingsTabFillColor;
                GUI.DrawTexture(tabRect, BaseContent.WhiteTex);
                if (selected)
                {
                    DrawInnerBoxBorder(tabRect, 2f, SettingsTabSelectedGlowColor);
                    GUI.color = SettingsTabSelectedGlowColor;
                    GUI.DrawTexture(new Rect(tabRect.x, tabRect.yMax - 2f, tabRect.width, 2f), BaseContent.WhiteTex);
                }
                else
                {
                    GUI.color = SettingsRowSeparatorColor;
                    GUI.DrawTexture(new Rect(tabRect.x, tabRect.yMax - 1f, tabRect.width, 1f), BaseContent.WhiteTex);
                    GUI.DrawTexture(new Rect(tabRect.xMax - 1f, tabRect.y, 1f, tabRect.height), BaseContent.WhiteTex);
                }

                GUI.color = previous;
            }

            if (Widgets.ButtonInvisible(tabRect))
            {
                selectedSettingsTab = kind;
            }

            TextAnchor previousAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(tabRect, label);
            Text.Anchor = previousAnchor;
        }

        private static float EstimateNotificationsListingCanvasHeight()
        {
            int checkboxRows =
                SuppressibleScreenMessages.All.Length
                + SuppressibleLetters.All.Length
                + SuppressibleAlerts.All.Length;
            const float overhead = 480f;
            float rowSlot = SettingsCheckboxRowHeight + SettingsCheckboxRowGap + 2f;
            return Mathf.Max(
                SettingsListingCanvasHeightMin,
                overhead + checkboxRows * rowSlot * 1.35f);
        }

        private void DrawTabScrollContent(
            Rect tabContentRect,
            ref Vector2 scrollPosition,
            ref float contentHeight,
            List<float> panelHeightCaches,
            Action<Listing_Standard, float> drawContents,
            float listingCanvasMin = SettingsListingCanvasHeightMin)
        {
            featureBodyPanelHeightCaches = panelHeightCaches;
            featureBodyPanelDrawIndex = 0;

            float viewWidth = tabContentRect.width - SettingsScrollBarWidth;
            float scrollHeight = Mathf.Max(contentHeight, tabContentRect.height);
            Rect viewRect = new Rect(0f, 0f, viewWidth, scrollHeight);
            Widgets.BeginScrollView(tabContentRect, ref scrollPosition, viewRect);
            FixErrorTraceUi.SetScrollContext(tabContentRect, scrollPosition);

            Listing_Standard listing = new Listing_Standard();
            listing.ColumnWidth = viewWidth;
            // Listing_Standard stops advancing CurHeight past the Begin() height — size canvas to catalog.
            float canvasHeight = Mathf.Max(listingCanvasMin, scrollHeight);
            listing.Begin(new Rect(0f, 0f, viewWidth, canvasHeight));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            drawContents(listing, viewWidth);
            float measured = listing.CurHeight + SettingsCheckboxRowGap;
            listing.End();
            Widgets.EndScrollView();

            if (measured > 1f)
            {
                contentHeight = Mathf.Max(measured, tabContentRect.height);
            }
        }

        private void DrawNotificationsTabContents(Listing_Standard listing, float fullWidth)
        {
            DrawKebabSwitchesBlock(listing, fullWidth);
        }

        private void DrawPatchesTabContents(Listing_Standard listing, float fullWidth)
        {
            // Non-fix utilities: more settings higher.
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.LowTpsPawnDump".Translate(),
                "KebabTweaks.Patch.LowTpsPawnDump.Tooltip".Translate(),
                SupersededStandaloneMods.LowTpsPawnDump,
                ref EnableLowTpsPawnDump, AppliedLowTpsPawnDump, true, false, ResetLowTpsPawnDump,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "LowTpsPawnDump.EnableLogging".Translate(),
                        () => LowTpsEnableLogging, v => LowTpsEnableLogging = v,
                        "LowTpsPawnDump.EnableLoggingTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing, "LowTpsPawnDump.ColonistsOnly".Translate(),
                        ref LowTpsColonistsOnly, false,
                        "LowTpsPawnDump.ColonistsOnlyTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing, "LowTpsPawnDump.SkipWhenPaused".Translate(),
                        ref LowTpsSkipWhenPaused, false,
                        "LowTpsPawnDump.SkipWhenPausedTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing, "LowTpsPawnDump.LogStartupMessage".Translate(),
                        ref LowTpsLogStartupMessage, false,
                        "LowTpsPawnDump.LogStartupMessageTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawBodyContentNumericFloatRow(
                        listing,
                        fullWidth,
                        "LowTpsPawnDump.TpsThreshold".Translate(),
                        ref LowTpsThreshold,
                        ref LowTpsThreshold_Buffer,
                        10f,
                        900f,
                        DefaultLowTpsThreshold);
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawBodyContentNumericFloatRow(
                        listing,
                        fullWidth,
                        "LowTpsPawnDump.LogCooldownSeconds".Translate(),
                        ref LowTpsLogCooldownSeconds,
                        ref LowTpsLogCooldownSeconds_Buffer,
                        0.25f,
                        10f,
                        DefaultLowTpsLogCooldownSeconds);
                    listing.Gap(SettingsCheckboxRowGap);
                },
                leadingSpacer: false);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.IdleWorkSearchCooldown".Translate(),
                "KebabTweaks.Patch.IdleWorkSearchCooldown.Tooltip".Translate(),
                null,
                ref EnableIdleWorkSearchCooldown, AppliedIdleWorkSearchCooldown, true, false,
                ResetIdleWorkSearchCooldown,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing,
                        "IdleWorkSearchCooldown.EnableLogging".Translate(),
                        () => IdleWorkSearchCooldownEnableLogging,
                        v => IdleWorkSearchCooldownEnableLogging = v,
                        "IdleWorkSearchCooldown.EnableLoggingTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawBodyContentNumericIntRow(
                        listing,
                        fullWidth,
                        "IdleWorkSearchCooldown.CooldownTicks".Translate(),
                        ref IdleWorkSearchCooldownTicks,
                        ref IdleWorkSearchCooldownTicks_Buffer,
                        0,
                        MaxIdleWorkSearchCooldownTicks,
                        DefaultIdleWorkSearchCooldownTicks);
                    listing.Gap(SettingsCheckboxRowGap);
                });

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.GrowerCutTrees".Translate(),
                "KebabTweaks.Patch.GrowerCutTrees.Tooltip".Translate(),
                SupersededStandaloneMods.GrowerCutTrees,
                ref EnableGrowerCutTrees, AppliedGrowerCutTrees, true, false, ResetGrowerCutTrees,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "GrowerCutTreesPatch.EnableLogging".Translate(),
                        () => GrowerCutTreesEnableLogging, v => GrowerCutTreesEnableLogging = v);
                });

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.ApparelPolicyLoad".Translate(),
                "KebabTweaks.Patch.ApparelPolicyLoad.Tooltip".Translate(),
                null,
                ref EnableApparelPolicyLoadFix, AppliedApparelPolicyLoadFix, true, false, ResetApparelPolicyLoadFix,
                null);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.BillRenamePrefill".Translate(),
                "KebabTweaks.Patch.BillRenamePrefill.Tooltip".Translate(),
                null,
                ref EnableBillRenamePrefill, AppliedBillRenamePrefill, true, false, ResetBillRenamePrefill,
                null);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.RecipeProductDropLostLog".Translate(),
                "KebabTweaks.Patch.RecipeProductDropLostLog.Tooltip".Translate(),
                null,
                ref EnableRecipeProductDropLostLog, AppliedRecipeProductDropLostLog, true, false,
                ResetRecipeProductDropLostLog,
                null);
        }

        private void DrawFixesTabContents(Listing_Standard listing, float fullWidth)
        {
            // Fixes: more settings higher, then header-only; 1.6-only blocks at bottom.
            bool firstHeaderOnTab = true;
#if !RIMWORLD_1_6
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.TakeFromMending".Translate(),
                "KebabTweaks.Patch.TakeFromMending.Tooltip".Translate(),
                SupersededStandaloneMods.TakeFromMending,
                ref EnableTakeFromMending, AppliedTakeFromMending, true, false, ResetTakeFromMending,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "TakeFromMendingPatch.EnableLogging".Translate(),
                        () => TakeFromMendingEnableLogging, v => TakeFromMendingEnableLogging = v,
                        "TakeFromMendingPatch.EnableLoggingTooltip".Translate());
                    DrawSettingsRowSeparator(listing, fullWidth);
                    DrawSettingsCheckboxRow(listing, "TakeFromMendingPatch.ShowMaintenanceMessages".Translate(),
                        ref TakeFromMendingShowMaintenanceMessages, false,
                        "TakeFromMendingPatch.ShowMaintenanceMessagesTooltip".Translate());
                },
                FixErrorTraceCatalog.TakeFromMending, FixErrorTraceCatalog.TakeFromMendingTipId,
                leadingSpacer: !firstHeaderOnTab);
            firstHeaderOnTab = false;
#endif

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CatCrazyTime".Translate(),
                "KebabTweaks.Patch.CatCrazyTime.Tooltip".Translate(),
                SupersededStandaloneMods.CatCrazyTime,
                ref EnableCatCrazyTime, AppliedCatCrazyTime, true, false, ResetCatCrazyTime,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "CatCrazyTimePatch.EnableLogging".Translate(),
                        () => CatCrazyTimeEnableLogging, v => CatCrazyTimeEnableLogging = v,
                        "CatCrazyTimePatch.EnableLoggingTooltip".Translate());
                },
                FixErrorTraceCatalog.CatCrazyTime, FixErrorTraceCatalog.CatCrazyTimeTipId,
                leadingSpacer: !firstHeaderOnTab);
            firstHeaderOnTab = false;

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CatFloorSleep".Translate(),
                "KebabTweaks.Patch.CatFloorSleep.Tooltip".Translate(),
                SupersededStandaloneMods.CatFloorSleep,
                ref EnableCatFloorSleep, AppliedCatFloorSleep, true, false, ResetCatFloorSleep,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "CatFloorSleepPatch.EnableLogging".Translate(),
                        () => CatFloorSleepEnableLogging, v => CatFloorSleepEnableLogging = v,
                        "CatFloorSleepPatch.EnableLoggingTooltip".Translate());
                },
                FixErrorTraceCatalog.CatFloorSleep, FixErrorTraceCatalog.CatFloorSleepTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.TradeCaravanLordFix".Translate(),
                "KebabTweaks.Patch.TradeCaravanLordFix.Tooltip".Translate(),
                SupersededStandaloneMods.TradeCaravanLordFix,
                ref EnableTradeCaravanLordFix, AppliedTradeCaravanLordFix, true, false, ResetTradeCaravanLordFix,
                () =>
                {
                    DrawSettingsEnableLoggingCheckboxRow(listing, "TradeCaravanLordFixPatch.EnableLogging".Translate(),
                        () => TradeCaravanLordEnableLogging, v => TradeCaravanLordEnableLogging = v,
                        "TradeCaravanLordFixPatch.EnableLoggingTooltip".Translate());
                },
                FixErrorTraceCatalog.TradeCaravanLordFix, FixErrorTraceCatalog.TradeCaravanLordFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.ArmorRacksAssignFix".Translate(),
                "KebabTweaks.Patch.ArmorRacksAssignFix.Tooltip".Translate(),
                SupersededStandaloneMods.ArmorRacksAssignFix,
                ref EnableArmorRacksAssignFix, AppliedArmorRacksAssignFix, true, false, ResetArmorRacksAssignFix,
                null,
                FixErrorTraceCatalog.ArmorRacksAssignFix, FixErrorTraceCatalog.ArmorRacksAssignFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.StorageSettingsAllowedToAcceptFix".Translate(),
                "KebabTweaks.Patch.StorageSettingsAllowedToAcceptFix.Tooltip".Translate(),
                null,
                ref EnableStorageSettingsAllowedToAcceptFix, AppliedStorageSettingsAllowedToAcceptFix,
                true, false, ResetStorageSettingsAllowedToAcceptFix,
                null,
                FixErrorTraceCatalog.StorageSettingsAllowedToAcceptFix,
                FixErrorTraceCatalog.StorageSettingsAllowedToAcceptFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CeRunForCoverDestFix".Translate(),
                "KebabTweaks.Patch.CeRunForCoverDestFix.Tooltip".Translate(),
                SupersededStandaloneMods.CeRunForCoverDestFix,
                ref EnableCeRunForCoverDestFix, AppliedCeRunForCoverDestFix, true, false, ResetCeRunForCoverDestFix,
                null,
                FixErrorTraceCatalog.CeRunForCoverDestFix, FixErrorTraceCatalog.CeRunForCoverDestFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.CeProjectileNullSoundFix".Translate(),
                "KebabTweaks.Patch.CeProjectileNullSoundFix.Tooltip".Translate(),
                null,
                ref EnableCeProjectileNullSoundFix, AppliedCeProjectileNullSoundFix, true, false,
                ResetCeProjectileNullSoundFix,
                null,
                FixErrorTraceCatalog.CeProjectileNullSoundFix,
                FixErrorTraceCatalog.CeProjectileNullSoundFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.FishTableTypeListFix".Translate(),
                "KebabTweaks.Patch.FishTableTypeListFix.Tooltip".Translate(),
                SupersededStandaloneMods.FishTableTypeListFix,
                ref EnableFishTableTypeListFix, AppliedFishTableTypeListFix, true, false, ResetFishTableTypeListFix,
                null,
                FixErrorTraceCatalog.FishTableTypeListFix, FixErrorTraceCatalog.FishTableTypeListFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.GetActiveRitualsFix".Translate(),
                "KebabTweaks.Patch.GetActiveRitualsFix.Tooltip".Translate(),
                SupersededStandaloneMods.GetActiveRitualsFix,
                ref EnableGetActiveRitualsFix, AppliedGetActiveRitualsFix, true, false, ResetGetActiveRitualsFix,
                null,
                FixErrorTraceCatalog.GetActiveRitualsFix, FixErrorTraceCatalog.GetActiveRitualsFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.IdleErrorWanderFix".Translate(),
                "KebabTweaks.Patch.IdleErrorWanderFix.Tooltip".Translate(),
                SupersededStandaloneMods.IdleErrorWanderFix,
                ref EnableIdleErrorWanderFix, AppliedIdleErrorWanderFix, true, false, ResetIdleErrorWanderFix,
                null,
                FixErrorTraceCatalog.IdleErrorWanderFix, FixErrorTraceCatalog.IdleErrorWanderFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.AllowToolHaulUrgentlyNreFix".Translate(),
                "KebabTweaks.Patch.AllowToolHaulUrgentlyNreFix.Tooltip".Translate(),
                null,
                ref EnableAllowToolHaulUrgentlyNreFix, AppliedAllowToolHaulUrgentlyNreFix,
                true, false, ResetAllowToolHaulUrgentlyNreFix,
                null,
                FixErrorTraceCatalog.AllowToolHaulUrgentlyNreFix,
                FixErrorTraceCatalog.AllowToolHaulUrgentlyNreFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.DubsAnalyzerBeginUpdateFix".Translate(),
                "KebabTweaks.Patch.DubsAnalyzerBeginUpdateFix.Tooltip".Translate(),
                null,
                ref EnableDubsAnalyzerBeginUpdateFix, AppliedDubsAnalyzerBeginUpdateFix,
                true, false, ResetDubsAnalyzerBeginUpdateFix,
                null,
                FixErrorTraceCatalog.DubsAnalyzerBeginUpdateFix,
                FixErrorTraceCatalog.DubsAnalyzerBeginUpdateFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.DebugLogSplitterDragFix".Translate(),
                "KebabTweaks.Patch.DebugLogSplitterDragFix.Tooltip".Translate(),
                null,
                ref EnableDebugLogSplitterDragFix, AppliedDebugLogSplitterDragFix,
                true, false, ResetDebugLogSplitterDragFix,
                null,
                FixErrorTraceCatalog.DebugLogSplitterDragFix,
                FixErrorTraceCatalog.DebugLogSplitterDragFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.HospitalityGuestApparelOptimizeFix".Translate(),
                "KebabTweaks.Patch.HospitalityGuestApparelOptimizeFix.Tooltip".Translate(),
                null,
                ref EnableHospitalityGuestApparelOptimizeFix, AppliedHospitalityGuestApparelOptimizeFix,
                true, false, ResetHospitalityGuestApparelOptimizeFix,
                null,
                FixErrorTraceCatalog.HospitalityGuestApparelOptimizeFix,
                FixErrorTraceCatalog.HospitalityGuestApparelOptimizeFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.PtgMedicalCare".Translate(),
                "KebabTweaks.Patch.PtgMedicalCare.Tooltip".Translate(),
                SupersededStandaloneMods.PtgMedicalCare,
                ref EnablePtgMedicalCare, AppliedPtgMedicalCare, true, true, ResetPtgMedicalCare,
                null,
                FixErrorTraceCatalog.PtgMedicalCare, FixErrorTraceCatalog.PtgMedicalCareTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.SeedsPleaseSowFix".Translate(),
                "KebabTweaks.Patch.SeedsPleaseSowFix.Tooltip".Translate(),
                SupersededStandaloneMods.SeedsPleaseSowFix,
                ref EnableSeedsPleaseSowFix, AppliedSeedsPleaseSowFix, true, true, ResetSeedsPleaseSowFix,
                null,
                FixErrorTraceCatalog.SeedsPleaseSowFix, FixErrorTraceCatalog.SeedsPleaseSowFixTipId);

#if RIMWORLD_1_6
            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.NeanderthalChiefLeaderFix".Translate(),
                "KebabTweaks.Patch.NeanderthalChiefLeaderFix.Tooltip".Translate(),
                SupersededStandaloneMods.NeanderthalChiefLeaderFix,
                ref EnableNeanderthalChiefLeaderFix, AppliedNeanderthalChiefLeaderFix,
                true, false, ResetNeanderthalChiefLeaderFix,
                null,
                FixErrorTraceCatalog.NeanderthalChiefLeaderFix,
                FixErrorTraceCatalog.NeanderthalChiefLeaderFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.MapPreviewRngBaselineFix".Translate(),
                "KebabTweaks.Patch.MapPreviewRngBaselineFix.Tooltip".Translate(),
                SupersededStandaloneMods.MapPreviewRngBaselineFix,
                ref EnableMapPreviewRngBaselineFix, AppliedMapPreviewRngBaselineFix,
                true, true, ResetMapPreviewRngBaselineFix,
                null,
                FixErrorTraceCatalog.MapPreviewRngBaselineFix,
                FixErrorTraceCatalog.MapPreviewRngBaselineFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.UnifiedXmlPathFix".Translate(),
                "KebabTweaks.Patch.UnifiedXmlPathFix.Tooltip".Translate(),
                SupersededStandaloneMods.UnifiedXmlPathFix,
                ref EnableUnifiedXmlPathFix, AppliedUnifiedXmlPathFix,
                true, true, ResetUnifiedXmlPathFix,
                null,
                FixErrorTraceCatalog.UnifiedXmlPathFix,
                FixErrorTraceCatalog.UnifiedXmlPathFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.MainMenuBgFitFix".Translate(),
                "KebabTweaks.Patch.MainMenuBgFitFix.Tooltip".Translate(),
                SupersededStandaloneMods.MainMenuBgFitFix,
                ref EnableMainMenuBgFitFix, AppliedMainMenuBgFitFix,
                true, true, ResetMainMenuBgFitFix,
                null,
                FixErrorTraceCatalog.MainMenuBgFitFix,
                FixErrorTraceCatalog.MainMenuBgFitFixTipId);

            DrawPatchBlock(listing, fullWidth,
                "KebabTweaks.Patch.RimatomicsGuidancePanelFix".Translate(),
                "KebabTweaks.Patch.RimatomicsGuidancePanelFix.Tooltip".Translate(),
                SupersededStandaloneMods.RimatomicsGuidancePanelFix,
                ref EnableRimatomicsGuidancePanelFix, AppliedRimatomicsGuidancePanelFix,
                true, true, ResetRimatomicsGuidancePanelFix,
                null,
                FixErrorTraceCatalog.RimatomicsGuidancePanelFix,
                FixErrorTraceCatalog.RimatomicsGuidancePanelFixTipId);
#endif
        }

        private void DrawFullResetButton(Rect row, float fullWidth)
        {
            Rect buttonRect = new Rect(
                row.x + (fullWidth - FullResetButtonWidth) / 2f,
                row.y,
                FullResetButtonWidth,
                FullResetButtonHeight);
            if (Widgets.ButtonText(buttonRect, "KebabTweaks.ResetAll".Translate()))
            {
                Find.WindowStack.Add(new Dialog_MessageBox(
                    "KebabTweaks.ResetAllConfirm".Translate(),
                    "Confirm".Translate(),
                    ResetAllToDefaults,
                    "Cancel".Translate(),
                    null,
                    "KebabTweaks.ResetAll".Translate(),
                    buttonADestructive: true));
            }
        }

        private void DrawKebabSwitchesBlock(Listing_Standard listing, float fullWidth)
        {
            DrawFeatureHeaderWithResetOnly(
                listing,
                "KebabTweaks.KebabSwitches.Header".Translate(),
                ResetKebabSwitches,
                SupersededStandaloneMods.KebabSwitches);

            DrawFeatureBodyPanel(listing, fullWidth, () =>
            {
                DrawSettingsEnableLoggingCheckboxRow(listing, "KebabSwitches.EnableLogging".Translate(),
                    () => KebabSwitchesEnableLogging, v => KebabSwitchesEnableLogging = v);
                DrawSettingsRowSeparator(listing, fullWidth);
                DrawSettingsCheckboxHeightSpacer(listing);
                DrawSettingsSectionHeader(listing, "KebabSwitches.ScreenMessagesHeader".Translate());
                DrawSettingsRowSeparator(listing, fullWidth);
                foreach (SuppressibleScreenMessageEntry entry in SuppressibleScreenMessages.All)
                {
                    bool value = entry.IsSuppressEnabled();
                    DrawSettingsCheckboxRow(listing, entry.CheckboxLabel, ref value, false, entry.SourceTooltip);
                    entry.SetSuppressEnabled(value);
                    DrawSettingsRowSeparator(listing, fullWidth);
                }

                DrawSettingsCheckboxHeightSpacer(listing);
                DrawSettingsSectionHeader(listing, "KebabSwitches.LettersHeader".Translate());
                DrawSettingsRowSeparator(listing, fullWidth);
                foreach (SuppressibleLetterEntry entry in SuppressibleLetters.All)
                {
                    bool value = entry.IsSuppressEnabled;
                    DrawSettingsCheckboxRow(listing, entry.CheckboxLabel, ref value, false, entry.SourceTooltip);
                    entry.SetSuppressEnabled(value);
                    DrawSettingsRowSeparator(listing, fullWidth);
                }

                DrawSettingsCheckboxHeightSpacer(listing);
                DrawSettingsSectionHeader(listing, "KebabSwitches.AlertsHeader".Translate());
                DrawSettingsRowSeparator(listing, fullWidth);
                foreach (SuppressibleAlertEntry entry in SuppressibleAlerts.All)
                {
                    bool value = entry.IsSuppressEnabled;
                    DrawSettingsCheckboxRow(listing, entry.CheckboxLabel, ref value, false, entry.SourceTooltip);
                    entry.SetSuppressEnabled(value);
                    DrawSettingsRowSeparator(listing, fullWidth);
                }
            });
        }

        private void DrawPatchBlock(
            Listing_Standard listing,
            float fullWidth,
            string headerLabel,
            string headerTooltip,
            string supersededPackageId,
            ref bool enabled,
            bool appliedAtLoad,
            bool defaultEnabled,
            bool requiresRestart,
            Action resetAction,
            Action drawBody,
            string errorTrace = null,
            int errorTraceTipId = 0,
            bool leadingSpacer = true)
        {
            if (leadingSpacer)
            {
                DrawSettingsCheckboxHeightSpacer(listing);
            }

            DrawPatchEnableHeaderRow(
                listing,
                headerLabel,
                headerTooltip,
                supersededPackageId,
                ref enabled,
                appliedAtLoad,
                defaultEnabled,
                requiresRestart,
                resetAction,
                errorTrace,
                errorTraceTipId);
            if (drawBody != null)
            {
                DrawFeatureBodyPanel(listing, fullWidth, drawBody);
            }
            else
            {
                // Header-only: rainbow top edge under the title (same role as panel top).
                DrawFeatureBlockRainbowTop(listing, fullWidth);
            }
        }

        /// <summary>
        /// Draws feature options inside a dimmed panel: rainbow 1px top (under the header), gray
        /// left/right/bottom matching the row separator, and a few px inset for left-aligned controls.
        ///
        /// Рисует опции фичи в затемнённой панели: радужный верх 1px (под заголовком), серые
        /// левый/правый/низ как у разделителя строк, и небольшой inset для выровненных влево контролов.
        /// </summary>
        private void DrawFeatureBodyPanel(Listing_Standard listing, float fullWidth, Action drawBody)
        {
            int index = featureBodyPanelDrawIndex++;
            while (featureBodyPanelHeightCaches.Count <= index)
            {
                featureBodyPanelHeightCaches.Add(0f);
            }

            float startY = listing.CurHeight;
            float cachedHeight = featureBodyPanelHeightCaches[index];
            // Fill behind body rows (Layout height from prior pass); drawing after content washes out labels.
            if (Event.current.type == EventType.Repaint && cachedHeight > 0.5f)
            {
                DrawFeatureBodyPanelFill(new Rect(0f, startY, fullWidth, cachedHeight));
            }

            float previousInset = bodyContentInset;
            bodyContentInset = FeatureBodyContentInset;
            try
            {
                drawBody();
            }
            finally
            {
                bodyContentInset = previousInset;
            }

            float height = listing.CurHeight - startY;
            featureBodyPanelHeightCaches[index] = height;
            if (Event.current.type == EventType.Repaint && height > 0.5f)
            {
                DrawFeatureBodyPanelChrome(new Rect(0f, startY, fullWidth, height));
            }
        }

        private static void DrawFeatureBodyPanelFill(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = FeatureBodyPanelFillColor;
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = previousColor;
        }

        /// <summary>
        /// Gray left/right/bottom + rainbow top (replaces inter-block gradient separators).
        ///
        /// Серые левый/правый/низ + радужный верх (вместо радуги между блоками).
        /// </summary>
        private static void DrawFeatureBodyPanelChrome(Rect panel)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            const float thickness = 1f;
            Color previousColor = GUI.color;
            GUI.color = SettingsRowSeparatorColor;
            Texture2D tex = BaseContent.WhiteTex;
            GUI.DrawTexture(new Rect(panel.x, panel.yMax - thickness, panel.width, thickness), tex);
            float sideHeight = Mathf.Max(0f, panel.height - thickness);
            GUI.DrawTexture(new Rect(panel.x, panel.y + thickness, thickness, sideHeight), tex);
            GUI.DrawTexture(new Rect(panel.xMax - thickness, panel.y + thickness, thickness, sideHeight), tex);
            GUI.color = previousColor;

            DrawGradientLine(new Rect(panel.x, panel.y, panel.width, thickness));
        }

        /// <summary>
        /// Rainbow 1px top edge under a feature header (panel top or header-only blocks).
        ///
        /// Радужный верх 1px под заголовком фичи (верх панели или header-only блоки).
        /// </summary>
        private static void DrawFeatureBlockRainbowTop(Listing_Standard listing, float fullWidth)
        {
            Rect lineRect = listing.GetRect(1f);
            lineRect.x = 0f;
            lineRect.width = fullWidth;
            DrawGradientLine(lineRect);
            listing.Gap(SettingsCheckboxRowGap);
        }

        private static void ApplyBodyContentInset(ref Rect row, float fullWidth)
        {
            if (bodyContentInset <= 0f)
            {
                return;
            }

            row.x = bodyContentInset;
            row.width = fullWidth - bodyContentInset * 2f;
        }

        private const float BodyNumericFieldWidthFraction = 0.25f;

        private static Rect CalcBodyNumericFieldRect(Rect row, Rect rightHalf, float fieldHeight)
        {
            float fieldWidth = rightHalf.width * BodyNumericFieldWidthFraction;
            return new Rect(
                rightHalf.xMax - fieldWidth,
                row.y + (row.height - fieldHeight) / 2f,
                fieldWidth,
                fieldHeight);
        }

        /// <summary>
        /// Body-row numeric float field (label left, TextFieldNumeric right) like idle work-search cooldown ticks.
        ///
        /// Числовое поле float в body: лейбл слева, TextFieldNumeric справа — как задержка idle work search.
        /// </summary>
        private static void DrawBodyContentNumericFloatRow(
            Listing_Standard listing,
            float fullWidth,
            string label,
            ref float value,
            ref string buffer,
            float min,
            float max,
            float defaultValue)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            ApplyBodyContentInset(ref row, fullWidth);
            Rect left = row.LeftHalf();
            Rect right = row.RightHalf();
            const float numericFieldHeight = 24f;
            Rect fieldRect = CalcBodyNumericFieldRect(row, right, numericFieldHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(left, label);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.TextFieldNumeric(fieldRect, ref value, ref buffer, min, max);
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            if (!Mathf.Approximately(value, defaultValue))
            {
                DrawNonDefaultTextUnderline(row.LeftHalf(), label, TextAnchor.MiddleLeft);
            }
        }

        /// <summary>
        /// Body-row numeric int field (label left, TextFieldNumeric right) like kebab limits slider max.
        ///
        /// Числовое поле в body: лейбл слева, TextFieldNumeric справа — как max ползунка в kebab limits.
        /// </summary>
        private static void DrawBodyContentNumericIntRow(
            Listing_Standard listing,
            float fullWidth,
            string label,
            ref int value,
            ref string buffer,
            int min,
            int max,
            int defaultValue)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            ApplyBodyContentInset(ref row, fullWidth);
            Rect left = row.LeftHalf();
            Rect right = row.RightHalf();
            const float numericFieldHeight = 24f;
            Rect fieldRect = CalcBodyNumericFieldRect(row, right, numericFieldHeight);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(left, label);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.TextFieldNumeric(fieldRect, ref value, ref buffer, min, max);
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            if (value != defaultValue)
            {
                DrawNonDefaultTextUnderline(row.LeftHalf(), label, TextAnchor.MiddleLeft);
            }
        }

        /// <summary>
        /// Centered feature title + reset on the right (no enable checkbox) — kebab switches.
        /// Red bold title while the superseded standalone package is still active.
        ///
        /// Центрированный заголовок + сброс справа (без enable) — kebab switches.
        /// Красный жирный заголовок, пока активен устаревший отдельный package.
        /// </summary>
        private void DrawFeatureHeaderWithResetOnly(
            Listing_Standard listing,
            string label,
            Action resetAction,
            string supersededPackageId)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            DrawFeatureHeaderBackground(row);
            float controlY = row.y + (row.height - CheckboxControlSize) / 2f;
            Rect resetRect = new Rect(
                row.xMax - HeaderControlsRightInset - FeatureResetButtonWidth,
                controlY,
                FeatureResetButtonWidth,
                CheckboxControlSize);

            bool supersededActive = SupersededStandaloneMods.IsActive(supersededPackageId);
            if (supersededActive)
            {
                if (Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                TooltipHandler.TipRegion(row, "KebabTweaks.SupersededStandaloneTooltip".Translate());
            }

            DrawFeatureHeaderLabel(row, label, supersededActive, differsFromDefault: false);
            DrawFeatureResetButton(resetRect, label, resetAction);
            listing.Gap(SettingsCheckboxRowGap);
        }

        /// <summary>
        /// Patch title centered on the full row width (ignores reset/checkbox for layout). Optional
        /// copy-trace button (Fixes tab) sits left of reset; reset left of enable. Yellow outline when
        /// restart-required and Enable* != Applied* (not while superseded silent-skip is active).
        ///
        /// Заголовок патча по центру полной ширины строки. На вкладке «Фиксы» слева от сброса —
        /// кнопка копирования trace; сброс слева от enable. Жёлтая обводка при рестарте и Enable* ≠ Applied*.
        /// </summary>
        private void DrawPatchEnableHeaderRow(
            Listing_Standard listing,
            string label,
            string tooltip,
            string supersededPackageId,
            ref bool value,
            bool appliedAtLoad,
            bool defaultEnabled,
            bool requiresRestart,
            Action resetAction,
            string errorTrace = null,
            int errorTraceTipId = 0)
        {
            Rect row = listing.GetRect(SettingsCheckboxRowHeight);
            DrawFeatureHeaderBackground(row);
            float controlY = row.y + (row.height - CheckboxControlSize) / 2f;
            Rect checkRect = new Rect(
                row.xMax - HeaderControlsRightInset - CheckboxControlSize,
                controlY,
                CheckboxControlSize,
                CheckboxControlSize);
            Rect resetRect = new Rect(
                checkRect.x - FeatureResetButtonGap - FeatureResetButtonWidth,
                controlY,
                FeatureResetButtonWidth,
                CheckboxControlSize);
            bool hasErrorTrace = !errorTrace.NullOrEmpty();
            if (hasErrorTrace)
            {
                Rect copyRect = new Rect(
                    resetRect.x - FixErrorTraceUi.CopyButtonGap - FixErrorTraceUi.CopyButtonSize,
                    controlY,
                    FixErrorTraceUi.CopyButtonSize,
                    FixErrorTraceUi.CopyButtonSize);
                FixErrorTraceUi.DrawCopyButton(copyRect, errorTrace, errorTraceTipId);
            }

            bool supersededActive = SupersededStandaloneMods.IsActive(supersededPackageId);

            string tip = tooltip ?? string.Empty;
            if (supersededActive)
            {
                if (!tip.NullOrEmpty())
                {
                    tip += "\n\n";
                }

                tip += "KebabTweaks.SupersededStandaloneTooltip".Translate();
            }
            else if (requiresRestart)
            {
                if (!tip.NullOrEmpty())
                {
                    tip += "\n\n";
                }

                tip += "KebabTweaks.RequiresRestartTooltip".Translate();
            }

            if (!tip.NullOrEmpty())
            {
                Rect headerTipRect = row;
                if (hasErrorTrace)
                {
                    headerTipRect.width -= HeaderControlsRightInset
                        + CheckboxControlSize
                        + FeatureResetButtonGap
                        + FeatureResetButtonWidth
                        + FixErrorTraceUi.CopyButtonReservedWidth;
                }

                if (Mouse.IsOver(headerTipRect))
                {
                    Widgets.DrawHighlight(headerTipRect);
                }

                TooltipHandler.TipRegion(headerTipRect, tip);
            }

            DrawFeatureHeaderLabel(row, label, supersededActive, differsFromDefault: false);
            DrawFeatureResetButton(resetRect, label, resetAction);
            Widgets.Checkbox(new Vector2(checkRect.x, checkRect.y), ref value, CheckboxControlSize);
            if (value != defaultEnabled)
            {
                DrawNonDefaultTextUnderline(row, label, TextAnchor.MiddleCenter);
            }

            // After restart Applied* matches Enable* — outline stays off until the player toggles again.
            // Superseded silent-skip keeps Applied* aligned with Enable*, so no yellow outline then.
            if (!supersededActive && requiresRestart && value != appliedAtLoad)
            {
                DrawInnerBoxBorder(checkRect, 2f, RestartPendingOutlineColor);
            }

            listing.Gap(SettingsCheckboxRowGap);
        }

        private static void DrawFeatureHeaderLabel(
            Rect row,
            string label,
            bool supersededActive,
            bool differsFromDefault)
        {
            TextAnchor previousAnchor = Text.Anchor;
            Color previousColor = GUI.color;
            FontStyle previousStyle = Text.CurFontStyle.fontStyle;
            Text.Anchor = TextAnchor.MiddleCenter;
            if (supersededActive)
            {
                GUI.color = SupersededHeaderColor;
                Text.CurFontStyle.fontStyle = FontStyle.Bold;
            }

            Widgets.Label(row, label);
            Text.CurFontStyle.fontStyle = previousStyle;
            GUI.color = previousColor;
            Text.Anchor = previousAnchor;

            if (differsFromDefault)
            {
                DrawNonDefaultTextUnderline(row, label, TextAnchor.MiddleCenter);
            }
        }

        private void DrawFeatureResetButton(Rect rect, string featureLabel, Action resetAction)
        {
            if (Widgets.ButtonText(rect, "KebabTweaks.ResetFeature".Translate()))
            {
                Find.WindowStack.Add(new Dialog_MessageBox(
                    "KebabTweaks.ResetFeatureConfirm".Translate(featureLabel.Named("FEATURE")),
                    "Confirm".Translate(),
                    () =>
                    {
                        resetAction();
                        Write();
                    },
                    "Cancel".Translate(),
                    null,
                    "KebabTweaks.ResetFeature".Translate(),
                    buttonADestructive: true));
            }
        }

        private static void DrawSettingsCheckboxHeightSpacer(Listing_Standard listing)
        {
            listing.GetRect(SettingsCheckboxRowHeight);
        }

        private static void DrawSettingsSectionHeader(Listing_Standard listing, string label)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect row = listing.GetRect(SettingsSectionHeaderHeight);
            Widgets.Label(row, label);
            Text.Anchor = TextAnchor.UpperLeft;
            listing.Gap(SettingsCheckboxRowGap);
        }

        /// <summary>
        /// Main feature/patch header fill only: black like tabs, alpha 0 at top → tab opacity
        /// (~80%) at bottom, via a bilinear texture (smooth, no strip banding). Not used inside
        /// body panels.
        ///
        /// Только главные заголовки мода/патча: чёрный как у вкладок, альфа 0 сверху →
        /// непрозрачность вкладок (~80%) снизу, через bilinear-текстуру (без полос). Внутри
        /// панелей не используется.
        /// </summary>
        private static void DrawFeatureHeaderBackground(Rect row)
        {
            if (Event.current.type != EventType.Repaint || row.height <= 0.5f)
            {
                return;
            }

            Color previous = GUI.color;
            GUI.color = Color.white;
            // Top of row = transparent, bottom = tab opacity (matches pixel layout: y=0 opaque).
            GUI.DrawTexture(row, GetFeatureHeaderAlphaGradientTex());
            GUI.color = previous;
        }

        private static Texture2D GetFeatureHeaderAlphaGradientTex()
        {
            if (featureHeaderAlphaGradientTex != null)
            {
                return featureHeaderAlphaGradientTex;
            }

            const int height = 128;
            Texture2D tex = new Texture2D(1, height, TextureFormat.ARGB32, mipChain: false)
            {
                name = "KebabTweaks.FeatureHeaderAlphaGradient",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };

            // Unity Texture2D (0,0) is bottom-left; GUI.DrawTexture maps that to the bottom of the rect.
            Color bottom = SettingsTabFillColor;
            Color[] pixels = new Color[height];
            for (int y = 0; y < height; y++)
            {
                float tFromBottom = height <= 1 ? 0f : (float)y / (height - 1);
                float alpha = Mathf.Lerp(bottom.a, 0f, tFromBottom);
                pixels[y] = new Color(bottom.r, bottom.g, bottom.b, alpha);
            }

            tex.SetPixels(pixels);
            tex.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            featureHeaderAlphaGradientTex = tex;
            return featureHeaderAlphaGradientTex;
        }

        private static void DrawSettingsRowSeparator(Listing_Standard listing, float fullWidth)
        {
            Rect lineRect = listing.GetRect(1f);
            lineRect.x = 0f;
            lineRect.width = fullWidth;
            if (Event.current.type == EventType.Repaint)
            {
                Color previousColor = GUI.color;
                GUI.color = SettingsRowSeparatorColor;
                GUI.DrawTexture(lineRect, BaseContent.WhiteTex);
                GUI.color = previousColor;
            }
        }

        private static void DrawGradientLine(Rect rect)
        {
            if (Event.current.type != EventType.Repaint || BlockSeparatorGradientColors.Length == 0)
            {
                return;
            }

            const float step = 2f;
            Color previous = GUI.color;
            for (float x = rect.xMin; x < rect.xMax; x += step)
            {
                float t = Mathf.InverseLerp(rect.xMin, rect.xMax, x);
                GUI.color = SampleGradientColor(t);
                GUI.DrawTexture(new Rect(x, rect.yMin, Mathf.Min(step, rect.xMax - x), rect.height),
                    BaseContent.WhiteTex);
            }

            GUI.color = previous;
        }

        private static Color SampleGradientColor(float position)
        {
            position = Mathf.Clamp01(position);
            if (BlockSeparatorGradientColors.Length <= 1)
            {
                return BlockSeparatorGradientColors[0];
            }

            float scaled = position * (BlockSeparatorGradientColors.Length - 1);
            int left = Mathf.FloorToInt(scaled);
            int right = Mathf.Min(left + 1, BlockSeparatorGradientColors.Length - 1);
            return Color.Lerp(BlockSeparatorGradientColors[left], BlockSeparatorGradientColors[right], scaled - left);
        }

        private static float CalcCheckboxRowHeight(Listing_Standard listing, string label)
        {
            float rowWidth = listing.ColumnWidth;
            if (bodyContentInset > 0f)
            {
                rowWidth -= bodyContentInset * 2f;
            }

            float labelWidth = Mathf.Max(10f, rowWidth - CheckboxControlSize - 6f);
            return Mathf.Max(SettingsCheckboxRowHeight, Text.CalcHeight(label, labelWidth));
        }

        private static void DrawSettingsCheckboxRow(
            Listing_Standard listing,
            string label,
            ref bool value,
            bool defaultValue,
            string tooltip = null)
        {
            Rect row = listing.GetRect(CalcCheckboxRowHeight(listing, label));
            ApplyBodyContentInset(ref row, listing.ColumnWidth);
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                TooltipHandler.TipRegion(row, tooltip);
            }

            Widgets.CheckboxLabeled(row, label, ref value);
            if (value != defaultValue)
            {
                DrawNonDefaultTextUnderline(row, label, TextAnchor.MiddleLeft);
            }

            listing.Gap(SettingsCheckboxRowGap);
        }

        /// <summary>
        /// Enable-logging checkbox: turning ON opens a confirm dialog (like feature reset); OFF is immediate.
        ///
        /// Чекбокс «Включить логирование»: включение — через confirm; выключение — без диалога.
        /// </summary>
        private void DrawSettingsEnableLoggingCheckboxRow(
            Listing_Standard listing,
            string label,
            Func<bool> getValue,
            Action<bool> setValue,
            string featureTooltip = null)
        {
            Rect row = listing.GetRect(CalcCheckboxRowHeight(listing, label));
            ApplyBodyContentInset(ref row, listing.ColumnWidth);
            if (!featureTooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(row))
                {
                    Widgets.DrawHighlight(row);
                }

                TooltipHandler.TipRegion(row, featureTooltip);
            }

            bool value = getValue();
            bool previous = value;
            Widgets.CheckboxLabeled(row, label, ref value);
            if (!previous && value)
            {
                value = false;
                string body = "KebabTweaks.EnableLoggingConfirm".Translate();
                if (!featureTooltip.NullOrEmpty())
                {
                    body += "\n\n" + featureTooltip;
                }

                Find.WindowStack.Add(new Dialog_MessageBox(
                    body,
                    "Confirm".Translate(),
                    () =>
                    {
                        setValue(true);
                        Write();
                    },
                    "Cancel".Translate(),
                    null,
                    "KebabTweaks.EnableLoggingConfirmTitle".Translate()));
            }
            else if (value != previous)
            {
                setValue(value);
            }

            if (getValue())
            {
                DrawNonDefaultTextUnderline(row, label, TextAnchor.MiddleLeft);
            }

            listing.Gap(SettingsCheckboxRowGap);
        }

        /// <summary>
        /// Light-blue underline under a setting label when the value differs from default.
        ///
        /// Голубая линия под текстом настройки, если значение отличается от дефолта.
        /// </summary>
        private static void DrawNonDefaultTextUnderline(Rect row, string text, TextAnchor anchor)
        {
            if (Event.current.type != EventType.Repaint || text.NullOrEmpty())
            {
                return;
            }

            Vector2 size = Text.CalcSize(text);
            float x;
            if (anchor == TextAnchor.MiddleCenter || anchor == TextAnchor.UpperCenter ||
                anchor == TextAnchor.LowerCenter)
            {
                x = row.x + (row.width - size.x) / 2f;
            }
            else
            {
                x = row.x;
            }

            float y = row.yMax - NonDefaultUnderlineThickness - 3f;
            Color previous = GUI.color;
            GUI.color = NonDefaultUnderlineColor;
            GUI.DrawTexture(new Rect(x, y, size.x, NonDefaultUnderlineThickness), BaseContent.WhiteTex);
            GUI.color = previous;
        }

        private static void DrawInnerBoxBorder(Rect rect, float thickness, Color color)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Color previousColor = GUI.color;
            GUI.color = color;
            Texture2D tex = BaseContent.WhiteTex;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), tex);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), tex);
            float innerHeight = rect.height - thickness * 2f;
            GUI.DrawTexture(new Rect(rect.x, rect.y + thickness, thickness, innerHeight), tex);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y + thickness, thickness, innerHeight), tex);
            GUI.color = previousColor;
        }

        public void ResetAllToDefaults()
        {
            ResetKebabSwitches();
            ResetCatCrazyTime();
            ResetCatFloorSleep();
            ResetCeRunForCoverDestFix();
            ResetCeProjectileNullSoundFix();
            ResetArmorRacksAssignFix();
            ResetStorageSettingsAllowedToAcceptFix();
            ResetFishTableTypeListFix();
            ResetGetActiveRitualsFix();
            ResetGrowerCutTrees();
            ResetIdleErrorWanderFix();
            ResetHospitalityGuestApparelOptimizeFix();
            ResetLowTpsPawnDump();
            ResetIdleWorkSearchCooldown();
            ResetPtgMedicalCare();
            ResetSeedsPleaseSowFix();
            ResetTakeFromMending();
            ResetTradeCaravanLordFix();
            ResetApparelPolicyLoadFix();
            ResetBillRenamePrefill();
            ResetRecipeProductDropLostLog();
            ResetAllowToolHaulUrgentlyNreFix();
            ResetDubsAnalyzerBeginUpdateFix();
            ResetDebugLogSplitterDragFix();
#if RIMWORLD_1_6
            ResetNeanderthalChiefLeaderFix();
            ResetMapPreviewRngBaselineFix();
            ResetUnifiedXmlPathFix();
            ResetMainMenuBgFitFix();
            ResetRimatomicsGuidancePanelFix();
#endif
            Write();
        }

        private static void ResetKebabSwitches()
        {
            KebabSwitchesEnableLogging = false;
            SuppressFilledMapMessage = false;
            SuppressGaveBirthMessage = false;
            SuppressAnimalIsPregnantMessage = false;
            SuppressMiscarriedStarvationMessage = false;
            SuppressMiscarriedPoorHealthMessage = false;
            SuppressSeasonBegunMessage = false;
            SuppressBillCompleteMessage = false;
            SuppressFullyHealedMessage = false;
            SuppressSocialFightMessage = false;
            SuppressNewBondRelationMessage = false;
            SuppressNewBondRelationNewNameMessage = false;
            SuppressFoodPoisoningMessage = false;
            SuppressRoamerLeavingMessage = false;
            SuppressHiveReproducedMessage = false;
            SuppressTraderCaravanLeavingMessage = false;
            SuppressTraderCaravanDismissedMessage = false;
            SuppressCompSpawnerSpawnedItemMessage = false;
            SuppressPlantDiedOfCold = false;
            SuppressPlantDiedOfRotUnharvested = false;
            SuppressPlantDiedOfRotLight = false;
            SuppressPlantDiedOfRot = false;
            SuppressPlantDiedOfPoison = false;
            SuppressPlantDiedOfBlight = false;
            SuppressPlantDiedOfPollution = false;
            SuppressPlantDiedOfNoPollution = false;
            SuppressPlantDiedOfRotPollutedTerrain = false;
            SuppressMinifiedTreeDied = false;
            SuppressRottedAwayInStorage = false;
            SuppressDeterioratedAway = false;
            SuppressWornApparelDeterioratedAway = false;
            EnsureSuppressLetterIds();
            SuppressLetterIds.Clear();
            EnsureSuppressAlertIds();
            SuppressAlertIds.Clear();
        }

        private static void ResetCatCrazyTime()
        {
            EnableCatCrazyTime = true;
            CatCrazyTimeEnableLogging = false;
        }

        private static void ResetCatFloorSleep()
        {
            EnableCatFloorSleep = true;
            CatFloorSleepEnableLogging = false;
        }

        private static void ResetCeRunForCoverDestFix()
        {
            EnableCeRunForCoverDestFix = true;
        }

        private static void ResetCeProjectileNullSoundFix()
        {
            EnableCeProjectileNullSoundFix = true;
        }

        private static void ResetArmorRacksAssignFix()
        {
            EnableArmorRacksAssignFix = true;
        }

        private static void ResetStorageSettingsAllowedToAcceptFix()
        {
            EnableStorageSettingsAllowedToAcceptFix = true;
        }

        private static void ResetFishTableTypeListFix()
        {
            EnableFishTableTypeListFix = true;
        }

        private static void ResetGetActiveRitualsFix()
        {
            EnableGetActiveRitualsFix = true;
        }

        private static void ResetGrowerCutTrees()
        {
            EnableGrowerCutTrees = true;
            GrowerCutTreesEnableLogging = false;
        }

        private static void ResetIdleErrorWanderFix()
        {
            EnableIdleErrorWanderFix = true;
        }

        private static void ResetHospitalityGuestApparelOptimizeFix()
        {
            EnableHospitalityGuestApparelOptimizeFix = true;
        }

        private static void ResetLowTpsPawnDump()
        {
            EnableLowTpsPawnDump = true;
            LowTpsEnableLogging = false;
            LowTpsColonistsOnly = false;
            LowTpsSkipWhenPaused = false;
            LowTpsLogStartupMessage = false;
            LowTpsThreshold = DefaultLowTpsThreshold;
            LowTpsLogCooldownSeconds = DefaultLowTpsLogCooldownSeconds;
            LowTpsThreshold_Buffer = null;
            LowTpsLogCooldownSeconds_Buffer = null;
        }

        private static void ResetIdleWorkSearchCooldown()
        {
            EnableIdleWorkSearchCooldown = true;
            IdleWorkSearchCooldownEnableLogging = false;
            IdleWorkSearchCooldownTicks = DefaultIdleWorkSearchCooldownTicks;
            IdleWorkSearchCooldownTicks_Buffer = null;
        }

        private static void ResetPtgMedicalCare()
        {
            EnablePtgMedicalCare = true;
        }

        private static void ResetSeedsPleaseSowFix()
        {
            EnableSeedsPleaseSowFix = true;
        }

        private static void ResetTakeFromMending()
        {
            EnableTakeFromMending = true;
            TakeFromMendingEnableLogging = false;
            TakeFromMendingShowMaintenanceMessages = false;
        }

        private static void ResetTradeCaravanLordFix()
        {
            EnableTradeCaravanLordFix = true;
            TradeCaravanLordEnableLogging = false;
        }

        private static void ResetApparelPolicyLoadFix()
        {
            EnableApparelPolicyLoadFix = true;
        }

        private static void ResetBillRenamePrefill()
        {
            EnableBillRenamePrefill = true;
        }

        private static void ResetRecipeProductDropLostLog()
        {
            EnableRecipeProductDropLostLog = true;
        }

        private static void ResetAllowToolHaulUrgentlyNreFix()
        {
            EnableAllowToolHaulUrgentlyNreFix = true;
        }

        private static void ResetDubsAnalyzerBeginUpdateFix()
        {
            EnableDubsAnalyzerBeginUpdateFix = true;
        }

        private static void ResetDebugLogSplitterDragFix()
        {
            EnableDebugLogSplitterDragFix = true;
        }

#if RIMWORLD_1_6
        private static void ResetNeanderthalChiefLeaderFix()
        {
            EnableNeanderthalChiefLeaderFix = true;
        }

        private static void ResetMapPreviewRngBaselineFix()
        {
            EnableMapPreviewRngBaselineFix = true;
        }

        private static void ResetUnifiedXmlPathFix()
        {
            EnableUnifiedXmlPathFix = true;
        }

        private static void ResetMainMenuBgFitFix()
        {
            EnableMainMenuBgFitFix = true;
        }

        private static void ResetRimatomicsGuidancePanelFix()
        {
            EnableRimatomicsGuidancePanelFix = true;
        }
#endif

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref EnableCatCrazyTime, "EnableCatCrazyTime", defaultValue: true);
            Scribe_Values.Look(ref EnableCatFloorSleep, "EnableCatFloorSleep", defaultValue: true);
            Scribe_Values.Look(ref EnableCeRunForCoverDestFix, "EnableCeRunForCoverDestFix", defaultValue: true);
            Scribe_Values.Look(ref EnableCeProjectileNullSoundFix, "EnableCeProjectileNullSoundFix", defaultValue: true);
            Scribe_Values.Look(ref EnableArmorRacksAssignFix, "EnableArmorRacksAssignFix", defaultValue: true);
            Scribe_Values.Look(ref EnableStorageSettingsAllowedToAcceptFix,
                "EnableStorageSettingsAllowedToAcceptFix", defaultValue: true);
            Scribe_Values.Look(ref EnableFishTableTypeListFix, "EnableFishTableTypeListFix", defaultValue: true);
            Scribe_Values.Look(ref EnableGetActiveRitualsFix, "EnableGetActiveRitualsFix", defaultValue: true);
            Scribe_Values.Look(ref EnableGrowerCutTrees, "EnableGrowerCutTrees", defaultValue: true);
            Scribe_Values.Look(ref EnableIdleErrorWanderFix, "EnableIdleErrorWanderFix", defaultValue: true);
            Scribe_Values.Look(ref EnableHospitalityGuestApparelOptimizeFix,
                "EnableHospitalityGuestApparelOptimizeFix", defaultValue: true);
            Scribe_Values.Look(ref EnableLowTpsPawnDump, "EnableLowTpsPawnDump", defaultValue: true);
            Scribe_Values.Look(ref EnableIdleWorkSearchCooldown, "EnableIdleWorkSearchCooldown",
                defaultValue: true);
            Scribe_Values.Look(ref EnablePtgMedicalCare, "EnablePtgMedicalCare", defaultValue: true);
            Scribe_Values.Look(ref EnableSeedsPleaseSowFix, "EnableSeedsPleaseSowFix", defaultValue: true);
            Scribe_Values.Look(ref EnableTakeFromMending, "EnableTakeFromMending", defaultValue: true);
            Scribe_Values.Look(ref EnableTradeCaravanLordFix, "EnableTradeCaravanLordFix", defaultValue: true);
            Scribe_Values.Look(ref EnableApparelPolicyLoadFix, "EnableApparelPolicyLoadFix", defaultValue: true);
            Scribe_Values.Look(ref EnableBillRenamePrefill, "EnableBillRenamePrefill", defaultValue: true);
            Scribe_Values.Look(ref EnableRecipeProductDropLostLog, "EnableRecipeProductDropLostLog", defaultValue: true);
            Scribe_Values.Look(ref EnableAllowToolHaulUrgentlyNreFix, "EnableAllowToolHaulUrgentlyNreFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableDubsAnalyzerBeginUpdateFix, "EnableDubsAnalyzerBeginUpdateFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableDebugLogSplitterDragFix, "EnableDebugLogSplitterDragFix",
                defaultValue: true);
#if RIMWORLD_1_6
            Scribe_Values.Look(ref EnableNeanderthalChiefLeaderFix, "EnableNeanderthalChiefLeaderFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableMapPreviewRngBaselineFix, "EnableMapPreviewRngBaselineFix",
                defaultValue: true);
            Scribe_Values.Look(ref EnableUnifiedXmlPathFix, "EnableUnifiedXmlPathFix", defaultValue: true);
            Scribe_Values.Look(ref EnableMainMenuBgFitFix, "EnableMainMenuBgFitFix", defaultValue: true);
            Scribe_Values.Look(ref EnableRimatomicsGuidancePanelFix, "EnableRimatomicsGuidancePanelFix",
                defaultValue: true);
#endif

            Scribe_Values.Look(ref CatCrazyTimeEnableLogging, "CatCrazyTimeEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref CatFloorSleepEnableLogging, "CatFloorSleepEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref GrowerCutTreesEnableLogging, "GrowerCutTreesEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref TakeFromMendingEnableLogging, "TakeFromMendingEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref TakeFromMendingShowMaintenanceMessages, "TakeFromMendingShowMaintenanceMessages",
                defaultValue: false);
            Scribe_Values.Look(ref TradeCaravanLordEnableLogging, "TradeCaravanLordEnableLogging", defaultValue: false);

            Scribe_Values.Look(ref LowTpsEnableLogging, "LowTpsEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref LowTpsColonistsOnly, "LowTpsColonistsOnly", defaultValue: false);
            Scribe_Values.Look(ref LowTpsSkipWhenPaused, "LowTpsSkipWhenPaused", defaultValue: false);
            Scribe_Values.Look(ref LowTpsLogStartupMessage, "LowTpsLogStartupMessage", defaultValue: false);
            Scribe_Values.Look(ref LowTpsThreshold, "LowTpsThreshold", defaultValue: DefaultLowTpsThreshold);
            Scribe_Values.Look(ref LowTpsLogCooldownSeconds, "LowTpsLogCooldownSeconds",
                defaultValue: DefaultLowTpsLogCooldownSeconds);

            Scribe_Values.Look(ref IdleWorkSearchCooldownEnableLogging,
                "IdleWorkSearchCooldownEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref IdleWorkSearchCooldownTicks, "IdleWorkSearchCooldownTicks",
                defaultValue: DefaultIdleWorkSearchCooldownTicks);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                IdleWorkSearchCooldownTicks = Mathf.Clamp(
                    IdleWorkSearchCooldownTicks, 0, MaxIdleWorkSearchCooldownTicks);
            }

            Scribe_Values.Look(ref KebabSwitchesEnableLogging, "KebabSwitchesEnableLogging", defaultValue: false);
            Scribe_Values.Look(ref SuppressFilledMapMessage, "SuppressFilledMapMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressGaveBirthMessage, "SuppressGaveBirthMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressAnimalIsPregnantMessage, "SuppressAnimalIsPregnantMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressMiscarriedStarvationMessage, "SuppressMiscarriedStarvationMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressMiscarriedPoorHealthMessage, "SuppressMiscarriedPoorHealthMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressSeasonBegunMessage, "SuppressSeasonBegunMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressBillCompleteMessage, "SuppressBillCompleteMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressFullyHealedMessage, "SuppressFullyHealedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressSocialFightMessage, "SuppressSocialFightMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressNewBondRelationMessage, "SuppressNewBondRelationMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressNewBondRelationNewNameMessage, "SuppressNewBondRelationNewNameMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressFoodPoisoningMessage, "SuppressFoodPoisoningMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressRoamerLeavingMessage, "SuppressRoamerLeavingMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressHiveReproducedMessage, "SuppressHiveReproducedMessage", defaultValue: false);
            Scribe_Values.Look(ref SuppressTraderCaravanLeavingMessage, "SuppressTraderCaravanLeavingMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressTraderCaravanDismissedMessage, "SuppressTraderCaravanDismissedMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressCompSpawnerSpawnedItemMessage, "SuppressCompSpawnerSpawnedItemMessage",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfCold, "SuppressPlantDiedOfCold", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfRotUnharvested, "SuppressPlantDiedOfRotUnharvested",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfRotLight, "SuppressPlantDiedOfRotLight", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfRot, "SuppressPlantDiedOfRot", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfPoison, "SuppressPlantDiedOfPoison", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfBlight, "SuppressPlantDiedOfBlight", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfPollution, "SuppressPlantDiedOfPollution", defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfNoPollution, "SuppressPlantDiedOfNoPollution",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressPlantDiedOfRotPollutedTerrain, "SuppressPlantDiedOfRotPollutedTerrain",
                defaultValue: false);
            Scribe_Values.Look(ref SuppressMinifiedTreeDied, "SuppressMinifiedTreeDied", defaultValue: false);
            Scribe_Values.Look(ref SuppressRottedAwayInStorage, "SuppressRottedAwayInStorage", defaultValue: false);
            Scribe_Values.Look(ref SuppressDeterioratedAway, "SuppressDeterioratedAway", defaultValue: false);
            Scribe_Values.Look(ref SuppressWornApparelDeterioratedAway, "SuppressWornApparelDeterioratedAway",
                defaultValue: false);
            Scribe_Collections.Look(ref SuppressLetterIds, "SuppressLetterIds", LookMode.Value);
            EnsureSuppressLetterIds();
            bool legacySurgeryFailedLetter = false;
            Scribe_Values.Look(ref legacySurgeryFailedLetter, "SuppressSurgeryFailedLetter", defaultValue: false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && legacySurgeryFailedLetter)
            {
                SuppressLetterIds.Add("SurgeryFailed");
            }

            Scribe_Collections.Look(ref SuppressAlertIds, "SuppressAlertIds", LookMode.Value);
            EnsureSuppressAlertIds();
        }
    }
}
