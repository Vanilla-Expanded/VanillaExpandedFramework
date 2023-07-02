using HarmonyLib;
using KTrie;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.AccessTools;

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

        private static readonly FieldRef<ModContentHolder<Texture2D>, StringTrieSet> contentListTrieRef =
    AccessTools.FieldRefAccess<ModContentHolder<Texture2D>, StringTrieSet>("contentListTrie");
        public List<string> LoadAllFiles(string folderPath)
		{
            var paths = new List<string>();
            var mods = LoadedModManager.RunningModsListForReading;
            var count = mods.Count;

            for (var i = 0; i < count; i++)
            {
                var contentListTrie = contentListTrieRef.Invoke(mods[i].GetContentHolder<Texture2D>());

                var prefix = !folderPath.NullOrEmpty() && folderPath![folderPath.Length - 1] == '/'
                    ? folderPath
                    : folderPath + '/';

                foreach (var path in contentListTrie.GetByPrefix(prefix))
                    paths.Add(path);
            }
            return paths;
        }
	}
}
