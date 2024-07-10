
using Verse;
using RimWorld;


namespace AnimalBehaviours
{
    public class HediffComp_Exploder : HediffComp
    {


        public HediffCompProperties_Exploder Props
        {
            get
            {
                return (HediffCompProperties_Exploder)this.props;
            }
        }



        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            Corpse corpse = this.parent.pawn?.Corpse;
            if (corpse != null && corpse.Map != null) {
                GenExplosion.DoExplosion(this.parent.pawn.Corpse.Position, this.parent.pawn.Corpse.Map, Props.explosionForce, Props.damageDef, this.parent.pawn.Corpse, -1, -1, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1);
            }


        }




    }
}
