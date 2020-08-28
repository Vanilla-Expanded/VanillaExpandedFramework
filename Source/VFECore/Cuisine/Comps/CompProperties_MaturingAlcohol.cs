using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaCookingExpanded
{

    //This code is mostly copied from CompProperties_Rotable, with a few tweaks. Instead of destroying an item when
    //it rots, this code will swap the item for a different one. For example, it is used with Vanilla Brewing Expanded Ambrosia mush 
    //to turn it into Ambrandy must

    public class CompProperties_MaturingAlcohol : CompProperties
    {
        public int TicksToRotStart
        {
            get
            {
                return Mathf.RoundToInt(this.daysToRotStart * 60000f);
            }
        }

        public int TicksToDessicated
        {
            get
            {
                return Mathf.RoundToInt(this.daysToDessicated * 60000f);
            }
        }

        public CompProperties_MaturingAlcohol()
        {
            this.compClass = typeof(CompMaturingAlcohol);
        }

        public CompProperties_MaturingAlcohol(float daysToRotStart)
        {
            this.daysToRotStart = daysToRotStart;
        }



        public float daysToRotStart = 2f;

        public bool rotDestroys;

        public float rotDamagePerDay = 40f;

        public float daysToDessicated = 999f;

        public float dessicatedDamagePerDay;

        public bool disableIfHatcher;

        public string maturingString;

        public string maturingProperly;

        public string maturingSlowly;

        public string maturingStopped;

        public string thingToTransformTo;

    }
}