using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VFECore
{
    public class WorldComponent_DoorTeleporterManager : WorldComponent
    {
        public static WorldComponent_DoorTeleporterManager Instance;

        private HashSet<DoorTeleporter> doorTeleporters = new();
        public WorldComponent_DoorTeleporterManager(World world) : base(world) => Instance = this;
        public HashSet<DoorTeleporter> DoorTeleporters
        {
            get
            {
                this.doorTeleporters.RemoveWhere(doorTeleporter => doorTeleporter is not { Spawned: true });
                return this.doorTeleporters;
            }
        }
    }
}
