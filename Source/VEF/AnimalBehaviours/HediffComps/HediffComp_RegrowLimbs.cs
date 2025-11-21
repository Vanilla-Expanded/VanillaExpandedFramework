
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.AI;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_RegrowLimbs : HediffComp
    {
        public HediffCompProperties_RegrowLimbs Props
        {
            get
            {
                return (HediffCompProperties_RegrowLimbs)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Find.TickManager.TicksGame % GenDate.TicksPerHour == 0)
            {
                bool healedOnce = false;
                var injuredHediffs = parent.pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList();

                var nonMissingParts = parent.pawn.health.hediffSet.GetNotMissingParts().ToList();
                var missingParts = parent.pawn.def.race.body.AllParts.Where(x => parent.pawn.health.hediffSet.PartIsMissing(x)
                    && nonMissingParts.Contains(x.parent) && !parent.pawn.health.hediffSet.AncestorHasDirectlyAddedParts(x)).ToList();
                if (missingParts.Any())
                {
                    var missingPart = missingParts.RandomElement();
                    var currentMissingHediffs = parent.pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList();
                    parent.pawn.health.RestorePart(missingPart);
                    var currentMissingHediffs2 = parent.pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList();
                    var removedMissingPartHediff = currentMissingHediffs.Where(x => !currentMissingHediffs2.Contains(x));
                    foreach (var missingPartHediff in removedMissingPartHediff)
                    {
                        var regeneratingHediff = HediffMaker.MakeHediff(Props.regeneratingHediff, parent.pawn, missingPartHediff.Part);
                        regeneratingHediff.Severity = missingPartHediff.Part.def.GetMaxHealth(parent.pawn) - 1;
                        parent.pawn.health.AddHediff(regeneratingHediff);
                    }
                    healedOnce = true;
                }

                if (healedOnce)
                {
                    FleckMaker.ThrowMetaIcon(parent.pawn.Position, parent.pawn.Map, FleckDefOf.HealingCross);
                }
            }

        }


    }
}
