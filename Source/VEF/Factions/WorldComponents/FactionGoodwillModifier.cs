using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VEF.Factions
{
    public class FactionGoodwillModifier : WorldComponent
    {
        public FactionGoodwillModifier(World world) : base(world)
        {
        }

        public override void FinalizeInit(bool fromLoad)
        {
            ScenPartUtility.goodwillScenParts = null;
            ScenPartUtility.startingGoodwillRangeCache.Clear();
            var factionList = DefDatabase<FactionDef>.AllDefsListForReading;
            for (var i = 0; i < factionList.Count; i++)
            {
                var faction = factionList[i];
                ScenPartUtility.FinaliseFactionGoodwillCharacteristics(faction);
            }
        }
    }
}