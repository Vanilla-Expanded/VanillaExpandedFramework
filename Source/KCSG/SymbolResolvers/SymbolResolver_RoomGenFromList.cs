using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_RoomGenFromList : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            CurrentGenerationOption.currentGenStep = "Generating structures";

            foreach (KeyValuePair<CustomVector, StructureLayoutDef> keyValue in CurrentGenerationOption.vectStruct)
            {
                CurrentGenerationOption.currentGenStepMoreInfo = "Generating " + keyValue.Value.defName;

                RectUtils.HeightWidthFromLayout(keyValue.Value, out int height, out int width);
                IntVec3 limitMin = new IntVec3
                {
                    x = CurrentGenerationOption.offset.x + (int)keyValue.Key.X,
                    z = CurrentGenerationOption.offset.z - (int)keyValue.Key.Y - height + 1
                };
                CellRect rect = new CellRect(limitMin.x, limitMin.z, width, height);

                foreach (List<string> item in keyValue.Value.layouts)
                {
                    GenUtils.GenerateRoomFromLayout(item, rect, BaseGen.globalSettings.map, keyValue.Value);
                }

                if (keyValue.Value.isStorage)
                {
                    ResolveParams rstock = rp;
                    rstock.rect = new CellRect(limitMin.x, limitMin.z, width, height);
                    BaseGen.symbolStack.Push("kcsg_storagezone", rstock, null);
                }
            }

            if (CurrentGenerationOption.settlementLayoutDef.addLandingPad && ModLister.RoyaltyInstalled)
            {
                if (rp.rect.TryFindRandomInnerRect(new IntVec2(9, 9), out CellRect rect, null))
                {
                    CurrentGenerationOption.currentGenStepMoreInfo = "Generating landing pad";
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