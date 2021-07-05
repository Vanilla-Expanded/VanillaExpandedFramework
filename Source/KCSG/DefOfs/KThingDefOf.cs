using RimWorld;
using Verse;

namespace KCSG
{
    [DefOf]
    public static class KThingDefOf
    {
        public static ThingDef KCSG_PowerConduit;

        public static ThingDef KCSG_LongMote_DustPuff;

        static KThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }
    }
}