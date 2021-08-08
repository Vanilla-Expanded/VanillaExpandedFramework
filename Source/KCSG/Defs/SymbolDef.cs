using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class SymbolDef : Def
    {
        // Terrain
        public bool isTerrain = false;
        public TerrainDef terrainDef = null;
        public string terrain = null;

        // Building & item basic infos
        public ThingDef thingDef = null;
        public string thing = null;
        public ThingDef stuffDef = null;
        public string stuff = null;

        // Building info
        public Rot4 rotation = Rot4.Invalid;
        
        // Plant
        public float plantGrowth = 0.5f;

        // Content holder info
        public PawnKindDef containPawnKindDef;
        public string containPawnKind;

        // Pawn
        public string pawnKindDef = null;
        public PawnKindDef pawnKindDefNS = null;
        public Type lordJob = null;
        public bool isSlave = false;
        public bool spawnPartOfFaction = true;
        public FactionDef faction;
        public int numberToSpawn = 1;

        public override void ResolveReferences()
        {
            if (this.terrain != null) this.terrainDef = DefDatabase<TerrainDef>.GetNamed(this.terrain, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (this.thing != null) this.thingDef = DefDatabase<ThingDef>.GetNamed(this.thing, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (this.stuff != null) this.stuffDef = DefDatabase<ThingDef>.GetNamed(this.stuff, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (this.containPawnKind != null) this.containPawnKindDef = DefDatabase<PawnKindDef>.GetNamed(this.containPawnKind, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (this.pawnKindDef != null) this.pawnKindDefNS = DefDatabase<PawnKindDef>.GetNamed(this.pawnKindDef, VFECore.VFEGlobal.settings.enableVerboseLogging);
        }
    }
}
