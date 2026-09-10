using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: HSK lighting hediffs Enlighten_Bright and Enlighten_Dark are attached only to
    /// OrganicStandard. SK.Enlighten.Def.ResolveReferences adds HediffGiver_Enlighten there.
    /// Androids, droids, and Skynet pawns use ChjAndroidStandard, so those givers never run.
    /// HediffGiver_Enlighten only requires Spawned, Humanlike, and Awake.
    ///
    /// Fix: After defs load, copy OrganicStandard Enlighten givers onto ChjAndroidStandard.
    /// Live toggle removes the clones. Soft-skip if the android set is missing.
    ///
    /// Проблема: хеддифы Enlighten_Bright и Enlighten_Dark вешаются только на OrganicStandard.
    /// SK.Enlighten.Def.ResolveReferences добавляет HediffGiver_Enlighten туда. Андроиды,
    /// дроиды и пешки Skynet используют ChjAndroidStandard, поэтому гиверы не вызываются.
    /// HediffGiver_Enlighten требует только Spawned, Humanlike и Awake.
    ///
    /// Исправление: после загрузки дефов копирует гиверы Enlighten с OrganicStandard на
    /// ChjAndroidStandard. Выключение снимает клоны. Мягкий пропуск, если набора андроидов нет.
    /// </summary>
    public static class AndroidEnlightenFixFeatures
    {
        const string OrganicSetDefName = "OrganicStandard";
        const string AndroidSetDefName = "ChjAndroidStandard";
        const string GiverTypeName = "SK.Enlighten.HediffGiver_Enlighten";

        static readonly List<HediffGiver> injected = new List<HediffGiver>();
        static bool lastEnabled;
        static bool loggedMissingAndroidSet;

        public static void Apply(Harmony harmony)
        {
            try
            {
                SyncGivers(force: true);
                Log.Message("[AndroidEnlightenFixPatch] Android lighting givers synced.");
            }
            catch (Exception e)
            {
                Log.Error("[AndroidEnlightenFixPatch] Failed to apply: " + e);
            }
        }

        /// <summary>
        /// Copies OrganicStandard Enlighten givers onto ChjAndroidStandard, or removes clones
        /// this feature added.
        ///
        /// Копирует гиверы Enlighten с OrganicStandard на ChjAndroidStandard или снимает
        /// клоны, которые добавила эта фича.
        /// </summary>
        public static void SyncGivers(bool force = false)
        {
            bool enable = KebabTweaksSettings.EnableAndroidEnlightenFix;
            if (!force && enable == lastEnabled)
            {
                return;
            }

            Type giverType = AccessTools.TypeByName(GiverTypeName);
            HediffGiverSetDef organic = DefDatabase<HediffGiverSetDef>.GetNamedSilentFail(OrganicSetDefName);
            HediffGiverSetDef android = DefDatabase<HediffGiverSetDef>.GetNamedSilentFail(AndroidSetDefName);
            if (giverType == null || organic == null || organic.hediffGivers == null)
            {
                lastEnabled = enable;
                return;
            }

            if (android == null || android.hediffGivers == null)
            {
                if (!loggedMissingAndroidSet)
                {
                    loggedMissingAndroidSet = true;
                    Log.Message(
                        "[AndroidEnlightenFixPatch] ChjAndroidStandard not loaded; patch skipped.");
                }

                lastEnabled = enable;
                return;
            }

            if (enable)
            {
                for (int i = 0; i < organic.hediffGivers.Count; i++)
                {
                    HediffGiver source = organic.hediffGivers[i];
                    if (source == null || !giverType.IsInstanceOfType(source) || source.hediff == null)
                    {
                        continue;
                    }

                    if (HasEnlightenGiver(android, giverType, source.hediff))
                    {
                        continue;
                    }

                    HediffGiver clone = CloneEnlightenGiver(giverType, source);
                    if (clone == null)
                    {
                        continue;
                    }

                    android.hediffGivers.Add(clone);
                    injected.Add(clone);
                }
            }
            else
            {
                for (int i = android.hediffGivers.Count - 1; i >= 0; i--)
                {
                    HediffGiver giver = android.hediffGivers[i];
                    if (giver != null && injected.Contains(giver))
                    {
                        android.hediffGivers.RemoveAt(i);
                    }
                }

                injected.Clear();
            }

            lastEnabled = enable;
        }

        static bool HasEnlightenGiver(HediffGiverSetDef set, Type giverType, HediffDef hediff)
        {
            List<HediffGiver> givers = set.hediffGivers;
            for (int i = 0; i < givers.Count; i++)
            {
                HediffGiver giver = givers[i];
                if (giver != null && giverType.IsInstanceOfType(giver) && giver.hediff == hediff)
                {
                    return true;
                }
            }

            return false;
        }

        static HediffGiver CloneEnlightenGiver(Type giverType, HediffGiver source)
        {
            HediffGiver clone = Activator.CreateInstance(giverType) as HediffGiver;
            if (clone == null)
            {
                return null;
            }

            clone.hediff = source.hediff;
            CopyField(giverType, source, clone, "glowMin");
            CopyField(giverType, source, clone, "glowMax");
            return clone;
        }

        static void CopyField(Type giverType, HediffGiver source, HediffGiver dest, string fieldName)
        {
            var field = AccessTools.Field(giverType, fieldName);
            if (field == null)
            {
                return;
            }

            field.SetValue(dest, field.GetValue(source));
        }
    }
}
