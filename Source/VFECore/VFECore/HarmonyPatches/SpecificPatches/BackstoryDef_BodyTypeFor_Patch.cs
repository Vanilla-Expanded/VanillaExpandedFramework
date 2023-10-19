using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(BackstoryDef), "BodyTypeFor")]
    public static class BackstoryDef_BodyTypeFor_Patch
    {
        public static void Postfix(ref BodyTypeDef __result, Gender g)
        {
            if (__result is null)
            {
                if (Rand.Value < 0.5f)
                {
                    __result = BodyTypeDefOf.Thin;
                }
                else if (g == Gender.Female)
                {
                    __result = BodyTypeDefOf.Female;
                }
                else
                {
                    __result = BodyTypeDefOf.Male;
                }
            }
        }
    }
}
