using Verse;
using RimWorld;
using System.Collections.Generic;
using Verse.AI.Group;

namespace AnimalBehaviours
{
    public class Comp_FactionAfterHealthLoss : ThingComp
    {
        public int tickCounter = 0;
        bool SetFactionOnce = true;

        public CompProperties_FactionAfterHealthLoss Props
        {
            get
            {
                return (CompProperties_FactionAfterHealthLoss)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            tickCounter++;
            //Only check things every tickInterval
            if (tickCounter > Props.tickInterval && SetFactionOnce)
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn != null && pawn.Map != null && !pawn.Dead && !pawn.Downed)
                {
                    //If pawn's health reaches a threshold
                    if (pawn.health.summaryHealth.SummaryHealthPercent < ((float)(Props.healthPercent) / 100))
                    {
                        //if nonHostileFaction is false and factionToReturnTo is not set, the pawn is set to a random enemy faction
                        if (Props.factionToReturnTo == "" && !Props.nonHostileFaction)
                        {
                            var faction = Find.FactionManager.RandomEnemyFaction(allowNonHumanlike: false);
                            pawn.SetFaction(faction);
                            SetFactionOnce = false;
                        }
                        //if nonHostileFaction is true and factionToReturnTo is not set, the pawn is set to a random non hostile faction
                        else if (Props.factionToReturnTo == "")
                        {
                            var faction = Find.FactionManager.RandomNonHostileFaction(allowNonHumanlike: false);
                            pawn.SetFaction(faction);
                            SetFactionOnce = false;
                        }
                        //if factionToReturnTo is set, the pawn is set to the faction it should belong to
                        else
                        {
                            parent.SetFaction(Find.FactionManager.FirstFactionOfDef(FactionDef.Named(Props.factionToReturnTo)), null);
                            SetFactionOnce = false;
                        };
                        //if attackColony is true the pawn will attack the colony
                        if (Props.attackColony)
                        {
                            var map = pawn.Map;
                            LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction, true, false, false, true, true, false, true), map, new List<Pawn> { pawn });
                        }
                    }
                }
                tickCounter = 0;
            }
        }
    }
}
