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

    [HarmonyPatch(typeof(ITab_Pawn_Gear), "DrawThingRow")]
    public static class ITab_Pawn_Gear_Patch
    {
        public static bool drawingThingRow;
        public static void Prefix()
        {
            drawingThingRow = true;
        }
        public static void Postfix()
        {
            drawingThingRow = false;
        }
    }

    [HarmonyPatch(typeof(ITab_Pawn_Gear), "CanControl", MethodType.Getter)]
    public static class ITab_Pawn_Gear_CanControl
    {
        public delegate Pawn PawnToShowInfoAbout(ITab_Pawn_Gear __instance);
        public static readonly PawnToShowInfoAbout pawnToShowInfoAbout = AccessTools.MethodDelegate<PawnToShowInfoAbout>
            (AccessTools.Method(typeof(ITab_Pawn_Gear), "get_SelPawnForGear"));
        public static void Postfix(ITab_Pawn_Gear __instance, ref bool __result)
        {
            if (ITab_Pawn_Gear_Patch.drawingThingRow)
            {
                Pawn pawn = pawnToShowInfoAbout(__instance);
                var comp = pawn.GetComp<CompMachine>();
                if (comp != null && !comp.Props.canPickupWeapons)
                {
                    __result = false;
                }
            }

        }
    }
}
