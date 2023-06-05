using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace KCSG
{
    public class StructureLayoutDef : Def
    {
        public List<List<string>> layouts = new List<List<string>>();
        public List<string> terrainGrid = new List<string>();
        public List<string> terrainColorGrid = new List<string>();
        public List<string> roofGrid = new List<string>();

        public bool isStorage = false;
        public bool spawnConduits = true;
        public bool forceGenerateRoof = false;
        public bool needRoofClearance = false;
        public bool randomRotation = false;
        public bool randomizeWallStuffAtGen = false;

        // Settings for SettlementDef
        public List<string> tags = new List<string>();

        // Mod requirements
        public List<string> modRequirements = new List<string>();

        // Spawn position
        public List<IntVec2> spawnAt = new List<IntVec2>();

        // Values used regularly in gen:
        internal int maxSize;
        internal IntVec2 sizes;
        // internal int gridCount;
        internal List<SymbolDef[,]> _layouts = new List<SymbolDef[,]>();
        internal TerrainDef[,] _terrainGrid;
        internal ColorDef[,] _terrainColorGrid;
        internal string[,] _roofGrid;

        internal bool RequiredModLoaded { get; private set; }

        internal bool IsForSlaves { get; private set; }

        public int MaxSize { get => maxSize; }

        public IntVec2 Sizes { get => sizes; }

        /// <summary>
        /// Resolve layout infos
        /// </summary>
        public void ResolveLayouts()
        {
            // Make it a even rect
            var height = layouts[0].Count;
            var width = layouts[0][0].Split(',').Count();
            maxSize = Math.Max(height, width);
            sizes = new IntVec2(width, height);
            // gridCount = size * size;

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
            var tCount = terrainGrid.Count;
            if (tCount == 0)
                return;

            _terrainGrid = new TerrainDef[sizes.z, sizes.x];
            for (int h = 0; h < sizes.z; h++)
            {
                if (h < tCount)
                {
                    var tLine = terrainGrid[h].Split(',');
                    var tLineCount = tLine.Length;

                    for (int w = 0; w < sizes.x; w++)
                    {
                        if (w < tLineCount)
                            _terrainGrid[h, w] = DefDatabase<TerrainDef>.GetNamedSilentFail(tLine[w]);
                        else
                            _terrainGrid[h, w] = null;
                    }
                }
                else
                {
                    for (int w = 0; w < sizes.x; w++)
                        _terrainGrid[h, w] = null;
                }
            }

            var tcCount = terrainColorGrid.Count;
            if (tcCount == 0)
                return;

            _terrainColorGrid = new ColorDef[sizes.z, sizes.x];
            for (int i = 0; i < sizes.z; i++)
            {
                if (i < tcCount)
                {
                    var colorLine = terrainColorGrid[i].Split(',');
                    var colorLineCount = colorLine.Length;

                    for (int w = 0; w < sizes.x; w++)
                    {
                        if (w < colorLineCount)
                            _terrainColorGrid[i, w] = DefDatabase<ColorDef>.GetNamedSilentFail(colorLine[w]);
                        else
                            _terrainColorGrid[i, w] = null;
                    }
                }
                else
                {
                    for (int w = 0; w < sizes.x; w++)
                        _terrainColorGrid[i, w] = null;
                }
            }
        }

        /// <summary>
        /// Resolve roof grid
        /// </summary>
        private void ResolveRoof()
        {
            var rCount = roofGrid.Count;
            if (rCount == 0)
                return;

            _roofGrid = new string[sizes.z, sizes.x];

            for (int h = 0; h < sizes.z; h++)
            {
                if (h < rCount)
                {
                    var rLine = roofGrid[h].Split(',');
                    var rLineCount = rLine.Length;

                    for (int w = 0; w < sizes.x; w++)
                    {
                        if (w < rLineCount)
                            _roofGrid[h, w] = rLine[w];
                        else
                            _roofGrid[h, w] = ".";
                    }
                }
                else
                {
                    for (int w = 0; w < sizes.x; w++)
                        _roofGrid[h, w] = ".";
                }
            }
        }

        /// <summary>
        /// Resolve symbols grids
        /// </summary>
        private void ResolveSymbols()
        {
            var modName = modContentPack?.Name;

            for (int l = 0; l < layouts.Count; l++)
            {
                var layout = layouts[l];
                var lCount = layout.Count;
                _layouts.Add(new SymbolDef[sizes.z, sizes.x]);

                for (int h = 0; h < sizes.z; h++)
                {
                    if (h < lCount)
                    {
                        var symbols = layout[h].Split(',');
                        var symbolsCount = symbols.Length;

                        for (int w = 0; w < sizes.x; w++)
                        {
                            if (w < symbolsCount)
                            {
                                var symbol = symbols[w];
                                if (symbol == ".")
                                {
                                    _layouts[l][h, w] = null;
                                }
                                else
                                {
                                    SymbolDef def = DefDatabase<SymbolDef>.GetNamedSilentFail(symbol);
                                    _layouts[l][h, w] = def;

                                    if (def == null)
                                        StartupActions.AddToMissing(modName, symbol);
                                    else if (def.isSlave)
                                        IsForSlaves = true;
                                }
                            }
                            else
                            {
                                _layouts[l][h, w] = null;
                            }
                        }
                    }
                    else
                    {
                        for (int w = 0; w < sizes.x; w++)
                            _layouts[l][h, w] = null;
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

            if (!terrainColorGrid.NullOrEmpty())
            {
                var l = new XElement("terrainColorGrid", null);
                foreach (var str in terrainColorGrid)
                {
                    l.Add(new XElement("li", str));
                }
                layoutDef.Add(l);
            }

            if (forceGenerateRoof)
                layoutDef.Add(new XElement("forceGenerateRoof", forceGenerateRoof));

            if (needRoofClearance)
                layoutDef.Add(new XElement("needRoofClearance", needRoofClearance));

            if (randomizeWallStuffAtGen)
                layoutDef.Add(new XElement("randomizeWallStuffAtGen", randomizeWallStuffAtGen));

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
