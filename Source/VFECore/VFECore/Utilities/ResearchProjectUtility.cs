using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace VFECore
{
    public static class ResearchProjectUtility
    {
        public static void AutoAssignRules()
        {
            foreach (var def in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                if (def.tab != ResearchTabDefOf.Anomaly)
                {
                    if (def.generalRules == null)
                    {
                        def.generalRules = Traverse.Create(VFEDefOf.VEF_Description_Schematic_Defaults).Field<RulePack>("rulePack").Value;
                    }
                }
            }
        }
    }
}