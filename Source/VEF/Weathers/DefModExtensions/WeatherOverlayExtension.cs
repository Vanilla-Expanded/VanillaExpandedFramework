using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace VEF.Weathers
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

   
}
