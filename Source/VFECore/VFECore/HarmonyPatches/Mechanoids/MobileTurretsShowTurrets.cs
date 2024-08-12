using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(PawnRenderUtility), "CarryWeaponOpenly")]
    public static class MobileTurretsShowTurrets
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (pawn != null && CompMachine.cachedMachinesPawns.TryGetValue(pawn, out CompMachine value) && value !=null 
                && (value.turretAttached != null || value.Props.violent==true))
            {
                __result = true;
            }
        }
    }
}
