using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AnimalBehaviours
{

    public class DamageWorker_ExtraDamageMechanoids : DamageWorker_Cut
    {


        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
            if (pawn.RaceProps.FleshType == FleshTypeDefOf.Mechanoid)
            {
                pawn.TakeDamage(new DamageInfo(DamageDefOf.EMP, 30, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));

            }

        }
    }
}