using Pixyz.Config;
using Pixyz.Plugin4Unity;
using Pixyz.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Editor {

    public enum ColorType { Highlight, Active }

    /// <summary>
    /// Extension methods for Unity Editor
    /// </summary>
    public static class EditorExtensions {

        /// <summary>
        /// Opens a browser to select a file
        /// </summary>
        /// <returns></returns>
        public static string SelectFile(string[] filter) {

            if (!Configuration.CheckLicense()) {
                if (EditorUtility.DisplayDialog("Pixyz Warning", "A Pixyz Plugin license is required", "Open License Manager", "Close")) {
                    OpenWindow<LicensingWindow>();
                }
                return null;
            }

            string file = EditorUtility.OpenFilePanelWithFilters("Select File", "", filter);
            if (string.IsNullOrEmpty(file))
                return null;

            if (!File.Exists(file))
                throw new FileNotFoundException();

            return file;
        }

        /// <summary>
        /// Creates an instance of a ScriptableObject, as well as creating an Asset for it at the active object location
        /// </summary>
        /// <typeparam name="TScriptableObject"></typeparam>
        public static TScriptableObject CreateAsset<TScriptableObject>(string assetName = null) where TScriptableObject : ScriptableObject {
            TScriptableObject asset = ScriptableObject.CreateInstance<TScriptableObject>();
            asset.SaveAsset(assetName, true);
            return asset;
        }

        /// <summary>
        /// Saves a ScriptableObject as an Asset.
        /// </summary>
        /// <param name="scriptableObject"></param>
        /// <param name="assetName">Asset name (which will be the file name without .asset extension)</param>
        public static void SaveAsset(this ScriptableObject scriptableObject, string assetName, bool focusOnSave = false, string path = null) {

            // If the assetName isn't given, it will use the Type name instead
            if (string.IsNullOrEmpty(assetName))
                assetName = "New " + scriptableObject.GetType().Name;

            if (string.IsNullOrEmpty(path)) {
                path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (string.IsNullOrEmpty(path)) {
                    path = "Assets";
                } else if (Path.GetExtension(path) != "") {
                    path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
                }
            }

            string assetPathAndName = $"{path}/{assetName}.asset";
            assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(assetPathAndName);

            AssetDatabase.CreateAsset(scriptableObject, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();

            if (focusOnSave) {
                Selection.activeObject = scriptableObject;
            }
        }

        private static Dictionary<ScriptableObject, UnityEditor.Editor> _Editors = new Dictionary<ScriptableObject, UnityEditor.Editor>();

        public static TEditor GetEditor<TEditor>(this ScriptableObject scriptableObject) where TEditor : UnityEditor.Editor {

            if (scriptableObject == null)
                return null;

            if (!_Editors.ContainsKey(scriptableObject)) {
                _Editors.Add(scriptableObject, UnityEditor.Editor.CreateEditor(scriptableObject) as TEditor);
            }

            if (_Editors[scriptableObject] == null || _Editors[scriptableObject].serializedObject.targetObject == null) {
                _Editors[scriptableObject] = UnityEditor.Editor.CreateEditor(scriptableObject) as TEditor;
            }
            
            return (TEditor)_Editors[scriptableObject];
        }

        /// <summary>
        /// Gets the main Unity Editor position and size
        /// </summary>
        /// <returns></returns>
        public static Rect GetEditorMainWindowPos() {
            var containerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes<ScriptableObject>().Where(t => t.Name == "ContainerWindow").FirstOrDefault();
            if (containerWinType == null)
                throw new MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
            var showModeField = containerWinType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var positionProperty = containerWinType.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
                throw new MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
            UnityEngine.Object[] ewindows = Resources.FindObjectsOfTypeAll(containerWinType);
            foreach (var win in ewindows) {
                var showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) // main window
                {
                    var pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }
            throw new NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
        }


        public static bool IsSerialized(this UnityEngine.Object unityObject) {
            return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(unityObject));
        }

        /// <summary>
        /// Centers the given EditorWindow relatively to the main Unity Editor
        /// </summary>
        /// <param name="editorWindow"></param>
        public static void CenterOnEditor(this EditorWindow editorWindow) {
            var main = GetEditorMainWindowPos();
            var pos = editorWindow.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            editorWindow.position = pos;
        }

        /// <summary>
        /// Uses reflection to return a tooltip contained in a TooltipAttribute for a given field. Otherwise, returns null.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        private static TooltipAttribute GetTooltip(FieldInfo field, bool inherit) {
            TooltipAttribute[] attributes = field.GetCustomAttributes(typeof(TooltipAttribute), inherit) as TooltipAttribute[];
            return attributes.Length > 0 ? attributes[0] : null;
        }

        /// <summary>
        /// Creates an Unity prefab from a GameObject and dependencies
        /// </summary>
        /// <param name="gameObject">Root GameObject to save a prefab</param>
        /// <param name="path">Path to the prefab without extension (Asset/..../MyPrefab)</param>
        /// <param name="dependencies">Dependencies to save inside the prefab</param>
        /// <returns></returns>
        public static GameObject CreatePrefab(this GameObject gameObject, string path, IList<UnityEngine.Object> dependencies = null) {

            path = path + ".prefab";

            /// Ensures directory exists (recursive)
            string projectPath = Directory.GetParent(Application.dataPath).FullName;
            Directory.CreateDirectory(Path.GetDirectoryName(projectPath + "/" + path));

            if (!Preferences.OverridePrefabs) {
                path = AssetDatabase.GenerateUniqueAssetPath(path);
            }

            /// We create prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path);
            /// We add the dependencies
            if (dependencies != null) {
                foreach (var dependency in dependencies) {
                    if (dependency != null) {
                        AssetDatabase.AddObjectToAsset(dependency, prefab);
                    }
                }
            }
            /// On resauvegarde le prefab... (pas le choix, sinon les liens vers le dependences sont perdues)
            prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path, InteractionMode.AutomatedAction);
            AssetDatabase.SaveAssets();
            
            return prefab;
        }

        public static void CreateAsset<TUnityObject>(this TUnityObject unityObject, string path) where TUnityObject : UnityEngine.Object
        {
            if (unityObject is UnityEngine.Material) {
                path += ".mat";
            } else {
                path += ".asset";
            }

            /// Ensures directory exists (recursive)
            string projectPath = Directory.GetParent(Application.dataPath).FullName;
            Directory.CreateDirectory(Path.GetDirectoryName(projectPath + "/" + path));

            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(unityObject, path);
        }

        /// <summary>
        /// Returns an editor window.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="utility">If true, a floating window is created. If false, a normal window is created</param>
        /// <param name="windowTitle"></param>
        /// <returns></returns>
        public static EditorWindow GetEditorWindow(Type type, bool utility, string windowTitle) {
            var window = EditorWindow.GetWindow(type, utility);
            string customVersionTag = Configuration.CustomVersionTag;
            window.titleContent = new GUIContent(windowTitle + (string.IsNullOrEmpty(customVersionTag) ? "" : $" [{customVersionTag}]"));
            return window;
        }

        /// <summary>
        /// Opens an editor window
        /// </summary>
        /// <typeparam name="TEditorWindow"></typeparam>
        public static TEditorWindow OpenWindow<TEditorWindow>() where TEditorWindow : SingletonEditorWindow
        {
            var window = EditorWindow.GetWindow(typeof(TEditorWindow), true) as TEditorWindow;
            string customVersionTag = Configuration.CustomVersionTag;
            window.titleContent = new GUIContent(window.WindowTitle + (string.IsNullOrEmpty(customVersionTag) ? "" : $" [{customVersionTag}]"));
            window.CenterOnEditor();
            window.Show();
            return window;
        }

        /// <summary>
        /// Returns all non persistent (also called volatile) dependencies from a given GameObject and it's children recursively.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static UnityEngine.Object[] GetVolatileDependencies(this GameObject gameObject)
        {
            return GetVolatileDependencies(new GameObject[] { gameObject }.GetChildren(true, true));
        }

        /// <summary>
        /// Returns all non persistent (also called volatile) dependencies from given GameObjects.
        /// </summary>
        /// <param name="gameObjects"></param>
        /// <returns></returns>
        public static UnityEngine.Object[] GetVolatileDependencies(this IList<GameObject> gameObjects)
        {

            var materials = gameObjects.GetMaterials();
            var meshes = gameObjects.GetMeshesUnique();

            var udependencies = new HashSet<UnityEngine.Object>();

            var deps = new List<UnityEngine.Object>(EditorUtility.CollectDependencies(gameObjects.ToArray()));
            for (int i = 0; i < deps.Count; i++)
            {
                CollectDependenciesRecursive(deps[i], ref udependencies);
            }

            var volatileAssets = new HashSet<UnityEngine.Object>();

            foreach (var dep in udependencies)
            {
                if ((dep is Mesh || dep is Material || dep is Texture) && !AssetDatabase.Contains(dep))
                {
                    volatileAssets.Add(dep);
                }
            }

            return volatileAssets.ToArray();
        }

        public static void CollectDependenciesRecursive(UnityEngine.Object obj, ref HashSet<UnityEngine.Object> dependencies)
        {
            if (!dependencies.Contains(obj))
            {
                dependencies.Add(obj);

                SerializedObject objSO = new SerializedObject(obj);
                SerializedProperty property = objSO.GetIterator();

                do
                {
                    if ((property.propertyType == SerializedPropertyType.ObjectReference)
                        && (property.objectReferenceValue != null)
                        && (property.name != "m_PrefabParentObject")  //Don't follow prefabs
                        && (property.name != "m_PrefabInternal")  //Don't follow prefab internals
                        && (property.name != "m_Father")) //Don't go back up the hierarchy in transforms
                    {
                        CollectDependenciesRecursive(property.objectReferenceValue, ref dependencies);
                    }
                } while (property.Next(true));
            }
        }

        /// <summary>
        /// Returns unique textures used in a given material.
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static HashSet<Texture> GetTextures(this Material material) {
            var textures = new HashSet<Texture>();
            Shader shader = material.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++) {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv) {
                    Texture texture = material.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                    if (texture)
                        textures.Add(texture);
                }
            }
            return textures;
        }
    }
}