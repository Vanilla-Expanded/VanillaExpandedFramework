using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse;
using System.Linq;


namespace ItemProcessor
{
    [StaticConstructorOnStartup]
    public class Command_SetFirstItemList : Command
    {

        public Map map;
        public Building_ItemProcessor building;
        public List<Thing> things;



        public Command_SetFirstItemList()
        {
            //Loops through all things selected right now to see if it finds an item processor building
            //Just in case, item processors disable its Gizmos (buttons) if there are more things selected
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (building.firstItem == "")
                    {
                        //If a first ingredient hasn't been chosen yet, show an empty "choose ingredient" button. This is configurable in each building
                        icon = ContentFinder<Texture2D>.Get(building.compItemProcessor.Props.chooseIngredientsIcon, true);
                        defaultLabel = "IP_ChooseIngredient".Translate();
                    }
                    foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 1)))
                    {
                        foreach (string item in element.items)
                        {
                            //If a first ingredient has been chosen, then show the graphic of that ingredient 

                            if (building.firstItem == item)
                            {
                                if (building.compItemProcessor.Props.isCategoryBuilding)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingCategoryDef.Named(item).iconPath, true);
                                    defaultLabel = "IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap);
                                }
                                else
                                {
                                    if (ThingDef.Named(item).graphicData.graphicClass == typeof(Graphic_StackCount))
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName + "_b", true);

                                    }
                                    else
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path, true);
                                    }

                                    defaultLabel = "IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap);
                                }


                            }
                        }
                    }
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            //This is for 2-3 slot recipes that allow one of the ingredients to be empty
            if (building.GetComp<CompItemProcessor>().Props.acceptsNoneAsInput)
            {
                list.Add(new FloatMenuOption("IP_None".Translate(), delegate
                {
                    building.firstItem = "None";
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            //For the rest, search for every recipe that is from this building, and this slot
            foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 1)))
            {
                foreach (string item in element.items.Where(item => item != "None"))
                {
                    if (building.compItemProcessor.Props.isCategoryBuilding)
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap), delegate
                        {
                            if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertFirstThing(item); }
                        }, MenuOptionPriority.Default, null, null, 29f, null, null));

                    }
                    else
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap), delegate
                        {
                            things = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed(item, true));
                            if (things.Count > 0)
                            {
                                if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertFirstThing(); }

                            }
                            else
                            {
                                Messages.Message("IP_CantFindThing".Translate(ThingDef.Named(item).LabelCap), null, MessageTypeDefOf.NegativeEvent, true);
                            }

                        }, MenuOptionPriority.Default, null, null, 29f, null, null));
                    }



                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertFirstThing(string item = "")
        {
            if (item != "")
            {
                building.firstCategory = ThingCategoryDef.Named(item).defName;
                building.firstItem = building.firstCategory;
            }
            else
            {
                building.firstItem = things.RandomElement().def.defName;
            }
            if (building.compItemProcessor.Props.isSemiAutomaticMachine)
            {
                if (building.compPowerTrader != null && !building.compPowerTrader.PowerOn && building.compItemProcessor.Props.noPowerDestroysProgress)
                {
                    Messages.Message("IP_NoPowerDestroysWarning".Translate(building.def.LabelCap), building, MessageTypeDefOf.NegativeEvent, true);
                }
                else if (building.compFuelable != null && !building.compFuelable.HasFuel && building.compItemProcessor.Props.noPowerDestroysProgress)
                {
                    Messages.Message("IP_NoFuelDestroysWarning".Translate(building.def.LabelCap), building, MessageTypeDefOf.NegativeEvent, true);
                }
                else
                {
                    building.IngredientsChosenBringThemIn();
                }

            }
            else
            {
                building.processorStage = ProcessorStage.IngredientsChosen;

            }



        }




    }

    [StaticConstructorOnStartup]
    public class Command_SetSecondItemList : Command
    {

        public Map map;
        public Building_ItemProcessor building;
        public List<Thing> things;



        public Command_SetSecondItemList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (building.secondItem == "")
                    {
                        icon = ContentFinder<Texture2D>.Get(building.compItemProcessor.Props.chooseIngredientsIcon, true);
                        defaultLabel = "IP_ChooseIngredientSecond".Translate();
                    }
                    foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 2)))
                    {
                        foreach (string item in element.items)
                        {
                            if (building.secondItem == item)
                            {
                                if (building.compItemProcessor.Props.isCategoryBuilding)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingCategoryDef.Named(item).iconPath, true);
                                    defaultLabel = "IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap);
                                }
                                else
                                {
                                    if (ThingDef.Named(item).graphicData.graphicClass == typeof(Graphic_StackCount))
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName + "_b", true);

                                    }
                                    else
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path, true);
                                    }
                                }


                            }
                        }
                    }
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            if (building.GetComp<CompItemProcessor>().Props.acceptsNoneAsInput)
            {
                list.Add(new FloatMenuOption("IP_None".Translate(), delegate
                {
                    building.secondItem = "None";
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 2)))
            {
                foreach (string item in element.items.Where(item => item != "None"))
                {
                    if (building.compItemProcessor.Props.isCategoryBuilding)
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap), delegate
                        {
                            if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertSecondThing(item); }
                        }, MenuOptionPriority.Default, null, null, 29f, null, null));

                    }
                    else
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap), delegate
                        {
                            things = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed(item, true));
                            if (things.Count > 0)
                            {

                                if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertSecondThing(); }
                            }
                            else
                            {
                                Messages.Message("IP_CantFindThing".Translate(ThingDef.Named(item).LabelCap), null, MessageTypeDefOf.NegativeEvent, true);
                            }

                        }, MenuOptionPriority.Default, null, null, 29f, null, null));
                    }

                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertSecondThing(string item = "")
        {
            building.processorStage = ProcessorStage.IngredientsChosen;
            if (item != "")
            {
                building.secondCategory = ThingCategoryDef.Named(item).defName;
                building.secondItem = building.secondCategory;
            }
            else
            {
                building.secondItem = things.RandomElement().def.defName;
            }

        }




    }

    [StaticConstructorOnStartup]
    public class Command_SetThirdItemList : Command
    {

        public Map map;
        public Building_ItemProcessor building;
        public List<Thing> things;



        public Command_SetThirdItemList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (building.thirdItem == "")
                    {
                        icon = ContentFinder<Texture2D>.Get(building.compItemProcessor.Props.chooseIngredientsIcon, true);
                        defaultLabel = "IP_ChooseIngredientThird".Translate();
                    }
                    foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 3)))
                    {
                        foreach (string item in element.items)
                        {
                            if (building.thirdItem == item)
                            {
                                if (building.compItemProcessor.Props.isCategoryBuilding)
                                {
                                    icon = ContentFinder<Texture2D>.Get(ThingCategoryDef.Named(item).iconPath, true);
                                    defaultLabel = "IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap);
                                }
                                else
                                {
                                    if (ThingDef.Named(item).graphicData.graphicClass == typeof(Graphic_StackCount))
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path + "/" + ThingDef.Named(item).defName + "_b", true);

                                    }
                                    else
                                    {
                                        icon = ContentFinder<Texture2D>.Get(ThingDef.Named(item).graphic.path, true);
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            if (building.GetComp<CompItemProcessor>().Props.acceptsNoneAsInput)
            {
                list.Add(new FloatMenuOption("IP_None".Translate(), delegate
                {
                    building.thirdItem = "None";
                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }

            foreach (ItemAcceptedDef element in DefDatabase<ItemAcceptedDef>.AllDefs.Where(element => (element.building == building.def.defName) && (element.slot == 3)))
            {
                foreach (string item in element.items.Where(item => item != "None"))
                {
                    if (building.compItemProcessor.Props.isCategoryBuilding)
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingCategoryDef.Named(item).LabelCap), delegate
                        {
                            if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertThirdThing(item); }
                        }, MenuOptionPriority.Default, null, null, 29f, null, null));

                    }
                    else
                    {
                        list.Add(new FloatMenuOption("IP_InsertVariable".Translate(ThingDef.Named(item).LabelCap), delegate
                        {
                            things = map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed(item, true));
                            if (things.Count > 0)
                            {

                                if (building.processorStage <= ProcessorStage.ExpectingIngredients) { this.TryInsertThirdThing(); }
                            }
                            else
                            {
                                Messages.Message("IP_CantFindThing".Translate(ThingDef.Named(item).LabelCap), null, MessageTypeDefOf.NegativeEvent, true);
                            }

                        }, MenuOptionPriority.Default, null, null, 29f, null, null));
                    }
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        private void TryInsertThirdThing(string item = "")
        {
            building.processorStage = ProcessorStage.IngredientsChosen;
            if (item != "")
            {
                building.thirdCategory = ThingCategoryDef.Named(item).defName;
                building.thirdItem = building.thirdCategory;
            }
            else
            {
                building.thirdItem = things.RandomElement().def.defName;
            }


        }




    }

    [StaticConstructorOnStartup]
    public class Command_SetQualityList : Command
    {

        public Map map;
        public Building_ItemProcessor building;

        public Command_SetQualityList()
        {
            foreach (object obj in Find.Selector.SelectedObjects)
            {
                building = obj as Building_ItemProcessor;
                if (building != null)
                {
                    if (!building.qualityEstablished)
                    {
                        icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityAuto", true);
                        defaultLabel = "IP_ChooseQualityAuto".Translate();
                    }


                    else
                    {

                        switch (building.qualityRequested)
                        {
                            case QualityCategory.Awful:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityAwful", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Awful.ToString());
                                break;
                            case QualityCategory.Poor:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityPoor", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Poor.ToString());
                                break;
                            case QualityCategory.Normal:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityNormal", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Normal.ToString());
                                break;
                            case QualityCategory.Good:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityGood", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Good.ToString());
                                break;
                            case QualityCategory.Excellent:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityExcellent", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Excellent.ToString());
                                break;
                            case QualityCategory.Masterwork:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityMasterwork", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Masterwork.ToString());
                                break;
                            case QualityCategory.Legendary:
                                icon = ContentFinder<Texture2D>.Get("UI/QualitySelectors/IP_QualityLegendary", true);
                                defaultLabel = "IP_QualityAutoIs".Translate(QualityCategory.Legendary.ToString());
                                break;
                        }

                    }

                }
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();




            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Awful.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Awful;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Poor.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Poor;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Normal.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Normal;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Good.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Good;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Excellent.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Excellent;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Masterwork.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Masterwork;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoIs".Translate(QualityCategory.Legendary.ToString()), delegate
            {
                building.qualityRequested = QualityCategory.Legendary;
                building.qualityEstablished = true;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("IP_QualityAutoNot".Translate(), delegate
            {
                building.qualityEstablished = false;
            }, MenuOptionPriority.Default, null, null, 29f, null, null));



            Find.WindowStack.Add(new FloatMenu(list));
        }






    }




}

