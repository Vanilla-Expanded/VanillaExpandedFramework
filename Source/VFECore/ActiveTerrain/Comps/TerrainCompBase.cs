using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace VFECore
{
    public class TerrainCompProperties
    {
    	public Type compClass;
    }
    public class TerrainComp
    {
    	public TerrainInstance parent;
        public TerrainCompProperties props;

        public virtual void Initialize(TerrainCompProperties props)
    	{
            this.props = props;
    	}

        /// <summary>
        /// Allows comps to add text to label
        /// </summary>
        public virtual string TransformLabel(string label) { return label; }
        /// <summary>
        /// Ticker for terrain comps
        /// </summary>
    	public virtual void CompTick() { }
        /// <summary>
        /// Similar to Draw() in regular Things
        /// </summary>
    	public virtual void CompUpdate() { }
        /// <summary>
        /// Called when terrain is set in-game.
        /// </summary>
    	public virtual void PlaceSetup() { }
        /// <summary>
        /// Called when terrain is removed.
        /// </summary>
    	public virtual void PostRemove() { }
        /// <summary>
        /// Saving/loading
        /// </summary>
        public virtual void PostExposeData() { }
        /// <summary>
        /// Comp signals - just like regular comps in game
        /// </summary>
        public virtual void ReceiveCompSignal(string sig) { }
        /// <summary>
        /// Called when terrain is instantiated after load.
        /// </summary>
        public virtual void PostPostLoad() { }
    }
}
