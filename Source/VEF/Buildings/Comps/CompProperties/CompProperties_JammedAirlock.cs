
using RimWorld;
using Verse;

namespace VEF.Buildings
{
    public class CompProperties_JammedAirlock : CompProperties_Interactable
    {
        public ThingDef doorToConvertTo;
        public string stringExtra;

        public CompProperties_JammedAirlock()
        {
            compClass = typeof(CompJammedAirlock);
        }
    }
}
