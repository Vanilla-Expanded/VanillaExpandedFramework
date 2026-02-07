using System;
using System.Collections.Generic;
using System.Linq;
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
        public int tickLeft;                                   // Ticks left before produce
        public bool pickUpReady;                               // Is it ready to pick-up
        private float progress;                                 // Progress percent

        private bool suspended;                                 // Is process suspended
        private bool spawning;                                  // Should spawn as item?

        public int targetCount;                                // Number of time this process should repeat
        private int processCount;                               // Number of time this process repeated
        private float ruinedPercent;                            // Ruining (due to temp) percent
        public int ticksOrQualityTicks;

        public bool forceQualityOut = false;
        public QualityCategory qualityToForce;
        public QualityCategory qualityToOutput;
        public QualityCategory currentQuality;
        public BillRepeatModeDef repeatMode = BillRepeatModeDefOf.RepeatCount;
        public bool outputFactoryHopperIncorrect = false;

        private string id;                                      // Process ID


        private List<FloatMenuOption> qualitySelections;

        private List<IntVec3> adjCells;
        private List<CompResource> resultsCompResources;
        private List<CompResource> ingredientsCompResources;
        private List<FloatMenuOption> options;
        private AdvancedProcessorsManager advancedProcessorsManager;
        public CompAdvancedResourceProcessor advancedProcessor;
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
                if (repeatMode == BillRepeatModeDefOf.Forever)
                {
                    return "Forever".Translate();
                }
                if (repeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    return ProcessUtility.CountResults(this) + "/" + targetCount;
                }
                return $"{targetCount - processCount}x";
            }
        }

        /// <summary>
        /// Repeat label Forever, Do X times or Do Until X
        /// </summary>
        public string RepeatLabel
        {
            get
            {
                if (repeatMode == BillRepeatModeDefOf.Forever)
                {
                    return BillRepeatModeDefOf.Forever.LabelCap;
                }
                if (repeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    return BillRepeatModeDefOf.TargetCount.LabelCap;
                }
                return BillRepeatModeDefOf.RepeatCount.LabelCap;
            }
        }

        /// <summary>
        /// Repeat options, Forever, Do X times or Do Until X
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
                            repeatMode = BillRepeatModeDefOf.RepeatCount;
                            advancedProcessor.ProcessStack.Notify_ProcessChange();
                        }),
                        new FloatMenuOption(BillRepeatModeDefOf.TargetCount.LabelCap, delegate
                        {
                            processCount = ProcessUtility.CountResults(this);
                            targetCount = 1;
                            repeatMode = BillRepeatModeDefOf.TargetCount;
                            advancedProcessor.ProcessStack.Notify_ProcessChange();
                        }),
                        new FloatMenuOption(BillRepeatModeDefOf.Forever.LabelCap, delegate
                        {
                            processCount = 0;
                            targetCount = -1;
                            repeatMode = BillRepeatModeDefOf.Forever;
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
        public float Progress
        {
            get
            {

                return progress;
            }
            set
            {
                progress = value;
            }
        }


        public int TickLeft => tickLeft;
        public List<ThingAndResourceOwner> IngredientsOwners => ingredientsOwners;
        public float RuinedPercent => ruinedPercent;

        public Process()
        { }

        public Process(ProcessDef processDef, ThingWithComps parent, QualityCategory forcedQuality = QualityCategory.Normal)
        {
            this.parent = parent;
            def = processDef;
            ticksOrQualityTicks = (def.ticksQuality.NullOrEmpty() ? def.ticks : def.ticksQuality[(int)forcedQuality]);
            tickLeft = def.isFactoryProcess ? (int)(GetFactoryAcceleration() * ticksOrQualityTicks) : ticksOrQualityTicks;
            progress = 0f;
            ruinedPercent = 0f;

            var map = parent.Map;
            id = $"Process_{parent.Map.uniqueID}_{def.defName}_{CachedAdvancedProcessorsManager.GetFor(map).ProcessIDsManager.GetNextProcessID(map)}";
            spawning = true;
            qualityToOutput = def.ticksQuality.NullOrEmpty() ? def.quality : QualityCategory.Normal;
        }

        /// <summary>
        /// Not implemented at the moment
        /// </summary>
        public float FactorIfAcceleratingProcess()
        {
            if (Def.isTemperatureAcceleratingProcess && advancedProcessor.parent.Map != null)
            {
                float currentTempInMap = advancedProcessor.parent.Position.GetTemperature(advancedProcessor.parent.Map);

                if ((currentTempInMap > Def.minAccelerationTemp) && (currentTempInMap < Def.maxAccelerationTemp))
                {
                    return Def.accelerationFactor;
                }
            }
            return 1f;
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
            Scribe_Values.Look(ref ticksOrQualityTicks, "ticksOrQualityTicks");
            Scribe_Values.Look(ref qualityToOutput, "qualityToOutput");
            Scribe_Values.Look(ref forceQualityOut, "forceQualityOut");
            Scribe_Values.Look(ref qualityToForce, "qualityToForce");
            Scribe_Defs.Look(ref repeatMode, "repeatMode");
            Scribe_Values.Look(ref outputFactoryHopperIncorrect, "outputFactoryHopperIncorrect");
            

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

            if (repeatMode == BillRepeatModeDefOf.Forever)
            {
                return true;
            }
            if (repeatMode == BillRepeatModeDefOf.TargetCount)
            {

                return ProcessUtility.CountResults(this) < targetCount;
            }

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
                    if (requirement.thing?.IsNutritionGivingIngestible == false)
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
                    ingredientsOwners.Add(new ThingAndResourceOwner(requirement.thing, requirement.pipeNet, (int)requirement.countNeeded, requirement.thingCategory));

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
                    Messages.Message(Def.noProperTempDestroyed.Translate(def.minSafeTemperature, def.maxSafeTemperature), parent, MessageTypeDefOf.NegativeEvent, true);
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
                if (!Def.autoGrabFromHoppers || Def.autoInputSlots.NullOrEmpty())
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
                    if (!workingSoundSustainer.Ended)
                    {
                        workingSoundSustainer.Maintain();
                    }

                }

            }
            // Set progress (for the bar)

            progress = Math.Min(1f - (tickLeft / (float)ticksOrQualityTicks), 1);

            // Set current quality

            if (!def.ticksQuality.NullOrEmpty() && !forceQualityOut)
            {
                int ticksDone = def.ticksQuality[(int)qualityToOutput] - tickLeft;
                if (ticksDone > def.ticksQuality[(int)QualityCategory.Masterwork])
                {

                    currentQuality = QualityCategory.Masterwork;
                }
                else if (ticksDone > def.ticksQuality[(int)QualityCategory.Excellent])
                {

                    currentQuality = QualityCategory.Excellent;
                }
                else if (ticksDone > def.ticksQuality[(int)QualityCategory.Good])
                {

                    currentQuality = QualityCategory.Good;
                }
                else if (ticksDone > def.ticksQuality[(int)QualityCategory.Normal])
                {

                    currentQuality = QualityCategory.Normal;
                }
                else if (ticksDone > def.ticksQuality[(int)QualityCategory.Poor])
                {

                    currentQuality = QualityCategory.Poor;
                }
                else if (ticksDone > def.ticksQuality[(int)QualityCategory.Awful])
                {

                    currentQuality = QualityCategory.Awful;
                }

            }

            // Check if processor should produce this tick
            if (tickLeft <= 0)
            {
                if (def.autoExtract)
                {
                    if (def.onlyGrabAndOutputToFactoryHoppers)
                    {
                        if (FactoryHopperDetected())
                        {
                            outputFactoryHopperIncorrect = false;
                            SpawnOrPushToNet(this.parent.InteractionCell, out _);
                        }
                        else
                        {
                            outputFactoryHopperIncorrect = true;
                            tickLeft = 1;
                        }
                    }
                    else { SpawnOrPushToNet(IntVec3.Invalid, out _); }
                }
                else
                {
                    pickUpReady = true;
                    advancedProcessorsManager.PickupReady(advancedProcessor);
                }
            }
        }

        /// <summary>
        /// Returns true if a factory hopper is detected on the interaction cell
        /// </summary>
        public bool FactoryHopperDetected()
        {
            List<Thing> hoppers = this.parent.InteractionCell.GetThingList(this.parent.Map);
            foreach (Thing hopper in hoppers)
            {
                FactoryHopperExtension extension = hopper.def.GetModExtension<FactoryHopperExtension>();
                if (extension != null && extension.isfactoryHopper)
                {
                    Building_Storage storage = hopper as Building_Storage;
                    if (storage?.GetStoreSettings().AllowedToAccept(def.results[0].thing) == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if a factory hoppers is detected on the input cell
        /// </summary>
        public bool InputFactoryHopperDetected(IntVec3 inputTile)
        {
            List<Thing> hoppers = inputTile.GetThingList(this.parent.Map);
            foreach (Thing hopper in hoppers)
            {
                FactoryHopperExtension extension = hopper.def.GetModExtension<FactoryHopperExtension>();
                if (extension != null && extension.isfactoryHopper)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks the defined input slots for ingredients to acquire
        /// </summary>
        /// <param name="ingredientsOwner">ingredientsOwner</param>
        public void CheckInputSlots(ThingAndResourceOwner ingredientsOwner)
        {
            foreach (IntVec3 slot in Def.autoInputSlots)
            {
                IntVec3 pos = parent.Position + slot.RotatedBy(parent.Rotation);
                if (!slot.InBounds(parent.Map))
                    continue;

                if(def.onlyGrabAndOutputToFactoryHoppers && !InputFactoryHopperDetected(slot))
                {
                    continue;
                }

                if (Def.ingredients.NullOrEmpty())
                {
                    Notify_Started();
                }

                List<Thing> thingList = pos.GetThingList(parent.Map);
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
                        net.DistributeAmongConverters(count, false);
                    }
                    // No storage/converter, try refuel connected things
                    else if (net.RefillableAmount >= count)
                    {
                        net.DistributeAmongRefillables(count, false);
                    }
                }
                // If can't go into net and should/can spawn
                // Bypass ground setting if thing can't be put in net
                else if (spawning && result.GetOutput(this) is ThingDef output && (extractor != null || advancedProcessor.outputOnGround || result.pipeNet == null))
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
            this.parent.Map.resourceCounter.UpdateResourceCounts();
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
                var output = result.GetOutput(this);
                // Try find thing of the same def
                var thing = cell.GetFirstThing(map, output);
                if (thing != null)
                {
                    // If adding would go past stack limit

                    if (!def.onlyGrabAndOutputToFactoryHoppers)
                    {
                        if ((thing.stackCount + result.count) > thing.def.stackLimit)
                            return false;
                    }

                    // We found some, modifying stack size
                    thing.stackCount += result.count;

                    outThing = thing;
                    HandleIngredientsAndQuality(outThing);
                    return true;
                }
                else
                {
                    // We didn't find any, creating thing
                    thing = ThingMaker.MakeThing(output);
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

                    foreach (ThingDef ingredientInput in advancedProcessor.cachedIngredients)
                    {

                        if (!compingredients.ingredients.Contains(ingredientInput)) { compingredients.ingredients.Add(ingredientInput); }
                    }
                    advancedProcessor.cachedIngredients.Clear();




                }
            }
            if (Def.stopAtQuality)
            {
                CompQuality compQuality = (outThing as ThingWithComps)?.compQuality;
                if (compQuality != null)
                {
                    if (forceQualityOut)
                    {

                        compQuality.SetQuality(qualityToForce, null);
                    }
                    else
                    {

                        compQuality.SetQuality(qualityToOutput, null);
                    }
                }

                forceQualityOut = false;
            }

        }

        /// <summary>
        /// Reset ingredients owners
        /// </summary>
        /// <param name="finished">Process finished normaly?</param>
        public void ResetOwners(bool finished)
        {
            if (!finished && !def.destroyIngredientsDirectly && !(def.destroyIngredientsOnStart && tickLeft < ticksOrQualityTicks))
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
            tickLeft = ticksOrQualityTicks;   // Reset ticks
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

            string qualityString = def.ticksQuality.NullOrEmpty() ? " " : " (" + qualityToOutput.GetLabel().CapitalizeFirst() + ") ";
            Widgets.Label(new Rect(28f, 0f, rect.width - 48f - 40f, rect.height + 5f), def.LabelCap + qualityString + "(" + ticksOrQualityTicks.ToStringTicksToDays() + ")");
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
            // QualitySelector
            if (!def.ticksQuality.NullOrEmpty())
            {
                var qualitySelectorRect = new Rect(rect.width - 24f - 24f - 24f, 0f, 24f, 24f);
                if (Widgets.ButtonImage(qualitySelectorRect, MaterialCreator.QualitySelect, color))
                {
                    Find.WindowStack.Add(new FloatMenu(QualitySelections));
                }
                TooltipHandler.TipRegionByKey(qualitySelectorRect, "PipeSystem_QualitySelector");
            }

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
        /// QualitySelections
        /// </summary>
        public List<FloatMenuOption> QualitySelections
        {
            get
            {
                if (qualitySelections == null)
                {
                    qualitySelections = new List<FloatMenuOption>
                    {
                        new FloatMenuOption(QualityCategory.Awful.GetLabel().CapitalizeFirst(), () => {
                            int ticksDone = def.ticksQuality[(int)qualityToOutput] - tickLeft;
                            tickLeft = def.ticksQuality[(int)QualityCategory.Awful]-ticksDone;
                            qualityToOutput =QualityCategory.Awful;
                            ticksOrQualityTicks=def.ticksQuality[(int)QualityCategory.Awful];
                        }, extraPartWidth: 24f),
                        new FloatMenuOption(QualityCategory.Poor.GetLabel().CapitalizeFirst(), () => {
                            int ticksDone = def.ticksQuality[(int)qualityToOutput] - tickLeft;
                            tickLeft = def.ticksQuality[(int)QualityCategory.Poor]-ticksDone;
                            qualityToOutput =QualityCategory.Poor;
                            ticksOrQualityTicks=def.ticksQuality[(int)QualityCategory.Poor];
                        }, extraPartWidth: 24f),
                        new FloatMenuOption(QualityCategory.Normal.GetLabel().CapitalizeFirst(), () => {
                           int ticksDone = def.ticksQuality[(int)qualityToOutput] - tickLeft;
                            tickLeft = def.ticksQuality[(int)QualityCategory.Normal]-ticksDone;
                            qualityToOutput =QualityCategory.Normal;
                            ticksOrQualityTicks=def.ticksQuality[(int)QualityCategory.Normal];
                        }, extraPartWidth: 24f),
                        new FloatMenuOption(QualityCategory.Good.GetLabel().CapitalizeFirst(), () => {
                            int ticksDone = def.ticksQuality[(int)qualityToOutput] - tickLeft;
                            tickLeft = def.ticksQuality[(int)QualityCategory.Good]-ticksDone;
                            qualityToOutput =QualityCategory.Good;
                            ticksOrQualityTicks=def.ticksQuality[(int)QualityCategory.Good];
                        }, extraPartWidth: 24f),
                        new FloatMenuOption(QualityCategory.Excellent.GetLabel().CapitalizeFirst(), () => {
                            int ticksDone = def.ticksQuality[(int)qualityToOutput] - tickLeft;
                            tickLeft = def.ticksQuality[(int)QualityCategory.Excellent]-ticksDone;
                            qualityToOutput =QualityCategory.Excellent;
                            ticksOrQualityTicks=def.ticksQuality[(int)QualityCategory.Excellent];
                        }, extraPartWidth: 24f),
                        new FloatMenuOption(QualityCategory.Masterwork.GetLabel().CapitalizeFirst(), () => {
                            int ticksDone = def.ticksQuality[(int)qualityToOutput] - tickLeft;
                            tickLeft = def.ticksQuality[(int)QualityCategory.Masterwork]-ticksDone;
                            qualityToOutput =QualityCategory.Masterwork;
                            ticksOrQualityTicks=def.ticksQuality[(int)QualityCategory.Masterwork];
                        }, extraPartWidth: 24f),
                        new FloatMenuOption(QualityCategory.Legendary.GetLabel().CapitalizeFirst(), () => {
                            int ticksDone = def.ticksQuality[(int)qualityToOutput] - tickLeft;
                            tickLeft = def.ticksQuality[(int)QualityCategory.Legendary]-ticksDone;
                            qualityToOutput =QualityCategory.Legendary;
                            ticksOrQualityTicks=def.ticksQuality[(int)QualityCategory.Legendary];
                        }, extraPartWidth: 24f)
                    };
                }
                return qualitySelections;
            }
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

        public ThingDef GetLastStoredIngredient()
        {
            var ingredientsOwners = IngredientsOwners;
            if (ingredientsOwners != null)
            {
                for (int i = 0; i < ingredientsOwners.Count; i++)
                {
                    var owner = ingredientsOwners[i];
                    if (owner.lastThingStored != null && owner.Count > 0)
                    {
                        return owner.lastThingStored;
                    }
                }
            }
            return null;
        }
    }
}