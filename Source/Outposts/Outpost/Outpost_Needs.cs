using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Outposts;

public partial class Outpost
{
    private List<Hediff_Injury> tmpHediffInjuries = [];
    private List<Hediff_MissingPart> tmpHediffMissing = [];
    
    public virtual void SatisfyNeeds()
    {
        for (var i = 0; i < occupants.Count; i++) SatisfyNeeds(occupants[i]);
    }

    public virtual void SatisfyNeedsInterval(int delta)
    {
        for (var i = 0; i < occupants.Count; i++) SatisfyNeedsInterval(occupants[i], delta);
    }

    public virtual void SatisfyNeeds(Pawn pawn)
    {
        if (pawn is null || pawn.Spawned || pawn.Dead) return;

        OutpostHealthTick(pawn);
        if (pawn.Dead)
        {
            occupants.Remove(pawn);
            containedItems.Add(pawn.Corpse);
        }
    }

    public virtual void SatisfyNeedsInterval(Pawn pawn, int delta)
    {
        if (pawn is null || pawn.Spawned || pawn.Dead) return;
        if (GenLocalDate.HourInteger(Tile) >= 23 || GenLocalDate.HourInteger(Tile) <= 5) pawn.needs?.rest?.TickResting(0.75f * delta);

        pawn.ageTracker?.AgeTickInterval(delta); //Making pawns age
        //Seperated out Health
        OutpostHealthTickInterval(pawn, delta);

        if (!pawn.IsHashIntervalTick(300, delta)) return;
        var food = pawn.needs?.food;
        if (food is not null && food.CurLevelPercentage <= pawn.RaceProps.FoodLevelPercentageWantEat &&
            ProvidedFood is { IsNutritionGivingIngestible: true } &&
            ProvidedFood.ingestible.HumanEdible)
        {
            var thing = ThingMaker.MakeThing(ProvidedFood);
            if (thing.IngestibleNow && pawn.RaceProps.CanEverEat(thing)) food.CurLevel += thing.Ingested(pawn, food.NutritionWanted);
        }

        if (pawn.needs != null)
        {
            foreach (var need in pawn.needs.needs)
            {
                if (need is Need_Chemical or Need_Chemical_Any)
                {
                    need.CurLevel = need.MaxLevel;
                }
            }
        }

    }

    public virtual void OutpostHealthTick(Pawn pawn)
    {
        if (pawn.health?.hediffSet == null || pawn.Dead) return;

        var removedAnything = false;
        var health = pawn.health;
        //Hediff ticks and immunes
        for (var index = health.hediffSet.hediffs.Count - 1; index >= 0; index--)
        {
            var hediff = health.hediffSet.hediffs[index];
            if (hediff is Hediff_ChemicalDependency or Hediff_Addiction)
            {
                hediff.Severity = 0;
            }
            else
            {
                try
                {
                    hediff.Tick();
                    hediff.PostTick();
                }
                catch
                {
                    health.RemoveHediff(hediff);
                }
            }

            if (pawn.Dead) return;
            if (hediff.ShouldRemove)
            {
                health.hediffSet.hediffs.RemoveAt(index);
                hediff.PostRemoved();
                removedAnything = true;
            }
        }

        if (removedAnything)
            health.Notify_HediffChanged(null);
    }

    //Profiled with 7 outposts 62 pawns
    //my "light" health tick Max 0.308 avg 0.122
    //pawn Health Tick 0.344 max avg 0.152
    //Just wounds 0.201 max avg 0.065
    //Light vs Health tick makes marginal difference. Though I might still leave it because health tick will cause disease traits which unesecarry and does have a bit of bloat
    //With Just wounds people are basically in stasis which leads to awkward situations. Like a pawn I had who had anesthia woozy still a year later when it got raided
    //In my mind the impact of performance is small enough that the "realism" is worth it.
    public virtual void OutpostHealthTickInterval(Pawn pawn, int delta)
    {
        if (pawn.health?.hediffSet == null || pawn.Dead) return; //Just in case the birthday killed them XD

        var removedAnything = false;
        var health = pawn.health;
        //Hediff ticks and immunes
        for (var index = health.hediffSet.hediffs.Count - 1; index >= 0; index--)
        {
            var hediff = health.hediffSet.hediffs[index];
            if (hediff is not (Hediff_ChemicalDependency or Hediff_Addiction))
            {
                try
                {
                    hediff.TickInterval(delta);
                    hediff.PostTickInterval(delta);
                }
                catch
                {
                    health.RemoveHediff(hediff);
                }
            }

            if (pawn.Dead) return;
            if (hediff.ShouldRemove)
            {
                health.hediffSet.hediffs.RemoveAt(index);
                hediff.PostRemoved();
                removedAnything = true;
            }
        }

        if (removedAnything)
        {
            health.Notify_HediffChanged(null);
            removedAnything = false;
        }

        health.immunity.ImmunityHandlerTickInterval(delta);

        //Tend and injuries
        //Changed interval to 600 as thats what health tick is, and ppl seemed to heal super fast
        if (pawn.IsHashIntervalTick(600, delta))
        {
            if (pawn.health.HasHediffsNeedingTend())
            {
                var doctor = AllPawns.Where(p => p.RaceProps.Humanlike && !p.Downed).MaxBy(p => p.skills?.GetSkill(SkillDefOf.Medicine)?.Level ?? -1f);
                if (doctor != null)
                {
                    Medicine medicine = null;
                    var potency = 0f;
                    CheckNoDestroyedOrNoStack();
                    foreach (var thing in containedItems.ToList())
                        if (thing.def.IsMedicine && (pawn.playerSettings is null || pawn.playerSettings.medCare.AllowsMedicine(thing.def)))
                        {
                            var statValue = thing.GetStatValue(StatDefOf.MedicalPotency);
                            if (statValue > potency || medicine == null)
                            {
                                potency = statValue;
                                medicine = (Medicine)TakeItem(thing);
                            }
                        }

                    TendUtility.DoTend(doctor, pawn, medicine);
                }
            }


            if (pawn.health.hediffSet.HasNaturallyHealingInjury())
            {
                // 8 base, +4 for laying down, +4 for normal bed
                var naturalHealingFactor = 16f;

                foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
                    var stage = hediff.CurStage;
                    if (stage != null && stage.naturalHealingFactor != -1f)
                        naturalHealingFactor *= stage.naturalHealingFactor;
                }

                pawn.health.hediffSet.GetHediffs(ref tmpHediffInjuries, x => x.CanHealNaturally());
                var injury = tmpHediffInjuries.RandomElement();
                injury.Heal(naturalHealingFactor * pawn.HealthScale * 0.01f * pawn.GetStatValue(StatDefOf.InjuryHealingFactor));
                if (injury.ShouldRemove)
                {
                    pawn.health.hediffSet.hediffs.Remove(injury);
                    injury.PostRemoved();
                    removedAnything = true;
                }
            }

            if (pawn.health.hediffSet.HasTendedAndHealingInjury())
            {
                pawn.health.hediffSet.GetHediffs(ref tmpHediffInjuries, x => x.CanHealFromTending());
                var injury = tmpHediffInjuries.RandomElement();
                injury.Heal(8f * 0.01f * GenMath.LerpDouble(0f, 1f, 0.5f, 1.5f, Mathf.Clamp01(injury.TryGetComp<HediffComp_TendDuration>().tendQuality)) * pawn.HealthScale *
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

        if (pawn.IsHashIntervalTick(15, delta) && pawn.health.hediffSet.HasRegeneration)
        {
            var regeneration = 0f;

            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                var stage = hediff.CurStage;
                // TODO: Remove the -1 check if default becomes 0 in vanilla.
                if (stage != null && stage.regeneration != -1f)
                    regeneration += stage.regeneration;
            }

            regeneration *= 15f / GenDate.TicksPerDay;
            if (regeneration > 0f)
            {
                pawn.health.hediffSet.GetHediffs(ref tmpHediffInjuries);
                foreach (var injury in tmpHediffInjuries)
                {
                    var val = Mathf.Min(regeneration, injury.Severity);
                    regeneration -= val;
                    injury.Heal(val);
                    pawn.health.hediffSet.Notify_Regenerated(val);
                    if (regeneration <= 0f)
                        break;
                }

                if (regeneration > 0f)
                {
                    pawn.health.hediffSet.GetHediffs(ref tmpHediffMissing,
                        hediff => hediff.Part.parent != null &&
                                  !tmpHediffMissing.Any(x => x.Part == hediff.Part.parent) &&
                                  pawn.health.hediffSet.GetFirstHediffMatchingPart<Hediff_MissingPart>(hediff.Part.parent) == null &&
                                  pawn.health.hediffSet.GetFirstHediffMatchingPart<Hediff_AddedPart>(hediff.Part.parent) == null);

                    foreach (var missing in tmpHediffMissing)
                    {
                        var part = missing.Part;
                        pawn.health.RemoveHediff(missing);

                        var newHediff = pawn.health.AddHediff(HediffDefOf.Misc, part);
                        var partHealth = pawn.health.hediffSet.GetPartHealth(part);

                        newHediff.Severity = Mathf.Max(partHealth - 1f, partHealth * 0.9f);
                        pawn.health.hediffSet.Notify_Regenerated(partHealth - newHediff.Severity);
                    }
                }
            }
        }
    }
}