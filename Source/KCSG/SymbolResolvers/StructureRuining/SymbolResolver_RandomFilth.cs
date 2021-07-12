using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    class SymbolResolver_RandomFilth : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            if (CGO.factionSettlement.filthTypes != null && CGO.factionSettlement.filthTypes.Any())
            {
                foreach (IntVec3 pos in rp.rect)
                {
                    if (Rand.Bool && pos.GetTerrain(map).filthAcceptanceMask != RimWorld.FilthSourceFlags.None)
                    {
                        GenSpawn.Spawn(ThingMaker.MakeThing(CGO.factionSettlement.filthTypes.RandomElement()), pos, BaseGen.globalSettings.map, WipeMode.FullRefund);
                    }
                }
            }
        }
    }
}
