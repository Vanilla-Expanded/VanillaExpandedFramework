using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace AnimalBehaviours
{



    public static class Patch_ThingDef
    {

        [HarmonyPatch(typeof(ThingDef))]
        [HarmonyPatch(nameof(ThingDef.SpecialDisplayStats))]
        public static class VanillaExpandedFramework_ThingDef_SpecialDisplayStats_Patch
        {

            public static void Postfix(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result)
            {

                if(__instance.GetModExtension<AnimalStatExtension>() != null && !__instance.IsCorpse)
                {
                    AnimalStatExtension extension = __instance.GetModExtension<AnimalStatExtension>();
                    if (extension.statToAdd != null) {
                        foreach (string stat in extension.statToAdd)
                        {
                            __result = __result.AddItem(new StatDrawEntry(StatCategoryDefOf.BasicsPawn, stat.Translate(), extension.statValues[extension.statToAdd.IndexOf(stat)].Translate(), extension.statDescriptions[extension.statToAdd.IndexOf(stat)].Translate()
                            , 1));
                        }
                    }
                    
                }
                
            }

        }

    }



}
