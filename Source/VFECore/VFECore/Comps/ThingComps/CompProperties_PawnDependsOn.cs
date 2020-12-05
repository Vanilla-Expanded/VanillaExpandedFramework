using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
    public class CompProperties_PawnDependsOn : CompProperties
    {
        public PawnKindDef pawnToSpawn;

        public CompProperties_PawnDependsOn()
        {
            this.compClass = typeof(CompPawnDependsOn);
        }
    }
}
