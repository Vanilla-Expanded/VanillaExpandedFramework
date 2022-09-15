using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace KCSG
{
    public class BiomeStructGenExtension : DefModExtension
    {
        public List<LayoutCommonality> structures = new List<LayoutCommonality>();

        public int spawnCount = 1;
        public bool countScaleHiliness = false;
        public Scaling scalingOptions = new Scaling();

        public bool canSpawnInMontains = true;
        public int clearCellRadiusAround = 0;

        public bool canSpawnOnWaterTerrain = true;

        public bool onlyOnPlayerMap = false;

        public bool postGenerateOre = false;
        public float maxMineableValue = float.MaxValue;
    }

    public class Scaling
    {
        readonly float flat = 0.15f;
        readonly float smallHills = 0.5f;
        readonly float largeHills = 0.75f;
        readonly float mountainous = 1f;

        public int GetScalingFor(Map map, int count)
        {
            switch (Find.WorldGrid[map.Tile].hilliness)
            {
                case Hilliness.Flat:
                    return (int)(count * flat);
                case Hilliness.SmallHills:
                    return (int)(count * smallHills);
                case Hilliness.LargeHills:
                    return (int)(count * largeHills);
                case Hilliness.Mountainous:
                    return (int)(count * mountainous);
            }
            return 0;
        }
    }
}
