
using RimWorld;
using Verse;

namespace VanillaFurnitureExpanded
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
