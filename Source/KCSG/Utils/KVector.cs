using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCSG.Utils
{
    public enum Type
    {
        NONE,
        BUILDING,
        DOOR,
        GROWING,
        ROAD,
        MAINROAD,
        DEBUG
    }

    public class KVector
    {
        public float Cost;

        public float DistanceToTarget;

        public KVector Parent;

        public float Weight;

        public bool useCost;

        public KVector(double x, double y, bool useCost = true, Type type = Type.NONE, float weight = 1f)
        {
            X = x;
            Y = y;
            this.type = type;

            Parent = null;
            DistanceToTarget = -1f;
            Cost = 1f;
            Weight = weight;
            this.useCost = useCost;
        }

        public HashSet<Triangle> AdjacentTriangles { get; } = new HashSet<Triangle>();

        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + (useCost ? (type == Type.MAINROAD ? Cost * 0.1f : type == Type.ROAD ? Cost * 0.4f : Cost) : Cost);
                else
                    return -1;
            }
        }

        public Type type { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public override string ToString() => $"{nameof(KVector)} {X:0.##}@{Y:0.##}";
    }
}
