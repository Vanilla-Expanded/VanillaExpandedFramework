using System.Collections.Generic;
using Verse;

namespace VEF.AnimalBehaviours
{
    public class CompProperties_SpawnPawnsOnMissingBodyPart : CompProperties
    {

        public List<PawnKindDef> pawnKindOptions = new List<PawnKindDef>();
        public ThingDef filthCreated;
        public IntRange filthCountRange;
        public SoundDef sound;

        public CompProperties_SpawnPawnsOnMissingBodyPart()
        {
            this.compClass = typeof(CompSpawnPawnsOnMissingBodyPart);
        }


    }
}