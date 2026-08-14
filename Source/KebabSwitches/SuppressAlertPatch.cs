using HarmonyLib;
using RimWorld;
using Verse;

using HSK.KebabTweaks;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Suppresses selected right-side HUD Alerts by forcing removal from AlertsReadout.activeAlerts.
    ///
    /// Подавляет выбранные Alert справа на HUD, принудительно убирая их из activeAlerts.
    /// </summary>
    [HarmonyPatch(typeof(AlertsReadout), "CheckAddOrRemoveAlert")]
    public static class SuppressAlertPatch
    {
        public static void Apply(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(AlertsReadout), "CheckAddOrRemoveAlert"),
                prefix: new HarmonyMethod(typeof(SuppressAlertPatch), nameof(Prefix)));
        }

        public static void Prefix(Alert alert, ref bool forceRemove)
        {
            if (forceRemove || alert == null)
            {
                return;
            }

            string suffix = AlertSuppressFilter.MatchTypeSuffix(alert.GetType().Name);
            if (suffix == null || !KebabTweaksSettings.IsAlertSuppressed(suffix))
            {
                return;
            }

            forceRemove = true;
            string label = alert.GetLabel();
            SwitchLog.Message(
                $"[HSK kebab tweaks][kebab switches] Suppressed alert ({suffix}): \"{label}\"");
        }
    }
}
