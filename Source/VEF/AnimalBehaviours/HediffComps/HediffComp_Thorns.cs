

using RimWorld;
using System.Collections.Generic;
using Verse;
using System.Linq;
using Verse.Sound;
using UnityEngine;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_Thorns : HediffComp
    {


        public HediffCompProperties_Thorns Props
        {
            get
            {
                return (HediffCompProperties_Thorns)this.props;
            }
        }


        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            if(dinfo.Instigator is Pawn pawn)
            {
                DamageInfo newdinfo = new DamageInfo( def: Props.damageType, amount: Props.damageDone.RandomInRange, armorPenetration: Props.armorPenetration, instigator: parent.pawn);

                pawn.TakeDamage(newdinfo);

            }
        }



    }
}
