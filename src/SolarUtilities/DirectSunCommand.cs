﻿// (C) Copyright 2013-2015 by Andrew Nicholas
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

namespace SCaddins.SolarUtilities
{
    using System.Collections.Generic;
    using System.Dynamic;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class DirectSunCommand : IExternalCommand
    {
        public static void RunAnalysis(IList<Reference> faceSelection, IList<Reference> massSelection, int divisions, UIDocument uidoc)
        {
            if (faceSelection == null) {
                return;
            }

            int lineCount = 0;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Transaction t = new Transaction(uidoc.Document);
            t.Start("testSolarVectorLines");

            foreach (Reference r in faceSelection) {
                Face f = (Face)uidoc.Document.GetElement(r).GetGeometryObjectFromReference(r);
                var bb = f.GetBoundingBox();
                for (double u = bb.Min.U; u < bb.Max.U; u += (bb.Max.U - bb.Min.U) / divisions) {
                    for (double v = bb.Min.V; v < bb.Max.V; v += (bb.Max.V - bb.Min.V) / divisions) {
                        UV uv = new UV(u, v);
                        if (f.IsInside(uv)) {
                            XYZ start = f.Evaluate(uv);
                            start.Add(f.ComputeNormal(uv).Normalize().Multiply(100));
                            XYZ sunDirection = SolarViews.GetSunDirectionalVector(uidoc.ActiveView, SolarViews.GetProjectPosition(uidoc.Document), out double azimuth);
                            start = start.Subtract(sunDirection.Normalize());
                            XYZ end = start.Subtract(sunDirection.Multiply(1000));
                            ////#if DEBUG
                            BuildingCoder.Creator.CreateModelLine(uidoc.Document, start, end);
                            ////#endif
                            ////Line line = Line.CreateBound(start, end);
                            lineCount++;
                        }
                    }
                }
            }

            t.Commit();
            stopwatch.Stop();
            TaskDialog.Show("Time Elapsed", lineCount + " lines drawn in " + stopwatch.Elapsed.ToString() + @"(hh:mm:ss:uu)");
        }

        ////[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Autodesk.Revit.UI.Result Execute(
            ExternalCommandData commandData,
            ref string message,
            Autodesk.Revit.DB.ElementSet elements)
        {
            if (commandData == null) {
                return Autodesk.Revit.UI.Result.Failed;
            }

            UIDocument udoc = commandData.Application.ActiveUIDocument;
            Document doc = udoc.Document;

            dynamic settings = new ExpandoObject();
            settings.Height = 480;
            settings.Width = 300;
            settings.Title = "Direct Sun - By Andrew Nicholas";
            settings.ShowInTaskbar = false;
            settings.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;

            var vm = new ViewModels.DirectSunViewModel(commandData.Application.ActiveUIDocument);
            SCaddinsApp.WindowManager.ShowDialog(vm, null, settings);
            if (vm.SelectedCloseMode == ViewModels.DirectSunViewModel.CloseMode.Analize) {
                RunAnalysis(vm.FaceSelection, vm.MassSelection, 10, udoc);
            }
            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}