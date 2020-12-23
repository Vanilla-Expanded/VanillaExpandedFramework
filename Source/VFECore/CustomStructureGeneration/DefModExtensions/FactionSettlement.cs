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
        public bool useStructureLayout;

        public List<KCSG.StructureLayoutDef> chooseFromlayouts = new List<KCSG.StructureLayoutDef>();
        public List<KCSG.SettlementLayoutDef> chooseFromSettlements = new List<KCSG.SettlementLayoutDef>();
        
        public string symbolResolver = null;

        // Not for users use
        public static string temp = null;
        public static bool tempUseStructureLayout;
    }
}
