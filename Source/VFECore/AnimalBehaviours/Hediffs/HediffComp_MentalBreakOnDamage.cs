
using Verse;
using RimWorld;


namespace AnimalBehaviours
{
    public class HediffComp_MentalBreakOnDamage : HediffComp
    {


        public HediffCompProperties_MentalBreakOnDamage Props
        {
            get
            {
                return (HediffCompProperties_MentalBreakOnDamage)this.props;
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            if (dinfo.Def == Props.damageTypeReceived && this.parent.pawn.Map != null)
            {
                this.parent.pawn.mindState.mentalBreaker.TryDoMentalBreak(Props.reason.Translate(),Props.mentalBreak);

            }

        }







    }
}
