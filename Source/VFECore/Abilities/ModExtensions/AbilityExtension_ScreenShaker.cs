namespace VFECore.Abilities
{
    using RimWorld;
    using Verse;

    public class AbilityExtension_ScreenShaker : AbilityExtension_AbilityMod
	{
		public float intensity;
		public override void Cast(LocalTargetInfo target, Ability ability)
		{
			Find.CameraDriver.shaker.DoShake(intensity);
		}
	}
}