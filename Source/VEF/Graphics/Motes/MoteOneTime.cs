﻿namespace VEF.Graphics
{
    using Verse;

    public class MoteOneTime : Mote, IAnimationOneTime
    {
        public int currentIndex;
        public int CurrentIndex()
        {
            return currentIndex;
        }
        protected override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick((this.Graphic.data as GraphicData_Animated).ticksPerFrame))
            {
                if (currentIndex < (this.Graphic as Graphic_Animated).SubGraphicCount)
                {
                    currentIndex++;
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentIndex, "currentIndex");
        }
    }
}