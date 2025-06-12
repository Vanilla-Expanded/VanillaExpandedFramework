using HarmonyLib;
using RimWorld.Planet;

namespace VEF.Planet
{
    [HarmonyPatch(typeof(Caravan_PathFollower), "StartPath")]
    public static class Caravan_PathFollower_StartPath_Patch
    {
        public static void Postfix(Caravan_PathFollower __instance, PlanetTile destTile)
        {
            if (VanillaExpandedFramework_Caravan_PathFollower_ExposeData_Patch.caravansToFollow.TryGetValue(__instance, out var movingBase)
                && destTile != movingBase.destination.Tile)
            {
                VanillaExpandedFramework_Caravan_PathFollower_ExposeData_Patch.caravansToFollow.Remove(__instance);
            }
        }
    }
}