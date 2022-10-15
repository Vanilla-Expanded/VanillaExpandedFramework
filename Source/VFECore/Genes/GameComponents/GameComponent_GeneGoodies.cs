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
    public class GameComponent_GeneGoodies : GameComponent
    {

        public bool sentOncePerGame = false;

        public GameComponent_GeneGoodies(Game game)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref this.sentOncePerGame, "sentOncePerGameGenes", false, true);


        }




    }


}

