using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Allow the player to choose from a list of result. Produce the choosen result.
    /// Either spawn thing, or fill another net.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompResourceProcessor : CompResource
    {
        public new CompProperties_ResourceProcessor Props => (CompProperties_ResourceProcessor)props;

        public float Storage { get => storage; }

        private static readonly Texture2D emptyIcon = ContentFinder<Texture2D>.Get("UI/EmptyImage");
        // Other comps we run check on
        private CompResource otherComp;
        private CompFlickable flickable;
        private CompPowerTrader compPower;
        // Stuff saved:
        // - Amount stored
        private float storage;
        // - Next produce tick
        private int nextProcessTick;
        // - Enough resource to process
        private bool enoughResource;
        // - Storages are full?
        private bool cantProcess;
        // - Choosed result
        private int resultIndex;

        // Initialized post spawn:
        private List<IntVec3> adjCells;
        private bool canPushToNet;
        private bool canCreateItems;
        private Command_Action chooseOuputGizmo;
        private PipeNetOverlayDrawer pipeNetOverlayDrawer;

        /// <summary>
        /// Should work? We check flickable comp, power comp, make sure we can process and there is enough resources
        /// </summary>
        public bool Working
        {
            get
            {
                return (flickable == null || flickable.SwitchIsOn) && (compPower == null || compPower.PowerOn) && enoughResource && !cantProcess;
            }
        }

        /// <summary>
        /// The current choosed processing result
        /// </summary>
        public CompProperties_ResourceProcessor.Result ChoosedResult { get => Props.results[resultIndex]; }

        /// <summary>
        /// Set-up vars
        /// </summary>
        public override void PostPostMake()
        {
            base.PostPostMake();
            nextProcessTick = Find.TickManager.TicksGame + ChoosedResult.eachTicks;
            storage = 0;
            resultIndex = 0;
            cantProcess = false;
            enoughResource = false;
        }

        /// <summary>
        /// Set up everything that ins't saved
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            // Get comps
            flickable = parent.GetComp<CompFlickable>();
            compPower = parent.GetComp<CompPowerTrader>();
            // Get adjacent cells for spawning
            adjCells = GenAdj.CellsAdjacent8Way(parent).ToList();

            pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();

            if (Props.results.Count > 1)
            {
                chooseOuputGizmo = new Command_Action()
                {
                    action = delegate
                    {
                        var floatMenuOptions = new List<FloatMenuOption>();
                        for (int rIndex = 0; rIndex < Props.results.Count; rIndex++)
                        {
                            var res = Props.results[rIndex];
                            var label = res.net != null ? res.net.resource.name : res.thing.label;
                            var count = res.net != null ? res.netCount : res.thingCount;
                            floatMenuOptions.Add(new FloatMenuOption(
                                "PipeSystem_Produce".Translate(count, label, res.countNeeded, PipeNet.def.resource.name.ToLower()), () =>
                                {
                                    resultIndex = Props.results.IndexOf(res);
                                    SetupForChoice();
                                    nextProcessTick = Find.TickManager.TicksGame + ChoosedResult.eachTicks;
                                }));
                        }
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "PipeSystem_ChooseResult".Translate(),
                    defaultDesc = "PipeSystem_ChooseResultDesc".Translate(),
                    icon = ChoosedResult.thing != null ? ChoosedResult.thing.uiIcon : emptyIcon,
                };
            }

            SetupForChoice();
        }

        /// <summary>
        /// Toggle off overlay
        /// </summary>
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            pipeNetOverlayDrawer?.TogglePulsing(parent, Props.pipeNet.offMat, false);
        }

        /// <summary>
        /// Get the comp that match result, change some variables and change gizmo icon
        /// </summary>
        private void SetupForChoice()
        {
            // Get the needed compResource
            if (ChoosedResult.net != null)
            {
                var comps = parent.GetComps<CompResource>();
                for (int i = 0; i < comps.Count(); i++)
                {
                    var comp = comps.ElementAt(i);
                    if (comp.Props.pipeNet == ChoosedResult.net)
                    {
                        otherComp = comp;
                        break;
                    }
                }
            }
            // Initiate variable then used in SpawnOrPushToNet
            canPushToNet = ChoosedResult.net != null && otherComp != null;
            canCreateItems = ChoosedResult.thing != null;
            // Change gizmo icon
            if (chooseOuputGizmo != null)
            {
                chooseOuputGizmo.icon = ChoosedResult.thing != null ? ChoosedResult.thing.uiIcon : emptyIcon;
            }
        }

        /// <summary>
        /// Each tick, update/maintain sound if needed.
        /// Each process tick, try to finish process if possible. 
        /// </summary>
        public override void CompTick()
        {
            // Check if we have enough resources
            if (storage >= ChoosedResult.countNeeded)
                enoughResource = true;
            else
                enoughResource = false;
            pipeNetOverlayDrawer?.TogglePulsing(parent, Props.pipeNet.offMat, !enoughResource);
            // Check if processor should produce this tick
            int tick = Find.TickManager.TicksGame;
            if (tick >= nextProcessTick)
            {
                if (enoughResource && (flickable == null || flickable.SwitchIsOn) && (compPower == null || compPower.PowerOn))
                    SpawnOrPushToNet();

                nextProcessTick = tick + ChoosedResult.eachTicks;
            }
            // Sound
            UpdateSustainer(Working);
        }

        /// <summary>
        /// Save storage, nextProcessTick, cantProcess, enoughResource and resultIndex
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref storage, "storage");
            Scribe_Values.Look(ref nextProcessTick, "nextProcessTick");
            Scribe_Values.Look(ref resultIndex, "resultIndex");
            Scribe_Values.Look(ref cantProcess, "cantProcess", false);
            Scribe_Values.Look(ref enoughResource, "enoughResource", false);
        }

        /// <summary>
        /// Print more info regarding current process
        /// </summary>
        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendInNewLine(base.CompInspectStringExtra());
            // Show storage percentage
            if (Props.showBufferInfo)
            {
                float percent = storage / ChoosedResult.countNeeded;
                percent = percent > 1f ? 1f : percent;
                sb.AppendInNewLine("PipeSystem_ProcessorBuffer".Translate(percent.ToStringPercent()));
            }
            // If can't process anymore, show given key
            if (cantProcess && Props.notWorkingKey != null)
                sb.AppendInNewLine(Props.notWorkingKey.Translate());
            // If working show the thing that will be produced, the amount, and the progress
            if (ChoosedResult.thing != null && Working && storage >= ChoosedResult.countNeeded)
            {
                sb.AppendInNewLine("PipeSystem_Producing".Translate(
                    ChoosedResult.thingCount,
                    ChoosedResult.thing.LabelCap,
                    (1f - ((nextProcessTick - Find.TickManager.TicksGame) / (float)ChoosedResult.eachTicks)).ToStringPercent()));
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Add the choose output gizmo
        /// </summary>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var gizmos = base.CompGetGizmosExtra();
            for (int i = 0; i < gizmos.Count(); i++)
                yield return gizmos.ElementAt(i);

            if (chooseOuputGizmo != null)
                yield return chooseOuputGizmo;
        }

        /// <summary>
        /// Push resource to processor. Return amount used.
        /// </summary>
        public float PushTo(float amount)
        {
            var used = 0f;
            var sub = ChoosedResult.countNeeded - storage;
            if (sub > 0f)
            {
                var toStore = sub > amount ? amount : sub;
                storage += toStore;
                used += toStore;
                // We reached count needed, start process
                if (storage == ChoosedResult.countNeeded)
                    nextProcessTick = Find.TickManager.TicksGame + ChoosedResult.eachTicks;
            }

            return used;
        }

        /// <summary>
        /// Create and spawn thing, or increase existing thing stacksize, or push to net
        /// </summary>
        private void SpawnOrPushToNet()
        {
            // If it can directly go into the net
            if (canPushToNet && otherComp.PipeNet is PipeNet net && net.connectors.Count > 1)
            {
                var count = ChoosedResult.netCount;
                // Available storage?
                if (net.AvailableCapacity > count)
                {
                    // Store it
                    net.DistributeAmongStorage(count, out _);
                    storage = 0;
                }
                // No storage but converters?
                else if (net.thingConverters.Count > 0)
                {
                    var outCap = 0;
                    // Convert it, if some left keep it inside here
                    for (int i = 0; i < net.thingConverters.Count; i++)
                    {
                        var converter = net.thingConverters[i];
                        if (converter.CanOutputNow)
                        {
                            outCap += converter.MaxCanOutput;
                        }
                    }
                    // If no converters are ready
                    if (outCap >= count)
                    {
                        net.DistributeAmongConverters(count);
                        storage = 0;
                    }
                }
                // No storage/converter, try refuel connected things
                else if (net.refillables.Count > 0)
                {
                    net.DistributeAmongRefillables(count);
                    storage = 0;
                }
                // We shouldn't have anymore resource, if we do -> storage full or converter full
                cantProcess = storage > 0;
            }
            // If can't go into net
            else if (canCreateItems)
            {
                var map = parent.Map;
                for (int i = 0; i < adjCells.Count; i++)
                {
                    // Find an output cell
                    var cell = adjCells[i];
                    if (cell.Walkable(map))
                    {
                        // Try find thing of the same def
                        var thing = cell.GetFirstThing(map, ChoosedResult.thing);
                        if (thing != null)
                        {
                            if ((thing.stackCount + ChoosedResult.thingCount) > thing.def.stackLimit)
                                continue;
                            // We found some, modifying stack size
                            thing.stackCount += ChoosedResult.thingCount;
                        }
                        else
                        {
                            // We didn't find any, creating thing
                            thing = ThingMaker.MakeThing(ChoosedResult.thing);
                            thing.stackCount = ChoosedResult.thingCount;
                            if (!GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near))
                                continue;
                        }
                        break;
                    }
                }
                // Reset buffer
                storage = 0;
                cantProcess = false;
            }
        }
    }
}