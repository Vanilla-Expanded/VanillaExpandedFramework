
using System.Collections.Generic;
using Verse.Sound;
using Verse;
using RimWorld;
using UnityEngine;


namespace AnimalBehaviours
{
    public class HediffComp_CorpseDecayer : HediffComp
    {


        public HediffCompProperties_CorpseDecayer Props
        {
            get
            {
                return (HediffCompProperties_CorpseDecayer)this.props;
            }
        }



        public int tickCounter = 0;
        public bool flagOnce = false;



        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);


            tickCounter++;

            //Only check every 2 rare ticks (8 seconds)
            if (tickCounter > Props.tickInterval)
            {
                Pawn pawn = this.parent.pawn as Pawn;

                //Null map check
                if (pawn.Map != null)
                {
                    //Check on radius
                    CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
                    rect = rect.ExpandedBy(Props.radius);

                    foreach (IntVec3 current in rect.Cells)
                    {
                        if (current.InBounds(pawn.Map))
                        {
                            HashSet<Thing> hashSet = new HashSet<Thing>(current.GetThingList(pawn.Map));
                            if (hashSet != null)
                            {
                                foreach (Thing thingInCell in hashSet)
                                {
                                    Corpse corpse = thingInCell as Corpse;
                                    //If anything in those cells was a corpse
                                    if (corpse != null)
                                    {
                                        //A FLESHY corpse, no mechanoid munching
                                        if (corpse.InnerPawn.def.race.IsFlesh)
                                        {
                                            //Damage the corpse, and feed the animal
                                            corpse.HitPoints -= 5;
                                            if (pawn?.needs?.food != null)
                                            {
                                                pawn.needs.food.CurLevel += Props.nutritionGained;

                                            }


                                            //This is for achievements
                                            if (ModLister.HasActiveModWithName("Alpha Animals") && (pawn.Faction == Faction.OfPlayer) && (corpse.InnerPawn.def.race.Humanlike))
                                            {
                                                pawn.health.AddHediff(HediffDef.Named("AA_CorpseFeast"));
                                            }

                                            //If the corpse can rot, do it
                                            CompRottable compRottable = corpse.TryGetComp<CompRottable>();
                                            if (compRottable.Stage == RotStage.Fresh)
                                            {
                                                compRottable.RotProgress += 100000;
                                            }
                                            //If the corpse reaches 0 HP, destroy it, and spawn corpse bile
                                            if (corpse.HitPoints < 0)
                                            {
                                                corpse.Destroy(DestroyMode.Vanish);
                                                for (int i = 0; i < 20; i++)
                                                {
                                                    IntVec3 c;
                                                    CellFinder.TryFindRandomReachableNearbyCell(pawn.Position, pawn.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                                                    FilthMaker.TryMakeFilth(c, pawn.Map, ThingDefOf.Filth_CorpseBile, pawn.LabelIndefinite(), 1, FilthSourceFlags.None);
                                                    SoundDef.Named(Props.corpseSound).PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                                                }
                                            }
                                            FilthMaker.TryMakeFilth(current, pawn.Map, ThingDefOf.Filth_CorpseBile, pawn.LabelIndefinite(), 1, FilthSourceFlags.None);
                                            //If it causes thoughts on nearby pawns, do it

                                            if (Props.causeThoughtNearby)
                                            {
                                                foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, Props.radiusForThought, true))
                                                {
                                                    Pawn affectedpawn = thing as Pawn;
                                                    if (affectedpawn != null && affectedpawn.needs?.mood?.thoughts != null && !affectedpawn.AnimalOrWildMan() && affectedpawn.RaceProps.IsFlesh && affectedpawn != pawn)
                                                    {
                                                        if (!affectedpawn.Dead && !affectedpawn.Downed && affectedpawn.GetStatValue(StatDefOf.PsychicSensitivity, true) > 0f)
                                                        {
                                                            pawn.needs.mood.thoughts.memories.TryGainMemory(Props.thought, null);

                                                        }
                                                    }
                                               }
                                            }

                                            flagOnce = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (flagOnce) { flagOnce = false; break; }
                    }
                }
                tickCounter = 0;
            }






        }





    }
}
