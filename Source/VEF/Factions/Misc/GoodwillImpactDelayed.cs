namespace VEF.Factions
{
    using RimWorld;
    using Verse;

    public class GoodwillImpactDelayed : IExposable
    {
        public int impactInTicks;
        public int goodwillImpact;
        public Faction factionToImpact;
        public HistoryEventDef historyEvent;
        public string letterLabel;
        public string letterDesc;
        public string relationInfoKey;
        public LetterDef letterType;
        public bool RemoveAfterImpact = true;
        
        public virtual void DoImpact()
        {
            Faction.OfPlayer.TryAffectGoodwillWith(factionToImpact, goodwillImpact, true, true, historyEvent);
            if (!relationInfoKey.NullOrEmpty())
            {
                letterDesc += "\n\n" + relationInfoKey.Translate(factionToImpact.Named("FACTION"),
                    Faction.OfPlayer.GoodwillWith(factionToImpact), goodwillImpact);
            }
            Find.LetterStack.ReceiveLetter(letterLabel, letterDesc, letterType, null, relatedFaction: factionToImpact);
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref impactInTicks, "impactInTicks");
            Scribe_Values.Look(ref goodwillImpact, "goodwillImpact");
            Scribe_Values.Look(ref letterLabel, "letterLabel");
            Scribe_Values.Look(ref letterDesc, "letterDesc");
            Scribe_Values.Look(ref relationInfoKey, "relationInfoKey");
            Scribe_References.Look(ref factionToImpact, "factionToImpact");
            Scribe_Defs.Look(ref historyEvent, "historyEvent");
            Scribe_Defs.Look(ref letterType, "letterType");
            Scribe_Values.Look(ref RemoveAfterImpact, "RemoveAfterImpact");
        }
    }
}