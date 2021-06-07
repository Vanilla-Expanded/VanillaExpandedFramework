using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public class FactionSettlement : DefModExtension
    {
        public bool canSpawnSettlements = true;
        public bool useStructureLayout;

        public List<StructureLayoutDef> chooseFromlayouts = new List<StructureLayoutDef>();
        public List<SettlementLayoutDef> chooseFromSettlements = new List<SettlementLayoutDef>();

        public string symbolResolver = null;

        /* More option */
        public bool preGenClear = true;
        public bool fullClear = false;

        public bool tryFindFreeArea = false;
    }
}