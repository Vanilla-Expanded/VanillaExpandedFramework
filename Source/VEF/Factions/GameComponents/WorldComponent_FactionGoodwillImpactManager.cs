using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace VEF.Factions.GameComponents;

public class WorldComponent_FactionGoodwillImpactManager(World world) : WorldComponent(world)
{
    protected List<GoodwillImpactDelayed> goodwillImpacts = new();

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        for (int i = goodwillImpacts.Count - 1; i >= 0; i--)
        {
            GoodwillImpactDelayed goodwillImpact = goodwillImpacts[i];
            if (Find.TickManager.TicksGame >= goodwillImpact.impactInTicks)
            {
                goodwillImpact.DoImpact();
                if (goodwillImpact.RemoveAfterImpact) goodwillImpacts.RemoveAt(i);
            }
        }
    }

    public void ImpactFactionGoodwill(GoodwillImpactDelayed goodwillImpact)
    {
        goodwillImpacts.Add(goodwillImpact);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref goodwillImpacts, "goodwillImpacts", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit) goodwillImpacts ??= new List<GoodwillImpactDelayed>();
    }
}