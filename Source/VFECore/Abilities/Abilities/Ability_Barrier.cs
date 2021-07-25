namespace VFECore.Abilities
{
    using Verse;

    public class Ability_Barrier : Ability
    {
        public override void Cast(LocalTargetInfo target)
        {
            if(this.pawn.GetComp<CompAbilities>().ReinitShield(this.GetPowerForPawn(), this.def.GetModExtension<AbilityExtension_Shield>()?.shieldTexPath, this.GetDurationForPawn()))
                base.Cast(target);
        }
    }

    public class AbilityExtension_Shield : DefModExtension
    {
        public string shieldTexPath;
    }
}