using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VFE.Mechanoids;
using VFE.Mechanoids.Needs;
using VFEMech;

namespace VFEMech
{
	public class ThinkNode_ConditionalHasPower : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.needs.TryGetNeed<Need_Power>().CurLevel > 0;
		}
	}
}

