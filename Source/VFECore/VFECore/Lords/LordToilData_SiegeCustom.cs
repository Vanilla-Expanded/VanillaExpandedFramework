using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace VFECore
{

    public class LordToilData_SiegeCustom : LordToilData_Siege
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref artilleryCounts, "artilleryBlueprintCounts", LookMode.Def, LookMode.Value, ref artilleryBlueprintCountsKeysWorkingList, ref artilleryBlueprintCountsValuesWorkingList);
        }

        private List<ThingDef> artilleryBlueprintCountsKeysWorkingList;
        private List<int> artilleryBlueprintCountsValuesWorkingList;
        public Dictionary<ThingDef, int> artilleryCounts = new Dictionary<ThingDef, int>();

    }

}
