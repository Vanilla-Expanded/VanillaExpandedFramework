namespace VFECore.Abilities
{
    using System.Linq;
    using RimWorld.Planet;
    using Verse;

    public class Ability_Barrier : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            if(this.pawn.GetComp<CompAbilities>().ReinitShield(this.GetPowerForPawn(), this.def.GetModExtension<AbilityExtension_Shield>()?.shieldTexPath, this.GetDurationForPawn()))
                base.Cast(targets);
        }
    }

    public class AbilityExtension_Shield : DefModExtension
    {
        public string shieldTexPath;
    }
}