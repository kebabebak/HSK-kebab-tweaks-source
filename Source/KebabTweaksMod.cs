using System;
using HarmonyLib;
using HSK.KebabTweaks.KebabSwitches;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Many small HSK QoL / fix patches were separate mods, so players had to juggle
    /// load order and could not toggle individual fixes from one settings panel.
    ///
    /// Fix: Bundle kebab switches and the listed Harmony fixes into one mod. Each feature keeps
    /// its own source file; settings expose per-patch enable + per-feature logging. Disabling
    /// this mod in the RimWorld mod list disables every bundled feature. If a superseded
    /// standalone package is still active, that feature's Harmony is silently skipped.
    ///
    /// Проблема: Много мелких HSK QoL / fix-патчей были отдельными модами — игроку приходилось
    /// следить за load order и нельзя было включать/выключать фиксы из одной панели настроек.
    ///
    /// Исправление: Собрать kebab switches и перечисленные Harmony-фиксы в один мод. Каждый
    /// feature остаётся в своём .cs; настройки дают enable патча и отдельное логирование.
    /// Выключение этого мода в списке модов RimWorld отключает все встроенные фичи. Если
    /// устаревший отдельный package всё ещё активен — Harmony этой фичи тихо пропускается.
    /// </summary>
    public class KebabTweaksMod : Mod
    {
        public const string HarmonyId = "kebabebak.hsk.kebab.tweaks";

        /// <summary>
        /// One Harmony instance for kebabebak.hsk.kebab.tweaks. Reused for the UXE early Prefix
        /// and the later feature apply pass so the same id is not constructed twice (Dubs Analyzer
        /// otherwise attributes the delayed LongEvent callback to mscorlib and warns).
        ///
        /// Один экземпляр Harmony для kebabebak.hsk.kebab.tweaks. Переиспользуется для раннего
        /// Prefix UXE и позднего apply фич, чтобы не создавать тот же id дважды (иначе Dubs Analyzer
        /// приписывает отложенный LongEvent к mscorlib и выдаёт предупреждение).
        /// </summary>
        private static Harmony harmony;

        private readonly KebabTweaksSettings settings;

        public KebabTweaksMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<KebabTweaksSettings>();
            harmony = new Harmony(HarmonyId);
#if RIMWORLD_1_6
            if (KebabTweaksSettings.EnableUnifiedXmlPathFix
                && !SupersededStandaloneMods.IsActive(SupersededStandaloneMods.UnifiedXmlPathFix))
            {
                KebabTweaksSettings.AppliedUnifiedXmlPathFix = true;
                try
                {
                    UnifiedXmlPathFixFeatures.ApplyEarly(harmony);
                }
                catch (Exception ex)
                {
                    Log.Error("[HSK kebab tweaks] Failed to apply Unified Xml path fix early: " + ex);
                }
            }
            else
            {
                KebabTweaksSettings.AppliedUnifiedXmlPathFix = KebabTweaksSettings.EnableUnifiedXmlPathFix;
            }
#endif
            LongEventHandler.ExecuteWhenFinished(ApplyAllFeatures);
        }

        /// <summary>
        /// Applies kebab switches and each bundled patch, skipping any feature whose superseded
        /// standalone mod is still active.
        ///
        /// Применяет kebab switches и каждый встроенный патч, пропуская фичи с активным
        /// устаревшим отдельным модом.
        /// </summary>
        private static void ApplyAllFeatures()
        {
            try
            {
                if (harmony == null)
                {
                    Log.Error("[HSK kebab tweaks] Harmony instance missing; feature apply skipped.");
                    return;
                }

                if (SupersededStandaloneMods.IsActive(SupersededStandaloneMods.KebabSwitches))
                {
                    Log.Message(
                        "[HSK kebab tweaks] KebabSwitchesFeatures silent-skipped (standalone still active: " +
                        SupersededStandaloneMods.KebabSwitches + ").");
                }
                else
                {
                    KebabSwitchesFeatures.Apply(harmony);
                }

                // Live-toggle (Harmony applied when not superseded; Enable* gated inside hooks).
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedCatCrazyTime,
                    KebabTweaksSettings.EnableCatCrazyTime, SupersededStandaloneMods.CatCrazyTime,
                    "CatCrazyTimeFeatures", () => CatCrazyTimeFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedCatFloorSleep,
                    KebabTweaksSettings.EnableCatFloorSleep, SupersededStandaloneMods.CatFloorSleep,
                    "CatFloorSleepFeatures", () => CatFloorSleepFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedArmorRacksAssignFix,
                    KebabTweaksSettings.EnableArmorRacksAssignFix,
                    SupersededStandaloneMods.ArmorRacksAssignFix, "ArmorRacksAssignFixFeatures",
                    () => ArmorRacksAssignFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedStorageSettingsAllowedToAcceptFix,
                    KebabTweaksSettings.EnableStorageSettingsAllowedToAcceptFix, null,
                    "StorageSettingsAllowedToAcceptFixFeatures",
                    () => StorageSettingsAllowedToAcceptFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedCeRunForCoverDestFix,
                    KebabTweaksSettings.EnableCeRunForCoverDestFix,
                    SupersededStandaloneMods.CeRunForCoverDestFix, "CeRunForCoverDestFixFeatures",
                    () => CeRunForCoverDestFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedCeProjectileNullSoundFix,
                    KebabTweaksSettings.EnableCeProjectileNullSoundFix, null,
                    "CeProjectileNullSoundFixFeatures",
                    () => CeProjectileNullSoundFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedFishTableTypeListFix,
                    KebabTweaksSettings.EnableFishTableTypeListFix,
                    SupersededStandaloneMods.FishTableTypeListFix, "FishTableTypeListFixFeatures",
                    () => FishTableTypeListFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedGetActiveRitualsFix,
                    KebabTweaksSettings.EnableGetActiveRitualsFix,
                    SupersededStandaloneMods.GetActiveRitualsFix, "GetActiveRitualsFixFeatures",
                    () => GetActiveRitualsFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedGrowerCutTrees,
                    KebabTweaksSettings.EnableGrowerCutTrees, SupersededStandaloneMods.GrowerCutTrees,
                    "GrowerCutTreesFeatures", () => GrowerCutTreesFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedIdleErrorWanderFix,
                    KebabTweaksSettings.EnableIdleErrorWanderFix,
                    SupersededStandaloneMods.IdleErrorWanderFix, "IdleErrorWanderFixFeatures",
                    () => IdleErrorWanderFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedHospitalityGuestApparelOptimizeFix,
                    KebabTweaksSettings.EnableHospitalityGuestApparelOptimizeFix, null,
                    "HospitalityGuestApparelOptimizeFixFeatures",
                    () => HospitalityGuestApparelOptimizeFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedLowTpsPawnDump,
                    KebabTweaksSettings.EnableLowTpsPawnDump, SupersededStandaloneMods.LowTpsPawnDump,
                    "LowTpsPawnDumpFeatures", () => LowTpsPawnDumpFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedIdleWorkSearchCooldown,
                    KebabTweaksSettings.EnableIdleWorkSearchCooldown, null,
                    "IdleWorkSearchCooldownFeatures",
                    () => IdleWorkSearchCooldownFeatures.Apply(harmony));
#if !RIMWORLD_1_6
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedTakeFromMending,
                    KebabTweaksSettings.EnableTakeFromMending, SupersededStandaloneMods.TakeFromMending,
                    "TakeFromMendingFeatures", () => TakeFromMendingFeatures.Apply(harmony));
#else
                KebabTweaksSettings.AppliedTakeFromMending = false;
#endif
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedApparelPolicyLoadFix,
                    KebabTweaksSettings.EnableApparelPolicyLoadFix, null,
                    "ApparelPolicyLoadFixFeatures", () => ApparelPolicyLoadFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedBillRenamePrefill,
                    KebabTweaksSettings.EnableBillRenamePrefill, null,
                    "BillRenamePrefillFeatures", () => BillRenamePrefillFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedRecipeProductDropLostLog,
                    KebabTweaksSettings.EnableRecipeProductDropLostLog, null,
                    "RecipeProductDropLostLogFeatures", () => RecipeProductDropLostLogFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedTradeCaravanLordFix,
                    KebabTweaksSettings.EnableTradeCaravanLordFix,
                    SupersededStandaloneMods.TradeCaravanLordFix, "TradeCaravanLordFixFeatures",
                    () => TradeCaravanLordFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedAllowToolHaulUrgentlyNreFix,
                    KebabTweaksSettings.EnableAllowToolHaulUrgentlyNreFix, null,
                    "AllowToolHaulUrgentlyNreFixFeatures",
                    () => AllowToolHaulUrgentlyNreFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedDubsAnalyzerBeginUpdateFix,
                    KebabTweaksSettings.EnableDubsAnalyzerBeginUpdateFix, null,
                    "DubsAnalyzerBeginUpdateFixFeatures",
                    () => DubsAnalyzerBeginUpdateFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedDebugLogSplitterDragFix,
                    KebabTweaksSettings.EnableDebugLogSplitterDragFix, null,
                    "DebugLogSplitterDragFixFeatures",
                    () => DebugLogSplitterDragFixFeatures.Apply(harmony));

                // Restart required (transpilers): apply only when enabled at load and not superseded.
                ApplyRestartUnlessSuperseded(ref KebabTweaksSettings.AppliedPtgMedicalCare,
                    KebabTweaksSettings.EnablePtgMedicalCare, SupersededStandaloneMods.PtgMedicalCare,
                    "PtgMedicalCareFeatures", () => PtgMedicalCareFeatures.Apply(harmony));
                ApplyRestartUnlessSuperseded(ref KebabTweaksSettings.AppliedSeedsPleaseSowFix,
                    KebabTweaksSettings.EnableSeedsPleaseSowFix,
                    SupersededStandaloneMods.SeedsPleaseSowFix, "SeedsPleaseSowFixFeatures",
                    () => SeedsPleaseSowFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedNeanderthalChiefLeaderFix,
                    KebabTweaksSettings.EnableNeanderthalChiefLeaderFix,
                    SupersededStandaloneMods.NeanderthalChiefLeaderFix,
                    "NeanderthalChiefLeaderFixFeatures",
                    () => NeanderthalChiefLeaderFixFeatures.Apply(harmony));
                ApplyLiveUnlessSuperseded(ref KebabTweaksSettings.AppliedDominantIngredientStuffFix,
                    KebabTweaksSettings.EnableDominantIngredientStuffFix, null,
                    "DominantIngredientStuffFixFeatures",
                    () => DominantIngredientStuffFixFeatures.Apply(harmony));

#if RIMWORLD_1_6
                // XML DefInjected + Rule_String typo safety (no settings toggle).
                OdysseyRuTradersGuildNamerFixFeatures.Apply(harmony);

                ApplyRestartUnlessSuperseded(ref KebabTweaksSettings.AppliedMapPreviewRngBaselineFix,
                    KebabTweaksSettings.EnableMapPreviewRngBaselineFix,
                    SupersededStandaloneMods.MapPreviewRngBaselineFix,
                    "MapPreviewRngBaselineFixFeatures",
                    () => MapPreviewRngBaselineFixFeatures.Apply(harmony));

                ApplyRestartUnlessSuperseded(ref KebabTweaksSettings.AppliedMainMenuBgFitFix,
                    KebabTweaksSettings.EnableMainMenuBgFitFix,
                    SupersededStandaloneMods.MainMenuBgFitFix,
                    "MainMenuBgFitFixFeatures",
                    () => MainMenuBgFitFixFeatures.Apply(harmony));

                ApplyRestartUnlessSuperseded(ref KebabTweaksSettings.AppliedRimatomicsGuidancePanelFix,
                    KebabTweaksSettings.EnableRimatomicsGuidancePanelFix,
                    SupersededStandaloneMods.RimatomicsGuidancePanelFix,
                    "RimatomicsGuidancePanelFixFeatures",
                    () => RimatomicsGuidancePanelFixFeatures.Apply(harmony));
#endif

                Log.Message("[HSK kebab tweaks] Feature apply pass finished.");
            }
            catch (Exception ex)
            {
                Log.Error("[HSK kebab tweaks] Failed to apply features: " + ex);
            }
        }

        private static void ApplyLiveUnlessSuperseded(
            ref bool appliedAtLoad,
            bool enabled,
            string supersededPackageId,
            string name,
            Action apply)
        {
            ApplyUnlessSuperseded(supersededPackageId, name, apply, restartStyle: false,
                ref appliedAtLoad, enabled);
        }

        private static void ApplyRestartUnlessSuperseded(
            ref bool appliedAtLoad,
            bool enabled,
            string supersededPackageId,
            string name,
            Action apply)
        {
            ApplyUnlessSuperseded(supersededPackageId, name, apply, restartStyle: true,
                ref appliedAtLoad, enabled);
        }

        /// <summary>
        /// Silent-skips Harmony when the superseded standalone package is active. Otherwise
        /// live patches always install hooks; restart patches install only when enabled.
        ///
        /// Тихо пропускает Harmony, если активен устаревший отдельный package. Иначе live
        /// всегда ставит хуки; restart — только если enable на загрузке.
        /// </summary>
        private static void ApplyUnlessSuperseded(
            string supersededPackageId,
            string name,
            Action apply,
            bool restartStyle,
            ref bool appliedAtLoad,
            bool enabled)
        {
            if (SupersededStandaloneMods.IsActive(supersededPackageId))
            {
                // Keep Applied* aligned with Enable* so yellow restart outline does not fire.
                appliedAtLoad = enabled;
                Log.Message(
                    $"[HSK kebab tweaks] {name} silent-skipped (standalone still active: {supersededPackageId}).");
                return;
            }

            if (restartStyle)
            {
                appliedAtLoad = enabled;
                if (!enabled)
                {
                    Log.Message(
                        $"[HSK kebab tweaks] {name} skipped (disabled in settings; restart to enable).");
                    return;
                }

                apply();
                return;
            }

            appliedAtLoad = enabled;
            apply();
        }

        public override string SettingsCategory()
        {
            return "KebabTweaks.SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DrawSettings(inRect);
        }
    }
}
