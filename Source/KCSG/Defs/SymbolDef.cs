using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace KCSG
{
    public class SymbolDef : Def
    {
        public bool spawnPartOfFaction = true;

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

        public override void ResolveReferences()
        {
            if (thing != null) thingDef = DefDatabase<ThingDef>.GetNamed(thing, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (stuff != null) stuffDef = DefDatabase<ThingDef>.GetNamed(stuff, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (pawnKindDef != null) pawnKindDefNS = DefDatabase<PawnKindDef>.GetNamed(pawnKindDef, VFECore.VFEGlobal.settings.enableVerboseLogging);
            if (thingSetMakerDef == null) thingSetMakerDef = ThingSetMakerDefOf.MapGen_AncientComplexRoomLoot_Default;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Def: {defName}");
            if (thingDef != null)
                sb.AppendInNewLine($"Thing: {thingDef.defName} | Stuff: {stuffDef?.defName} | Stack size: {maxStackSize} | Rot: {rotation}");
            if (pawnKindDefNS != null)
                sb.AppendInNewLine($"Pawn: {pawnKindDefNS.defName} | Slave: {isSlave} | Number: {numberToSpawn} | Dead: {spawnDead} | Rotten: {spawnRotten}");

            return sb.ToString();
        }
    }
}
