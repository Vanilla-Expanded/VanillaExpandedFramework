using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public class CustomGenOption : DefModExtension
    {
        public bool UsingSingleLayout => chooseFromlayouts.Count > 0;

        /* Nomadic faction */
        public bool canSpawnSettlements = true;

        /* Structure generation */
        public List<StructureLayoutDef> chooseFromlayouts = new List<StructureLayoutDef>();
        public List<SettlementLayoutDef> chooseFromSettlements = new List<SettlementLayoutDef>();

        public string symbolResolver = null;

        public bool tryFindFreeArea = false;
        public bool preGenClear = true;
        public bool fullClear = false;
        public bool preventBridgeable = false;
        public bool clearFogInRect = false;

        public List<string> symbolResolvers = null;
        public List<ThingDef> scatterThings = new List<ThingDef>();
        public List<ThingDef> filthTypes = new List<ThingDef>();
        public float scatterChance = 0.4f;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var error in base.ConfigErrors())
                yield return error;

            for (int i = 0; i < filthTypes.Count; i++)
            {
                if (filthTypes[i].category != ThingCategory.Filth)
                    yield return $"{filthTypes[i].defName} in filthTypes, but isn't in category Filth.";
            }
        }
    }
}