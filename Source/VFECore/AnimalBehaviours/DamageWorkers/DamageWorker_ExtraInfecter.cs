using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AnimalBehaviours
{

    public class DamageWorker_ExtraInfecter : DamageWorker_Cut
    {

        //A damage class that causes additional infections when causing damage. The percentage chance
        //is passed by adding a comp class, CompInfecter, to the animal using this damage class

        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
            Random random = new Random();
            CompInfecter comp = dinfo.Instigator.TryGetComp<CompInfecter>();
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.WoundInfection);

            if (hediff != null && comp.Props.worsenExistingInfection)
            {
                pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.WoundInfection, false).Severity += comp.Props.severityToAdd;
            } else

            if (pawn.GetStatValue(StatDefOf.ToxicSensitivity, true) > 0f && random.NextDouble() > ((float)(100 - comp.GetChance) / 100))
            {
                pawn.health.AddHediff(HediffDefOf.WoundInfection, dinfo.HitPart, null, null);
            }

        }
    }
}

