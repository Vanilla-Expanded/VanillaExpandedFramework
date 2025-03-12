using HarmonyLib;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace VFECore
{
    [HarmonyPatch(typeof(Caravan_PathFollower), "ExposeData")]
    public static class Caravan_PathFollower_ExposeData_Patch
    {
        public static Dictionary<Caravan_PathFollower, MovingBaseDestinationAction> caravansToFollow = new Dictionary<Caravan_PathFollower, MovingBaseDestinationAction>();
        public static void Postfix(Caravan_PathFollower __instance)
        {
            caravansToFollow.TryGetValue(__instance, out var caravanToFollow);
            Scribe_Deep.Look(ref caravanToFollow, "caravanToFollow");
            if (caravanToFollow != null)
            {
                caravansToFollow[__instance] = caravanToFollow;
            }
        }
    }
}