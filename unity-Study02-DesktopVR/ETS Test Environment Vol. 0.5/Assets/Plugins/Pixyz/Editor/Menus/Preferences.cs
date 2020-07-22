using UnityEngine;
using UnityEditor;
using Pixyz.Plugin4Unity;
using System.IO;

namespace Pixyz.Editor {

    internal class PreferencesProvider : SettingsProvider
    {
        public PreferencesProvider(string path, SettingsScope scopes) : base(path, scopes)
        {

        }

        public override void OnGUI(string searchContext)
        {
            // Draw background pixyz transparent logo
            GUI.DrawTexture(new Rect(50, 75, 500, 500), TextureCache.GetTexture("IconPixyzWatermark"));

            EditorGUIUtility.labelWidth = 260;

            EditorGUILayout.LabelField("Import", EditorStyles.boldLabel);
            GUILayout.Space(5);
            Preferences.AliasExecutable = EditorGUILayout.TextField("Alias Executable (Alias.exe)", Preferences.AliasExecutable);
            Preferences.VREDExecutable = EditorGUILayout.TextField("VRED Executable (VREDPro.exe)", Preferences.VREDExecutable);

            GUILayout.Space(5);
            Preferences.PrefabFolder = EditorGUILayout.TextField("Prefab Save Folder", Preferences.PrefabFolder);
            Preferences.OverridePrefabs = EditorGUILayout.Toggle("Override Prefabs", Preferences.OverridePrefabs);
            Preferences.PrefabDependenciesDestination = (PrefabDependenciesDestination)EditorGUILayout.EnumPopup(new GUIContent("Prefab Dependencies Destination", "It is possible to choose where to save dependencies when creating a prefab of a model imported with Pixyz. This is useful for very large models or to have more control over dependencies."), Preferences.PrefabDependenciesDestination);
            Preferences.FileDragAndDropInScene = EditorGUILayout.Toggle("File Drag and Drop in Scene", Preferences.FileDragAndDropInScene);
            Preferences.ImportFilesInAssets = EditorGUILayout.Toggle(new GUIContent("Import Files in Assets folder", "Pixyz can import files in the Asset folder.\nWARNING : Formats that are already supported by Unity (.fbx, .dae, .3ds, .dxf, .obj) can't be imported by Pixyz this way. You can however use the 'Pixyz > Import Model' to import such files."), Preferences.ImportFilesInAssets);
            GUILayout.Space(5);
            Preferences.UXImprovementStats = EditorGUILayout.Toggle(new GUIContent("UX Improvement Stats", "Enables the Pixyz PLUGIN for Unity to collect data that will be sent anonymously for UX improvements"), Preferences.UXImprovementStats);
            Preferences.LogImportTime = EditorGUILayout.Toggle("Log Import Time", Preferences.LogImportTime);
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
            GUILayout.Space(5);
            Preferences.RightClickInSceneForToolbox = EditorGUILayout.Toggle("Show Toolbox on Right Click in Scene", Preferences.RightClickInSceneForToolbox);
            Preferences.LogTimeWithToolbox = EditorGUILayout.Toggle("Log Toolbox Processing Time", Preferences.LogTimeWithToolbox);
            Preferences.LogTimeWithRuleEngine = EditorGUILayout.Toggle("Log RuleEngine Processing Time", Preferences.LogTimeWithRuleEngine);
            GUILayout.Space(10);

            bool hasTokenUnityRuntime = NativeInterface.IsTokenValid("UnityRuntime");
            EditorGUILayout.LabelField("Include Pixyz in Builds", EditorStyles.boldLabel);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.width = rect.height = rect.height + 6;
            rect.y -= 3;
            rect.x = 170;
            GUI.Button(rect, new GUIContent("", "On platforms below, Pixyz is able to .pxz models at runtime. However, this requires the Pixyz libraries to be included in the build.\n" +
                "To include librairies for a specific plateform, tick the checkbox for the corresponding platform.\n" +
                "Pixyz librairies weights about a few hundred megabytes."), StaticStyles.IconInfoSmall);
            GUILayout.Space(5);
            if (!hasTokenUnityRuntime) {
                EditorGUILayout.HelpBox("The user of the built application will require a special license to be able to import or optimize at runtime using Pixyz. Please contact us at sales@pi.xyz for more information", MessageType.Warning);
            }
            Preferences.IncludeForRuntimeWindows = EditorGUILayout.Toggle("Windows Standalone x64", Preferences.IncludeForRuntimeWindows);
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Update", EditorStyles.boldLabel);
            GUILayout.Space(5);
            Preferences.AutomaticUpdate = EditorGUILayout.Toggle("Automatically check for Pixyz update", Preferences.AutomaticUpdate);
        }
    }

    public enum PrefabDependenciesDestination
    {
        InPrefab = 0,
        InFolder = 1
    }

    public static class Preferences {

        [SettingsProvider]
        private static SettingsProvider MyNewPrefCode()
        {
            return new PreferencesProvider("Preferences/Pixyz", SettingsScope.User);
        }

        // Import
        public static string PrefabFolder {
            get { return EditorPrefs.GetString("Pixyz_PrefabFolder", "3DModels"); }
            set { EditorPrefs.SetString("Pixyz_PrefabFolder", value); }
        }
        
        public static string AliasExecutable {
            get {
                return EditorPrefs.GetString("Pixyz_AliasExecutable", "");
            }
            set {
                EditorPrefs.SetString("Pixyz_AliasExecutable", value);
            }
        }

        public static string VREDExecutable {
            get {
                return EditorPrefs.GetString("Pixyz_VREDExecutable", @"C:\Program Files\Autodesk\VREDPro-12.0\Bin\WIN64\VREDPro.exe");
            }
            set {
                EditorPrefs.SetString("Pixyz_VREDExecutable", value);
            }
        }

        public static string PluginLocation
        {
            get
            {
                string path = AssetDatabase.GUIDToAssetPath("4c77e143aa0c37d498d4c8d9dcbfb570");
                path = path.Replace("/Runtime/Import/ImportSettings.cs", string.Empty);
                return path;
            }
        }

        public static string PluginLocationAbsolute
        {
            get
            {
                return Path.GetFullPath(PluginLocation);
            }
        }

        public static bool OverridePrefabs {
            get { return EditorPrefs.GetBool("Pixyz_OverridePrefabs", false); }
            set { EditorPrefs.SetBool("Pixyz_OverridePrefabs", value); }
        }

        public static bool FileDragAndDropInScene {
            get { return EditorPrefs.GetBool("Pixyz_EnableFileDragAndDropInScene", true); }
            set { EditorPrefs.SetBool("Pixyz_EnableFileDragAndDropInScene", value); }
        }

        public static bool LogImportTime {
            get { return EditorPrefs.GetBool("Pixyz_LogImportTime", false); }
            set { EditorPrefs.SetBool("Pixyz_LogImportTime", value); }
        }

        public static bool ImportFilesInAssets {
            get { return EditorPrefs.GetBool("Pixyz_ImportFilesInAssets", true); }
            set { EditorPrefs.SetBool("Pixyz_ImportFilesInAssets", value); }
        }

        public static PrefabDependenciesDestination PrefabDependenciesDestination {
            get { return (PrefabDependenciesDestination)EditorPrefs.GetInt("Pixyz_PrefabDependenciesDestination", (int)PrefabDependenciesDestination.InPrefab); }
            set { EditorPrefs.SetInt("Pixyz_PrefabDependenciesDestination", (int)value); }
        }

        public static bool UXImprovementStats {
            get { return PlayerPrefs.GetInt("Pixyz_UXImprovementStats", 1) == 1; }
            set {
                if (UXImprovementStats != value) {
                    PlayerPrefs.SetInt("Pixyz_UXImprovementStats", value ? 1 : 0);
                }
            }
        }

        // Toolbox
        public static bool RightClickInSceneForToolbox {
            get { return EditorPrefs.GetBool("Pixyz_RightClickInSceneForToolbox", true); }
            set { EditorPrefs.SetBool("Pixyz_RightClickInSceneForToolbox", value); }
        }

        public static bool LogTimeWithToolbox {
            get { return EditorPrefs.GetBool("Pixyz_LogTimeWithToolbox", false); }
            set { EditorPrefs.SetBool("Pixyz_LogTimeWithToolbox", value); }
        }

        // RuleEngine
        public static bool LogTimeWithRuleEngine {
            get { return EditorPrefs.GetBool("Pixyz_LogTimeWithRuleEngine", false); }
            set { EditorPrefs.SetBool("Pixyz_LogTimeWithRuleEngine", value); }
        }

        // CheckForUpdate
        public static bool AutomaticUpdate {
            get { return EditorPrefs.GetBool("Pixyz_AutomaticUpdate", true); }
            set { EditorPrefs.SetBool("Pixyz_AutomaticUpdate", value); }
        }

        public static bool IncludeForRuntimeWindows {
            get {
                return EditorPrefs.GetBool("Pixyz_IncludeForRuntimeWindows", NativeInterface.IsTokenValid("UnityRuntime"));
            }
            set { EditorPrefs.SetBool("Pixyz_IncludeForRuntimeWindows", value); }
        }
    }
}