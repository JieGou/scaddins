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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.9.0.0")]
    internal sealed partial class ViewUtilitiesSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ViewUtilitiesSettings defaultInstance = ((ViewUtilitiesSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ViewUtilitiesSettings())));
        
        public static ViewUtilitiesSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("$user-<<View Name>>-$date")]
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
        [global::System.Configuration.DefaultSettingValueAttribute("SC-View_Category_Primary")]
        public string FirstParamName {
            get {
                return ((string)(this["FirstParamName"]));
            }
            set {
                this["FirstParamName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("USER")]
        public string FirstParamValue {
            get {
                return ((string)(this["FirstParamValue"]));
            }
            set {
                this["FirstParamValue"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SC-View_Category_Secondary")]
        public string SecondParamName {
            get {
                return ((string)(this["SecondParamName"]));
            }
            set {
                this["SecondParamName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("$user")]
        public string SecondParamValue {
            get {
                return ((string)(this["SecondParamValue"]));
            }
            set {
                this["SecondParamValue"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ThirdParamName {
            get {
                return ((string)(this["ThirdParamName"]));
            }
            set {
                this["ThirdParamName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ThirdParamValue {
            get {
                return ((string)(this["ThirdParamValue"]));
            }
            set {
                this["ThirdParamValue"] = value;
            }
        }
    }
}
