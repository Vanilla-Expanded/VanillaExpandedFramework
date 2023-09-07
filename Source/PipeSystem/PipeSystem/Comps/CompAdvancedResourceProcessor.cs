using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PipeSystem
{
    [StaticConstructorOnStartup]
    public class CompAdvancedResourceProcessor : ThingComp
    {
        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

        private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

        // Other comps we run check on
        private CompFlickable flickable;
        private CompPowerTrader compPower;
        private CompRefuelable compRefuelable;

        private Vector3 itemDrawPos;                            // Drawing item position
        private Material barFilledCachedMat;                    // Cached progress bar material
        private GenDraw.FillableBarRequest fillableBarRequest;  // FillableBarRequest cache

        private List<FloatMenuOption> processesOptions;         // List of processes
        private ProcessStack processStack = new ProcessStack(); // Process stack

        public CompProperties_AdvancedResourceProcessor Props => (CompProperties_AdvancedResourceProcessor)props;

        /// <summary>
        /// Should work? We check flickable comp, power comp
        /// </summary>
        public bool AllCompsOn
        {
            get
            {
                return (flickable == null || flickable.SwitchIsOn)
                       && (compPower == null || compPower.PowerOn)
                       && (compRefuelable == null || compRefuelable.HasFuel);
            }
        }

        /// <summary>
        /// Create material for progress bar filling, every tick rare
        /// </summary>
        private Material BarFilledMat
        {
            get
            {
                if (barFilledCachedMat == null)
                {
                    var res = ProcessDef;
                    barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(res.lowProgressColor, res.finishedColor, Process.Progress));
                }
                return barFilledCachedMat;
            }
        }

        public ProcessStack ProcessStack => processStack;

        public Process Process => processStack.FirstCanDo;

        public ProcessDef ProcessDef => Process.Def;

        /// <summary>
        /// All processes as list of FloatMenuOption
        /// </summary>
        public List<FloatMenuOption> ProcessesOptions
        {
            get
            {
                if (processesOptions == null)
                {
                    processesOptions = new List<FloatMenuOption>();
                    for (int i = 0; i < Props.processes.Count; i++)
                    {
                        var process = Props.processes[i];
                        var name = process.thing != null ? process.thing.LabelCap.ToStringSafe() : process.pipeNet.resource.name;
                        var label = "PipeSystem_MakeProcess".Translate(name);
                        if (process.count > 1)
                        {
                            label += " x" + process.count;
                        }
                        processesOptions.Add(new FloatMenuOption(label, () => processStack.AddProcess(process, parent),
                                                                 process.thing, null, false, MenuOptionPriority.Default,
                                                                 (Rect rect) => process.DoProcessInfoWindow(i, rect),
                                                                 null, 29f,
                                                                 (Rect rect) => process.thing != null && Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, process.thing),
                                                                 null, true));
                    }
                }

                return processesOptions;
            }
        }

        public bool PickupReady => Process.PickUpReady;

        /// <summary>
        /// Setup comps, gizmo, pre result setup
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            // Get comps
            flickable = parent.GetComp<CompFlickable>();
            compPower = parent.GetComp<CompPowerTrader>();
            compRefuelable = parent.GetComp<CompRefuelable>();
            // Setup FillableBarRequest
            if (Props.showProgressBar)
            {
                var drawPos = parent.TrueCenter() + Props.progressBarOffset;
                drawPos.y += 3f / 74f;
                drawPos.z += 0.25f;

                fillableBarRequest = new GenDraw.FillableBarRequest
                {
                    center = drawPos,
                    size = BarSize,
                    unfilledMat = BarUnfilledMat,
                    margin = 0.1f,
                    rotation = Rot4.North
                };
            }
            if (Props.showResultItem)
            {
                itemDrawPos = parent.TrueCenter();
                itemDrawPos.y += 4f / 74f;
                itemDrawPos += Props.resultItemOffset;
            }
            // Post spawn setup processes
            foreach (var process in processStack)
            {
                process.PostSpawnSetup();
            }
        }

        /// <summary>
        /// Clear def on destroy/despawn. Give back required resource if wanted
        /// </summary>
        public override void PostDeSpawn(Map map)
        {
            foreach (var process in processStack)
            {
                process.ResetProcess(false, map);
            }
            var manager = CachedAdvancedProcessorsManager.GetFor(map);
            manager.PickupDone(this);
            manager.RemoveFromAwaiting(this);
        }

        /// <summary>
        /// Save processStack, nextProcessTick, noNetCapacity, resultIndex, progressInt, pickUpReady and ingredientsOwners
        /// </summary>
        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref processStack, "processStack");
        }

        /// <summary>
        /// Call CompTickRare every 250 ticks
        /// </summary>
        public override void CompTick()
        {
            if (parent.IsHashIntervalTick(100) && AllCompsOn)
            {
                Process?.Tick(100);
                barFilledCachedMat = null;
            }
        }

        /// <summary>
        /// Tick process
        /// </summary>
        public override void CompTickRare()
        {
            if (AllCompsOn)
            {
                Process?.Tick(GenTicks.TickRareInterval);
                barFilledCachedMat = null;
            }
        }

        /// <summary>
        /// Tick process
        /// </summary>
        public override void CompTickLong()
        {
            if (AllCompsOn)
            {
                Process?.Tick(GenTicks.TickLongInterval);
                barFilledCachedMat = null;
            }
        }

        /// <summary>
        /// Draw progress bar
        /// </summary>
        public override void PostDraw()
        {
            if (Process == null)
                return;

            if (Props.showResultItem && ProcessDef.thing != null)
            {
                var matrix = default(Matrix4x4);
                matrix.SetTRS(itemDrawPos, Quaternion.identity, Props.resultItemSize);
                Graphics.DrawMesh(MeshPool.plane10, matrix, ProcessDef.thing.graphic.MatNorth, 0);
            }
            if (Props.showProgressBar)
            {
                fillableBarRequest.fillPercent = Process.Progress;
                fillableBarRequest.filledMat = BarFilledMat;
                GenDraw.DrawFillableBar(fillableBarRequest);
            }
        }
    }
}
