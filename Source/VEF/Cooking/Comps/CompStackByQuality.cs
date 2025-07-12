using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VEF.Cooking
{
    public class CompStackByQuality : ThingComp
    {

        //A comp class to make items only stack if their qualities are the same

        //Used for example in Vanilla Cooking Expanded to avoid cheese of different qualities stacking, and
        //thus "ruining" the higher quality

        public override bool AllowStackWith(Thing other)
        {

            if ((other as ThingWithComps)?.compQuality is { } otherComp)
            {
                QualityCategory quality1 = otherComp.Quality;
                QualityCategory quality2 = this.parent.compQuality.Quality;


                if (quality1 != quality2)
                {
                    return false;
                }

            }



            return base.AllowStackWith(other);
        }



    }
}
