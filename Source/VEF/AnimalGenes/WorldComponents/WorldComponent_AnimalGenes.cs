using RimWorld.Planet;
using Verse;
using System.Collections.Generic;

namespace VEF.AnimalGenes
{
    public class WorldComponent_AnimalGenes : WorldComponent
    {

        //Mostly obsolete since a dictionary is no longer used to cache the animal comps, but removing it will pop up an error message for people

        public static int maxStabilityPenalty = 5;
        
        public static WorldComponent_AnimalGenes Instance;

        public WorldComponent_AnimalGenes(World world) : base(world) => Instance = this;


    }
}
