using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public class CustomGenOption : DefModExtension
    {
        public bool AdditionalResolvers { get; set; }
        public bool UsingSingleLayout { get; set; }

        /* Nomadic faction */
        public bool canSpawnSettlements = true;

        /* Structure generation */
        public List<StructureLayoutDef> chooseFromlayouts = new List<StructureLayoutDef>();
        public List<SettlementLayoutDef> chooseFromSettlements = new List<SettlementLayoutDef>();

        public string symbolResolver = null;

        public bool tryFindFreeArea = false;
        public bool preGenClear = true;
        public bool fullClear = false;

        public List<string> symbolResolvers = new List<string>();
        public List<ThingDef> scatterThings = new List<ThingDef>();
        public List<ThingDef> filthTypes = new List<ThingDef>();
        public float scatterChance = 0.4f;

        // TODO Obsolete - To remove in next rimworld version
        [Obsolete]
        public bool useStructureLayout;
        [Obsolete]
        public bool shouldRuin = false;
        [Obsolete]
        public List<string> ruinSymbolResolvers;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
                yield return error;

            for (int i = 0; i < filthTypes.Count; i++)
            {
                if (filthTypes[i].category != ThingCategory.Filth)
                    yield return $"{filthTypes[i].defName} in filthTypes, but isn't in category Filth.";
            }

            // TODO Compat - To remove in next rimworld version
            if (!ruinSymbolResolvers.NullOrEmpty())
                symbolResolvers = ruinSymbolResolvers;
            //
            AdditionalResolvers = symbolResolvers.Count > 0;

            UsingSingleLayout = chooseFromlayouts.Count > 0;
        }
    }

    // TODO Obsolete - To remove in next rimworld version
    [Obsolete]
    public class FactionSettlement : CustomGenOption { }
}