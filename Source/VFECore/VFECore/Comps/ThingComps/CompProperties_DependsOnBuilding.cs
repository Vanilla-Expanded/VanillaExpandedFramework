using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
    public class CompProperties_DependsOnBuilding : CompProperties
    {

        public CompProperties_DependsOnBuilding()
        {
            this.compClass = typeof(CompPawnDependsOn);
        }
    }
}
