using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace HeavyWeapons
{
    public class HeavyWeapon : DefModExtension
    {
        public List<string> supportedArmors;
        public int weaponHitPointsDeductionOnShot;
        public bool isHeavy;
    }
}
