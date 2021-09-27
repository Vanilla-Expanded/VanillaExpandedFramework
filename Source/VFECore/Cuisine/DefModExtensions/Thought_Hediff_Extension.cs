
using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace VanillaCookingExpanded
{

    //This extension class feeds some additional XML fields to the Though_Hediff class.
    //Explanation about what it does in that class

    public class Thought_Hediff_Extension : DefModExtension
    {

        //A first hediff to create when the thought is initialized, as well as which body part to affect
        //(and a generic "Body" is an option) as well as how much to increase the hediff
        public HediffDef hediffToAffect = null;
        public BodyPartDef partToAffect = null;
        public float percentage = 1f;

        //The Though_Hediff class can also create a second hediff
        public HediffDef secondHediffToAffect = null;
        public BodyPartDef secondPartToAffect = null;
        public float secondPercentage = 1f;

        //Joy increase from this thought
        public bool increaseJoy = false;
        public float extraJoy = 0;



    }
}
