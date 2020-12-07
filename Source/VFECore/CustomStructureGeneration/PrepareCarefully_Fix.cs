using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System.Reflection;

namespace KCSG
{
    [StaticConstructorOnStartup]
    static class PrepareCarefully_Util
    {
        public static Dictionary<StructureLayoutDef, bool> pcScenariosSave = new Dictionary<StructureLayoutDef, bool>();
    }

    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(RimWorld.Page_ConfigureStartingPawns))]
    [HarmonyPatch("PreOpen", MethodType.Normal)]
    public class PrepareCarefully_Fix
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            ScenPart_AddStartingStructure spart = (ScenPart_AddStartingStructure) Current.Game.Scenario.AllParts.ToList().Find(s => s.def.defName == "VFEC_AddStartingStructure");
            PrepareCarefully_Util.pcScenariosSave.Add(spart.chooseFrom.RandomElement(), spart.nearMapCenter);
        }
    }
}
