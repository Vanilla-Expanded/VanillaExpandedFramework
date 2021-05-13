using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    class SymbolResolver_KCSG_RoomGenFromList : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            foreach (var keyValue in CurrentGenerationOption.vectStruct)
            {
                Log.Message($"Generating {keyValue.Value.defName}");
                KCSG_Utilities.HeightWidthFromLayout(keyValue.Value, out int height, out int width);
                CellRect rect = CellRect.CenteredOn(new IntVec3(CurrentGenerationOption.gridStartPoint.x + width / 2, 0, CurrentGenerationOption.gridStartPoint.z - height / 2), width, height);
                foreach (List<string> item in keyValue.Value.layouts)
                {
                    KCSG_Utilities.GenerateRoomFromLayout(item, rect, BaseGen.globalSettings.map, keyValue.Value);
                }
            }
        }
    }
}
