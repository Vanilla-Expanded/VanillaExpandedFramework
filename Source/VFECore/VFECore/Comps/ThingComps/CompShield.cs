using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace VFECore
{
    public class CompShield : ThingComp
    {
        public bool equippedOffHand;
        public CompProperties_Shield Props => (CompProperties_Shield)props;

        public Pawn EquippingPawn
        {
            get
            {
                if (this.parent is Apparel equipment)
                    return equipment.Wearer;
                return null;
            }
        }
        public bool UsableNow
        {
            get
            {
                if (EquippingPawn != null)
                {
                    var equipment = EquippingPawn.equipment;

                    // Too few hands
                    if (!EquippingPawn.CanUseShields())
                        return false;

                    var primary = equipment.Primary;
                    if (primary != null)
                    {
                        // Dual wielding - has offhand
                        if (ModCompatibilityCheck.DualWield && NonPublicMethods.DualWield.Ext_Pawn_EquipmentTracker_TryGetOffHandEquipment(EquippingPawn.equipment,
                            out ThingWithComps offHand))
                            return false;

                        // Pawn's primary is usable with shields or not usable with shields
                        if (primary.def.UsableWithShields())
                            return true;
                        else
                            return false;
                    }
                }

                // No pawn or primary, therefore can be used
                return true;
            }
        }

        public bool CoversBodyPart(BodyPartRecord partRec)
        {
            if (Props.coveredBodyPartGroups == null)
                return false;

            // Go through each covered body part group in Props and each body part group within partRec; return if there are any matches
            return Props.coveredBodyPartGroups.Any(p => partRec.groups.Any(p2 => p == p2));
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            try
            {
                // Conversion from old shields to new ones (thing class is changed)
                if (respawningAfterLoad && this.parent.GetType() == typeof(ThingWithComps) && this.parent.def.thingClass != typeof(ThingWithComps))
                {
                    var newShield = ThingMaker.MakeThing(ThingDef.Named(this.parent.def.defName), this.parent.Stuff) as Apparel_Shield;
                    newShield.HitPoints = this.parent.HitPoints;
                    if (this.parent.TryGetQuality(out QualityCategory quality))
                    {
                        newShield.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
                    }
                    GenSpawn.Spawn(newShield, this.parent.Position, this.parent.Map);
                    this.parent.Destroy(DestroyMode.Vanish);
                }
            }
            catch { }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref equippedOffHand, "equippedOffHand");
            base.PostExposeData();
            // Conversion from old shields to new ones (thing class is changed)
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.parent.GetType() == typeof(ThingWithComps) && this.parent.def.thingClass != typeof(ThingWithComps))
            {
                try
                {
                    if (this.ParentHolder is Pawn_EquipmentTracker eq)
                    {
                        var newShield = ThingMaker.MakeThing(ThingDef.Named(this.parent.def.defName), this.parent.Stuff) as Apparel_Shield;
                        newShield.HitPoints = this.parent.HitPoints;
                        if (this.parent.TryGetQuality(out QualityCategory quality))
                        {
                            newShield.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
                        }
                        eq.Remove(this.parent);
                        eq.pawn.AddShield(newShield);
                        this.parent.Destroy(DestroyMode.Vanish);
                    }
                    else if (this.ParentHolder is Pawn_ApparelTracker ap)
                    {
                        var newShield = ThingMaker.MakeThing(ThingDef.Named(this.parent.def.defName), this.parent.Stuff) as Apparel_Shield;
                        newShield.HitPoints = this.parent.HitPoints;
                        if (this.parent.TryGetQuality(out QualityCategory quality))
                        {
                            newShield.TryGetComp<CompQuality>()?.SetQuality(quality, ArtGenerationContext.Colony);
                        }
                        ap.Remove(this.parent as Apparel);
                        ap.pawn.AddShield(newShield);
                        this.parent.Destroy(DestroyMode.Vanish);
                    }
                }
                catch { }
            }
        }
    }
}
