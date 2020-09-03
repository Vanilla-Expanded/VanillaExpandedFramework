using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Verse;
using UnityEngine;
using RimWorld;
using HarmonyLib;

namespace KCSG
{
    [StaticConstructorOnStartup]
    internal static class HarmonyInit
    {
        static HarmonyInit()
        {
            Harmony.DEBUG = false;
            new Harmony("CSG.Mod").Patch(AccessTools.Method(typeof(GenStep_Settlement), "ScatterAt"), new HarmonyMethod(typeof(GenStepPatches), "Prefix"), null, null, null); //.PatchAll();
        }
    }
}
