using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public static class Patch_ResearchProjectDef
    {

        [HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.HasTag))]
        public static class HasTag
        {

            public static void Postfix(ResearchProjectDef __instance, ResearchProjectTagDef tag, ref bool __result)
            {
                // 'Greylist' certain research projects based on start tags - just saves XML work
                if (__result)
                {
                    var researchProjectDefExtension = ResearchProjectDefExtension.Get(__instance);
                    if (!researchProjectDefExtension.greylistedTags.NullOrEmpty() && researchProjectDefExtension.greylistedTags.Contains(tag))
                        __result = false;
                        
                }
            }

        }

    }

}
