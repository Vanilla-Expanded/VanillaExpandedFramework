
using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace VanillaCookingExpanded
{
    public class Thought_Hediff_Extension : DefModExtension
    {
        public HediffDef hediffToAffect = null;
        public BodyPartDef partToAffect = null;
        public float percentage = 1f;

        public HediffDef secondHediffToAffect = null;
        public BodyPartDef secondPartToAffect = null;
        public float secondPercentage = 1f;



    }
}
