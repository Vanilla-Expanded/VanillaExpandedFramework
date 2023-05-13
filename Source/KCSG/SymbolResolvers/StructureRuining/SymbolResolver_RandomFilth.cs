using RimWorld.BaseGen;
using System.Linq;
using Verse;

namespace KCSG
{
    class SymbolResolver_RandomFilth : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            if (GenOption.customGenExt.filthTypes != null && GenOption.customGenExt.filthTypes.Any())
            {
                foreach (IntVec3 pos in rp.rect)
                {
                    if (Rand.Bool && pos.GetTerrain(map).filthAcceptanceMask != RimWorld.FilthSourceFlags.None)
                    {
                        GenSpawn.Spawn(ThingMaker.MakeThing(GenOption.customGenExt.filthTypes.RandomElement()), pos, BaseGen.globalSettings.map, WipeMode.FullRefund);
                    }
                }
            }
        }
    }
}
