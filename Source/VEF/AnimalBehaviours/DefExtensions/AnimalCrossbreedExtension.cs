using Verse;
using System.Collections.Generic;

namespace VEF.AnimalBehaviours
{

    public class AnimalCrossbreedExtension : DefModExtension
    {        
        public FatherOrMother crossBreedKindDef;

        //If crossBreedKindDef is set to OtherPawnKind, this needs to be set too, or it will default to mother
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
