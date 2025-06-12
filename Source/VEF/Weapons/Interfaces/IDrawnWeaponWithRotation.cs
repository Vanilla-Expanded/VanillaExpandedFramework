using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VEF.Weapons
{
    interface IDrawnWeaponWithRotation
    {
        float RotationOffset
        {
            get;
            set;
        }
    }
}
