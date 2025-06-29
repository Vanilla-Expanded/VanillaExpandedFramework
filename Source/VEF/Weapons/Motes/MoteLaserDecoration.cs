﻿using System;
using Verse;

namespace VEF.Weapons
{
    class MoteLaserDecoration : MoteThrown
    {
        public LaserBeamGraphic beam;
        public float baseSpeed;
        public float speedJitter;
        public float speedJitterOffset;

        public override float Alpha
        {
            get
            {
                Speed = (float)(baseSpeed + speedJitter * Math.Sin(Math.PI * (Find.TickManager.TicksGame * 18f + speedJitterOffset) / 180.0));

                if (beam != null) return beam.Opacity;
                return base.Alpha;
            }
        }
    }
}
