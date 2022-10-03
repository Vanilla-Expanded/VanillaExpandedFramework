using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace AnimalBehaviours
{
    public class CompChangeWeather : ThingComp
    {




        public CompProperties_ChangeWeather Props
        {
            get
            {
                return (CompProperties_ChangeWeather)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            //Checks every rare tick to not be very spammy

            if (this.parent.Map != null && this.parent.IsHashIntervalTick(Props.tickInterval))
            {
                if (Props.isRandomWeathers)
                {
                    this.parent.Map.weatherManager.curWeather = Props.randomWeathers.RandomElement();
                    this.parent.Map.weatherManager.TransitionTo(Props.randomWeathers.RandomElement());
                }
                else
                { //If the weather isn't already this
                    if (this.parent.Map.weatherManager.curWeather != WeatherDef.Named(Props.weatherDef))
                    {
                        //Set both curWeather and TransitionTo to ensure the weather changes immediately
                        this.parent.Map.weatherManager.curWeather = WeatherDef.Named(Props.weatherDef);
                        this.parent.Map.weatherManager.TransitionTo(WeatherDef.Named(Props.weatherDef));
                    }
                }
            }
        }




    }
}