using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace VanillaGenesExpanded
{
    public class MapComponent_GeneGoodies : MapComponent
    {



        public MapComponent_GeneGoodies(Map map) : base(map)
        {

        }



        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (!Current.Game.GetComponent<GameComponent_GeneGoodies>().sentOncePerGame)
            {

                List<Thing> things = new List<Thing>();

                foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonists)
                {
                    foreach (Gene gene in pawn.genes.GenesListForReading)
                    {
                        GeneExtension extension = gene.def.GetModExtension<GeneExtension>();
                        if (extension?.thingSetMaker != null)
                        {
                            things.AddRange(extension?.thingSetMaker.root.Generate());
                        }
                    }              

                }
                if (things.Count > 0) { DropPodUtility.DropThingsNear(MapGenerator.PlayerStartSpot, map, things, 110); }

                




                Current.Game.GetComponent<GameComponent_GeneGoodies>().sentOncePerGame = true;
            }


        }


    }


}

