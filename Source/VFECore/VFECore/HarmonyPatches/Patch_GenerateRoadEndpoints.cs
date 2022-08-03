using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using System.Reflection;

namespace VFECore
{
    [HarmonyPatch]
    public static class Patch_GenerateRoadEndpoints
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return typeof(WorldGenStep_Roads).GetNestedTypes(AccessTools.all)
            .SelectMany(x => x.GetMethods(AccessTools.all))
            .FirstOrDefault(x => x.Name.Contains("<GenerateRoadEndpoints>") && x.ReturnType == typeof(bool));
        }

        public static void Postfix(ref bool __result, WorldObject wo)
        {
            if (wo is Settlement settlement && settlement.Faction != null)
            {
                var extension = settlement.Faction.def.GetModExtension<FactionDefExtension>();
                if (extension != null && extension.neverConnectToRoads)
                {
                    __result = false;
                }
            }
        }
    }
}