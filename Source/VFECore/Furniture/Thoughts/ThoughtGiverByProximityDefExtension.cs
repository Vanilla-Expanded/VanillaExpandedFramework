using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace VanillaFurnitureExpanded
{
   
    public class ThoughtGiverByProximityDefExtension : DefModExtension
    {
        public ThingDef ThingToGiveThought = null;
        public float DistanceToGiveThought = 15f;
    }

}
