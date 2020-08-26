using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace ItemProcessor
{
    public class ItemProcessor_MapComponent : MapComponent
    {

        //This class receives calls when a new item processor is built or destroyed, storing or deleting it from a List
        //This List is used on WorkGivers. They'll only look for things on the List, causing less lag

        public HashSet<Thing> itemProcessors_InMap = new HashSet<Thing>();


        public ItemProcessor_MapComponent(Map map) : base(map)
        {

        }

        public override void FinalizeInit()
        {

            base.FinalizeInit();

        }

        public void AddItemProcessorToMap(Thing thing)
        {
            if (!itemProcessors_InMap.Contains(thing))
            {
                itemProcessors_InMap.Add(thing);
            }
        }

        public void RemoveItemProcessorFromMap(Thing thing)
        {
            if (itemProcessors_InMap.Contains(thing))
            {
                itemProcessors_InMap.Remove(thing);
            }

        }


    }


}
