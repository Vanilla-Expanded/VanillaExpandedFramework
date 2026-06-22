using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VEF.Plants
{

    public class CompGlowerBlooming : CompGlower
    {
        Plant_Blooming plant;

        public new CompProperties_GlowerBlooming Props => (CompProperties_GlowerBlooming)props;

        protected override bool ShouldBeLitNow
        {

            get
            {
                
                return plant!=null && plant.Growth>=1 && plant.isBlooming && base.ShouldBeLitNow;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            plant = this.parent as Plant_Blooming;

            base.PostSpawnSetup(respawningAfterLoad);
        }


    }
}