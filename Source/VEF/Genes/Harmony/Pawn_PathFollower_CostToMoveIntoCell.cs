using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace VEF.Genes;

[HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell", typeof(Pawn), typeof(IntVec3))]
[HarmonyPatchCategory(VEF_HarmonyCategories.MoveSpeedFactorByTerrainTagCategory)]
public class VanillaExpandedFramework_Pawn_PathFollower_CostToMoveIntoCell
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        // Insert right after PawnKindDef.moveSpeedFactorByTerrainTag is applied.
        matcher.MatchEndForward(
                CodeMatch.LoadsLocal(),
                CodeMatch.LoadsField(typeof(TerrainDef).DeclaredField(nameof(TerrainDef.tags))),
                CodeMatch.Branches()
            )
            .Insert(
                // Load in argument for Pawn
                CodeInstruction.LoadArgument(0),
                // Load in local holding TerrainDef
                CodeInstruction.LoadLocal(3),
                // Load in local holding return value using ref (by address)
                CodeInstruction.LoadLocal(0, true),
                // Call our method to modify the speed
                new CodeInstruction(OpCodes.Call,
                    typeof(VanillaExpandedFramework_Pawn_PathFollower_CostToMoveIntoCell).DeclaredMethod(nameof(ModifySpeedFactorForPawn)))
            );

        return matcher.Instructions();
    }

    public static void ModifySpeedFactorForPawn(Pawn pawn, TerrainDef terrain, ref float speed)
    {
        if (terrain == null || terrain.tags.NullOrEmpty())
            return;
        if (!StaticCollectionsClass.moveSpeedFactorByTerrainTag_gene_pawns.TryGetValue(pawn, out var factors))
            return;

        // Patch should be inserted in a place where terrain.tags is guaranteed not null
        factors.ApplySpeed(terrain.tags, ref speed);
    }
}