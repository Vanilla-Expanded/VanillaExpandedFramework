using System.Collections.Generic;
using RimWorld.BaseGen;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RoomGenFromList : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            GenOption.currentGenStep = "Generating rooms";

            foreach (KeyValuePair<CustomVector, StructureLayoutDef> keyValue in GenOption.vectStruct)
            {
                StructureLayoutDef layoutDef = keyValue.Value;
                GenOption.currentGenStepMoreInfo = "Generating " + layoutDef.defName;

                IntVec3 limitMin = new IntVec3
                {
                    x = GenOption.offset.x + (int)keyValue.Key.X,
                    z = GenOption.offset.z - (int)keyValue.Key.Y - layoutDef.height + 1
                };
                CellRect rect = new CellRect(limitMin.x, limitMin.z, layoutDef.width, layoutDef.height);

                for (int i = 0; i < layoutDef.layouts.Count; i++)
                {
                    GenUtils.GenerateRoomFromLayout(layoutDef, i, rect, BaseGen.globalSettings.map);
                }
                GenUtils.GenerateRoofGrid(layoutDef, rect, BaseGen.globalSettings.map);

                if (keyValue.Value.isStorage)
                {
                    ResolveParams rstock = rp;
                    rstock.rect = new CellRect(limitMin.x, limitMin.z, layoutDef.width, layoutDef.height);
                    BaseGen.symbolStack.Push("kcsg_storagezone", rstock, null);
                }
            }

            if (GenOption.settlementLayoutDef.addLandingPad && ModLister.RoyaltyInstalled)
            {
                if (rp.rect.TryFindRandomInnerRect(new IntVec2(9, 9), out CellRect rect, null))
                {
                    GenOption.currentGenStepMoreInfo = "Generating landing pad";
                    ResolveParams resolveParams = rp;
                    resolveParams.rect = rect;
                    BaseGen.symbolStack.Push("landingPad", resolveParams, null);
                    BaseGen.globalSettings.basePart_landingPadsResolved++;
                }
            }

            BaseGen.symbolStack.Push("kcsg_gridsecondpass", rp, null);
        }
    }
}