using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RoomGenFromStructure : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            GenOption.mineables = new Dictionary<IntVec3, Mineable>();
            foreach (var cell in rp.rect)
                GenOption.mineables.Add(cell, cell.GetFirstMineable(map));

            GenUtils.GenerateLayout(GenOption.structureLayoutDef, rp.rect, map);
            BaseGen.symbolStack.Push("kcsg_runresolvers", rp, null);
        }
    }
}