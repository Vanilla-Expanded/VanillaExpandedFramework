using HarmonyLib;
using RimWorld.Planet;
using System;


namespace VEF.Planet
{
    [HarmonyPatch(typeof(Caravan_PathFollower), "PatherTickInterval")]
    public static class VanillaExpandedFramework_Caravan_PathFollower_PatherTickInterval_Patch
    {
        public static void Prefix(Caravan_PathFollower __instance)
        {
            if (VanillaExpandedFramework_Caravan_PathFollower_ExposeData_Patch.caravansToFollow.TryGetValue(__instance, out var caravanToFollow))
            {
                if (caravanToFollow.destination != null)
                {
                    if (__instance.Destination != caravanToFollow.destination.Tile)
                    {
                        var action = Activator.CreateInstance(caravanToFollow.arrivalActionType) as CaravanArrivalAction_MovingBase;
                        action.movingBase = caravanToFollow.destination;
                        __instance.StartPath(caravanToFollow.destination.Tile, action, repathImmediately: true);
                    }
                }
                else
                {
                    VanillaExpandedFramework_Caravan_PathFollower_ExposeData_Patch.caravansToFollow.Remove(__instance);
                }
            }
        }
    }
}