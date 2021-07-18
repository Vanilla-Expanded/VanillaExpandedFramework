using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public class FactionSettlement : DefModExtension
    {
        /* Nomadic faction */
        public bool canSpawnSettlements = true;

        /* Settlement generation */
        public bool useStructureLayout; // Settlement def or single structure

        public List<StructureLayoutDef> chooseFromlayouts = new List<StructureLayoutDef>();
        public List<SettlementLayoutDef> chooseFromSettlements = new List<SettlementLayoutDef>();

        /* Custom symbol resolver */
        public string symbolResolver = null;
        /* Clear filth, buildings, chunks, remove non-natural terrain */
        public bool preGenClear = true;
        /* Clear everything */
        public bool fullClear = false;
        /* Try to find a clear area to fit the structure */
        public bool tryFindFreeArea = false;

        /* Ruin */
        public bool shouldRuin = false;
        public bool spawnCropsField = true;
        public List<ThingDef> filthTypes = new List<ThingDef>();
        public List<ThingDef> scatterThings = new List<ThingDef>();
        public float scatterChance = 0.4f;
        public List<string> ruinSymbolResolvers = new List<string>();

        /* Handle errors */
        public override IEnumerable<string> ConfigErrors()
        {
            if (shouldRuin && !ruinSymbolResolvers.Any())
                Log.Error($"FactionSettlement DefModExtension have shouldRuin at true but have empty ruinSymbolResolvers list");
            if (filthTypes.Any(t => t.category != ThingCategory.Filth))
                Log.Error($"FactionSettlement DefModExtension have filthTypes list with non filth thing(s)");

            return base.ConfigErrors();
        }
    }
}