
using AnimalBehaviours;
using RimWorld;
using UnityEngine;
using Verse;
namespace AnimalBehaviours
{
    [StaticConstructorOnStartup]
    public class CompAttachEffecter : ThingComp
    {

        private Effecter effecter;

        public CompProperties_AttachEffecter Props => (CompProperties_AttachEffecter)props;


        public override void CompTick()
        {
            if (this.parent.Map != null)
            {
                if (effecter == null)
                {
                    effecter = Props.effecterDef.SpawnAttached(this.parent, this.parent.Map);
                }
                effecter?.EffectTick(this.parent, this.parent);

            }


        }



    }
}
