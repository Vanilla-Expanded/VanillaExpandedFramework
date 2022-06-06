using System;
using System.Collections.Generic;
using RimWorld;
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
        public int maxStackSize = -1;

        // Building info
        public Rot4 rotation = Rot4.North;

        // Plant
        public float plantGrowth = 0.5f;

        // CryptosleepCasket and CorpseCasket
        public float chanceToContainPawn = 1f;
        public List<PawnKindDef> containPawnKindAnyOf = new List<PawnKindDef>();
        public List<PawnKindDef> containPawnKindForPlayerAnyOf = new List<PawnKindDef>();

        // Crate
        public ThingSetMakerDef thingSetMakerDef = null;
        public ThingSetMakerDef thingSetMakerDefForPlayer = null;
        public float crateStackMultiplier = 1f;

        // Pawn
        public string pawnKindDef = null;
        public PawnKindDef pawnKindDefNS = null;
        public bool isSlave = false;
        public FactionDef faction;
        public int numberToSpawn = 1;
        public bool spawnDead = false;
        public bool spawnRotten = false;
        public bool spawnFilthAround = false;
        public bool defendSpawnPoint = false;

        [Obsolete]
        public string lordJob = null;

        public override void ResolveReferences()
        {
            if (terrain != null) terrainDef = DefDatabase<TerrainDef>.GetNamed(terrain, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (thing != null) thingDef = DefDatabase<ThingDef>.GetNamed(thing, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (stuff != null) stuffDef = DefDatabase<ThingDef>.GetNamed(stuff, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (pawnKindDef != null) pawnKindDefNS = DefDatabase<PawnKindDef>.GetNamed(pawnKindDef, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (thingSetMakerDef == null) thingSetMakerDef = ThingSetMakerDefOf.MapGen_AncientComplexRoomLoot_Default;
        }
    }
}
