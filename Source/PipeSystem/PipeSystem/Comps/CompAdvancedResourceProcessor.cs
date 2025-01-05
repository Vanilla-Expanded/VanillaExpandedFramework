using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
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

        private static readonly Material WasteBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f));
        private static readonly Material WasteBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 1f));

        public List<ThingDef> cachedIngredients = new List<ThingDef>();

        // Other comps we run check on
        private CompFlickable flickable;
        private CompPowerTrader compPower;
        private CompRefuelable compRefuelable;
        private CompWasteProducer wasteProducer;
        private CompThingContainer container;

        private bool shouldProduceWastePack = false;            // Can and should produce wastepack?
        private float wasteProduced = 0;
        private GenDraw.FillableBarRequest fillableWasteBar;    // FillableBarRequest cache

        private Vector3 itemDrawPos;                            // Drawing gizmo position
        private Material barFilledCachedMat;                    // Cached progress bar material
        private GenDraw.FillableBarRequest fillableBarRequest;  // FillableBarRequest cache

        private List<FloatMenuOption> processesOptions;         // List of processes
        private ProcessStack processStack = new ProcessStack(); // Process stack

        private List<FloatMenuOption> settingsOptions;          // List of settings
        internal bool outputOnGround = false;                   // Should output on ground

        public Graphic_Single cachedProgressGraphic = null;
        public Graphic_Single cachedFinishedGraphic = null;


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
                processesOptions = new List<FloatMenuOption>();
                List<ProcessDef> processes = Props.processes.OrderBy(x => x.priorityInBillList).ToList();
                for (int i = 0; i < processes.Count; i++)
                {
                    var process = processes[i];
                    if (process.researchPrerequisites != null && process.researchPrerequisites.Any(p => !p.IsFinished)) continue;


                    var name = process.results[0].thing != null ? process.results[0].thing.LabelCap.ToStringSafe() : process.results[0].pipeNet.resource.name;



                    var label ="";
                    if (process.labelOverride != "")
                    {
                        label = process.labelOverride;
                    }
                    else
                    {
                        label = "PipeSystem_MakeProcess".Translate(name);
                        if (process.results[0].count > 1)
                        {
                            label += " x" + process.results[0].count;
                        }
                    }
                    
                    
                    processesOptions.Add(new FloatMenuOption(label, () => processStack.AddProcess(process, parent),
                                                             process.results[0].thing, null, false, MenuOptionPriority.Default,
                                                             (Rect rect) => process.DoProcessInfoWindow(i, rect),
                                                             null, 29f,
                                                             (Rect rect) => process.results[0].thing != null && Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, process.results[0].thing),
                                                             null, true));
                }

                if (processesOptions.Count == 0)
                {
                    processesOptions.Add(new FloatMenuOption("PipeSystem_NoProcessAvailable".Translate(), () => { }));
                }

                return processesOptions;
            }
        }

        /// <summary>
        /// Settings
        /// </summary>
        public List<FloatMenuOption> Settings
        {
            get
            {
                if (settingsOptions == null)
                {
                    settingsOptions = new List<FloatMenuOption>
                    {
                        new FloatMenuOption("PipeSystem_OutputOnGround".Translate(), () => outputOnGround = !outputOnGround, extraPartWidth: 24f, extraPartOnGUI: (Rect rect) =>
                        {
                            var tex = outputOnGround ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
                            GUI.DrawTexture(new Rect(rect.x + 6f, rect.y + 3f, 24f, 24f), tex);
                            return false;
                        })
                    };
                }
                return settingsOptions;
            }
        }

        public bool PickupReady => Process.PickUpReady;

        public CompWasteProducer WasteProducer
        {
            get
            {
                if (wasteProducer == null)
                {
                    wasteProducer = parent.GetComp<CompWasteProducer>();
                }
                return wasteProducer;
            }
        }

        public CompThingContainer Container
        {
            get
            {
                if (container == null)
                {
                    container = parent.GetComp<CompThingContainer>();
                }
                return container;
            }
        }

        private int WasteProducedPerCycle => Container.Props.stackLimit;

        private float WasteProducedPercentFull => Container.Full ? 1f : wasteProduced / WasteProducedPerCycle;

        public ThingDef FirstIngredientMissing
        {
            get
            {
                var ingredientsOwners = Process.IngredientsOwners;
                for (int i = 0; i < ingredientsOwners.Count; i++)
                {
                    var ingredientOwner = ingredientsOwners[i];
                    if (ingredientOwner.Require && !ingredientOwner.BeingFilled && ingredientOwner.ThingDef != null)
                    {
                        return ingredientOwner.ThingDef;
                    }
                }
                return null;
            }
        }

        public ThingCategoryDef FirstCategoryMissing
        {
            get
            {
                var ingredientsOwners = Process.IngredientsOwners;
                for (int i = 0; i < ingredientsOwners.Count; i++)
                {
                    var ingredientOwner = ingredientsOwners[i];
                    if (ingredientOwner.Require && !ingredientOwner.BeingFilled && ingredientOwner.ThingCategoryDef != null)
                    {
                        return ingredientOwner.ThingCategoryDef;
                    }
                }
                return null;
            }
        }

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
            if (Props.showWastepackBar)
            {
                var drawPos = parent.TrueCenter() + Props.wastepackBarOffset;
                drawPos.y += 3f / 74f;
                drawPos.z += 0.25f;

                fillableWasteBar = new GenDraw.FillableBarRequest
                {
                    center = drawPos,
                    size = BarSize,
                    unfilledMat = WasteBarUnfilledMat,
                    filledMat = WasteBarFilledMat,
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
            if (Props.inProgressTexture != "")
            {
                LongEventHandler.ExecuteWhenFinished(delegate { StoreProgressGraphics(); });
            }
            if (Props.finishedTexture != "")
            {
                LongEventHandler.ExecuteWhenFinished(delegate { StoreFinishGraphics(); });
            }

            // Post spawn setup processes
            foreach (var process in processStack)
            {
                process.PostSpawnSetup();
            }

            shouldProduceWastePack = Props.processes.Any(p => p.wastePackToProduce > 0) && ModsConfig.BiotechActive;
        }

        public void StoreProgressGraphics()
        {
            
            cachedProgressGraphic = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(Props.inProgressTexture, ShaderDatabase.Cutout,
                     this.parent.def.graphicData.drawSize, this.parent.def.graphicData.color);

        }
        public void StoreFinishGraphics()
        {

            cachedFinishedGraphic = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(Props.finishedTexture, ShaderDatabase.Cutout,
                     this.parent.def.graphicData.drawSize, this.parent.def.graphicData.color);

        }

        /// <summary>
        /// Clear def on destroy/despawn. Give back required resource if wanted
        /// </summary>
        public override void PostDeSpawn(Map map)
        {
            foreach (var process in processStack)
            {
                process.ResetProcess(false);
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

            Scribe_Values.Look(ref outputOnGround, "outputOnGround");
            Scribe_Values.Look(ref wasteProduced, "wasteProduced");
            Scribe_Collections.Look(ref cachedIngredients, "cachedIngredients",LookMode.Def);
        }

        /// <summary>
        /// Tick 100 ticks
        /// </summary>
        public override void CompTick()
        {
            if (parent.IsHashIntervalTick(100))
                Tick(100);
        }

        /// <summary>
        /// Tick 250 ticks
        /// </summary>
        public override void CompTickRare() => Tick(GenTicks.TickRareInterval);

        /// <summary>
        /// Tick 2000 ticks
        /// </summary>
        public override void CompTickLong() => Tick(GenTicks.TickLongInterval);

        /// <summary>
        /// Tick process, heat push
        /// </summary>
        /// <param name="ticks">Number of ticks that passed</param>
        private void Tick(int ticks)
        {
            if (AllCompsOn)
            {
                // Wastepack stop check
                if (Props.stopWhenWastepackFull && Container.Full) return;
                // Tick process
                Process?.Tick(ticks);
                // Push heat
                if (Props.heatPushWhileWorking && Process != null && !Process.MissingIngredients)
                    GenTemperature.PushHeat(parent, parent.def.building.heatPerTickWhileWorking * ticks);

                barFilledCachedMat = null;
            }
        }

        /// <summary>
        /// Draw progress bar
        /// </summary>
        public override void PostDraw()
        {
            if (shouldProduceWastePack && Props.showWastepackBar)
            {
                fillableWasteBar.fillPercent = WasteProducedPercentFull;
                GenDraw.DrawFillableBar(fillableWasteBar);
            }

            if (Process == null)
                return;

            if (Props.showResultItem && ProcessDef.results[0].thing != null)
            {
                var matrix = default(Matrix4x4);
                matrix.SetTRS(itemDrawPos, Quaternion.identity, Props.resultItemSize);
                Graphics.DrawMesh(MeshPool.plane10, matrix, ProcessDef.results[0].thing.graphic.MatNorth, 0);
            }
            if (Props.showProgressBar)
            {
                fillableBarRequest.fillPercent = Process.Progress;
                fillableBarRequest.filledMat = BarFilledMat;
                GenDraw.DrawFillableBar(fillableBarRequest);
            }
            if (Props.inProgressTexture != "")
            {
                if (cachedProgressGraphic != null && Process.Progress >0&& Process.Progress<1) {
                    var vector = parent.DrawPos + Altitudes.AltIncVect;

                    vector.y += Altitudes.AltInc;


                    cachedProgressGraphic.DrawFromDef(vector, Rot4.North, null);

                }

            }
            if (Props.finishedTexture != "")
            {
                if (cachedFinishedGraphic != null && Process.pickUpReady)
                {
                    var vector = parent.DrawPos + Altitudes.AltIncVect;

                    vector.y += Altitudes.AltInc;


                    cachedFinishedGraphic.DrawFromDef(vector, Rot4.North, null);

                }

            }
        }

      

        /// <summary>
        /// Add debug gizmos
        /// </summary>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Finish in 10 ticks",
                    action = () => Process?.Tick(Process.TickLeft - 10)
                };
                yield return new Command_Action
                {
                    defaultLabel = "Empty wastepack(s)",
                    action = () => Container?.innerContainer.Clear()
                };
            }
        }

        /// <summary>
        /// Add wastepack info in inspect string
        /// </summary>
        public override string CompInspectStringExtra()
        {
            var process = Process;
            if (process == null) return null;

            var sb = new StringBuilder();

            if (!process.Def.hideProgressInInfobox)
            {
                sb.AppendLine("PipeSystem_ProgressInInfobox".Translate(process.Def.label, process.Progress.ToStringPercent()));
            }

            if (process.MissingIngredients)
            {
                string requirements = "";
                for (int i = 0; i < process.IngredientsOwners.Count; i++)
                {
                    if (process.IngredientsOwners[i].Require)
                    {
                        requirements += process.IngredientsOwners[i].ToStringHumanReadable();
                    }
                }
                sb.AppendLine("PipeSystem_MissingInputIngredients".Translate(requirements));
            }

            if (process.RuinedByTemp)
                sb.AppendLine("RuinedByTemperature".Translate());

            if (process.RuinedPercent > 0f)
            {
                var ambient = parent.AmbientTemperature;
                if (ambient > process.Def.maxSafeTemperature)
                {
                    sb.AppendLine("Overheating".Translate() + ": " + process.RuinedPercent.ToStringPercent());
                }
                else if (ambient < process.Def.minSafeTemperature)
                {
                    sb.AppendLine("Freezing".Translate() + ": " + process.RuinedPercent.ToStringPercent());
                }
            }

            if (shouldProduceWastePack)
                sb.Append("WasteLevel".Translate() + ": " + WasteProducedPercentFull.ToStringPercent());

            return sb.ToString().TrimEndNewlines();
        }

        /// <summary>
        /// Add amount to wasteProduced
        /// </summary>
        /// <param name="amount"></param>
        public void ProduceWastepack(int amount)
        {
            if (Container?.Full == false && ModsConfig.BiotechActive)
            {
                wasteProduced += amount;
                if (wasteProduced >= WasteProducedPerCycle && !Container.innerContainer.Any)
                {
                    wasteProduced = 0f;
                    WasteProducer.ProduceWaste(WasteProducedPerCycle);
                }
            }
        }
    }
}
