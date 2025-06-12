using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace VEF.Weathers
{
	

	public class WeatherOverlay_Custom : WeatherOverlayDualPanner
    {
		public WeatherDef weatherDef;

		public WeatherDef curWeather;
		public WeatherOverlay_Custom()
		{

		}
		public override void TickOverlay(Map map, float lerpFactor)
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
			base.TickOverlay(map, lerpFactor);
		}
	}
}
