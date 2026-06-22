using System.Linq;
using RimWorld;
using Verse;

namespace VEF.Plants
{
    //This class is not referenced by XML in the Framework itself, but since it interacts directly
    //with the weeds system in blooming plants it is implemented here. It's up to mod to create an
    //IncidentDef that uses it

    public class IncidentWorker_Weeds : IncidentWorker
    {
        private const float Radius = 11f;

        private static readonly SimpleCurve WeedChancePerRadius = new SimpleCurve
    {
        new CurvePoint(0f, 1f),
        new CurvePoint(8f, 1f),
        new CurvePoint(11f, 0.3f)
    };

        private static readonly SimpleCurve RadiusFactorPerPointsCurve = new SimpleCurve
    {
        new CurvePoint(100f, 0.6f),
        new CurvePoint(500f, 1f),
        new CurvePoint(2000f, 2f)
    };

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Plant_Blooming plant;
            return TryFindRandomWeedablePlant((Map)parms.target, out plant);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            float num = RadiusFactorPerPointsCurve.Evaluate(parms.points);
            if (!TryFindRandomWeedablePlant(map, out var plant))
            {
                return false;
            }
            Room room = plant.GetRoom();
            int i = 0;
            for (int num2 = GenRadial.NumCellsInRadius(Radius * num); i < num2; i++)
            {
                IntVec3 intVec = plant.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map) && intVec.GetRoom(map) == room)
                {
                    Plant_Blooming firstWeedableNowPlant = GetFirstWeedableNowPlant(intVec, map);
                    if (firstWeedableNowPlant != null && Rand.Chance(WeedChance(firstWeedableNowPlant.Position, plant.Position, num)))
                    {
                        firstWeedableNowPlant.hasWeeds=true;
                    }
                }
            }
            SendStandardLetter("VEF_LetterLabelWeeds".Translate(new NamedArgument(plant.def, "PLANTDEF")), "VEF_LetterWeeds".Translate(new NamedArgument(plant.def, "PLANTDEF")), LetterDefOf.NegativeEvent, parms, new TargetInfo(plant.Position, map));
            return true;
        }

        private bool TryFindRandomWeedablePlant(Map map, out Plant_Blooming plant)
        {
            Thing result;
            bool result2 = (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
                            where (x is Plant_Blooming plant_Blooming && plant_Blooming?.GetExtension.ImmuneToWeeds==false)
                            select x).TryRandomElement(out result);
            plant = (Plant_Blooming)result;
            return result2;
        }

        private float WeedChance(IntVec3 c, IntVec3 root, float radiusFactor)
        {
            float x = c.DistanceTo(root) / radiusFactor;
            return WeedChancePerRadius.Evaluate(x);
        }

        public Plant_Blooming GetFirstWeedableNowPlant(IntVec3 c, Map map)
        {
            Plant_Blooming plant = c.GetPlant(map) as Plant_Blooming;
            if (plant != null && !plant.hasWeeds && plant?.GetExtension.ImmuneToWeeds == false)
            {
                return plant;
            }
            return null;
        }
    }
}