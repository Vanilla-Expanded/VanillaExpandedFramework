using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VEF.Global
{
    public class MoteAttachedScaled : MoteAttached
    {
        public float maxScale;

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            if (!Destroyed)
                if (def.mote.growthRate != 0f)
                {
                    linearScale = new(linearScale.x + def.mote.growthRate * deltaTime, linearScale.y, linearScale.z + def.mote.growthRate * deltaTime);
                    linearScale.x = Mathf.Min(Mathf.Max(linearScale.x, 0.0001f), maxScale);
                    linearScale.z = Mathf.Min(Mathf.Max(linearScale.z, 0.0001f), maxScale);
                }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref maxScale, "maxScale");
        }
    }

}
