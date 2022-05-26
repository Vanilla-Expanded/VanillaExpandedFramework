using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RoomGenFromStructure : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            GenOption.currentGenStep = "Generating single structure";

            Map map = BaseGen.globalSettings.map;

            for (int i = 0; i < GenOption.structureLayoutDef.layouts.Count; i++)
            {
                GenUtils.GenerateRoomFromLayout(GenOption.structureLayoutDef, i, rp.rect, map);
            }
            GenUtils.GenerateRoofGrid(GenOption.structureLayoutDef, rp.rect, map);
        }
    }
}