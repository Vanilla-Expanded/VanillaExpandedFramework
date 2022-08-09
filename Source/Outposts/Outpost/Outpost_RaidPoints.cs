using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Outposts;

public partial class Outpost
{
    private static readonly SimpleCurve ThreatPointsOverPointsCurve = new() //A
    {
        new CurvePoint(35f, 38.5f),
        new CurvePoint(400f, 165f),
        new CurvePoint(10000f, 4125f)
    };

    private static readonly SimpleCurve ThreatPointsFactorOverPawnCountCurve = new() //B
    {
        new CurvePoint(1f, 0.5f),
        new CurvePoint(2f, 0.55f),
        new CurvePoint(5f, 1f),
        new CurvePoint(8f, 1.1f),
        new CurvePoint(20f, 2f)
    };

    private static readonly SimpleCurve ThreatPointsFactorOverLocalWealth = new() //C
    {
        new CurvePoint(1000f, 0.5f),
        new CurvePoint(24000f, 1f),
        new CurvePoint(50000f, 1.1f),
        new CurvePoint(250000f, 2f)
    };

    public virtual float WealthForCurve //using the map PlayerWealth is awkward because it requires the pawns to be spawned
    {
        get
        {
            var wealth = 0f;
            foreach (var pawn in AllPawns.Where(x => (x.RaceProps.Humanlike && !x.IsPrisoner) || x.training.CanAssignToTrain(TrainableDefOf.Release).Accepted))
            {
                wealth += WealthWatcher.GetEquipmentApparelAndInventoryWealth(pawn);
                var marketValue = pawn.MarketValue;
                if (pawn.IsSlave) marketValue *= 0.75f;
                wealth += marketValue;
            }

            return wealth;
        }
    }

    public virtual SimpleCurve ThreatCurve => ThreatPointsOverPointsCurve;

    public virtual SimpleCurve PawnCurve => ThreatPointsFactorOverPawnCountCurve;

    public virtual SimpleCurve WealthCurve => ThreatPointsFactorOverLocalWealth;

    //Basic explination of raid points. Make it harder as main colonies wealth goes up
    //Based on 3 curves and a rand range.
    //A: Colony threat points -> base points
    //B: Factor based on # local of fighting colonists/animals
    //C: factor based on Local wealth
    //D: FloatRange that reduces further
    //(A*B*C)*D
    public virtual float ResolveRaidPoints(IncidentParms parms, float rangeMin = 0.25f, float rangeMax = 0.35f)
    {
        var pointFactorRange = new FloatRange(rangeMin, rangeMax);
        var mapPoints = HasMap ? StorytellerUtility.DefaultThreatPointsNow(parms.target) : 35f; //Min points 
        float fighters = AllPawns.Count(x =>
            (x.RaceProps.Humanlike && !x.IsPrisoner) ||
            (x.training?.CanAssignToTrain(TrainableDefOf.Release).Accepted ?? false)); //Humans and fighting animals           
        var points = ThreatCurve.Evaluate(parms.points) * PawnCurve.Evaluate(fighters) * WealthCurve.Evaluate(WealthForCurve);
        points *= pointFactorRange.RandomInRange;
        //Log.Message("PreMultiPoints," + points.ToString() + "," + mapPoints.ToString());
        points = Mathf.Max(points, mapPoints) * OutpostsMod.Settings.RaidDifficultyMultiplier;
        return Mathf.Clamp(points, 35f, 10000f); //I pity whoever makes it hit 10k via settings
    }


    //Debug to test impact of colony
    public void Debug(IncidentParms parms, float rangeMin = 0.25f, float rangeMax = 0.35f)
    {
        var origParm = parms.points;
        var mapPoints = StorytellerUtility.DefaultThreatPointsNow(parms.target); //Min points 
        Log.Message("Min: " + mapPoints);
        for (var i = 1; i < 100; i++)
        {
            parms.points = 100 * i;
            var points = ResolveRaidPoints(parms, rangeMin, rangeMax);
            Log.Message("Colony Points: " + parms.points + "," + points);
        }

        parms.points = origParm;
    }
}