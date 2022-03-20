using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace VFECore
{
    public class TerrainComp_PowerTrader : TerrainComp
    {
        public readonly int tickInterval = 50;

        private float powerOutputInt;

        private CompPowerTraderFloor connectParentInt;

        public CompPowerTraderFloor ConnectParent
        {
            get
            {
                return connectParentInt;
            }
            set
            {
                connectParentInt?.Notify_TerrainCompRemoved(this);
                connectParentInt = value;
                value?.ReceiveTerrainComp(this);
            }
        }

        public TerrainCompProperties_PowerTrader Props { get { return (TerrainCompProperties_PowerTrader)props; } }

        public bool PowerOn { get { return ConnectParent != null && ConnectParent.PowerOn; } }

        public virtual float PowerOutput
        {
            get
            {
                return powerOutputInt;
            }
            set
            {
                powerOutputInt = value;
                ConnectParent?.UpdatePowerOutput();
            }
        }

        public override void CompUpdate()
        {
            if (!PowerOn && !Props.ignoreNeedsPower)
            {
                ActiveTerrainUtility.RenderPulsingNeedsPowerOverlay(parent.Position);
            }
        }

        bool curSignal;
        public override void CompTick()
        {
            if (PowerOn != curSignal)
            {
                parent.BroadcastCompSignal(PowerOn ? CompSignals.PowerTurnedOn : CompSignals.PowerTurnedOff);
                curSignal = PowerOn;
            }
            if (!PowerOn && Find.TickManager.TicksGame % tickInterval == this.HashCodeToMod(tickInterval))
            {
                var comp = ActiveTerrainUtility.TryFindNearestPowerConduitFloor(parent.Position, parent.Map);
                if (comp != null)
                {
                    ConnectParent = comp;
                }
            }
        }

        public override void Initialize(TerrainCompProperties props)
        {
            base.Initialize(props);
            powerOutputInt = -Props.basePowerConsumption;
        }

        public override void PostRemove()
        {
            ConnectParent = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref curSignal, "curCompSignal");
            Thing thing = null;
            if (Scribe.mode == LoadSaveMode.Saving && ConnectParent != null)
            {
                thing = ConnectParent.parent;
            }
            Scribe_References.Look(ref thing, "parentThing");
            if (thing != null)
            {
                ConnectParent = ((ThingWithComps)thing).GetComp<CompPowerTraderFloor>();
            }
        }
    }
    public class TerrainComp_HeatPushPowered : TerrainComp_HeatPush
    {
        protected override bool ShouldPushHeat { get { return parent.GetComp<TerrainComp_PowerTrader>() == null || parent.GetComp<TerrainComp_PowerTrader>().PowerOn; } }
    }
    public class TerrainComp_SelfCleanPowered : TerrainComp_SelfClean
    {
        protected override bool CanClean { get { return parent.GetComp<TerrainComp_PowerTrader>() == null || parent.GetComp<TerrainComp_PowerTrader>().PowerOn; } }
    }
}
