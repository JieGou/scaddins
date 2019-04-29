﻿namespace SCaddins.HatchEditor
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Text;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class Command : IExternalCommand
    {
        public static List<FillPattern> FillPatterns(Document doc)
        {
            var result = new List<FillPattern>();
            using (var f = new FilteredElementCollector(doc))
            {
                f.OfClass(typeof(FillPatternElement));
                foreach (FillPatternElement e in f)
                {
                    result.Add(e.GetFillPattern());
                }
            }
            return result;
        }

        public static string GetPatternDefinition(FillPattern pattern)
        {
            StringBuilder s = new StringBuilder();
            foreach (var p in pattern.GetFillGrids())
            {
                s.Append(string.Format("{0},\t{1},\t{2},\t{3},\t{4}", p.Angle.ToDeg(), p.Origin.U.ToMM(), p.Origin.V.ToMM(), p.Offset.ToMM(), p.Shift.ToMM()));
                s.Append(System.Environment.NewLine);
            }
            return s.ToString();
        }

        public Autodesk.Revit.UI.Result Execute(
            ExternalCommandData commandData,
            ref string message,
            Autodesk.Revit.DB.ElementSet elements)
        {
            if (commandData == null)
            {
                return Result.Failed;
            }

            UIDocument udoc = commandData.Application.ActiveUIDocument;

            dynamic settings = new ExpandoObject();
            settings.Height = 480;
            settings.Width = 768;
            settings.Title = "Hatch Editor - By Andrew Nicholas";
            settings.ShowInTaskbar = false;
            settings.SizeToContent = System.Windows.SizeToContent.Manual;

            var vm = new ViewModels.HatchEditorViewModel(udoc.Document);
            SCaddinsApp.WindowManager.ShowDialog(vm, null, settings);
            return Result.Succeeded;
        }
    }
}