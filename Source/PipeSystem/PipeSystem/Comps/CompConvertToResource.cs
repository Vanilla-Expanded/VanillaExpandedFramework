using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public class CompConvertToResource : CompResource
    {
        private CompBreakdownable compBreakdownable;
        private CompPowerTrader compPowerTrader;
        private CompFlickable compFlickable;
        public new CompProperties_ConvertThingToResource Props => (CompProperties_ConvertThingToResource)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            compBreakdownable = parent.GetComp<CompBreakdownable>();
            compPowerTrader = parent.GetComp<CompPowerTrader>();
            compFlickable = parent.GetComp<CompFlickable>();
        }

        public bool CanInputNow
        {
            get
            {
                return (compBreakdownable == null || !compBreakdownable.BrokenDown)
                       && (compPowerTrader == null || compPowerTrader.PowerOn)
                       && (compFlickable == null || compFlickable.SwitchIsOn);
            }
        }

        public Thing HeldThing
        {
            get
            {
                List<Thing> thingList = parent.Map.thingGrid.ThingsListAt(parent.Position);

                if (Props.category != null)
                {
                    List<ThingDef> categoryList = Props.category.childThingDefs;
                    for (int j = 0; j < categoryList.Count; j++)
                    {
                        for (int i = 0; i < thingList.Count; i++)
                        {
                            if (thingList[i].def == categoryList[j])
                            {
                                //Log.Message("Eating " + thingList[i].ToString() + " from category " + Props.category.ToString());
                                return thingList[i];
                            }
                        }
                    }
                }
                else if (Props.thing != null)
                {
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i].def == Props.thing)
                            return thingList[i];
                    }
                }
                return null;
            }
        }

        public int MaxCanInput => (int)PipeNet.AvailableCapacity;

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(250))
                CompTickRare();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            var heldThing = HeldThing;
            if (CanInputNow && heldThing != null)
            {
                var resourceToAdd = heldThing.stackCount / Props.ratio;
                PipeNet.DistributeAmongStorage(resourceToAdd);
                heldThing.DeSpawn();
            }
        }
    }
}
