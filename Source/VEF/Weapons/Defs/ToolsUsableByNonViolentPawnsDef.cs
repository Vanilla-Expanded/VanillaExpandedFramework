using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VEF.Weapons
{
    public class ToolsUsableByNonViolentPawnsDef : Def
    {
        //A list of weapon defNames
        public List<ThingDef> toolsUsableByNonViolentPawns;
    }
}