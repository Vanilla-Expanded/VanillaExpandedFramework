using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_SummonOnSpawn : CompProperties
    {

        //A comp class to make an animal "summon" a configurable group of additional animals when spawned.
        //For example a spider spawning with a host of smaller spiders. Similar to base game wild spawns, but
        //with different defs

        public string pawnDef = "Pig";
        public List<int> groupMinMax;
        public bool summonsAreManhunters = true;

        public CompProperties_SummonOnSpawn()
        {
            this.compClass = typeof(CompSummonOnSpawn);
        }
    }
}
