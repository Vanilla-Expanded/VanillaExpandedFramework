using UnityEngine;
using Verse;

namespace VFECore
{
	public class WeatherOverlayExtensionTwo : DefModExtension
    {
		public string overlayPath;
		public string copyPropertiesFrom;
		public float worldOverlayPanSpeed1;
		public Vector2 worldPanDir1;
		public float worldOverlayPanSpeed2;
		public Vector2 worldPanDir2;
	}
	public class WeatherOverlay_CustomTwo : SkyOverlay
	{
		public WeatherDef curWeather;
		public WeatherOverlay_CustomTwo()
		{

		}
		public override void TickOverlay(Map map)
		{
			if (curWeather != map.weatherManager.curWeather)
			{
				curWeather = map.weatherManager.curWeather;
				var extension = curWeather.GetModExtension<WeatherOverlayExtensionTwo>();
				if (extension != null)
                {
					worldOverlayPanSpeed1 = extension.worldOverlayPanSpeed1;
					worldPanDir1 = extension.worldPanDir1;
					worldPanDir1.Normalize();
					worldOverlayPanSpeed2 = extension.worldOverlayPanSpeed2;
					worldPanDir2 = extension.worldPanDir2;
					worldPanDir2.Normalize();
					worldOverlayMat = MaterialPool.MatFrom(extension.overlayPath);
					var mat = MatLoader.LoadMat(extension.copyPropertiesFrom);
					worldOverlayMat.CopyPropertiesFromMaterial(mat);
					worldOverlayMat.shader = mat.shader;
					var texture = ContentFinder<Texture2D>.Get(extension.overlayPath);
					worldOverlayMat.SetTexture("_MainTex", texture);
					worldOverlayMat.SetTexture("_MainTex2", texture);
				}
			}
			base.TickOverlay(map);
		}
	}
}
