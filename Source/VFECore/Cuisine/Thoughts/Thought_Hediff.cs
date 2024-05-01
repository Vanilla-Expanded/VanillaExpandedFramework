using Verse;
using System;
using System.Linq;
using RimWorld;

namespace VanillaCookingExpanded
{

    //The Though_Hediff class creates a hediff as soon as a pawn receives a thought

    //But there is already a vanilla class that creates a thought when a hediff is present! Why do you do that?

    //Well, it's because eating something in RW can create a hediff, and then it can create a thought, but when you consume
    //something AS AN INGREDIENT, it can only create a Thought (for example, insect meat), but no hediffs. So this class
    //basically allows us to make ingredients that cause thoughts and hediffs

    class Thought_Hediff : Thought_Memory
    {
        //This checks that we have already added the hediff
        public bool added = false;

        //And the bool is saved to the savegame, so it is really only done once
        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.added, "added", false, false);
        }

        //Soooo, here was the problem, the Thought class doesn't have an Init() or Start() or similar method, so we hooked into
        //a method that runs ALL THE TIME, MoodOffset, and by using a simple bool variable we assured this code won't run all 
        //the time and create HUGE amounts of lag
        public override float MoodOffset()
        {
            if (!added)
            {

                if (!ThoughtUtility.ThoughtNullified(pawn, def))
                {
                    //First, we use the hediff field of the Thought XML to add that hediff
                    if (this.def.hediff != null)
                    {
                        this.pawn.health.AddHediff(this.def.hediff);
                    }
                    //And if we want to add more hediffs, we can add up to two more. This could be extended to any number using
                    //a simple list
                    if (this.def.HasModExtension<Thought_Hediff_Extension>())
                    {
                        Thought_Hediff_Extension extension = this.def.GetModExtension<Thought_Hediff_Extension>();
                        if (extension.hediffToAffect != null)
                        {
                            BodyPartRecord part = this.pawn.RaceProps.body.GetPartsWithDef(extension.partToAffect).FirstOrDefault();

                            this.pawn.health.AddHediff(extension.hediffToAffect, part);
                            pawn.health.hediffSet.GetFirstHediffOfDef(extension.hediffToAffect, false).Severity += extension.percentage;

                        }

                        if (extension.secondHediffToAffect != null)
                        {
                            BodyPartRecord part2 = this.pawn.RaceProps.body.GetPartsWithDef(extension.secondPartToAffect).FirstOrDefault();

                            this.pawn.health.AddHediff(extension.secondHediffToAffect, part2);
                            pawn.health.hediffSet.GetFirstHediffOfDef(extension.secondHediffToAffect, false).Severity += extension.secondPercentage;


                        }
                        if (extension.increaseJoy)
                        {
                            pawn.needs?.joy?.GainJoy(extension.extraJoy, JoyKindDefOf.Gluttonous);

                        }




                    }
                }

                



                added = true;
            }


            return base.MoodOffset();
        }


    }
}
