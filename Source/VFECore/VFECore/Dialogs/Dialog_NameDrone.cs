using RimWorld;
using Verse;
using VFEMech;

namespace VFECore
{
	// Dialog_NamePawn only supports persons or animals. Drones need a separate (but simpler) implementation.
	public class Dialog_NameDrone : Dialog_Rename<Machine>
	{
		private readonly Machine drone;

		public Dialog_NameDrone(Machine pawn) : base(pawn)
		{
			drone = pawn;
			curName = drone.Name.ToStringShort;
		}

        protected override void OnRenamed(string name)
        {
            base.OnRenamed(name);
            Messages.Message("VFEM.DroneGainsName".Translate((NamedArgument)name), (Thing)drone,
				MessageTypeDefOf.PositiveEvent, false);
        }
	}
}