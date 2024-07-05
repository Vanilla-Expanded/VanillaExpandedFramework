
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    class HediffComp_Animation : HediffComp
    {


        public HediffCompProperties_Animation Props
        {
            get
            {
                return (HediffCompProperties_Animation)this.props;
            }
        }



        public override void CompPostPostAdd(DamageInfo? dinfo)
        {

            this.parent.pawn.Drawer.renderer.SetAnimation(Props.animation);

        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!this.parent.pawn.Drawer.renderer.HasAnimation)
            {
                this.parent.pawn.Drawer.renderer.SetAnimation(Props.animation);
            }
            if (Props.shamblerParticles && Rand.MTBEventOccurs(1f, 60f, 1f))
            {
                FleckMaker.ThrowShamblerParticles(this.parent.pawn);
            }
        }

        public override void CompPostPostRemoved()
        {
            this.parent.pawn.Drawer.renderer.SetAnimation(null);

        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            this.parent.pawn.Drawer.renderer.SetAnimation(null);

        }

        public override void Notify_PawnKilled()
        {
            this.parent.pawn.Drawer.renderer.SetAnimation(null);

        }


    }
}
