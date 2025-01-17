using System;
using System.Collections.Generic;
using System.Linq;
using ItemProcessor;
using RimWorld;
using Unity.Jobs;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static PipeSystem.ProcessDef;

namespace PipeSystem
{
    public class Process : IExposable, ILoadReferenceable
    {
        private List<ThingAndResourceOwner> ingredientsOwners;  // Process ingredients
        private ThingWithComps parent;                          // Parent thing
        private ProcessDef def;                                 // Process def
        private int tickLeft;                                   // Ticks left before produce
        public bool pickUpReady;                               // Is it ready to pick-up
        private float progress;                                 // Progress percent

        private bool suspended;                                 // Is process suspended
        private bool spawning;                                  // Should spawn as item?

        private int targetCount;                                // Number of time this process should repeat
        private int processCount;                               // Number of time this process repeated
        private float ruinedPercent;                            // Ruining (due to temp) percent


        private string id;                                      // Process ID

        private List<IntVec3> adjCells;
        private List<CompResource> resultsCompResources;
        private List<CompResource> ingredientsCompResources;
        private List<FloatMenuOption> options;
        private AdvancedProcessorsManager advancedProcessorsManager;
        private CompAdvancedResourceProcessor advancedProcessor;
        protected Sustainer workingSoundSustainer;

        public bool IsRunning => ShouldDoNow() && !MissingIngredients;

        /// <summary>
        /// Any missing ingredients? Loop over ingredientsOwners.
        /// </summary>
        public bool MissingIngredients
        {
            get
            {
                var missingStuff = false;
                for (int i = 0; i < ingredientsOwners.Count; i++)
                {
                    if (ingredientsOwners[i].Require)
                    {
                        missingStuff = true;
                        break;
                    }
                }
                return missingStuff;
            }
        }

        /// <summary>
        /// Color slighly red when shouldn't do now
        /// </summary>
        private Color BaseColor
        {
            get
            {
                if (ShouldDoNow())
                {
                    return Color.white;
                }
                return new Color(1f, 0.7f, 0.7f, 0.7f);
            }
        }

        /// <summary>
        /// Repeat text forever or Nx
        /// </summary>
        public string RepeatInfoText
        {
            get
            {
                if (targetCount == -1)
                {
                    return "Forever".Translate();
                }
                return $"{targetCount - processCount}x";
            }
        }

        /// <summary>
        /// Repeat label Forever or Do X times
        /// </summary>
        public string RepeatLabel
        {
            get
            {
                if (targetCount == -1)
                {
                    return BillRepeatModeDefOf.Forever.LabelCap;
                }
                return BillRepeatModeDefOf.RepeatCount.LabelCap;
            }
        }

        /// <summary>
        /// Repeat options, Forever or Do X times
        /// </summary>
        public List<FloatMenuOption> Options
        {
            get
            {
                if (options == null)
                {
                    options = new List<FloatMenuOption>
                    {
                        new FloatMenuOption(BillRepeatModeDefOf.RepeatCount.LabelCap, delegate
                        {
                            processCount = 0;
                            targetCount = 1;
                            advancedProcessor.ProcessStack.Notify_ProcessChange();
                        }),
                        new FloatMenuOption(BillRepeatModeDefOf.Forever.LabelCap, delegate
                        {
                            processCount = 0;
                            targetCount = -1;
                            advancedProcessor.ProcessStack.Notify_ProcessChange();
                        })
                    };
                }
                return options;
            }
        }

        /// <summary>
        /// Ruined
        /// </summary>
        public bool RuinedByTemp => ruinedPercent >= 1f;

        public ProcessDef Def => def;
        public bool PickUpReady => pickUpReady;
        public float Progress => progress;
        public int TickLeft => tickLeft;
        public List<ThingAndResourceOwner> IngredientsOwners => ingredientsOwners;
        public float RuinedPercent => ruinedPercent;

        public Process()
        { }

        public Process(ProcessDef processDef, ThingWithComps parent)
        {
            this.parent = parent;
            def = processDef;
            tickLeft = def.isFactoryProcess ? (int)(GetFactoryAcceleration() * processDef.ticks) : processDef.ticks;
            progress = 0f;
            ruinedPercent = 0f;
            var map = parent.Map;
            id = $"Process_{parent.Map.uniqueID}_{def.defName}_{CachedAdvancedProcessorsManager.GetFor(map).ProcessIDsManager.GetNextProcessID(map)}";
            spawning = true;
        }

        /// <summary>
        /// Accelerates or decelerates factory processes according to precepts
        /// </summary>
        public float GetFactoryAcceleration()
        {
            if (Current.Game.World.factionManager.OfPlayer.ideos.GetPrecept(PSDefOf.VME_AutomationEfficiency_Increased) != null)
            {
                return 0.75f;
            }
            if (Current.Game.World.factionManager.OfPlayer.ideos.GetPrecept(PSDefOf.VME_AutomationEfficiency_Decreased) != null)
            {
                return 1.5f;
            }
            return 1f;

        }

        /// <summary>
        /// Save things
        /// </summary>
        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "process");

            Scribe_Values.Look(ref id, "id");
            Scribe_Values.Look(ref tickLeft, "tickLeft");
            Scribe_Values.Look(ref progress, "progress");
            Scribe_Values.Look(ref spawning, "spawning");
            Scribe_Values.Look(ref suspended, "suspended");
            Scribe_Values.Look(ref targetCount, "targetCount");
            Scribe_Values.Look(ref pickUpReady, "pickUpReady");
            Scribe_Values.Look(ref processCount, "processCount");
            Scribe_Values.Look(ref ruinedPercent, "ruinedPercent", 0f);

            Scribe_References.Look(ref parent, "parent");

            Scribe_Collections.Look(ref ingredientsOwners, "thingOwnerList", LookMode.Deep);
        }

        public string GetUniqueLoadID() => id;

        public override string ToString() => GetUniqueLoadID();

        /// <summary>
        /// Not suspended, doing forever or didn't reached target count
        /// </summary>
        /// <returns>Should be done</returns>
        public bool ShouldDoNow()
        {
            if (suspended)
                return false;

            if (targetCount == -1)
                return true;

            return processCount < targetCount;
        }

        /// <summary>
        /// Setup ingredientsOwners
        /// </summary>
        public void Setup()
        {
            ingredientsOwners = new List<ThingAndResourceOwner>();
            for (int i = 0; i < def.ingredients.Count; i++)
            {
                var requirement = def.ingredients[i];
                if (requirement.nutritionGetter)
                {
                    if (!requirement.thing.IsNutritionGivingIngestible)
                    {
                        ingredientsOwners.Add(new ThingAndResourceOwner(requirement.thing, requirement.pipeNet, 1, requirement.thingCategory));

                    }
                    else
                    {
                        ingredientsOwners.Add(new ThingAndResourceOwner(requirement.thing, requirement.pipeNet, (int)(requirement.countNeeded / requirement.thing.GetStatValueAbstract(StatDefOf.Nutrition)), requirement.thingCategory));

                    }


                }
                else
                {
                    ingredientsOwners.Add(new ThingAndResourceOwner(requirement.thing, requirement.pipeNet, requirement.countNeeded, requirement.thingCategory));

                }


            }
            targetCount = 1;
        }

        /// <summary>
        /// Only used for the glower at the moment
        /// </summary>
        public void Notify_Started()
        {
            Notify_Glower();
            if (def.sustainerWhenWorking && def.sustainerDef != null)
            {
                Notify_StartWorkingSound();
            }
        }

        /// <summary>
        /// Only used for the glower at the moment
        /// </summary>
        public void Notify_Ended()
        {
            Notify_Glower();
            if (def.sustainerWhenWorking && def.sustainerDef != null)
            {
                Notify_StopWorkingSound();
            }

        }

        /// <summary>
        /// Toggle CompGlowerOnProcess on or off
        /// </summary>
        public void Notify_Glower()
        {
            CompGlowerOnProcess compGlower = advancedProcessor.parent.TryGetComp<CompGlowerOnProcess>();
            compGlower?.UpdateLit(advancedProcessor.parent.Map);

        }

        /// <summary>
        /// Toggle sustainer on
        /// </summary>
        public void Notify_StartWorkingSound()
        {
            if (workingSoundSustainer is null)
            {
                SoundInfo info = SoundInfo.InMap(advancedProcessor.parent, MaintenanceType.PerTickRare);
                workingSoundSustainer = def.sustainerDef.TrySpawnSustainer(info);
            }



        }
        /// <summary>
        /// Toggle sustainer off
        /// </summary>
        public void Notify_StopWorkingSound()
        {

            if (workingSoundSustainer != null)
            {
                workingSoundSustainer.End();
                workingSoundSustainer = null;
            }



        }


        /// <summary>
        /// Manage the temperature ruining mechanic
        /// </summary>
        /// <param name="ticks">Number of ticks that passed</param>
        public void TryRuin(int ticks)
        {
            if (def.temperatureRuinable)
            {
                var ambient = parent.AmbientTemperature;
                if (ambient > def.maxSafeTemperature)
                {
                    ruinedPercent += (ambient - def.maxSafeTemperature) * def.progressPerDegreePerTick * ticks;
                }
                else if (ambient < def.minSafeTemperature)
                {
                    ruinedPercent -= (ambient - def.minSafeTemperature) * def.progressPerDegreePerTick * ticks;
                }

                if (ruinedPercent >= 1f)
                {
                    ruinedPercent = 1f;
                    // TODO: Alert
                    ResetProcess(false);
                }
                else if (ruinedPercent < 0f)
                {
                    ruinedPercent = 0f;
                }
            }
        }

        /// <summary>
        /// Manage ingredientsOwners, ticksLeft and process result
        /// </summary>
        /// <param name="ticks">ticks passed</param>
        public void Tick(int ticks)
        {
            // Try filling owners from their comps
            for (int i = 0; i < ingredientsOwners.Count; i++)
            {
                var owner = ingredientsOwners[i];
                if (!owner.Require)
                    continue;

                var associatedComp = ingredientsCompResources[i];
                if (owner.PipeNetDef != null && associatedComp != null)
                {
                    owner.AddFromNet(associatedComp.PipeNet);
                }
                // Set awaiting
                if (!Def.autoGrabFromHoppers)
                {
                    advancedProcessorsManager.SetAwaitingIngredients(advancedProcessor);
                }
                else
                {
                    CheckInputSlots(ingredientsOwners[i]);

                }

            }
            // Continue only if we have all ingredients
            if (MissingIngredients)
                return;
            // We are active for ticks
            if (tickLeft > 0 && !RuinedByTemp)
            {
                TryRuin(ticks);
                tickLeft -= ticks;
                if (def.sustainerWhenWorking && workingSoundSustainer != null)
                {
                    workingSoundSustainer.Maintain();
                }

            }
            // Set progress (for the bar)
            progress = 1f - (tickLeft / (float)def.ticks);
            // Check if processor should produce this tick
            if (tickLeft <= 0)
            {
                if (def.autoExtract)
                {
                    SpawnOrPushToNet(IntVec3.Invalid, out _);
                }
                else
                {
                    pickUpReady = true;
                    advancedProcessorsManager.PickupReady(advancedProcessor);
                }
            }
        }

        public void CheckInputSlots(ThingAndResourceOwner ingredientsOwner)
        {
            foreach (IntVec3 slot in Def.autoInputSlots)
            {
                List<Thing> thingList = (parent.Position + slot.RotatedBy(parent.Rotation)).GetThingList(parent.Map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    Thing thingToCheck = thingList[j];

                    foreach (ProcessDef.Ingredient ingredient in Def.ingredients)
                    {
                        if (ingredient.thingCategory != null)
                        {
                            if (thingToCheck.def.IsWithinCategory(ingredient.thingCategory))
                            {
                                ingredientsOwner.BeingFilled = true;
                                advancedProcessorsManager.AddIngredient(advancedProcessor, thingToCheck);
                            }
                        }
                        else
                        {
                            if (thingToCheck.def == ingredient.thing)
                            {
                                ingredientsOwner.BeingFilled = true;
                                advancedProcessorsManager.AddIngredient(advancedProcessor, thingToCheck);
                            }

                        }

                    }

                }
            }
            if (!ingredientsOwner.Require)
            {
                Notify_Started();
            }
        }


        /// <summary>
        /// Get comps and general setup that happen after each reload
        /// </summary>
        public void PostSpawnSetup()
        {
            // Get the result(s) resultsCompResources(s)
            var comps = parent.GetComps<CompResource>().ToList();
            resultsCompResources = new List<CompResource>();
            for (int i = 0; i < def.results.Count; i++)
            {
                var result = def.results[i];
                if (result.pipeNet != null)
                {
                    for (int j = 0; j < comps.Count; j++)
                    {
                        var comp = comps[j];
                        if (comp.Props.pipeNet == result.pipeNet)
                        {
                            resultsCompResources.Add(comp);
                            break;
                        }
                    }
                }
            }
            // Get map comp
            advancedProcessorsManager = CachedAdvancedProcessorsManager.GetFor(parent.Map);
            // Get processor comp
            advancedProcessor = CachedCompAdvancedProcessor.GetFor(parent);
            // Get others comps
            ingredientsCompResources = new List<CompResource>();
            for (int i = 0; i < def.ingredients.Count; i++)
            {
                var requirement = def.ingredients[i];
                if (requirement.pipeNet != null)
                {
                    for (int j = 0; j < comps.Count; j++)
                    {
                        var comp = comps[j];
                        if (comp.Props.pipeNet == requirement.pipeNet)
                        {
                            ingredientsCompResources.Add(comp);
                            break;
                        }
                    }
                }
                else
                {
                    ingredientsCompResources.Add(null);
                }
            }
            // Get adjacent cells for spawning
            adjCells = GenAdj.CellsAdjacent8Way(parent).ToList();
        }

        /// <summary>
        /// Create and spawn thing, or increase existing thing stacksize, or push to net
        /// </summary>
        /// <param name="spawnPos">Spawning position if needed</param>
        /// <param name="outThings">Empty if pushed to net</param>
        /// <param name="extractor">Pawn extracint result</param>
        public void SpawnOrPushToNet(IntVec3 spawnPos, out List<Thing> outThings, Pawn extractor = null)
        {
            // Thing created, in case it's not pushed to net
            outThings = new List<Thing>();
            for (int i = 0; i < def.results.Count; i++)
            {
                var result = def.results[i];
                var resComp = resultsCompResources.Find(c => c.Props.pipeNet == result.pipeNet);
                // If it can directly go into the net
                if (result.pipeNet != null && resComp != null && resComp.PipeNet is PipeNet net && net.connectors.Count > 1)
                {
                    var count = result.count;
                    // Available storage, store it
                    if (net.AvailableCapacity > count)
                    {
                        net.DistributeAmongStorage(count, out _);
                    }
                    // No storage but converters
                    else if (net.ThingConvertersMaxOutput >= count)
                    {
                        net.DistributeAmongConverters(count);
                    }
                    // No storage/converter, try refuel connected things
                    else if (net.RefillableAmount >= count)
                    {
                        net.DistributeAmongRefillables(count);
                    }
                }
                // If can't go into net and should/can spawn
                // Bypass ground setting if thing can't be put in net
                else if (spawning && result.thing != null && (extractor != null || advancedProcessor.outputOnGround || result.pipeNet == null))
                {
                    var map = parent.Map;
                    // If defined outputCellOffset
                    if (result.outputCellOffset != IntVec3.Invalid && SpawnResultAt(result, parent.Position + (result.outputCellOffset.RotatedBy(parent.Rotation)), map, ref outThings)) { continue; }
                    if (Def.spawnOnInteractionCell)
                    {
                        spawnPos = parent.InteractionCell;
                    }

                    // Try spawning at spawnPos
                    if (spawnPos != IntVec3.Invalid && SpawnResultAt(result, spawnPos, map, ref outThings)) { continue; }



                    // If invalid or couldn't, find an adj cell
                    for (int j = 0; j < adjCells.Count; j++)
                    {
                        if (SpawnResultAt(result, adjCells[j], map, ref outThings)) { break; }
                    }
                }
            }

            ResetProcess();
            Notify_Ended();
        }

        /// <summary>
        /// Spawn result at cell
        /// </summary>
        /// <param name="result">Result to spawn</param>
        /// <param name="cell">Cell to spawn at</param>
        /// <param name="map">Map to spawn in</param>
        /// <param name="outThings">List of results things</param>
        /// <returns>True if spawned, false otherwise</returns>
        private bool SpawnResultAt(ProcessDef.Result result, IntVec3 cell, Map map, ref List<Thing> outThings)
        {
            if (TrySpawnAtCell(result, cell, map, out Thing outThing))
            {
                outThings.Add(outThing);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try spawn result as item on cell. 
        /// </summary>
        /// <param name="result">ProcessDef result</param>
        /// <param name="cell">Spawn cell</param>
        /// <param name="map">Map</param>
        /// <param name="outThing">Thing spawned or null</param>
        /// <returns>True if spawned, false if not</returns>
        private bool TrySpawnAtCell(ProcessDef.Result result, IntVec3 cell, Map map, out Thing outThing)
        {
            outThing = null;

            if (cell.Walkable(map))
            {
                // Try find thing of the same def
                var thing = cell.GetFirstThing(map, result.thing);
                if (thing != null)
                {
                    // If adding would go past stack limit
                    if ((thing.stackCount + result.count) > thing.def.stackLimit)
                        return false;
                    // We found some, modifying stack size
                    thing.stackCount += result.count;

                    outThing = thing;
                    HandleIngredientsAndQuality(outThing);
                    return true;
                }
                else
                {
                    // We didn't find any, creating thing
                    thing = ThingMaker.MakeThing(result.thing);
                    thing.stackCount = result.count;
                    if (!GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near))
                        return false;

                    outThing = thing;
                    HandleIngredientsAndQuality(outThing);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handle ingredients lists and quality
        /// </summary>
        /// <param name="outThing">process output item</param>
        public void HandleIngredientsAndQuality(Thing outThing)
        {
            if (Def.useIngredients)
            {
                if (outThing.TryGetComp<CompIngredients>() != null)
                {
                    CompIngredients compingredients = outThing.TryGetComp<CompIngredients>();
                    if (Def.transfersIngredientList)
                    {

                        foreach (ThingDef ingredientInput in advancedProcessor.cachedIngredients)
                        {

                            if (!compingredients.ingredients.Contains(ingredientInput)) { compingredients.ingredients.Add(ingredientInput); }
                        }
                        advancedProcessor.cachedIngredients.Clear();

                    }
                    else
                    {
                        foreach (ProcessDef.Ingredient ingredient in Def.ingredients)
                        {
                            if (!compingredients.ingredients.Contains(ingredient.thing)) { compingredients.ingredients.Add(ingredient.thing); }
                        }
                    }


                }
            }
            if (Def.stopAtQuality)
            {
                CompQuality compQuality = outThing.TryGetComp<CompQuality>();
                if (compQuality != null)
                {
                    compQuality.SetQuality(Def.quality, null);
                }
            }

        }

        /// <summary>
        /// Reset ingredients owners
        /// </summary>
        /// <param name="finished">Process finished normaly?</param>
        public void ResetOwners(bool finished)
        {
            if (!finished && !def.destroyIngredientsDirectly && !(def.destroyIngredientsOnStart && tickLeft < def.ticks))
            {
                // Refund ingredients
                for (int i = 0; i < ingredientsOwners.Count; i++)
                {
                    var owner = ingredientsOwners[i];
                    var linkedComp = ingredientsCompResources[i];
                    // Refund in net if possible
                    if (linkedComp != null && linkedComp.PipeNet is PipeNet net && net.AvailableCapacity >= owner.Count)
                    {
                        net.DistributeAmongStorage(owner.Count, out _);
                    }
                    // Refund as item
                    else if (owner.ThingDef is ThingDef def)
                    {
                        if (owner.Count > 0)
                        {
                            var thing = ThingMaker.MakeThing(def);
                            thing.stackCount = owner.Count;
                            GenPlace.TryPlaceThing(thing, adjCells.RandomElement(), parent.Map, ThingPlaceMode.Near);
                        }

                    }
                    // Reset owner
                    owner.Reset();
                }
            }
            else
            {
                // No refund
                for (int i = 0; i < ingredientsOwners.Count; i++)
                    ingredientsOwners[i].Reset();
            }
        }

        /// <summary>
        /// Reset all thingAndResourceOwner
        /// </summary>
        /// <param name="finished">Process finished normaly?</param>
        public void ResetProcess(bool finished = true)
        {
            ResetOwners(finished);  // Reset ingredients owners
            tickLeft = def.ticks;   // Reset ticks
            pickUpReady = false;    // Reset pickup status
            ruinedPercent = 0;      // Reset ruining status
            progress = 0;           // Reset progress
            // If finished normaly, increment process count, produce wastepack
            if (finished)
            {
                if (targetCount - processCount == 0) targetCount++;         // Prevent going into negative
                advancedProcessor.ProduceWastepack(Def.wastePackToProduce); // Create wastepack
                processCount++;                                             // Increment process count
            }
            // Notify process ended to the stack
            advancedProcessor.ProcessStack.Notify_ProcessEnded();
        }

        /// <summary>
        /// Get ThingAndResourceOwner matching thingDef
        /// </summary>
        /// <param name="thingDef"></param>
        /// <returns></returns>
        public ThingAndResourceOwner GetOwnerFor(ThingDef thingDef)
        {
            for (int i = 0; i < ingredientsOwners.Count; i++)
            {
                var owner = ingredientsOwners[i];
                if (owner.ThingDef == thingDef)
                    return owner;
            }

            return null;
        }

        /// <summary>
        /// Get ThingAndResourceOwner matching ThingCategoryDef
        /// </summary>
        /// <param name="thingcategoryDef"></param>
        /// <returns></returns>
        public ThingAndResourceOwner GetOwnerForCategory(List<ThingCategoryDef> thingcategoryDefs)
        {
            for (int i = 0; i < ingredientsOwners.Count; i++)
            {
                var owner = ingredientsOwners[i];
                if (thingcategoryDefs.Contains(owner.ThingCategoryDef))
                    return owner;
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Rect DoInterface(float x, float y, float width, int index)
        {
            var rect = new Rect(x, y, width, 53f);
            var color = (GUI.color = BaseColor);
            Text.Font = GameFont.Small;
            // Draw background
            if (index % 2 == 0) Widgets.DrawAltRect(rect);
            Widgets.BeginGroup(rect);
            var stack = advancedProcessor.ProcessStack;
            // Process isn't the first, we can move it up
            var processIndex = stack.IndexOf(this);
            if (processIndex > 0)
            {
                var upRect = new Rect(0f, 0f, 24f, 24f);
                if (Widgets.ButtonImage(upRect, TexButton.ReorderUp, color))
                {
                    stack.Reorder(this, -1);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegionByKey(upRect, "ReorderBillUpTip");
            }
            // Process isn't the last, we can move it down
            if (processIndex < stack.Processes.Count - 1)
            {
                var downRect = new Rect(0f, 24f, 24f, 24f);
                if (Widgets.ButtonImage(downRect, TexButton.ReorderDown, color))
                {
                    stack.Reorder(this, 1);
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegionByKey(downRect, "ReorderBillDownTip");
            }
            GUI.color = color;
            // Process label
            Widgets.Label(new Rect(28f, 0f, rect.width - 48f - 20f, rect.height + 5f), def.LabelCap + " (" + def.ticks.ToStringTicksToDays() + ")");
            // Config
            var baseRect = rect.AtZero();
            GUI.color = new Color(1f, 1f, 1f, 0.65f);
            Widgets.Label(new Rect(28f, 32f, 100f, 30f), RepeatInfoText);
            GUI.color = color;

            var widgetRow = new WidgetRow(baseRect.xMax, baseRect.y + 29f, UIDirection.LeftThenUp);
            /*if (widgetRow.ButtonText("Details".Translate() + "..."))
            {
                // Find.WindowStack.Add(GetBillDialog());
            }*/
            if (widgetRow.ButtonText(RepeatLabel.PadRight(20)))
            {
                Find.WindowStack.Add(new FloatMenu(Options));
            }
            if (widgetRow.ButtonIcon(TexButton.Plus))
            {
                // Forever, click plus put it in count
                if (targetCount == -1)
                {
                    processCount = 0;
                    targetCount = 1;
                }
                else if (targetCount > -1)
                {
                    targetCount += GenUI.CurrentAdjustmentMultiplier();
                }
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                advancedProcessor.ProcessStack.Notify_ProcessChange();
            }
            if (widgetRow.ButtonIcon(TexButton.Minus))
            {
                // Forever, click minus put it in count
                if (targetCount == -1)
                {
                    processCount = 0;
                    targetCount = 1;
                }
                else if (targetCount > -1)
                {
                    targetCount = Mathf.Max(0, targetCount - GenUI.CurrentAdjustmentMultiplier());
                }
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                advancedProcessor.ProcessStack.Notify_ProcessChange();
            }
            // Delete process
            var deleteRect = new Rect(rect.width - 24f, 0f, 24f, 24f);
            if (Widgets.ButtonImage(deleteRect, TexButton.Delete, color, color * GenUI.SubtleMouseoverColor))
            {
                stack.Delete(this);
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            TooltipHandler.TipRegionByKey(deleteRect, "DeleteBillTip");
            // Suspend
            var suspendRect = new Rect(rect.width - 24f - 24f, 0f, 24f, 24f);
            if (Widgets.ButtonImage(suspendRect, TexButton.Suspend, color))
            {
                suspended = !suspended;
                SoundDefOf.Click.PlayOneShotOnCamera();
                advancedProcessor.ProcessStack.Notify_ProcessChange();
            }
            TooltipHandler.TipRegionByKey(suspendRect, "SuspendBillTip");
            Widgets.EndGroup();
            // Draw suspended
            if (suspended)
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                var suspendedRect = new Rect(rect.x + rect.width / 2f - 70f, rect.y + rect.height / 2f - 20f, 140f, 40f);
                GUI.DrawTexture(suspendedRect, TexUI.GrayTextBG);
                Widgets.Label(suspendedRect, "SuspendedCaps".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            return rect;
        }

        /// <summary>
        /// Do simple process progress interface in rect
        /// </summary>
        /// <param name="rect"></param>
        public void DoSimpleProgressInterface(Rect rect)
        {
            Widgets.DrawHighlight(rect);

            Text.Anchor = TextAnchor.MiddleCenter;
            var labelRect = new Rect(rect.x + 6f, rect.y, rect.width - 30f, 24f);
            Widgets.Label(labelRect, $"{def.LabelCap} ({Progress.ToStringPercent()})");
            Text.Anchor = TextAnchor.UpperLeft;
            // TODO: Hover show missing/meet requirements
            var deleteRect = new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f);
            if (Widgets.ButtonImage(deleteRect, TexButton.Delete, Color.white, Color.white * GenUI.SubtleMouseoverColor))
            {
                ResetProcess(false);
                advancedProcessor.ProcessStack.Delete(this);

                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            TooltipHandler.TipRegionByKey(deleteRect, "PipeSystem_CancelCurrentProcess");
        }
    }
}