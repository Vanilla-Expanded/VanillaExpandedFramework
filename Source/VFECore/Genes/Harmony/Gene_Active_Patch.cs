using HarmonyLib;
using Verse;

namespace VanillaGenesExpanded
{
    [HarmonyPatch(typeof(Gene), "Active", MethodType.Getter)]
    public static class VanillaGenesExpanded_Gene_Active_Patch
    {
        public static void Postfix(Gene __instance, ref bool __result)
        {
            var extension = __instance.def.GetModExtension<GeneExtension>();
            if (extension != null && extension.forGenderOnly.HasValue && __instance.pawn.gender != extension.forGenderOnly.Value)
            {
                __result = false;
            }
        }
    }
}