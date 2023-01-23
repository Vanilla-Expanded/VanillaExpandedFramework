using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using System.Xml;
using RimWorld.Planet;

namespace VFECore
{

    public class FactionDefExtension : DefModExtension
    {

        private static readonly FactionDefExtension DefaultValues = new FactionDefExtension();
        public static FactionDefExtension Get(Def def) => def.GetModExtension<FactionDefExtension>() ?? DefaultValues;

        public override IEnumerable<string> ConfigErrors()
        {
            // The closest we have to ResolveReferences :/
            if (!siegeParameterSet.NullOrEmpty())
                siegeParameterSetDef = DefDatabase<SiegeParameterSetDef>.GetNamed(siegeParameterSet);

            yield break;
        }

        public bool hasCities = true;
        public string settlementGenerationSymbol;
        public string packAnimalTexNameSuffix;
        public PawnKindDef strangerInBlackReplacement;
        private string siegeParameterSet ="";
        public SiegeParameterSetDef siegeParameterSetDef;
        public List<StartingGoodwillByFaction> startingGoodwillByFactionDefs = new List<StartingGoodwillByFaction>();

        public List<BiomeDef> allowedBiomes = new List<BiomeDef>();
        public List<BiomeDef> disallowedBiomes = new List<BiomeDef>();
        public List<Hilliness> requiredHillLevels;
        public bool spawnOnCoastalTilesOnly;
        public bool neverConnectToRoads;
        public float minDistanceToOtherSettlements;
        public bool excludeFromCommConsole;
        public bool excludeFromQuests;

        public List<RaidStrategyDef> allowedStrategies = new List<RaidStrategyDef>();
    }

    // Pairs a given factionDef to a range of allowed starting goodwill
    public class StartingGoodwillByFaction
    {
        public FactionDef factionDef;

        public IntRange startingGoodwill;

        public int Min => startingGoodwill.min;

        public int Max => startingGoodwill.max;

        public StartingGoodwillByFaction()
        {
        }

        public StartingGoodwillByFaction(FactionDef factionDef, int min, int max)
            : this(factionDef, new IntRange(min, max))
        {
        }

        public StartingGoodwillByFaction(FactionDef factionDef, IntRange startingGoodwill)
        {
            this.factionDef = factionDef;
            this.startingGoodwill = startingGoodwill;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Misconfigured StartingGoodwillByFaction: " + xmlRoot.OuterXml);
                return;
            }
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "factionDef", xmlRoot.Name);
            startingGoodwill = ParseHelper.FromString<IntRange>(xmlRoot.FirstChild.Value);
        }

        public override string ToString()
        {
            return "(" + ((factionDef != null) ? factionDef.defName : "null") + " with starting goodwill of " + startingGoodwill + ")";
        }
    }

}
