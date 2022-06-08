using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RoomGenFromStructure : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;

            GenUtils.GenerateLayout(GenOption.structureLayoutDef, rp.rect, map);
            BaseGen.symbolStack.Push("kcsg_handleruins", rp, null);
        }
    }
}