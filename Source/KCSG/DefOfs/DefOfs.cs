using RimWorld;
using Verse;

namespace KCSG
{
    [DefOf]
    public static class DefOfs
    {
        public static ThingDef KCSG_PowerConduit;

        public static ThingDef KCSG_LongMote_DustPuff;

        public static ThingDef Limestone;

        public static ThingDef Slate;

        public static ThingDef Marble;


        static DefOfs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }
    }
}