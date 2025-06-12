using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace VEF.Buildings
{
    public class MapComponent_InteractableBuildingsInMap : MapComponent
    {
        public HashSet<Thing> lootables_InMap = new HashSet<Thing>();
        public HashSet<Thing> studiables_InMap = new HashSet<Thing>();

        public MapComponent_InteractableBuildingsInMap(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.lootables_InMap, "lootables_InMap", LookMode.Reference);
            Scribe_Collections.Look(ref this.studiables_InMap, "studiables_InMap", LookMode.Reference);
        }

        public void AddLootableToMap(Thing thing)
        {
            if (!lootables_InMap.Contains(thing))
            {
                lootables_InMap.Add(thing);
            }
        }

        public void RemoveLootableFromMap(Thing thing)
        {
            if (lootables_InMap.Contains(thing))
            {
                lootables_InMap.Remove(thing);
            }
        }
        public void AddStudiablesToMap(Thing thing)
        {
            if (!studiables_InMap.Contains(thing))
            {
                studiables_InMap.Add(thing);
            }
        }

        public void RemoveStudiablesFromMap(Thing thing)
        {
            if (studiables_InMap.Contains(thing))
            {
                studiables_InMap.Remove(thing);
            }
        }
    }
}
