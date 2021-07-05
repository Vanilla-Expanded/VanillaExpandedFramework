using System.Collections.Generic;
using Verse;

namespace KCSG
{
    public class FactionSettlement : DefModExtension
    {
        public bool canSpawnSettlements = true;

        /* Settlement generation */
        public bool useStructureLayout;

        public List<StructureLayoutDef> chooseFromlayouts = new List<StructureLayoutDef>();
        public List<SettlementLayoutDef> chooseFromSettlements = new List<SettlementLayoutDef>();

        public string symbolResolver = null;
        public bool preGenClear = true;
        public bool fullClear = false;

        public bool tryFindFreeArea = false;
    }
}