using System.Collections.Generic;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace VFECore
{
    /// <summary>
    /// Just a reference class for comp signals.
    /// </summary>
    public static class CompSignals
    {
        public static readonly string PowerTurnedOff = "PowerTurnedOff";
        public static readonly string PowerTurnedOn = "PowerTurnedOn";
    }
    public enum TempControlType : byte
    {
        None = 0,
        Heater = 1,
        Cooler = 2,
        Both = 3
    }
    [StaticConstructorOnStartup]
    public static class ActiveTerrainUtility
    {
    	/// <summary>
    	/// .cctor gets the material for needs power
    	/// </summary>
        static ActiveTerrainUtility()
        {
            NeedsPowerMat = MaterialPool.MatFrom("UI/Overlays/NeedsPower", ShaderDatabase.MetaOverlay);
        }

        //Misc

        /// <summary>
        /// Generates number between 0 and mod - 1 (inclusive) based on object's hash code.
        /// </summary>
        public static int HashCodeToMod(this object obj, int mod)
        {
            return Math.Abs(obj.GetHashCode()) % mod;
        }

        public static CompTempControl GetTempControl(this Room room, TempControlType targetType)
        {
            foreach (var c in room.Cells)
            {
                Building building = c.GetFirstBuilding(room.Map);
                if (building != null && building.Powered())
                {
                    var comp = building.GetComp<CompTempControl>();
                    if (comp != null)
                    {
                        if ((byte)(comp.AnalyzeType() & targetType) != 0)
                        {
                            return comp;
                        }
                    }
                }
            }
            return null;
        }
        public static TempControlType AnalyzeType(this CompTempControl tempControl)
        {
            float f = tempControl.Props.energyPerSecond;
            return f > 0 ? TempControlType.Heater : f < 0 ? TempControlType.Cooler : TempControlType.None;
        }
        public static TempControlType AnalyzeType(this TerrainComp_TempControl tempControl)
        {
            float f = tempControl.Props.energyPerSecond;
            return f > 0 ? TempControlType.Heater : f < 0 ? TempControlType.Cooler : TempControlType.None;
        }

        //Power related

        public static bool Powered(this ThingWithComps t)
        {
            return t.GetComp<CompPowerTrader>()?.PowerOn ?? true;
        }
        public static bool Powered(this TerrainInstance t)
        {
            return t.GetComp<TerrainComp_PowerTrader>()?.PowerOn ?? true;
        }

        /// <summary>
        /// Material for needs power
        /// </summary>
        public static readonly Material NeedsPowerMat;
        
        /// <summary>
        /// Render needs power overlay for terrain
        /// </summary>
        /// <param name="loc">Render location</param>
        public static void RenderPulsingNeedsPowerOverlay(IntVec3 loc)
        {
            Vector3 drawPos = loc.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
            float num = (Time.realtimeSinceStartup + 397f * loc.HashCodeToMod(37)) * 4f;
            float num2 = ((float)Math.Sin(num) + 1f) * 0.5f;
            num2 = 0.3f + num2 * 0.7f;
            Material material = FadedMaterialPool.FadedVersionOf(NeedsPowerMat, num2);
            Graphics.DrawMesh(MeshPool.plane08, drawPos, Quaternion.identity, material, 0);
        }
        
        /// <summary>
        /// Trying to be as close to the core algorithm as possible.
        /// </summary>
        public static CompPowerTraderFloor TryFindNearestPowerConduitFloor(IntVec3 center, Map map)
        {
        	var cellRect = CellRect.CenteredOn(center, 6);
        	Building best = null;
        	float bestDist = 3.40282347E+38f;
        	for (int i = cellRect.minZ; i <= cellRect.maxZ; i++) 
        	{
        		for (int j = cellRect.minX; j <= cellRect.maxX; j++)
        		{
        			var loc = new IntVec3(j, 0, i);
        			var transmitter = loc.GetTransmitter(map);
        			if (transmitter != null && transmitter.GetComp<CompPowerTraderFloor>() != null)
        			{
        				var distance = (loc - center).LengthHorizontalSquared;
        				if (bestDist > distance)
        				{
        					best = transmitter;
        					bestDist = distance;
        				}
        			}
        		}
        	}
        	return best?.GetComp<CompPowerTraderFloor>();
        }

        //Terrain instance

        public static TerrainInstance MakeTerrainInstance(this ActiveTerrainDef tDef, Map map, IntVec3 loc)
        {
            var terr = (TerrainInstance)Activator.CreateInstance(tDef.terrainInstanceClass);
            terr.def = tDef;
            terr.Map = map;
            terr.Position = loc;
            return terr;
        }
    }
}
