﻿// (C) Copyright 2017 by Andrew Nicholas
//
// This file is part of SCaddins.
//
// SCaddins is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SCaddins is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SCaddins.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace SCaddins.RenameUtilities
{
    public class RenameManager
    {
        private Document doc;
        public Caliburn.Micro.BindableCollection<SCaddins.RenameUtilities.RenameCandidate> renameCandidates;
        public Caliburn.Micro.BindableCollection<SCaddins.RenameUtilities.RenameCommand> renameCommands;


        public RenameManager(Document doc)
        {
            this.doc = doc;
            renameCandidates = new Caliburn.Micro.BindableCollection<SCaddins.RenameUtilities.RenameCandidate>();
            //renameCommands.Add(new RenameUtilities.RenameCommand(RenameTest, "Custom"));
            //renameCommands.Add(new RenameUtilities.RenameCommand(RenameTest, "Custom2"));
            renameCommands.Add(new RenameUtilities.RenameCommand(RenameTest, "Custom3"));
        }
        
        public void Rename(List<RenameCandidate> renameCandidates)
        {
            int fails = 0;
            int successes = 0;
            using (var t = new Transaction(doc)) {
                if (t.Start("Bulk Rename") == TransactionStatus.Started) { 
                    foreach (RenameCandidate candidate in renameCandidates) {
                        if (candidate.ValueChanged()) {
                            if (candidate.Rename()) {
                                successes++;
                            } else {
                                fails++;
                            }
                        }
                    }
                    t.Commit();
                    Autodesk.Revit.UI.TaskDialog.Show(@"Bulk Rename", successes + @" parameters succesfully renames, " + fails + @" errors.");
                } else {
                    Autodesk.Revit.UI.TaskDialog.Show("Error", "Failed to start Bulk Rename Revit Transaction...");
                }
            }
        }
        
        private static void ConvertViewName(View view)
        {
            string newName = NewString(view.Name);
            if (ValidRevitName(newName)) {
                view.Name = newName;
            }
        }
        
        private static string NewString(string oldString)
        {
            return oldString.ToUpper(CultureInfo.CurrentCulture);
        }

        private static bool ValidRevitName(string s)
        {
            return !(s.Contains("{") || s.Contains("}"));
        }

        private static void ConvertAnnotation(TextElement text)
        {
            text.Text = NewString(text.Text);
        }

        private static void ConvertRoom(Room room)
        {
            Parameter param = room.LookupParameter("Name");
            param.Set(NewString(param.AsString()));
        }
        
        public static void ConvertSelectionToUppercase(Document doc, IList<ElementId> elements)
        {
            if (elements == null || doc == null) {
                return;
            }
            using (var trans = new Transaction(doc)) {
                trans.Start("Convert selected elements to uppercase (SCulcase)");
                foreach (Autodesk.Revit.DB.ElementId eid in elements) {
                    Element e = doc.GetElement(eid);
                    Category category = e.Category;
                    var enumCategory = (BuiltInCategory)category.Id.IntegerValue;
                    switch (enumCategory) {
                        case BuiltInCategory.OST_Views:
                            var v = (View)e;
                            ConvertViewName(v);
                            break;
                        case BuiltInCategory.OST_TextNotes:
                            var text = (TextElement)e;
                            ConvertAnnotation(text);
                            break;
                        case BuiltInCategory.OST_Rooms:
                            var room = (Room)e;
                            ConvertRoom(room);
                            break;
                    }
                }
                trans.Commit();
            }
        }

        public Caliburn.Micro.BindableCollection<SCaddins.RenameUtilities.RenameCandidate> RenameCandidates
        {
            get
            {
                return renameCandidates;
            }
        }

        public Caliburn.Micro.BindableCollection<String> AvailableParameterTypes
        {
            get
            {
                Caliburn.Micro.BindableCollection<String> result = new Caliburn.Micro.BindableCollection<String>();
                result.Add("Rooms");
                result.Add("Text");
                result.Add("Views");
                result.Add("Sheets");
                result.Add("Revisions");
                result.Add("Walls");
                result.Add("Doors");
                result.Add("Floors");
                result.Add("Roofs");
                result.Add(@"Model Groups");
                return result;
            }
        }

        public Caliburn.Micro.BindableCollection<RenameCommand> RenameModes
        {
            get
            {
                return renameCommands;
            }
        }

        public static string RenameTest(string s, string t, string u)
        {
            return "hello";
        }

        public Caliburn.Micro.BindableCollection<RenameCandidate> GetTextNoteValues(BuiltInCategory category){
            Caliburn.Micro.BindableCollection<RenameCandidate> candidates = new Caliburn.Micro.BindableCollection<RenameCandidate>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(category);
            foreach (Element element in collector) {
                var textNote = (TextElement)element;
                if(textNote != null) {
                    candidates.Add(new RenameCandidate(textNote));
                }
            }
            return candidates;
        }
             
        public void SetCandidatesByParameter(Parameter parameter, BuiltInCategory category){
            if (category == BuiltInCategory.OST_TextNotes || category == BuiltInCategory.OST_IOSModelGroups) {
                renameCandidates = GetTextNoteValues(category);
                return;
            }
            renameCandidates.Clear();
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(category);
            foreach (Element element in collector) {
                var p = element.GetParameters(parameter.Definition.Name);
                if (p.Count > 0) {
                    renameCandidates.Add(new RenameCandidate(p[0]));
                }
            }
        }

        public Caliburn.Micro.BindableCollection<RenameParameter> RenameParametersByCategory(string parameterCategory)
        {
                if (parameterCategory == "Rooms") {
                    return GetParametersByCategory(BuiltInCategory.OST_Rooms);
                }
                if (parameterCategory == "Views") {
                    return GetParametersByCategory(BuiltInCategory.OST_Views);
                }
                if (parameterCategory == "Sheets") {
                    return GetParametersByCategory(BuiltInCategory.OST_Sheets);
                }
                if (parameterCategory == "Walls") {
                    return GetParametersByCategory(BuiltInCategory.OST_Walls);
                }
                if (parameterCategory == "Doors") {
                    return GetParametersByCategory(BuiltInCategory.OST_Doors);
                }
                if (parameterCategory == "Windows") {
                    return GetParametersByCategory(BuiltInCategory.OST_Windows);
                }
                if (parameterCategory == "Windows") {
                    return GetParametersByCategory(BuiltInCategory.OST_Revisions);
                }
                if (parameterCategory == "Floors") {
                    return GetParametersByCategory(BuiltInCategory.OST_Floors);
                }
                if (parameterCategory == @"Text") {
                    return GetParametersByCategory(BuiltInCategory.OST_TextNotes);
                }
                if (parameterCategory == @"Model Groups") {
                    return GetParametersByCategory(BuiltInCategory.OST_IOSModelGroups);
                }
                return new Caliburn.Micro.BindableCollection<RenameParameter>();
        }
        
        public Caliburn.Micro.BindableCollection<RenameParameter> GetParametersByCategory(BuiltInCategory category)
        {
            Caliburn.Micro.BindableCollection<RenameParameter> parametersList = new Caliburn.Micro.BindableCollection<RenameParameter>();
            if(category == BuiltInCategory.OST_TextNotes || category == BuiltInCategory.OST_IOSModelGroups) {
                parametersList.Add(new RenameParameter(category));
            }
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(category);
            var elem = collector.FirstElement();
            foreach (Parameter param in elem.Parameters) {
                if (param.StorageType == StorageType.String && !param.IsReadOnly) {
                    parametersList.Add(new RenameParameter(param, category));
                }
            }
            return parametersList;
        }
    }
}
