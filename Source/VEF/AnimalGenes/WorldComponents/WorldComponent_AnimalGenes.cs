using RimWorld.Planet;
using Verse;
using System.Collections.Generic;

namespace VEF.AnimalGenes
{
    public class WorldComponent_AnimalGenes : WorldComponent
    {

        public static int maxStabilityPenalty = 5;

        public Dictionary<Thing, CompAnimalGenes> pawnToCompAnimalGenes = new Dictionary<Thing, CompAnimalGenes>();

        public static WorldComponent_AnimalGenes Instance;

        public WorldComponent_AnimalGenes(World world) : base(world) => Instance = this;

        public void AddAnimalComp(Thing pawn, CompAnimalGenes comp)
        {
            if (!pawnToCompAnimalGenes.ContainsKey(pawn))
            {
                pawnToCompAnimalGenes[pawn] = comp;
            }
        }

        public void RemoveAnimalComp(Thing pawn)
        {
            if (pawnToCompAnimalGenes.ContainsKey(pawn))
            {
                pawnToCompAnimalGenes.Remove(pawn);
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (Find.TickManager.TicksGame % 2000 != 0)
                return;

            List<Thing> toRemove = null;

            foreach (Thing thing in pawnToCompAnimalGenes.Keys)
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null && pawn.Dead && pawn.Corpse == null)
                {
                    toRemove ??= new List<Thing>();
                    toRemove.Add(pawn);
                }
            }

            if (toRemove != null)
            {
                foreach (Thing pawn in toRemove)
                {
                    pawnToCompAnimalGenes.Remove(pawn);
                }
            }
        }


    }
}
