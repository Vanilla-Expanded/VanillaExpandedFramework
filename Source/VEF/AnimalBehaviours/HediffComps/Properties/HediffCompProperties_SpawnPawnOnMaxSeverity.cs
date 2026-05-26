using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Text;

namespace VEF.AnimalBehaviours
{
    public class HediffCompProperties_SpawnPawnOnMaxSeverity : HediffCompProperties
    {
        public List<PawnKindDef> pawnKindOptions = new List<PawnKindDef>();
        public ThingDef filthCreated;
        public IntRange filthCountRange;
        public SoundDef sound;
        public DamageDef damage;
        public FloatRange damageAmount; 

        public HediffCompProperties_SpawnPawnOnMaxSeverity()
        {
            this.compClass = typeof(HediffComp_SpawnPawnOnMaxSeverity);
        }
    }
}
