using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace VEF.Genes;

[HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell", typeof(Pawn), typeof(IntVec3))]
[HarmonyPatchCategory(VEF_Mod.LateHarmonyPatchCategory)]
public class VanillaExpandedFramework_Pawn_PathFollower_CostToMoveIntoCell
{
    public static bool Prepare(MethodBase baseMethod)
    {
        // Always allow after first pass
        if (baseMethod != null)
            return true;

        // Only apply the patch if there's any GeneDef with GeneExtension,
        // moveSpeedFactorByTerrainTag isn't null/empty, and there's at least
        // a single terrain that has its terrain tag.
        foreach (var def in DefDatabase<GeneDef>.AllDefs)
        {
            var extension = def.GetModExtension<GeneExtension>();
            if (extension != null && !extension.moveSpeedFactorByTerrainTag.NullOrEmpty())
            {
                foreach (var tag in extension.moveSpeedFactorByTerrainTag.Keys)
                {
                    // Check if there's a TerrainDef that has a tag matching our tags
                    if (DefDatabase<TerrainDef>.AllDefs.Any(terrain => terrain.tags != null && terrain.tags.Contains(tag)))
                        return true;
                }
            }
        }

        return false;
    }

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
        if (!StaticCollectionsClass.moveSpeedFactorByTerrainTag_gene_pawns.TryGetValue(pawn, out var factors))
            return;

        // Patch should be inserted in a place where terrain.tags is guaranteed not null
        factors.ApplySpeed(terrain.tags, ref speed);
    }
}