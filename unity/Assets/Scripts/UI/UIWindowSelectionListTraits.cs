﻿using UnityEngine;
using Assets.Scripts.Content;
using System.Collections.Generic;

namespace Assets.Scripts.UI
{
    public class UIWindowSelectionListTraits : UIWindowSelectionList
    {
        protected List<TraitGroup> traitData = new List<TraitGroup>();

        protected List<SelectionItemTraits> traitItems = new List<SelectionItemTraits>();

        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, string title = "") : base(call, title)
        {
        }

        public UIWindowSelectionListTraits(UnityEngine.Events.UnityAction<string> call, StringKey title) : base(call, title)
        {
        }

        override public void Draw()
        {
            foreach (SelectionItem item in items)
            {
                SelectionItemTraits itemT = item as SelectionItemTraits;
                if (itemT == null)
                {
                    traitItems.Add(new SelectionItemTraits(item));
                }
                else
                {
                    traitItems.Add(itemT);
                }
            }

            foreach (SelectionItemTraits item in traitItems)
            {
                foreach (string category in item.GetTraits().Keys)
                {
                    bool found = false;
                    foreach (TraitGroup tg in traitData)
                    {
                        if (tg.GetName().Equals(category))
                        {
                            found = true;
                            tg.AddTraits(item);
                        }
                    }

                    if (!found)
                    {
                        TraitGroup tg = new TraitGroup(category);
                        tg.AddTraits(item);
                        traitData.Add(tg);
                    }
                }
            }

            foreach (SelectionItemTraits item in traitItems)
            {
                foreach (TraitGroup tg in traitData)
                {
                    tg.AddItem(item);
                }
            }

            Update();
        }

        protected void Update()
        {
            // Border
            UIElement ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-18), 0, 36, 30);
            new UIElementBorder(ui);

            // Title
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-10), 0, 20, 1);
            ui.SetText(_title);

            UIElementScrollVertical traitScrollArea = new UIElementScrollVertical();
            traitScrollArea.SetLocation(UIScaler.GetHCenter(-17.5f), 2, 13, 25);

            float offset = 0;
            foreach (TraitGroup tg in traitData)
            {
                ui = new UIElement(traitScrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 12, 1);
                ui.SetText(tg.GetName(), Color.black);
                ui.SetTextAlignment(TextAnchor.MiddleLeft);
                ui.SetBGColor(new Color(0.5f, 1, 0.5f));
                offset += 1.05f;

                bool noneSelected = tg.NoneSelected();

                foreach (string s in tg.traits.Keys)
                {
                    TraitGroup tmpGroup = tg;
                    string tmpTrait = s;
                    ui = new UIElement(traitScrollArea.GetScrollTransform());
                    ui.SetLocation(0, offset, 12, 1);
                    if (tg.traits[s].selected)
                    {
                        ui.SetBGColor(Color.white);
                        ui.SetButton(delegate { SelectTrait(tmpGroup, tmpTrait); });
                    }
                    else
                    {
                        int itemCount = 0;
                        foreach (SelectionItemTraits item in tg.traits[s].items)
                        {
                            bool display = true;
                            foreach (TraitGroup g in traitData)
                            {
                                display &= g.ActiveItem(item);
                            }
                            if (display) itemCount++;
                        }
                        if (itemCount > 0)
                        {
                            if (noneSelected)
                            {
                                ui.SetBGColor(Color.white);
                            }
                            else
                            {
                                ui.SetBGColor(Color.grey);
                            }
                            ui.SetButton(delegate { SelectTrait(tmpGroup, tmpTrait); });
                        }
                        else
                        {
                            ui.SetBGColor(new Color(0.5f, 0, 0));
                        }
                    }
                    ui.SetText(s, Color.black);
                    offset += 1.05f;
                }
                offset += 1.05f;
            }
            traitScrollArea.SetScrollSize(offset);


            UIElementScrollVertical itemScrollArea = new UIElementScrollVertical();
            itemScrollArea.SetLocation(UIScaler.GetHCenter(-3.5f), 2, 21, 25);

            offset = 0;
            foreach (SelectionItemTraits item in items)
            {
                bool display = true;
                foreach (TraitGroup tg in traitData)
                {
                    display &= tg.ActiveItem(item);
                }

                if (!display) continue;

                // Print the name but select the key
                string key = item.GetKey();
                ui = new UIElement(itemScrollArea.GetScrollTransform());
                ui.SetLocation(0, offset, 20, 1);
                if (key != null)
                {
                    ui.SetButton(delegate { SelectItem(key); });
                }
                ui.SetBGColor(Color.white);
                ui.SetText(item.GetDisplay(), Color.black);
                offset += 1.05f;
            }

            itemScrollArea.SetScrollSize(offset);

            // Cancel button
            ui = new UIElement();
            ui.SetLocation(UIScaler.GetHCenter(-4.5f), 28, 9, 1);
            ui.SetBGColor(new Color(0.03f, 0.0f, 0f));
            ui.SetText(CommonStringKeys.CANCEL);
            ui.SetButton(delegate { Destroyer.Dialog(); });
            new UIElementBorder(ui);
        }

        protected void SelectTrait(TraitGroup group, string trait)
        {
            group.traits[trait].selected = !group.traits[trait].selected;
            Update();
        }

        public void AddItem(StringKey stringKey, Dictionary<string, IEnumerable<string>> traits)
        {
            items.Add(new SelectionItemTraits(stringKey.Translate(), stringKey.key, traits));
        }

        public void AddItem(string item, Dictionary<string, IEnumerable<string>> traits)
        {
            items.Add(new SelectionItemTraits(item, item, traits));
        }

        public void AddItem(string display, string key, Dictionary<string, IEnumerable<string>> traits)
        {
            items.Add(new SelectionItemTraits(display, key, traits));
        }

        public void AddItem(QuestData.QuestComponent qc)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { new StringKey("val", qc.typeDynamic.ToUpper()).Translate() });
            traits.Add(new StringKey("val", "SOURCE").Translate(), new string[] { qc.source });

            items.Add(new SelectionItemTraits(qc.sectionName, qc.sectionName, traits));
        }

        public void AddNewComponentItem(string type)
        {
            Dictionary<string, IEnumerable<string>> traits = new Dictionary<string, IEnumerable<string>>();

            traits.Add(new StringKey("val", "TYPE").Translate(), new string[] { type });
            traits.Add(new StringKey("val", "SOURCE").Translate(), new string[] { new StringKey("val", "NEW").Translate() });

            items.Add(new SelectionItemTraits(new StringKey("val", "NEW_X", type.ToUpper()).Translate(), "{NEW:" + type + "}", traits));
        }

        protected class SelectionItemTraits : SelectionItem
        {
            Dictionary<string, IEnumerable<string>> _traits = new Dictionary<string, IEnumerable<string>>();

            public SelectionItemTraits(string display, string key) : base(display, key)
            {
            }

            public SelectionItemTraits(string display, string key, Dictionary<string, IEnumerable<string>> traits) : base(display, key)
            {
                _traits = traits;
            }

            public SelectionItemTraits(SelectionItem item) : base(item.GetDisplay(), item.GetKey())
            {
            }

            public Dictionary<string, IEnumerable<string>> GetTraits()
            {
                return _traits;
            }
        }

        protected class TraitGroup
        {
            public Dictionary<string, Trait> traits = new Dictionary<string, Trait>();
            public List<SelectionItem> ungrouped = new List<SelectionItem>();
            public string _name = "";

            public TraitGroup(string name)
            {
                _name = name;
            }

            public string GetName()
            {
                return _name;
            }

            public bool NoneSelected()
            {
                bool anySelected = false;
                foreach (Trait t in traits.Values)
                {
                    anySelected |= t.selected;
                }
                return !anySelected;
            }

            public bool ActiveItem(SelectionItemTraits item)
            {
                if (NoneSelected()) return true;

                foreach (Trait t in traits.Values)
                {
                    if (t.selected && !t.items.Contains(item)) return false;
                }
                return true;
            }

            public void AddTraits(SelectionItemTraits item)
            {
                foreach (string trait in item.GetTraits()[_name])
                {
                    if (!traits.ContainsKey(trait))
                    {
                        traits.Add(trait, new Trait());
                    }
                }
            }

            public void AddItem(SelectionItemTraits item)
            {
                if (!item.GetTraits().ContainsKey(_name))
                {
                    ungrouped.Add(item);
                }
                else
                {
                    foreach (string s in item.GetTraits()[_name])
                    {
                        traits[s].items.Add(item);
                    }
                }
            }

            public class Trait
            {
                public bool selected = false;
                public List<SelectionItem> items = new List<SelectionItem>();
            }
        }
    }
}
