using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Reflection;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(ITab_Pawn_Character))]
    [HarmonyPatch("IsVisible", MethodType.Getter)]
    public static class NoBioForMachines
    {
        static PropertyInfo propertyInfo = typeof(ITab_Pawn_Character).GetProperty("PawnToShowInfoAbout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        public static void Postfix(ITab_Pawn_Character __instance, ref bool __result)
        {
            Pawn pawn= (Pawn)propertyInfo.GetValue(__instance);
            if (pawn.RaceProps.IsMechanoid)
                __result = false;
        }
    }
}
