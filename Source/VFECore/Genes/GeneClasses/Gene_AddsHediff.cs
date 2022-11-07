
using RimWorld;
using Verse;

namespace VanillaGenesExpanded
{

	public class Gene_AddsHediff : Gene
	{
		public override void PostAdd()
		{
			base.PostAdd();
			GeneExtension extension = this.def.GetModExtension<GeneExtension>();
			if (extension?.hediffToWholeBody != null)
            {
				pawn.health.AddHediff(extension?.hediffToWholeBody);

			}
			if (extension?.hediffsToBodyParts != null)
			{
				
				foreach(HediffToBodyparts hediffToBodypart in extension?.hediffsToBodyParts)
                {
					int enumerator = 0;
					foreach (BodyPartDef bodypart in hediffToBodypart.bodyparts)
                    {
						if (!pawn.RaceProps.body.GetPartsWithDef(bodypart).EnumerableNullOrEmpty())
						{
							if(enumerator<= pawn.RaceProps.body.GetPartsWithDef(bodypart).Count) {
								pawn.health.AddHediff(hediffToBodypart.hediff, pawn.RaceProps.body.GetPartsWithDef(bodypart).ToArray()[enumerator]);
								enumerator++;
							}
							
						}

					}
                }
			}


		}

        public override void PostRemove()
        {
            base.PostRemove();
			GeneExtension extension = this.def.GetModExtension<GeneExtension>();
			if (extension?.hediffToWholeBody != null)
			{
				pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(extension?.hediffToWholeBody));
			}

			
		}
    }
}
