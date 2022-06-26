using Verse;
using System.Collections.Generic;

namespace VanillaPlantsExpanded
{

    public class DualCropExtension : DefModExtension
    {     
        public ThingDef secondaryOutput;
        public int outPutAmount;
        public bool randomOutput = false;
        public List<ThingDef> randomSecondaryOutput;
    }

}
