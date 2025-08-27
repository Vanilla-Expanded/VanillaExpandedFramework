using System.Collections.Generic;
using Verse;

namespace VEF.Factions.GameComponents;

public class GameComponent_FactionGoodwillImpactManager : GameComponent
{
    public List<GoodwillImpactDelayed> goodwillImpacts = new();

    public GameComponent_FactionGoodwillImpactManager(Game game)
    {
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        for (int i = goodwillImpacts.Count - 1; i >= 0; i--)
        {
            GoodwillImpactDelayed goodwillImpact = goodwillImpacts[i];
            if (Find.TickManager.TicksGame >= goodwillImpact.impactInTicks)
            {
                goodwillImpact.DoImpact();
                goodwillImpacts.RemoveAt(i);
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref goodwillImpacts, "goodwillImpacts", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit) goodwillImpacts ??= new List<GoodwillImpactDelayed>();
    }
}