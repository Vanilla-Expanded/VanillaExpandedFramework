using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool onlyOnPlayerMap = false;
                
        public bool postGenerateOre = false;
        public float maxMineableValue = float.MaxValue;
    }

    public class LayoutCommonality
    {
        public StructureLayoutDef layout;
        public float commonality = 1f;
    }

    public class Scaling
    {
        float flat = 0.15f;
        float smallHills = 0.5f;
        float largeHills = 0.75f;
        float mountainous = 1f;

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
