
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;


namespace VEF.Buildings
{
    [StaticConstructorOnStartup]
    public static class StaticCollectionsClass
    {

        static StaticCollectionsClass()
        {

            foreach (HiddenDesignatorsDef hiddenDesignatorDef in DefDatabase<HiddenDesignatorsDef>.AllDefsListForReading)
            {
               
                foreach (BuildableDef thing in hiddenDesignatorDef.hiddenDesignators)
                {
                   
                    hidden_designators.Add(thing);
                }
            }          

        }

        //This static class stores lists for different things.    

        // A list of designators that shouldn't appear on the architect menu.
        public static HashSet<BuildableDef> hidden_designators = new HashSet<BuildableDef>();

     
    }
}
