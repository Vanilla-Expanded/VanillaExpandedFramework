using RimWorld;
using Verse;

namespace VFECore
{
    internal class WeatherOverlay_Effects : SkyOverlay
    {
        public override void TickOverlay(Map map)
        {
            base.TickOverlay(map);
            if (map.weatherManager.curWeather.HasModExtension<WeatherEffectsExtension>())
            {
                var options = map.weatherManager.curWeather.GetModExtension<WeatherEffectsExtension>();
                if (options.ticksInterval > 0)
                {
                    if (Find.TickManager.TicksGame % options.ticksInterval == 0)
                    {
                        DoDamage(options, map);
                    }
                }
                else
                {
                    DoDamage(options, map);
                }
            }
        }

        public void DoDamage(WeatherEffectsExtension options, Map map)
        {
            for (int i = map.listerThings.AllThings.Count - 1; i >= 0; i--)
            {
                Thing thing = map.listerThings.AllThings[i];
                if (thing is Pawn pawn && thing.Spawned && !thing.Position.Roofed(map))
                {
                    DoPawnDamage(pawn, options);
                }
                else if (thing.Spawned && !thing.Position.Roofed(map))
                {
                    DoThingDamage(thing, options);
                }
            }
        }

        public void DoPawnDamage(Pawn p, WeatherEffectsExtension options)
        {
            if (!p.RaceProps.IsFlesh)
            {
                return;
            }
            foreach (var opt in options.hediffs)
            {
                var hediffDef = HediffDef.Named(opt.hediffDefName);
                var severity = options.severity * p.GetStatValue(opt.affectingStat, true);
                if (severity != 0f)
                {
                    HealthUtility.AdjustSeverity(p, hediffDef, severity);
                }
            }
        }

        public void DoThingDamage(Thing thing, WeatherEffectsExtension options)
        {
            if (options.killingPlants && thing is Plant)
            {
                if (Rand.Value < 0.0065f)
                {
                    thing.Kill(null, null);
                }
            }
            else if (thing.def.category == ThingCategory.Item)
            {
                CompRottable compRottable = thing.TryGetComp<CompRottable>();
                if (options.causesRotting && compRottable != null && compRottable.Stage < RotStage.Dessicated)
                {
                    compRottable.RotProgress += 3000f;
                }
            }
        }
    }
}