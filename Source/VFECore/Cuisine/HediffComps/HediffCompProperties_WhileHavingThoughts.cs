

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Text;

namespace VanillaCookingExpanded
{
    class HediffCompProperties_WhileHavingThoughts : HediffCompProperties
    {

        public List<ThoughtDef> thoughtDefs = new List<ThoughtDef>();
        public List<ThoughtDef> removeThoughtDefs = new List<ThoughtDef>();
        public bool resurrectionEffect = false;

        public HediffCompProperties_WhileHavingThoughts()
        {
            this.compClass = typeof(HediffComp_WhileHavingThoughts);
        }
    }
}
