using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using UnityEngine;
using System.Collections;

namespace AnimalBehaviours
{
    public class CompInfecter : ThingComp
    {

        public CompProperties_Infecter Props
        {
            get
            {
                return (CompProperties_Infecter)this.props;
            }
        }

        public int GetChance
        {
            get
            {
                return this.Props.infectionChance;
            }
        }
    }
}
