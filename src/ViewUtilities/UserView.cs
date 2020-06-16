﻿// (C) Copyright 2014-2020 by Andrew Nicholas
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

namespace SCaddins.ViewUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Autodesk.Revit.DB;
    using Common;

    /// <summary>
    /// Copy a view; give it a user name, remove any view templates and
    /// categorize it nicely.
    /// </summary>
    public static class UserView
    {
        public static List<View> Create(View sourceView, Document doc)
        {
            if (sourceView == null || doc == null)
            {
                return null;
            }

            if (sourceView.ViewType == ViewType.DrawingSheet)
            {
                return Create(sourceView as ViewSheet, doc);
            }

            if (ValidViewType(sourceView.ViewType))
            {
                List<View> result = new List<View> { CreateView(sourceView, doc) };
                return result;
            }

            return null;
        }

        public static List<View> Create(ICollection<ExportManager.ExportSheet> sheets, Document doc)
        {
            List<View> result = new List<View>();
            if (sheets == null || doc == null)
            {
                return null;
            }
            else
            {
                using (var t = new Transaction(doc, "SCuv Copies User Views"))
                {
                    if (t.Start() == TransactionStatus.Started)
                    {
                        foreach (ExportManager.ExportSheet sheet in sheets)
                        {
                            var list = Create(sheet.Sheet, doc);
                            foreach (View v in list)
                            {
                                result.Add(v);
                            }
                        }
                        t.Commit();
                    }
                    else
                    {
                        SCaddinsApp.WindowManager.ShowMessageBox("Error", "Could not start user view transaction");
                        return null;
                    }
                }
            }
            return result;
        }

        public static void ShowSummaryDialog(List<View> newUserViews)
        {
                string message = string.Empty;
                if (newUserViews == null) {
                    message = "No valid views found, User view not created." + Environment.NewLine
                    + "\tValid views types are: " + Environment.NewLine
                    + Environment.NewLine
                    + "\t\tViewType.FloorPlan" + Environment.NewLine
                    + "\t\tViewType.Elevation" + Environment.NewLine
                    + "\t\tViewType.CeilingPlan" + Environment.NewLine
                    + "\t\tViewType.Section" + Environment.NewLine
                    + "\t\tViewType.AreaPlan" + Environment.NewLine
                    + "\t\tViewType.ThreeD";
                } else {
                    message += "Summary of users view created:" + Environment.NewLine;
                    foreach (View view in newUserViews) {
                        message += view.Name + Environment.NewLine;
                    }
                }
                SCaddinsApp.WindowManager.ShowMessageBox(message);
        }

        private static List<View> Create(ViewSheet vs, Document doc)
        {
            List<View> result = new List<View>();
            foreach (ElementId id in vs.GetAllPlacedViews())
            {
                var v = (View)doc.GetElement(id);
                if (ValidViewType(v.ViewType))
                {
                    result.Add(CreateView(v, doc));
                }
            }
            return result;
        }

        private static View CreateView(View srcView, Document doc)
        {
            ElementId destViewId = srcView.Duplicate(ViewDuplicateOption.Duplicate);
            var newView = doc.GetElement(destViewId) as View;
            newView.Name = GetNewViewName(doc, srcView);
            newView.ViewTemplateId = ElementId.InvalidElementId;
            var p = newView.GetParameters("SC-View_Category");
            if (p.Count < 1)
            {
                return newView;
            }
            var param = p[0];
            if (param == null)
            {
                return newView;
            }

            if (param.IsReadOnly)
            {
                SCaddinsApp.WindowManager.ShowMessageBox("SCuv Error", "SC-View_Category is read only!");
                return null;
            }

            if (param.Set("User"))
            {
                return newView;
            }
            SCaddinsApp.WindowManager.ShowMessageBox("SCuv Error", "Error setting SC-View_Category parameter!");
            return null;
        }

        ////private static string GetNewViewName(Document doc, Element sourceView)
        ////{
        ////    if (doc == null || sourceView == null)
        ////    {
        ////        return string.Empty;
        ////    }
        ////    string name = sourceView.Name;

        ////    // Revit wont allow { or } so replace them if they exist
        ////    name = name.Replace(@"{", string.Empty).Replace(@"}", string.Empty);
        ////    name = Environment.UserName + "-" + name + "-" + MiscUtilities.GetDateString;
        ////    if (SolarAnalysis.SolarAnalysisManager.ViewNameIsAvailable(doc, name))
        ////    {
        ////        return name;
        ////    }
        ////    else
        ////    {
        ////        return SolarAnalysis.SolarAnalysisManager.GetNiceViewName(doc, name);
        ////    }
        ////}

        private static string GetNewViewName(Document doc, Element sourceView)
        {
            if (doc == null || sourceView == null)
            {
                return string.Empty;
            }

            string user = Environment.UserName;
            string date = MiscUtilities.GetDateString;
            string name = ViewUtilitiesSettings.Default.UserViewNameFormat;

            name = name.Replace(@"$user", user);
            name = name.Replace(@"$date", date);

            string pattern = @"(<<)(.*?)(>>)";
            name = Regex.Replace(
                name,
                pattern,
                m => RoomConverter.RoomConversionCandidate.GetParamValueAsString(ParamFromString(m.Groups[2].Value, sourceView)));

            // Revit wont allow { or } so replace them if they exist
            name = name.Replace(@"{", string.Empty).Replace(@"}", string.Empty);

            // FIXME move the below method somewhere else
            if (SolarAnalysis.SolarAnalysisManager.ViewNameIsAvailable(doc, name))
            {
                return name;
            }
            else
            {
                return SolarAnalysis.SolarAnalysisManager.GetNiceViewName(doc, name);
            }
        }

        public static Parameter ParamFromString(string name, Element element)
        {
            if (element.GetParameters(name).Count > 0)
            {
                return element.GetParameters(name)[0];
            }
            return null;
        }

        private static bool ValidViewType(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.FloorPlan:
                case ViewType.Elevation:
                case ViewType.CeilingPlan:
                case ViewType.Section:
                case ViewType.AreaPlan:
                case ViewType.ThreeD:
                    return true;
            }
            return false;
        }
    }
}