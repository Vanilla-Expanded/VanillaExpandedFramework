using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Reflection;
using VFECore;
using VFEMech;
using UnityEngine;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(ITab_Pawn_Character))]
    [HarmonyPatch("IsVisible", MethodType.Getter)]
    public static class NoBioForMachines
    {
        public delegate Pawn PawnToShowInfoAbout(ITab_Pawn_Character __instance);
        public static readonly PawnToShowInfoAbout pawnToShowInfoAbout = AccessTools.MethodDelegate<PawnToShowInfoAbout>
            (AccessTools.Method(typeof(ITab_Pawn_Character), "get_PawnToShowInfoAbout"));
        public static void Postfix(ITab_Pawn_Character __instance, ref bool __result)
        {
            Pawn pawn = pawnToShowInfoAbout(__instance);
            if (pawn is Machine)
                __result = false;
        }
    }
}
