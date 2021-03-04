using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VWEMakeshift 
{
    public class MoteProperties : DefModExtension
    {
        private const float MoteSizer = 32f;

        public float size = -1;
        public float velocity = -1;
        public FloatRange? angleRange;
        public FloatRange? rotationRange;
        public ThingDef moteDef;
        public int numTimesThrown = 1;

        public float Velocity => velocity > 0 ? velocity : Rand.Range(0.5f, 0.7f);

        public float Angle => angleRange is null ? Rand.Range(30, 40) : angleRange.Value.RandomInRange;

        public float Rotation => rotationRange is null ? Rand.Range(-30f, 30f) : rotationRange.Value.RandomInRange;

        public float Size(int damage) => size > 0 ? size : Mathf.Clamp01(damage / MoteSizer);

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }

            if (moteDef is null)
            {
                yield return "<color=teal>moteDef</color> cannot be null. This field must be populated.";
            }
        }
    }
}
