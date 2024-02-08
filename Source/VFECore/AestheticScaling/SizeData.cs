using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VanillaGenesExpanded;
using Verse;
using Verse.Noise;

namespace VFECore
{
    public class SizeData : ICacheable
    {
        public static Dictionary<Pawn, SizeData> cache = new();

        public CacheTimer Timer { get; set; } = new();
        public Pawn pawn;
        public float totalSize;

        // Change to size in...
        public float percentChange;
        public float quadraticChange;
        public float cubicChange;  // Unused.

        // Rendering data.
        public float bodyRenderSize;
        public float headRenderSize;
        public float renderPosOffset;

        // Health data.
        public float healthMultiplier;

        public SizeData(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void RegenerateCache()
        {
            List<Gene> genes = pawn.genes == null ? new List<Gene>() : pawn.genes.GenesListForReading;
            List<GeneExtension> geneExts = genes.Where(x => x.Active && x.def.modExtensions != null && x.def.modExtensions
                                                .Any(y => y.GetType() == typeof(GeneExtension)))
                                                .Select(x => x.def.GetModExtension<GeneExtension>()).ToList();

            var dStage = pawn.DevelopmentalStage;
            float sizeFromAge = pawn.ageTracker.CurLifeStage.bodySizeFactor;
            float baseSize = pawn.RaceProps.baseBodySize;
            float previousTotalSize = sizeFromAge * baseSize;

            StatDef statOffsetDef = StatDef.Named("VEF_BodySize_Offset");
            float sizeOffset = pawn.GetStatValue(statOffsetDef);

            StatDef statCosmeticOffsetDef = StatDef.Named("VEF_CosmeticBodySize_Offset");
            float cosmeticSizeOffset = pawn.GetStatValue(statCosmeticOffsetDef);

            float offsetFromSizeByAge = geneExts.Where(x=>x.sizeByAge != null).Sum(x=>x.sizeByAge.GetSize(pawn.ageTracker.AgeBiologicalYearsFloat));
            sizeOffset+= offsetFromSizeByAge;

            cosmeticSizeOffset += sizeOffset;

            StatDef statMultDef = StatDef.Named("VEF_BodySize_Multiplier");
            float sizeMultiplier = pawn.GetStatValue(statMultDef);

            // Calculate the offset.
            float bodySizeOffset = ((baseSize + sizeOffset) * sizeMultiplier * sizeFromAge) - previousTotalSize;

            float bodySizeCosmeticOffset = ((baseSize + cosmeticSizeOffset) * sizeMultiplier * sizeFromAge) - previousTotalSize;

            // Get total size
            float totalSize = bodySizeOffset + previousTotalSize;
            //float totalCosmeticSize = bodySizeCosmeticOffset + previousTotalSize;

            // Clamp total size based on developmental stage. These are based around the notion of functional scale, not cosmetic scale, 
            // but it feels like reasonable to clamp there anyway.

            // Prevent babies from being too large for cribs, or too smol in general.
            if (dStage < DevelopmentalStage.Child)
            {
                totalSize = Mathf.Clamp(totalSize, 0.05f, 0.24f);
            }
            else if (totalSize < 0.10) totalSize = 0.10f;

            ////////////////////////////////// 
            // Clamp Offset to avoid extremes
            if (totalSize < 0.05f && dStage < DevelopmentalStage.Child)
            {
                bodySizeOffset = -(previousTotalSize - 0.05f);
            }
            // Don't permit babies too large to fit in cribs (0.25)
            else if (totalSize > 0.24f && dStage < DevelopmentalStage.Child && pawn.RaceProps.Humanlike)
            {
                bodySizeOffset = -(previousTotalSize - 0.24f);
            }
            else if (totalSize < 0.10f && dStage == DevelopmentalStage.Child)
            {
                bodySizeOffset = -(previousTotalSize - 0.10f);
            }
            // If adult basically limit size to 0.10
            else if (totalSize < 0.10f && dStage > DevelopmentalStage.Child && pawn.RaceProps.Humanlike)
            {
                bodySizeOffset = -(previousTotalSize - 0.10f);
            }

            (float percentChange, float quadraticChange, float cubicChange) = GetPercentChange(bodySizeOffset, pawn);
            (float percentChangeCosmetic, float _, float _) = GetPercentChange(bodySizeCosmeticOffset, pawn);

            StatDef statHeadSizeDef = StatDef.Named("VEF_HeadSize_Cosmetic");
            float headScaleStat = pawn.GetStatValue(statHeadSizeDef);

            StatDef renderOffsetDef = StatDef.Named("VEF_PawnRenderPosOffset");
            var renderOffsetVal = pawn.GetStatValue(renderOffsetDef);

            // Set Values
            this.totalSize = totalSize;
            this.percentChange = percentChange;
            this.quadraticChange = quadraticChange;
            this.cubicChange = cubicChange;
            bodyRenderSize = GetBodyRenderSize(percentChangeCosmetic);
            headRenderSize = GetHeadRenderSize(percentChangeCosmetic) * headScaleStat;
            renderPosOffset = GetYPositionOffset(bodyRenderSize, renderOffsetVal);

            healthMultiplier = CalculateHealthMultiplier(percentChange, quadraticChange);
        }

        private static (float, float, float) GetPercentChange(float bodySizeOffset, Pawn pawn)
        {
            float minimum = 0.2f;
            float sizeFromAge = pawn.ageTracker.CurLifeStage.bodySizeFactor;
            float baseSize = pawn.RaceProps.baseBodySize;
            float prevBodySize = sizeFromAge * baseSize;
            float postBodySize = prevBodySize + bodySizeOffset;
            float percentChange = postBodySize / prevBodySize;
            float quadratic = Mathf.Pow(percentChange, 2) - 1; ;
            float cubic = Mathf.Pow(percentChange, 3) - 1;

            // Ensure we don't get negative values.
            percentChange = Mathf.Max(percentChange, 0.04f);
            quadratic = Mathf.Max(quadratic, 0.04f);
            cubic = Mathf.Max(cubic, 0.04f);

            // Let's not make humans sprites unreasonably small.
            if (percentChange < minimum) percentChange = minimum;
            return (percentChange, quadratic, cubic);
        }

        public float GetBodyRenderSize(float size)
        {
            // If there is no change, then just return 1.
            if (size == 1) return 1f;

            // If Smol.
            else if (size < 1)
            {
                // Make Scale down children and babies a bit more to make sure they end up smaller than parents even if the parents are minimal size.
                if (pawn.DevelopmentalStage < DevelopmentalStage.Child)
                {
                    size = Mathf.Pow(size, 0.95f);
                }
                else if (pawn.DevelopmentalStage < DevelopmentalStage.Adult)
                {
                    size = Mathf.Pow(size, 0.90f);
                }
                // Don't make children/adults too small on screen even with stupidly low values it just looks bad.
                else
                {
                    size = Mathf.Pow(size, 0.75f);
                }
            }
            // If Large.
            else if (size > 1)
            {
                if (pawn.DevelopmentalStage < DevelopmentalStage.Child) // Makes babies smaller
                {
                    size = Mathf.Pow(size, 0.40f);
                }
                else if (pawn.DevelopmentalStage < DevelopmentalStage.Adult) // Don't oversize children too much.
                {
                    size = Mathf.Pow(size, 0.50f);
                }
                else // Don't make large characters unreasonably huge.
                {
                    size = Mathf.Pow(size, 0.7f);
                }
            }
            return size;
        }

        public static float GetHeadRenderSize(float size)
        {
            float headPowLarge = 0.8f;
            float headPowSmall = 0.65f;

            float headSize = size;

            if (headSize > 1)
            {
                //headSize = Mathf.Pow(bodyRSize, 0.8f);
                headSize = Mathf.Pow(size, headPowLarge);
                headSize = Math.Max(size - 0.5f, headSize);
            }
            else
            {
                // Beeg head for tiny people.
                headSize = Mathf.Pow(size, headPowSmall);
            }
            return headSize;
        }

        public float GetYPositionOffset(float bodyRenderSize, float offsetFromCache)
        {
            var factor = bodyRenderSize;
            var originalFactor = factor;
            if (factor < 1) { factor = 1; }

            // Will be null early in the load-step.
            float bodyGraphicsScale = pawn?.story?.bodyType?.bodyGraphicScale.y == null ? 1 : pawn.story.bodyType.bodyGraphicScale.y;

            return (factor - 1) / 2 * (offsetFromCache + 1) + offsetFromCache * 0.25f * (originalFactor < 1 ? originalFactor : 1) * bodyGraphicsScale;
        }

        /// <summary>
        /// This method is mostly just a bunch of fudged numbers to get scale values that are somewhat in line with the vanilla animals of a given size.
        /// 
        /// Or so I hope, it was ages since I did this math. 
        /// 
        /// Cached the math because HeathScale gets called an ungodly amount of times.
        /// </summary>
        private static float CalculateHealthMultiplier(float percentChange, float quadraticChange)
        {
            float quad = quadraticChange;
            float roughylLinear = percentChange;
            if (roughylLinear > 1)
            {
                roughylLinear = (percentChange - 1) * 0.8f + 1; // Nerf scaling a bit, large pawns are tanky enough already.
            }
            if (roughylLinear > quad) { quad = roughylLinear; } // Make sure small creatures don't get absolutely unreasonably low health.
            return Mathf.Lerp(roughylLinear, quad, 0.18f);
        }
    }
}
