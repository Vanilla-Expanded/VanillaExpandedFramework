using HarmonyLib;
using RimWorld.Planet;
using System;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Caravan_PathFollower), "PatherTick")]
    public static class Caravan_PathFollower_PatherTick_Patch
    {
        public static void Prefix(Caravan_PathFollower __instance)
        {
            if (Caravan_PathFollower_ExposeData_Patch.caravansToFollow.TryGetValue(__instance, out var caravanToFollow))
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
                    Caravan_PathFollower_ExposeData_Patch.caravansToFollow.Remove(__instance);
                }
            }
        }
    }
}