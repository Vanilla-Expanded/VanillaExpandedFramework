using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KCSG
{
    public struct Pos
    {
        public int x;
        public int y;

        public Pos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Pos FromString(string str)
        {
            string[] array = str.Split(',');

            if (array.Length == 2)
            {
                return new Pos(int.Parse(array[0]), int.Parse(array[1]));
            }

            return new Pos(0, 0);
        }
    }

    public class StructureLayoutDef : Def
    {
        public bool isStorage = false;
        public bool spawnConduits = true;
        public List<List<string>> layouts = new List<List<string>>();
        public List<string> roofGrid = new List<string>();

        // Settings for SettlementDef
        public List<string> tags = new List<string>();

        // Mod requirements
        public List<string> modRequirements = new List<string>();

        /* --- Obsolete --- */
        public bool requireRoyalty = false;
        /* --- ------- --- */

        // Spawn position
        public List<Pos> spawnAtPos = new List<Pos>();
        public List<string> spawnAt = new List<string>();

        internal int width;
        internal int height;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (requireRoyalty)
            {
                Log.Warning($"{defName} is using obsolete field requireRoyalty. Report this to {modContentPack.Name}");
                modRequirements.Add("ludeon.rimworld.royalty");
            }
            foreach (string sPos in spawnAt)
            {
                spawnAtPos.Add(Pos.FromString(sPos));
            }

            height = layouts[0].Count;
            width = layouts[0][0].Split(',').Count();
        }
    }
}
