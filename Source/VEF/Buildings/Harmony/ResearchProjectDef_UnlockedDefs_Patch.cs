using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.Buildings
{
    [HarmonyPatch(typeof(ResearchProjectDef), nameof(ResearchProjectDef.UnlockedDefs), MethodType.Getter)]
    public static class VanillaExpandedFramework_ResearchProjectDef_UnlockedDefs_Patch
    {
        private static HashSet<BuildableDef> cachedHiddenDesignators;

        public static void Postfix(ref List<Def> __result)
        {
            if (cachedHiddenDesignators is null)
            {
                cachedHiddenDesignators = new HashSet<BuildableDef>();
                foreach (HiddenDesignatorsDef hiddenDef in DefDatabase<HiddenDesignatorsDef>.AllDefs)
                {
                    foreach (BuildableDef hiddenBuildable in hiddenDef.hiddenDesignators)
                    {
                        cachedHiddenDesignators.Add(hiddenBuildable);
                    }
                }
            }
            __result.RemoveAll(d => d is BuildableDef bd && cachedHiddenDesignators.Contains(bd));
        }
    }
}