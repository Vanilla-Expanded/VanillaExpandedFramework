using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RemovePerishable : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            List<Thing> things = map.listerThings.AllThings.FindAll(t => rp.rect.Contains(t.Position) && t.def.category == ThingCategory.Item && t.def.HasComp(typeof(CompRottable)));
            for (int i = 0; i < things.Count; i++)
                things[i].Destroy();
        }
    }
}