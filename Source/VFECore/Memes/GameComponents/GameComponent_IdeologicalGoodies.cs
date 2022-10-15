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
    public class GameComponent_IdeologicalGoodies : GameComponent
    {

        public bool sentOncePerGame = false;

        public GameComponent_IdeologicalGoodies(Game game)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref this.sentOncePerGame, "sentOncePerGame", false, true);


        }




    }


}

