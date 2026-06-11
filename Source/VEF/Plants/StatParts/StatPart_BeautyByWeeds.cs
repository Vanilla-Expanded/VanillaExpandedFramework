
using RimWorld;
using System;
using Verse;
namespace VEF.Plants
{
    public class StatPart_BeautyByWeeds : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            Plant_Blooming bloomingPlant = req.Thing as Plant_Blooming;
            if (bloomingPlant?.hasWeeds==true)
            {                      
                val = bloomingPlant.GetExtension.WeededBeauty;               
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            Plant_Blooming bloomingPlant = req.Thing as Plant_Blooming;
            if (bloomingPlant?.hasWeeds == true)
            {
                return "VPE_BeautyByWeeds".Translate(bloomingPlant.GetExtension.WeededBeauty);
            }
            return null;
        }
    }
}