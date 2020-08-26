using RimWorld;
using Verse;
using System.Collections.Generic;

namespace VanillaCookingExpanded
{
    public class CompStackByQuality : ThingComp
    {

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
