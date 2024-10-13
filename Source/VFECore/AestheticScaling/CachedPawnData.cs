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
    /// <summary>
    /// Fallback class for the SizeData in case something forgets to call forceRefresh and we end up with out of date data for a long time.
    /// </summary>
    public class CachedPawnDataSlowUpdate : GameComponent
    {
        public Queue<Pawn> pawnsToRefresh = new();

        public CachedPawnDataSlowUpdate(Game game) { }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (pawnsToRefresh.Count == 0)
            {
                foreach (var cache in PawnDataCache.Cache.Values)
                {
                    if (cache.pawn != null)
                        pawnsToRefresh.Enqueue(cache.pawn);
                }
            }
            else if (Find.TickManager.TicksGame % 25 == 0)
            {
                var cachedPawn = pawnsToRefresh.Dequeue();
                PawnDataCache.GetPawnDataCache(cachedPawn, forceRefresh: true);
            }
        }
    }

    public class CachedPawnData : ICacheable
    {
        public static bool cacheCanBeRecalculated = true;
        public static CachedPawnData defaultCache = new();

        public static Dictionary<Pawn, CachedPawnData> cache = new();

        public Pawn pawn = null;
        public float totalSize = 1;
        public float bodySizeOffset = 0;

        public float headPositionMultiplier = 1;

        // Change to size in...
        public float percentChange = 1;
        public float quadraticChange = 1;
        public float cubicChange = 1;  // Unused.

        // Rendering data.
        public bool renderCacheOff = false;
        public float bodyRenderSize = 1;
        public float headRenderSize = 1;
        public float renderPosOffset = 0;
        public Vector3 vCosmeticScale = Vector3.one;
        public bool isHumanlike = false;

        // Health data.
        public float healthMultiplier = 1;

        // Food
        public float foodCapacityMult = 1;

        // Children "Learning" point accumulation.
        public float growthPointMultiplier = 1;

        public CachedPawnData() { }

        public CachedPawnData(Pawn pawn)
        {
            this.pawn = pawn;
            // This might get called from a rendering thread. Should be fine, but just in case.
            try { isHumanlike = pawn.RaceProps?.Humanlike == true; }
            catch { Log.Error($"[VEF] Error checking Humanlike when setting up {pawn}"); }
        }

        public bool RegenerateCache()
        {
            if (!cacheCanBeRecalculated || pawn == null) return false;
            if (Scribe.mode == LoadSaveMode.LoadingVars) return false;

            // If the needs are null (and the pawn is not just suffering a case of "dead") skip the cache generation.
            // It typically means the pawn isn't fully initialized yet or otherwise unsuitable.
            // Afaik. we don't have any creatures with null needs using this system.
            if (pawn.needs == null && !pawn.Dead) return false;

            try
            {
                cacheCanBeRecalculated = false;  // Prevent recursion. Just to be safe.

                List<Gene> genes = pawn.genes == null ? new List<Gene>() : pawn.genes.GenesListForReading;
                List<GeneExtension> geneExts = genes.Where(x => x.Active && x.def.modExtensions != null && x.def.modExtensions
                                                    .Any(y => y.GetType() == typeof(GeneExtension)))
                                                    .Select(x => x.def.GetModExtension<GeneExtension>()).ToList();

                isHumanlike = pawn.RaceProps?.Humanlike == true;  // In case of PawnMorpher or something.
                var lifestage = pawn.ageTracker.CurLifeStage;
                Vector2 bodyLifeStageCosmeticScale = new(1, 1);
                var bodyLifeStageCosScaleExts = geneExts.Where(x => x.bodyScaleFactorsPerLifestages != null && x.bodyScaleFactorsPerLifestages.ContainsKey(lifestage));
                if (bodyLifeStageCosScaleExts.Any())
                {
                    bodyLifeStageCosmeticScale = bodyLifeStageCosScaleExts.Aggregate(bodyLifeStageCosmeticScale, (acc, x) => acc * x.bodyScaleFactorsPerLifestages[lifestage]);
                }

                var dStage = pawn.DevelopmentalStage;
                float sizeFromAge = pawn.ageTracker.CurLifeStage.bodySizeFactor;
                float baseSize = pawn.RaceProps.baseBodySize;
                float previousTotalSize = sizeFromAge * baseSize;

                float sizeOffset = pawn.GetStatValue(VFEDefOf.VEF_BodySize_Offset);

                float cosmeticSizeOffset = pawn.GetStatValue(VFEDefOf.VEF_CosmeticBodySize_Offset);

                float offsetFromSizeByAge = geneExts.Where(x => x.sizeByAge != null).Sum(x => x.sizeByAge.GetSize(pawn?.ageTracker?.AgeBiologicalYearsFloat));
                sizeOffset += offsetFromSizeByAge;

                cosmeticSizeOffset += sizeOffset;

                float sizeMultiplier = pawn.GetStatValue(VFEDefOf.VEF_BodySize_Multiplier);
                float cosmeticMultiplier = pawn.GetStatValue(VFEDefOf.VEF_CosmeticBodySize_Multiplier);
                float totalCosmeticMultiplier = sizeMultiplier + cosmeticMultiplier - 1;

                // Calculate the offset.
                float bodySizeOffset = ((baseSize + sizeOffset) * sizeMultiplier * sizeFromAge) - previousTotalSize;

                float bodySizeCosmeticOffset = ((baseSize + cosmeticSizeOffset) * totalCosmeticMultiplier * sizeFromAge) - previousTotalSize;

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

                if (!pawn.RaceProps.Humanlike)
                {
                    // Because of how we scale animals in the ELSE-statement the scaling of animals/Mechs gets run twice.
                    // Checking their node explicitly risks missing cases where someone uses another node.
                    percentChangeCosmetic = Mathf.Sqrt(percentChangeCosmetic);
                }

                float headScaleStat = pawn.GetStatValue(VFEDefOf.VEF_HeadSize_Cosmetic);

                var renderOffsetVal = pawn.GetStatValue(VFEDefOf.VEF_PawnRenderPosOffset);

                // Set Values
                this.totalSize = totalSize;
                this.percentChange = percentChange;
                this.quadraticChange = quadraticChange;
                this.cubicChange = cubicChange;
                this.bodySizeOffset = bodySizeOffset;
                bodyRenderSize = GetBodyRenderSize(percentChangeCosmetic);
                headRenderSize = GetHeadRenderSize(bodyRenderSize) * headScaleStat;
                vCosmeticScale = new Vector3(bodyRenderSize * bodyLifeStageCosmeticScale.x, 1, bodyRenderSize * bodyLifeStageCosmeticScale.y);
                renderPosOffset = GetYPositionOffset(bodyRenderSize, renderOffsetVal);
                renderCacheOff = geneExts.Any(x => x.renderCacheOff);

                healthMultiplier = CalculateHealthMultiplier(percentChange, pawn);

                // Other cached data
                foodCapacityMult = pawn.GetStatValue(VFEDefOf.VEF_FoodCapacityMultiplier);
                growthPointMultiplier = pawn.GetStatValue(VFEDefOf.VEF_GrowthPointMultiplier);

                CalculateHeadOffset();
            }
            finally
            {
                cacheCanBeRecalculated = true;
            }
            return true;
        }
        private void CalculateHeadOffset()
        {
            var headPos = Mathf.Lerp(bodyRenderSize, headRenderSize, 0.8f);
            // Move up the head for small pawns so they don't end up a walking head.
            if (headPos < 1) { headPos = Mathf.Pow(headPos, 0.96f); }
            headPositionMultiplier = headPos;
        }

        private static (float, float, float) GetPercentChange(float bodySizeOffset, Pawn pawn)
        {
            float minimum = 0.2f;
            float sizeFromAge = pawn.ageTracker.CurLifeStage.bodySizeFactor;
            float baseSize = pawn.RaceProps.baseBodySize;
            float prevBodySize = sizeFromAge * baseSize;
            float postBodySize = prevBodySize + bodySizeOffset;
            float percentChange = postBodySize / prevBodySize;
            float quadratic = Mathf.Pow(postBodySize, 2) - Mathf.Pow(prevBodySize, 2);
            float cubic = Mathf.Pow(postBodySize, 3) - Mathf.Pow(prevBodySize, 3);

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
            if (factor <= 1.0001) { factor = 1; }

            // Will be null early in the load-step.
            float bodyGraphicsScale = pawn?.story?.bodyType?.bodyGraphicScale.y == null ? 1 : pawn.story.bodyType.bodyGraphicScale.y;

            return (factor - 1) / 2 * (offsetFromCache + 1) + offsetFromCache * 0.25f * (originalFactor < 1 ? originalFactor : 1) * bodyGraphicsScale;
        }

        /// <summary>
        /// Calculates the health based on some fudged math. Technically it should probably be the cubic change, but that makes large creatures tank antigrain warheads.
        /// 
        /// Cached the math because HeathScale gets called a lot.
        /// </summary>
        private static float CalculateHealthMultiplier(float percentChange, Pawn pawn)
        {
            if (percentChange <= 1) return percentChange;

            const float maxHealthScale = 4;
            const float lerpScapeDiv = 1; // Change this to determine how soon the creature's health scales up and hits the maxHealthScale.
            float lerpScaleFactor = maxHealthScale / lerpScapeDiv;

            float raceHealthBase = pawn.RaceProps?.baseHealthScale ?? 1;
            float raceSize = pawn.RaceProps?.baseBodySize ?? 1;

            float raceHealth = raceHealthBase / raceSize;
            float targetRaceHScale = Mathf.Max(maxHealthScale, raceHealth);

            float baseSize = raceSize * pawn?.ageTracker?.CurLifeStage?.bodySizeFactor ?? 1;
            float newSize = percentChange * baseSize;
            float sizeChange = newSize - baseSize;

            float n = Mathf.Clamp01(sizeChange / lerpScaleFactor);

            // This is a bit of a hack to make sure we don't get too much hp at small increases.
            float newScale = Mathf.SmoothStep(raceHealth, targetRaceHScale, n);
            float newScale2 = Mathf.Lerp(raceHealth, targetRaceHScale, n);
            newScale = Mathf.Lerp(newScale, newScale2, 0.5f);

            float changeInRaceScale = newScale / raceHealth;
            return percentChange * changeInRaceScale;
        }
    }
}
