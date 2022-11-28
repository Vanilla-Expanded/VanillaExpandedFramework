
using Verse;
using RimWorld;

namespace AnimalBehaviours
{
    class HediffComp_HighlyFlammable : HediffComp
    {

        public int tickCounter = 0;

        public HediffCompProperties_HighlyFlammable Props
        {
            get
            {
                return (HediffCompProperties_HighlyFlammable)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            tickCounter++;
            if (tickCounter > Props.tickInterval)
            {
                Pawn pawn = parent.pawn;

                bool flagIsBurning = (pawn.IsBurning() && !Props.sunlightBurns) || (Props.sunlightBurns && this.parent.pawn.Map!=null&&this.parent.pawn.Position.InSunlight(this.parent.pawn.Map));
                //Only do things if pawn is burning (or in sunlight if sunlightBurns is true)
                if (pawn.Map != null && flagIsBurning)
                {
                    BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = null;
                    if (pawn != null)
                    {
                        battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire, pawn);
                        Find.BattleLog.Add(battleLogEntry_DamageTaken);
                    }
                    //Apply the additional Hediff
                    DamageDef flame = Props.damageToInflict;
                    float amount = Props.damageAmount;
                    Thing instigator = parent.pawn;
                    parent.pawn.TakeDamage(new DamageInfo(flame, amount, 0f, -1f, instigator, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null)).AssociateWithLog(battleLogEntry_DamageTaken);
                }
                tickCounter = 0;
            }
        }

       
    }
}
