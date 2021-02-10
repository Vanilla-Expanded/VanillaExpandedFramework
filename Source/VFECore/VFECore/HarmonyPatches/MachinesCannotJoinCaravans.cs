using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFEMech;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(CaravanFormingUtility), "AllSendablePawns")]
    public static class MachinesCannotJoinCaravans
    {
        public static void Postfix(ref List<Pawn> __result)
        {
            __result = __result.FindAll(pawn => pawn.def.thingClass != typeof(Machine));
        }
    }
}
