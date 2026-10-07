using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: NewRatkin.StockGenerator_Mercenary.GenerateThings builds PawnGenerationRequest with
    /// default and sets only KindDef, Faction, Tile, and ForceAddFreeWarmLayerIfNeeded.
    /// PawnGenerationRequest.ValidateAndFix sets context to NonPlayer, but leaves
    /// AllowedDevelopmentalStages at None. GenerateRandomAge then fails
    /// AgeAllowed (None matches no stage, and RatkinMercenary minGenerationAge is 30) and after 300
    /// tries keeps the last age-curve sample, often a child. Apparel can then be refused.
    ///
    /// Fix: Prefix on PawnGenerator.GeneratePawn replaces that request with a real constructor call
    /// (NonPlayer, Adult, the same faction, tile, and warm-layer flag) when KindDef is RatkinMercenary
    /// and AllowedDevelopmentalStages is None. A request that already used the constructor has Adult
    /// stages and is left alone. No NewRatkin reference; a missing kind never matches.
    ///
    /// Проблема: NewRatkin.StockGenerator_Mercenary.GenerateThings собирает PawnGenerationRequest
    /// через default и задаёт только KindDef, Faction, Tile и ForceAddFreeWarmLayerIfNeeded.
    /// PawnGenerationRequest.ValidateAndFix ставит контекст NonPlayer, но оставляет
    /// AllowedDevelopmentalStages равным None. GenerateRandomAge не проходит
    /// AgeAllowed (None не содержит стадию, а у RatkinMercenary minGenerationAge 30) и после 300
    /// попыток оставляет последний сэмпл кривой возраста, часто ребёнка. Одежду после этого могут не надеть.
    ///
    /// Исправление: Prefix на PawnGenerator.GeneratePawn заменяет такой запрос настоящим конструктором
    /// (NonPlayer, Adult, та же фракция, tile и флаг тёплого слоя), если KindDef - RatkinMercenary
    /// и AllowedDevelopmentalStages равен None. Запрос, уже собранный конструктором, имеет стадии Adult
    /// и не трогается. Ссылки на NewRatkin нет; отсутствующий kind не совпадёт.
    /// </summary>
    public static class RatkinMercenaryAgeFixFeatures
    {
        /// <summary>
        /// Registers the GeneratePawn request repair.
        ///
        /// Подключает починку запроса в GeneratePawn.
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    AccessTools.Method(
                        typeof(PawnGenerator),
                        nameof(PawnGenerator.GeneratePawn),
                        new[] { typeof(PawnGenerationRequest) }),
                    prefix: new HarmonyMethod(
                        typeof(PawnGenerator_GeneratePawn_RatkinMercenaryAgeFix),
                        nameof(PawnGenerator_GeneratePawn_RatkinMercenaryAgeFix.Prefix)));

                Log.Message("[RatkinMercenaryAgeFix] Patches applied (RatkinMercenary request constructor).");
            }
            catch (Exception ex)
            {
                Log.Error("[RatkinMercenaryAgeFix] Failed to apply patches: " + ex);
            }
        }
    }

    /// <summary>
    /// Rebuilds a default RatkinMercenary request through the real constructor before generation.
    ///
    /// Пересобирает default-запрос RatkinMercenary настоящим конструктором до генерации.
    /// </summary>
    public static class PawnGenerator_GeneratePawn_RatkinMercenaryAgeFix
    {
        public static void Prefix(ref PawnGenerationRequest request)
        {
            if (!KebabTweaksSettings.EnableRatkinMercenaryAgeFix)
            {
                return;
            }

            if (request.AllowedDevelopmentalStages != DevelopmentalStage.None)
            {
                return;
            }

            PawnKindDef kind = request.KindDef;
            if (kind == null || kind.defName != "RatkinMercenary")
            {
                return;
            }

            request = new PawnGenerationRequest(
                kind,
                request.Faction,
                PawnGenerationContext.NonPlayer,
                tile: request.Tile,
                forceAddFreeWarmLayerIfNeeded: request.ForceAddFreeWarmLayerIfNeeded,
                developmentalStages: DevelopmentalStage.Adult);
        }
    }
}
