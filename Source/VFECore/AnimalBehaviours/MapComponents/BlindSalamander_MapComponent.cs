using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace AnimalBehaviours
{
    public class BlindSalamander_MapComponent : MapComponent
    {

        public const int checkingInterval = 1000;
        public int tickCounter = 0;
        public List<string> salamanderGraphics = new List<string>(){"Things/Pawn/Animal/VAE_BlindSalamander/VAE_BlindSalamanderBlack",
            "Things/Pawn/Animal/VAE_BlindSalamander/VAE_BlindSalamanderBlue","Things/Pawn/Animal/VAE_BlindSalamander/VAE_BlindSalamanderRed",
            "Things/Pawn/Animal/VAE_BlindSalamander/VAE_BlindSalamanderOrange","Things/Pawn/Animal/VAE_BlindSalamander/VAE_BlindSalamanderYellow" };

        public BlindSalamander_MapComponent(Map map) : base(map)
        {

        }

        public override void FinalizeInit()
        {

            base.FinalizeInit();

        }
        public override void MapComponentTick()
        {
            base.MapComponentTick();


            if (tickCounter > checkingInterval)
            {
                if (AnimalCollectionClass.salamander_graphics.Count > 0)
                {

                    if (AnimalCollectionClass.salamander_graphics.Values.Intersect(salamanderGraphics).Count() >= 5)
                    {

                        foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists)
                        {

                            if (DefDatabase<ThoughtDef>.GetNamedSilentFail("VAE_SalamanderThought") != null)
                            {
                                if (!pawn.story.traits.HasTrait(TraitDefOf.Psychopath)) {

                                    bool flag = false;
                                    foreach (Thing thing in AnimalCollectionClass.salamander_graphics.Keys)
                                    {
                                        if (pawn.relations.DirectRelationExists(PawnRelationDefOf.Bond, (Pawn)thing))
                                        {
                                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("VAE_SalamanderThoughtBonded"), null);
                                            flag = true;
                                        }
                                    }
                                    if (!flag) { pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("VAE_SalamanderThought"), null); }

                                }

                            }


                        }
                    }
                    else
                    {
                        foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists)
                        {

                            if (DefDatabase<ThoughtDef>.GetNamedSilentFail("VAE_SalamanderThought") != null)
                            {
                                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("VAE_SalamanderThought"));
                                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDef.Named("VAE_SalamanderThoughtBonded"));
                            }


                        }

                    }


                }


                tickCounter = 0;
            }
            tickCounter++;

        }


    }


}
