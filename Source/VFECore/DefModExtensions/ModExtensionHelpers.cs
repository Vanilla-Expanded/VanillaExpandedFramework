using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFECore
{
    public static class ModExtensionHelpers
    {
        public static List<T> GetModExtensions<T>(this Def def) where T : DefModExtension
        {
            if (def.modExtensions.NullOrEmpty()) return [];
            return def.modExtensions.OfType<T>().ToList();
        }
        public static bool TryGetModExtensions<T>(this Def def, out List<T> extension) where T : DefModExtension
        {
            if (def.modExtensions.NullOrEmpty())
            {
                extension = [];
                return false;
            }
            extension = def.modExtensions.OfType<T>().ToList();
            return extension.Count > 0;
        }
    }
}
