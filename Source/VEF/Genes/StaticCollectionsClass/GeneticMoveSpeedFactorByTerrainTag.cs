using System.Collections.Generic;
using Verse;

namespace VEF.Genes;

public class GeneticMoveSpeedFactorByTerrainTag
{
    private readonly Dictionary<string, Dictionary<Gene, float>> moveSpeedFactorByTerrainTag = [];
    private readonly Dictionary<string, Dictionary<string, (float speedFactor, HashSet<Gene> activeGenes)>> taggedMoveSpeedFactorByTerrainTag = [];

    public bool Empty => moveSpeedFactorByTerrainTag.Count == 0 && taggedMoveSpeedFactorByTerrainTag.Count == 0;

    public void Add(Gene gene, Dictionary<string, List<GeneExtension.MoveSpeedFactor>> speedFactors)
    {
        foreach (var (terrainTag, speedFactor) in speedFactors)
        {
            foreach (var factor in speedFactor)
            {
                if (factor.tag == null)
                {
                    if (!moveSpeedFactorByTerrainTag.TryGetValue(terrainTag, out var currentFactors))
                        moveSpeedFactorByTerrainTag[terrainTag] = currentFactors = [];
                    currentFactors[gene] = factor.moveSpeedFactor;
                }
                else
                {
                    if (!taggedMoveSpeedFactorByTerrainTag.TryGetValue(terrainTag, out var currentFactors))
                        taggedMoveSpeedFactorByTerrainTag[terrainTag] = currentFactors = [];

                    if (!currentFactors.TryGetValue(factor.tag, out var tuple))
                        currentFactors[factor.tag] = tuple = (factor.moveSpeedFactor, []);

                    tuple.activeGenes.Add(gene);
                }
            }
        }
    }

    public void Remove(Gene gene)
    {
        moveSpeedFactorByTerrainTag.RemoveAll(x =>
        {
            x.Value.Remove(gene);
            return x.Value.Count == 0;
        });

        taggedMoveSpeedFactorByTerrainTag.RemoveAll(x =>
        {
            x.Value.RemoveAll(z =>
            {
                z.Value.activeGenes.Remove(gene);
                return z.Value.activeGenes.Count == 0;
            });
            return x.Value.Count == 0;
        });
    }

    public void ApplySpeed(List<string> terrainTags, ref float speed)
    {
        foreach (var tag in terrainTags)
            ApplySpeed(tag, ref speed);
    }

    public void ApplySpeed(string terrainTag, ref float speed)
    {
        if (moveSpeedFactorByTerrainTag.TryGetValue(terrainTag, out var entries))
        {
            foreach (var value in entries.Values)
                speed /= value;
        }

        if (taggedMoveSpeedFactorByTerrainTag.TryGetValue(terrainTag, out var taggedEntries))
        {
            foreach (var (value, _) in taggedEntries.Values)
                speed /= value;
        }
    }
}