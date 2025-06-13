using RimWorld;
using Verse;

namespace VEF.Plants
{
    class Plant_NeedsOutside : Plant
    {

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
                return GrowthRateFactor_Fertility * GrowthRateFactor_Temperature * GrowthRateFactor_NoxiousHaze * GrowthRateFactor_Drought * GrowthRateFactor_OutsideAndRoofed;
            }
        }

        public float GrowthRateFactor_OutsideAndRoofed
        {
            get
            {
                Room room = this.Position.GetRoom(this.Map);

                if (room?.OutdoorsForWork == true && this.Map.roofGrid.Roofed(this.Position))
                {
                    return 1;
                }
                else return 0;


            }
        }

        public override string GetInspectString()
        {
            if (GrowthRateFactor_OutsideAndRoofed == 0)
            {
                return base.GetInspectString() + "\n" + "VCE_NeedsShade".Translate();
            }
            else
            {
                return base.GetInspectString();
            }
        }
    }
}
