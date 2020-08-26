


using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace VanillaCookingExpanded
{
    class Thought_Hediff : Thought_Memory
    {
        public bool added = false;


        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.added, "added", false, false);
        }

        public override float MoodOffset()
        {
            if (!added)
            {
                if (this.def.hediff != null) { this.pawn.health.AddHediff(this.def.hediff); }

                if (this.def.HasModExtension<Thought_Hediff_Extension>())
                {
                    Thought_Hediff_Extension extension = this.def.GetModExtension<Thought_Hediff_Extension>();
                    BodyPartRecord part = this.pawn.RaceProps.body.GetPartsWithDef(extension.partToAffect).FirstOrDefault();
                    this.pawn.health.AddHediff(extension.hediffToAffect, part);
                    pawn.health.hediffSet.GetFirstHediffOfDef(extension.hediffToAffect, false).Severity += extension.percentage;

                    if (extension.secondHediffToAffect != null)
                    {
                        BodyPartRecord part2 = this.pawn.RaceProps.body.GetPartsWithDef(extension.secondPartToAffect).FirstOrDefault();
                        this.pawn.health.AddHediff(extension.secondHediffToAffect, part2);
                        pawn.health.hediffSet.GetFirstHediffOfDef(extension.secondHediffToAffect, false).Severity += extension.secondPercentage;
                    }



                }
                added = true;
            }


            return base.MoodOffset();
        }


    }
}
