using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE.Mechanoids
{
    public class CompProperties_Machine : CompProperties
    {
        public bool violent;
        public bool canPickupWeapons;
        public float hoursActive = 24;
        public bool canUseTurrets;
        public List<string> blackListTurretGuns = new List<string>();
        public CompProperties_Machine()
        {
            this.compClass = typeof(CompMachine);
        }
    }
}
