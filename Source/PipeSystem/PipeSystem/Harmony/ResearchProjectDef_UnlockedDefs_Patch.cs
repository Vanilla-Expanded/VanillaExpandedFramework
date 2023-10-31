using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace PipeSystem
{
    /// <summary>
    /// Add processDef unlocked by ResearchProjectDef
    /// </summary>
    [HarmonyPatch(typeof(ResearchProjectDef))]
    [HarmonyPatch("UnlockedDefs", MethodType.Getter)]
    public static class ResearchProjectDef_UnlockedDefs_Patch
    {
        public static void Postfix(ref ResearchProjectDef __instance, ref List<Def> __result)
        {
            var research = __instance;
            var defs = DefDatabase<ProcessDef>.AllDefs
                .Where(p => p.researchPrerequisites != null && p.researchPrerequisites.Any(pr => pr == research))
                .Distinct()
                .ToList();

            if (defs != null)
            {
                if (__result == null)
                {
                    __result = new List<Def>(defs);
                }
                else
                {
                    for (int i = 0; i < defs.Count; i++)
                    {
                        var def = defs[i];
                        if (!__result.Contains(def)) __result.Add(def);
                    }
                }
            }
        }
    }
}
