using RimWorld;
using HarmonyLib;
using System.Collections.Generic;

namespace VFECore
{
    [HarmonyPatch(typeof(Building_CommsConsole), "GetCommTargets")]
    public static class Building_CommsConsole_GetCommTargets_Patch
    {
        public static IEnumerable<ICommunicable> Postfix(IEnumerable<ICommunicable> __result)
        {
            foreach (var r in __result)
            {
                if (r is Faction faction)
                {
                    var extension = faction.def.GetModExtension<FactionDefExtension>();
                    if (extension != null && extension.excludeFromCommConsole)
                    {
                        continue;
                    }
                }
                yield return r;
            }
        }
    }
}
