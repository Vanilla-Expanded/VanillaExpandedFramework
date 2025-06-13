using RimWorld;
using Verse;

namespace VEF.Plants
{
    public class Plant_ChecksSea : Plant
    {

        const int radius = 6;
        int numberOfSea = 0;

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

                    if (terrain != null && terrain.IsWater && !terrain.IsRiver)
                    {
                        numberOfSea++;
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
                return GrowthRateFactor_Fertility * GrowthRateFactor_Temperature * GrowthRateFactor_Light* GrowthRateFactor_NoxiousHaze * GrowthRateFactor_Drought * GrowthRateFactor_Sea;
            }
        }

        public float GrowthRateFactor_Sea
        {
            get
            {
                float rate = 1f + (0.01f * numberOfSea);
                if (rate > 1.3)
                {
                    return 1.3f;
                }
                else return 1f + (0.01f * numberOfSea);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref this.numberOfSea, "numberOfSea", 0, false);

        }

        public override string GetInspectString()
        {
            if (GrowthRateFactor_Sea == 1f)
            {
                return base.GetInspectString() + "\n" + "VCE_NoSeaTilesNearby".Translate();

            }
            else
            {
                return base.GetInspectString() + "\n" + "VCE_SeaTilesNearby".Translate(numberOfSea);
            }



        }


    }
}
