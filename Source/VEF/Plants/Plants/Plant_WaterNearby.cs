using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;

using Verse;

namespace VEF.Plants
{
    public class Plant_WaterNearby : Plant
    {

        const int radius = 6;
        bool waterFound = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            int num = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < num; i++)
            {
                IntVec3 c = Position + GenRadial.RadialPattern[i];
                if (c.InBounds(map))
                {
                    TerrainDef terrain = c.GetTerrain(map);

                    if (terrain != null && terrain.IsWater)
                    {
                        waterFound = true;
                        break;
                    }
                }
            }
        }

        public override float GrowthRate
        {
            get
            {
                if (Blighted)
                {
                    return 0f;
                }
                if (Spawned && !PlantUtility.GrowthSeasonNow(Position, Map, def))
                {
                    return 0f;
                }
                return GrowthRateFactor_Fertility * GrowthRateFactor_Temperature * GrowthRateFactor_Light * GrowthRateFactor_Water * GrowthRateFactor_NoxiousHaze * GrowthRateFactor_Drought;
            }
        }

        public float GrowthRateFactor_Water
        {
            get
            {
                if (waterFound)
                {
                    return 1f;
                }
                else return 0.75f;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<bool>(ref this.waterFound, "waterFound", false, false);

        }

        public override string GetInspectString()
        {
            if (GrowthRateFactor_Water == 0.75f)
            {
                return base.GetInspectString() + "\n" + "VCE_NoWaterNearby".Translate();

            }
            else
            {
                return base.GetInspectString();

            }



        }


    }
}
