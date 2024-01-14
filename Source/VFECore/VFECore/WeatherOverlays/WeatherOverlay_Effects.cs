using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VFECore
{
    internal class WeatherOverlay_Effects : SkyOverlay
    {
        public Dictionary<Map, int> nextDamageTickForMap = new();
        public override void TickOverlay(Map map)
        {
            base.TickOverlay(map);
            if (VFEGlobal.settings.weatherDamagesOptions.TryGetValue(map.weatherManager.curWeather.defName, out var option) && !option)
            {
                return;
            }
            var options = map.weatherManager.curWeather.GetModExtension<WeatherEffectsExtension>();
            if (options != null)
            {
                if (options.activeOnWeatherPerceived is null || map.weatherManager.CurWeatherPerceived == options.activeOnWeatherPerceived)
                {
                    // If not in dictionary will fallback to default(int) - 0
                    nextDamageTickForMap.TryGetValue(map, out var nextDamageTick);
                    if (nextDamageTick == 0 || (Find.TickManager.TicksGame - nextDamageTick) > options.ticksInterval.max)
                    {
                        nextDamageTickForMap[map] = nextDamageTick = NextDamageTick(options);
                    }
                    if (Find.TickManager.TicksGame > nextDamageTick)
                    {
                        DoDamage(options, map);
                        nextDamageTickForMap[map] = NextDamageTick(options);
                    }
                }
            }
        }

        public int NextDamageTick(WeatherEffectsExtension options)
        {
            return Find.TickManager.TicksGame + Rand.RangeInclusive(options.ticksInterval.min, options.ticksInterval.max);
        }

        public void DoDamage(WeatherEffectsExtension options, Map map)
        {
            for (int i = map.listerThings.AllThings.Count - 1; i >= 0; i--)
            {
                Thing thing = map.listerThings.AllThings[i];
                if (CanDamage(thing, map, options))
                {
                    if (thing is Pawn pawn)
                    {
                        DoPawnDamage(pawn, options);
                    }
                    else
                    {
                        DoThingDamage(thing, options);
                    }
                }
            }
            if (options.damageToApply != null)
            {
                var victimCandidates = map.mapPawns.AllPawns.Where(x => CanDamage(x, map, options));
                var victims = RandomlySelectedItems(victimCandidates, (int)(victimCandidates.Count() * options.percentOfPawnsToDealDamage)).ToList();
                for (int num = victims.Count - 1; num >= 0; num--)
                {
                    var damageInfo = new DamageInfo(options.damageToApply, options.damageRange.RandomInRange);
                    victims[num].TakeDamage(damageInfo);
                }
            }
        }

        public bool CanDamage(Thing thing, Map map, WeatherEffectsExtension options)
        {
            if (thing is Pawn pawn)
            {
                if (!pawn.RaceProps.IsFlesh && !options.worksOnNonFleshPawns)
                {
                    return false;
                }
            }

            if (!thing.Position.InBounds(map) || thing.Position.Roofed(map) && !options.worksIndoors)
            {
                return false;
            }
            return true;
        }
        public void DoPawnDamage(Pawn p, WeatherEffectsExtension options)
        {
            if (options.hediffsToApply != null)
            {
                foreach (var opt in options.hediffsToApply)
                {
                    var hediffDef = HediffDef.Named(opt.hediff);
                    if (hediffDef != null)
                    {
                        var severity                                    = opt.severityOffset;

                        if (opt.effectMultiplyingStat != null) severity *= p.GetStatValue(opt.effectMultiplyingStat);

                        if (severity != 0f)
                        {
                            HealthUtility.AdjustSeverity(p, hediffDef, severity);
                        }
                    }
                }
            }
        }

        public void DoThingDamage(Thing thing, WeatherEffectsExtension options)
        {
            if (options.killsPlants && thing is Plant)
            {
                if (Rand.Value < options.chanceToKillPlants)
                {
                    thing.Kill(null, null);
                }
            }

            else if (thing.def.category == ThingCategory.Item)
            {
                CompRottable compRottable = thing.TryGetComp<CompRottable>();
                if (options.causesRotting && compRottable != null && compRottable.Stage < RotStage.Dessicated)
                {
                    compRottable.RotProgress += options.rotProgressPerDamage;
                }
            }
        }

        public static IEnumerable<Pawn> RandomlySelectedItems(IEnumerable<Pawn> sequence, int count)
        {
            return sequence.InRandomOrder().Take(count);
        }
    }
}