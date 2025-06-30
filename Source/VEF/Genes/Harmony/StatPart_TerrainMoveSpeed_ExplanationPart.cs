using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Genes;

[HarmonyPatch(typeof(StatPart_TerrainMoveSpeed), nameof(StatPart_TerrainMoveSpeed.ExplanationPart))]
[HarmonyPatchCategory(VEF_HarmonyCategories.MoveSpeedFactorByTerrainTagCategory)]
public static class VanillaExpandedFramework_StatPart_TerrainMoveSpeed_ExplanationPart
{
    private static readonly Dictionary<string, float> totalSpeed = [];
    private static readonly HashSet<(string, string)> usedTags = [];

    public static bool Prefix(StatRequest req, out string __result)
    {
        if (req.Thing is not Pawn pawn)
        {
            __result = null;
            return false;
        }

        try
        {
            FillSpeedFactorsData(pawn);

            if (totalSpeed.Count == 0)
            {
                __result = null;
                return false;
            }
        
            var builder = new StringBuilder();

            // Display in the same style as original StatReport_TerrainSpeedMultiplier
            foreach (var (terrainTag, speedFactor) in totalSpeed.OrderBy(x => x.Key))
            {
                if (builder.Length > 0)
                    builder.AppendLine();

                var taggedString = ("TerrainTag" + terrainTag).Translate();
                builder.AppendLine("StatsReport_TerrainSpeedMultiplier".Translate(taggedString) + ": x" + speedFactor.ToStringPercent());
            }

            __result = builder.ToString();
            return false;
        }
        finally
        {
            // Cleanup the collections once done with them so no data bleeds over to the next pawn.
            // Nothing should be accessing those collections, so there's no need to clean beforehand.
            totalSpeed.Clear();
            usedTags.Clear();
        }
    }

    private static void FillSpeedFactorsData(Pawn pawn)
    {
        // Add vanilla PawnKindDef's moveSpeedFactorByTerrainTag
        if (pawn.kindDef?.moveSpeedFactorByTerrainTag != null)
        {
            foreach (var (terrainTag, factor) in pawn.kindDef.moveSpeedFactorByTerrainTag)
                AddSpeed(factor, terrainTag);
        }

        // Don't use StaticCollectionsClass in case the pawn doesn't have the effects applied yet.
        // I'm not sure if that should be possible, but let's be safe here I suppose?

        if (ModsConfig.BiotechActive && pawn.genes?.GenesListForReading != null)
        {
            foreach (var gene in pawn.genes.GenesListForReading)
                AddSpeed(gene.def.GetModExtension<GeneExtension>()?.moveSpeedFactorByTerrainTag);
        }

        if (pawn.health?.hediffSet?.hediffs != null)
        {
            foreach (var hediff in pawn.health.hediffSet.hediffs)
                AddSpeed(hediff.TryGetComp<HediffComp_MoveSpeedFactorByTerrainTag>()?.Props.moveSpeedFactorByTerrainTag);
        }
    }

    private static void AddSpeed(Dictionary<string, List<MoveSpeedFactor>> moveSpeedFactorByTerrainTag)
    {
        if (moveSpeedFactorByTerrainTag.NullOrEmpty())
            return;

        foreach (var (terrainTag, factors) in moveSpeedFactorByTerrainTag)
        {
            foreach (var factor in factors)
                AddSpeed(factor.moveSpeedFactor, terrainTag, factor.tag);
        }
    }

    private static void AddSpeed(float speedFactor, string terrainTag, string speedFactorTag = null)
    {
        // Check for speed factors with duplicate tags applied to the same terrain tag
        if (speedFactorTag != null && !usedTags.Add((terrainTag, speedFactorTag)))
        {
            if (totalSpeed.TryGetValue(terrainTag, out var speed))
                speed *= speedFactor;
            else
                speed = speedFactor;

            totalSpeed[terrainTag] = speed;
        }
    }
}