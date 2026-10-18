using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// One white Player.log line listing settings that differ from their defaults, then one white
    /// line whenever any setting changes. Always on: Enable logging and per-feature log checkboxes
    /// do not hide it. A missing-mod skip stays a yellow FeatureAvailability warning and is not
    /// repeated here.
    ///
    /// Одна белая строка Player.log с настройками, которые отличаются от значений по умолчанию,
    /// и ещё одна белая строка при каждом изменении любой настройки. Всегда включено: «Включить
    /// логирование» и логи отдельных фич это не прячут. Пропуск из-за отсутствующего мода остаётся
    /// жёлтым предупреждением FeatureAvailability и сюда не копируется.
    /// </summary>
    public static class SettingsAudit
    {
        const string Prefix = "[HSK kebab tweaks] ";
        const float SettleSeconds = 0.5f;

        enum Kind
        {
            Bool,
            Int,
            Float,
            Mode
        }

        sealed class Track
        {
            public string Token;
            public Func<int> Read;
            public Func<int> Default;
            public Kind Kind;
            public string[] Modes;
        }

        static readonly HashSet<string> Empty = new HashSet<string>(StringComparer.Ordinal);
        static readonly List<Track> tracks = Build();
        static readonly Func<HashSet<string>> readLetters = () => KebabTweaksSettings.SuppressLetterIds;
        static readonly Func<HashSet<string>> readAlerts = () => KebabTweaksSettings.SuppressAlertIds;

        static int[] scratch;
        static int[] committed;
        static int[] lastSeen;
        static HashSet<string> committedLetters = Empty;
        static HashSet<string> committedAlerts = Empty;
        static HashSet<string> lastLetters = Empty;
        static HashSet<string> lastAlerts = Empty;
        static bool installed;
        static bool armed;
        static bool pending;
        static bool pollFaulted;
        static float pendingSince;

        /// <summary>
        /// Watches both UI roots so a change is seen on the main menu and in a colony, including
        /// a write that did not go through the settings window.
        ///
        /// Следит за обоими UI root, чтобы изменение было видно и в главном меню, и в колонии,
        /// в том числе если значение записали не через окно настроек.
        /// </summary>
        public static void Install(Harmony harmony)
        {
            if (installed || harmony == null)
            {
                return;
            }

            installed = true;
            PatchRoot(harmony, typeof(UIRoot_Entry));
            PatchRoot(harmony, typeof(UIRoot_Play));
        }

        /// <summary>
        /// Writes the session line and starts change tracking. Call after the leftover reset and
        /// the one-shot restore so the line is the configuration this launch will use.
        ///
        /// Пишет строку этой сессии и включает слежение за изменениями. Вызывать после сброса
        /// устаревших и разового восстановления, чтобы строка совпала с конфигурацией этого запуска.
        /// </summary>
        public static void Publish()
        {
            if (armed)
            {
                return;
            }

            scratch = ReadInto(null);
            CopyTo(scratch, ref committed);
            CopyTo(scratch, ref lastSeen);
            committedLetters = CopySet(readLetters());
            committedAlerts = CopySet(readAlerts());
            lastLetters = CopySet(committedLetters);
            lastAlerts = CopySet(committedAlerts);
            pending = false;
            armed = true;
            Log.Message(Prefix + FormatStartup());
        }

        /// <summary>
        /// Checkboxes, modes, and ignore rows log on the next UI frame. Typed numbers and sliders
        /// wait until the value sits still, so one drag is one line from the previous value to the last.
        ///
        /// Чекбоксы, режимы и строки игнорирования пишутся на следующем кадре UI. Вводимые числа и
        /// ползунки ждут, пока значение перестанет меняться: одно перетаскивание - одна строка
        /// от прежнего значения к последнему.
        /// </summary>
        public static void Poll()
        {
            if (!armed || pollFaulted)
            {
                return;
            }

            try
            {
                scratch = ReadInto(scratch);
                bool intsMoved = !SameInts(scratch, lastSeen);
                bool setsMoved = SetsMoved();
                if (!intsMoved && !setsMoved)
                {
                    if (pending && Time.realtimeSinceStartup - pendingSince >= SettleSeconds)
                    {
                        Flush();
                    }

                    return;
                }

                bool debounced = intsMoved && DebouncedMoved(scratch, lastSeen);
                if (intsMoved)
                {
                    CopyTo(scratch, ref lastSeen);
                }

                if (setsMoved)
                {
                    lastLetters = CopySet(readLetters());
                    lastAlerts = CopySet(readAlerts());
                }

                if (debounced)
                {
                    pending = true;
                    pendingSince = Time.realtimeSinceStartup;
                    return;
                }

                Flush();
            }
            catch (Exception ex)
            {
                pollFaulted = true;
                Log.Error(Prefix + "Settings audit stopped: " + ex);
            }
        }

        static void PatchRoot(Harmony harmony, Type rootType)
        {
            MethodInfo method = AccessTools.Method(rootType, "UIRootOnGUI");
            if (method == null)
            {
                return;
            }

            harmony.Patch(method, postfix: new HarmonyMethod(typeof(SettingsAudit), nameof(Poll)));
        }

        static void Flush()
        {
            pending = false;
            HashSet<string> letters = readLetters();
            HashSet<string> alerts = readAlerts();
            if (SameInts(scratch, committed) && SameSet(letters, committedLetters) && SameSet(alerts, committedAlerts))
            {
                return;
            }

            string body = FormatChanges(letters, alerts);
            CopyTo(scratch, ref committed);
            committedLetters = CopySet(letters);
            committedAlerts = CopySet(alerts);
            if (!body.NullOrEmpty())
            {
                Log.Message(Prefix + body);
            }
        }

        static bool SetsMoved()
        {
            return !SameSet(readLetters(), lastLetters) || !SameSet(readAlerts(), lastAlerts);
        }

        static bool DebouncedMoved(int[] now, int[] prev)
        {
            if (prev == null)
            {
                return true;
            }

            for (int i = 0; i < tracks.Count; i++)
            {
                if (now[i] == prev[i])
                {
                    continue;
                }

                Kind kind = tracks[i].Kind;
                if (kind == Kind.Int || kind == Kind.Float)
                {
                    return true;
                }
            }

            return false;
        }

        static string FormatStartup()
        {
            var parts = new List<string>();
            for (int i = 0; i < tracks.Count; i++)
            {
                Track track = tracks[i];
                int value = committed[i];
                if (value == track.Default())
                {
                    continue;
                }

                parts.Add(track.Token + "=" + Format(track, value));
            }

            AppendSetStartup(parts, "letter:", committedLetters);
            AppendSetStartup(parts, "alert:", committedAlerts);
            return parts.Count == 0 ? "defaults" : string.Join(", ", parts.ToArray());
        }

        static string FormatChanges(HashSet<string> letters, HashSet<string> alerts)
        {
            var parts = new List<string>();
            for (int i = 0; i < tracks.Count; i++)
            {
                int before = committed[i];
                int after = scratch[i];
                if (before == after)
                {
                    continue;
                }

                Track track = tracks[i];
                parts.Add(track.Token + " " + Format(track, before) + " -> " + Format(track, after));
            }

            AppendSetChanges(parts, "letter:", committedLetters, letters);
            AppendSetChanges(parts, "alert:", committedAlerts, alerts);
            return parts.Count == 0 ? null : string.Join(", ", parts.ToArray());
        }

        static void AppendSetStartup(List<string> parts, string tokenPrefix, HashSet<string> ids)
        {
            foreach (string id in Sorted(ids))
            {
                parts.Add(tokenPrefix + id + "=on");
            }
        }

        static void AppendSetChanges(List<string> parts, string tokenPrefix, HashSet<string> before, HashSet<string> after)
        {
            foreach (string id in Sorted(before))
            {
                if (after == null || !after.Contains(id))
                {
                    parts.Add(tokenPrefix + id + " on -> off");
                }
            }

            foreach (string id in Sorted(after))
            {
                if (before == null || !before.Contains(id))
                {
                    parts.Add(tokenPrefix + id + " off -> on");
                }
            }
        }

        static List<string> Sorted(HashSet<string> ids)
        {
            var list = new List<string>();
            if (ids == null || ids.Count == 0)
            {
                return list;
            }

            foreach (string id in ids)
            {
                if (!id.NullOrEmpty())
                {
                    list.Add(id);
                }
            }

            list.Sort(StringComparer.Ordinal);
            return list;
        }

        static string Format(Track track, int bits)
        {
            switch (track.Kind)
            {
                case Kind.Bool:
                    return bits != 0 ? "on" : "off";
                case Kind.Float:
                    return (bits / 10000.0).ToString("0.####", CultureInfo.InvariantCulture);
                case Kind.Mode:
                    if (track.Modes != null && bits >= 0 && bits < track.Modes.Length)
                    {
                        return track.Modes[bits];
                    }

                    return bits.ToString(CultureInfo.InvariantCulture);
                default:
                    return bits.ToString(CultureInfo.InvariantCulture);
            }
        }

        static int Quant(float value)
        {
            return (int)Math.Round(value * 10000.0, MidpointRounding.AwayFromZero);
        }

        static int[] ReadInto(int[] dest)
        {
            if (dest == null || dest.Length != tracks.Count)
            {
                dest = new int[tracks.Count];
            }

            for (int i = 0; i < tracks.Count; i++)
            {
                dest[i] = tracks[i].Read();
            }

            return dest;
        }

        static void CopyTo(int[] src, ref int[] dest)
        {
            if (dest == null || dest.Length != src.Length)
            {
                dest = new int[src.Length];
            }

            Array.Copy(src, dest, src.Length);
        }

        static bool SameInts(int[] left, int[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        static bool SameSet(HashSet<string> live, HashSet<string> snap)
        {
            bool liveEmpty = live == null || live.Count == 0;
            bool snapEmpty = snap == null || snap.Count == 0;
            if (liveEmpty || snapEmpty)
            {
                return liveEmpty && snapEmpty;
            }

            if (live.Count != snap.Count)
            {
                return false;
            }

            foreach (string id in live)
            {
                if (!snap.Contains(id))
                {
                    return false;
                }
            }

            return true;
        }

        static HashSet<string> CopySet(HashSet<string> src)
        {
            if (src == null || src.Count == 0)
            {
                return Empty;
            }

            return new HashSet<string>(src, StringComparer.Ordinal);
        }

        static void Bool(List<Track> list, string token, Func<bool> read, bool fallback)
        {
            Bool(list, token, read, () => fallback);
        }

        static void Bool(List<Track> list, string token, Func<bool> read, Func<bool> fallback)
        {
            list.Add(new Track
            {
                Token = token,
                Read = () => read() ? 1 : 0,
                Default = () => fallback() ? 1 : 0,
                Kind = Kind.Bool
            });
        }

        static void Int(List<Track> list, string token, Func<int> read, int fallback)
        {
            list.Add(new Track
            {
                Token = token,
                Read = read,
                Default = () => fallback,
                Kind = Kind.Int
            });
        }

        static void Float(List<Track> list, string token, Func<float> read, float fallback)
        {
            int bits = Quant(fallback);
            list.Add(new Track
            {
                Token = token,
                Read = () => Quant(read()),
                Default = () => bits,
                Kind = Kind.Float
            });
        }

        static void Mode(List<Track> list, string token, Func<int> read, int fallback, params string[] names)
        {
            list.Add(new Track
            {
                Token = token,
                Read = read,
                Default = () => fallback,
                Kind = Kind.Mode,
                Modes = names
            });
        }

        static void Msg(List<Track> list, string token, Func<bool> read)
        {
            Bool(list, "msg:" + token, read, false);
        }

        static List<Track> Build()
        {
            var list = new List<Track>(160);
            Bool(list, "low-tps", () => KebabTweaksSettings.EnableLowTpsPawnDump, true);
            Bool(list, "low-tps:log", () => KebabTweaksSettings.LowTpsEnableLogging, false);
            Bool(list, "low-tps:colonists", () => KebabTweaksSettings.LowTpsColonistsOnly, false);
            Bool(list, "low-tps:paused", () => KebabTweaksSettings.LowTpsSkipWhenPaused, false);
            Bool(list, "low-tps:startup", () => KebabTweaksSettings.LowTpsLogStartupMessage, false);
            Float(list, "low-tps:threshold", () => KebabTweaksSettings.LowTpsThreshold,
                KebabTweaksSettings.DefaultLowTpsThreshold);
            Float(list, "low-tps:cooldown", () => KebabTweaksSettings.LowTpsLogCooldownSeconds,
                KebabTweaksSettings.DefaultLowTpsLogCooldownSeconds);

            Bool(list, "drill", () => KebabTweaksSettings.EnableDrillLimitPatch, true);
            Bool(list, "drill:value", () => KebabTweaksSettings.EnableDrillLimitValueField, true);
            Bool(list, "drill:any", () => KebabTweaksSettings.EnableDrillLimitAnyResource, true);
            Int(list, "drill:max", () => KebabTweaksSettings.DrillLimitMax, KebabTweaksSettings.DefaultDrillLimitMax);

            Bool(list, "idle-search", () => KebabTweaksSettings.EnableIdleWorkSearchCooldown, true);
            Bool(list, "idle-search:log", () => KebabTweaksSettings.IdleWorkSearchCooldownEnableLogging, false);
            Bool(list, "paranoid", () => KebabTweaksSettings.EnableParanoidPerfectionist, false);
            Bool(list, "paranoid:critical", () => KebabTweaksSettings.ParanoidCatchCritical, true);
            Bool(list, "paranoid:any", () => KebabTweaksSettings.ParanoidCatchAnyError, false);
            Int(list, "idle-search:ticks", () => KebabTweaksSettings.IdleWorkSearchCooldownTicks,
                KebabTweaksSettings.DefaultIdleWorkSearchCooldownTicks);

            Bool(list, "grower", () => KebabTweaksSettings.EnableGrowerCutTrees, true);
            Bool(list, "grower:log", () => KebabTweaksSettings.GrowerCutTreesEnableLogging, false);

            Bool(list, "corpses", () => KebabTweaksSettings.EnableStartingCorpsesNoHaul, true);
            Mode(list, "corpses:mark", () => KebabTweaksSettings.StartingCorpseMarkMode,
                KebabTweaksSettings.DefaultStartingCorpseMarkMode, "nohaul", "forbid", "both");

            Bool(list, "apparel", () => KebabTweaksSettings.EnableApparelPolicyLoadFix, true);
            Bool(list, "bill-rename", () => KebabTweaksSettings.EnableBillRenamePrefill, true);
            Bool(list, "recipe-drop", () => KebabTweaksSettings.EnableRecipeProductDropLostLog, true);
            Bool(list, "numbers", () => KebabTweaksSettings.EnableNumbersLoadDefaultFallback, true);
            Bool(list, "android", () => KebabTweaksSettings.EnableAndroidEnlightenFix, true);

            Bool(list, "wikirim", () => KebabTweaksSettings.EnableWikiRimCompat, true);
            Bool(list, "wikirim:multi", () => KebabTweaksSettings.EnableWikiRimMultilingualSearch, false);

            Bool(list, "mending", () => KebabTweaksSettings.EnableTakeFromMending, true);
            Bool(list, "mending:log", () => KebabTweaksSettings.TakeFromMendingEnableLogging, false);
            Bool(list, "mending:msg", () => KebabTweaksSettings.TakeFromMendingShowMaintenanceMessages, false);

            Bool(list, "cat-crazy", () => KebabTweaksSettings.EnableCatCrazyTime,
                () => !FixSymptomProbe.IsCatCrazyTimeLeftover());
            Bool(list, "cat-crazy:log", () => KebabTweaksSettings.CatCrazyTimeEnableLogging, false);
            Bool(list, "cat-floor", () => KebabTweaksSettings.EnableCatFloorSleep, true);
            Bool(list, "cat-floor:log", () => KebabTweaksSettings.CatFloorSleepEnableLogging, false);
            Bool(list, "trade-lord", () => KebabTweaksSettings.EnableTradeCaravanLordFix, true);
            Bool(list, "trade-lord:log", () => KebabTweaksSettings.TradeCaravanLordEnableLogging, false);
            Bool(list, "compressible", () => KebabTweaksSettings.EnableCompressibleOverlapFix, true);
            Bool(list, "compressible:log", () => KebabTweaksSettings.CompressibleOverlapFixEnableLogging, false);
            Bool(list, "trader-stay", () => KebabTweaksSettings.EnableTraderCaravanStayFix, true);
            Bool(list, "ratkin", () => KebabTweaksSettings.EnableRatkinMercenaryAgeFix, true);
            Bool(list, "armor-racks", () => KebabTweaksSettings.EnableArmorRacksAssignFix, true);
            Bool(list, "save-settings", () => KebabTweaksSettings.EnableSaveSettingsLoadFix, true);
            Bool(list, "ce-cover", () => KebabTweaksSettings.EnableCeRunForCoverDestFix, true);
            Bool(list, "ce-projectile", () => KebabTweaksSettings.EnableCeProjectileNullSoundFix, true);
            Bool(list, "ce-loadout", () => KebabTweaksSettings.EnableCeExtendedLoadoutMedicineLabelFix, true);
            Bool(list, "fish-table", () => KebabTweaksSettings.EnableFishTableTypeListFix, true);
            Bool(list, "rituals", () => KebabTweaksSettings.EnableGetActiveRitualsFix, true);
            Bool(list, "idle-wander", () => KebabTweaksSettings.EnableIdleErrorWanderFix, true);
            Bool(list, "hospitality", () => KebabTweaksSettings.EnableHospitalityGuestApparelOptimizeFix, true);
            Bool(list, "allow-tool", () => KebabTweaksSettings.EnableAllowToolHaulUrgentlyNreFix, true);
            Bool(list, "dubs", () => KebabTweaksSettings.EnableDubsAnalyzerBeginUpdateFix, true);
            Bool(list, "debug-log", () => KebabTweaksSettings.EnableDebugLogSplitterDragFix, true);
            Bool(list, "ptg", () => KebabTweaksSettings.EnablePtgMedicalCare, true);
            Bool(list, "seeds", () => KebabTweaksSettings.EnableSeedsPleaseSowFix,
                () => !FixSymptomProbe.IsSeedsPleaseSowFixLeftover());
            Bool(list, "chief", () => KebabTweaksSettings.EnableNeanderthalChiefLeaderFix,
                () => !FixSymptomProbe.IsNeanderthalChiefLeaderFixLeftover());
            Bool(list, "craft-stuff", () => KebabTweaksSettings.EnableDominantIngredientStuffFix,
                () => !FixSymptomProbe.IsCraftStuffFixLeftover());
            Bool(list, "child-age", () => KebabTweaksSettings.EnableStartingPawnChildAgeFix,
                () => !FixSymptomProbe.IsStartingPawnChildAgeFixLeftover());
            Bool(list, "pawn-name", () => KebabTweaksSettings.EnableStartingPawnNameFix,
                () => !FixSymptomProbe.IsStartingPawnNameFixLeftover());

            Bool(list, "uf-fill", () => KebabTweaksSettings.EnableUfFillExtraIngredientsFix,
                () => !FixSymptomProbe.IsUfFillFixLeftover());
            Bool(list, "uf-fill:cap", () => KebabTweaksSettings.EnableUfFillCapPickupToBillCount, true);
            Bool(list, "uf-fill:log", () => KebabTweaksSettings.UfFillExtraIngredientsFixEnableLogging, false);
            Bool(list, "burn", () => KebabTweaksSettings.EnableBurnWeaponBillFix,
                () => !FixSymptomProbe.IsBurnWeaponBillFixLeftover());
            Bool(list, "breach", () => KebabTweaksSettings.EnableBreachAxeWorkAmountFix,
                () => !FixSymptomProbe.IsBreachAxeWorkAmountFixLeftover());
            Bool(list, "fungus", () => KebabTweaksSettings.EnableRawFungusBillFix,
                () => !FixSymptomProbe.IsRawFungusBillFixLeftover());
            Bool(list, "anomaly", () => KebabTweaksSettings.EnableAnomalyEspFleeFix,
                () => !FixSymptomProbe.IsAnomalyEspFleeFixLeftover());
            Bool(list, "solar", () => KebabTweaksSettings.EnableStratumSolarRoofEnergyTabFix, true);
            Bool(list, "info-box", () => KebabTweaksSettings.EnableArchitectInfoBoxFpsFix, true);
            Bool(list, "map-preview", () => KebabTweaksSettings.EnableMapPreviewRngBaselineFix, true);
            Bool(list, "unified-xml", () => KebabTweaksSettings.EnableUnifiedXmlPathFix, true);
            Bool(list, "menu-bg", () => KebabTweaksSettings.EnableMainMenuBgFitFix,
                () => !FixSymptomProbe.IsMainMenuBgFitFixLeftover());
            Bool(list, "readout", () => KebabTweaksSettings.EnableToggleableReadoutsGuiFix,
                () => !FixSymptomProbe.IsToggleableReadoutsGuiFixLeftover());
            Bool(list, "rimatomics", () => KebabTweaksSettings.EnableRimatomicsGuidancePanelFix, true);

            Bool(list, "obsolete", () => KebabTweaksSettings.EnableObsoleteFixes, false);
            Bool(list, "obsolete:keep", () => KebabTweaksSettings.DoNotResetObsoleteFixesOnReload, false);

            Bool(list, "notify:log", () => KebabTweaksSettings.KebabSwitchesEnableLogging, false);
            Msg(list, "filled-map", () => KebabTweaksSettings.SuppressFilledMapMessage);
            Msg(list, "gave-birth", () => KebabTweaksSettings.SuppressGaveBirthMessage);
            Msg(list, "animal-pregnant", () => KebabTweaksSettings.SuppressAnimalIsPregnantMessage);
            Msg(list, "miscarried-starvation", () => KebabTweaksSettings.SuppressMiscarriedStarvationMessage);
            Msg(list, "miscarried-poor-health", () => KebabTweaksSettings.SuppressMiscarriedPoorHealthMessage);
            Msg(list, "season-begun", () => KebabTweaksSettings.SuppressSeasonBegunMessage);
            Msg(list, "bill-complete", () => KebabTweaksSettings.SuppressBillCompleteMessage);
            Msg(list, "fully-healed", () => KebabTweaksSettings.SuppressFullyHealedMessage);
            Msg(list, "social-fight", () => KebabTweaksSettings.SuppressSocialFightMessage);
            Msg(list, "ship-chunk", () => KebabTweaksSettings.SuppressShipChunkDropMessage);
            Msg(list, "struck-mineable", () => KebabTweaksSettings.SuppressStruckMineableMessage);
            Msg(list, "bed-lost-assignment", () => KebabTweaksSettings.SuppressBedLostAssignmentMessage);
            Msg(list, "bed-destroyed", () => KebabTweaksSettings.SuppressBedDestroyedMessage);
            Msg(list, "pawns-woken", () => KebabTweaksSettings.SuppressSleepingPawnsWokenUpMessage);
            Msg(list, "social-fight-ended", () => KebabTweaksSettings.SuppressNoLongerSocialFightingMessage);
            Msg(list, "insult-ended", () => KebabTweaksSettings.SuppressNoLongerTargetedInsultMessage);
            Msg(list, "insult-spree-ended", () => KebabTweaksSettings.SuppressInsultingSpreeEndedMessage);
            Msg(list, "insult-stream-ended", () => KebabTweaksSettings.SuppressTargetedInsultStreamEndedMessage);
            Msg(list, "food-binge-ended", () => KebabTweaksSettings.SuppressFoodBingeEndedMessage);
            Msg(list, "drug-binge-ended", () => KebabTweaksSettings.SuppressNoLongerBingingOnDrugMessage);
            Msg(list, "berserk-ended", () => KebabTweaksSettings.SuppressBerserkEndedMessage);
            Msg(list, "fire-spree-ended", () => KebabTweaksSettings.SuppressFireStartingEndedMessage);
            Msg(list, "jailbreak-ended", () => KebabTweaksSettings.SuppressJailbreakerEndedMessage);
            Msg(list, "slaughter-ended", () => KebabTweaksSettings.SuppressSlaughtererEndedMessage);
            Msg(list, "murderous-rage-ended", () => KebabTweaksSettings.SuppressMurderousRageEndedMessage);
            Msg(list, "give-up-returned", () => KebabTweaksSettings.SuppressGiveUpReturnedMessage);
            Msg(list, "tantrum-ended", () => KebabTweaksSettings.SuppressTantrumEndedMessage);
            Msg(list, "redirected-anger", () => KebabTweaksSettings.SuppressRedirectedAngerMessage);
            Msg(list, "sadistic-rage-ended", () => KebabTweaksSettings.SuppressSadisticRageEndedMessage);
            Msg(list, "corpse-obsession-ended", () => KebabTweaksSettings.SuppressCorpseObsessionEndedMessage);
            Msg(list, "wander-sad-ended", () => KebabTweaksSettings.SuppressWanderSadEndedMessage);
            Msg(list, "hide-in-room-ended", () => KebabTweaksSettings.SuppressHideInRoomEndedMessage);
            Msg(list, "psychotic-wander-ended", () => KebabTweaksSettings.SuppressPsychoticWanderEndedMessage);
            Msg(list, "confused-ended", () => KebabTweaksSettings.SuppressConfusedEndedMessage);
            Msg(list, "panic-flee-ended", () => KebabTweaksSettings.SuppressPanicFleeEndedMessage);
            Msg(list, "manhunter-ended", () => KebabTweaksSettings.SuppressManhunterEndedMessage);
            Msg(list, "new-bond", () => KebabTweaksSettings.SuppressNewBondRelationMessage);
            Msg(list, "new-bond-name", () => KebabTweaksSettings.SuppressNewBondRelationNewNameMessage);
            Msg(list, "food-poisoning", () => KebabTweaksSettings.SuppressFoodPoisoningMessage);
            Msg(list, "roamer-leaving", () => KebabTweaksSettings.SuppressRoamerLeavingMessage);
            Msg(list, "hive-reproduced", () => KebabTweaksSettings.SuppressHiveReproducedMessage);
            Msg(list, "trader-leaving", () => KebabTweaksSettings.SuppressTraderCaravanLeavingMessage);
            Msg(list, "trader-dismissed", () => KebabTweaksSettings.SuppressTraderCaravanDismissedMessage);
            Msg(list, "spawner-item", () => KebabTweaksSettings.SuppressCompSpawnerSpawnedItemMessage);
            Msg(list, "fish-trap", () => KebabTweaksSettings.SuppressFishCaughtInTrap);
            Msg(list, "plant-cold", () => KebabTweaksSettings.SuppressPlantDiedOfCold);
            Msg(list, "plant-rot-unharvested", () => KebabTweaksSettings.SuppressPlantDiedOfRotUnharvested);
            Msg(list, "plant-rot-light", () => KebabTweaksSettings.SuppressPlantDiedOfRotLight);
            Msg(list, "plant-rot", () => KebabTweaksSettings.SuppressPlantDiedOfRot);
            Msg(list, "plant-poison", () => KebabTweaksSettings.SuppressPlantDiedOfPoison);
            Msg(list, "plant-blight", () => KebabTweaksSettings.SuppressPlantDiedOfBlight);
            Msg(list, "plant-pollution", () => KebabTweaksSettings.SuppressPlantDiedOfPollution);
            Msg(list, "plant-no-pollution", () => KebabTweaksSettings.SuppressPlantDiedOfNoPollution);
            Msg(list, "plant-rot-polluted", () => KebabTweaksSettings.SuppressPlantDiedOfRotPollutedTerrain);
#if RIMWORLD_1_6
            Msg(list, "plant-vacuum", () => KebabTweaksSettings.SuppressPlantDiedOfVacuum);
            Msg(list, "esp-injured", () => KebabTweaksSettings.SuppressEspInjuredMessage);
#endif
            Msg(list, "minified-tree", () => KebabTweaksSettings.SuppressMinifiedTreeDied);
            Msg(list, "rotted-storage", () => KebabTweaksSettings.SuppressRottedAwayInStorage);
            Msg(list, "deteriorated", () => KebabTweaksSettings.SuppressDeterioratedAway);
            Msg(list, "apparel-deteriorated", () => KebabTweaksSettings.SuppressWornApparelDeterioratedAway);
            return list;
        }
    }
}
