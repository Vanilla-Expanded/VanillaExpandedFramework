using System;
using System.IO;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class DoorTeleporterMaterials
    {
        public Texture2D DestroyIcon;
        public Texture2D RenameIcon;
        public Material MainMat;
        public Material DistortionMat;
        public Material maskMat;
        public Texture2D backgroundTex;
        private Pair<Texture2D, Texture2D> GetBackgroundTextures(DoorTeleporterExtension extension, ModContentPack content)
        {
            Texture2D bg = null, mask = null;
            foreach (ModContentPack mod in LoadedModManager.RunningModsListForReading)
            {
                foreach (var t in mod.GetContentHolder<Texture2D>().contentList)
                {
                    if (t.Key == extension.doorTeleporterBackgroundPath)
                    {
                        bg = GetReadableTexture(t.Value);
                    }
                    else if (t.Key == extension.doorTeleporterMaskPath)
                    {
                        mask = GetReadableTexture(t.Value);
                    }
                }
            }
            return new Pair<Texture2D, Texture2D>(bg, mask);
        }

        public static Texture2D GetReadableTexture(Texture2D texture)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, temporary);
            RenderTexture.active = temporary;
            Texture2D texture2D = new Texture2D(texture.width, texture.height);
            texture2D.ReadPixels(new Rect(0f, 0f, (float)temporary.width, (float)temporary.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            return texture2D;
        }

        private void CacheBackground(DoorTeleporterExtension extension, ThingDef def, Texture2D bg, Texture2D mask)
        {
            backgroundTex = bg;
            Texture2D invertedMask = new(bg.width, bg.height);

            for (int x = 0; x < bg.width; x++)
                for (int y = 0; y < bg.height; y++)
                {
                    Color maskColor = mask.GetPixel(x, y);
                    Color result = maskColor.IndistinguishableFromFast(Color.black) || maskColor.r <= extension.maskThreshold
                        ? Color.red
                        : Color.black;
                    invertedMask.SetPixel(x, y, result);
                }

            invertedMask.Apply();
            maskMat = new Material(ShaderDatabase.CutoutComplex) { name = def.defName + "_Static_BackgroundMask", color = Color.clear };
            maskMat.SetTexture(ShaderPropertyIDs.MaskTex, invertedMask);
            maskMat.SetColor(ShaderPropertyIDs.ColorTwo, Color.clear);
        }

        public void Init(ThingDef def)
        {
            var content = def.modContentPack;
            var extension = def.GetModExtension<DoorTeleporterExtension>();
            Pair<Texture2D, Texture2D> pair = GetBackgroundTextures(extension, content);
            CacheBackground(extension, def, pair.First, pair.Second);
            if (extension.destroyIconPath.NullOrEmpty() is false)
            {
                DestroyIcon = ContentFinder<Texture2D>.Get(extension.destroyIconPath);
            }
            if (extension.renameIconPath.NullOrEmpty() is false)
            {
                RenameIcon = ContentFinder<Texture2D>.Get(extension.renameIconPath);
            }
            MainMat = MaterialPool.MatFrom(extension.mainMatPath, ShaderDatabase.TransparentPostLight);
            DistortionMat = DistortedMaterialsPool.DistortedMaterial(extension.distortionMatPath, extension.distortionMaskPath, 0.02f, 1.1f);
        }
    }
}
