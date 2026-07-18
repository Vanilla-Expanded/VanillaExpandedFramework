using Verse;
using System.Collections.Generic;
using RimWorld;

namespace VEF.AnimalBehaviours
{

    public class AnimalCrossbreedExtension : DefModExtension
    {        
        public FatherOrMother crossBreedKindDef;

        //If crossBreedKindDef is set to OtherPawnKind, one of those needs to be set too, or it will default to mother.
        // otherPawnKindsByWeight, if specified, is used first. It allows for a list of outcomes with different probability.
        public List<PawnKindDefWeight> otherPawnKindsByWeight;
        // otherPawnKind will be used if otherPawnKindsByWeight is unspecified (or fails for whatever reason)
        public PawnKindDef otherPawnKind;
        // This is the chance for the mother PawnKindDef to be chosen for Random. By default 0.5 is true random
        public float weightForMother = 0.5f;

    }

    public enum FatherOrMother
    {
        AlwaysMother,  //The default, vanilla behaviour
        AlwaysFather,
        Random,
        OtherPawnKind
    }

}
