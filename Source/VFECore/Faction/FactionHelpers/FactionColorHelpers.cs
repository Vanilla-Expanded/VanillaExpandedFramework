using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFECore
{
    public static partial class FactionHelper
    {
        public static Color? FactionOrIdeoColor(this Faction faction)
        {
            if (ModsConfig.IdeologyActive && faction?.ideos?.PrimaryIdeo?.Color is Color ideoColor)
            {
                return ideoColor;
            }
            else if (faction?.Color is Color factionColor)
            {
                return factionColor;
            }
            return null;
        }
    }
}

