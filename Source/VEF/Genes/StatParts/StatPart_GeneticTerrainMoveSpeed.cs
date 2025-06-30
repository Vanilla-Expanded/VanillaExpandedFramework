using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace VEF.Genes;

public class StatPart_GeneticTerrainMoveSpeed : StatPart
{
    private static readonly Dictionary<string, float> totalSpeed = [];
    private static readonly HashSet<(string, string)> usedTags = [];

    public override void TransformValue(StatRequest req, ref float val)
    {
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!ModsConfig.BiotechActive || req.Thing is not Pawn { genes: not null } pawn)
            return null;

        try
        {
            // Don't use StaticCollectionsClass in case the pawn is not spawned/doesn't have the effects applied yet.
            // Iterate over each gene, gather all speed factors for terrain tag into a Dictionary,
            // and store tagged speed factors for each terrain in a separate HashSet.
            foreach (var gene in pawn.genes.GenesListForReading)
            {
                var extension = gene.def.GetModExtension<GeneExtension>();
                if (extension == null || extension.moveSpeedFactorByTerrainTag.NullOrEmpty())
                    continue;

                foreach (var (terrainTag, factors) in extension.moveSpeedFactorByTerrainTag)
                {
                    foreach (var factor in factors)
                    {
                        // Check for speed factors with duplicate tags applied to the same terrain tag
                        if (factor.tag != null && !usedTags.Add((terrainTag, factor.tag)))
                            continue;

                        if (totalSpeed.TryGetValue(terrainTag, out var speed))
                            speed *= factor.moveSpeedFactor;
                        else
                            speed = factor.moveSpeedFactor;

                        totalSpeed[terrainTag] = speed;
                    }
                }
            }

            if (totalSpeed.Count == 0)
                return null;

            var builder = new StringBuilder();

            // Display in a style that matches vanilla StatReport_TerrainSpeedMultiplier
            foreach (var (terrainTag, speedFactor) in totalSpeed.OrderBy(x => x.Key))
            {
                if (builder.Length > 0)
                    builder.AppendLine();

                var taggedString = ("TerrainTag" + terrainTag).Translate();
                builder.AppendLine("VEF.StatsReport_GeneticTerrainSpeedMultiplier".Translate(taggedString) + ": x" + speedFactor.ToStringPercent());
            }

            return builder.ToString();
        }
        finally
        {
            // Cleanup the collections once done with them so no data bleeds over to the next pawn.
            // Nothing should be accessing those collections, so there's no need to clean beforehand.
            totalSpeed.Clear();
            usedTags.Clear();
        }
    }
}