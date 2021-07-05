using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace NocturnalAnimals
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        
        static HarmonyPatches()
        {
            var h = new Harmony("XeoNovaDan.NocturnalAnimals");
            h.PatchAll();
        }

    }

}
