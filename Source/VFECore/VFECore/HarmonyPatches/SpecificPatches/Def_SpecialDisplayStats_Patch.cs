using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Def), "SpecialDisplayStats")]
    public static class Def_SpecialDisplayStats_Patch
    {
        public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result)
        {
            foreach (var entry in __result)
            {
                if (entry.category == StatCategoryDefOf.Source && VFEGlobal.settings.disableModSourceReport)
                {
                    continue;
                }
                else
                {
                    yield return entry;
                }
            }
        }
    }
}
