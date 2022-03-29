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
    using System.Text;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Caliburn.Micro;

    internal class RunScriptViewModel : Screen
    {
        private string output;
        private string script;
        private BindableCollection<string> outputList;
        private string currentFileName;
        private ExternalCommandData commandData;
        private ElementSet elements;
        private System.Drawing.Color backgroundColour;

        public RunScriptViewModel(ExternalCommandData commandData, ElementSet elements)
        {
            this.commandData = commandData;
            this.elements = elements;
            currentFileName = string.Empty;
            output = string.Empty;
            outputList = new BindableCollection<string>();
            LoadScratch();
            FontSize = 12;
            backgroundColour = System.Drawing.Color.LightGray;
        }

        public static dynamic DefaultViewSettings
        {
            get
            {
                dynamic settings = new ExpandoObject();
                settings.Height = 480;
                settings.Width = 300;
                settings.Title = "Run Lua Script";
                settings.ShowInTaskbar = false;
                settings.Icon = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/SCaddins;component/Assets/lua.png"));
                settings.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
                return settings;
            }
        }

        public bool CanSave => !string.IsNullOrEmpty(currentFileName);

        public double FontSize
        {
            get; set;
        }

        public System.Drawing.Color Background
        {   
            get => backgroundColour;

            set
            {
                backgroundColour = value;
                NotifyOfPropertyChange(() => Background);
            }
        }

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
                if (!string.IsNullOrEmpty(output))
                {
                    using (StringReader sr = new StringReader(Output))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            outputList.Add(line);
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
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                output = value;
                NotifyOfPropertyChange(() => Output);
                NotifyOfPropertyChange(() => OutputList);
            }
        }

        public void DarkMode()
        {
            Background = System.Drawing.Color.Black;
        }

        public void LightMode()
        {
            Background = System.Drawing.Color.LightGray;
        }

        public void IncreaseFontSize()
        {
            FontSize++;
            NotifyOfPropertyChange(() => FontSize);
        }

        public void DecreaseFontSize()
        {
            FontSize--;
            NotifyOfPropertyChange(() => FontSize);
        }

        public void LoadSample()
        {
            var f = SCaddinsApp.WindowManager.ShowFileSelectionDialog(@"C:\Program Files\SCaddins\SCaddins\share\RunScript\HelloWorld.cs", out currentFileName);
            if (f.HasValue && f.Value)
            {
                if (File.Exists(currentFileName))
                {
                    Script = File.ReadAllText(CurrentFileName);
                    SCaddinsApp.WindowManager.LastWindow.Title = CurrentFileName;
                    NotifyOfPropertyChange(() => CanSave);
                }
            }
        }

        public void LoadScratch()
        {
            var s = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var p = Path.Combine(s, "SCaddins", "Script.lua");
            if (!File.Exists(p))
            {
                return;
            }
            Script = File.ReadAllText(p);
        }

        public void LoadScriptFromFile()
        {
            var f = SCaddinsApp.WindowManager.ShowFileSelectionDialog(string.Empty, out currentFileName);
            if (f.HasValue && f.Value)
            {
                if (File.Exists(currentFileName))
                {
                    Script = File.ReadAllText(CurrentFileName);
                    SCaddinsApp.WindowManager.LastWindow.Title = currentFileName;
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
            var r = RunScriptCommand.RunScript(Script, commandData, elements, false);

            var sb = new StringBuilder();
            foreach (var v in r)
            {
                sb.Append(v.ToString());
                sb.AppendLine();
            }
            if (sb.Length == 0) {
                    Output = "No output";
                    return;
            }
            OutputList.Clear();
            Output = sb.ToString();
            SaveScratch();
        }

        public void SaveAs()
        {
            var b = SCaddinsApp.WindowManager.ShowSaveFileDialog(defaultFileName: "script.lua", defaultExtension: "*.lua", filter: "lua-script | *.lua", savePath: out var path);
            if (b.HasValue && b.Value)
            {
                File.WriteAllText(path: path, contents: Script);
                CurrentFileName = path;
                SCaddinsApp.WindowManager.LastWindow.Title = CurrentFileName;
            }
        }

        public void Save()
        {
            if (CanSave)
            {
                SCaddinsApp.WindowManager.LastWindow.Title = currentFileName;
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
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }
            File.WriteAllText(Path.Combine(p, "Script.lua"), Script);
        }
    }
}
