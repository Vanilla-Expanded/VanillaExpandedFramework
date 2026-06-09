
using RimWorld;
using System;
using Verse;
namespace VEF.Plants
{
    public class StatPart_BeautyByBlooming : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            Plant_Blooming bloomingPlant = req.Thing as Plant_Blooming;
            if (bloomingPlant?.isBlooming ==true && !bloomingPlant.LeaflessNow)
            {
                         
                val *= bloomingPlant.GetExtension.BloomBeautyModifier;               
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            Plant_Blooming bloomingPlant = req.Thing as Plant_Blooming;
            if (bloomingPlant?.isBlooming == true && !bloomingPlant.LeaflessNow)
            {
                return "VPE_BeautyByBloming".Translate(bloomingPlant.GetExtension.BloomBeautyModifier);
            }
            return null;
        }
    }
}