using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
    public class CompDependsOnBuilding : ThingComp
    {
        public Building myBuilding;

        public CompProperties_DependsOnBuilding Props
        {
            get
            {
                return (CompProperties_DependsOnBuilding)this.props;
            }
        }

        public virtual void OnBuildingDestroyed(CompPawnDependsOn compPawnDependsOn)
        {
            //Do something - or just fall over dead
        }

        public override void CompTick()
        {
            base.CompTick();
            if (myBuilding != null && ((Pawn)parent).Dead)
            {
                myBuilding.TryGetComp<CompPawnDependsOn>().OnPawnDestroyed();
                myBuilding.TryGetComp<CompPawnDependsOn>().myPawn = null;
                myBuilding = null;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Building>(ref myBuilding, "myBuilding");
        }
    }
}
