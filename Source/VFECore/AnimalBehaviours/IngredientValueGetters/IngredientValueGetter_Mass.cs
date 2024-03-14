
using RimWorld;
using Verse;
namespace AnimalBehaviours
{
	public class IngredientValueGetter_Mass : IngredientValueGetter
	{
		public override float ValuePerUnitOf(ThingDef t)
		{
			if (t.BaseMass!=0)
			{
				return t.BaseMass;
			}
			return 1f;
		}

		public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
		{
			
			return "VEF_BillRequiresMass".Translate(ing.GetBaseCount());
		}

		
	}
}