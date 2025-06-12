using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using VFE.Mechanoids.Needs;
using VFEMech;

namespace VFE.Mechanoids
{
	[HarmonyPatch(typeof(CaravanUIUtility), "AddPawnsSections")]
	public static class CaravanUIUtility_AddPawnsSections_Patch
	{
		public static void Postfix(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
		{
			if (ModsConfig.BiotechActive is false)
            {
                IEnumerable<TransferableOneWay> source = transferables.Where((TransferableOneWay x) => x.ThingDef.category == ThingCategory.Pawn);
                widget.AddSection("VEF.MechsSection".Translate(), source.Where((TransferableOneWay x) 
					=> x.AnyThing is Pawn pawn && pawn.RaceProps.IsMechanoid && pawn.Faction == Faction.OfPlayer));
            }
        }
	}

    [HarmonyPatch]
    public static class CaravanUIUtility_AddPawnsSections_MechSection_Patch
    {
		public static MethodBase TargetMethod()
		{
			foreach (var type in typeof(CaravanUIUtility).GetNestedTypes(AccessTools.all))
			{
				foreach (var method in type.GetMethods(AccessTools.all))
				{
					if (method.Name.Contains("<AddPawnsSections>b__8_6"))
					{
						return method;
					}
				}
			}
			return null;
		}

        public static void Postfix(TransferableOneWay x, ref bool __result)
        {
            __result = x.AnyThing is Pawn pawn && pawn.RaceProps.IsMechanoid && pawn.Faction == Faction.OfPlayer;
        }
    }

    [HarmonyPatch(typeof(Caravan_NeedsTracker), "TrySatisfyPawnNeeds")]
	public static class TrySatisfyPawnNeeds_Patch
	{
		public static void Postfix(Caravan_NeedsTracker __instance, Pawn pawn)
		{
            var need = pawn.needs.TryGetNeed<Need_Power>();
            if (need != null)
            {
                if (need.CurLevel <= 0)
                {
                    PawnBanishUtility.Banish(pawn);
                    pawn.Kill(null);
                    Messages.Message("VFEM.CaravanMachineRanOutPower".Translate(pawn.Named("MACHINE")), __instance.caravan, MessageTypeDefOf.CautionInput);
                }
            }
        }
	}
	
	[StaticConstructorOnStartup]
	[HarmonyPatch(typeof(TransferableUIUtility), "DoExtraIcons")]
	public static class TransferableUIUtility_DoExtraIcons_Patch
	{
		private static float RideableIconWidth = 24f;
	
		private static readonly Texture2D RideableIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Rideable");
		public static void Postfix(Transferable trad, Rect rect, ref float curX)
		{
			Pawn pawn = trad.AnyThing as Pawn;
			if (pawn != null)
            {
				var extension = pawn.def.GetModExtension<MechanoidExtension>();
				if (extension != null)
                {
					if (pawn.IsCaravanRideable())
					{
						Rect rect2 = new Rect(curX - RideableIconWidth, (rect.height - RideableIconWidth) / 2f, RideableIconWidth, RideableIconWidth);
						curX -= rect2.width;
						GUI.DrawTexture(rect2, RideableIcon);
						if (Mouse.IsOver(rect2))
						{
							TooltipHandler.TipRegion(rect2, GetIconTooltipText(pawn));
						}
					}
				}
            }
		}
	
		public static string GetIconTooltipText(Pawn pawn)
		{
			float statValue = pawn.GetStatValue(StatDefOf.CaravanRidingSpeedFactor);
			return "VFEMRideableMachineTip".Translate() + "\n\n" + StatDefOf.CaravanRidingSpeedFactor.LabelCap + ": " + statValue.ToStringPercent();
		}
	}
}
