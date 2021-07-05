using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public static class UIUtilityData
    {
        public static Dictionary<FactionDef, int> factionCounts = new Dictionary<FactionDef, int>();
    }

    [HarmonyPatch(typeof(WorldFactionsUIUtility), "DoWindowContents")]
    public static class Patch_WorldFactionsUIUtility
    {
        [HarmonyPostfix]
        public static void Postfix(ref Dictionary<FactionDef, int> factionCounts)
        {
            foreach (var item in factionCounts)
            {
                UIUtilityData.factionCounts.SetOrAdd(item.Key, item.Value);
            }
        }
    }
}