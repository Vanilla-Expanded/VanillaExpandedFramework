

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Text;

namespace AnimalBehaviours
{
    public class HediffCompProperties_Exploder : HediffCompProperties
    {


        public float explosionForce = 5.9f;
        public DamageDef damageDef = null;

        public HediffCompProperties_Exploder()
        {
            this.compClass = typeof(HediffComp_Exploder);
        }
    }
}
