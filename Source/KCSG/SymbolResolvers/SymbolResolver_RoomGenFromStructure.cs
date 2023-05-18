using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RoomGenFromStructure : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            GenOption.GetAllMineableIn(rp.rect, map);
            GenOption.structureLayout.Generate(rp.rect, map);

            // Clear fog in rect if wanted
            if (GenOption.customGenExt.clearFogInRect)
            {
                foreach (var c in rp.rect)
                {
                    if (map.fogGrid.IsFogged(c))
                        map.fogGrid.Unfog(c);
                    else
                        MapGenerator.rootsToUnfog.Add(c);
                }
            }

            BaseGen.symbolStack.Push("kcsg_runresolvers", rp, null);
        }
    }
}