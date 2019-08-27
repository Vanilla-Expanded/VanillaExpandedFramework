using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace VFECore
{

    public class FactionGoodwillModifier : WorldComponent
    {

        public FactionGoodwillModifier(World world) : base(world)
        {
        }

        public override void FinalizeInit()
        {
            ScenPartUtility.goodwillScenParts = null;
            foreach (var faction in DefDatabase<FactionDef>.AllDefs)
                ScenPartUtility.FinaliseFactionGoodwillCharacteristics(faction);
        }

    }

}
