// (C) Copyright 2014-2015 by Andrew Nicholas
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

namespace SCaddins.SCopy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Diagnostics;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    
    public class SheetCopy
    {
        private Document doc;
        private System.ComponentModel.BindingList<SCopySheet> sheets;
        private Dictionary<string, View> existingSheets =
            new Dictionary<string, View>();
        
        private Dictionary<string, View> existingViews =
            new Dictionary<string, View>();
        
        private Dictionary<string, View> viewTemplates =
            new Dictionary<string, View>();
        
        private Dictionary<string, Level> levels =
            new Dictionary<string, Level>();
        
        private Collection<string> sheetCategories = 
            new Collection<string>();
        
        private ElementId floorPlanViewFamilyTypeId = null;
           
        public SheetCopy(Document doc)
        {
            this.doc = doc;
            this.sheets = new System.ComponentModel.BindingList<SCopySheet>();
            this.GetViewTemplates();
            this.GetAllSheets();
            this.GetAllLevelsInModel();
            this.GetAllViewsInModel();
            this.GetFloorPlanViewFamilyTypeId();
            this.GetAllSheetCategories();
        }
               
        #region properties

        public System.ComponentModel.BindingList<SCopySheet> Sheets {
            get {
                return this.sheets;
            }
        }

        public Dictionary<string, View> ViewTemplates {
            get {
                return this.viewTemplates;
            }
        }

        public Dictionary<string, Level> Levels {
            get {
                return this.levels;
            }
        }
    
        public Dictionary<string, View> ExistingViews {
            get {
                return this.existingViews;
            }
        }
        
        public Collection<string> SheetCategories {
            get {
                return this.sheetCategories;
            }    
        }
    
        #endregion

        #region public methods
        /// <summary>
        /// Cast a view to a viewsheet
        /// </summary>
        /// <param name="view">A Revit View</param>
        /// <returns>A Revit ViewSHeet, or null if the source view is not a ViewSheet</returns>
        public static ViewSheet ViewToViewSheet(View view)
        {
            return (view.ViewType != ViewType.DrawingSheet) ? null : view as ViewSheet;
        }
                  
        public bool SheetNumberAvailable(string number)
        {
            foreach (SCopySheet s in this.sheets) {
                if (s.Number.ToUpper(CultureInfo.InvariantCulture).Equals(number.ToUpper(CultureInfo.InvariantCulture))) {
                    return false;
                }
            }
            return !this.existingSheets.ContainsKey(number);
        }

        public bool ViewNameAvailable(string title)
        {
            foreach (SCopySheet s in this.sheets) {
                foreach (SCopyViewOnSheet v in s.ViewsOnSheet) {
                    if (v.Title.ToUpper(CultureInfo.InvariantCulture).Equals(title.ToUpper(CultureInfo.InvariantCulture))) {
                        return false;
                    }
                }
            }
            return !this.existingViews.ContainsKey(title);
        }

        public void CreateSheets()
        {
            if (this.sheets.Count < 1) {
                return;
            }
            var t = new Transaction(this.doc, "SCopy");
            t.Start();
            string summaryText = string.Empty;
            foreach (SCopySheet sheet in this.sheets) {
                this.CreateAndPopulateNewSheet(sheet, ref summaryText);
            }
            t.Commit();
            var td = new TaskDialog("SCopy - Summary");
            td.MainInstruction = "SCopy - Summary";
            td.MainContent = summaryText;
            td.MainIcon = TaskDialogIcon.TaskDialogIconNone;
            td.Show();           
        }
    
        public void AddSheet(ViewSheet sourceSheet)
        {
            string n = this.GetNewSheetNumber(sourceSheet.SheetNumber);
            string t = sourceSheet.Name + SCopyConstants.MenuItemCopy;
            this.sheets.Add(new SCopySheet(n, t, this, sourceSheet));
        }
        
        #endregion

        #region private methods
         
        private static XYZ ViewCenterFromTBBottomLeft(BoundingBoxXYZ viewBounds)
        {
            var x1 = viewBounds.Min.X;
            var x2 = viewBounds.Max.X;
            var y1 = viewBounds.Min.Y;
            var y2 = viewBounds.Max.Y;
            XYZ xyzPosition = new XYZ(x1 + (x2 - x1) / 2, y1 + (y2 - y1) / 2, viewBounds.Min.Z);
            return xyzPosition;
        }
                      
        private static Dictionary<ElementId, BoundingBoxXYZ> GetVPDictionary(
            ViewSheet srcSheet, Document doc)
        {
            var result = new Dictionary<ElementId, BoundingBoxXYZ>();
            foreach (ElementId viewPortId in srcSheet.GetAllViewports()) {
                var viewPort = (Viewport)doc.GetElement(viewPortId);
                var viewPortBounds = viewPort.GetBoxOutline();
                BoundingBoxXYZ bb = new BoundingBoxXYZ();
                bb.Min = viewPortBounds.MinimumPoint;
                bb.Max = viewPortBounds.MaximumPoint;
                result.Add(
                    viewPort.ViewId, bb);
            }
            return result;
        }

        private void GetViewTemplates()
        {
            this.viewTemplates.Clear();
            FilteredElementCollector c = new FilteredElementCollector(this.doc);
            c.OfCategory(BuiltInCategory.OST_Views);
            foreach (View view in c) {
                if (view.IsTemplate) {
                    this.viewTemplates.Add(view.Name, view);
                }
            }
        }
          
        private void GetAllSheetCategories()
        {
            this.sheetCategories.Clear();
            FilteredElementCollector c1 = new FilteredElementCollector(this.doc);
            c1.OfCategory(BuiltInCategory.OST_Sheets);
            foreach (View view in c1) {
                #if REVIT2015
                var viewCategoryParamList = view.GetParameters(SCopyConstants.SheetCategory);
                if (viewCategoryParamList != null && viewCategoryParamList.Count > 0) {
                    Parameter viewCategoryParam = viewCategoryParamList.First();
                    string s = viewCategoryParam.AsString();
                    if (!string.IsNullOrEmpty(s) && !this.sheetCategories.Contains(s)) {
                        this.sheetCategories.Add(s);
                    }
                } 
                #else
                var viewCategoryParam = view.get_Parameter(SCopyConstants.SheetCategory);
                if (viewCategoryParam != null) {
                    string s = viewCategoryParam.AsString();
                    if (!string.IsNullOrEmpty(s) && !this.sheetCategories.Contains(s)) {
                        this.sheetCategories.Add(s);
                    }
                } 
                #endif
            }
        }

        private void GetAllSheets()
        {
            this.existingSheets.Clear();
            FilteredElementCollector c1 = new FilteredElementCollector(this.doc);
            c1.OfCategory(BuiltInCategory.OST_Sheets);
            foreach (View view in c1) {
                ViewSheet vs = view as ViewSheet;
                this.existingSheets.Add(vs.SheetNumber, view);
            }
        }

        private void GetFloorPlanViewFamilyTypeId()
        {
            foreach (ViewFamilyType vft in new FilteredElementCollector(this.doc).OfClass(typeof(ViewFamilyType))) {
                if (vft.ViewFamily == ViewFamily.FloorPlan) {
                    this.floorPlanViewFamilyTypeId = vft.Id;
                }
            }
        }
        
        private void GetAllViewsInModel()
        {
            this.existingViews.Clear();
            FilteredElementCollector c = new FilteredElementCollector(this.doc);
            c.OfClass(typeof(Autodesk.Revit.DB.View));
            foreach (View view in c) {
                View v = view as View;
                View vv;
                if (!this.existingViews.TryGetValue(v.Name, out vv)) {
                    this.existingViews.Add(v.Name, view);
                }
            }
        }

        private void GetAllLevelsInModel()
        {
            this.levels.Clear();
            FilteredElementCollector c3 = new FilteredElementCollector(this.doc);
            c3.OfClass(typeof(Level));
            foreach (Level l in c3) {
                this.levels.Add(l.Name.ToString(), l);
            }
        }

        // this is where the action happens
        private bool CreateAndPopulateNewSheet(SCopySheet sheet, ref string summary)
        {        
            // create the "blank canvas"
            sheet.DestinationSheet = this.AddEmptySheetToDocument(
                sheet.Number,
                sheet.Title,
                sheet.SheetCategory);
 
            if (sheet.DestinationSheet != null) {
                Debug.WriteLine(sheet.Number + " added to document.");
                this.CreateViewports(sheet);
            } else {
                return false;
            }
            
            try {
                this.CopyElementsBetweenSheets(sheet);
            } catch (InvalidOperationException) {
            
            }
    
            // create a log...
            var oldNumber = sheet.SourceSheet.SheetNumber;
            var msg = " Sheet: " + oldNumber + " copied to: " + sheet.Number;
            summary += msg + System.Environment.NewLine;

            return true;
        }
    
        // add an empty sheet to the doc.
        // this comes first before copying titleblock, views etc.
        private ViewSheet AddEmptySheetToDocument(
            string sheetNumber,
            string sheetTitle,
            string viewCategory)
        {
            ViewSheet result;
            result = ViewSheet.Create(this.doc, ElementId.InvalidElementId);           
            result.Name = sheetTitle;
            result.SheetNumber = sheetNumber;
            #if REVIT2015
            var viewCategoryParamList = result.GetParameters(SCopyConstants.SheetCategory);
            if (viewCategoryParamList.Count > 0) {
                Parameter viewCategoryParam = viewCategoryParamList.First();
                viewCategoryParam.Set(viewCategory);
            }
            #else
            var s = result.get_Parameter(SCopyConstants.SheetCategory);
            if (s != null) {
                s.Set(viewCategory);
            }
            #endif
            return result;
        }
        
        private void PlaceViewPortOnSheet(
            ViewSheet destSheet, ElementId destViewId, XYZ viewCentre)
        {
            Viewport.Create(this.doc, destSheet.Id, destViewId, viewCentre);
        }

        private string GetNewSheetNumber(string originalNumber)
        {
            int inc = 0;
            do {
                inc++;
            } while (!this.SheetNumberAvailable(originalNumber + "-" + inc.ToString(CultureInfo.InvariantCulture)));
            return originalNumber + "-" + inc.ToString(CultureInfo.InvariantCulture);
        }
        
        private void TryAssignViewTemplate(View view, string templateName)
        {
            if (templateName != SCopyConstants.MenuItemCopy) {
                View vt = null;
                if (this.viewTemplates.TryGetValue(templateName, out vt)) {
                    view.ViewTemplateId = vt.Id;
                }
            }   
        }
               
        private void PlaceNewViewOnSheet(
            SCopyViewOnSheet view, SCopySheet sheet, XYZ sourceViewCentre)
        {
            Level level = null;
            this.levels.TryGetValue(view.AssociatedLevelName, out level);
            if (level != null) {
                ViewPlan vp = ViewPlan.Create(this.doc, this.floorPlanViewFamilyTypeId, level.Id);
                vp.CropBox = view.OldView.CropBox;
                vp.CropBoxActive = view.OldView.CropBoxActive;
                vp.CropBoxVisible = view.OldView.CropBoxVisible;
                this.TryAssignViewTemplate(vp, view.ViewTemplateName);
                this.PlaceViewPortOnSheet(sheet.DestinationSheet, vp.Id, sourceViewCentre);
            }
        }
        
        private void DuplicateViewOntoSheet(
            SCopyViewOnSheet view, SCopySheet sheet, XYZ sourceViewCentre)
        {
            var d = view.DuplicateWithDetailing == true ? ViewDuplicateOption.WithDetailing : ViewDuplicateOption.Duplicate;          
            ElementId destViewId = view.OldView.Duplicate(d);
            string newName = sheet.GetNewViewName(view.OldView.Id);
            var v = this.doc.GetElement(destViewId) as View;
            if (newName != null) {
                v.Name = newName;
                View dv = this.doc.GetElement(destViewId) as View;  
                this.TryAssignViewTemplate(dv, view.ViewTemplateName);                
            }
            this.PlaceViewPortOnSheet(sheet.DestinationSheet, destViewId, sourceViewCentre);
        }
                  
        private void CopyElementsBetweenSheets(SCopySheet sheet)
        {
            IList<ElementId> list = new List<ElementId>();
            foreach (Element e in new FilteredElementCollector(this.doc).OwnedByView(sheet.SourceSheet.Id)) {
                if (!(e is Viewport)) {
                    Debug.WriteLine("adding " + e.GetType().ToString() + " to copy list(CopyElementsBetweenSheets).");
                    if(e is CurveElement) {
                        continue;
                    }
                    if (e.IsValidObject && e.ViewSpecific) {
                        list.Add(e.Id);
                    }
                }
            } 
            
//            #if DEBUG
//            if (list.Count > 0) {
//                foreach (ElementId id in list) {
//                    IList<ElementId> list2 = new List<ElementId>();
//                    list2.Add(id);
//                    Debug.WriteLine("Beggining element copy");
//                    if(sheet.SourceSheet.IsValidObject && sheet.DestinationSheet.IsValidObject) {
//                        try {
//                            ElementTransformUtils.CopyElements(
//                                sheet.SourceSheet,
//                                list2,
//                                sheet.DestinationSheet,
//                                new Transform(ElementTransformUtils.GetTransformFromViewToView(sheet.SourceSheet, sheet.DestinationSheet)),
//                                new CopyPasteOptions());
//                        } catch (InvalidOperationException e) {
//                            Debug.WriteLine("Element coping " + list2[0].ToString() + " - " + e.Message);
//                        }
//                    }
//                }
//            }
//            #else
             if (list.Count > 0) {
                Debug.WriteLine("Beggining element copy");
                ElementTransformUtils.CopyElements(
                    sheet.SourceSheet,
                    list,
                    sheet.DestinationSheet,
                    new Transform(ElementTransformUtils.GetTransformFromViewToView(sheet.SourceSheet, sheet.DestinationSheet)),
                    new CopyPasteOptions());
            }
            #endif
        }
             
        private void CreateViewports(SCopySheet sheet)
        {
            Dictionary<ElementId, BoundingBoxXYZ> viewPorts =
                SheetCopy.GetVPDictionary(sheet.SourceSheet, this.doc);

            foreach (SCopyViewOnSheet view in sheet.ViewsOnSheet) {
                BoundingBoxXYZ srcViewPort = null;
                if (!viewPorts.TryGetValue(view.OldId, out srcViewPort)) {
                    TaskDialog.Show("SCopy", "Error...");
                    continue;
                }
            
                XYZ sourceViewPortCentre = SheetCopy.ViewCenterFromTBBottomLeft(srcViewPort);
              
                switch (view.CreationMode) {
                    case ViewPortPlacementMode.Copy:
                        this.DuplicateViewOntoSheet(view, sheet, sourceViewPortCentre);
                        break;
                    case ViewPortPlacementMode.New:
                        this.PlaceNewViewOnSheet(view, sheet, sourceViewPortCentre);
                        break;     
                    case ViewPortPlacementMode.Legend:
                        this.PlaceViewPortOnSheet(sheet.DestinationSheet, view.OldView.Id, sourceViewPortCentre);
                        break;                 
                }
            }       
        }
        #endregion
    }
}
/* vim: set ts=4 sw=4 nu expandtab: */
