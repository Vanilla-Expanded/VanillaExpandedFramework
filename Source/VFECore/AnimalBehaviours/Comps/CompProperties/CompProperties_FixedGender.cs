using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AnimalBehaviours
{
    public class CompProperties_FixedGender : CompProperties
    {

        //This comp class makes an animal always spawn with a given gender

        public Gender gender = Gender.Female;

        public CompProperties_FixedGender()
        {
            this.compClass = typeof(CompFixedGender);
        }


    }
}
