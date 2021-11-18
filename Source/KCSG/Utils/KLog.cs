using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace KCSG
{
    public class KLog
    {
        public static void Message(string message)
        {
            if (VFECore.VFEGlobal.settings.enableVerboseLogging) Log.Message(message);
        }
    }
}
