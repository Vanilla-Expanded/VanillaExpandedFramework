using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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





    }
}