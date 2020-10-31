

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Text;

namespace VanillaCookingExpanded
{
    class HediffCompProperties_WhileHavingThoughts : HediffCompProperties
    {

        //A comp class that keeps a hediff active while a thought (or thoughts) is active on the pawn

        //It also checks if other given thoughts are active on the pawn, and removes them as needed

        public List<ThoughtDef> thoughtDefs = new List<ThoughtDef>();
        public List<ThoughtDef> removeThoughtDefs = new List<ThoughtDef>();
        public string hediffReduction = "";
        public float reductionAmount = 0f;
        public bool resurrectionEffect = false;

        public HediffCompProperties_WhileHavingThoughts()
        {
            this.compClass = typeof(HediffComp_WhileHavingThoughts);
        }
    }
}
