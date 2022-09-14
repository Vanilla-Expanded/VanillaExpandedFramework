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
    [HarmonyPatch(typeof(ThingDefGenerator_Corpses), nameof(ThingDefGenerator_Corpses.ImpliedCorpseDefs))]
    public static class ThingDefGenerator_Corpses_ImpliedCorpseDefs_Patch
    {
        public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> __result)
        {
            foreach (var thingDef in __result)
            {
                if (!SkipDef(thingDef))
                {
                    yield return thingDef;
                }
            }
        }

        public static bool SkipDef(ThingDef thingDef)
        {
            if (thingDef.ingestible?.sourceDef != null)
            {
                var extension = thingDef.ingestible.sourceDef.GetModExtension<ThingDefExtension>();
                if (extension != null && extension.destroyCorpse)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
