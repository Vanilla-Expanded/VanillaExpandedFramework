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
using EdB;
using System.Reflection;

namespace KCSG
{
    /*[StaticConstructorOnStartup]
    [HarmonyPatch]
    static class PrepareCarefully_Fix
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod() =>
            AccessTools.TypeByName("EdB.PrepareCarefully.HarmonyPatches").GetMethods(AccessTools.all).First(mi => mi.Name.Contains("InitNewGamePostfix"));


        [HarmonyPrefix]
        static void Prefix(out KCSG.ScenPart_AddStartingStructure __state)
        {
            __state = Current.Game.Scenario.AllParts.ToList().Find(s => s.def.defName == "VFEC_AddStartingStructure") as KCSG.ScenPart_AddStartingStructure;
            Log.Message("IN PC Prefix: " + __state.chooseFrom.RandomElement().defName);
        }

        [HarmonyPostfix]
        static void Postfix(KCSG.ScenPart_AddStartingStructure __state)
        {
            Current.Game.Scenario.AllParts.AddItem(__state);
            Log.Message("IN PC PostFix: " + __state.chooseFrom.RandomElement().defName);
        }
    }*/
}
