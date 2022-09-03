namespace VFECore.Abilities
{
    using RimWorld;
    using Verse;

    public class AbilityExtension_Hediff : AbilityExtension_AbilityMod
    {
        public HediffDef hediff;
        public BodyPartDef bodyPartToApply;
        public float     severity  = -1f;
        public bool      applyAuto = true;
        public StatDef durationMultiplier;
        public bool durationMultiplierFromCaster;
        public bool targetOnlyEnemies;
        public bool applyToCaster;
        public override bool ValidateTarget(LocalTargetInfo target, Ability ability, bool throwMessages = false)
        {
            if (this.targetOnlyEnemies && target.Thing != null && !target.Thing.HostileTo(ability.pawn))
            {
                if (throwMessages)
                {
                    Messages.Message("VFEA.TargetMustBeHostile".Translate(), target.Thing, MessageTypeDefOf.CautionInput, null);
                }
                return false;
            }
            return base.ValidateTarget(target, ability, throwMessages);
        }
    }
}