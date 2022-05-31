using System.Linq;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_GenerateRoad : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            // Generate road between doors
            Debug.Message($"Doors count: {SettlementGenUtils.doors.Count}");
            var toLink = SettlementGenUtils.Delaunay.Run(rp.rect.Corners.ElementAt(2), SettlementGenUtils.doors, rp.rect.Width, rp.rect.Height);
            Debug.Message($"To link count: {toLink.Count}");

            for (int i = 0; i < toLink.Count; i++)
            {
                var edge = toLink[i];

            }

            // TODO: Field gen
            // BaseGen.symbolStack.Push("kcsg_addfields", rp, null);

            // rp.rect.EdgeCells.ToList().ForEach(cell => SpawnConduit(cell, map));
        }
    }
}