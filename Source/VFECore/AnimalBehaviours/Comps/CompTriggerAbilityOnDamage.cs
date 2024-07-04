using RimWorld;
using Verse;

namespace AnimalBehaviours
{
    class CompTriggerAbilityOnDamage : ThingComp
    {


        public CompProperties_TriggerAbilityOnDamage Props
        {
            get
            {
                return (CompProperties_TriggerAbilityOnDamage)this.props;
            }
        }


        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
           
            Pawn pawn = this.parent as Pawn;
          
            if (pawn != null && totalDamageDealt >= Props.minDamageToTrigger && dinfo.Instigator!=null)
            {
            
                Ability ability = pawn.abilities?.GetAbility(Props.ability);
                if (ability != null && !ability.OnCooldown)
                {
                    ability.QueueCastingJob(dinfo.Instigator, dinfo.Instigator.Position);
                  

                }
            }

        }

    }
}
