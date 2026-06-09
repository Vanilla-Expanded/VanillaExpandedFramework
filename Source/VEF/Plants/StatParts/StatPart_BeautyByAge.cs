
using RimWorld;
using System;
using Verse;
namespace VEF.Plants
{
    public class StatPart_BeautyByAge : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            Plant_Blooming bloomingPlant = req.Thing as Plant_Blooming;
            if (bloomingPlant != null && !bloomingPlant.LeaflessNow)
            {
                int age = bloomingPlant.realAge / 3600000;            
                val += Math.Min(bloomingPlant.GetExtension.MaxAgeBeautyModifier, bloomingPlant.GetExtension.AgeBeautyModifier*age);               
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            Plant_Blooming bloomingPlant = req.Thing as Plant_Blooming;
            if (bloomingPlant != null && !bloomingPlant.LeaflessNow)
            {
                int age = bloomingPlant.realAge / 3600000;
                int val = Math.Min(bloomingPlant.GetExtension.MaxAgeBeautyModifier, bloomingPlant.GetExtension.AgeBeautyModifier * age);
                return "VPE_BeautyByAge".Translate(val);
            }
            return null;
        }
    }
}