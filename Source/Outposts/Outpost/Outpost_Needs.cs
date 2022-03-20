using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Outposts
{
    public partial class Outpost
    {
        public virtual void SatisfyNeeds()
        {
            for (var i = 0; i < occupants.Count; i++) SatisfyNeeds(occupants[i]);
        }

        public virtual void SatisfyNeeds(Pawn pawn)
        {
            if (pawn is null) return;
            var food = pawn.needs?.food;
            if (GenLocalDate.HourInteger(Tile) >= 23 || GenLocalDate.HourInteger(Tile) <= 5) pawn.needs?.rest?.TickResting(0.75f);
            if (!pawn.IsHashIntervalTick(300)) return;
            if (food is not null && food.CurLevelPercentage <= pawn.RaceProps.FoodLevelPercentageWantEat && ProvidedFood is {IsNutritionGivingIngestible: true} &&
                ProvidedFood.ingestible.HumanEdible)
            {
                var thing = ThingMaker.MakeThing(ProvidedFood);
                if (thing.IngestibleNow && pawn.RaceProps.CanEverEat(thing)) food.CurLevel += thing.Ingested(pawn, food.NutritionWanted);
            }

            if (pawn.health is null) return;

            if (pawn.health.HasHediffsNeedingTend())
            {
                var doctor = AllPawns.Where(p => p.RaceProps.Humanlike && !p.Downed).MaxBy(p => p.skills?.GetSkill(SkillDefOf.Medicine)?.Level ?? -1f);
                Medicine medicine = null;
                var potency = 0f;
                foreach (var thing in containedItems)
                    if (thing.def.IsMedicine && (pawn.playerSettings is null || pawn.playerSettings.medCare.AllowsMedicine(thing.def)))
                    {
                        var statValue = thing.GetStatValue(StatDefOf.MedicalPotency);
                        if (statValue > potency || medicine == null)
                        {
                            potency = statValue;
                            medicine = (Medicine) thing;
                        }
                    }

                TendUtility.DoTend(doctor, pawn, medicine);
            }

            if (pawn.health.hediffSet is null) return;
            var removedAnything = false;

            if (pawn.health.hediffSet.HasNaturallyHealingInjury())
            {
                var injury = pawn.health.hediffSet.GetHediffs<Hediff_Injury>().Where(x => x.CanHealNaturally()).RandomElement();
                injury.Heal(pawn.HealthScale * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                if (injury.ShouldRemove)
                {
                    pawn.health.hediffSet.hediffs.Remove(injury);
                    injury.PostRemoved();
                    removedAnything = true;
                }
            }

            if (pawn.health.hediffSet.HasTendedAndHealingInjury())
            {
                var injury = pawn.health.hediffSet.GetHediffs<Hediff_Injury>().Where(x => x.CanHealFromTending()).RandomElement();
                injury.Heal(GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(injury.TryGetComp<HediffComp_TendDuration>().tendQuality)) * pawn.HealthScale *
                            pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                if (injury.ShouldRemove)
                {
                    pawn.health.hediffSet.hediffs.Remove(injury);
                    injury.PostRemoved();
                    removedAnything = true;
                }
            }

            if (removedAnything) pawn.health.Notify_HediffChanged(null);
        }
    }
}