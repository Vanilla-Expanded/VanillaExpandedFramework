using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VEF.Buildings
{
    public class LootBoxExtension : DefModExtension
    {
        public ThingSetMakerDef thingSetMakerDef;
        public FloatRange totalMarketValueRange = new FloatRange(850, 1000);
        public float? minSingleItemMarketValuePct;
        public bool allowNonStackableDuplicates = true;
        public IntRange countRange = new IntRange(1, 1);


    }
}
