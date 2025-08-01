﻿using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_ChangeWeather : HediffComp
    {




        public HediffCompProperties_ChangeWeather Props
        {
            get
            {
                return (HediffCompProperties_ChangeWeather)this.props;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            //Checks every rare tick to not be very spammy

            if (this.parent.pawn.Map != null && this.parent.pawn.IsHashIntervalTick(Props.tickInterval, delta))
            {
                if (Props.isRandomWeathers)
                {
                    this.parent.pawn.Map.weatherManager.curWeather = Props.randomWeathers.RandomElement();
                    this.parent.pawn.Map.weatherManager.TransitionTo(Props.randomWeathers.RandomElement());
                }
                else
                { //If the weather isn't already this
                    if (this.parent.pawn.Map.weatherManager.curWeather != WeatherDef.Named(Props.weatherDef))
                    {
                        //Set both curWeather and TransitionTo to ensure the weather changes immediately
                        this.parent.pawn.Map.weatherManager.curWeather = WeatherDef.Named(Props.weatherDef);
                        this.parent.pawn.Map.weatherManager.TransitionTo(WeatherDef.Named(Props.weatherDef));
                    }
                }
                
            }
        }




    }
}