using Verse;
using VEF.Storyteller;
using System.Collections.Generic;

namespace VEF.Storyteller
{
	public class GenStep_Site : GenStep
	{
		public StructureSetDef structureSetDef;

        public override int SeedPart => def.GetHashCode();

        public List<CellRect> BuildStructure(Map map, GenStepParams parms)
		{
            return StructureSetGenerator.Generate(map, structureSetDef, map.ParentFaction, parms.sitePart.parms.points);
        }

        public override void Generate(Map map, GenStepParams parms)
        {
            BuildStructure(map, parms);
        }
    }
}
