
using Verse;
using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using VEF.CacheClearing;


namespace VEF.Apparels
{

    public static class StaticCollectionsClass
    {

        //This static class stores pawn lists for different things.

        // A list of pawns that are wearing camouflage apparel
        public static HashSet<Thing> camouflaged_pawns = new HashSet<Thing>();

        static StaticCollectionsClass()
        {
            ClearCaches.clearCacheTypes.Add(typeof(StaticCollectionsClass));
        }

        public static void AddCamouflagedPawnToList(Thing thing)
        {

            if (!camouflaged_pawns.Contains(thing))
            {
                camouflaged_pawns.Add(thing);
            }
        }

        public static void RemoveCamouflagedPawnFromList(Thing thing)
        {
            if (camouflaged_pawns.Contains(thing))
            {
                camouflaged_pawns.Remove(thing);
            }

        }




    }
}
