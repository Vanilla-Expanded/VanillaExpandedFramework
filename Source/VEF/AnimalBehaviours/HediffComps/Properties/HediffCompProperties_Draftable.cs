﻿using Verse;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_Draftable : HediffCompProperties
    {

        //This is equivalent to the CompDraftable class, but just adds things through a hediff so draftability can be added via implant

        public int checkingInterval = 500;
        //If true, adds animals to the non-fleeing mechanic too
        public bool makeNonFleeingToo = false;

        //If true, the animal can equip and fire weapons
        public bool canHandleWeapons = false;

        public HediffCompProperties_Draftable()
        {
            this.compClass = typeof(HediffComp_Draftable);
        }
    }
}

