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

          
            if (Pawn.IsHashIntervalTick(Props.tickInterval, delta))
            {
                ResearchProjectDef proj = Find.ResearchManager.GetProject();
                if (proj != null)
                {
                    FieldInfo fieldInfo = AccessTools.Field(typeof(ResearchManager), "progress");
                    Dictionary<ResearchProjectDef, float> dictionary = fieldInfo.GetValue(Find.ResearchManager) as Dictionary<ResearchProjectDef, float>;
                    if (dictionary.ContainsKey(proj))
                    {
                        dictionary[proj] += Props.researchPoints;
                    }
                    if (proj.IsFinished)
                    {
                        Find.ResearchManager.FinishProject(proj, doCompletionDialog: true);
                    }
                }
            }

        }


    }
}
