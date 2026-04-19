using Verse;
using System.Collections.Generic;
using System.Xml;

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
        public IntRange count;
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