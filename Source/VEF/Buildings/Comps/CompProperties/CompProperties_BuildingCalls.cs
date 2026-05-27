using Verse;
using System.Collections.Generic;

namespace VEF.Buildings
{
    public class CompProperties_BuildingCalls : CompProperties
    {
        public IntRange interval;
        public List<SoundDef> soundDefs;

        public CompProperties_BuildingCalls()
        {
            this.compClass = typeof(CompBuildingCalls);
        }
    }
}