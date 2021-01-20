using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AnimalBehaviours
{

    public class DamageWorker_ExtraDamagePirates : DamageWorker_Bite
    {


        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
            if (pawn.Faction != null && pawn.Faction.def.defName == "Pirate")
            {
                pawn.TakeDamage(new DamageInfo(DamageDefOf.Scratch, 50, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
                if (dinfo.HitPart.def.bleedRate > 0)
                {
                    HediffSet hediffSet = pawn.health.hediffSet;
                    HealthUtility.AdjustSeverity(pawn, pawn.health.hediffSet.hediffs.Last().def, hediffSet.BleedRateTotal * 0.01f);
                }
            }

        }
    }
}