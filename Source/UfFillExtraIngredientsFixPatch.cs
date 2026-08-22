#if RIMWORLD_1_6
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HSK.KebabTweaks
{
    /// <summary>
    /// Problem: UniversalFermenterSK JobDriver_FillUF.CommitIngredients aborts when
    /// ValidatePlacedIngredients sees leftover count after allocating the recipe (RemainingCount
    /// must be 0 unless ignoreIngredientCountTakeEntireStacks). Haulers with high carry capacity
    /// (Misc. Robots cargo drones) pick extra matching stacks via JumpToCollectNextIntoHandsForBill
    /// or a full stack larger than the bill. Pawns usually carry closer to the required count
    /// because of mass, so they finish the fill. The job loops: walk to the building, abort, try again.
    ///
    /// Fix: Soft-optional Prefix on CommitIngredients reduces job.placedThings counts to the
    /// recipe requirement so validate/commit can finish. Optional cap clamps TryStartCarry and
    /// prunes the FillUF ingredient queue so extras never leave the source stack. Skips when
    /// Universal Fermenter is absent.
    ///
    /// Проблема: JobDriver_FillUF.CommitIngredients в UniversalFermenterSK отменяет работу, если
    /// ValidatePlacedIngredients видит остаток после набора рецепта (RemainingCount должен быть 0,
    /// если не ignoreIngredientCountTakeEntireStacks). Грузчики с большой грузоподъёмностью
    /// (грузовой дрон Misc. Robots) через JumpToCollectNextIntoHandsForBill или целый стак больше
    /// нормы берут лишнее. Пешки из-за массы обычно несут ближе к норме и загрузку завершают.
    /// Цикл: дойти до здания, обрыв, снова попытка.
    ///
    /// Исправление: Soft-optional Prefix на CommitIngredients уменьшает count в job.placedThings
    /// до нормы рецепта, чтобы проверка/запись прошли. Опция ограничения забора обрезает
    /// TryStartCarry и очередь ингредиентов FillUF, чтобы лишнее не уходило из исходного стака.
    /// Без Universal Fermenter патч пропускается.
    /// </summary>
    public static class UfFillExtraIngredientsFixFeatures
    {
        public static void Apply(Harmony harmony)
        {
            Type driverType = AccessTools.TypeByName("UniversalFermenterSK.JobDriver_FillUF");
            if (driverType == null)
            {
                Log.Message("[HSK kebab tweaks] Universal Fermenter not loaded; fermenter fill amounts fix skipped.");
                return;
            }

            MethodInfo commit = AccessTools.Method(driverType, "CommitIngredients", new[] { typeof(Pawn) });
            if (commit == null)
            {
                Log.Message(
                    "[HSK kebab tweaks] JobDriver_FillUF.CommitIngredients not found; fermenter fill amounts fix skipped.");
                return;
            }

            harmony.Patch(
                commit,
                prefix: new HarmonyMethod(
                    typeof(JobDriver_FillUF_CommitIngredients_Patch),
                    nameof(JobDriver_FillUF_CommitIngredients_Patch.Prefix)));

            MethodInfo tryStartCarry = AccessTools.Method(
                typeof(Pawn_CarryTracker),
                nameof(Pawn_CarryTracker.TryStartCarry),
                new[] { typeof(Thing), typeof(int), typeof(bool) });
            if (tryStartCarry != null)
            {
                harmony.Patch(
                    tryStartCarry,
                    prefix: new HarmonyMethod(
                        typeof(Pawn_CarryTracker_TryStartCarry_UfFillCap_Patch),
                        nameof(Pawn_CarryTracker_TryStartCarry_UfFillCap_Patch.Prefix)),
                    postfix: new HarmonyMethod(
                        typeof(Pawn_CarryTracker_TryStartCarry_UfFillCap_Patch),
                        nameof(Pawn_CarryTracker_TryStartCarry_UfFillCap_Patch.Postfix)));
            }
            else
            {
                Log.Message(
                    "[HSK kebab tweaks] Pawn_CarryTracker.TryStartCarry(Thing, int, bool) not found; fermenter fill pickup cap skipped.");
            }

            MethodInfo startCarryThing = AccessTools.Method(typeof(Toils_Haul), nameof(Toils_Haul.StartCarryThing));
            if (startCarryThing != null)
            {
                harmony.Patch(
                    startCarryThing,
                    postfix: new HarmonyMethod(
                        typeof(Toils_Haul_StartCarryThing_UfFillCap_Patch),
                        nameof(Toils_Haul_StartCarryThing_UfFillCap_Patch.Postfix)));
            }

            MethodInfo jumpToCollect = AccessTools.Method(
                typeof(JobDriver_DoBill),
                nameof(JobDriver_DoBill.JumpToCollectNextIntoHandsForBill));
            if (jumpToCollect != null)
            {
                harmony.Patch(
                    jumpToCollect,
                    postfix: new HarmonyMethod(
                        typeof(JobDriver_DoBill_JumpToCollect_UfFillCap_Patch),
                        nameof(JobDriver_DoBill_JumpToCollect_UfFillCap_Patch.Postfix)));
            }

            Log.Message(
                "[HSK kebab tweaks] Fermenter fill amounts fix loaded (verbose logging " +
                $"{(KebabTweaksSettings.UfFillExtraIngredientsFixEnableLogging ? "ON" : "OFF")}, " +
                $"cap pickup {(KebabTweaksSettings.EnableUfFillCapPickupToBillCount ? "ON" : "OFF")}).");
        }
    }

    /// <summary>
    /// Writes Log.Message only when UfFillExtraIngredientsFixEnableLogging is on.
    ///
    /// Пишет Log.Message только при включённом UfFillExtraIngredientsFixEnableLogging.
    /// </summary>
    internal static class UfFillExtraIngredientsFixLog
    {
        public static void Message(string text)
        {
            if (KebabTweaksSettings.UfFillExtraIngredientsFixEnableLogging)
            {
                Log.Message(text);
            }
        }
    }

    /// <summary>
    /// Before FillUF validate/commit, trims placedThings counts down to the bill requirement.
    ///
    /// Перед проверкой/записью FillUF обрезает count в placedThings до нормы задания.
    /// </summary>
    internal static class JobDriver_FillUF_CommitIngredients_Patch
    {
        public static void Prefix(Pawn actor)
        {
            if (!KebabTweaksSettings.EnableUfFillExtraIngredientsFix)
            {
                return;
            }

            if (actor == null)
            {
                return;
            }

            Job job = actor.CurJob;
            int before = UfFillExtraPlacedCounts.SumPlacedCounts(job);
            UfFillExtraPlacedCounts.TrimToRecipeNeed(job);
            int after = UfFillExtraPlacedCounts.SumPlacedCounts(job);
            if (after != before)
            {
                UfFillExtraIngredientsFixLog.Message(
                    $"[UfFillExtraIngredientsFix] Trimmed placed counts {before} -> {after} for {actor} " +
                    $"bill {job?.bill?.Label}.");
            }
        }
    }

    /// <summary>
    /// Clamps FillUF pickup count to the remaining bill need. If a hauler still grabs more,
    /// splits the extra back onto the source cell so it never rides to the fermenter.
    ///
    /// Ограничивает забор FillUF остатком нормы задания. Если грузчик всё же взял больше,
    /// отделяет лишнее обратно в клетку источника, чтобы оно не ехало к ферментеру.
    /// </summary>
    internal static class Pawn_CarryTracker_TryStartCarry_UfFillCap_Patch
    {
        public static void Prefix(Pawn_CarryTracker __instance, Thing item, ref int count)
        {
            if (!UfFillCarryCap.CapEnabled() || __instance?.pawn == null || item?.def == null)
            {
                return;
            }

            Job job = __instance.pawn.CurJob;
            if (!UfFillCarryCap.CanCapJob(job))
            {
                return;
            }

            int remaining = UfFillCarryCap.RemainingNeed(__instance.pawn, job, item.def);
            if (remaining <= 0)
            {
                return;
            }

            int original = count;
            if (count > remaining)
            {
                count = remaining;
            }

            if (job.count > remaining)
            {
                job.count = remaining;
            }

            if (count != original)
            {
                UfFillExtraIngredientsFixLog.Message(
                    $"[UfFillExtraIngredientsFix] Clamped pickup for {__instance.pawn} {item.def.defName} " +
                    $"{original} -> {count} (need {remaining}).");
            }
        }

        public static void Postfix(Pawn_CarryTracker __instance, Thing item)
        {
            if (!UfFillCarryCap.CapEnabled() || __instance?.pawn == null)
            {
                return;
            }

            Pawn pawn = __instance.pawn;
            Job job = pawn.CurJob;
            if (!UfFillCarryCap.CanCapJob(job))
            {
                return;
            }

            Thing carried = __instance.CarriedThing;
            if (carried?.def == null)
            {
                return;
            }

            int keep = UfFillCarryCap.MaxKeepOfDef(job, carried.def);
            if (keep <= 0 || carried.stackCount <= keep)
            {
                return;
            }

            int extra = carried.stackCount - keep;
            Thing split = carried.SplitOff(extra);
            if (split == null || split.Destroyed)
            {
                return;
            }

            Map map = pawn.Map;
            IntVec3 cell = pawn.Position;
            if (item != null && item.Spawned && item.Map == map)
            {
                cell = item.Position;
            }

            if (map == null || !GenPlace.TryPlaceThing(split, cell, map, ThingPlaceMode.Near))
            {
                UfFillExtraIngredientsFixLog.Message(
                    $"[UfFillExtraIngredientsFix] Could not return extra {split} for {pawn}; left in carry.");
                return;
            }

            UfFillExtraIngredientsFixLog.Message(
                $"[UfFillExtraIngredientsFix] Returned extra {extra} {carried.def.defName} at {cell} for {pawn}.");
        }
    }

    /// <summary>
    /// After FillUF StartCarryThing queues a stack remainder, drops that remainder from the
    /// ingredient queue when the bill need is already in hand.
    ///
    /// После того как FillUF StartCarryThing ставит остаток стака в очередь, убирает этот
    /// остаток из очереди ингредиентов, если норма задания уже в руках.
    /// </summary>
    internal static class Toils_Haul_StartCarryThing_UfFillCap_Patch
    {
        public static void Postfix(Toil __result)
        {
            if (__result == null)
            {
                return;
            }

            Action previous = __result.initAction;
            __result.initAction = delegate
            {
                previous?.Invoke();
                UfFillCarryCap.PruneQueueIfBillSatisfied(__result.actor);
            };
        }
    }

    /// <summary>
    /// Skips JumpToCollectNextIntoHandsForBill when FillUF already carries the bill need, and
    /// prunes leftover queue entries of that def.
    ///
    /// Пропускает JumpToCollectNextIntoHandsForBill, если FillUF уже несёт норму задания, и
    /// чистит лишние записи очереди этого def.
    /// </summary>
    internal static class JobDriver_DoBill_JumpToCollect_UfFillCap_Patch
    {
        public static void Postfix(Toil __result)
        {
            if (__result == null)
            {
                return;
            }

            Action previous = __result.initAction;
            __result.initAction = delegate
            {
                Pawn actor = __result.actor;
                if (UfFillCarryCap.HasEnoughOfCarried(actor))
                {
                    ThingDef def = actor.carryTracker.CarriedThing.def;
                    int removed = UfFillCarryCap.PruneQueuedOfDef(actor.CurJob, def);
                    UfFillExtraIngredientsFixLog.Message(
                        $"[UfFillExtraIngredientsFix] Skip further collect for {actor} {def.defName} " +
                        $"(pruned {removed} queue entries).");
                    return;
                }

                previous?.Invoke();
            };
        }
    }

    /// <summary>
    /// FillUF pickup-cap helpers: recipe need vs already carried/placed, queue prune.
    /// Uses the current bill recipe per ingredient def. Prune only removes the satisfied def
    /// so other queued ingredients stay.
    ///
    /// Помощники ограничения забора FillUF: норма рецепта против уже несомого/положенного,
    /// очистка очереди. Берёт рецепт текущего задания по каждому def. Из очереди убирается
    /// только закрытый def, остальные остаются.
    /// </summary>
    internal static class UfFillCarryCap
    {
        private const string FillUfJobDefName = "FillUniversalFermenter";

        public static bool CapEnabled()
        {
            return KebabTweaksSettings.EnableUfFillExtraIngredientsFix
                && KebabTweaksSettings.EnableUfFillCapPickupToBillCount;
        }

        public static bool IsFillUfJob(Job job)
        {
            return job?.def != null && job.def.defName == FillUfJobDefName;
        }

        public static bool CanCapJob(Job job)
        {
            if (!IsFillUfJob(job) || job.bill?.recipe == null)
            {
                return false;
            }

            RecipeDef recipe = job.bill.recipe;
            if (recipe.allowMixingIngredients || recipe.ignoreIngredientCountTakeEntireStacks)
            {
                return false;
            }

            return recipe.ingredients != null && recipe.ingredients.Count > 0;
        }

        public static int RequiredCountForDef(Job job, ThingDef def)
        {
            RecipeDef recipe = job.bill.recipe;
            int total = 0;
            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                IngredientCount requirement = recipe.ingredients[i];
                if (requirement == null || !requirement.filter.Allows(def))
                {
                    continue;
                }

                total += requirement.CountRequiredOfFor(def, recipe, job.bill);
            }

            return total;
        }

        public static int PlacedCountForDef(Job job, ThingDef def)
        {
            if (job.placedThings == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < job.placedThings.Count; i++)
            {
                ThingCountClass placed = job.placedThings[i];
                if (placed?.thing != null && placed.thing.def == def && placed.Count > 0)
                {
                    total += placed.Count;
                }
            }

            return total;
        }

        public static int CarriedCountForDef(Pawn pawn, ThingDef def)
        {
            Thing carried = pawn.carryTracker?.CarriedThing;
            if (carried == null || carried.def != def)
            {
                return 0;
            }

            return carried.stackCount;
        }

        public static int RemainingNeed(Pawn pawn, Job job, ThingDef def)
        {
            int required = RequiredCountForDef(job, def);
            int have = PlacedCountForDef(job, def) + CarriedCountForDef(pawn, def);
            int remaining = required - have;
            return remaining > 0 ? remaining : 0;
        }

        public static int MaxKeepOfDef(Job job, ThingDef def)
        {
            int keep = RequiredCountForDef(job, def) - PlacedCountForDef(job, def);
            return keep > 0 ? keep : 0;
        }

        public static bool HasEnoughOfCarried(Pawn pawn)
        {
            if (!CapEnabled() || pawn == null)
            {
                return false;
            }

            Job job = pawn.CurJob;
            if (!CanCapJob(job))
            {
                return false;
            }

            Thing carried = pawn.carryTracker?.CarriedThing;
            if (carried?.def == null)
            {
                return false;
            }

            return PlacedCountForDef(job, carried.def) + carried.stackCount
                >= RequiredCountForDef(job, carried.def);
        }

        public static void PruneQueueIfBillSatisfied(Pawn pawn)
        {
            if (!HasEnoughOfCarried(pawn))
            {
                return;
            }

            ThingDef def = pawn.carryTracker.CarriedThing.def;
            int removed = PruneQueuedOfDef(pawn.CurJob, def);
            if (removed > 0)
            {
                UfFillExtraIngredientsFixLog.Message(
                    $"[UfFillExtraIngredientsFix] Pruned {removed} queued leftover {def.defName} for {pawn}.");
            }
        }

        public static int PruneQueuedOfDef(Job job, ThingDef def)
        {
            if (job == null || def == null)
            {
                return 0;
            }

            List<LocalTargetInfo> queue = job.targetQueueB;
            if (queue == null || queue.Count == 0)
            {
                return 0;
            }

            List<int> counts = job.countQueue;
            int removed = 0;
            for (int i = queue.Count - 1; i >= 0; i--)
            {
                Thing thing = queue[i].Thing;
                if (thing == null || thing.def != def)
                {
                    continue;
                }

                queue.RemoveAt(i);
                if (counts != null && i < counts.Count)
                {
                    counts.RemoveAt(i);
                }

                removed++;
            }

            return removed;
        }
    }

    /// <summary>
    /// Lowers ThingCountClass.Count on extra placed stacks so ValidatePlacedIngredients sees an
    /// exact match. CalculateIngredients then SplitOff only that many; leftover stays spawned.
    ///
    /// Уменьшает ThingCountClass.Count у лишних положенных стаков, чтобы ValidatePlacedIngredients
    /// видел точное совпадение. CalculateIngredients делает SplitOff только этого числа; остаток
    /// остаётся на карте.
    /// </summary>
    internal static class UfFillExtraPlacedCounts
    {
        private const float IngredientValueTolerance = 0.0001f;

        private sealed class Portion
        {
            public Thing Thing;
            public int Remaining;
        }

        public static int SumPlacedCounts(Job job)
        {
            if (job?.placedThings == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < job.placedThings.Count; i++)
            {
                ThingCountClass placed = job.placedThings[i];
                if (placed != null && placed.Count > 0)
                {
                    total += placed.Count;
                }
            }

            return total;
        }

        public static void TrimToRecipeNeed(Job job)
        {
            if (job == null || job.bill == null || job.placedThings == null || job.placedThings.Count == 0)
            {
                return;
            }

            RecipeDef recipe = job.bill.recipe;
            if (recipe == null || recipe.ingredients == null || recipe.ingredients.Count == 0)
            {
                return;
            }

            if (recipe.ignoreIngredientCountTakeEntireStacks)
            {
                return;
            }

            List<Portion> portions = new List<Portion>(job.placedThings.Count);
            Dictionary<Thing, int> merged = new Dictionary<Thing, int>();
            for (int i = 0; i < job.placedThings.Count; i++)
            {
                ThingCountClass placed = job.placedThings[i];
                if (placed == null || placed.thing == null || placed.Count <= 0 || placed.thing.Destroyed)
                {
                    return;
                }

                int running;
                merged.TryGetValue(placed.thing, out running);
                int total = running + placed.Count;
                if (total > placed.thing.stackCount)
                {
                    return;
                }

                merged[placed.thing] = total;
            }

            foreach (KeyValuePair<Thing, int> pair in merged)
            {
                if (!job.bill.IsFixedOrAllowedIngredient(pair.Key))
                {
                    return;
                }

                portions.Add(new Portion
                {
                    Thing = pair.Key,
                    Remaining = pair.Value
                });
            }

            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                IngredientCount requirement = recipe.ingredients[i];
                bool allocated = recipe.allowMixingIngredients
                    ? AllocateMixed(portions, requirement, recipe, job.bill)
                    : AllocateSingleDef(portions, requirement, recipe, job.bill);
                if (!allocated)
                {
                    return;
                }
            }

            Dictionary<Thing, int> keepByThing = new Dictionary<Thing, int>();
            for (int i = 0; i < portions.Count; i++)
            {
                Portion portion = portions[i];
                int original;
                if (!merged.TryGetValue(portion.Thing, out original))
                {
                    continue;
                }

                int keep = original - portion.Remaining;
                if (keep < 0)
                {
                    keep = 0;
                }

                keepByThing[portion.Thing] = keep;
            }

            for (int i = job.placedThings.Count - 1; i >= 0; i--)
            {
                ThingCountClass placed = job.placedThings[i];
                if (placed == null || placed.thing == null)
                {
                    job.placedThings.RemoveAt(i);
                    continue;
                }

                int keep;
                if (!keepByThing.TryGetValue(placed.thing, out keep))
                {
                    continue;
                }

                int take = Mathf.Min(placed.Count, keep);
                placed.Count = take;
                keepByThing[placed.thing] = keep - take;
                if (placed.Count <= 0)
                {
                    job.placedThings.RemoveAt(i);
                }
            }
        }

        private static bool AllocateMixed(
            List<Portion> portions,
            IngredientCount requirement,
            RecipeDef recipe,
            Bill bill)
        {
            float need = recipe.Worker.GetIngredientCount(requirement, bill);
            for (int i = 0; i < portions.Count; i++)
            {
                Portion portion = portions[i];
                if (portion.Remaining <= 0 || !requirement.filter.Allows(portion.Thing))
                {
                    continue;
                }

                float perUnit = recipe.IngredientValueGetter.ValuePerUnitOf(portion.Thing.def);
                if (perUnit <= 0f)
                {
                    continue;
                }

                int want = Mathf.CeilToInt(need / perUnit);
                int take = Mathf.Min(portion.Remaining, want);
                portion.Remaining -= take;
                need -= take * perUnit;
                if (need <= IngredientValueTolerance)
                {
                    return true;
                }
            }

            return need <= IngredientValueTolerance;
        }

        private static bool AllocateSingleDef(
            List<Portion> portions,
            IngredientCount requirement,
            RecipeDef recipe,
            Bill bill)
        {
            HashSet<ThingDef> seen = new HashSet<ThingDef>();
            for (int i = 0; i < portions.Count; i++)
            {
                Portion portion = portions[i];
                if (portion.Remaining <= 0 || !requirement.filter.Allows(portion.Thing))
                {
                    continue;
                }

                ThingDef def = portion.Thing.def;
                if (!seen.Add(def))
                {
                    continue;
                }

                int required = requirement.CountRequiredOfFor(def, recipe, bill);
                int available = 0;
                for (int j = 0; j < portions.Count; j++)
                {
                    Portion other = portions[j];
                    if (other.Thing.def == def)
                    {
                        available += other.Remaining;
                    }
                }

                if (available < required)
                {
                    continue;
                }

                int left = required;
                for (int j = 0; j < portions.Count; j++)
                {
                    Portion other = portions[j];
                    if (other.Thing.def != def || other.Remaining <= 0)
                    {
                        continue;
                    }

                    int take = Mathf.Min(other.Remaining, left);
                    other.Remaining -= take;
                    left -= take;
                    if (left == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
#endif
