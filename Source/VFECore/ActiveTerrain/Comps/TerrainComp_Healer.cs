using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class TerrainCompProperties_Healer : TerrainCompProperties
    {
        public float amountToHeal;
        public TerrainCompProperties_Healer()
        {
            compClass = typeof(TerrainComp_Healer);
        }
    }
    public class TerrainComp_Healer : TerrainComp
    {
        public TerrainCompProperties_Healer Props { get { return (TerrainCompProperties_Healer)props; } }

        public override void CompTick()
        {
            base.CompTick();
            foreach (var pawn in this.parent.Position.GetThingList(this.parent.Map).OfType<Pawn>())
            {
                var injury = FirstInjuryToThreat(pawn);
                if (injury != null)
                {
                    injury.Heal(Props.amountToHeal);
                    pawn.health.Notify_HediffChanged(injury);
                }
            }
        }

        public Hediff FirstInjuryToThreat(Pawn pawn)
        {
            var injuries = new List<Hediff_Injury>();
            pawn.health.hediffSet.GetHediffs<Hediff_Injury>(ref injuries);
            var minorInjuries = new List<Hediff>();
            var permanentInjuries = new List<Hediff>();
            foreach (var injury in injuries)
            {
                var comp = injury.TryGetComp<HediffComp_GetsPermanent>();
                if (comp is null || !comp.IsPermanent)
                {
                    minorInjuries.Add(injury);
                }
                else if (comp?.IsPermanent ?? false)
                {
                    permanentInjuries.Add(injury);
                }
            }
            if (minorInjuries.Any())
            {
                return minorInjuries.MinBy(x => x.BleedRate);
            }
            if (permanentInjuries.Any())
            {
                return permanentInjuries.MinBy(x => x.Part.def.GetMaxHealth(pawn) - pawn.health.hediffSet.GetPartHealth(x.Part));
            }
            return null;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();

        }
    }
}
