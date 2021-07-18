using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_DestroyRefuelableLightSource : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            List<Thing> things = map.listerThings.AllThings.FindAll(t => rp.rect.Contains(t.Position) && t is Building b && b.TryGetComp<CompRefuelable>() != null && b.TryGetComp<CompGlower>() != null);
            if (map.ParentFaction != null)
                things.RemoveAll(t => t.Faction != map.ParentFaction);

            for (int i = 0; i < things.Count; i++)
            {
                things[i].DeSpawn();
            }
        }
    }
}