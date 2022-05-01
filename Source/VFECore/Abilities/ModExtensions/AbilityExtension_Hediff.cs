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
        public bool scalesWithTargetStat = true;
        public bool scalesWithCasterStat;
    }
}