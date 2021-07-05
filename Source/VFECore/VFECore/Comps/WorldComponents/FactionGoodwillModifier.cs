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
            ScenPartUtility.SetCache();
            ScenPartUtility.goodwillScenParts = null;
            var factionList = DefDatabase<FactionDef>.AllDefsListForReading;
            for (int i = 0; i < factionList.Count; i++)
            {
                var faction = factionList[i];
                ScenPartUtility.FinaliseFactionGoodwillCharacteristics(faction);
            }
        }
    }
}