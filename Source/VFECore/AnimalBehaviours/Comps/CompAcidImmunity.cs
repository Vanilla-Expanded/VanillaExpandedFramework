using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AnimalBehaviours
{
    public class CompAcidImmunity : ThingComp
    {

        //This is just an empty Comp. The new Hediff_AcidBuildup checks if the creature has it, and doesn't apply damage if so


        public CompProperties_AcidImmunity Props
        {
            get
            {
                return (CompProperties_AcidImmunity)this.props;
            }
        }





    }
}

