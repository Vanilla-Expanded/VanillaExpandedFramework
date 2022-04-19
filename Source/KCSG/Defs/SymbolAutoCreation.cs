using System;
using System.Collections.Generic;
using Verse;

namespace KCSG
{
    [Obsolete]
    class SymbolAutoCreation : Def
    {
        public bool autoSymbolsCreation = false;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var msg in base.ConfigErrors())
                yield return msg;

            yield return $"SymbolAutoCreation def will soon be deprecated. {modContentPack.Name} need to swith to regular SymbolDefs.";
        }
    }
}
