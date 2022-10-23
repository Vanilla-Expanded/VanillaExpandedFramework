using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace VFECore
{
	public class WeatherOverlayExtension : DefModExtension
	{
		public string overlayPath;
		public string copyPropertiesFrom;
		public float worldOverlayPanSpeed1;
		public Vector2 worldPanDir1;
		public float worldOverlayPanSpeed2;
		public Vector2 worldPanDir2;
	}

	[HarmonyPatch(typeof(WeatherWorker), MethodType.Constructor, new Type[] { typeof(WeatherDef) })]
	public static class WeatherWorker_Constructor_Patch
	{
		public static void Postfix(WeatherWorker __instance, WeatherDef def)
		{
			if (__instance.overlays != null)
			{
				for (int i = __instance.overlays.Count - 1; i >= 0; i--)
				{
					SkyOverlay overlay = __instance.overlays[i];
					if (overlay is WeatherOverlay_Custom)
					{
						__instance.overlays[i] = new WeatherOverlay_Custom
						{
							weatherDef = def
						};
					}
				}
			}
		}
	}

	public class WeatherOverlay_Custom : SkyOverlay
	{
		public WeatherDef weatherDef;

		public WeatherDef curWeather;
		public WeatherOverlay_Custom()
		{

		}
		public override void TickOverlay(Map map)
		{
			if (curWeather != map.weatherManager.curWeather && weatherDef == map.weatherManager.curWeather)
			{
				curWeather = map.weatherManager.curWeather;
				WeatherOverlayExtension extension = curWeather.GetModExtension<WeatherOverlayExtension>();
				if (extension != null)
				{
					worldOverlayPanSpeed1 = extension.worldOverlayPanSpeed1;
					worldPanDir1 = extension.worldPanDir1;
					worldPanDir1.Normalize();
					worldOverlayPanSpeed2 = extension.worldOverlayPanSpeed2;
					worldPanDir2 = extension.worldPanDir2;
					worldPanDir2.Normalize();
					worldOverlayMat = new Material(MaterialPool.MatFrom(extension.overlayPath));
					Material mat = new(MatLoader.LoadMat(extension.copyPropertiesFrom));
					worldOverlayMat.CopyPropertiesFromMaterial(mat);
					worldOverlayMat.shader = mat.shader;
					Texture2D texture = ContentFinder<Texture2D>.Get(extension.overlayPath);
					worldOverlayMat.SetTexture("_MainTex", texture);
					worldOverlayMat.SetTexture("_MainTex2", texture);
				}
			}
			base.TickOverlay(map);
		}
	}
}
