using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public enum CellType
    {
        NONE,
        BUILDING,
        DOOR,
        GROWING,
        ROAD,
        MAINROAD,
        WATER
    }

    public class CustomVector
    {
        public float Cost;

        public float DistanceToTarget;

        public CustomVector Parent;

        public bool useCost;

        public float Weight;

        public CustomVector(double x, double y, CellType type = CellType.NONE, float weight = 1f, ThingDef plant = null)
        {
            X = x;
            Y = y;
            this.Type = type;

            Parent = null;
            DistanceToTarget = -1f;
            Cost = 1f;
            Weight = weight;
            PlantType = plant;
        }

        public HashSet<Triangle> AdjacentTriangles { get; } = new HashSet<Triangle>();

        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                {
                    if (Type == CellType.MAINROAD && CurrentGenerationOption.usePathCostReduction)
                    {
                        return DistanceToTarget + (Cost * 0.1f);
                    }
                    else if (Type == CellType.ROAD && CurrentGenerationOption.usePathCostReduction)
                    {
                        return DistanceToTarget + (Cost * 0.4f);
                    }
                    else
                    {
                        return DistanceToTarget + Cost;
                    }
                }
                else
                    return -1;
            }
        }

        public CellType Type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public ThingDef PlantType { get; set; }

        public bool IsNoneOrRoad()
        {
            return Type == CellType.ROAD || Type == CellType.MAINROAD || Type == CellType.NONE;
        }

        public double DistanceTo(CustomVector otherVector)
        {
            return Math.Sqrt(Math.Pow(X - otherVector.X, 2) + Math.Pow(Y - otherVector.Y, 2));
        }

        public override string ToString() => $"{nameof(CustomVector)} {X:0.##}@{Y:0.##}";
    }
}