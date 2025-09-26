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
        public List<string> foundationGrid = new List<string>();
        public List<string> underGrid = new List<string>();
        public List<string> tempGrid = new List<string>();
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
        internal TerrainDef[,] _foundationGrid;
        internal TerrainDef[,] _underGrid;
        internal TerrainDef[,] _tempGrid;
        internal ColorDef[,] _terrainColorGrid;
        internal string[,] _roofGrid;

        internal bool RequiredModLoaded { get; private set; }

        internal bool IsForSlaves { get; private set; }

        public int MaxSize { get => maxSize; }

        public IntVec2 Sizes { get => sizes; }

        /// <summary>
        /// Get foundation terrain at position
        /// </summary>
        public TerrainDef FoundationAt(int h, int w)
        {
            if (_foundationGrid == null || h < 0 || w < 0 || h >= sizes.z || w >= sizes.x)
                return null;
            return _foundationGrid[h, w];
        }

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
                if (!ModsConfig.ActiveModsInLoadOrder.Any(m => m.PackageIdNonUnique == modRequirements[o].ToLower()))
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
            if (tCount > 0)
            {
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
            }

            var fCount = foundationGrid.Count;
            if (fCount > 0)
            {
                _foundationGrid = new TerrainDef[sizes.z, sizes.x];
                for (int h = 0; h < sizes.z; h++)
                {
                    if (h < fCount)
                    {
                        var fLine = foundationGrid[h].Split(',');
                        var fLineCount = fLine.Length;

                        for (int w = 0; w < sizes.x; w++)
                        {
                            if (w < fLineCount)
                                _foundationGrid[h, w] = DefDatabase<TerrainDef>.GetNamedSilentFail(fLine[w]);
                            else
                                _foundationGrid[h, w] = null;
                        }
                    }
                    else
                    {
                        for (int w = 0; w < sizes.x; w++)
                            _foundationGrid[h, w] = null;
                    }
                }
            }

            var uCount = underGrid.Count;
            if (uCount > 0)
            {
                _underGrid = new TerrainDef[sizes.z, sizes.x];
                for (int h = 0; h < sizes.z; h++)
                {
                    if (h < uCount)
                    {
                        var uLine = underGrid[h].Split(',');
                        var uLineCount = uLine.Length;

                        for (int w = 0; w < sizes.x; w++)
                        {
                            if (w < uLineCount)
                                _underGrid[h, w] = DefDatabase<TerrainDef>.GetNamedSilentFail(uLine[w]);
                            else
                                _underGrid[h, w] = null;
                        }
                    }
                    else
                    {
                        for (int w = 0; w < sizes.x; w++)
                            _underGrid[h, w] = null;
                    }
                }
            }

            var tempCount = tempGrid.Count;
            if (tempCount > 0)
            {
                _tempGrid = new TerrainDef[sizes.z, sizes.x];
                for (int h = 0; h < sizes.z; h++)
                {
                    if (h < tempCount)
                    {
                        var tempLine = tempGrid[h].Split(',');
                        var tempLineCount = tempLine.Length;

                        for (int w = 0; w < sizes.x; w++)
                        {
                            if (w < tempLineCount)
                                _tempGrid[h, w] = DefDatabase<TerrainDef>.GetNamedSilentFail(tempLine[w]);
                            else
                                _tempGrid[h, w] = null;
                        }
                    }
                    else
                    {
                        for (int w = 0; w < sizes.x; w++)
                            _tempGrid[h, w] = null;
                    }
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
        /// Hot generate rotation symbols
        /// </summary>
        private SymbolDef HotGenerateRotationSymbols(Rot4 rotation, string rotationString, string symbol)
        {
            SymbolDef symbolToCopy = DefDatabase<SymbolDef>.GetNamedSilentFail(symbol.Replace(rotationString, ""));
            SymbolDef symbolToReturn = new SymbolDef
            {
                defName = $"{symbol}",
                rotation = rotation,
                thingDef = symbolToCopy?.thingDef,
                thing = symbolToCopy?.thing,
                maxStackSize = symbolToCopy?.maxStackSize ?? 1,
                replacementDef = symbolToCopy?.replacementDef,

                randomizeStuff = symbolToCopy?.randomizeStuff ?? false,
                stuff = symbolToCopy?.stuff,
                stuffDef = symbolToCopy?.stuffDef,
                color = symbolToCopy?.color,
                colorDef= symbolToCopy?.colorDef,
                styleCategory = symbolToCopy?.styleCategory,
                styleCategoryDef= symbolToCopy?.styleCategoryDef,
                fuel = symbolToCopy?.fuel,
                chanceToContainPawn = symbolToCopy?.chanceToContainPawn ?? 0,
                containPawnKindAnyOf = symbolToCopy?.containPawnKindAnyOf,
                containPawnKindForPlayerAnyOf = symbolToCopy?.containPawnKindForPlayerAnyOf,

                thingSetMakerDef = symbolToCopy?.thingSetMakerDef,
                thingSetMakerDefForPlayer = symbolToCopy?.thingSetMakerDefForPlayer,
                crateStackMultiplier = symbolToCopy?.crateStackMultiplier ?? 1,


            };
            

            return symbolToReturn;


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
                                    if (def is null)
                                    {
                                        if (symbol.Contains("_North"))
                                        {
                                            def = HotGenerateRotationSymbols(Rot4.North, "_North", symbol);
                                        }
                                        else if (symbol.Contains("_East"))
                                        {
                                            def = HotGenerateRotationSymbols(Rot4.East, "_East", symbol);
                                        }
                                        else if (symbol.Contains("_South"))
                                        {

                                            def = HotGenerateRotationSymbols(Rot4.South, "_South", symbol);
                                        }
                                        else if (symbol.Contains("_West"))
                                        {
                                            def = HotGenerateRotationSymbols(Rot4.West, "_West", symbol);
                                        }
                                    }


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

            if (!foundationGrid.NullOrEmpty())
            {
                var l = new XElement("foundationGrid", null);
                foreach (var str in foundationGrid)
                {
                    l.Add(new XElement("li", str));
                }
                layoutDef.Add(l);
            }

            if (!underGrid.NullOrEmpty())
            {
                var l = new XElement("underGrid", null);
                foreach (var str in underGrid)
                {
                    l.Add(new XElement("li", str));
                }
                layoutDef.Add(l);
            }

            if (!tempGrid.NullOrEmpty())
            {
                var l = new XElement("tempGrid", null);
                foreach (var str in tempGrid)
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
