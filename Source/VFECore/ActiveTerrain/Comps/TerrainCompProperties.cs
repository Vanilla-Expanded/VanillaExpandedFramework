using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace VFECore
{
    public class TerrainCompProperties_HeatPush : TerrainCompProperties
    {
        public TerrainCompProperties_HeatPush()
        {
            compClass = typeof(TerrainComp_HeatPush);
        }
        public float pushAmount;
        public int targetTemp = 5000;
    }
    public class TerrainCompProperties_SelfClean : TerrainCompProperties
    {
        public TerrainCompProperties_SelfClean()
        {
            compClass = typeof(TerrainComp_SelfClean);
        }
    }
    public class TerrainCompProperties_PowerTrader : TerrainCompProperties
    {
        public TerrainCompProperties_PowerTrader()
        {
            compClass = typeof(TerrainComp_PowerTrader);
        }
        public float basePowerConsumption;
        public bool ignoreNeedsPower;
    }
    public class TerrainCompProperties_TempControl : TerrainCompProperties
    {
        public TerrainCompProperties_TempControl()
        {
            compClass = typeof(TerrainComp_TempControl);
        }
        public float energyPerSecond;
        public bool reliesOnPower = true;
        public float lowPowerConsumptionFactor = 0.2f;
        public bool cleansSnow = true;
        public float snowMeltAmountPerSecond = 0.02f;
    }
    public class TerrainCompProperties_Glower : TerrainCompProperties
    {
        public TerrainCompProperties_Glower()
        {
            compClass = typeof(TerrainComp_Glower);
        }

        public float overlightRadius;

        public float glowRadius = 14f;

        public ColorInt glowColor = new ColorInt(255, 255, 255, 0) * 1.45f;

        public bool powered = true;
    }
}
