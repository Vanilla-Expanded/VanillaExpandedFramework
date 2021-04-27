using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

namespace AnimalBehaviours
{
    public class CompEatWeirdFood : ThingComp
    {

        public int currentFeedings = 0;


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.currentFeedings, "currentFeedings", 0, false);
        }


        public CompProperties_EatWeirdFood Props
        {
            get
            {
                return (CompProperties_EatWeirdFood)this.props;
            }
        }
    }
}