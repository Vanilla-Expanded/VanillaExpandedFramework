using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;


namespace VEF.Factions
{

    public static class Patch_Cities_GenCity
    {

        public static class manual_RandomCityFaction_predicate
        {

            public static void Postfix(Faction x, ref bool __result)
            {
                // Also factor in our DefModExtension for if a faction can have cities
                if (__result)
                {
                    var factionDefExtension = FactionDefExtension.Get(x.def);
                    __result = factionDefExtension.hasCities;
                }
            }

        }

    }

}
