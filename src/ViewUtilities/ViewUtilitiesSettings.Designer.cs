﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SCaddins.ViewUtilities {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.6.0.0")]
    internal sealed partial class ViewUtilitiesSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ViewUtilitiesSettings defaultInstance = ((ViewUtilitiesSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ViewUtilitiesSettings())));
        
        public static ViewUtilitiesSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("$userName-<<View Name>>-$date")]
        public string UserViewNameFormat {
            get {
                return ((string)(this["UserViewNameFormat"]));
            }
            set {
                this["UserViewNameFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>SC-View_Category;User</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection UserViewParameterReplacements {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["UserViewParameterReplacements"]));
            }
            set {
                this["UserViewParameterReplacements"] = value;
            }
        }
    }
}
