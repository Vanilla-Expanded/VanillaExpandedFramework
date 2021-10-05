using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class SymbolDef : Def
    {
        public bool spawnPartOfFaction = true;

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

        // CryptosleepCasket and CorpseCasket
        public float chanceToContainPawn = 1f;
        public List<PawnKindDef> containPawnKindAnyOf = new List<PawnKindDef>();
        public List<PawnKindDef> containPawnKindForPlayerAnyOf = new List<PawnKindDef>();

        /* --- Obsolete --- */
        public string containPawnKind;
        public string containPawnKindForPlayer;
        public PawnKindDef containPawnKindDef = null;
        public PawnKindDef containPawnKindDefForPlayer = null;
        /* --- ------- --- */

        // Crate
        public ThingSetMakerDef thingSetMakerDef = null;
        public ThingSetMakerDef thingSetMakerDefForPlayer = null;
        public float crateStackMultiplier = 1f;

        // Pawn
        public string pawnKindDef = null;
        public PawnKindDef pawnKindDefNS = null;
        public Type lordJob = null;
        public bool isSlave = false;
        public FactionDef faction;
        public int numberToSpawn = 1;

        public override void ResolveReferences()
        {
            if (this.terrain != null) this.terrainDef = DefDatabase<TerrainDef>.GetNamed(this.terrain, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (this.thing != null) this.thingDef = DefDatabase<ThingDef>.GetNamed(this.thing, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (this.stuff != null) this.stuffDef = DefDatabase<ThingDef>.GetNamed(this.stuff, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (this.pawnKindDef != null) this.pawnKindDefNS = DefDatabase<PawnKindDef>.GetNamed(this.pawnKindDef, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (this.thingSetMakerDef == null) this.thingSetMakerDef = ThingSetMakerDefOf.MapGen_AncientComplexRoomLoot_Default;

            // Obsolete
            if (this.containPawnKind != null)
            {
                this.containPawnKindDef = DefDatabase<PawnKindDef>.GetNamed(this.containPawnKind, VFECore.VFEGlobal.settings.enableVerboseLogging);
                Log.Warning($"{this.defName} is using obsolete field containPawnKind. Report this to {this.modContentPack.Name}");
            }
            if (this.containPawnKindForPlayer != null)
            {
                this.containPawnKindDefForPlayer = DefDatabase<PawnKindDef>.GetNamed(this.containPawnKindForPlayer, VFECore.VFEGlobal.settings.enableVerboseLogging);
                Log.Warning($"{this.defName} is using obsolete field containPawnKindDefForPlayer. Report this to {this.modContentPack.Name}");
            }
        }
    }
}
