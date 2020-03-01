using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VFECore
{

    public class VFECore : Mod
    {
        public VFECore(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony("OskarPotocki.VFECore");

            // Parsing
            ParseHelper.Parsers<TechLevelRange>.Register(s => TechLevelRange.FromString(s));
        }

        public static Harmony harmonyInstance;

    }

}
