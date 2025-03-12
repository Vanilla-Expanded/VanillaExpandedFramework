using HarmonyLib;
using RimWorld.Planet;

namespace VFECore
{
    [HarmonyPatch(typeof(Caravan_PathFollower), "StartPath")]
    public static class Caravan_PathFollower_StartPath_Patch
    {
        public static void Postfix(Caravan_PathFollower __instance, int destTile)
        {
            if (Caravan_PathFollower_ExposeData_Patch.caravansToFollow.TryGetValue(__instance, out var movingBase)
                && destTile != movingBase.destination.Tile)
            {
                Caravan_PathFollower_ExposeData_Patch.caravansToFollow.Remove(__instance);
            }
        }
    }
}