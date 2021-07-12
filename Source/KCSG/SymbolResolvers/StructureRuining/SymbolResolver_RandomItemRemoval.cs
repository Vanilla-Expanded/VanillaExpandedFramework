using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RandomItemRemoval : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            List<Thing> things = map.listerThings.AllThings.FindAll(t => rp.rect.Contains(t.Position) && t.def.category == ThingCategory.Item);
            for (int i = 0; i < things.Count; i++)
            {
                if (Rand.Bool) things[i].Destroy();
            }
        }
    }
}