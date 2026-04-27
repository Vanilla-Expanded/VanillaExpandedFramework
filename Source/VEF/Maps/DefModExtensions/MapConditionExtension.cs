using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VEF.Maps
{
    public class MapConditionExtension : DefModExtension
    {
        //This value only affects VE Furniture - Power
        public float tideStrengthMultiplier = 1;

        //This value affects VE Furniture - Power and base game watermill generators
        public float watermillStrengthMultiplier = 1;

    }
}
