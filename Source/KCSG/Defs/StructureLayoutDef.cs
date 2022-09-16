using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Verse;

namespace KCSG
{
    public class StructureLayoutDef : Def
    {
        internal bool RequiredModLoaded { get; private set; }
        internal bool IsForSlaves { get; private set; }

        public List<List<string>> layouts = new List<List<string>>();
        public List<string> terrainGrid = new List<string>();
        public List<string> roofGrid = new List<string>();

        public bool isStorage = false;
        public bool spawnConduits = true;
        public bool forceGenerateRoof = false;
        public bool needRoofClearance = false;
        public bool randomRotation = false;

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

        /// <summary>
        /// Resolve layout infos
        /// </summary>
        public void ResolveLayouts()
        {
            // Read pos from strings
            foreach (string sPos in spawnAt)
                spawnAtPos.Add(Pos.FromString(sPos));

            // Get height and width
            height = layouts[0].Count;
            width = layouts[0][0].Split(',').Count();

            // Resolve
            ResolveModRequirements();
            ResolveSymbols();
            ResolveTerrain();
            ResolveRoof();
        }

        /// <summary>
        /// Resolve requirements
        /// </summary>
        private void ResolveModRequirements()
        {
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
        /// Resolve terrain grid
        /// </summary>
        private void ResolveTerrain()
        {
            for (int i = 0; i < terrainGrid.Count; i++)
            {
                var tList = terrainGrid[i].Split(',');
                for (int o = 0; o < tList.Length; o++)
                {
                    var terrain = DefDatabase<TerrainDef>.GetNamedSilentFail(tList[o]);
                    terrainGridResolved.Add(terrain);
                }
            }
        }

        /// <summary>
        /// Resolve roof grid
        /// </summary>
        private void ResolveRoof()
        {
            for (int i = 0; i < roofGrid.Count; i++)
            {
                var rList = roofGrid[i].Split(',');
                for (int o = 0; o < width; o++)
                {
                    roofGridResolved.Add(rList[o]);
                }
            }
        }

        /// <summary>
        /// Resolve symbols grids
        /// </summary>
        private void ResolveSymbols()
        {
            var modName = modContentPack?.Name;

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
        }

        /// <summary>
        /// Create XML elements
        /// </summary>
        public string ToXMLString()
        {
            XElement layoutDef = new XElement("KCSG.StructureLayoutDef", null);

            layoutDef.Add(new XElement("defName", defName));

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
