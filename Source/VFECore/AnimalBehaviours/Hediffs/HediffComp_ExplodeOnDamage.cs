
using Verse;
using RimWorld;


namespace AnimalBehaviours
{
    public class HediffComp_ExplodeOnDamage : HediffComp
    {
       

        public HediffCompProperties_ExplodeOnDamage Props
        {
            get
            {
                return (HediffCompProperties_ExplodeOnDamage)this.props;
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            if(totalDamageDealt >= Props.minDamageToExplode && this.parent.pawn.Map!=null)
            {
                GenExplosion.DoExplosion(this.parent.pawn.Position, this.parent.pawn.Map, Props.radius, Props.damageType, this.parent.pawn, Props.damageAmount, -1, Props.sound, 
                    null, null, null, Props.spawnThingDef, Props.spawnThingChance, 1, null, false, null, 0f, 1);

            }

        }



       



    }
}
