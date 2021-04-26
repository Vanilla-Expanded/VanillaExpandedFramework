using System;
using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;


namespace AnimalBehaviours
{

	[HarmonyPatch(typeof(Dialog_InfoCard), "FillCard")]
	public static class Dialog_InfoCard_FillCard_Patch
	{
		public static AnimalStatExtension extension;
		public static Rect rect;

		public static bool Prefix(Rect cardRect, Dialog_InfoCard __instance, Thing ___thing)
		{
			Pawn pawn = ___thing as Pawn;
			rect = cardRect;
			bool result;
			
			if (pawn == null)
			{
				
				result = true;
			}
			else
			{
				
				if (pawn.def.GetModExtension<AnimalStatExtension>() != null)
				{
					
					extension = pawn.def.GetModExtension<AnimalStatExtension>();
					if (extension.showImageInInfoCard)
					{
						Type typ = typeof(Dialog_InfoCard);
						FieldInfo type = typ.GetField("tab", BindingFlags.Instance | BindingFlags.NonPublic);
						Dialog_InfoCard.InfoCardTab tab = (Dialog_InfoCard.InfoCardTab)type.GetValue(__instance);
						if (tab == Dialog_InfoCard.InfoCardTab.Stats) {
							Texture2D texture = ContentFinder<Texture2D>.Get(extension.ImageToShowInInfoCard, false);
							Rect position = rect.AtZero();
							position.width = 384f;
							position.height = 576f;
							position.x = rect.width * 0.75f - position.width / 2f + 18f;
							position.y = rect.center.y - position.height / 2f + 120;
							GUI.DrawTexture(position, texture, ScaleMode.ScaleToFit, true);
						}
						
						

					}

				}
				result = true;
			}


			return result;
		}


	}

	
}