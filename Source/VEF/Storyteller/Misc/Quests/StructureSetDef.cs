using System;
using Verse;
using System.Collections.Generic;
using System.Xml;
using RimWorld;

namespace VEF.Storyteller
{
    public class StructureSetDef : Def
    {
        public List<StructurePatternOffset> structureLayouts;
    }

    public class StructurePatternOffset
    {
        public string pattern;
        public IntVec3 offset;
        public IntRange count = new IntRange(1, 1);
        public bool scatter;
        public int radialCount;
        public float radialDistance;
        public bool faceCenter;
        public bool randomRotated;
        public int rotationOffset;
        public bool putAnywhere;
        public List<PawnSpawnOption> spawnPawns;
        public List<ThingSpawnOption> spawnThings;
        public bool forceSpawnEnemiesIndoor;
        public bool unwaveringlyLoyal;
        public List<ThingDef> weapons;
        public FloatRange? pointsRange;
    }

    public class PawnSpawnOption
    {
        public PawnKindDef kind;
        public IntRange count = IntRange.Invalid;

        public virtual void SpawnPawns(Map map, List<IntVec3> walkableCells, CellRect structureRect, StructurePatternOffset layout, Faction faction, List<Pawn> spawnedPawns)
        {
            var range = count.RandomInRange;
            for (var i = 0; i < range; i++)
            {
                var rootCell = walkableCells.RandomElement();
                if (!rootCell.IsValid) rootCell = structureRect.CenterCell;
                var spawnCell = CellFinder.RandomSpawnCellForPawnNear(rootCell, map, 5);
                if (!spawnCell.IsValid) continue;

                var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, forceGenerateNewPawn: true));
                if (pawn.RaceProps.Humanlike && layout.weapons.NullOrEmpty() is false)
                {
                    pawn.equipment.DestroyAllEquipment();
                    pawn.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(layout.weapons.RandomElement()));
                }
                if (layout.unwaveringlyLoyal && pawn.guest != null) pawn.guest.Recruitable = false;
                GenSpawn.Spawn(pawn, spawnCell, map);
                spawnedPawns.Add(pawn);
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "kind", xmlRoot.Name);
            count = xmlRoot.FirstChild != null ? ParseHelper.FromString<IntRange>(xmlRoot.FirstChild.Value) : new IntRange(1, 1);
        }
    }

    public class ThingSpawnOption
    {
        public ThingDef thing;
        public IntRange count;
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", xmlRoot.Name);
            count = xmlRoot.FirstChild != null ? ParseHelper.FromString<IntRange>(xmlRoot.FirstChild.Value) : new IntRange(1, 1);
        }
    }
}