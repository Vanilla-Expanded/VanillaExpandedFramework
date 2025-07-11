using System.Collections.Generic;
using Verse;

namespace VEF.Genes;

public class ExtendedMoveSpeedFactorByTerrainTag
{
    private readonly Dictionary<string, Dictionary<object, float>> moveSpeedFactorByTerrainTag = [];
    private readonly Dictionary<string, Dictionary<string, (float speedFactor, HashSet<object> activeGenes)>> taggedMoveSpeedFactorByTerrainTag = [];

    public bool Empty => moveSpeedFactorByTerrainTag.Count == 0 && taggedMoveSpeedFactorByTerrainTag.Count == 0;

    public void Add(object effectHolder, Dictionary<string, List<MoveSpeedFactor>> speedFactors)
    {
        foreach (var (terrainTag, speedFactor) in speedFactors)
        {
            foreach (var factor in speedFactor)
            {
                if (factor.tag == null)
                {
                    if (!moveSpeedFactorByTerrainTag.TryGetValue(terrainTag, out var currentFactors))
                        moveSpeedFactorByTerrainTag[terrainTag] = currentFactors = [];
                    currentFactors[effectHolder] = factor.moveSpeedFactor;
                }
                else
                {
                    if (!taggedMoveSpeedFactorByTerrainTag.TryGetValue(terrainTag, out var currentFactors))
                        taggedMoveSpeedFactorByTerrainTag[terrainTag] = currentFactors = [];

                    if (!currentFactors.TryGetValue(factor.tag, out var tuple))
                        currentFactors[factor.tag] = tuple = (factor.moveSpeedFactor, []);

                    tuple.activeGenes.Add(effectHolder);
                }
            }
        }
    }

    public void Remove(object effectHolder)
    {
        moveSpeedFactorByTerrainTag.RemoveAll(x =>
        {
            x.Value.Remove(effectHolder);
            return x.Value.Count == 0;
        });

        taggedMoveSpeedFactorByTerrainTag.RemoveAll(x =>
        {
            x.Value.RemoveAll(z =>
            {
                z.Value.activeGenes.Remove(effectHolder);
                return z.Value.activeGenes.Count == 0;
            });
            return x.Value.Count == 0;
        });
    }

    public void ApplySpeed(List<string> terrainTags, ref float speed)
    {
        if (terrainTags == null)
            return;

        foreach (var tag in terrainTags)
            ApplySpeed(tag, ref speed);
    }

    public void ApplySpeed(string terrainTag, ref float speed)
    {
        if (terrainTag == null)
            return;

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