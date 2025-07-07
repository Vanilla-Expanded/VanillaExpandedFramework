
using System.Collections.Generic;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;

namespace VEF.AnimalBehaviours
{

    public class CompGiveThoughtsOnCaravan : ThingComp
    {

        public CompProperties_GiveThoughtsOnCaravan Props => (CompProperties_GiveThoughtsOnCaravan)props;


        public override void CompTickInterval(int delta)
        {
            if (parent.IsHashIntervalTick(Props.intervalTicks, delta))
            {



                Pawn pawn = parent as Pawn;

                var caravan = pawn.GetCaravan();
                if (caravan != null)
                {

                    foreach (Pawn pawnMember in caravan.PawnsListForReading)
                    {

                        if (!Props.causeNegativeAtRandom) {

                            pawnMember.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.thought, pawn);

                        }
                        else
                        {
                            if (Rand.Chance(Props.randomNegativeChance))
                            {
                                if (pawnMember.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(Props.thought) == null)
                                {
                                    pawnMember.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.negativeThought, pawn);
                                }
                            }
                            else {
                                if (pawnMember.needs?.mood?.thoughts?.memories?.GetFirstMemoryOfDef(Props.negativeThought)==null)
                                {
                                    pawnMember.needs?.mood?.thoughts?.memories?.TryGainMemory(Props.thought, pawn);
                                }

                            }



                        }
                        
                        
                        
                        

                    }



                }


            }
        }








    }
}