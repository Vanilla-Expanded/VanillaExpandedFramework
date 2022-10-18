using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace VanillaMemesExpanded
{
    public class MapComponent_IdeologicalGoodies : MapComponent
    {



        public MapComponent_IdeologicalGoodies(Map map) : base(map)
        {

        }



        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (!Current.Game.GetComponent<GameComponent_IdeologicalGoodies>().sentOncePerGame)
            {

                List<Thing> things = new List<Thing>();

                foreach (StartingItemsByIdeologyDef startingItems in DefDatabase<StartingItemsByIdeologyDef>.AllDefsListForReading)
                {
                    if (Current.Game?.World?.factionManager?.OfPlayer?.ideos?.PrimaryIdeo?.HasMeme(startingItems.associatedMeme)==true)
                    {
                        things.AddRange(startingItems.thingSetMaker.root.Generate());
                    }

                }
                if (things.Count > 0) { DropPodUtility.DropThingsNear(MapGenerator.PlayerStartSpot, map, things, 110); }

                if (Current.Game?.World?.factionManager?.OfPlayer?.ideos?.PrimaryIdeo?.memes != null)
                {
                    foreach (MemeDef meme in Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo.memes)
                    {
                        ExtendedMemeProperties extendedMemeProperties = meme.GetModExtension<ExtendedMemeProperties>();
                        if (extendedMemeProperties != null)
                        {
                            if (extendedMemeProperties.factionOpinionOffset != 0)
                            {
                                var factions = Find.FactionManager.AllFactions;
                                foreach (var faction in factions)
                                {
                                    faction.TryAffectGoodwillWith(Faction.OfPlayer, extendedMemeProperties.factionOpinionOffset, true, true);
                                }
                            }


                        }



                    }
                }
                

                


                Current.Game.GetComponent<GameComponent_IdeologicalGoodies>().sentOncePerGame = true;
            }


        }


    }


}

