using RimWorld;
using System.Collections.Generic;
using Verse;

namespace VEF.Buildings
{
    public class LootBox : ThingWithComps, IOpenable
    {

        public LootBoxExtension cachedLootBoxExtension;
        public CompQuality cachedCompQuality;

        public LootBoxExtension GetExtension
        {
            get
            {
                if (cachedLootBoxExtension is null)
                {
                    cachedLootBoxExtension = this.def.GetModExtension<LootBoxExtension>();
                }
                return cachedLootBoxExtension;
            }
        }

        public CompQuality GetQuality
        {
            get
            {
                if (cachedCompQuality is null)
                {
                    cachedCompQuality = this.GetComp<CompQuality>();
                }
                return cachedCompQuality;
            }
        }

        public float AmountByQuality(QualityCategory quality)
        {
            switch (quality)
            {
                case QualityCategory.Awful:
                    return 0.5f;
                case QualityCategory.Poor:
                    return 0.75f;
                case QualityCategory.Normal:
                    return 1;
                case QualityCategory.Good:
                    return 1.25f;
                case QualityCategory.Excellent:
                    return 1.5f;
                case QualityCategory.Masterwork:
                    return 2.5f;
                case QualityCategory.Legendary:
                    return 5f;
            }
            return 1;
        }

        public int OpenTicks => 300;

        public bool CanOpen => true;

        public void Open()
        {

            ThingSetMakerParams parms = default(ThingSetMakerParams);

            parms.totalMarketValueRange = GetExtension.totalMarketValueRange * AmountByQuality(GetQuality.Quality);
            parms.minSingleItemMarketValuePct = GetExtension.minSingleItemMarketValuePct;
            parms.allowNonStackableDuplicates = GetExtension.allowNonStackableDuplicates;

            int fixStupidIntRangeStuff = GetExtension.countRange.RandomInRange;
            parms.countRange = new IntRange(fixStupidIntRangeStuff, fixStupidIntRangeStuff);

            List<Thing> list2 = GetExtension.thingSetMakerDef.root.Generate(parms);

            if (list2 != null)
            {

                foreach (Thing thing in list2)
                {
                    GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near);
                }

                if (this.Spawned)
                {
                    this.Destroy();
                }
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (StatDrawEntry item in base.SpecialDisplayStats())
            {
                yield return item;
            }

            yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "VEF.TotalMarketValueRange".Translate(), GetExtension.totalMarketValueRange.ToString(), "VEF.TotalMarketValueRange_Desc".Translate(), 2749);
            yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "VEF.TotalMarketValueRange_Quality".Translate(), (GetExtension.totalMarketValueRange * AmountByQuality(GetQuality.Quality)).ToString(), "VEF.TotalMarketValueRange_Quality_Desc".Translate(AmountByQuality(GetQuality.Quality).ToString()), 2748);

        }


    }
}
