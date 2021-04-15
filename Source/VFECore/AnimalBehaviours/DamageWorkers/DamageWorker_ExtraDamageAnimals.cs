using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AnimalBehaviours
{

    public class DamageWorker_ExtraDamageAnimals : DamageWorker_Cut
    {


        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
            if (pawn.RaceProps.Animal)
            {
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Cut, 20, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
                if (dinfo.HitPart.def.bleedRate > 0)
                {
                    HediffSet hediffSet = pawn.health.hediffSet;
                    HealthUtility.AdjustSeverity(pawn, pawn.health.hediffSet.hediffs.Last().def, hediffSet.BleedRateTotal * 0.01f);
                }
            }

        }
    }
}