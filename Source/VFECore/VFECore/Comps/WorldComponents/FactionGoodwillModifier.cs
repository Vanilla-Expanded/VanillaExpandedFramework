using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VFECore
{
    public class FactionGoodwillModifier : WorldComponent
    {
        public FactionGoodwillModifier(World world) : base(world)
        {
        }

        public override void FinalizeInit()
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