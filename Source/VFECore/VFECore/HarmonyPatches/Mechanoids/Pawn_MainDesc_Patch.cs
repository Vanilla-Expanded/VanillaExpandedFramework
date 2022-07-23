using HarmonyLib;
using Verse;

namespace VFE.Mechanoids.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.MainDesc))]
    public static class Pawn_MainDesc_Patch 
    {
        public static void Postfix(ref string __result)
        {
            var substringToRemove = " ";
            int index = __result.IndexOf(substringToRemove);
            if (index == 0)
            {
                __result = __result.Remove(index, substringToRemove.Length).CapitalizeFirst();
            }
        }
    }
}
