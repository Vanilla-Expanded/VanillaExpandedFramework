using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    public static class Init
    {
        static Init()
        {
            Harmony harmonyInstance = new Harmony("Kikohi.KCSG");
            harmonyInstance.PatchAll();
        }
    }
}
