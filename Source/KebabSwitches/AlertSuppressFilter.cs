using System;

namespace HSK.KebabTweaks.KebabSwitches
{
    /// <summary>
    /// Matches live Alert instances to suppressible catalog entries by alert type name.
    ///
    /// Сопоставляет Alert с каталогом по имени типа alert.
    /// </summary>
    public static class AlertSuppressFilter
    {
        /// <summary>
        /// Returns catalog id (Alert_* suffix) for this alert type name, or null.
        ///
        /// Id каталога (суффикс Alert_*) для имени типа alert, либо null.
        /// </summary>
        public static string MatchTypeSuffix(string typeName)
        {
            if (string.IsNullOrEmpty(typeName) || !typeName.StartsWith("Alert_", StringComparison.Ordinal))
            {
                return null;
            }

            string suffix = typeName.Substring(6);
            for (int i = 0; i < SuppressibleAlerts.All.Length; i++)
            {
                if (string.Equals(SuppressibleAlerts.All[i].Id, suffix, StringComparison.Ordinal))
                {
                    return suffix;
                }
            }

            return null;
        }

        public static SuppressibleAlertEntry FindEntry(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            for (int i = 0; i < SuppressibleAlerts.All.Length; i++)
            {
                if (string.Equals(SuppressibleAlerts.All[i].Id, id, StringComparison.Ordinal))
                {
                    return SuppressibleAlerts.All[i];
                }
            }

            return null;
        }
    }
}
