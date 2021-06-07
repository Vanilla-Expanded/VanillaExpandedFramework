using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_ThingSetOnlyRoofed : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            ThingSetMakerParams parms;
            parms = rp.thingSetMakerParams.Value;
            parms.makingFaction = rp.faction;

            List<Thing> list = ThingSetMakerDefOf.MapGen_DefaultStockpile.root.Generate(parms);
            for (int i = 0; i < list.Count; i++)
            {
                ResolveParams resolveParams = rp;
                resolveParams.singleThingToSpawn = list[i];
                BaseGen.symbolStack.Push("kcsg_thingroofed", resolveParams, null);
            }
        }
    }
}