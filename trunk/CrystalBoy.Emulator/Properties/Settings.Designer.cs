﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.225
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CrystalBoy.Emulator.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Direct3DRenderMethod")]
        public string RenderMethod {
            get {
                return ((string)(this["RenderMethod"]));
            }
            set {
                this["RenderMethod"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("320, 288")]
        public global::System.Drawing.Size RenderSize {
            get {
                return ((global::System.Drawing.Size)(this["RenderSize"]));
            }
            set {
                this["RenderSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int ZoomFactor {
            get {
                return ((int)(this["ZoomFactor"]));
            }
            set {
                this["ZoomFactor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>CrystalBoy.Emulation.Rendering.GdiPlus.dll</string>
  <string>CrystalBoy.Emulation.Rendering.Direct3D.dll</string>
  <string>CrystalBoy.Emulation.Rendering.SlimDX.dll</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection PluginAssemblies {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["PluginAssemblies"]));
            }
            set {
                this["PluginAssemblies"] = value;
            }
        }
    }
}
