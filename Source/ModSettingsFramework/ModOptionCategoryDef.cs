using System.Collections.Generic;
using Verse;

namespace ModSettingsFramework
{
    public class ModOptionCategoryDef : Def
    {
        public string modSettingsName;
        public string modPackageSettingsID;
        public int order;
        public List<string> mods;
    }
}
