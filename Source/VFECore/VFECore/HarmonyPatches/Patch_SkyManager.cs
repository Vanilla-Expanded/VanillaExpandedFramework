using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    
    internal static class Patch_SkyManager
    {
        [HarmonyPatch(typeof(SkyManager), "CurrentSkyTarget")]
        public class CurrentSkyTarget
        {
            [HarmonyPostfix]
            private static void PostFix(ref Map ___map, ref SkyTarget __result)
            {
                List<Thing> list = ___map.listerThings.AllThings.FindAll(t => t.TryGetComp<CompAffectSkyWithToggle>() != null);
                for (int k = 0; k < list.Count; k++)
                {
                    CompAffectSkyWithToggle affectSkyWithToggle = list[k].TryGetComp<CompAffectSkyWithToggle>();
                    if (affectSkyWithToggle.shouldAffectSky)
                    {
                        Log.Message("affecting sky");
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
}