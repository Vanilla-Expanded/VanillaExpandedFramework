using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Verse;
using RimWorld;

namespace KCSG
{
    public class FactionSettlement : DefModExtension
    {
        public List<KCSG.SettlementLayoutDef> chooseFrom = new List<KCSG.SettlementLayoutDef>();
        public string symbolResolver = null;

        public static SettlementLayoutDef temp = null;
        public static List<CellRect> tempRectList = null;
    }
}
