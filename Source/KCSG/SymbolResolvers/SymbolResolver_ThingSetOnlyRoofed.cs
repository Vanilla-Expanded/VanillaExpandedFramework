using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_ThingSetOnlyRoofed : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            ThingSetMakerDef thingSetMakerDef = ThingSetMakerDefOf.MapGen_DefaultStockpile;
            ThingSetMakerParams parms;
            if (rp.thingSetMakerParams != null)
            {
                parms = rp.thingSetMakerParams.Value;
            }
            else
            {
                int num = rp.rect.Cells.Count((IntVec3 x) => x.Standable(map) && x.Roofed(map) && x.GetFirstItem(map) == null);
                parms = default;
                parms.countRange = new IntRange?(new IntRange(num, num));
                parms.techLevel = new TechLevel?((rp.faction != null) ? rp.faction.def.techLevel : TechLevel.Undefined);
            }
            parms.makingFaction = rp.faction;
            List<Thing> list = thingSetMakerDef.root.Generate(parms);
            for (int i = 0; i < list.Count; i++)
            {
                ResolveParams resolveParams = rp;
                resolveParams.singleThingToSpawn = list[i];
                BaseGen.symbolStack.Push("thing", resolveParams, null);
            }
        }
    }
}