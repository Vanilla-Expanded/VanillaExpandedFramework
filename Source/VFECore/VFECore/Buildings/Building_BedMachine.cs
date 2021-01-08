using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids.Buildings
{
    public class Building_BedMachine : Building
    {
        public Pawn occupant
        {
            get
            {
                Pawn pawn = this.TryGetComp<CompMachineChargingStation>()?.myPawn;
                if (pawn?.Position == this.Position)
                {
                    return pawn;
                }
                return null;
            }
        }
    }
}
