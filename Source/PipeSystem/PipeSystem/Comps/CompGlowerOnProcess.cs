using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PipeSystem
{
   
    public class CompGlowerOnProcess : CompGlower
    {
        CompAdvancedResourceProcessor processor;

        public new CompProperties_GlowerOnProcess Props => (CompProperties_GlowerOnProcess)props;

        protected override bool ShouldBeLitNow
        {

            get {
                bool stop = processor?.Process?.IsRunning ==true;
               
                return stop && base.ShouldBeLitNow;                    
            
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            processor = this.parent.GetComp<CompAdvancedResourceProcessor>();

            base.PostSpawnSetup(respawningAfterLoad);
        }


    }
}