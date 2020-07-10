using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Import.Editor {

    /// <summary>
    /// Editor for Metadatas and Metadata Instances
    /// </summary>
    [CustomEditor(typeof(Metadata), editorForChildClasses: true)]
    public sealed class MetadataEditor : UnityEditor.Editor {

        public Metadata metadata { get { return target as Metadata; } }
        private bool showPXZProperties = false;

        const float COLUMN_RATIO = 0.4f;
        const float ROW_HEIGHT = 17;

        public override void OnInspectorGUI() {

            Dictionary<string, string> properties = metadata.getProperties();

            List<string> metadataNames = new List<string>();
            List<string> metadataValues = new List<string>();
            List<string> metadataPxzNames = new List<string>();
            List<string> metadataPxzValues = new List<string>();

            foreach (KeyValuePair<string, string> property in properties) {
                if (property.Key.StartsWith("PXZ")) {
                    metadataPxzNames.Add(property.Key);
                    metadataPxzValues.Add(property.Value);
                } else {
                    metadataNames.Add(property.Key);
                    metadataValues.Add(property.Value);
                }
            }

            // Display Metadata
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Name", EditorStyles.boldLabel);
                    for (int i = 0; i < metadataNames.Count; i++) {
                        EditorGUILayout.SelectableLabel(metadataNames[i], EditorStyles.textField, GUILayout.Height(ROW_HEIGHT));
                    }
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
                    for (int i = 0; i < metadataValues.Count; i++) {
                        EditorGUILayout.SelectableLabel(metadataValues[i], EditorStyles.textField, GUILayout.Height(ROW_HEIGHT));
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            // Display PXZ Metadata
            if (metadataPxzNames.Count > 0) {
                showPXZProperties = EditorGUILayout.Foldout(showPXZProperties, "Pixyz properties");
                if (showPXZProperties) {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("Name", EditorStyles.boldLabel);
                            for (int i = 0; i < metadataPxzNames.Count; i++) {
                                EditorGUILayout.SelectableLabel(metadataPxzNames[i], EditorStyles.textField, GUILayout.Height(ROW_HEIGHT));
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical();
                        {
                            EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
                            for (int i = 0; i < metadataPxzValues.Count; i++) {
                                EditorGUILayout.SelectableLabel(metadataPxzValues[i], EditorStyles.textField, GUILayout.Height(ROW_HEIGHT));
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}