#if UNITY_EDITOR

using Pixyz.Config;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Pixyz.Editor {

    /// <summary>
    /// Extension methods for Unity Editor
    /// </summary>
    public static class ReferencableEditorUtils {

        /// <summary>
        /// Opens a browser to select a file
        /// </summary>
        /// <returns></returns>
        public static string SelectFile(string[] filter) {

            if (!Configuration.CheckLicense()) {
                EditorUtility.DisplayDialog("Pixyz Warning", "Your Pixyz license is inexistant or invalid. Please check the status in the License Manager", "Close");
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
        /// Returns all defined tags.
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> GetTags() {

            HashSet<string> tags = new HashSet<string>();

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            for (int i = 0; i < tagsProp.arraySize; i++) {
                SerializedProperty tagProp = tagsProp.GetArrayElementAtIndex(i);
                tags.Add(tagProp.stringValue);
            }

            return tags;
        }

        /// <summary>
        /// Adds a tag programmatically.
        /// </summary>
        /// <param name="tag"></param>
        public static void AddTag(string tag) {

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty tagProp = tagsProp.GetArrayElementAtIndex(0);
            tagProp.stringValue = tag;

            tagManager.ApplyModifiedProperties();
        }

        /// <summary>
        /// Returns all defined layers.
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> GetLayers() {

            HashSet<string> layers = new HashSet<string>();

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            for (int i = 0; i < layersProp.arraySize; i++) {
                SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                layers.Add(layerProp.stringValue);
            }

            return layers;
        }

        /// <summary>
        /// Adds a layer programmatically.
        /// </summary>
        /// <param name="layer"></param>
        public static void AddLayer(string layer) {

            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            layersProp.InsertArrayElementAtIndex(0);
            SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(0);
            layerProp.stringValue = layer;

            tagManager.ApplyModifiedProperties();
        }
    }
}

#endif