using System;
using System.Collections.Generic;
using Verse.Sound;
using Verse;
using RimWorld;
using UnityEngine;

namespace AnimalBehaviours
{
    public class CompHediffEffecter : ThingComp
    {
        public int tickCounter = 0;
        public List<Pawn> pawnList = new List<Pawn>();
        public Pawn thisPawn;


        public CompProperties_HediffEffecter Props
        {
            get
            {
                return (CompProperties_HediffEffecter)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (AnimalBehaviours_Settings.flagEffecters)
            {
                tickCounter++;
                //Only do anything every tickInterval
                if (tickCounter > Props.tickInterval)
                {

                    thisPawn = this.parent as Pawn;
                    //Null map check. Also will only work if pawn is not dead or downed
                    if (thisPawn != null && thisPawn.Map != null && !thisPawn.Dead && !thisPawn.Downed)
                    {
                        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(thisPawn.Position, thisPawn.Map, Props.radius, true))
                        {
                            Pawn pawn = thing as Pawn;
                            //Only work on colonists, unless notOnlyAffectColonists
                            if (pawn != null && (pawn.IsColonist || Props.notOnlyAffectColonists))
                            {
                                //Only work on not dead, not downed, not psychically immune colonists
                                if (!pawn.Dead && !pawn.Downed && pawn.GetStatValue(StatDefOf.PsychicSensitivity, true) > 0f)
                                {
                                    FleckMaker.AttachedOverlay(this.parent, DefDatabase<FleckDef>.GetNamed("PsycastPsychicEffect"), Vector3.zero, 1f, -1f);
                                    pawn.health.AddHediff(HediffDef.Named(Props.hediff));
                                }
                            }
                        }
                    }
                    tickCounter = 0;
                }
            }
           
        }
    }
}

