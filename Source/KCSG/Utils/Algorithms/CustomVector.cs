using System.Collections.Generic;

namespace KCSG
{
    public class CustomVector
    {
        public float Cost;

        public float DistanceToTarget;

        public CustomVector Parent;

        public float Weight;

        public bool useCost;

        public CustomVector(double x, double y, CellType type = CellType.NONE, float weight = 1f)
        {
            X = x;
            Y = y;
            this.Type = type;

            Parent = null;
            DistanceToTarget = -1f;
            Cost = 1f;
            Weight = weight;
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
                    else if (Type == CellType.WATER)
                    {
                        return DistanceToTarget + (Cost * 3f);
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

        public override string ToString() => $"{nameof(CustomVector)} {X:0.##}@{Y:0.##}";
    }

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
}