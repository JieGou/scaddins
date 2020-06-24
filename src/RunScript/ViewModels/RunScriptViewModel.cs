﻿// (C) Copyright 2019-2020 by Andrew Nicholas
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

namespace SCaddins.RunScript.ViewModels
{
    using System;
    using System.Dynamic;
    using System.IO;
    using Caliburn.Micro;

    internal class RunScriptViewModel : Screen
    {
        private string output;
        private string script;
        private BindableCollection<string> outputList;
        private string currentFileName;

        public RunScriptViewModel()
        {
            currentFileName = string.Empty;
            output = string.Empty;
            outputList = new BindableCollection<string>();
            Script =
@"using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using SCaddins;

public static void Main(Document doc)
{
    using (var t = new Transaction(doc)) {
        t.Start(""Run Script"");
        var fec = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);
            foreach (var r in fec)
            {
                var room = r as Autodesk.Revit.DB.Architecture.Room;
                room.Name = room.Name.ToUpper();
            }
            t.Commit();
        }
}
";
        }
        
        public static dynamic DefaultViewSettings
        {
            get
            {
                dynamic settings = new ExpandoObject();
                settings.Height = 480;
                settings.Width = 300;
                settings.Title = "Run (cs)Script";
                settings.ShowInTaskbar = false;
                settings.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
                return settings;
            }
        }

        public bool CanSave => !string.IsNullOrEmpty(currentFileName);
             
        public string Script
        {
            get => script;

            set
            {
                script = value;
                NotifyOfPropertyChange(() => Script);
            }
        }
        
        public string CurrentFileName
        {
            get => currentFileName;
            set
            {
                currentFileName = value;
                NotifyOfPropertyChange(() => CanSave);
            }
        }
        
        public BindableCollection<string> OutputList
        {
            get
            {
                outputList.Clear();
                if (!string.IsNullOrEmpty(output)) {
                    using (StringReader sr = new StringReader(Output)) {
                        string line;
                        while ((line = sr.ReadLine()) != null) {
                            outputList.Add(line.Substring(line.IndexOf("(", StringComparison.Ordinal)));
                        }
                    }
                }
                return outputList;
            }
        }
        
        public string Output
        {
            get => output;

            set
            {
                if (string.IsNullOrEmpty(value)) {
                    return;
                }
                output = value;
                NotifyOfPropertyChange(() => Output);
                NotifyOfPropertyChange(() => OutputList);
            }
        }

        public void LoadSample()
        {
            var f = SCaddinsApp.WindowManager.ShowFileSelectionDialog(@"C:\Program Files\SCaddins\SCaddins\share\RunScript\HelloWorld.cs", out currentFileName);
            if (f.HasValue && f.Value)
            {
                if (File.Exists(currentFileName))
                {
                    Script = File.ReadAllText(CurrentFileName);
                    NotifyOfPropertyChange(() => CanSave);
                }
            }
        }
         
        public void LoadScratch()
        {
            var s = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var p = Path.Combine(s, "SCaddins", "Script.cs");
            if (!File.Exists(p)) {
                return;
            }
            Script = File.ReadAllText(p);
        }
        
        public void LoadScriptFromFile()
        {
            var f = SCaddinsApp.WindowManager.ShowFileSelectionDialog(string.Empty, out currentFileName);
            if (f.HasValue && f.Value) {
                if (File.Exists(currentFileName)) {
                    Script = File.ReadAllText(CurrentFileName);
                    NotifyOfPropertyChange(() => CanSave);
                }
            }
        }

        // ReSharper disable once OptionalParameterHierarchyMismatch
        public override void TryClose(bool? dialogResult = false)
        {
            SaveScratch();
            base.TryClose(dialogResult);
        }

        public void Run()
        {
            var result = RunScriptCommand.VerifyScript(RunScriptCommand.ClassifyScript(Script), out var compileResults);
            Output = compileResults;
            if (result) {
                TryClose(true);
            }
        }

        public void SaveAs()
        {
            var b = SCaddinsApp.WindowManager.ShowSaveFileDialog(defaultFileName: "script.cs", defaultExtension: "*.cs", filter: "cs-script | *.cs", savePath: out var path);
            if (b.HasValue && b.Value)
            {
                File.WriteAllText(path: path, contents: Script);
                CurrentFileName = path;
            }
        }
        
        public void Save()
        {
            if (CanSave)
            {
                File.WriteAllText(currentFileName, Script);
            }
            else
            {
                SaveAs();
            }
        }
        
        public void SaveScratch()
        {
            var s = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var p = Path.Combine(s, "SCaddins");
            if (!Directory.Exists(p)) {
                Directory.CreateDirectory(p);
            }
            File.WriteAllText(Path.Combine(p, "Script.cs"), Script);
        }
    }
}