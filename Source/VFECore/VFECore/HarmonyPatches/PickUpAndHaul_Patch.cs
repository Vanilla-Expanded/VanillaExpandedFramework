using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFEM.HarmonyPatches
{
    [HarmonyPatch]
    public static class PickUpAndHaul_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            Type t = AccessTools.TypeByName("PickUpAndHaul.WorkGiver_HaulToInventory");
            if(t != null)
            {
                return AccessTools.Method(t, nameof(WorkGiver_Scanner.HasJobOnThing));
            }
            return AccessTools.Method(typeof(PickUpAndHaul_Patch), nameof(Throwaway));
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            __result &= pawn.RaceProps.Humanlike;
        }

        public static bool Throwaway(Pawn pawn)
        {
            return true;
        }
    }
}
