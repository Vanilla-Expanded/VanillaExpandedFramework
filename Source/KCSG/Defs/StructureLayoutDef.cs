using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
        public List<string> terrainGrid = new List<string>();
        public bool forceGenerateRoof = false;
        public bool needRoofClearance = false;

        // Settings for SettlementDef
        public List<string> tags = new List<string>();

        // Mod requirements
        public List<string> modRequirements = new List<string>();

        // Spawn position
        public List<Pos> spawnAtPos = new List<Pos>();
        public List<string> spawnAt = new List<string>();

        // Values used regularly in gen:
        internal int width;
        internal int height;
        internal List<List<SymbolDef>> symbolsLists = new List<List<SymbolDef>>();
        internal List<TerrainDef> terrainGridResolved = new List<TerrainDef>();
        internal List<string> roofGridResolved = new List<string>();

        internal bool RequiredModLoaded { get; private set; }

        internal bool IsForSlaves { get; private set; }

        /// <summary>
        /// Populate symbol list and roofgrid. Prevent the need of doing it at each gen
        /// </summary>
        public void ResolveLayouts()
        {
            foreach (string sPos in spawnAt)
            {
                spawnAtPos.Add(Pos.FromString(sPos));
            }

            // Get height and width
            height = layouts[0].Count;
            width = layouts[0][0].Split(',').Count();

            var modName = modContentPack?.Name;
            // Populate symbolsLists and setup IsForSlaves
            for (int i = 0; i < layouts.Count; i++)
            {
                var layout = layouts[i];
                symbolsLists.Add(new List<SymbolDef>());
                for (int o = 0; o < layout.Count; o++)
                {
                    var symbols = layout[o].Split(',');
                    for (int p = 0; p < symbols.Length; p++)
                    {
                        string symbol = symbols[p];
                        if (symbol == ".")
                        {
                            symbolsLists[i].Add(null);
                        }
                        else
                        {
                            SymbolDef def = DefDatabase<SymbolDef>.GetNamedSilentFail(symbol);
                            symbolsLists[i].Add(def);

                            if (def == null)
                                StartupActions.AddToMissing($"{modName} {symbol}");
                            else if (def.isSlave)
                                IsForSlaves = true;
                        }
                    }
                }
            }
            // Populate roofgrid
            for (int i = 0; i < roofGrid.Count; i++)
            {
                roofGridResolved.AddRange(roofGrid[i].Split(','));
            }
            // Populate terrainGrid
            for (int i = 0; i < terrainGrid.Count; i++)
            {
                var tList = terrainGrid[i].Split(',');
                for (int o = 0; o < tList.Length; o++)
                {
                    var terrain = DefDatabase<TerrainDef>.GetNamedSilentFail(tList[o]);
                    terrainGridResolved.Add(terrain);
                }
            }
            // Resolve mod requirement(s)
            RequiredModLoaded = true;
            for (int o = 0; o < modRequirements.Count; o++)
            {
                if (!ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageId == modRequirements[o].ToLower()))
                {
                    RequiredModLoaded = false;
                    break;
                }
            }
        }

        /// <summary>
        /// Create XML elements
        /// </summary>
        public string ToXMLString()
        {
            XElement layoutDef = new XElement("KCSG.StructureLayoutDef", null);

            if (isStorage)
                layoutDef.Add(new XElement("isStorage", isStorage));

            if (!spawnConduits)
                layoutDef.Add(new XElement("spawnConduits", spawnConduits));

            if (!layouts.NullOrEmpty())
            {
                var l = new XElement("layouts", null);
                foreach (var lst in layouts)
                {
                    var ll = new XElement("li", null);
                    foreach (var str in lst)
                        ll.Add(new XElement("li", str));

                    l.Add(ll);
                }
                layoutDef.Add(l);
            }

            if (!roofGrid.NullOrEmpty())
            {
                var l = new XElement("roofGrid", null);
                foreach (var str in roofGrid)
                {
                    l.Add(new XElement("li", str));
                }
                layoutDef.Add(l);
            }

            if (!terrainGrid.NullOrEmpty())
            {
                var l = new XElement("terrainGrid", null);
                foreach (var str in terrainGrid)
                {
                    l.Add(new XElement("li", str));
                }
                layoutDef.Add(l);
            }

            if (forceGenerateRoof)
                layoutDef.Add(new XElement("forceGenerateRoof", forceGenerateRoof));

            if (needRoofClearance)
                layoutDef.Add(new XElement("needRoofClearance", needRoofClearance));

            if (!tags.NullOrEmpty())
            {
                var l = new XElement("tags", null);
                foreach (var str in tags)
                {
                    l.Add(new XElement("li", str));
                }
                layoutDef.Add(l);
            }

            if (!modRequirements.NullOrEmpty())
            {
                var l = new XElement("modRequirements", null);
                foreach (var str in modRequirements)
                {
                    l.Add(new XElement("li", str));
                }
                layoutDef.Add(l);
            }

            return layoutDef.ToString();
        }
    }
}
