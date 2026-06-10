using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace VEF.Plants
{
    public class MapComponent_ExtractablePlants : MapComponent
    {

        //This class receives calls when a new blooming plant appears on the map, storing or deleting it from a List
        //This List is used on WorkGivers. They'll only look for things on the List, causing less lag

        public HashSet<Thing> objects_InMap = new HashSet<Thing>();


        public MapComponent_ExtractablePlants(Map map) : base(map)
        {

        }



        public void AddObjectToMap(Thing thing)
        {
            if (!objects_InMap.Contains(thing))
            {
                objects_InMap.Add(thing);
            }

        }

        public void RemoveObjectFromMap(Thing thing)
        {
            if (objects_InMap.Contains(thing))
            {
                objects_InMap.Remove(thing);
            }

        }


    }


}
