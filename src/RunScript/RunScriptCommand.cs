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

namespace SCaddins.RunScript
{
    using System;
    using System.Text;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using NLua;

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class RunScriptCommand : IExternalCommand
    {
        public static object[] RunScript(string script, ExternalCommandData commandData, ElementSet elements, bool createTransaction)
        {
            if (createTransaction)
            {
                    using (Transaction t = new Transaction(commandData.Application.ActiveUIDocument.Document))
                    {
                            t.Start("Run Lua Script");
                            Lua state = new Lua();
                            state.LoadCLRPackage();
                            state["commandData"] = commandData;
                            state["elements"] = elements;
                            try
                            {
                                    var r = state.DoString(script);
                                    t.Commit();
                                    return r;
                            }
                            catch (NLua.Exceptions.LuaScriptException lse)
                            {
                                    object[] obj = new object[1];
                                    obj[0] = lse.Message;
                                    t.RollBack();
                                    return obj;
                            }
                    }
            } 
            else
            {
                    Lua state = new Lua();
                    state.LoadCLRPackage();
                    state["commandData"] = commandData;
                    state["elements"] = elements;
                    try
                    {
                            var r = state.DoString(script);
                            return r;
                    }
                    catch (NLua.Exceptions.LuaScriptException lse)
                    {
                            object[] obj = new object[1];
                            obj[0] = lse.Message;
                            return obj;
                    }
            }
        }

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            if (commandData == null)
            {
                return Result.Failed;
            }

            var vm = new ViewModels.RunScriptViewModel(commandData, elements);
            bool? result = SCaddinsApp.WindowManager.ShowDialog(vm, null, ViewModels.RunScriptViewModel.DefaultViewSettings);

            return Result.Succeeded;
        }
    }
}
