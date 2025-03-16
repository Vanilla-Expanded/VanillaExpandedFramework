using System.Collections.Generic;
using System.Linq;
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

            foreach (var error in forcedFactionData.ConfigErrors())
                yield return error;
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

        public ForcedFactionData forcedFactionData = new ForcedFactionData();
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

    public class ForcedFactionData
    {
        // World generation
        public int requiredFactionCountAtWorldGeneration = 0;
        public bool preventRemovalAtWorldGeneration = false;
        public bool displayMissingWarningIfNoFactionPresent = false;
        public string factionDisabledAtWorldGenerationMessage = null;

        // Existing games/faction discovery
        public int requiredFactionCountDuringGameplay = 0;
        public bool forceAddFactionIfMissing = false;
        public bool forcePlayerToAddFactionIfMissing = false;
        public string factionDiscoverySpecialMessage = null;
        public string factionDiscoveryFailedMessage = null;
        public float factionDiscoveryFactionCountFactor = 1f;
        // 0 or negative values will force faction discovery to use the default value.
        public int factionDiscoveryMinimumDistanceFromPlayer = -1;

        public bool UnderRequiredWorldGenFactionCount(FactionDef faction, List<FactionDef> factions)
            => requiredFactionCountAtWorldGeneration > 0 && factions.Count(x => x == faction) < Mathf.Min(faction.maxConfigurableAtWorldCreation, requiredFactionCountAtWorldGeneration);

        public bool UnderRequiredGameplayFactionCount(FactionDef faction)
            => UnderRequiredGameplayFactionCount(faction, Find.FactionManager.AllFactions.Count(f => f.def == faction));
        public bool UnderRequiredGameplayFactionCount(FactionDef faction, int factionCount)
            => requiredFactionCountDuringGameplay > 0 && factionCount < Mathf.Min(faction.maxConfigurableAtWorldCreation, requiredFactionCountDuringGameplay);

        public TaggedString GetWorldGenMissingFactionMessage(FactionDef faction, List<FactionDef> factions)
            => ((string.IsNullOrWhiteSpace(factionDisabledAtWorldGenerationMessage), preventRemovalAtWorldGeneration) switch
                {
                    (false, _) => factionDisabledAtWorldGenerationMessage,
                    (_, true) => "VanillaFactionsExpanded.FactionRequired",
                    (_, false) => "VanillaFactionsExpanded.FactionRecommended",
                })
                .Translate(
                    faction.label.Named(SignalArgsNames.Faction),
                    factions.Count(x => x == faction).Named(SignalArgsNames.Count),
                    requiredFactionCountAtWorldGeneration.Named("REQUIRED")
                )
                .CapitalizeFirst();

        public TaggedString GetFactionDiscoveryMessage(FactionDef faction)
            => (string.IsNullOrWhiteSpace(factionDiscoverySpecialMessage)
                    ? "VanillaFactionsExpanded.FactionDiscoveryRequired"
                    : factionDiscoverySpecialMessage
                )
                .Translate(
                    faction.label.Named(SignalArgsNames.Faction),
                    Find.FactionManager.AllFactions.Count(f => f.def == faction).Named(SignalArgsNames.Count),
                    requiredFactionCountAtWorldGeneration.Named("REQUIRED")
                )
                .CapitalizeFirst();

        public TaggedString GetFactionDiscoveryFailedMessage(FactionDef faction)
            => (string.IsNullOrWhiteSpace(factionDiscoveryFailedMessage)
                    ? "VanillaFactionsExpanded.FactionFailedMessage"
                    : factionDiscoveryFailedMessage
                )
                .Translate(
                    faction.label.Named(SignalArgsNames.Faction),
                    Find.FactionManager.AllFactions.Count(f => f.def == faction).Named(SignalArgsNames.Count),
                    requiredFactionCountAtWorldGeneration.Named("REQUIRED")
                )
                .CapitalizeFirst();

        public IEnumerable<string> ConfigErrors()
        {
            if (requiredFactionCountAtWorldGeneration < 0)
            {
                yield return $"{nameof(requiredFactionCountAtWorldGeneration)} cannot be less than 0, but currently it is {requiredFactionCountAtWorldGeneration}. Fixing.";
                requiredFactionCountAtWorldGeneration = 0;
            }

            if (requiredFactionCountDuringGameplay < 0)
            {
                yield return $"{nameof(requiredFactionCountDuringGameplay)} cannot be less than 0, but currently it is {requiredFactionCountDuringGameplay}. Fixing.";
                requiredFactionCountDuringGameplay = 0;
            }

            if (factionDiscoveryFactionCountFactor < 0f)
            {
                yield return $"{nameof(factionDiscoveryFactionCountFactor)} cannot be less than 0, but currently it is {factionDiscoveryFactionCountFactor}. Fixing.";
                factionDiscoveryFactionCountFactor = 0f;
            }

            if (factionDiscoveryMinimumDistanceFromPlayer > 10)
            {
                yield return $"{nameof(factionDiscoveryMinimumDistanceFromPlayer)} cannot be more than 10, but currently it is {factionDiscoveryMinimumDistanceFromPlayer}. Fixing.";
                factionDiscoveryMinimumDistanceFromPlayer = 10;
            }
        }
    }
}