using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VFECore
{
	public class ExpandableGraphicData
	{
		[NoTranslate]

		public Type graphicClass;

		public ShaderTypeDef shaderType;

		public List<ShaderParameter> shaderParameters;

		public Color color = Color.white;

		public Color colorTwo = Color.white;

		public bool drawRotated = true;

		public bool allowFlip = true;

		public float flipExtraRotation;

		public ShadowData shadowData;

		public string texPath;
		public string texPathFadeOut;

		private Material[] cachedMaterials;

		private Material[] cachedMaterialsFadeOut;

		private static Dictionary<string, Material[]> loadedMaterials = new Dictionary<string, Material[]>();
		public Material[] Materials
		{
			get
			{
				if (cachedMaterials == null)
				{
					InitMainTextures();
				}
				return cachedMaterials;
			}
		}

		public Material[] MaterialsFadeOut
		{
			get
			{
				if (cachedMaterialsFadeOut == null)
				{
					InitFadeOutTextures();
				}
				return cachedMaterialsFadeOut;
			}
		}
		public void InitMainTextures()
		{
			if (!loadedMaterials.TryGetValue(texPath, out var materials))
			{
				var mainTextures = LoadAllFiles(texPath).OrderBy(x => x).ToList();
				if (mainTextures.Count > 0)
				{
					cachedMaterials = new Material[mainTextures.Count];
					for (var i = 0; i < mainTextures.Count; i++)
					{
						var shader = this.shaderType != null ? this.shaderType.Shader : ShaderDatabase.DefaultShader;
						cachedMaterials[i] = MaterialPool.MatFrom(mainTextures[i], shader, color);
					}
				}
				else
				{
					Log.Error("Error loading materials by this path: " + texPath);
				}
				loadedMaterials[texPath] = cachedMaterials;
			}
			else
            {
				cachedMaterials = materials;
			}
		}
		public void InitFadeOutTextures()
		{
			if (!loadedMaterials.TryGetValue(texPathFadeOut, out var materials))
            {
				var fadeOutTextures = LoadAllFiles(texPathFadeOut).OrderBy(x => x).ToList();
				if (fadeOutTextures.Count > 0)
				{
					cachedMaterialsFadeOut = new Material[fadeOutTextures.Count];
					for (var i = 0; i < fadeOutTextures.Count; i++)
					{
						var shader = this.shaderType != null ? this.shaderType.Shader : ShaderDatabase.DefaultShader;
						cachedMaterialsFadeOut[i] = MaterialPool.MatFrom(fadeOutTextures[i], shader, color);
					}
				}
				loadedMaterials[texPathFadeOut] = cachedMaterialsFadeOut;
			}
			else
            {
				cachedMaterialsFadeOut = materials;
			}
		}

		public List<string> LoadAllFiles(string folderPath)
		{
			var list = new List<string>();
			foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
			{
				foreach (var f in ModContentPack.GetAllFilesForMod(mod, "Textures/" + folderPath))
				{
					var path = f.Value.FullName;
					if (path.EndsWith(".png"))
					{
						path = path.Replace("\\", "/");
						path = path.Substring(path.IndexOf("/Textures/") + 10);
						path = path.Replace(".png", "");
						list.Add(path);
					}
				}
			}
			return list;
		}
	}
}
