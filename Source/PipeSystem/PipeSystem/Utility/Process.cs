using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PipeSystem
{
    public class Process : IExposable, ILoadReferenceable
    {
        private List<ThingAndResourceOwner> ingredientsOwners;  // Process ingredients
        private ThingWithComps parent;                          // Parent thing
        private ProcessDef def;                                 // Process def
        private int tickLeft;                                   // Ticks left before produce
        private bool pickUpReady;                               // Is it ready to pick-up
        private float progress;                                 // Progress percent

        private bool suspended;                                 // Is process suspended
        private bool spawning;                                  // Should spawn as item?

        private int targetCount;                                // Number of time this process should repeat
        private int processCount;                               // Number of time this process repeated
        private float ruinedPercent;                            // Ruining (due to temp) percent

        private string id;                                      // Process ID

        private List<IntVec3> adjCells;
        private CompResource compResource;
        private List<CompResource> compResources;
        private List<FloatMenuOption> options;
        private AdvancedProcessorsManager advancedProcessorsManager;
        private CompAdvancedResourceProcessor advancedProcessor;

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
            tickLeft = processDef.ticks;
            progress = 0f;
            ruinedPercent = 0f;
            var map = parent.Map;
            id = $"Process_{parent.Map.uniqueID}_{def.defName}_{CachedAdvancedProcessorsManager.GetFor(map).ProcessIDsManager.GetNextPlanetID(map)}";
            spawning = true;
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
                ingredientsOwners.Add(new ThingAndResourceOwner(requirement.thing, requirement.pipeNet, requirement.countNeeded));
            }
            targetCount = 1;
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

                var associatedComp = compResources[i];
                if (owner.PipeNetDef != null && associatedComp != null)
                {
                    owner.AddFromNet(associatedComp.PipeNet);
                }
                // Set awaiting
                advancedProcessorsManager.SetAwaitingIngredients(advancedProcessor);
            }
            // Continue only if we have all ingredients
            if (MissingIngredients)
                return;
            // We are active for ticks
            if (tickLeft > 0 && !RuinedByTemp)
            {
                TryRuin(ticks);
                tickLeft -= ticks;
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

        /// <summary>
        /// Get comps and general setup that happen after each reload
        /// </summary>
        public void PostSpawnSetup()
        {
            // Get the result compResource
            var comps = parent.GetComps<CompResource>().ToList();
            if (def.pipeNet != null)
            {
                for (int i = 0; i < comps.Count; i++)
                {
                    var comp = comps[i];
                    if (comp.Props.pipeNet == def.pipeNet)
                    {
                        compResource = comp;
                        break;
                    }
                }
            }
            // Get map comp
            advancedProcessorsManager = CachedAdvancedProcessorsManager.GetFor(parent.Map);
            // Get processor comp
            advancedProcessor = CachedCompAdvancedProcessor.GetFor(parent);
            // Get others comps
            compResources = new List<CompResource>();
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
                            compResources.Add(comp);
                            break;
                        }
                    }
                }
                else
                {
                    compResources.Add(null);
                }
            }
            // Get adjacent cells for spawning
            adjCells = GenAdj.CellsAdjacent8Way(parent).ToList();
        }

        /// <summary>
        /// Create and spawn thing, or increase existing thing stacksize, or push to net
        /// </summary>
        /// <param name="spawnPos">Spawning position if needed</param>
        /// <param name="outThing">Null if pushed to net</param>
        /// <param name="extractor">Pawn extracint result</param>
        public void SpawnOrPushToNet(IntVec3 spawnPos, out Thing outThing, Pawn extractor = null)
        {
            // Thing created, in case it's not pushed to net
            outThing = null;
            // If it can directly go into the net
            if (def.pipeNet != null && compResource != null && compResource.PipeNet is PipeNet net && net.connectors.Count > 1)
            {
                var count = def.count;
                // Available storage, store it
                if (net.AvailableCapacity > count)
                {
                    net.DistributeAmongStorage(count, out _);
                    ResetProcess();
                    return;
                }
                // No storage but converters
                if (net.ThingConvertersMaxOutput >= count)
                {
                    net.DistributeAmongConverters(count);
                    ResetProcess();
                    return;
                }
                // No storage/converter, try refuel connected things
                if (net.RefillableAmount >= count)
                {
                    net.DistributeAmongRefillables(count);
                    ResetProcess();
                    return;
                }
            }
            // If can't go into net and should/can spawn
            // Bypass ground setting if thing can't be put in net
            if (spawning && def.thing != null && (extractor != null || advancedProcessor.outputOnGround || def.pipeNet == null))
            {
                var map = parent.Map;
                // Try spawning at spawnPos
                if (spawnPos != IntVec3.Invalid && TrySpawnAtCell(spawnPos, map, out outThing))
                {
                    ResetProcess();
                    return;
                }
                // If invalid or couldn't, find an adj cell
                for (int i = 0; i < adjCells.Count; i++)
                {
                    if (TrySpawnAtCell(adjCells[i], map, out outThing))
                    {
                        ResetProcess();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Try spawn result as item on cell. 
        /// </summary>
        /// <param name="cell">Spawn cell</param>
        /// <param name="map">Map</param>
        /// <param name="outThing">Thing spawned or null</param>
        /// <returns>True if spawned, false if not</returns>
        private bool TrySpawnAtCell(IntVec3 cell, Map map, out Thing outThing)
        {
            outThing = null;

            if (cell.Walkable(map))
            {
                // Try find thing of the same def
                var thing = cell.GetFirstThing(map, def.thing);
                if (thing != null)
                {
                    // If adding would go past stack limit
                    if ((thing.stackCount + def.count) > thing.def.stackLimit)
                        return false;
                    // We found some, modifying stack size
                    thing.stackCount += def.count;
                    return true;
                }
                else
                {
                    // We didn't find any, creating thing
                    thing = ThingMaker.MakeThing(def.thing);
                    thing.stackCount = def.count;
                    if (!GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near))
                        return false;

                    outThing = thing;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Reset all thingAndResourceOwner
        /// </summary>
        /// <param name="finished"></param>
        /// <param name="map">Map, used in postdespawn</param>
        public void ResetProcess(bool finished = true, Map map = null)
        {
            if (finished)
            {
                for (int i = 0; i < ingredientsOwners.Count; i++)
                    ingredientsOwners[i].Reset();

                tickLeft = def.ticks;
                pickUpReady = false;
                progress = 0;
                // Prevent going into negative
                if (targetCount - processCount == 0) targetCount++;
                // Create wastepack
                advancedProcessor.ProduceWastepack(Def.wastePackToProduce);

                processCount++;
                advancedProcessor.ProcessStack.Notify_ProcessEnded();
            }
            else
            {
                pickUpReady = false;
                progress = 0;

                if (!def.destroyIngredientsDirectly && !(def.destroyIngredientsOnStart && tickLeft < def.ticks))
                {
                    for (int i = 0; i < ingredientsOwners.Count; i++)
                    {
                        var owner = ingredientsOwners[i];
                        var linkedComp = compResources[i];

                        if (linkedComp != null && linkedComp.PipeNet is PipeNet net && net.AvailableCapacity >= owner.Count)
                        {
                            net.DistributeAmongStorage(owner.Count, out _);
                        }
                        else if (owner.ThingDef is ThingDef def)
                        {
                            var thing = ThingMaker.MakeThing(def);
                            thing.stackCount = owner.Count;
                            GenPlace.TryPlaceThing(thing, adjCells.RandomElement(), map ?? parent.Map, ThingPlaceMode.Near);
                        }
                    }
                }
            }
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
            Widgets.Label(new Rect(28f, 0f, rect.width - 48f - 20f, rect.height + 5f), def.label);
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
            if (Widgets.ButtonImage(deleteRect, TexButton.DeleteX, color, color * GenUI.SubtleMouseoverColor))
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

            var deleteRect = new Rect(rect.x + rect.width - 24f, rect.y, 24f, 24f);
            if (Widgets.ButtonImage(deleteRect, TexButton.DeleteX, Color.white, Color.white * GenUI.SubtleMouseoverColor))
            {
                ResetProcess(false);
                advancedProcessor.ProcessStack.Delete(this);

                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            TooltipHandler.TipRegionByKey(deleteRect, "PipeSystem_CancelCurrentProcess");
        }
    }
}