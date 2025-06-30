using System.Collections.Generic;
using Verse;

namespace VEF.Genes;

public class HediffCompProperties_MoveSpeedFactorByTerrainTag : HediffCompProperties
{
    // For a more detail description, check GeneExtension.moveSpeedFactorByTerrainTag.
    public Dictionary<string, List<MoveSpeedFactor>> moveSpeedFactorByTerrainTag;
}