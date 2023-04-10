using RimWorld;
using Verse;
using System.Collections.Generic;

namespace AnimalBehaviours
{
    public class CompUntameable : ThingComp
    {

        public bool externalOverride = false;

        public CompProperties_Untameable Props
        {
            get
            {
                return (CompProperties_Untameable)this.props;
            }
        }

        public override void PostExposeData()
        {
          
            Scribe_Values.Look(ref externalOverride, "externalOverride", false, true);



        }
        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.IsHashIntervalTick(500) && !externalOverride)
            {
                this.CheckFaction();
            }
        }

        public void CheckFaction()
        {
            //If I detect the creature is part of the player's faction (has been tamed)
            if (AnimalBehaviours_Settings.flagUntameable &&  this.parent.Faction == Faction.OfPlayer)
            {

                Pawn pawn = parent as Pawn;
                if (pawn != null)
                {
                    if (Props.goWild)
                    {
                        parent.SetFaction(null, null);
                    }
                    //if goesManhunter is false, the creature is just returned to the wild, no faction
                    if (!Props.goesManhunter)
                    {
                        parent.SetFaction(null, null);
                    }
                    //if goesManhunter is true and factionToReturnTo is not set, the creature is made manhunter
                    else if (Props.factionToReturnTo == "")
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                    }
                    //if goesManhunter is true and factionToReturnTo is set, the creature is made manhunter and placed on the 
                    //faction it should belong to
                    else
                    {
                        parent.SetFaction(Find.FactionManager.FirstFactionOfDef(FactionDef.Named(Props.factionToReturnTo)), null);
                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                    };
                    //Optionally a message can be sent when this happens
                    if (Props.sendMessage)
                    {
                        Messages.Message(Props.message.Translate(pawn.LabelIndefinite().CapitalizeFirst()), pawn, MessageTypeDefOf.NegativeEvent, true);
                    }
                }


            }

        }


    }
}
