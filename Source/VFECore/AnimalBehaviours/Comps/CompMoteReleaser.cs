
using AnimalBehaviours;
using RimWorld;
using UnityEngine;
using Verse;
namespace AnimalBehaviours
{
    [StaticConstructorOnStartup]
    public class CompMoteReleaser : ThingComp
    {

        private Mote mote;

        public CompProperties_MoteReleaser Props => (CompProperties_MoteReleaser)props;


        public override void CompTick()
        {
            if (this.parent.Map != null)
            {
                if (mote == null)
                {
                    mote = MoteMaker.MakeStaticMote(parent.DrawPos, parent.Map, Props.moteDef);
                    mote.instanceColor = parent.DrawColor;
                }
                if (mote.def.mote.needsMaintenance)
                {
                    mote.Maintain();
                }

            }
        }

        public void Notify_ColorChanged()
        {
            mote = null;
        }





    }
}
