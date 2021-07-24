using RimWorld;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class SpawnAtWorldGen : DefModExtension
    {
        public int spawnCount = 1;
        public FactionDef spawnPartOfFaction = null;
        public List<SitePartDef> parts = new List<SitePartDef>();
        public List<BiomeDef> allowedBiomes = new List<BiomeDef>();
        public List<BiomeDef> disallowedBiomes = new List<BiomeDef>();

        public override IEnumerable<string> ConfigErrors()
        {
            if (parts == null || parts.Count == 0)
            {
                Log.Error("SpawnAtWorldGen extension need at least one SitePartDef inside parts");
            }
            return base.ConfigErrors();
        }
    }
}