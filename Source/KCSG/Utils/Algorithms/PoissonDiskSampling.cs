using System;
using System.Collections.Generic;
using System.Linq;

namespace KCSG
{
    public static class PoissonDiskSampling
    {
        /// https://bl.ocks.org/mbostock/raw/dbb02448b0f93e4c82c3/?raw=true
        public static List<CustomVector> Run(int radius, int maxTries, int Width, int Height, Random r, CustomVector[][] grid)
        {
            List<CustomVector> points = new List<CustomVector>();
            List<CustomVector> active = new List<CustomVector>();

            /* Initial random point */
            CustomVector p0 = new CustomVector(0, 0);
            grid[(int)p0.X][(int)p0.Y] = p0;
            points.Add(p0);
            active.Add(p0);

            while (active.Count > 0)
            {
                int random_index = r.Next(active.Count);
                CustomVector p = active.ElementAt(random_index);

                for (int tries = 1; tries <= maxTries; tries++)
                {
                    /* Pick a random angle */
                    int theta = r.Next(361);
                    /* Pick a random radius between r and 1.5r */
                    int new_radius = r.Next(radius, (int)(1.5f * radius));
                    /* Find X & Y coordinates relative to point p */
                    int pnewx = (int)(p.X + new_radius * Math.Cos(ConvertToRadians(theta)));
                    int pnewy = (int)(p.Y + new_radius * Math.Sin(ConvertToRadians(theta)));
                    CustomVector pnew = new CustomVector(pnewx, pnewy);

                    if (IsInBound(pnew, Width, Height, radius) && InsideCircles(pnew, radius, points))
                    {
                        points.Add(pnew);
                        active.Add(pnew);
                        break;
                    }
                    else if (tries == maxTries)
                    {
                        active.RemoveAt(random_index);
                    }
                }
            }
            return points;
        }

        private static double ConvertToRadians(int angle)
        {
            return (Math.PI / 180) * angle;
        }

        private static bool InsideCircle(CustomVector center, CustomVector tile, float radius)
        {
            float dx = (float)(center.X - tile.X),
                  dy = (float)(center.Y - tile.Y);
            float distance_squared = dx * dx + dy * dy;
            return distance_squared <= radius * radius;
        }

        private static bool InsideCircles(CustomVector tile, float radius, List<CustomVector> allPoints)
        {
            foreach (CustomVector item in allPoints)
            {
                if (InsideCircle(item, tile, radius))
                    return false;
            }
            return true;
        }

        private static bool IsInBound(CustomVector p, int gridWidth, int gridHeight, int radius)
        {
            if (p.X < 0)
                return false;
            if (p.X >= gridWidth - radius)
                return false;
            if (p.Y < 0)
                return false;
            if (p.Y >= gridHeight - radius)
                return false;
            return true;
        }
    }
}