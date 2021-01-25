using Verse;
using System.Collections.Generic;


namespace AnimalBehaviours
{
    public class CompProperties_ExtremeXenophobia : CompProperties
    {

        //This comp class makes this animal attack any member of its faction that doesn't belong
        //to a specified list of defNames. Checks every berserkRate to avoid lag

        public int berserkRate = 10000;
        
        public List<string> AcceptedDefnames = null;

        public CompProperties_ExtremeXenophobia()
        {
            this.compClass = typeof(CompExtremeXenophobia);
        }


    }
}