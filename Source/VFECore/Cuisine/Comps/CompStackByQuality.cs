using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VanillaCookingExpanded
{
    public class CompStackByQuality : ThingComp
    {

        //A comp class to make items only stack if their qualities are the same

        //Used for example in Vanilla Cooking Expanded to avoid cheese of different qualities stacking, and
        //thus "ruining" the higher quality

        public override bool AllowStackWith(Thing other)
        {

            if (other.TryGetComp<CompQuality>() != null)
            {
                QualityCategory quality1 = other.TryGetComp<CompQuality>().Quality;
                QualityCategory quality2 = this.parent.TryGetComp<CompQuality>().Quality;


                if (quality1 != quality2)
                {
                    return false;
                }

            }



            return base.AllowStackWith(other);
        }



    }
}
