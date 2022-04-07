using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PipeSystem
{
    public class CompProperties_ResourceTrader : CompProperties_Resource
    {
        public float consumptionPerTick;
        // TODO: Add idle consumption as an option

        public SoundDef soundAmbientReceivingResource;

        private string nameCapitalized;
        private string nameLowered;

        public CompProperties_ResourceTrader()
        {
            compClass = typeof(CompResourceTrader);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            nameCapitalized = Resource.name.CapitalizeFirst();
            nameLowered = Resource.name.ToLower();
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (var item in base.SpecialDisplayStats(req))
                yield return item;

            string unit = Resource.unit;
            if (consumptionPerTick > 0f)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building,
                                               "PipeSystem_Consumption".Translate(nameCapitalized),
                                               $"{(consumptionPerTick / 100) * GenDate.TicksPerDay:##0} {unit}/d",
                                               "PipeSystem_ConsumptionExplained".Translate(nameLowered, nameLowered),
                                               5500);
            }
            else
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Building,
                                               "PipeSystem_Production".Translate(nameCapitalized),
                                               $"{(-consumptionPerTick / 100) * GenDate.TicksPerDay:##0} {unit}/d",
                                               "PipeSystem_ProductionExplained".Translate(nameLowered, nameLowered),
                                               5500);
            }
        }
    }
}