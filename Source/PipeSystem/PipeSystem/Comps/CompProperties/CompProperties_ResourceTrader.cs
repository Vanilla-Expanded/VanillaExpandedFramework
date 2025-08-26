using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PipeSystem
{
    public class CompProperties_ResourceTrader : CompProperties_Resource
    {
        public float consumptionPerTick;
        public float idleConsumptionPerTick = -1f;
        // Should the overlay for resource being off be drawn (if flicked off, etc.)
        public bool resourceOffOverlay = true;
        // If true, the consumption/production will be disabled (to be handled manually), but its value will still be included in the network stats.
        public bool visualOnlyConsumption = false;
        // If true, the resource trader will handle ticking refuelable itself, which will only happen if the resource trader is actually used. You need to set "externalTicking" for associated refuelable to true.
        public bool handleCompRefuelableTicking = false;
        // If true, the trader's PowerTrader power usage will be changed to its idle power consumption when refuelable comp is empty.
        public bool lowPowerWhenRefuelableEmpty = false;

        // Producer configs
        // If true, the producer's PowerTrader power usage will be changed to its idle power consumption when all storages are full.
        public bool producerLowPowerWhenStorageFull = false;

        public SoundDef soundAmbientReceivingResource;

        private string nameCapitalized;
        private string nameLowered;

        /// <summary>
        /// Determines if a producer/consumer ever has a low power mode.
        /// This uses value provided by <see cref="InitializeEverHasLowPowerMode"/>, cached during <see cref="ResolveReferences"/> call.
        /// </summary>
        public bool EverHasLowPowerMode { get; private set; }

        public CompProperties_ResourceTrader()
        {
            compClass = typeof(CompResourceTrader);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            nameCapitalized = Resource.name.CapitalizeFirst();
            nameLowered = Resource.name.ToLower();

            EverHasLowPowerMode = InitializeEverHasLowPowerMode(parentDef);
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
                                               $"{consumptionPerTick / 100 * GenDate.TicksPerDay:##0} {unit}/d",
                                               "PipeSystem_ConsumptionExplained".Translate(nameLowered, nameLowered),
                                               5500);

                if (idleConsumptionPerTick >= 0f)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.Building,
                                                   "PipeSystem_IdleConsumption".Translate(nameCapitalized),
                                                   $"{idleConsumptionPerTick / 100 * GenDate.TicksPerDay:##0} {unit}/d",
                                                   "PipeSystem_IdleConsumptionExplained".Translate(nameLowered, nameLowered),
                                                   5500);
                }
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

        /// <summary>
        /// Provides the value for <see cref="EverHasLowPowerMode"/> property.
        /// </summary>
        /// <param name="parentDef">The parent def of this comp props.</param>
        /// <returns>The value that <see cref="EverHasLowPowerMode"/> will be set to.</returns>
        protected virtual bool InitializeEverHasLowPowerMode(ThingDef parentDef)
        {
            // Can't have low power mode when there's no power trader
            if (!parentDef.HasComp<CompPowerTrader>())
                return false;

            // Producer conditions
            if (consumptionPerTick < 0f)
            {
                if (producerLowPowerWhenStorageFull)
                    return true;
            }

            // Currently, no consumer-only conditions

            // Universal checks
            if (lowPowerWhenRefuelableEmpty && parentDef.HasComp<CompRefuelable>())
                return true;

            // None of the checks passed
            return false;
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var error in base.ConfigErrors(parentDef))
                yield return error;

            if (handleCompRefuelableTicking)
            {
                var props = parentDef.GetCompProperties<CompProperties_Refuelable>();
                if (props == null)
                    yield return $"{nameof(CompProperties_ResourceTrader)} has {nameof(handleCompRefuelableTicking)} set to true, but has no associated refuelable comp.";
                else if (!props.externalTicking)
                    yield return $"{nameof(CompProperties_ResourceTrader)} has {nameof(handleCompRefuelableTicking)} set to true, but its associated refuelable comp has {nameof(CompProperties_Refuelable.externalTicking)} set to false.";
            }
        }
    }
}