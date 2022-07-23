using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RimWorld;
using Verse;

namespace Outposts
{
    public partial class Outpost
    {
        //Basic explination of raid points. Make it harder as main colonies wealth goes up
        //Based on 3 curves and a rand range.
        //A: Colony threat points -> base points
        //B: Factor based on # local of fighting colonists/animals
        //C: factor based on Local wealth
        //D: FloatRange that reduces further
        //(A*B*C)*D
        public virtual float ResolveRaidPoints(IncidentParms parms,float rangeMin = 0.25f, float rangeMax = 0.35f)        
        {            
            FloatRange pointFactorRange = new FloatRange(rangeMin, rangeMax);
            float mapPoints = HasMap ? StorytellerUtility.DefaultThreatPointsNow(parms.target) : 35f;//Min points 
            float fighters = AllPawns.Where(x => (x.RaceProps.Humanlike && !x.IsPrisoner )  || x.training.CanAssignToTrain(TrainableDefOf.Release).Accepted).Count();//Humans and fighting animals           
            float points = ThreatCurve.Evaluate(parms.points) * PawnCurve.Evaluate(fighters)* WealthCurve.Evaluate(WealthForCurve);
            points *= pointFactorRange.RandomInRange;
            //Log.Message("PreMultiPoints," + points.ToString() + "," + mapPoints.ToString());
            points = Mathf.Max(points, mapPoints) * OutpostsMod.Settings.RaidDifficultyMultiplier;            
            return Mathf.Clamp(points, 35f,10000f);//I pitty whoever makes it hit 10k via settings
        }
        
        public virtual float WealthForCurve//using the map PlayerWealth is awkward because it requires the pawns to be spawned
        {
            get
            {
                float wealth = 0f;
                foreach (var pawn in AllPawns.Where(x => (x.RaceProps.Humanlike && !x.IsPrisoner) || x.training.CanAssignToTrain(TrainableDefOf.Release).Accepted))
                {
                    wealth += WealthWatcher.GetEquipmentApparelAndInventoryWealth(pawn);
                    float marketValue = pawn.MarketValue;
                    if (pawn.IsSlave)
                    {
                        marketValue *= 0.75f;
                    }
                    wealth += marketValue;
                }
                return wealth;
            }

        }
        public virtual SimpleCurve ThreatCurve
        {
            get
            {
                return ThreatPointsOverPointsCurve;
            }
        }
        public virtual SimpleCurve PawnCurve
        {
            get
            {
                return ThreatPointsFactorOverPawnCountCurve;
            }
        }
        public virtual SimpleCurve WealthCurve
        {
            get
            {
                return ThreatPointsFactorOverLocalWealth;
            }
        }


        //Debug to test impact of colony
        public void Debug(IncidentParms parms, float rangeMin = 0.25f, float rangeMax = 0.35f)
        {
            float origParm = parms.points;
            float mapPoints = StorytellerUtility.DefaultThreatPointsNow(parms.target);//Min points 
            Log.Message("Min: " + mapPoints.ToString());
            for (int i = 1; i < 100; i++)
            {
                parms.points = 100 * i;
                float points = ResolveRaidPoints(parms, rangeMin, rangeMax);
                Log.Message("Colony Points: " + parms.points + "," + points.ToString());
            }
            parms.points = origParm;
        }
        private static SimpleCurve ThreatPointsOverPointsCurve = new SimpleCurve//A
        {
            {
                new CurvePoint(35f, 38.5f),
                true
            },
            {
                new CurvePoint(400f, 165f),
                true
            },
            {
                new CurvePoint(10000f, 4125f),
                true
            }
        };
        private static SimpleCurve ThreatPointsFactorOverPawnCountCurve = new SimpleCurve//B
        {
            {
                new CurvePoint(1f, 0.5f),
                true
            },
            {
                new CurvePoint(2f, 0.55f),
                true
            },
            {
                new CurvePoint(5f, 1f),
                true
            },
            {
                new CurvePoint(8f, 1.1f),
                true
            },
            {
                new CurvePoint(20f, 2f),
                true
            }
        };
        private static SimpleCurve ThreatPointsFactorOverLocalWealth = new SimpleCurve//C
        {
            {
                new CurvePoint(1000f, 0.5f),
                true
            },
            {
                new CurvePoint(24000f, 1f),
                true
            },
            {
                new CurvePoint(50000f, 1.1f),
                true
            },
            {
                new CurvePoint(250000f, 2f),
                true
            }
        };
    }

    
}