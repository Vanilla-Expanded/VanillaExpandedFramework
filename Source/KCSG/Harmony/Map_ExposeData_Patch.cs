using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    [HarmonyPatch(typeof(Map), "ExposeData")]
    public static class Map_ExposeData_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Map __instance)
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<Thing> kcsgSkyfallers = __instance.listerThings.AllThings.FindAll(t => t is KCSG_Skyfaller);
                if (kcsgSkyfallers.Count > 0)
                {
                    foreach (Thing faller in kcsgSkyfallers)
                    {
                        if (faller is KCSG_Skyfaller skyfaller && skyfaller != null)
                        {
                            skyfaller.SaveImpact();
                        }
                    }
                }
            }
        }
    }
}