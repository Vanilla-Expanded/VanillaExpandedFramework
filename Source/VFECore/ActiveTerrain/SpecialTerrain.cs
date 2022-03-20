using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace VFECore
{
    public class ActiveTerrainDef : TerrainDef
    {
        public List<TerrainCompProperties> terrainComps = new List<TerrainCompProperties>();

        public Type terrainInstanceClass = typeof(TerrainInstance);

        public TickerType tickerType;
        /// <summary>
        /// Gets the terrain comp properties for a SpecialTerrain.
        /// </summary>
        /// <returns>The terrain comp properties of the type T.</returns>
        public T GetCompProperties<T>() where T : TerrainCompProperties
        {
            for (int i = 0; i < terrainComps.Count; i++)
            {
                if (terrainComps[i] is T t)
                    return t;
            }
            return null;
        }
    }
    public class TerrainInstance : IExposable
    {
    	public ActiveTerrainDef def;

    	public List<TerrainComp> comps = new List<TerrainComp>();

        //Local private variables
        Map mapInt;
        IntVec3 positionInt;

        /// <summary>
        /// These instances stay even after removed. Note: Changing the values of these instances won't do anything to the terrain grid.
        /// </summary>
    	public Map Map { get { return mapInt; } set { mapInt = value; } }
        /// <summary>
        /// These instances stay even after removed. Note: Changing the values of these instances won't do anything to the terrain grid.
        /// </summary>
    	public IntVec3 Position { get { return positionInt; } set { positionInt = value; } }
        public TerrainInstance() { }

        public virtual void Init()
        {
            InitializeComps();
        }

        public virtual string Label
        {
            get
            {
                var label = def.LabelCap;
                for (int i = 0; i < comps.Count; i++)
                {
                    label = comps[i].TransformLabel(label);
                }
                return label;
            }
        }

        /// <summary>
        /// Gets the terrain comp for a SpecialTerrain.
        /// </summary>
        /// <returns>The terrain comp of the type T.</returns>
        public T GetComp<T>() where T : TerrainComp
        {
            for (int i = 0; i < comps.Count; i++)
            {
                if (comps[i] is T t)
                    return t;
            }
            return null;
        }

        public void InitializeComps()
    	{
    		foreach (var prop in def.terrainComps)
    		{
                var comp = (TerrainComp)Activator.CreateInstance(prop.compClass);
                comp.parent = this;
                comps.Add(comp);
                comp.Initialize(prop);
    		}
        }
        /// <summary>
        /// Ticker for terrain
        /// </summary>
    	public virtual void Tick()
    	{
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].CompTick();
            }
        }

        public virtual void TickRare()
        {
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].CompTick();
            }
        }
        public virtual void TickLong()
        {
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].CompTick();
            }
        }

        /// <summary>
        /// Similar to Draw() in regular Things
        /// </summary>
    	public virtual void Update()
        {
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].CompUpdate();
            }
        }
        /// <summary>
        /// Called when terrain is set in-game. Does not trigger when loading
        /// </summary>
    	public virtual void PostPlacedDown()
        {
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].PlaceSetup();
            }
        }
        /// <summary>
        /// Called when terrain is removed.
        /// </summary>
    	public virtual void PostRemove()
        {
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].PostRemove();
            }
        }
        /// <summary>
        /// Called when terrain is instantiated after load.
        /// </summary>
        public virtual void PostLoad()
        {
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].PostPostLoad();
            }
        }
        /// <summary>
        /// Comp signals - just like regular comps in game
        /// </summary>
        public virtual void BroadcastCompSignal(string sig)
        {
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].ReceiveCompSignal(sig);
            }
            //MoteMaker.ThrowText(Position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteLow), Map, sig, 1);
            //Above line for debugging purposes
        }
        /// <summary>
        /// Saving/loading
        /// </summary>
        public virtual void ExposeData()
        {
            Scribe_References.Look(ref mapInt, "map");
            Scribe_Values.Look(ref positionInt, "pos");
            Scribe_Defs.Look(ref def, "def");
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                InitializeComps();
            }
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].PostExposeData();
            }
        }
    }
}
