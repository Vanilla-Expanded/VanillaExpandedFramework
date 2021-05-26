using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    internal class SymbolResolver_KCSG_RoomGenFromList : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            foreach (var keyValue in CurrentGenerationOption.vectStruct)
            {
                RectUtils.HeightWidthFromLayout(keyValue.Value, out int height, out int width);
                IntVec3 center = new IntVec3
                {
                    x = CurrentGenerationOption.offset.x + (int)keyValue.Key.X + (width / 2),
                    z = CurrentGenerationOption.offset.z - (int)keyValue.Key.Y - (height / 2)
                };
                CellRect rect = CellRect.CenteredOn(center, width, height);

                foreach (List<string> item in keyValue.Value.layouts)
                {
                    GenUtils.GenerateRoomFromLayout(item, rect, BaseGen.globalSettings.map, keyValue.Value);
                }

                if (keyValue.Value.isStorage)
                {
                    ResolveParams rstock = rp;
                    //rect.TryFindRandomInnerRect(minRectSize, out rstock.rect);
                    rstock.rect = CellRect.CenteredOn(center, width-2, height-2);
                    BaseGen.symbolStack.Push("stockpile", rstock, null);
                }
            }
        }
    }
}