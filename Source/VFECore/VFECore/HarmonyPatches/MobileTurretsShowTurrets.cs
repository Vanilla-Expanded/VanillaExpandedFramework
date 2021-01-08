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
    [HarmonyPatch(typeof(PawnRenderer), "CarryWeaponOpenly")]
    public static class MobileTurretsShowTurrets
    {

        public static void Postfix(PawnRenderer __instance, ref bool __result)
        {
            if (CompMachine.cachedMachines.TryGetValue(__instance, out CompMachine value) && value.turretAttached != null)
            {
                __result = true;
            }
        }
    }
}
