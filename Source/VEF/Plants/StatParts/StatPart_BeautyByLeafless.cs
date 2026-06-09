
using RimWorld;
using System;
using Verse;
namespace VEF.Plants
{
    public class StatPart_BeautyByLeafless : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            Plant_Blooming bloomingPlant = req.Thing as Plant_Blooming;
            if (bloomingPlant?.LeaflessNow==true)
            {                      
                val = bloomingPlant.GetExtension.LeaflessBeauty;               
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            Plant_Blooming bloomingPlant = req.Thing as Plant_Blooming;
            if (bloomingPlant?.LeaflessNow == true)
            {
                return "VPE_BeautyByLeafless".Translate(bloomingPlant.GetExtension.LeaflessBeauty);
            }
            return null;
        }
    }
}