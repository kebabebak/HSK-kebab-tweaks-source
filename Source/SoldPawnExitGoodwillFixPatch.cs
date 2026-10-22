using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: Selling a tended prisoner runs Pawn.PreTraded and then
    /// Pawn_TraderTracker.AddPawnToStock. PreTraded drops prisoner status. AddPawnToStock sets
    /// the trader faction and Slave. Pawn.ExitMap then notifies that faction with freed false.
    /// Faction.GetGoodwillGainForExit still pays when the faction is not hostile and
    /// timesGuestTendedToByPlayer is above 0: +12, the tend count up to 10, and +40 for a
    /// faction leader. Tradeable_RoyalFavor already paid honor. The tribute collector letter
    /// grants honor only.
    ///
    /// Fix: Postfix on Pawn.PreTraded for TradeAction.PlayerSells sets
    /// timesGuestTendedToByPlayer to 0 and guest.Released to false. PlayerBuys is unchanged.
    /// GenGuest.GuestRelease does not call PreTraded, so releasing a prisoner still pays the
    /// old faction. The postfix no-ops unless IsSoldPawnExitGoodwillFixEnabled. The probe is
    /// Closed when SK.Patch_SoldPawnExitGoodwill.Pawn_PreTraded already stores 0 in that field
    /// and passes false to set_Released.
    ///
    /// Проблема: Продажа вылеченного пленника идёт через Pawn.PreTraded, затем
    /// Pawn_TraderTracker.AddPawnToStock. PreTraded снимает статус пленника. AddPawnToStock
    /// ставит фракцию торговца и Slave. Pawn.ExitMap сообщает этой фракции с freed ложным.
    /// Faction.GetGoodwillGainForExit всё равно платит, если фракция не враг и
    /// timesGuestTendedToByPlayer больше 0: +12, число лечений до 10 и +40 лидеру фракции.
    /// Tradeable_RoyalFavor уже дал честь. Письмо сборщика подати обещает только честь.
    ///
    /// Исправление: Postfix на Pawn.PreTraded при TradeAction.PlayerSells ставит
    /// timesGuestTendedToByPlayer в 0 и guest.Released в false. PlayerBuys не меняется.
    /// GenGuest.GuestRelease не вызывает PreTraded, поэтому отпуск пленника по-прежнему
    /// платит старой фракции. Postfix ничего не делает, пока выключен
    /// IsSoldPawnExitGoodwillFixEnabled. Зонд Closed, когда
    /// SK.Patch_SoldPawnExitGoodwill.Pawn_PreTraded уже пишет 0 в это поле и передаёт false
    /// в set_Released.
    /// </summary>
    public static class SoldPawnExitGoodwillFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Pawn), nameof(Pawn.PreTraded)),
                postfix: new HarmonyMethod(
                    typeof(SoldPawnExitGoodwillFixFeatures),
                    nameof(Pawn_PreTraded)));
            Log.Message("[HSK kebab tweaks] Sold prisoner relations fix loaded.");
        }

        /// <summary>
        /// Clears the tend count and Released after the player sells a pawn. Buying a pawn
        /// and a real release do not enter this branch.
        ///
        /// Обнуляет счётчик лечений и Released после продажи пешки игроком. Покупка пешки
        /// и настоящий отпуск в эту ветку не попадают.
        /// </summary>
        public static void Pawn_PreTraded(Pawn __instance, TradeAction action)
        {
            if (!KebabTweaksSettings.IsSoldPawnExitGoodwillFixEnabled())
            {
                return;
            }

            if (action != TradeAction.PlayerSells || __instance == null)
            {
                return;
            }

            if (__instance.mindState != null)
            {
                __instance.mindState.timesGuestTendedToByPlayer = 0;
            }

            if (__instance.guest != null)
            {
                __instance.guest.Released = false;
            }
        }
    }
}
