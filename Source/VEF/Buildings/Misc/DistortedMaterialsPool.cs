using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VEF.Buildings
{
    public class DistortedMaterialsPool
    {
        private static readonly AccessTools.FieldRef<ShaderParameter, string> nameField = AccessTools.FieldRefAccess<ShaderParameter, string>("name");
        private static readonly AccessTools.FieldRef<ShaderParameter, Vector4> valueField = AccessTools.FieldRefAccess<ShaderParameter, Vector4>("value");
        private static readonly AccessTools.FieldRef<ShaderParameter, Texture2D> valueTexField = AccessTools.FieldRefAccess<ShaderParameter, Texture2D>("valueTex");
        private static readonly AccessTools.FieldRef<ShaderParameter, int> typeField = AccessTools.FieldRefAccess<ShaderParameter, int>("type");

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
            nameField(result) = name;
            valueField(result) = Vector4.one * value;
            typeField(result) = 0;
            return result;
        }

        private static ShaderParameter CreateShaderParam(string name, Vector4 value)
        {
            ShaderParameter result = new();
            nameField(result) = name;
            valueField(result) = value;
            typeField(result) = 1;
            return result;
        }

        private static ShaderParameter CreateShaderParam(string name, Matrix4x4 value) => throw new NotImplementedException();

        private static ShaderParameter CreateShaderParam(string name, Texture2D value)
        {
            ShaderParameter result = new();
            nameField(result) = name;
            valueTexField(result) = value;
            typeField(result) = 3;
            return result;
        }
    }
}
