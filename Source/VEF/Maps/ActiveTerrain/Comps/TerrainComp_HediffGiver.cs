using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace VEF.Maps
{
    public class TerrainCompProperties_HediffGiver : TerrainCompProperties
    {
        public List<HediffData> hediffsForHumanlike;
        public List<HediffData> hediffsForAnimals;

        public TerrainCompProperties_HediffGiver()
        {
            compClass = typeof(TerrainCompHediffGiver);
        }
    }

    public class HediffData
    {

        public HediffDef hediff;
        public int hediffLimit;
        public bool randomBodyParts = false;

    }

    public class TerrainCompHediffGiver : TerrainComp
    {
        public TerrainCompProperties_HediffGiver Props { get { return (TerrainCompProperties_HediffGiver)props; } }

        public override void CompTick()
        {
            base.CompTick();
            foreach (var pawn in this.parent.Position.GetThingList(this.parent.Map).OfType<Pawn>())
            {

                HediffData chosenHediffData = null;
                if (pawn.RaceProps.Humanlike)
                {
                    chosenHediffData =  Props.hediffsForHumanlike.Where(x => x.hediff != null)?.RandomElement();
                }else if (pawn.IsAnimal)
                {
                    chosenHediffData = Props.hediffsForAnimals.Where(x => x.hediff != null)?.RandomElement();
                }
                if (chosenHediffData != null)
                {
                    if (pawn.health.hediffSet.GetHediffCount(chosenHediffData.hediff) < chosenHediffData.hediffLimit)
                    {
                        if (chosenHediffData.randomBodyParts)
                        {
                            List<BodyPartRecord> pieces = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).ToList();
                            if (pieces != null && pieces.Count > 0)
                            {                               
                                    BodyPartRecord part = pieces.RandomElement();
                                    var hediff = HediffMaker.MakeHediff(chosenHediffData.hediff, pawn, part);
                                    pawn.health.AddHediff(hediff, part);
                                
                            }
                        }
                        else
                        {
                            pawn.health.AddHediff(chosenHediffData.hediff);
                        }                           
                    }
                }
            }
        }

        
        public override void PostExposeData()
        {
            base.PostExposeData();

        }
    }
}
