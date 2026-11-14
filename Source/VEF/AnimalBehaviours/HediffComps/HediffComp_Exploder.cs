using Verse;
using RimWorld;

namespace VEF.AnimalBehaviours
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
                GenExplosion.DoExplosion(corpse.Position, corpse.Map, Props.explosionForce, Props.damageDef, corpse, Props.damageAmount, Props.damagePenetration, null, null, null, null, null, 0f, 1, null, null, 255,false, null, 0f, 1);
            }
        }
    }
}
