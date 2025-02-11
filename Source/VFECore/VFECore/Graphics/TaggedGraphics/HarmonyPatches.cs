using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore.TaggedGraphics
{
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        [HarmonyPatch(typeof(FactionGenerator), nameof(FactionGenerator.NewGeneratedFaction))]
        public static void NewGeneratedFactionPostfix(ref Faction __result, FactionGeneratorParms parms)
        {
            foreach (var taggedProps in __result.def.GetModExtensions<TaggedDefProperties>())
            {
                taggedProps.GenerateTags(__result);
            }
        }
    }
}
