namespace VFECore.Abilities
{
    using RimWorld;
    using Verse;

    public class CompProperties_UseEffectGiveAbility : CompProperties_UseEffect
    {
        public AbilityDef ability;
        public int   level = 1;
        public HediffDef  requiredHediff;

        public CompProperties_UseEffectGiveAbility() => 
            this.compClass = typeof(CompUseEffect_GiveAbility);
    }

    public class CompUseEffect_GiveAbility : CompUseEffect
    {
        public CompProperties_UseEffectGiveAbility Props => (CompProperties_UseEffectGiveAbility) this.props;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            if (this.Props.ability != null)
            {
                usedBy.GetComp<CompAbilities>()?.GiveAbility(this.Props.ability);
            }
            else
            {
                (usedBy.health.hediffSet.GetFirstHediffOfDef(this.Props.requiredHediff) as Hediff_Abilities)?.GiveRandomAbilityAtLevel(this.Props.level);
            }
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            if (!base.CanBeUsedBy(p))
                return false;

            CompAbilities comp = p.GetComp<CompAbilities>();
            return comp != null;
        }
    }
}