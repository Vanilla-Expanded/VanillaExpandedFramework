using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_SpawnMote : HediffCompProperties
    {
        public ThingDef moteDef;
        public Vector3 offset;
        public float maxScale;
        public HediffCompProperties_SpawnMote()
        {
            compClass = typeof(HediffComp_SpawnMote);
        }
    }
}
