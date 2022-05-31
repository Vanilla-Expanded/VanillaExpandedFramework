using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AnimalBehaviours
{
    public class CompSwallowWhole : ThingComp
    {

        //This is just an empty Comp that passes the parameters


        public CompProperties_SwallowWhole Props
        {
            get
            {
                return (CompProperties_SwallowWhole)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (Props.filthToMake == null)
            {
                Props.filthToMake = ThingDefOf.Filth_AmnioticFluid;
            }
        }





    }
}