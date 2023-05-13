using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RandomRoofRemoval : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            foreach (IntVec3 c in rp.rect)
            {
                if (Rand.Bool && c.GetRoof(map) is RoofDef roofDef && roofDef == RoofDefOf.RoofConstructed)
                {
                    map.roofGrid.SetRoof(c, null);
                }
            }

            if (GenOption.customGenExt.scatterThings?.Count > 0)
                BaseGen.symbolStack.Push("kcsg_scatterstuffaround", rp, null);
        }
    }
}