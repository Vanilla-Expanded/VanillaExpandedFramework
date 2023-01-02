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

        private static readonly Texture2D transferIcon = ContentFinder<Texture2D>.Get("UI/TransferStorageContent");

        public new CompProperties_ResourceStorage Props => (CompProperties_ResourceStorage)props;

        public float AmountStored => amountStored;
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

        /// <summary>
        /// Ticks methods are only needed for contentRequirePower
        /// </summary>
        public override void CompTick()
        {
            if (ContentCanRot) Tick();
        }

        public override void CompTickRare()
        {
            if (ContentCanRot) Tick(250);
        }

        public override void CompTickLong()
        {
            if (ContentCanRot) Tick(2000);
        }

        private void Tick(int ticks = 1)
        {
            if (!powerComp.PowerOn)
            {
                ticksWithoutPower += ticks;
                if (ticksWithoutPower > GenDate.TicksPerDay * Props.daysToRotStart)
                {
                    amountStored = 0;
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
            // Fillable bar request
            request = new FillableBarRequest
            {
                center = parent.DrawPos + Props.centerOffset + Vector3.up * 0.1f,
                size = Props.barSize,
                fillPercent = AmountStoredPct,
                filledMat = MaterialCreator.materials.TryGetValue(Props.pipeNet, MaterialCreator.BarFallbackMat),
                unfilledMat = MaterialCreator.BarUnfilledMat,
                margin = Props.margin,
                rotation = parent.Rotation.Rotated(RotationDirection.Clockwise)
            };
            // Extract gizmo
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
                    defaultLabel = Props.extractOptions.labelKey.Translate(),
                    defaultDesc = Props.extractOptions.descKey.Translate(),
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
            }
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
                    },
                    defaultLabel = "PipeSystem_TransferContent".Translate(),
                    defaultDesc = "PipeSystem_TransferContentDesc".Translate(),
                    icon = transferIcon
                };
            }
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
            if (markedForTransfer)
                IconOverlay.Render(MaterialCreator.transferMat, request.center, MeshPool.plane08);
        }

        /// <summary>
        /// Save data
        /// </summary>
        public override void PostExposeData()
        {
            if (amountStored > Props.storageCapacity) amountStored = Props.storageCapacity;

            Scribe_Values.Look(ref amountStored, "storedResource", 0f);
            Scribe_Values.Look(ref ticksWithoutPower, "tickWithoutPower");
            Scribe_Values.Look(ref markedForExtract, "markedForExtract");
            Scribe_Values.Look(ref markedForTransfer, "markedForTransfer");
            Scribe_Values.Look(ref markedForTransfer, "markedForRefill");
            base.PostExposeData();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (Props.destroyOption != null)
            {
                var pos = parent.Position;
                int num = (int)(amountStored / Props.destroyOption.ratio);
                for (int i = 0; i < num; i++)
                {
                    FilthMaker.TryMakeFilth(CellFinder.StandableCellNear(pos, previousMap, Props.destroyOption.maxRadius), previousMap, Props.destroyOption.filth);
                }
            }
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
            amountStored += amount;
            if (amountStored > Props.storageCapacity) // Capping the amount to the maximum acceptable
                amountStored = Props.storageCapacity;
        }

        /// <summary>
        /// Withdraw resource from storage
        /// </summary>
        /// <param name="amount">Amount to withdraw</param>
        public void DrawResource(float amount)
        {
            amountStored -= amount;
            if (amountStored < 0f) // If we withdrawn to much, error and set stored amount to 0
            {
                Log.Error("[PipeSystem] Drawing resource we don't have from " + parent);
                amountStored = 0f;
            }
        }

        /// <summary>
        /// Handle breakdown signal
        /// </summary>
        public override void ReceiveCompSignal(string signal)
        {
            if (signal == "Breakdown") // If the parent break down, we set the amount stored to 0
                amountStored = 0f;
        }

        /// <summary>
        /// Add storage info to inspect string
        /// </summary>
        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (Props.addStorageInfo)
                sb.AppendInNewLine($"{"PipeSystem_ResourceStored".Translate(Resource.name)} {amountStored:#00} / {Props.storageCapacity:F0} {Resource.unit}"); // Show the amount stored

            if (markedForTransfer)
                sb.AppendInNewLine("PipeSystem_MarkedToTransferContent".Translate());

            if (ContentCanRot && !powerComp.PowerOn)
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
                extractGizmo.disabled = AmountStored < extractResourceAmount;
                yield return extractGizmo;
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = new Action(() =>
                    {
                        amountStored = Props.storageCapacity;
                    })
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Add 5",
                    action = new Action(() =>
                    {
                        if (amountStored + 5 > Props.storageCapacity)
                            amountStored = Props.storageCapacity;
                        else
                            amountStored += 5;
                    })
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = new Action(() =>
                    {
                        amountStored = 0f;
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
    }
}