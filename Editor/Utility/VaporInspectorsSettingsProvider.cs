using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.UIElements;
using VaporUIElements;

namespace VaporUIElementsEditor
{
    public static class VaporInspectorQuickMenu
    {
        [MenuItem("Vapor/Inspector/Open Settings")]
        private static void OpenInspectorSettings()
        {
            SettingsService.OpenUserPreferences("Vapor/Inspector Settings");
        }
    }
    
    public class VaporInspectorsSettingsProvider : SettingsProvider
    {
        private const string EnableVaporInspectors = "enableVaporInspectors";
        private const string EnableExplicitImplementation = "enableExplicitImplementation";

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new VaporInspectorsSettingsProvider("Vapor/Inspector Settings", SettingsScope.User);
        }

        public static bool VaporInspectorsEnabled
        {
            get => EditorPrefs.GetBool(EnableVaporInspectors, true);
            set => EditorPrefs.SetBool(EnableVaporInspectors, value);
        }
        
        public static bool ExplicitImplementationEnabled
        {
            get => EditorPrefs.GetBool(EnableExplicitImplementation, false);
            set => EditorPrefs.SetBool(EnableExplicitImplementation, value);
        }

        public VaporInspectorsSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
            
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var header = new StyledHeaderBox("Inspector Settings")
            {
                style =
                {
                    marginLeft = 3,
                    marginRight = 3
                }
            };

            var enableTog = new Toggle("Enable Vapor Inspectors")
            {
                tooltip = "Enables the VAPOR_INSPECTOR define allowing for drawing to be done."
            };
            enableTog.SetValueWithoutNotify(VaporInspectorsEnabled);
            enableTog.RegisterValueChangedCallback(x =>
            {
                VaporInspectorsEnabled = x.newValue;
                DefineVaporEnabled();
            });
            var explicitTog = new Toggle("Only Explicit Implementation")
            {
                tooltip =
                    "The user must inherit BaseVaporInspector on custom classes to draw vapor inspectors." +
                    "\nThis enables the VAPOR_INSPECTOR_EXPLICIT define."
            };
            explicitTog.SetValueWithoutNotify(ExplicitImplementationEnabled);
            explicitTog.RegisterValueChangedCallback(x =>
            {
                ExplicitImplementationEnabled = x.newValue;
                DefineExplicitImplementation();
            });

            DefineVaporEnabled();
            DefineExplicitImplementation();
            header.Add(enableTog);
            header.Add(explicitTog);
            rootElement.Add(header);
            base.OnActivate(searchContext, rootElement);
        }

        private static void DefineVaporEnabled()
        {
            var enabled = VaporInspectorsEnabled;
            PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), out var defines);
            if (enabled)
            {
                if (!defines.Contains("VAPOR_INSPECTOR"))
                {
                    ArrayUtility.Add(ref defines, "VAPOR_INSPECTOR");
                    PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), defines);
                }
            }
            else
            {
                if (defines.Contains("VAPOR_INSPECTOR"))
                {
                    ArrayUtility.Remove(ref defines, "VAPOR_INSPECTOR");
                    PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), defines);
                }
            }
        }
        
        private static void DefineExplicitImplementation()
        {
            var isExplicit = ExplicitImplementationEnabled;
            PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), out var defines);
            if (isExplicit)
            {
                if (!defines.Contains("VAPOR_INSPECTOR_EXPLICIT"))
                {
                    ArrayUtility.Add(ref defines, "VAPOR_INSPECTOR_EXPLICIT");
                    PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), defines);
                }
            }
            else
            {
                if (defines.Contains("VAPOR_INSPECTOR_EXPLICIT"))
                {
                    ArrayUtility.Remove(ref defines, "VAPOR_INSPECTOR_EXPLICIT");
                    PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), defines);
                }
            }
        }
    }
}
