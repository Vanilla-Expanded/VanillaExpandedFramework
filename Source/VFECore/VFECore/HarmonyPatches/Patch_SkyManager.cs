using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(SkyManager))]
    internal class Patch_SkyManager
    {
        [HarmonyPostfix]
        [HarmonyPatch("CurrentSkyTarget")]
        private static void PostFix(ref Map ___map, ref SkyTarget __result)
        {
            List<Thing> list = ___map.listerThings.AllThings.FindAll(t => t.TryGetComp<CompAffectSkyWithToggle>() != null);
            for (int k = 0; k < list.Count; k++)
            {
                CompAffectSkyWithToggle affectSkyWithToggle = list[k].TryGetComp<CompAffectSkyWithToggle>();
                if (affectSkyWithToggle.LerpFactor > 0f)
                {
                    if (affectSkyWithToggle.Props.lerpDarken)
                    {
                        __result = SkyTarget.LerpDarken(__result, affectSkyWithToggle.SkyTarget, affectSkyWithToggle.LerpFactor);
                    }
                    else
                    {
                        __result = SkyTarget.Lerp(__result, affectSkyWithToggle.SkyTarget, affectSkyWithToggle.LerpFactor);
                    }
                }
            }
        }
    }
}