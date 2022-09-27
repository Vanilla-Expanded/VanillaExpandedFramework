
using RimWorld;
using Verse;

namespace VanillaGenesExpanded
{

	public class Gene_AddsHediff : Gene
	{
		public override void PostAdd()
		{
			base.PostAdd();
			pawn.health.AddHediff(this.def.hediff);

		}
	}
}
