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
        // - Amount left to distribute, as percentage (so it supports distributing to both pipe net and as an item)
        private float toDistributePercent;
        // - Decides if we're distributing the output (which pauses the production)
        private bool isDistributing = false;

        // Initialized post spawn:
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
            toDistributePercent = 0;
            isDistributing = false;

            InitializeComps();
        }

        /// <summary>
        /// Set up everything that ins't saved
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();

            SetupForChoice();

            // Only initialize when loading a game, no need to re-initialize the gizmo each time the building is installed
            if (respawningAfterLoad && Props.results.Count > 1)
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
        }

        /// <summary>
        /// Set up comps when building is made or game is loaded.
        /// Don't do it from PostSpawnSetup, as it wouldn't be called on minified buildings and comps may end up null.
        /// </summary>
        private void InitializeComps()
        {
            // Get comps
            flickable = parent.GetComp<CompFlickable>();
            compPower = parent.GetComp<CompPowerTrader>();
        }

        /// <summary>
        /// Get the comp that match result, change some variables and change gizmo icon
        /// </summary>
        private void SetupForChoice()
        {
            // Get the needed compResource
            if (ChoosedResult.net != null)
            {
                foreach (var comp in parent.GetComps<CompResource>())
                {
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
        /// Each 100 ticks, try to distribute temporarily stored output
        /// Each process tick and if not distributing, try to finish process if possible. 
        /// </summary>
        public override void CompTick()
        {
            if (!parent.Spawned)
                return;

            // Handle distributing resources if we weren't able to empty them the first time around
            if (isDistributing)
            {
                pipeNetOverlayDrawer?.TogglePulsing(parent, Props.pipeNet.offMat, false);

                // Only distribute once per pipe net tick at most
                if (parent.IsHashIntervalTick(100))
                    SpawnOrPushToNet();

                // Sound
                UpdateSustainer(Working);
                return;
            }

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
                {
                    toDistributePercent += 1f;
                    isDistributing = true;
                    SpawnOrPushToNet();
                }

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
            Scribe_Values.Look(ref toDistributePercent, "toDistributePercent");
            Scribe_Values.Look(ref isDistributing, "isDistributing", false);
            Scribe_Values.Look(ref nextProcessTick, "nextProcessTick");
            Scribe_Values.Look(ref resultIndex, "resultIndex");
            Scribe_Values.Look(ref cantProcess, "cantProcess", false);
            Scribe_Values.Look(ref enoughResource, "enoughResource", false);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
                InitializeComps();
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
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (chooseOuputGizmo != null)
                yield return chooseOuputGizmo;
        }

        /// <summary>
        /// Push resource to processor. Return amount used.
        /// </summary>
        public float PushTo(float amount)
        {
            if (isDistributing)
                return 0;

            var used = 0f;
            var sub = ChoosedResult.countNeeded - storage;
            if (sub > 0f)
            {
                var toStore = sub > amount ? amount : sub;
                storage += toStore;
                used += toStore;
                // We reached count needed, start process (while handling possible rounding errors)
                if (storage >= ChoosedResult.countNeeded - Mathf.Epsilon)
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
                var count = ChoosedResult.netCount * toDistributePercent;
                var startingCount = count;
                // Fill storages first
                net.DistributeAmongStorage(count, out var stored);
                count -= stored;
                // Try filling converters after storages
                count -= net.DistributeAmongConverters(count, false);
                // Try filling refuelables next
                count -= net.DistributeAmongRefillables(count, false);
                // Try filling other resource processors
                count -= net.DistributeAmongProcessors(count, false);

                // Reduce the percentage to distribute by the amount we used up (as a percentage of total)
                toDistributePercent -= (startingCount - count) / ChoosedResult.netCount;

                // If we have anymore to distribute means that nothing can accept anymore resources
                if (toDistributePercent > 0)
                {
                    cantProcess = true;
                }
                // If we've used up all our inner storage
                else
                {
                    cantProcess = false;
                    isDistributing = false;
                    storage = 0;
                    // Just a precaution
                    toDistributePercent = 0;
                }
            }
            // If can't go into net
            else if (canCreateItems)
            {
                var count = Mathf.FloorToInt(ChoosedResult.thingCount * toDistributePercent);

                if (count > 0)
                {
                    var startingCount = count;

                    var map = parent.Map;
                    var thing = ThingMaker.MakeThing(ChoosedResult.thing);
                    thing.stackCount = count;
                    GenPlace.TryPlaceThing(thing, parent.Position, map, ThingPlaceMode.Near, (_, spawned) => count -= spawned);

                    toDistributePercent -= (startingCount - count) / ChoosedResult.thingCount;
                    // Just a precaution
                    if (toDistributePercent < 0)
                       toDistributePercent = 0;
                }

                // Unable to place stuff nearby
                if (count > 0)
                {
                    cantProcess = true;
                }
                // Reset buffer if we've used up all our inner storage
                else
                {
                    storage = 0;
                    cantProcess = false;
                    isDistributing = false;
                }
            }
            // If we can't refill the net yet and create items, mark that we can't process.
            // This will display proper info (tanks full) and process won't be running on loop (despite not using resources).
            else
            {
                cantProcess = true;
            }
        }
    }
}