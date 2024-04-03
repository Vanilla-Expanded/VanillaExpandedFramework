using HarmonyLib;
using RimWorld;
using System.Linq;
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
                        var rulepack = Traverse.Create(VFEDefOf.VEF_Description_Schematic_Defaults).Field<RulePack>("rulePack").Value;
                        def.generalRules = rulepack;
                    }
                }
            }

            var veTab = DefDatabase<ResearchTabDef>.GetNamedSilentFail("VanillaExpanded");
            if (veTab != null)
            {
                ThingDefOf.Schematic.GetCompProperties<CompProperties_Book>().doers.OfType<BookOutcomeProperties_GainResearch>().FirstOrDefault()?.tabs.Add
                    (new BookOutcomeProperties_GainResearch.BookTabItem
                    {
                        tab = veTab
                    });
            }
        }
    }
}