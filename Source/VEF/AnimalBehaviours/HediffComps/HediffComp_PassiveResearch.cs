using System.Reflection;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse.Sound;
using UnityEngine;
using HarmonyLib;

namespace VEF.AnimalBehaviours
{
    public class HediffComp_PassiveResearch : HediffComp
    {


       
        public HediffCompProperties_PassiveResearch Props
        {
            get
            {
                return (HediffCompProperties_PassiveResearch)this.props;
            }
        }


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);


            if (Pawn.IsHashIntervalTick(Props.tickInterval, delta) && Pawn.Faction == Faction.OfPlayerSilentFail && Pawn.Map!=null)
            {
                ResearchProjectDef proj = Find.ResearchManager.GetProject();
                if (proj != null)
                {
                    Find.ResearchManager.AddProgress(proj, Props.researchPoints);
                }
            }

        }


    }
}
