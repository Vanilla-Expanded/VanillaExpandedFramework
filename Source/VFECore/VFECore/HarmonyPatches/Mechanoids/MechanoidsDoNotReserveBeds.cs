using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(JobDriver_LayDown), "TryMakePreToilReservations")]
    class MechanoidsDoNotReserveBeds
    {
        public static bool Prefix(JobDriver_LayDown __instance)
        {
            if (__instance.pawn != null && __instance.pawn.RaceProps.IsMechanoid)
                return false;
            return true;
        }

        public static void Postfix(JobDriver_LayDown __instance, ref bool __result)
        {
            if (__instance.pawn != null && __instance.pawn.RaceProps.IsMechanoid)
                __result = true;
        }
    }
}
