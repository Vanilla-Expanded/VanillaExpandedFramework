using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PipeSystem
{
    /// <summary>
    /// Comp for resource users and producers.
    /// </summary>
    public class CompResourceTrader : CompResource
    {
        public new CompProperties_ResourceTrader Props => (CompProperties_ResourceTrader)props;

        private bool resourceOnInt;
        private Sustainer sustainerResourceOn;

        private CompFlickable compFlickable;
        private CompSchedule compSchedule;
        private CompBreakdownable compBreakdownable;
        private CompRefuelable compRefuelable;
        private CompPowerTrader compPowerTrader;

        private PipeNetOverlayDrawer pipeNetOverlayDrawer;

        public string OnSignal => $"Resource{Resource.name}TurnedOn";
        public string OffSignal => $"Resource{Resource.name}TurnedOff";

        public float BaseConsumption { get; set; }
        public bool UsedLastTick { get; set; }
        public float Consumption
        {
            get
            {
                return Props.idleConsumptionPerTick >= 0f ? (UsedLastTick ? BaseConsumption : Props.idleConsumptionPerTick) : BaseConsumption;
            }
        }

        public bool ResourceOn
        {
            get
            {
                return resourceOnInt;
            }
            set
            {
                if (resourceOnInt == value)
                {
                    return;
                }
                resourceOnInt = value;
                if (resourceOnInt)
                {
                    if (!FlickUtility.WantsToBeOn(parent))
                    {
                        Log.Warning(string.Concat("Tried to turn resource on ", parent, " which did not desire it."));
                        return;
                    }
                    if (parent.IsBrokenDown())
                    {
                        Log.Warning(string.Concat("Tried to turn resource on ", parent, " which is broken down."));
                        return;
                    }
                    parent.BroadcastCompSignal(OnSignal);
                    StartSustainerIfInactive();
                }
                else
                {
                    parent.BroadcastCompSignal(OffSignal);
                    EndSustainerIfActive();
                }
                pipeNetOverlayDrawer?.TogglePulsing(parent, Props.pipeNet.offMat, !resourceOnInt);
            }
        }

        /// <summary>
        /// Get comps, mapComp, setup overlay and sustainer
        /// </summary>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            compFlickable = parent.TryGetComp<CompFlickable>();
            compSchedule = parent.TryGetComp<CompSchedule>();
            compBreakdownable = parent.TryGetComp<CompBreakdownable>();
            compRefuelable = parent.TryGetComp<CompRefuelable>();
            compPowerTrader = parent.TryGetComp<CompPowerTrader>();

            BaseConsumption = Props.consumptionPerTick;

            pipeNetOverlayDrawer = parent.Map.GetComponent<PipeNetOverlayDrawer>();
            pipeNetOverlayDrawer?.TogglePulsing(parent, Props.pipeNet.offMat, !resourceOnInt);

            if (ResourceOn)
                LongEventHandler.ExecuteWhenFinished(() => StartSustainerIfInactive());

            base.PostSpawnSetup(respawningAfterLoad);
        }

        /// <summary>
        /// Stop sustainer, toggle off overlay
        /// </summary>
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            pipeNetOverlayDrawer?.TogglePulsing(parent, Props.pipeNet.offMat, false);
            EndSustainerIfActive();
        }

        /// <summary>
        /// Save state
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref resourceOnInt, "resourceOn", true);
        }

        /// <summary>
        /// Show consumption or output
        /// </summary>
        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            if (Consumption >= 0)
            {
                sb.Append($"{"PipeSystem_ResourceNeeded".Translate(Resource.name)} {Consumption / 100 * GenDate.TicksPerDay:##0} {Resource.unit}/d");
            }
            else
            {
                sb.Append($"{"PipeSystem_ResourceOutput".Translate(Resource.name)} {(ResourceOn ? (-Consumption / 100) * GenDate.TicksPerDay : 0f):##0} {Resource.unit}/d");
            }

            sb.AppendInNewLine(base.CompInspectStringExtra());

            if (Prefs.DevMode)
                sb.AppendInNewLine(DebugString);

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Start the ambient sound sustainer.
        /// </summary>
        private void StartSustainerIfInactive()
        {
            if (Props.soundAmbientReceivingResource.NullOrUndefined() || sustainerResourceOn != null)
                return;
            sustainerResourceOn = Props.soundAmbientReceivingResource.TrySpawnSustainer(SoundInfo.InMap(parent));
        }

        /// <summary>
        /// End the ambient sound sustainer.
        /// </summary>
        private void EndSustainerIfActive()
        {
            if (sustainerResourceOn == null)
                return;
            sustainerResourceOn.End();
            sustainerResourceOn = null;
        }

        /// <summary>
        /// Managing ambient sound.
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();
            if (Props.soundAmbientReceivingResource == null)
                return;

            if (ResourceOn)
            {
                if (sustainerResourceOn == null || sustainerResourceOn.Ended)
                    sustainerResourceOn = Props.soundAmbientReceivingResource.TrySpawnSustainer(SoundInfo.InMap(parent));
                sustainerResourceOn.Maintain();
            }
            else
            {
                if (sustainerResourceOn == null)
                    return;
                sustainerResourceOn.End();
                sustainerResourceOn = null;
            }
        }

        /// <summary>
        /// Was used => consume
        /// </summary>
        public void Notify_UsedThisTick()
        {
            if (UsedLastTick)
                return;

            UsedLastTick = true;
            PipeNet.receiversDirty = true;
        }

        /// <summary>
        /// Treat signals
        /// </summary>
        /// <param name="signal"></param>
        public override void ReceiveCompSignal(string signal)
        {
            PipeSystemDebug.Message($"Received comp signal: {signal}");
            switch (signal)
            {
                case "FlickedOff":
                case "ScheduledOff":
                case "Breakdown":
                case "RanOutOfFuel":
                case "PowerTurnedOff":
                    ResourceOn = false;
                    return;
            }
            if (signal == OffSignal)
                ResourceOn = false;

            if (Consumption != 0f)
            {
                PipeNet.receiversDirty = true;
                PipeNet.producersDirty = true;
            }
        }

        /// <summary>
        /// Check for multiple comps (flickable, schedule, breakdownable, refuelable, powertrader) to determine if
        /// resource production can be enabled.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanBeOn()
        {
            return (compFlickable == null || compFlickable.SwitchIsOn)
                && (compSchedule == null || compSchedule.Allowed)
                && (compBreakdownable == null || !compBreakdownable.BrokenDown)
                && (compRefuelable == null || compRefuelable.HasFuel)
                && (compPowerTrader == null || compPowerTrader.PowerOn);
        }

        /// <summary>
        /// More comp info for debug.
        /// </summary>
        public string DebugString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(parent.LabelCap + " CompResourceTrader:");
                sb.AppendLine("   ResourceOn: " + ResourceOn.ToString());
                sb.AppendLine("   consumption: " + Consumption);
                return sb.ToString().Trim();
            }
        }
    }
}