using HarmonyLib;
using RimWorld;

namespace VFECore.Abilities;

[HarmonyPatch(typeof(PawnFlyer), "RecomputePosition")]
public static class PawnFlyer_RecomputePosition_Patch
{
    // Replace the vanilla method if the flyer is AbilityPawnFlyer and returned true.
    public static bool Prefix(PawnFlyer __instance) => __instance is not AbilityPawnFlyer flyer || !flyer.CustomRecomputePosition();
}