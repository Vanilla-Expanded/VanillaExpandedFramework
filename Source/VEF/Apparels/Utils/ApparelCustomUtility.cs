using System.Linq;
using Verse;

namespace VEF.Apparels
{
    public static class ApparelCustomUtility
    {
        public static bool WearsApparel(this Pawn pawn, ThingDef thingDef)
        {
            if (pawn.apparel?.WornApparel != null)
            {
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    if (apparel.def == thingDef)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}