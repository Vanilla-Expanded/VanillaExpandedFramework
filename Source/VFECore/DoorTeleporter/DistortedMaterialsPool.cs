using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFECore
{
    public class DistortedMaterialsPool
    {
        public static Material DistortedMaterial(string matPath, string texPath, float intesity, float brightness)
        {
            Material mat = MaterialPool.MatFrom(new MaterialRequest
            {
                mainTex = ContentFinder<Texture2D>.Get(matPath),
                shader = ShaderDatabase.MoteGlowDistortBG,
                color = Color.white,
                shaderParameters = new List<ShaderParameter>
                {
                    CreateShaderParam("_DistortionTex",        ContentFinder<Texture2D>.Get(texPath)),
                    CreateShaderParam("_distortionIntensity",  intesity),
                    CreateShaderParam("_brightnessMultiplier", brightness)
                }
            });
            return mat;
        }

        private static ShaderParameter CreateShaderParam(string name, float value)
        {
            ShaderParameter result = new();
            Traverse traverse = Traverse.Create(result);
            traverse.Field("name").SetValue(name);
            traverse.Field("value").SetValue(Vector4.one * value);
            traverse.Field("type").SetValue(0);
            return result;
        }

        private static ShaderParameter CreateShaderParam(string name, Vector4 value)
        {
            ShaderParameter result = new();
            Traverse traverse = Traverse.Create(result);
            traverse.Field("name").SetValue(name);
            traverse.Field("value").SetValue(value);
            traverse.Field("type").SetValue(1);
            return result;
        }

        private static ShaderParameter CreateShaderParam(string name, Matrix4x4 value) => throw new NotImplementedException();

        private static ShaderParameter CreateShaderParam(string name, Texture2D value)
        {
            ShaderParameter result = new();
            Traverse traverse = Traverse.Create(result);
            traverse.Field("name").SetValue(name);
            traverse.Field("valueTex").SetValue(value);
            traverse.Field("type").SetValue(3);
            return result;
        }
    }
}
