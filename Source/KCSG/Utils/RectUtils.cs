using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    internal class RectUtils
    {
        public static List<IntVec3> AreaToSquare(Area a, int height, int widht)
        {
            List<IntVec3> list = a.ActiveCells.ToList();
            MinMaxXZ(list, out int zMin, out int zMax, out int xMin, out int xMax);

            List<IntVec3> listOut = new List<IntVec3>();

            for (int zI = zMin; zI <= zMax; zI++)
            {
                for (int xI = xMin; xI <= xMax; xI++)
                {
                    listOut.Add(new IntVec3(xI, 0, zI));
                }
            }
            listOut.Sort((x, y) => x.z.CompareTo(y.z));
            return listOut;
        }

        public static void EdgeFromArea(List<IntVec3> cellExport, out int height, out int width)
        {
            height = 0;
            width = 0;
            foreach (IntVec3 f in cellExport)
            {
                int tempW = 0, tempH = 0;
                foreach (IntVec3 c in cellExport)
                {
                    if (f.z == c.z) tempW++;
                }
                foreach (IntVec3 c in cellExport)
                {
                    if (f.x == c.x) tempH++;
                }
                if (tempW > width) width = tempW;
                if (tempH > height) height = tempH;
            }
        }

        public static void EdgeFromList(List<IntVec3> cellExport, out int height, out int width)
        {
            height = 0;
            width = 0;
            IntVec3 first = cellExport.First();
            foreach (IntVec3 c in cellExport)
            {
                if (first.z == c.z) width++;
            }
            foreach (IntVec3 c in cellExport)
            {
                if (first.x == c.x) height++;
            }
        }

        public static int GetMaxThingOnOneCell(List<IntVec3> cellExport, Dictionary<IntVec3, List<Thing>> pairsCellThingList)
        {
            int max = 1;
            foreach (IntVec3 item in cellExport)
            {
                List<Thing> things = pairsCellThingList.TryGetValue(item);
                things.RemoveAll(t => t is Pawn || t.def.building == null || t.def.defName == "PowerConduit");
                if (things.Count > max) max = things.Count;
            }
            return max;
        }

        public static void HeightWidthFromLayout(StructureLayoutDef structureLayoutDef, out int height, out int width)
        {
            if (structureLayoutDef == null || structureLayoutDef.layouts.Count == 0)
            {
                Log.Warning("StructureLayoutDef was null or empty. Throwing 10 10 size");
                height = 10;
                width = 10;
                return;
            }
            height = structureLayoutDef.layouts[0].Count;
            width = structureLayoutDef.layouts[0][0].Split(',').ToList().Count;
        }

        public static void MinMaxXZ(List<IntVec3> list, out int zMin, out int zMax, out int xMin, out int xMax)
        {
            zMin = list[0].z;
            zMax = 0;
            xMin = list[0].x;
            xMax = 0;
            foreach (IntVec3 c in list)
            {
                if (c.z < zMin) zMin = c.z;
                if (c.z > zMax) zMax = c.z;
                if (c.x < xMin) xMin = c.x;
                if (c.x > xMax) xMax = c.x;
            }
        }
    }
}