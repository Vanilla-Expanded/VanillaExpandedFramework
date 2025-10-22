using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using static Verse.GenDraw;

namespace PipeSystem
{
    /// <summary>
    /// Comp used by CompProperties_ResourceStorage.
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompResourceStorage : CompResource
    {
        public bool markedForExtract = false;
        public bool markedForTransfer = false;
        public bool markedForRefill = false;
        public float extractResourceAmount;

        private float amountStored;
        private int ticksWithoutPower = 0;
        private bool isBreakdownable;
        private FillableBarRequest request;
        private Command_Action extractGizmo;
        private Command_Action transferGizmo;
        private Command_Toggle refillGizmo;

        private PipeNetOverlayDrawer pipeNetOverlayDrawer;

        private static readonly Texture2D transferIcon = ContentFinder<Texture2D>.Get("UI/TransferStorageContent");

        public new CompProperties_ResourceStorage Props => (CompProperties_ResourceStorage)props;

        public float AmountStored
        {
            get => amountStored;
            private set
            {
                amountStored = value;
                // Delete the drain designation if it's present and we can no longer drain from the storage
                if (parent.Map != null && !CanExtract)
                    parent.Map.designationManager.DesignationOn(parent, PSDefOf.PS_Drain)?.Delete();
            }
        }

        public float AmountStoredPct => amountStored / Props.storageCapacity;
        public float AmountCanAccept
        {
            get
            {
                if (isBreakdownable && parent.IsBrokenDown())
                    return 0f;
                if (Props.contentRequirePower && !powerComp.PowerOn)
                    return 0f;
                return Props.storageCapacity - amountStored;
            }
        }

        public bool ContentCanRot { get; private set; }

        private bool CanExtract
        {
            get
            {
                var opt = Props.extractOptions;
                if (opt == null)
                    return false;

                return CurrentManualExtractAmount().itemAmount >= 1;
            }
        }

        /// <summary>
        /// Ticks methods are only needed for contentRequirePower
        /// </summary>
        public override void CompTickInterval(int delta) => Tick(delta);

        public override void CompTickRare() => Tick(GenTicks.TickRareInterval);

        public override void CompTickLong() => Tick(GenTicks.TickLongInterval);

        /// <summary>
        /// Tick storage if contentRequirePower set to true
        /// </summary>
        /// <param name="ticks">Number of tick(s) passed</param>
        private void Tick(int ticks = 1)
        {
            if (!ContentCanRot)
                return;

            if (!powerComp.PowerOn && amountStored > 0)
            {
                if (Props.preventRotInNegativeTemp)
                {
                    var map = parent.MapHeld;
                    if (map == null || parent.Position.GetTemperature(map) < 0)
                        return;
                }

                ticksWithoutPower += ticks;
                if (ticksWithoutPower > GenDate.TicksPerDay * Props.daysToRotStart)
                {
                    AmountStored = 0;
                    Messages.Message("PipeSystem_StorageContentRotted".Translate(parent.def.LabelCap), parent, MessageTypeDefOf.NegativeEvent);
                    ticksWithoutPower = 0;
                }
            }
            else
            {
                ticksWithoutPower = 0;
            }
        }

        /// <summary>
        /// Create FillableBarRequest and gizmos
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            CachedCompResourceStorage.Cache(this);
            isBreakdownable = parent.TryGetComp<CompBreakdownable>() != null;
            ContentCanRot = Props.contentRequirePower && powerComp != null;

            pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();
            // Fillable bar request
            request = new FillableBarRequest
            {
                center = parent.DrawPos + Props.centerOffset + Vector3.up * 0.1f,
                size = Props.barSize,
                fillPercent = AmountStoredPct,
                filledMat = MaterialCreator.materials.TryGetValue(Props.pipeNet, MaterialCreator.BarFallbackMat),
                unfilledMat = MaterialCreator.BarUnfilledMat,
                margin = Props.margin,
                rotation = (Props.barHorizontal, Props.rotateBarWithBuilding) switch
                {
                    (false, false) => Rot4.East,
                    (true, false) => Rot4.North,
                    (false, true) => parent.Rotation.Rotated(RotationDirection.Clockwise),
                    (true, true) => parent.Rotation,
                },
            };
            if (Props.extractOptions != null)
            {
                extractResourceAmount = Props.extractOptions.ratio * Props.extractOptions.extractAmount;

                extractGizmo = new Command_Action()
                {
                    action = delegate
                    {
                        markedForExtract = !markedForExtract;
                        UpdateDesignation(parent);
                    },
                    defaultLabel = Props.extractOptions.labelKey.Translate(ExtractResourceArguments()),
                    defaultDesc = Props.extractOptions.descKey.Translate(ExtractResourceArguments()),
                    icon = Props.extractOptions.tex
                };
            }
            // Refill gizmo
            if (Props.refillOptions != null && !Props.refillOptions.alwaysRefill)
            {
                refillGizmo = new Command_Toggle()
                {
                    isActive = () => markedForRefill,
                    toggleAction = delegate
                    {
                        markedForRefill = !markedForRefill;
                        PipeNetManager.UpdateRefillableWith(parent);
                    },
                    defaultLabel = "PipeSystem_AllowManualRefill".Translate(),
                    defaultDesc = "PipeSystem_AllowManualRefillDesc".Translate(),
                    icon = TexCommand.ForbidOff
                };
                // Loading save, marked for refill: update refillables
                if (markedForRefill) PipeNetManager.UpdateRefillableWith(parent);
            }
            // Always refill: update refillables
            else if (Props.refillOptions != null && Props.refillOptions.alwaysRefill)
            {
                PipeNetManager.UpdateRefillableWith(parent);
            }
            // Transfer gizmo
            if (Props.addTransferGizmo)
            {
                transferGizmo = new Command_Action()
                {
                    action = delegate
                    {
                        markedForTransfer = !markedForTransfer;
                        if (markedForTransfer)
                        {
                            PipeNet.markedForTransfer.Add(this);
                            PipeNet.storages.Remove(this);
                        }
                        else
                        {
                            PipeNet.markedForTransfer.Remove(this);
                            PipeNet.storages.Add(this);
                        }
                        pipeNetOverlayDrawer?.ToggleStatic(parent, MaterialCreator.transferMat, markedForTransfer);
                    },
                    defaultLabel = "PipeSystem_TransferContent".Translate(),
                    defaultDesc = "PipeSystem_TransferContentDesc".Translate(),
                    icon = transferIcon
                };
                pipeNetOverlayDrawer?.ToggleStatic(parent, MaterialCreator.transferMat, markedForTransfer);
            }
        }

        /// <summary>
        /// Toggle off overlay
        /// </summary>
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            base.PostDeSpawn(map,mode);
            pipeNetOverlayDrawer?.ToggleStatic(parent, MaterialCreator.transferMat, false);
        }

        /// <summary>
        /// Draw the fillable bar
        /// </summary>
        public override void PostDraw()
        {
            base.PostDraw();
            if (Props.drawStorageBar)
            {
                request.fillPercent = AmountStoredPct;
                DrawFillableBar(request);
            }
        }

        /// <summary>
        /// Save data
        /// </summary>
        public override void PostExposeData()
        {
            if (amountStored > Props.storageCapacity) AmountStored = Props.storageCapacity;

            Scribe_Values.Look(ref amountStored, "storedResource", 0f);
            Scribe_Values.Look(ref ticksWithoutPower, "tickWithoutPower");
            Scribe_Values.Look(ref markedForExtract, "markedForExtract");
            Scribe_Values.Look(ref markedForTransfer, "markedForTransfer");
            Scribe_Values.Look(ref markedForRefill, "markedForRefill");
            base.PostExposeData();
        }

        /// <summary>
        /// Apply destroy option(s)
        /// </summary>
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            // Only drop stuff in those specific 4 destroy modes.
            // Vanish and QuestLogic generally don't leave anything behind. WillReplace, as name suggests, will replace the thing that is destroyed.
            // Deconstruct, FailConstruction, and Cancel only apply to blueprints and frames, so they won't be called.
            if (previousMap != null && Props.destroyOptions.HasData() && mode is DestroyMode.KillFinalize or DestroyMode.KillFinalizeLeavingsOnly or DestroyMode.Deconstruct or DestroyMode.Refund)
            {
                var pos = parent.Position;

                foreach (var destroyOption in Props.destroyOptions)
                {
                    // Check if we're allowed to spawn stuff and move on to the next destroy option
                    switch (mode)
                    {
                        case DestroyMode.KillFinalize when !destroyOption.spawnWhenDestroyed:
                        case DestroyMode.KillFinalizeLeavingsOnly when !destroyOption.spawnWhenDestroyed: // Probably will never trigger
                        case DestroyMode.Deconstruct when !destroyOption.spawnWhenDeconstructed:
                        case DestroyMode.Refund when !destroyOption.spawnWhenRefunded:
                            continue;
                    }

                    var count = destroyOption.amount;
                    // Avoid division by 0
                    if (!Mathf.Approximately(destroyOption.ratio, 0f))
                        count += Mathf.FloorToInt(amountStored / destroyOption.ratio);
                    var def = destroyOption.thing;

                    if (def.IsFilth)
                    {
                        for (var i = 0; i < count; i++)
                        {
                            FilthMaker.TryMakeFilth(CellFinder.StandableCellNear(pos, previousMap, destroyOption.maxRadius), previousMap, def);
                        }
                    }
                    else
                    {
                        ThingPlaceMode placeMode;
                        int squareRadius;

                        if (destroyOption.maxRadius > 0)
                        {
                            placeMode = ThingPlaceMode.Radius;
                            squareRadius = destroyOption.maxRadius * destroyOption.maxRadius;
                        }
                        else
                        {
                            placeMode = ThingPlaceMode.Near;
                            squareRadius = 1;
                        }

                        while (count > 0)
                        {
                            var thing = ThingMaker.MakeThing(def);
                            if (thing.def.CanHaveFaction && parent.Faction is {} faction)
                                thing.SetFaction(faction);
                            if (destroyOption.spawnMinified)
                                thing = thing.TryMakeMinified();
                            thing.stackCount = Mathf.Min(count, thing.def.stackLimit);
                            count -= thing.stackCount;
                            GenPlace.TryPlaceThing(thing, pos, previousMap, placeMode, squareRadius: squareRadius);
                        }
                    }
                }

            }
            base.PostDestroy(mode, previousMap);
        }

        /// <summary>
        /// Add resources to a storage
        /// </summary>
        /// <param name="amount">Amount to add</param>
        public void AddResource(float amount)
        {
            if (amount < 0f) // If it's less than 0, error and out
            {
                Log.Error("[PipeSystem] Cannot add negative resource " + amount);
                return;
            }
            AmountStored += amount;
            if (amountStored > Props.storageCapacity) // Capping the amount to the maximum acceptable
                AmountStored = Props.storageCapacity;
        }

        /// <summary>
        /// Withdraw resource from storage
        /// </summary>
        /// <param name="amount">Amount to withdraw</param>
        public void DrawResource(float amount)
        {
            AmountStored -= amount;
            if (amountStored < 0f) // If we withdrawn to much, error and set stored amount to 0
            {
                Log.Error("[PipeSystem] Drawing resource we don't have from " + parent);
                AmountStored = 0f;
            }
        }

        /// <summary>
        /// Empty storage
        /// </summary>
        public void Empty() => AmountStored = 0;

        /// <summary>
        /// Handle breakdown signal
        /// </summary>
        public override void ReceiveCompSignal(string signal)
        {
            if (signal == CompBreakdownable.BreakdownSignal) // If the parent break down, we set the amount stored to 0
                AmountStored = 0f;
        }

        /// <summary>
        /// Add storage info to inspect string
        /// </summary>
        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (Props.addStorageInfo)
                sb.AppendInNewLine($"{"PipeSystem_ResourceStored".Translate(Resource.name)} {amountStored:##0} / {Props.storageCapacity:F0} {Resource.unit}"); // Show the amount stored

            if (markedForTransfer)
                sb.AppendInNewLine("PipeSystem_MarkedToTransferContent".Translate());

            if (ContentCanRot && !powerComp.PowerOn && amountStored > 0 && (!Props.preventRotInNegativeTemp || parent.MapHeld == null || parent.Position.GetTemperature(parent.MapHeld) >= 0))
                sb.AppendInNewLine("PipeSystem_ContentWillRot".Translate(((int)((GenDate.TicksPerDay * Props.daysToRotStart) - ticksWithoutPower)).ToStringTicksToPeriod()));

            sb.AppendInNewLine(base.CompInspectStringExtra());
            return sb.ToString().TrimEndNewlines();
        }

        /// <summary>
        /// Add gizmos to storage
        /// </summary>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (transferGizmo != null)
                yield return transferGizmo;

            if (refillGizmo != null)
                yield return refillGizmo;

            if (extractGizmo != null)
            {
                if (CanExtract)
                    extractGizmo.Disabled = false;
                else
                    extractGizmo.Disable(Props.extractOptions.disabledReasonKey.NullOrEmpty() ? null : Props.extractOptions.disabledReasonKey.Translate(ExtractResourceArguments()));
                yield return extractGizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = new Action(() =>
                    {
                        AmountStored = Props.storageCapacity;
                    })
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Add 5",
                    action = new Action(() =>
                    {
                        if (amountStored + 5 > Props.storageCapacity)
                            AmountStored = Props.storageCapacity;
                        else
                            AmountStored += 5;
                    })
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = new Action(() =>
                    {
                        AmountStored = 0f;
                    })
                };
            }
        }

        /// <summary>
        /// Manage drain designation on parent
        /// </summary>
        private void UpdateDesignation(Thing t)
        {
            Designation designation = t.Map.designationManager.DesignationOn(t, PSDefOf.PS_Drain);
            if (designation == null)
            {
                t.Map.designationManager.AddDesignation(new Designation(t, PSDefOf.PS_Drain));
            }
            else
            {
                designation.Delete();
            }
        }

        private NamedArgument[] ExtractResourceArguments()
        {
            var opt = Props.extractOptions;
            if (opt == null)
                return [];

            var itemExtractAmount = Mathf.FloorToInt(amountStored / opt.ratio);

            // Check comments in CompProperties_ResourceStorage.ExtractOptions class for explanation/more info
            return
            [
                (opt.extractExactAmount ? extractResourceAmount : opt.ratio).Named("RESOURCEMIN"),
                amountStored.Named("RESOURCECOUNT"),
                (itemExtractAmount * opt.ratio).Named("RESOURCEEXTRACTCOUNT"),
                extractResourceAmount.Named("RESOURCEMAX"),
                (opt.extractExactAmount ? opt.extractAmount : 1).Named("THINGMIN"),
                itemExtractAmount.Named("THINGCOUNT"),
                itemExtractAmount.Named("THINGEXTRACTCOUNT"),
                opt.extractAmount.Named("THINGMAX")
            ];
        }

        public (float resourceAmount, int itemAmount) CurrentManualExtractAmount()
        {
            var opt = Props.extractOptions;
            if (opt == null)
                return (0, 0);
            if (opt.extractExactAmount)
            {
                if (AmountStored < extractResourceAmount)
                    return (0, 0);
                return (extractResourceAmount, opt.extractAmount);
            }

            var itemAmount = Mathf.FloorToInt(Mathf.Min(AmountStored, extractResourceAmount) / opt.ratio);
            return (itemAmount * opt.ratio, itemAmount);
        }
    }
}