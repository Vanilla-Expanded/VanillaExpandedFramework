using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace VFECore
{

    public class VFECore : Mod
    {
        public VFECore(ModContentPack content) : base(content)
        {
            harmonyInstance = HarmonyInstance.Create("OskarPotocki.VFECore");

            // Parsing
            ParseHelper.Parsers<TechLevelRange>.Register(s => TechLevelRange.FromString(s));
        }

        public static HarmonyInstance harmonyInstance;

    }

}
