using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
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

        public List<CachedIngredient> cachedIngredients = new List<CachedIngredient>();

        // Other comps we run check on
        public CompFlickable flickable;
        public CompPowerTrader compPower;
        public CompRefuelable compRefuelable;
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
        private ProcessStack cachedProcessStack;

        private List<FloatMenuOption> settingsOptions;          // List of settings
        internal bool outputOnGround = false;                   // Should output on ground

        public Graphic_Single cachedProgressGraphic = null;
        public Graphic_Single cachedFinishedGraphic = null;

        public Graphic_Multi cachedProgressGraphic_multi = null;
        public Graphic_Multi cachedFinishedGraphic_multi = null;

        //A flag to only send the power warning message once
        public bool onlySendWarningMessageOnce = false;
        //A flag to only send the light warning message once
        public bool onlySendLightWarningMessageOnce = false;
        //A flag to only send the rain warning message once
        public bool onlySendRainWarningMessageOnce = false;
        //A flag to only send the temperature warning message once
        public bool onlySendTempWarningMessageOnce = false;

        public int noPowerDestructionCounter = 0;
        public int noGoodLightDestructionCounter = 0;
        public int noGoodWeatherDestructionCounter = 0;
        public int noGoodTempDestructionCounter = 0;

        public float overclockMultiplier = 1;


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
                List<ProcessDef> processes = Props.processes.Where(x => !x.hideProcessIfNotNaturalRock || Find.World.NaturalRockTypesIn(this.parent.Map.Tile).Contains(x.rockToDetect)).OrderBy(x => x.priorityInBillList).ToList();
                for (int i = 0; i < processes.Count; i++)
                {
                    var process = processes[i];
                    if (process.researchPrerequisites != null && process.researchPrerequisites.Any(p => !p.IsFinished)) continue;





                    var label = "";
                    if (process.labelOverride != "")
                    {
                        label = process.labelOverride;
                    }
                    else
                    {
                        var name = process.results[0].thing != null
                        ? process.results[0].thing.LabelCap.ToStringSafe()
                        : process.results[0].pipeNet.resource.name;

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
                                                             (Rect rect) => process.results[0].thing != null && Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, process),
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
            var shader = parent.def.graphicData?.shaderType?.Shader ?? ShaderDatabase.Cutout;
            if (ContentFinder<Texture2D>.Get(Props.inProgressTexture + "_north", reportFailure: false) != null)
            {
                cachedProgressGraphic_multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(Props.inProgressTexture, shader,
                     this.parent.def.graphicData.drawSize, this.parent.DrawColor);
            }
            else

                cachedProgressGraphic = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(Props.inProgressTexture, shader,
                         this.parent.def.graphicData.drawSize, this.parent.DrawColor);

        }
        public void StoreFinishGraphics()
        {
            var shader = parent.def.graphicData?.shaderType?.Shader ?? ShaderDatabase.Cutout;
            if (ContentFinder<Texture2D>.Get(Props.finishedTexture + "_north", reportFailure: false) != null)
            {
                cachedFinishedGraphic_multi = (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(Props.finishedTexture, shader,
                     this.parent.def.graphicData.drawSize, this.parent.DrawColor);
            }
            else
                cachedFinishedGraphic = (Graphic_Single)GraphicDatabase.Get<Graphic_Single>(Props.finishedTexture, shader,
                     this.parent.def.graphicData.drawSize, this.parent.DrawColor);

        }

        /// <summary>
        /// Clear def on destroy/despawn. Give back required resource if wanted
        /// </summary>
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode != DestroyMode.WillReplace)
            {
                foreach (var process in processStack)
                {
                    process.ResetProcess(false);
                }
                var manager = CachedAdvancedProcessorsManager.GetFor(map);
                manager.PickupDone(this);
                manager.RemoveFromAwaiting(this);
            }

        }

        /// <summary>
        /// Save processStack, nextProcessTick, noNetCapacity, resultIndex, progressInt, pickUpReady and ingredientsOwners
        /// </summary>
        public override void PostExposeData()
        {
            Scribe_Deep.Look(ref processStack, "processStack");

            Scribe_Values.Look(ref outputOnGround, "outputOnGround");
            Scribe_Values.Look(ref wasteProduced, "wasteProduced");
            Scribe_Values.Look(ref overclockMultiplier, "overclockMultiplier");
            Scribe_Collections.Look(ref cachedIngredients, "cachedIngredients", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                cachedIngredients ??= new List<CachedIngredient>();
            }
        }

        /// <summary>
        /// Tick 100 ticks
        /// </summary>
        public override void CompTickInterval(int delta)
        {
            if (parent.IsHashIntervalTick(100, delta))
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
            CheckProcessRuiners();
            if (AllCompsOn)
            {
                // Wastepack stop check
                if (Props.stopWhenWastepackFull && Container.Full) return;
                // Tick process
                //Process?.Tick((int)(ticks * overclockMultiplier * GetNotInRoomRoleFactor(parent)));
                Process?.Tick(ticks);
                // Push heat
                if (Props.heatPushWhileWorking && Process != null && !Process.MissingIngredients)
                    GenTemperature.PushHeat(parent, parent.def.building.heatPerTickWhileWorking * ticks);
                if (compRefuelable != null && compRefuelable.Props.consumeFuelOnlyWhenUsed && Process != null && !Process.MissingIngredients)
                {
                    compRefuelable.ConsumeFuel(ticks * (compRefuelable.Props.fuelConsumptionRate / 60000f));
                }

                barFilledCachedMat = null;
            }
        }

        private void CheckProcessRuiners()
        {
            if (Process?.Progress > 0 && parent.Map != null)
            {
                if ((Process.Def.noPowerDestroysProgress && compPower != null && !compPower.PowerOn) ||
                                (Process.Def.noPowerDestroysProgress && compRefuelable != null && !compRefuelable.HasFuel))
                {
                    if (!onlySendWarningMessageOnce)
                    {
                        Messages.Message(Process.Def.noPowerDestroysInitialWarning.Translate(), parent, MessageTypeDefOf.NegativeEvent, true);
                        onlySendWarningMessageOnce = true;
                    }
                    noPowerDestructionCounter++;
                    if (noPowerDestructionCounter > Process.Def.rareTicksToDestroy)
                    {
                        Messages.Message(Process.Def.noPowerDestroysMessage.Translate(), parent, MessageTypeDefOf.NegativeEvent, true);
                        Process.ResetProcess(false);
                    }
                }
                else if (Process.Def.isLightDependingProcess)
                {
                    float num = parent.Map.glowGrid.GroundGlowAt(parent.Position, false);
                    if ((num > Process.Def.maxLight) || (num < Process.Def.minLight))
                    {
                        if (!onlySendWarningMessageOnce)
                        {
                            Messages.Message(Process.Def.messageIfOutsideLightRangesWarning.Translate(), parent, MessageTypeDefOf.NegativeEvent, true);
                            onlySendWarningMessageOnce = true;
                        }
                        noGoodLightDestructionCounter++;
                        if (noGoodLightDestructionCounter > Process.Def.rareTicksToDestroy)
                        {
                            Messages.Message(Process.Def.messageIfOutsideLightRanges.Translate(), parent, MessageTypeDefOf.NegativeEvent, true);
                            Process.ResetProcess(false);
                        }
                    }
                }
                else if (Process.Def.isRainDependingProcess)
                {
                    if (parent.Map.weatherManager.curWeather.rainRate > 0 && !parent.Position.Roofed(parent.Map))
                    {
                        if (!onlySendWarningMessageOnce)
                        {
                            Messages.Message(Process.Def.messageIfRainingWarning.Translate(), parent, MessageTypeDefOf.NegativeEvent, true);
                            onlySendWarningMessageOnce = true;
                        }
                        noGoodWeatherDestructionCounter++;
                        if (noGoodWeatherDestructionCounter > Process.Def.rareTicksToDestroy)
                        {
                            Messages.Message(Process.Def.messageIfRaining.Translate(), parent, MessageTypeDefOf.NegativeEvent, true);
                            Process.ResetProcess(false);
                        }
                    }

                }
                else
                {
                    noPowerDestructionCounter = 0;
                    noGoodLightDestructionCounter = 0;
                    noGoodWeatherDestructionCounter = 0;
                    onlySendWarningMessageOnce = false;
                }




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

            if (Props.showResultItem && ProcessDef.results[0].GetOutput(Process) is ThingDef def)
            {
                var matrix = default(Matrix4x4);
                matrix.SetTRS(itemDrawPos, Quaternion.identity, Props.resultItemSize);
                Graphics.DrawMesh(MeshPool.plane10, matrix, def.graphic.MatNorth, 0);
            }
            if (Props.showProgressBar && (Props.alwaysShowProgressBar || (!Props.alwaysShowProgressBar && parent.OccupiedRect().Cells.Contains(UI.MouseCell()))))
            {
                fillableBarRequest.fillPercent = Process.Progress;
                fillableBarRequest.filledMat = BarFilledMat;
                GenDraw.DrawFillableBar(fillableBarRequest);
            }
            if (Props.inProgressTexture != "")
            {
                if (cachedProgressGraphic != null && Process.Progress > 0 && Process.Progress < 1)
                {
                    var vector = parent.DrawPos + Altitudes.AltIncVect;
                    vector.y += Altitudes.AltInc;
                    cachedProgressGraphic.DrawFromDef(vector, Rot4.North, null);
                }
                if (cachedProgressGraphic_multi != null && Process.Progress > 0 && Process.Progress < 1)
                {
                    var vector = parent.DrawPos + Altitudes.AltIncVect;
                    vector.y += Altitudes.AltInc;
                    cachedProgressGraphic_multi.DrawFromDef(vector, this.parent.Rotation, null);
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
                if (cachedFinishedGraphic_multi != null && Process.pickUpReady)
                {
                    var vector = parent.DrawPos + Altitudes.AltIncVect;
                    vector.y += Altitudes.AltInc;
                    cachedFinishedGraphic_multi.DrawFromDef(vector, this.parent.Rotation, null);
                }
            }
        }



        /// <summary>
        /// Add gizmos
        /// </summary>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (Process?.Def.allowExtractAtCurrentQuality == true)
            {
                int ticksDone = Process.Def.ticksQuality[(int)Process.qualityToOutput] - Process.tickLeft;
                Command_Action command_Action = new Command_Action();
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/PS_ExtractNow");
                command_Action.defaultLabel = "PipeSystem_ExtractAtCurrentQuality".Translate();
                command_Action.defaultDesc = "PipeSystem_ExtractAtCurrentQuality_Desc".Translate();
                command_Action.action = () =>
                {
                    Process.forceQualityOut = true;
                    Process.qualityToForce = Process.currentQuality;

                    Process.Tick(Process.TickLeft - 10);
                };

                if (ticksDone < Process.Def.ticksQuality[(int)QualityCategory.Awful])
                {
                    command_Action.Disable("PipeSystem_ProcessNeedsAwfulAtLeast".Translate(Process.Def.ticksQuality[(int)QualityCategory.Awful].ToStringTicksToDays()));
                }
                yield return command_Action;

            }

            if (ProcessUtility.Clipboard != null && ProcessUtility.Clipboard.TryGetValue(parent.def, out var processList))
            {
                Command_Action command_PasteProcesses = new Command_Action();
                command_PasteProcesses.icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings");
                command_PasteProcesses.defaultLabel = "PipeSystem_PasteAllRecipes".Translate();
                command_PasteProcesses.defaultDesc = "PipeSystem_PasteAllRecipes_Desc".Translate();
                command_PasteProcesses.action = () =>
                {
                    ProcessStack.Processes.Clear();
                    foreach (Process process in processList)
                    {
                        ProcessStack.AddProcess(process.Def, (ThingWithComps)parent, process.targetCount);
                    }

                    foreach (Process process in ProcessStack.Processes)
                    {
                        process.Progress = 0;
                    }
                };


                yield return command_PasteProcesses;


            }

            if (Props.canOverclock)
            {
                Command_Action command_overclock = new Command_Action();

                command_overclock.defaultDesc = Props.overclockDesc.Translate(overclockMultiplier.ToStringPercent());
                command_overclock.defaultLabel = Props.overclockLabel.Translate(overclockMultiplier.ToStringPercent());
                command_overclock.icon = ContentFinder<Texture2D>.Get(Props.overclockGizmo, true);
                command_overclock.hotKey = KeyBindingDefOf.Misc1;
                command_overclock.action = delegate
                    {
                        Window_Overclock overclockWindow = new Window_Overclock(this);
                        Find.WindowStack.Add(overclockWindow);
                    };
                yield return command_overclock;

            }

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Finish in 10 ticks",
                    action = () => Process?.Tick(Process.TickLeft - 10)
                };
                yield return new Command_Action
                {
                    defaultLabel = "Advance progress 1 day",
                    action = () => Process?.Tick(60000)
                };
                if (shouldProduceWastePack)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Empty wastepack(s)",
                        action = () => Container?.innerContainer.Clear()
                    };
                }

            }
        }


        /// <summary>
        /// Add miscellaneous info in inspect string
        /// </summary>
        public override string CompInspectStringExtra()
        {
            var sb = new StringBuilder();
            if (GetNotInRoomRoleFactor(parent) != 1)
                sb.AppendLine("NotInRoomRole".Translate(parent.def.building.workTableRoomRole.label).CapitalizeFirst() + ": " + parent.def.building.workTableNotInRoomRoleFactor.ToStringPercent() + " " + "PipeSystem_WorkSpeed".Translate());

            var process = Process;
            if (process == null) return sb.ToString().TrimEndNewlines();

            if (!process.Def.hideProgressInInfobox)
            {
                sb.AppendLine("PipeSystem_ProgressInInfobox".Translate(process.Def.label, process.Progress.ToStringPercent()));
            }

            if (process.Def.allowExtractAtCurrentQuality)
            {
                int ticksDone = Process.Def.ticksQuality[(int)Process.qualityToOutput] - Process.tickLeft;
                if (Process.forceQualityOut)
                {
                    sb.AppendLine("PipeSystem_ExtractingAtCurrent".Translate());
                }
                else
                if (ticksDone < Process.Def.ticksQuality[(int)QualityCategory.Awful])
                {
                    sb.AppendLine("PipeSystem_CurrentNotReachedAwful".Translate());
                }
                else
                {
                    sb.AppendLine("PipeSystem_CurrentQuality".Translate(Process.currentQuality.GetLabel().CapitalizeFirst()));

                }

            }

            if (process.MissingIngredients)
            {
                string requirements = "";

                for (int i = 0; i < process.IngredientsOwners.Count; i++)
                {
                    if (process.IngredientsOwners[i].Require)
                    {
                        if (process.Def.disallowMixing && process.GetLastStoredIngredient() is ThingDef def)
                        {
                            requirements += process.IngredientsOwners[i].ToStringHumanReadable(def);
                        }
                        else
                        {
                            requirements += process.IngredientsOwners[i].ToStringHumanReadable();
                        }
                    }
                }

                sb.AppendLine("PipeSystem_MissingInputIngredients".Translate(requirements));
            }

            if (process.Def.temperatureRuinable)
                sb.AppendLine("IP_TempRangeInThisMachine".Translate(process.Def.minSafeTemperature, process.Def.maxSafeTemperature));

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
                sb.AppendLine("WasteLevel".Translate() + ": " + WasteProducedPercentFull.ToStringPercent());
            if (process.outputFactoryHopperIncorrect)
                sb.AppendLine("PipeSystem_OutputFactoryHopperIncorrect".Translate());

            return sb.ToString().TrimEndNewlines();
        }


        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {


            if (GetNotInRoomRoleFactor(this.parent) != 1)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "NotInRoomRole".Translate(parent.def?.building?.workTableRoomRole?.label).CapitalizeFirst(), parent.def?.building?.workTableNotInRoomRoleFactor.ToStringPercent(), "PipeSystem_NotInRoomExplanation".Translate(parent.def?.building?.workTableRoomRole?.label, parent.def?.building?.workTableNotInRoomRoleFactor.ToStringPercent()), 2001);
            }
        }

        /// <summary>
        /// Divides times by workTableNotInRoomRoleFactor if the building has an assigned workTableRoomRole
        /// </summary>
        public float GetNotInRoomRoleFactor(ThingWithComps parent)
        {
            if (parent?.def?.building?.workTableRoomRole != null)
            {
                Room room = parent.GetRoom();
                if (room?.Role != parent.def.building.workTableRoomRole)
                {
                    return parent.def.building.workTableNotInRoomRoleFactor;
                }
            }

            return 1f;
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

        /// <summary>
        /// Used for Graveship lift-off / landing
        /// </summary>

        public override void PostSwapMap()
        {
            processStack = cachedProcessStack;
            base.PostSwapMap();
        }

        /// <summary>
        /// Used for Graveship lift-off / landing
        /// </summary>

        public override void PreSwapMap()
        {
            cachedProcessStack = processStack;
            base.PreSwapMap();
        }


        public override void PostDrawExtraSelectionOverlays()
        {
            
            if (Process != null && !Process.Def.autoInputSlots.NullOrEmpty())
            {
                foreach (IntVec3 inputSlot in Process.Def.autoInputSlots)
                {
                    ProcessUtility.DrawSlot(parent, inputSlot, GraphicsCache.InputCellMaterial);
                }
            }
            if (Process != null && parent.def.hasInteractionCell && Process.Def.autoExtract)
            {
                ProcessUtility.DrawSlot(parent, parent.def.interactionCellOffset, GraphicsCache.OutputCellMaterial);

            }
        }

    }

    public class CachedIngredient : IExposable
    {
        public ThingDef thingDef;
        public int count;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref thingDef, "thingDef");
            Scribe_Values.Look(ref count, "count");
        }
    }

}
