using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore;

namespace VFE.Mechanoids
{
    public class CompProperties_MachineChargingStation : CompProperties_PawnDependsOn
    {
        public List<WorkTypeDef> allowedWorkTypes;
        public int skillLevel = 5;
        public bool draftable=false;
        public float extraChargingPower;
        public ThingDef spawnWithWeapon = null;
        public bool turret = false;
        public float hoursToRecharge = 24;
        public List<string> blackListTurretGuns = new List<string>();
        public CompProperties_MachineChargingStation()
        {
            this.compClass = typeof(CompMachineChargingStation);
        }
    }
}
