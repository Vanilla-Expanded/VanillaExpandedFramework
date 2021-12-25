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
        public List<string> supportedTraits;
        public List<string> supportedArmors;
        public string disableOptionLabelKey;
        public int weaponHitPointsDeductionOnShot;
        public bool isHeavy;
    }
}
