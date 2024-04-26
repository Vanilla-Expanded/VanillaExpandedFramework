using System;
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
            if (!pawn.IsGhoul) {
                Random random = new Random();
                CompInfecter comp = dinfo.Instigator?.TryGetComp<CompInfecter>();
                Hediff hediff = pawn?.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.WoundInfection);

                if (hediff != null && comp?.Props.worsenExistingInfection == true)
                {
                    pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.WoundInfection, false).Severity += comp.Props.severityToAdd;
                }
                else if (pawn.GetStatValue(StatDefOf.ToxicResistance, true) < 1f && random.NextDouble() > ((float)(100 - comp.GetChance) / 100))
                {
                    pawn.health.AddHediff(HediffDefOf.WoundInfection, dinfo.HitPart, null, null);
                }

            }
            
        }
    }
}