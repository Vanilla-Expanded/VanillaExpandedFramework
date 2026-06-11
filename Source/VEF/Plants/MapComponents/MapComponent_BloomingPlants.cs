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
    public class MapComponent_BloomingPlants : MapComponent
    {

        //This class receives calls when a new blooming plant appears on the map, storing or deleting it from a List
        //This List is used on WorkGivers. They'll only look for things on the List, causing less lag

        public bool alternateBloomingTextures = false;

        public HashSet<Thing> flowersOrderedForExtraction_InMap = new HashSet<Thing>();
        public HashSet<Thing> weedsOrderedForRemoval_InMap = new HashSet<Thing>();


        public MapComponent_BloomingPlants(Map map) : base(map)
        {

        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            alternateBloomingTextures = Rand.Chance(0.5f);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref alternateBloomingTextures, "alternateBloomingTextures", false);
        }

        public void AddObjectToMap(Thing thing)
        {
            if (!flowersOrderedForExtraction_InMap.Contains(thing))
            {
                flowersOrderedForExtraction_InMap.Add(thing);
            }
        }

        public void RemoveObjectFromMap(Thing thing)
        {
            if (flowersOrderedForExtraction_InMap.Contains(thing))
            {
                flowersOrderedForExtraction_InMap.Remove(thing);
            }
        }

        public void AddWeedToMap(Thing thing)
        {
            if (!weedsOrderedForRemoval_InMap.Contains(thing))
            {
                weedsOrderedForRemoval_InMap.Add(thing);
            }
        }

        public void RemoveWeedFromMap(Thing thing)
        {
            if (weedsOrderedForRemoval_InMap.Contains(thing))
            {
                weedsOrderedForRemoval_InMap.Remove(thing);
            }
        }


    }


}
