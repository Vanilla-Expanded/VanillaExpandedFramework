using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Things;

[HarmonyPatch(typeof(Building_TurretFoam), nameof(Building_TurretFoam.TryFindNewTarget))]
[HarmonyPatchCategory(VEF_HarmonyCategories.LateHarmonyPatchCategory)]
public static class VanillaExpandedFramework_Building_TurretFoam_Patch
{
    private static bool Prepare() => CustomFireDef.AnyFireAffectedByVanillaExtinguishDamage;

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instr, generator);

        VanillaExpandedFramework_CompFirefoamPack_ChanceToUse_Patch.InsertCustomFireToCondition(matcher);

        return matcher.Instructions();
    }
}