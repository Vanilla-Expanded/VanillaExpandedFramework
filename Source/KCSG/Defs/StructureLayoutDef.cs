using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class StructureLayoutDef : Def
    {
        public bool isStorage = false;
        public bool spawnConduits = true;
        public List<List<string>> layouts = new List<List<string>>();
        public List<string> roofGrid = new List<string>();
        // Settings for SettlementDef
        public List<string> tags = new List<string>();
        // Mod requirements
        [Obsolete] public bool requireRoyalty = false;
        public List<string> modRequirements = new List<string>();

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (this.requireRoyalty) this.modRequirements.Add("ludeon.rimworld.royalty");
        }
    }
}
