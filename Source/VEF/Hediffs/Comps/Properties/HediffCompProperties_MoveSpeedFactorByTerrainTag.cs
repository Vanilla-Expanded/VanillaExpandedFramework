using System.Collections.Generic;
using VEF.Genes;
using Verse;

namespace VEF.Hediffs;

public class HediffCompProperties_MoveSpeedFactorByTerrainTag : HediffCompProperties
{
    // For a more detail description, check GeneExtension.moveSpeedFactorByTerrainTag.
    public Dictionary<string, List<MoveSpeedFactor>> moveSpeedFactorByTerrainTag;
}