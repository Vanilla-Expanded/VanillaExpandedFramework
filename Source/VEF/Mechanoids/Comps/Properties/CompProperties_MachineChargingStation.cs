using RimWorld;
using System.Collections.Generic;
using Verse;
using VEF.Pawns;

namespace VFE.Mechanoids
{
    public class CompProperties_MachineChargingStation : CompProperties_PawnDependsOn
    {
        public List<WorkGiverDef> disallowedWorkGivers;
        public int skillLevel = 5;
        public bool draftable=false;
        public float extraChargingPower;
        public ThingDef spawnWithWeapon = null;
        public bool turret = false;
        public float hoursToRecharge = 24;
        public bool showSetArea = true;
        public List<string> blackListTurretGuns = new List<string>();
        public CompProperties_MachineChargingStation()
        {
            this.compClass = typeof(CompMachineChargingStation);
        }
    }
}
