namespace VFECore.Abilities
{
	using RimWorld;
	using RimWorld.Planet;
	using Verse;

	public class AbilityExtension_ScreenShaker : AbilityExtension_AbilityMod
	{
		public float intensity;

		public override void Cast(GlobalTargetInfo[] targets, Ability ability)
		{
			Find.CameraDriver.shaker.DoShake(intensity);
		}
	}
}