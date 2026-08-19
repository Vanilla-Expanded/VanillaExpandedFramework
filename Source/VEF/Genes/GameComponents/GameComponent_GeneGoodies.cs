using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace VEF.Genes
{
    public class GameComponent_GeneGoodies : GameComponent
    {
        public static GameComponent_GeneGoodies Instance;

        public bool sentOncePerGame = false;

        public GameComponent_GeneGoodies(Game game)
        {
            Instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref this.sentOncePerGame, "sentOncePerGameGenes", false, true);


        }




    }


}

