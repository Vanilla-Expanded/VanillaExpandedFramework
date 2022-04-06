﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Comp used by CompProperties_ResourceStorage.
    /// </summary>
    public class CompResourceStorage : CompResource
    {
        public bool markedForExtract = false;
        public float extractResourceAmount;

        private float amountStored;
        private bool isBreakdownable;
        private GenDraw.FillableBarRequest request;
        private Command_Action extractGizmo;

        public new CompProperties_ResourceStorage Props => (CompProperties_ResourceStorage)props;

        public float AmountStored => amountStored;
        public float AmountStoredPct => amountStored / Props.storageCapacity;
        public float AmountCanAccept
        {
            get
            {
                if (isBreakdownable && parent.IsBrokenDown())
                    return 0f;
                return Props.storageCapacity - amountStored;
            }
        }

        /// <summary>
        /// Create FillableBarRequest and gizmos
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            isBreakdownable = parent.TryGetComp<CompBreakdownable>() != null;
            // Fillable bar request
            request = new GenDraw.FillableBarRequest
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
                GenDraw.DrawFillableBar(request);
            }
        }

        /// <summary>
        /// Save data
        /// </summary>
        public override void PostExposeData()
        {
            if (amountStored > Props.storageCapacity) amountStored = Props.storageCapacity;

            Scribe_Values.Look(ref amountStored, "storedResource", 0f);
            Scribe_Values.Look(ref markedForExtract, "markedForExtract");
            base.PostExposeData();
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
                sb.Append($"{"PipeSystem_ResourceStored".Translate(Resource.name)} {amountStored:##0} / {Props.storageCapacity:F0} {Resource.unit}"); // Show the amount stored
            sb.AppendInNewLine(base.CompInspectStringExtra());

            return sb.ToString();
        }

        /// <summary>
        /// Add gizmos to storage
        /// </summary>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (extractGizmo != null)
            {
                extractGizmo.disabled = AmountStored < extractResourceAmount;
                yield return extractGizmo;
            }

            if (Prefs.DevMode)
            {
                Command_Action fill = new Command_Action
                {
                    defaultLabel = "DEBUG: Fill",
                    action = new Action(() =>
                    {
                        amountStored = Props.storageCapacity;
                    })
                };
                yield return fill;
                Command_Action draw = new Command_Action
                {
                    defaultLabel = "DEBUG: Empty",
                    action = new Action(() =>
                    {
                        amountStored = 0f;
                    })
                };
                yield return draw;
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