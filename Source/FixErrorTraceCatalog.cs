namespace HSK.KebabTweaks
{
    /// <summary>
    /// Verbatim error traces shown on Fixes-tab copy buttons (tooltip + clipboard).
    ///
    /// Дословные trace ошибок для кнопок копирования на вкладке «Фиксы» (тултип + буфер).
    /// </summary>
    public static class FixErrorTraceCatalog
    {
        public const int TakeFromMendingTipId = 1;
        public const int CatCrazyTimeTipId = 2;
        public const int CatFloorSleepTipId = 3;
        public const int TradeCaravanLordFixTipId = 4;
        public const int ArmorRacksAssignFixTipId = 5;
        public const int StorageSettingsAllowedToAcceptFixTipId = 14;
        public const int CeRunForCoverDestFixTipId = 6;
        public const int CeProjectileNullSoundFixTipId = 12;
        public const int FishTableTypeListFixTipId = 7;
        public const int GetActiveRitualsFixTipId = 8;
        public const int IdleErrorWanderFixTipId = 9;
        public const int HospitalityGuestApparelOptimizeFixTipId = 13;
        public const int PtgMedicalCareTipId = 10;
        public const int SeedsPleaseSowFixTipId = 11;
        public const int AllowToolHaulUrgentlyNreFixTipId = 15;
        public const int DubsAnalyzerBeginUpdateFixTipId = 16;
        public const int DebugLogSplitterDragFixTipId = 21;
        public const int NeanderthalChiefLeaderFixTipId = 17;
        public const int MapPreviewRngBaselineFixTipId = 18;
        public const int UnifiedXmlPathFixTipId = 19;
        public const int MainMenuBgFitFixTipId = 20;
        public const int RimatomicsGuidancePanelFixTipId = 21;
        public const int SaveSettingsLoadFixTipId = 22;

        public const string TakeFromMending = @"Doio threw exception in WorkGiver DoBillsMakeWeapons: System.NullReferenceException: Object reference not set to an instance of an object
[Ref 79A5B8F5]
 at RimWorld.Bill.get_DeletedOrDereferenced () [0x0000a] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
 at HSK.TakeFromMendingPatch.TakeFromMendingPatchLogic+<EnumerateBillData>d__7.MoveNext () [0x00061] in <47f19bea838743ae86e04efb216d106a>:0 
 at HSK.TakeFromMendingPatch.TakeFromMendingPatchLogic.TryResolveTakeFromParents (System.String route, RimWorld.Bill bill, Verse.Pawn pawn, Verse.Thing billGiver, System.Collections.Generic.List`1[RimWorld.ISlotGroupParent]& parents, System.String& resolution) [0x0023c] in <47f19bea838743ae86e04efb216d106a>:0 
 at HSK.TakeFromMendingPatch.TakeFromMendingPatchLogic.VanillaPrefix (RimWorld.Bill bill, Verse.Pawn pawn, Verse.Thing billGiver, System.Collections.Generic.List`1[T] chosen, System.Collections.Generic.List`1[T] missingIngredients, System.Boolean& __result) [0x00017] in <47f19bea838743ae86e04efb216d106a>:0 
 at HSK.TakeFromMendingPatch.VanillaTryFindBestBillIngredientsPatch.Prefix (RimWorld.Bill bill, Verse.Pawn pawn, Verse.Thing billGiver, System.Collections.Generic.List`1[T] chosen, System.Collections.Generic.List`1[T] missingIngredients, System.Boolean& __result) [0x00000] in <47f19bea838743ae86e04efb216d106a>:0 
 at RimWorld.WorkGiver_DoBill.TryFindBestBillIngredients (RimWorld.Bill bill, Verse.Pawn pawn, Verse.Thing billGiver, System.Collections.Generic.List`1[T] chosen, System.Collections.Generic.List`1[T] missingIngredients) [0x00031] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
     - PREFIX hsk.takefrom.mending.patch: Boolean HSK.TakeFromMendingPatch.VanillaTryFindBestBillIngredientsPatch:Prefix(Bill bill, Pawn pawn, Thing billGiver, List`1 chosen, List`1 missingIngredients, Boolean& __result)
     - PREFIX legodude17.HaulToBuilding: Boolean HaulToBuilding.HaulToBuildingMod:GetIngredients(Bill bill, Pawn pawn, Thing billGiver, List`1 chosen, List`1 missingIngredients, Boolean& __result)
     - POSTFIX hsk.takefrom.mending.patch: Void HSK.TakeFromMendingPatch.VanillaTryFindBestBillIngredientsPatch:Postfix(Bill bill, Pawn pawn, Thing billGiver, List`1 chosen, List`1 missingIngredients, Boolean& __result)
     - POSTFIX net.avilmask.rimworld.mod.CommonSense: Void CommonSense.IngredientPriority+WorkGiver_DoBill_TryStartNewDoBillJob_CommonSensePatch:Postfix(Boolean __result, List`1 chosen)
 at RimWorld.WorkGiver_DoBill.StartOrResumeBillJob (Verse.Pawn pawn, RimWorld.IBillGiver giver, System.Boolean forced) [0x00384] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
     - TRANSPILER Doug.NoJobAuthors: IEnumerable`1 NoJobAuthors.Mod_NoJobAuthors+WorkGiver_DoBill_StartOrResumeBillJob_Patch:StartOrResumeBillJob(IEnumerable`1 instructions)
     - PREFIX TheLoneTec.AssortedTweaks: Boolean AssortedTweaks.StartOrResumeBillJob_Patch:Prefix(Job& __result, WorkGiver_DoBill __instance, Pawn pawn, IBillGiver giver, Boolean forced)
 at RimWorld.WorkGiver_DoBill.JobOnThing (Verse.Pawn pawn, Verse.Thing thing, System.Boolean forced) [0x000a0] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
 at RimWorld.WorkGiver_Scanner.HasJobOnThing (Verse.Pawn pawn, Verse.Thing t, System.Boolean forced) [0x00006] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
     - POSTFIX CodeOptimist.WhileYoureUp: Void WhileYoureUp.Mod+WorkGiver_Scanner__HasJobOnThing_Patch:ClearTempDetour(Pawn pawn)
     - POSTFIX hsk.grower.cut.trees.patch: Void HSK.GrowerCutTreesPatch.GardenerGrowingZoneCutHasJobOnThingPatch:Postfix(Boolean& __result, Pawn pawn, Thing t, WorkGiver_Scanner __instance)
 at RimWorld.JobGiver_Work+<>c__DisplayClass3_1.<TryIssueJobPackage>g__Validator|0 (Verse.Thing t) [0x00013] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
 at Verse.GenClosest+<>c__DisplayClass3_0.<ClosestThingReachable_NewTemp>b__0 (Verse.Thing t) [0x00034] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
 at Verse.GenClosest.<ClosestThing_Global_NewTemp>g__ValidateThing|9_1 (Verse.Thing t, System.Single distSquared, Verse.GenClosest+<>c__DisplayClass9_0& ) [0x00008] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
 at Verse.GenClosest.<ClosestThing_Global_NewTemp>g__Process|9_0 (Verse.Thing t, Verse.GenClosest+<>c__DisplayClass9_0& ) [0x00047] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
 at Verse.GenClosest.ClosestThing_Global_NewTemp (Verse.IntVec3 center, System.Collections.IEnumerable searchSet, System.Single maxDistance, System.Predicate`1[T] validator, System.Func`2[T,TResult] priorityGetter, System.Boolean lookInHaulSources) [0x00066] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
 at Verse.GenClosest.ClosestThing_Global (Verse.IntVec3 center, System.Collections.IEnumerable searchSet, System.Single maxDistance, System.Predicate`1[T] validator, System.Func`2[T,TResult] priorityGetter) [0x0000a] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
     - POSTFIX PureMJ.MjRimMods.WhileYouAreNearby: Void MjRimMods.WhileYouAreNearby.ClosestThing_GlobalPatchForLog:Postfix(Thing __result)
 at Verse.GenClosest.ClosestThingReachable_NewTemp (Verse.IntVec3 root, Verse.Map map, Verse.ThingRequest thingReq, Verse.AI.PathEndMode peMode, Verse.TraverseParms traverseParams, System.Single maxDistance, System.Predicate`1[T] validator, System.Collections.Generic.IEnumerable`1[T] customGlobalSearchSet, System.Int32 searchRegionsMin, System.Int32 searchRegionsMax, System.Boolean forceAllowGlobalSearch, Verse.RegionType traversableRegionTypes, System.Boolean ignoreEntirelyForbiddenRegions, System.Boolean lookInHaulSources) [0x0014e] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
 at Verse.GenClosest.ClosestThingReachable (Verse.IntVec3 root, Verse.Map map, Verse.ThingRequest thingReq, Verse.AI.PathEndMode peMode, Verse.TraverseParms traverseParams, System.Single maxDistance, System.Predicate`1[T] validator, System.Collections.Generic.IEnumerable`1[T] customGlobalSearchSet, System.Int32 searchRegionsMin, System.Int32 searchRegionsMax, System.Boolean forceAllowGlobalSearch, Verse.RegionType traversableRegionTypes, System.Boolean ignoreEntirelyForbiddenRegions) [0x0000a] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
 at ExpandedRoofing.ClosestThingReachableHelper.ClosestThingReachableWrapper (Verse.IntVec3 root, Verse.Map map, Verse.ThingRequest thingReq, Verse.AI.PathEndMode peMode, Verse.TraverseParms traverseParams, System.Single maxDistance, System.Predicate`1[T] validator, System.Collections.Generic.IEnumerable`1[T] customGlobalSearchSet, System.Int32 searchRegionsMin, System.Int32 searchRegionsMax, System.Boolean forceGlobalSearch, Verse.RegionType traversableRegionTypes, System.Boolean ignoreEntirelyForbiddenRegions) [0x00049] in <6fd483508e674e5981ffbdbc452962cb>:0 
 at RimWorld.JobGiver_Work.TryIssueJobPackage (Verse.Pawn pawn, Verse.AI.JobIssueParams jobParams) [0x00671] in <630e2863bc9a4a3493f2eff01e3a9556>:0 
     - TRANSPILER rimworld.whyisthat.expandedroofing.fixbuildorder: IEnumerable`1 ExpandedRoofing.FixFinishFrameBuildOrder:Transpiler(IEnumerable`1 instructions)
     - PREFIX Orion.Hospitality: Boolean Hospitality.Patches.JobGiver_Work_Patch+TryIssueJobPackage:Prefix(Pawn pawn)
     - POSTFIX PureMJ.MjRimMods.WhileYouAreNearby: Void MjRimMods.WhileYouAreNearby.JobGiver_Work_TryIssueJobPackagePatch:After_TryIssueJobPackage(ThinkResult& __result, JobGiver_Work& __instance, Pawn pawn, JobIssueParams jobParams)";

        public const string CatCrazyTime = @"Cat656054 with job CrazyTime (Job_11749015) Giver = JobGiver_CrazyTime [workGiverDef: null] tried to get CurToil with curToilIndex=4 but only has 4 toils.";

        public const string CatFloorSleep = @"Could not reserve (163, 0, 130) (layer: null) for Cat742433 for job LayDown (Job_11436963) A = (163, 0, 130) Giver = JobGiver_GetRestPawnBedOK [workGiverDef: null] (now doing job LayDown (Job_11436963) A = (163, 0, 130) Giver = JobGiver_GetRestPawnBedOK [workGiverDef: null](curToil=-1)) for maxPawns 1 and stackCount -1.
Existing reservers:
   [0] Cat11999 (job: LayDown (Job_11428041) A = (163, 0, 130) Giver = JobGiver_GetRestPawnBedOK [workGiverDef: null], toil: 1, maxPawns: 1, stackCount: -1)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch4 (string)
Verse.AI.ReservationManager:LogCouldNotReserveError (Verse.Pawn,Verse.AI.Job,Verse.LocalTargetInfo,int,int,Verse.ReservationLayerDef)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.ReservationManager.Reserve_Patch1 (Verse.AI.ReservationManager,Verse.Pawn,Verse.AI.Job,Verse.LocalTargetInfo,int,int,Verse.ReservationLayerDef,bool,bool,bool)
Verse.AI.ReservationUtility:Reserve (Verse.Pawn,Verse.LocalTargetInfo,Verse.AI.Job,int,int,Verse.ReservationLayerDef,bool,bool)
RimWorld.JobDriver_LayDown:TryMakePreToilReservations (bool)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.Pawn_JobTracker.StartJob_Patch2 (Verse.AI.Pawn_JobTracker,Verse.AI.Job,Verse.AI.JobCondition,Verse.AI.ThinkNode,bool,bool,Verse.ThinkTreeDef,System.Nullable`1<Verse.AI.JobTag>,bool,bool,System.Nullable`1<bool>,bool,bool,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.Pawn_JobTracker.EndCurrentJob_Patch1 (Verse.AI.Pawn_JobTracker,Verse.AI.JobCondition,bool,bool)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.Pawn_JobTracker.JobTrackerTick_Patch0 (Verse.AI.Pawn_JobTracker)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Pawn.Tick_Patch2 (Verse.Pawn)
Verse.TickList:Tick ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.TickManager.DoSingleTick_Patch2 (Verse.TickManager)
Verse.TickManager:TickManagerUpdate ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Game.UpdatePlay_Patch3 (Verse.Game)
Verse.Root_Play:Update ()
";

        public const string TradeCaravanLordFix = @"Exception while ticking lord with job RimWorld.LordJob_TradeWithColony: 
System.NullReferenceException: Object reference not set to an instance of an object
[Ref 9FCD5A94]
 at RimWorld.LordJob_TradeWithColony+<>c__DisplayClass7_0.<CreateGraph>b__0 (Verse.AI.Group.TriggerSignal s) [0x00009] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at Verse.AI.Group.Trigger_Custom.ActivateOn (Verse.AI.Group.Lord lord, Verse.AI.Group.TriggerSignal signal) [0x00000] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at Verse.AI.Group.Transition.CheckSignal (Verse.AI.Group.Lord lord, Verse.AI.Group.TriggerSignal signal) [0x00013] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at Verse.AI.Group.Lord.CheckTransitionOnSignal (Verse.AI.Group.TriggerSignal signal) [0x00050] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at Verse.AI.Group.Lord.LordTick () [0x000bb] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at Verse.AI.Group.LordManager.LordManagerTick () [0x00014] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - POSTFIX net.skyarkhangel.SkyAI: Void SkyMind.Patch_LordManager_LordManagerTick:Postfix(LordManager __instance)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch4 (string)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.Group.LordManager.LordManagerTick_Patch1 (Verse.AI.Group.LordManager)
Verse.Map:MapPostTick ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.TickManager.DoSingleTick_Patch2 (Verse.TickManager)
Verse.Game/<>c:<LoadGame>b__69_1 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Root.Update_Patch1 (Verse.Root)
Verse.Root_Play:Update ()";

        public const string ArmorRacksAssignFix = @"Exception filling window for RimWorld.Dialog_AssignBuildingOwner: System.NullReferenceException: Object reference not set to an instance of an object
[Ref A4F6C5BD]
 at ArmorRacks.ThingComps.CompAssignableToPawn_ArmorRacks.AssignedAnything (Verse.Pawn pawn) [0x00007] in <3a396fb82b854f1abc0d75395cf20597>:0 
 at RimWorld.Dialog_AssignBuildingOwner.DrawUnassignedRow (Verse.Pawn pawn, System.Single& y, UnityEngine.Rect viewRect, System.Int32 i) [0x001e3] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - TRANSPILER sensiblebedownership.1trickPwnyta: IEnumerable`1 SensibleBedOwnership.Patch_Dialog_AssignBuildingOwner_DrawUnassignedRow:Transpiler(IEnumerable`1 instructions)
     - PREFIX sensiblebedownership.1trickPwnyta: Boolean SensibleBedOwnership.Patch_Dialog_AssignBuildingOwner_DrawUnassignedRow:Prefix(CompAssignableToPawn ___assignable, Pawn pawn)
 at RimWorld.Dialog_AssignBuildingOwner.DoWindowContents (UnityEngine.Rect inRect) [0x001a9] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - TRANSPILER sensiblebedownership.1trickPwnyta: IEnumerable`1 SensibleBedOwnership.Patch_Dialog_AssignBuildingOwner_DoWindowContents:Transpiler(IEnumerable`1 instructions)
     - POSTFIX sensiblebedownership.1trickPwnyta: Void SensibleBedOwnership.Patch_Dialog_AssignBuildingOwner_DoWindowContents:Postfix(CompAssignableToPawn ___assignable, Rect inRect)
 at Verse.Window.InnerWindowOnGUI (System.Int32 x) [0x001d3] in <1d3901981bf845c4a969d40122c14f9e>:0";

        public const string StorageSettingsAllowedToAcceptFix = @"Exception in JobDriver tick for pawn Chantal driver=JobDriver_UnloadYourHauledInventory (toilIndex=9) driver.job=(UnloadYourHauledInventory (Job_13146769) A = Thing_Ammo_StoneBall1501771 B = (192, 0, 92) Giver = ThinkNode_QueuedJob [workGiverDef: null])
System.NullReferenceException: Object reference not set to an instance of an object
[Ref EC9DE02]
 at RimWorld.StorageSettings.AllowedToAccept (Verse.Thing t) [0x00020] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - PREFIX bs.performance: Boolean PerformanceFish.Hauling.StorageSettingsPatches+AllowedToAcceptPatch:Prefix(StorageSettings __instance, Thing t, Boolean& __result, Boolean& __state)
     - POSTFIX bs.performance: Void PerformanceFish.Hauling.StorageSettingsPatches+AllowedToAcceptPatch:Postfix(StorageSettings __instance, Thing t, Boolean __result, Boolean __state)
 at RimWorld.Building_Storage.Accepts (Verse.Thing t) [0x00006] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at WhileYoureUp.Mod.TryFindBestBetterStoreCellFor_MidwayToTarget (Verse.Thing thing, Verse.LocalTargetInfo opportunityTarget, Verse.LocalTargetInfo beforeCarryTarget, Verse.Pawn carrier, Verse.Map map, RimWorld.StoragePriority currentPriority, RimWorld.Faction faction, Verse.IntVec3& foundCell, System.Boolean needAccurateResult, System.Collections.Generic.HashSet`1[T] skipCells) [0x001a0] in <3cfc80d7c4da484480ebcb704e074590>:0 
 at WhileYoureUp.Mod+StoreUtility__TryFindBestBetterStoreCellFor_Patch.DetourAware_TryFindStore (System.Boolean& __result, Verse.Thing t, Verse.Pawn carrier, Verse.Map map, RimWorld.StoragePriority currentPriority, RimWorld.Faction faction, Verse.IntVec3& foundCell, System.Boolean needAccurateResult) [0x00181] in <3cfc80d7c4da484480ebcb704e074590>:0 
 at RimWorld.StoreUtility.TryFindBestBetterStoreCellFor (Verse.Thing t, Verse.Pawn carrier, Verse.Map map, RimWorld.StoragePriority currentPriority, RimWorld.Faction faction, Verse.IntVec3& foundCell, System.Boolean needAccurateResult) [0x00043] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - PREFIX CodeOptimist.WhileYoureUp: Boolean WhileYoureUp.Mod+StoreUtility__TryFindBestBetterStoreCellFor_Patch:DetourAware_TryFindStore(Boolean& __result, Thing t, Pawn carrier, Map map, StoragePriority currentPriority, Faction faction, IntVec3& foundCell, Boolean needAccurateResult)
     - PREFIX likeafox.rimworld.haulexplicitly: Boolean HaulExplicitly.StoreUtility_TryFindBestBetterStoreCellFor_Patch:Prefix(Thing t, Map map, Boolean& __result)
     - POSTFIX kebabebak.hsk.kebab.limits: Void HSKKebabLimits.HaulingLimitPatches+TryFindBestBetterStoreCellForPostfix:Postfix(Boolean& __result, Thing t, Map map, IntVec3& foundCell)
 at Verse.AI.Toils_Haul+<>c__DisplayClass8_0.<PlaceHauledThingInCell>b__0 () [0x001b6] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at Verse.AI.JobDriver.DriverTick () [0x001d2] in <1d3901981bf845c4a969d40122c14f9e>:0";

        public const string CeRunForCoverDestFix = @"Pawn destination reservation manager failed to clean up properly; Enethwen/RunForCover (Job_12613575) A = (98, 0, 76)/RunForCover still reserving (98, 0, 76), prev job: {prevJob}";

        public const string CeProjectileNullSoundFix = @"Tried to PlayOneShot with null SoundDef. Info=(World from (178, 0, 108), Map_0)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch4 (string)
Verse.Sound.SoundStarter:PlayOneShot (Verse.SoundDef,Verse.Sound.SoundInfo)
CombatExtended.ProjectileCE:ImpactSomething ()
CombatExtended.ProjectileCE:Tick ()
CombatExtended.ProjectileCE_Explosive:Tick ()
Verse.TickList:Tick ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.TickManager.DoSingleTick_Patch2 (Verse.TickManager)
Verse.TickManager:TickManagerUpdate ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Game.UpdatePlay_Patch3 (Verse.Game)
Verse.Root_Play:Update ()";

        public const string FishTableTypeListFix = @"Soyuz caught this error. Please don't report this to the RocketMan team unless you're certain RocketMan caused this error. with error System.InvalidOperationException: Failed to find parent index in FishTable<Verse.Pawn, System.Int32> for key: 'System.Object', hashCode: '779', value: '0', count: '4', bucket array length: '16', total tailing entries count: '2', known chain of tails:
{ index: '7' key: 'Lousa', hashCode: '103875', value: '3 }
[Ref EA3716EA]
 at FisheryLib.Collections.FishTable`2+ThrowHelper[TKey,TValue].ThrowFailedToFindParentInvalidOperationException (FisheryLib.Collections.FishTable`2[TKey,TValue] fishTable, System.Int32 childBucketIndex) [0x001ae] in <a3e8b53a325844e98f60ef692520ce62>:0 
 at FisheryLib.Collections.FishTable`2[TKey,TValue].GetParentBucketIndex (System.Int32 childBucketIndex, System.Boolean throwOnFailure) [0x0001f] in <a3e8b53a325844e98f60ef692520ce62>:0 
 at FisheryLib.Collections.FishTable`2[TKey,TValue].InsertAsTail (FisheryLib.Collections.FishTable`2+Entry[TKey,TValue]& entry, System.Int32 bucketIndex) [0x0002c] in <a3e8b53a325844e98f60ef692520ce62>:0 
 at FisheryLib.Collections.FishTable`2[TKey,TValue].InsertEntryInternal (FisheryLib.Collections.FishTable`2+Entry[TKey,TValue]& entry, FisheryLib.Collections.FishTable`2+ReplaceBehaviour[TKey,TValue] replaceBehaviour) [0x000b5] in <a3e8b53a325844e98f60ef692520ce62>:0 
 at FisheryLib.Collections.FishTable`2[TKey,TValue].InsertEntry (FisheryLib.Collections.FishTable`2+Entry[TKey,TValue]& entry, FisheryLib.Collections.FishTable`2+ReplaceBehaviour[TKey,TValue] replaceBehaviour, System.Boolean shifting) [0x00000] in <a3e8b53a325844e98f60ef692520ce62>:0 
 at FisheryLib.Collections.FishTable`2[TKey,TValue].InsertEntry (TKey key, TValue value, FisheryLib.Collections.FishTable`2+ReplaceBehaviour[TKey,TValue] replaceBehaviour, System.Boolean shifting) [0x00008] in <a3e8b53a325844e98f60ef692520ce62>:0 
 at FisheryLib.Collections.FishTable`2[TKey,TValue].TryAdd (TKey key, TValue value) [0x0000b] in <a3e8b53a325844e98f60ef692520ce62>:0 
 at FisheryLib.Collections.IndexedFishSet`1[T].Add (T item) [0x00012] in <a3e8b53a325844e98f60ef692520ce62>:0 
 at FisheryLib.Collections.IndexedFishSet`1[T].Add (System.Object value) [0x00007] in <a3e8b53a325844e98f60ef692520ce62>:0 
 at PerformanceFish.Listers.ThingsPrepatches.AddToTypeList (Verse.ListerThings lister, Verse.Thing thing) [0x0003b] in <66cfa82fbdc943bd9afc22f2027e1965>:0 
 at Verse.ListerThings.Add (Verse.Thing t) [0x0008c] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - POSTFIX ChippedChap.BlueprintTotalsTooltip: Void BlueprintTotalsTooltip.LTChangeNotifiers.LTAddNotifier:Postfix(Thing t)
 at Verse.RegionListersUpdater.RegisterInRegions (Verse.Thing thing, Verse.Map map) [0x0003a] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at Verse.Thing.set_Position (Verse.IntVec3 value) [0x000f0] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - TRANSPILER CombatExtended.HarmonyCE: IEnumerable`1 CombatExtended.HarmonyCE.Harmony_Thing_Position:Transpiler(IEnumerable`1 instructions, ILGenerator generator)
     - PREFIX net.littlewhitemouse.LWM.DeepStorage: Void LWM.DeepStorage.Patch_Thing_set_Position:Prefix(Thing __instance)
     - POSTFIX net.littlewhitemouse.LWM.DeepStorage: Void LWM.DeepStorage.Patch_Thing_set_Position:Postfix(Thing __instance)
 at Verse.AI.Pawn_PathFollower.TryEnterNextPathCell () [0x0011a] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at Verse.AI.Pawn_PathFollower.PatherTick () [0x00404] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - PREFIX Krkr.RocketMan.Soyuz: Void Soyuz.Patches.Pawn_PathFollower_Patch+Pawn_PathFollower_PatherTick:Prefix(Pawn_PathFollower __instance)
     - POSTFIX Krkr.RocketMan.Soyuz: Void Soyuz.Patches.Pawn_PathFollower_Patch+Pawn_PathFollower_PatherTick:Postfix(Pawn_PathFollower __instance)
     - FINALIZER Krkr.RocketMan.Soyuz: Void Soyuz.Patches.Pawn_PathFollower_Patch+Pawn_PathFollower_PatherTick:Finalizer(Exception __exception)
 at Verse.Pawn.Tick () [0x000d8] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - TRANSPILER Krkr.RocketMan.Soyuz: IEnumerable`1 Soyuz.Patches.Pawn_Tick_Patch:Transpiler(IEnumerable`1 instructions, ILGenerator generator)
     - POSTFIX net.skyarkhangel.SkyAI: Void SkyMind.Patch_Pawn_Tick:Postfix(Pawn __instance)
     - FINALIZER Krkr.RocketMan.Soyuz: Void Soyuz.Patches.Pawn_Tick_Patch:Finalizer(Pawn __instance, Exception __exception)
UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch4 (string)
RocketMan.Logger:Debug (string,System.Exception,string)
Soyuz.Patches.Pawn_Tick_Patch:Finalizer (Verse.Pawn,System.Exception)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Pawn.Tick_Patch2 (Verse.Pawn)
Verse.TickList:Tick ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.TickManager.DoSingleTick_Patch2 (Verse.TickManager)
Verse.TickManager:TickManagerUpdate ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Game.UpdatePlay_Patch3 (Verse.Game)
Verse.Root_Play:Update ()";

        public const string GetActiveRitualsFix = @"System.IndexOutOfRangeException: index + length > size
[Ref 654CA89D]
 at System.Array.Clear (System.Array array, System.Int32 index, System.Int32 length) [0x00044] in <eae584ce26bc40229c1b1aa476bfa589>:0 
 at System.Collections.Generic.List`1[T].Clear () [0x00009] in <eae584ce26bc40229c1b1aa476bfa589>:0 
 at RimWorld.IdeoManager.GetActiveRituals (Verse.Map map) [0x00000] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at RimWorld.IdeoManager.GetActiveRitualOn (Verse.TargetInfo target) [0x00021] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at RimWorld.RitualObligationTargetFilter.CanUseTarget (Verse.TargetInfo target, RimWorld.RitualObligation obligation) [0x00028] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at RimWorld.Precept_Ritual.ShouldShowGizmo (Verse.TargetInfo target) [0x0001f] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at Verse.Thing+<GetGizmos>d__181.MoveNext () [0x00111] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at HaulExplicitly.Thing_GetGizmos_Patch+<Postfix>d__0.MoveNext () [0x00089] in <51862bf1165a4d1aafe414a13a2c3509>:0 
 at Verse.ThingWithComps+<GetGizmos>d__35.MoveNext () [0x00072] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at HaulExplicitly.ThingWithComps_GetGizmos_Patch+<Postfix>d__0.MoveNext () [0x00089] in <51862bf1165a4d1aafe414a13a2c3509>:0 
 at Verse.Building+<GetGizmos>d__27.MoveNext () [0x00088] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at SaveStorageSettings.Patch_Building_GetGizmos+<Postfix>d__0.MoveNext () [0x00093] in <2c529ef3a2e943759c4570cc360f1404>:0 
 at RimWorld.Building_Casket+<GetGizmos>d__17.MoveNext () [0x00078] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at RimWorld.Building_Grave+<GetGizmos>d__13.MoveNext () [0x00072] in <1d3901981bf845c4a969d40122c14f9e>:0 
 at <0x1ad392c88f0 + 0x002c9> <unknown method>
 at System.Linq.Enumerable.ToList[TSource] (System.Collections.Generic.IEnumerable`1[T] source) [0x00018] in <351e49e2a5bf4fd6beabb458ce2255f3>:0 
 at PerformanceOptimizer.Optimization_InspectGizmoGrid_DrawInspectGizmoGridFor.GetGizmosFast (Verse.ISelectable selectable) [0x00046] in <8a0aa6730bc448aea50ccf1986a7f53e>:0 
 at RimWorld.InspectGizmoGrid.DrawInspectGizmoGridFor (System.Collections.Generic.IEnumerable`1[T] selectedObjects, Verse.Gizmo& mouseoverGizmo) [0x000f7] in <1d3901981bf845c4a969d40122c14f9e>:0 
     - TRANSPILER UnlimitedHugs.AllowTool: IEnumerable`1 AllowTool.Patches.InspectGizmoGrid_DrawInspectGizmoGridFor_Patch:ClearReverseDesignators(IEnumerable`1 instructions)
     - TRANSPILER PerformanceOptimizer.Main: IEnumerable`1 PerformanceOptimizer.Optimization_InspectGizmoGrid_DrawInspectGizmoGridFor:InspectGizmoGrid_DrawInspectGizmoGridForTranspiler(IEnumerable`1 instructions) currentSelectable: null
UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch4 (string)
Verse.Log:ErrorOnce (string,int)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:RimWorld.InspectGizmoGrid.DrawInspectGizmoGridFor_Patch0 (System.Collections.Generic.IEnumerable`1<object>,Verse.Gizmo&)
RimWorld.MainTabWindow_Inspect:DrawInspectGizmos ()
RimWorld.InspectPaneUtility:ExtraOnGUI (RimWorld.IInspectPane)
RimWorld.MainTabWindow_Inspect:ExtraOnGUI ()
Verse.WindowStack:WindowStackOnGUI ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:RimWorld.UIRoot_Play.UIRootOnGUI_Patch3 (RimWorld.UIRoot_Play)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Root.OnGUI_Patch2 (Verse.Root)";

        public const string IdleErrorWanderFix = @"CГіcabrei issued IdleError wait job. The behavior tree should never get here.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch4 (string)
Verse.Log:ErrorOnce (string,int)
Verse.AI.JobGiver_IdleError:TryGiveJob (Verse.Pawn)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.ThinkNode_JobGiver.TryIssueJobPackage_Patch1 (Verse.AI.ThinkNode_JobGiver,Verse.Pawn,Verse.AI.JobIssueParams)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Glue:AbiFixup<Verse.AI.ThinkResult Verse.AI.ThinkNode_JobGiver:TryIssueJobPackage(Verse.Pawn, Verse.AI.JobIssueParams),Verse.AI.ThinkResult Verse.AI.ThinkNode_JobGiver.TryIssueJobPackage_Patch1(Verse.AI.ThinkNode_JobGiver, Verse.Pawn, Verse.AI.JobIssueParams)> (Verse.AI.ThinkNode_JobGiver,Verse.AI.ThinkResult&,Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Conditional:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Conditional:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Tagger:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Subtree:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
RimWorld.ThinkNode_JoinVoluntarilyJoinableLord:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.Pawn_JobTracker:DetermineNextJob (Verse.ThinkTreeDef&,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.Pawn_JobTracker.JobTrackerTick_Patch0 (Verse.AI.Pawn_JobTracker)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Pawn.Tick_Patch2 (Verse.Pawn)
Verse.TickList:Tick ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.TickManager.DoSingleTick_Patch2 (Verse.TickManager)
Verse.Game/<>c:<LoadGame>b__69_1 ()
Verse.LongEventHandler:ExecuteToExecuteWhenFinished ()
Verse.LongEventHandler:UpdateCurrentAsynchronousEvent ()
Verse.LongEventHandler:LongEventsUpdate (bool&)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Root.Update_Patch1 (Verse.Root)
Verse.Root_Play:Update ()";

        public const string HospitalityGuestApparelOptimizeFix = @"Non-colonist Baras tried to optimize apparel.
UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch4 (string)
Verse.Log:ErrorOnce (string,int)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:RimWorld.JobGiver_OptimizeApparel.TryGiveJob_Patch2 (RimWorld.JobGiver_OptimizeApparel,Verse.Pawn)
Hospitality.JobGiver_OptimizeApparel_Guest:TryGiveJob (Verse.Pawn)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.ThinkNode_JobGiver.TryIssueJobPackage_Patch1 (Verse.AI.ThinkNode_JobGiver,Verse.Pawn,Verse.AI.JobIssueParams)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Glue:AbiFixup<Verse.AI.ThinkResult Verse.AI.ThinkNode_JobGiver:TryIssueJobPackage(Verse.Pawn, Verse.AI.JobIssueParams),Verse.AI.ThinkResult Verse.AI.ThinkNode_JobGiver.TryIssueJobPackage_Patch1(Verse.AI.ThinkNode_JobGiver, Verse.Pawn, Verse.AI.JobIssueParams)> (Verse.AI.ThinkNode_JobGiver,Verse.AI.ThinkResult&,Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Tagger:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Hospitality.ThinkNode_OnlyAllowed:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
RimWorld.ThinkNode_Duty:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Conditional:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Tagger:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Subtree:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
RimWorld.ThinkNode_JoinVoluntarilyJoinableLord:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.Pawn_JobTracker:DetermineNextJob (Verse.ThinkTreeDef&,bool)
Verse.AI.Pawn_JobTracker:TryFindAndStartJob ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.Pawn_JobTracker.JobTrackerTick_Patch0 (Verse.AI.Pawn_JobTracker)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Pawn.Tick_Patch2 (Verse.Pawn)
Verse.TickList:Tick ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.TickManager.DoSingleTick_Patch2 (Verse.TickManager)
Verse.TickManager:TickManagerUpdate ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Game.UpdatePlay_Patch3 (Verse.Game)
Verse.Root_Play:Update ()";

        public const string PtgMedicalCare = @"[PTG] Failed to draw group header cell GroupColumnWorker_MedicalCare, disabling it until game restart:
IndexOutOfRangeException: Index was outside the bounds of the array.

UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch4 (string)
PawnTableGrouped.LogHelper:Log (string,PawnTableGrouped.MessageType)
PawnTableGrouped.LogHelper:LogException (string,System.Exception)
PawnTableGrouped.CPawnTableGroupRow:DoCell (UnityEngine.Rect,int,bool,bool)
PawnTableGrouped.CTableGrid:DoTableContent (int,int)
PawnTableGrouped.CTableGrid:DoContent ()
RWLayout.alpha2.CElement:DoElementContent ()
RWLayout.alpha2.CElement:DoElementContent ()
PawnTableGrouped.PawnTableGroupedView:OnGUI (UnityEngine.Vector2,int)
PawnTableGrouped.PawnTableGroupedImpl:PawnTableOnGUI (UnityEngine.Vector2)
PawnTableGrouped.PawnTablePatches:PawnTableOnGUI_prefix (RimWorld.PawnTable,UnityEngine.Vector2)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:RimWorld.PawnTable.PawnTableOnGUI_Patch3 (RimWorld.PawnTable,UnityEngine.Vector2)
RimWorld.MainTabWindow_PawnTable:DoWindowContents (UnityEngine.Rect)
AnimalTab.MainTabWindow_Animals:DoWindowContents (UnityEngine.Rect)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Window.InnerWindowOnGUI_Patch0 (Verse.Window,int)
UnityEngine.GUI:CallWindowDelegate (UnityEngine.GUI/WindowFunction,int,int,UnityEngine.GUISkin,int,single,single,UnityEngine.GUIStyle)";

        public const string SeedsPleaseSowFix = @"[WhileYouAreNearby 0.7.1] Checho threw exception in WorkGiver GrowerSow: System.NullReferenceException: Object reference not set to an instance of an object
[Ref E0817131] Duplicate stacktrace, see ref for original
UnityEngine.StackTraceUtility:ExtractStackTrace ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Log.Error_Patch4 (string)
MjRimMods.WhileYouAreNearby.Utils:WriteLog (System.Action`1<string>,string,object[])
MjRimMods.WhileYouAreNearby.Utils:Error (string,object[])
MjRimMods.WhileYouAreNearby.JobGiver_Work_TryIssueJobPackagePatch:After_TryIssueJobPackage (Verse.AI.ThinkResult&,RimWorld.JobGiver_Work&,Verse.Pawn,Verse.AI.JobIssueParams)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:RimWorld.JobGiver_Work.TryIssueJobPackage_Patch2 (RimWorld.JobGiver_Work,Verse.Pawn,Verse.AI.JobIssueParams)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Glue:AbiFixup<Verse.AI.ThinkResult RimWorld.JobGiver_Work:TryIssueJobPackage(Verse.Pawn, Verse.AI.JobIssueParams),Verse.AI.ThinkResult RimWorld.JobGiver_Work.TryIssueJobPackage_Patch2(RimWorld.JobGiver_Work, Verse.Pawn, Verse.AI.JobIssueParams)> (RimWorld.JobGiver_Work,Verse.AI.ThinkResult&,Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_PrioritySorter:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Tagger:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Subtree:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Conditional:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.ThinkNode_Priority:TryIssueJobPackage (Verse.Pawn,Verse.AI.JobIssueParams)
Verse.AI.Pawn_JobTracker:DetermineNextJob (Verse.ThinkTreeDef&,bool)
Verse.AI.Pawn_JobTracker:CheckForJobOverride_NewTemp (single,bool)
Verse.AI.Pawn_JobTracker:CheckForJobOverride (single)
RimWorld.JobDriver_Reading:<ReadBook>b__17_1 ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.JobDriver.DriverTick_Patch0 (Verse.AI.JobDriver)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.AI.Pawn_JobTracker.JobTrackerTick_Patch0 (Verse.AI.Pawn_JobTracker)
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Pawn.Tick_Patch2 (Verse.Pawn)
Verse.TickList:Tick ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.TickManager.DoSingleTick_Patch2 (Verse.TickManager)
Verse.TickManager:TickManagerUpdate ()
(wrapper dynamic-method) MonoMod.Utils.DynamicMethodDefinition:Verse.Game.UpdatePlay_Patch3 (Verse.Game)
Verse.Root_Play:Update ()

Лилишна with job SowWithSeeds (Job_3975492) A = (107, 0, 143) B = Thing_Seed_Haygrass3107148 Giver = JobGiver_Work [workGiverDef: GrowerSow] tried to get CurToil with curToilIndex=8 but only has 8 toils.";

        public const string AllowToolHaulUrgentlyNreFix =
@"[HugsLib][ERR] AllowTool caused an exception during OnFixedUpdate: System.NullReferenceException: Object reference not set to an instance of an object
[Ref 96E93BE8]
 at AllowTool.HaulUrgentlyCacheHandler.GetHaulUrgentlyDesignatedThings (Verse.Map map, System.Collections.Generic.ICollection`1[T] targetList) [0x00021] in <ac74685a1bbb43a3b1f5bc3916a10c86>:0 
 at AllowTool.HaulUrgentlyCacheHandler.RecacheIfNeeded (Verse.Map map, System.Single currentTime) [0x00035] in <ac74685a1bbb43a3b1f5bc3916a10c86>:0 
 at AllowTool.HaulUrgentlyCacheHandler.GetDesignatedThingsForMap (Verse.Map map, System.Single currentTime) [0x00001] in <ac74685a1bbb43a3b1f5bc3916a10c86>:0 
 at AllowTool.HaulUrgentlyCacheHandler+NoStorageSpaceTracker.ProcessDesignations (Verse.Map map, System.Int32 currentUpdate, System.Single currentTime) [0x0002b] in <ac74685a1bbb43a3b1f5bc3916a10c86>:0 
 at AllowTool.HaulUrgentlyCacheHandler.ProcessCacheEntries (System.Int32 currentFrame, System.Single currentTime) [0x00029] in <ac74685a1bbb43a3b1f5bc3916a10c86>:0 
 at AllowTool.AllowToolController.FixedUpdate () [0x00012] in <ac74685a1bbb43a3b1f5bc3916a10c86>:0 
 at HugsLib.HugsLibController.OnFixedUpdate () [0x00021] in <9270b4f3a0574e6ab5e5325b2e8ba741>:0";

        public const string DubsAnalyzerBeginUpdateFix =
@"[Analyzer] [CRITICAL] Caught analyzer trying to begin a new update cycle before finishing the previous one.
Analyzer.Profiling.ProfileController:BeginUpdate()
Analyzer.Profiling.H_RootUpdate:Prefix()
Verse.Root_Play:Update()
      - Prefixes: {  } Postfixes: {  }";

        public const string DebugLogSplitterDragFix =
@"Debug Log details pane keeps resizing after Alt+Tab while dragging the stack-trace splitter (borderDragging stuck true; MouseUp lost).
LudeonTK.EditWindow_Log.DoMessageDetails
(borderDragging cleared only on Event.rawType == MouseUp)";

        public const string NeanderthalChiefLeaderFix =
@"Could not generate a pawn after 70 tries. Last error: Generated pawn with disabled requiredWorkTags. Ignoring scenario requirements.
Could not generate a pawn after 100 tries. Last error: Generated pawn with disabled requiredWorkTags. Ignoring validator.
Pawn generation error: Generated pawn with disabled requiredWorkTags. Too many tries (120), returning null. Generation request: kindDef=Tribal_ChiefMelee_Neanderthal, context=NonPlayer, faction=ExampleTribe, tile=-1, forceGenerateNewPawn=False, mustBeCapableOfViolence=False, fixedGender=Male
Error in WorldGenStep: NullReferenceException at Faction.TryGenerateNewLeader / WorldGenStep_Factions
Pawn generation error: Generated pawn incapable of required skill: Melee Too many tries (120), returning null. Generation request: kindDef=Tribal_ChiefMelee_Neanderthal, context=NonPlayer, faction=ExampleTribe, tile=-1,0, forceGenerateNewPawn=False, allowedDevelopmentalStages=Adult, allowDead=False, allowDowned=False, canGeneratePawnRelations=True, mustBeCapableOfViolence=False, colonistRelationChanceFactor=1, forceAddFreeWarmLayerIfNeeded=False, allowGay=True, prohibitedTraits=, allowFood=True, allowAddictions=True, inhabitant=False, certainlyBeenInCryptosleep=False, biocodeWeaponChance=0, validatorPreGear=, validatorPostGear=, fixedBiologicalAge=, fixedChronologicalAge=, fixedGender=Male, fixedLastName=, fixedBirthName=";

        public const string MapPreviewRngBaselineFix =
@"[Map Preview v1.12.25] Map Preview has detected an issue causing previews to be inaccurate: Vanilla map components have modified the RNG state by 7 in their constructors, which does not match the expected amount of 58 iterations.Please report this on the Map Preview workshop page, so this issue can be fixed.";

        public const string UnifiedXmlPathFix =
@"[UXE] Exception thrown during xml export: DirectoryNotFoundException: Could not find a part of the path ""HSK-Launcher-4.6.3\Mods\Unified.xml"".
UnifiedXmlExport.LoadedModManagerPatches:ParseAndProcessXML_postfix
Verse.LoadedModManager.ParseAndProcessXML
Verse.LoadedModManager.LoadAllActiveMods
Verse.PlayDataLoader.DoPlayLoad";

        public const string MainMenuBgFitFix =
@"RimThemesLite UI_BackgroundMain_Patch.Prefix sizes animated main-menu VideoPlayer dest rect with Screen.width/height while GUI.DrawTexture uses UI.screenWidth/Height. With UI Scale > 1 the video background is clipped (often the right edge).";

        public const string RimatomicsGuidancePanelFix =
@"GUI Error: You are pushing more GUIClips than you are popping. Make sure they are balanced.
Mouse position stack is not empty. There were more calls to BeginScrollView than EndScrollView.
(Filename: Rimatomics MainTabWindow_Rimatomics.DrawPanel — Guidance System / ResearchGuidenceSystem with empty Unlocks)";

        public const string SaveSettingsLoadFix =
@"JobDriver threw exception in toil MakeUnfinishedThingIfNeeded's initAction for pawn ExampleColonist driver=JobDriver_DoBill (toilIndex=20) driver.job=(DoBill (Job_1403737) A = Thing_FabricationBench B = Thing_Steel Giver = JobGiver_Work)
System.InvalidCastException: Specified cast is not valid.
  at Verse.AI.Toils_Recipe+<>c__DisplayClass1_0.<MakeUnfinishedThingIfNeeded>b__0 ()
  at Verse.AI.JobDriver.TryActuallyStartNextToil ()
This version of save files is not supported Please create a new one. File: 
Problem loading storage settings file 'example.txt'.
Trying to read from an empty file";

    }
}
