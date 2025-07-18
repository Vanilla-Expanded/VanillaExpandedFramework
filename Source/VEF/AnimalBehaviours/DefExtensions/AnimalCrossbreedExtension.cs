using Verse;
using System.Collections.Generic;

namespace VEF.AnimalBehaviours
{

    public class AnimalCrossbreedExtension : DefModExtension
    {        
        public FatherOrMother crossBreedKindDef;

        //If crossBreedKindDef is set to OtherPawnKind, one of those needs to be set too, or it will default to mother.
        // otherPawnKindsByWeight, if specified, is used first. It allows for a list of outcomes with different probability.
        public Dictionary<PawnKindDef, float> otherPawnKindsByWeight;
        // otherPawnKind will be used if otherPawnKindsByWeight is unspecified (or fails for whatever reason)
        public PawnKindDef otherPawnKind;

    }

    public enum FatherOrMother
    {
        AlwaysMother,  //The default, vanilla behaviour
        AlwaysFather,
        Random,
        OtherPawnKind
    }

}
