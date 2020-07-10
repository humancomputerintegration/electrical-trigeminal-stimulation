using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Pixyz.Editor;
using Pixyz.Plugin4Unity;

namespace Pixyz.Import.Editor {

    /// <summary>
    /// Editor class for ImportStamp
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ImportStamp))]
    public class ImportStampEditor : UnityEditor.Editor {

        static private HashSet<int> _openedStampEditor = new HashSet<int>();

        public ImportStamp importStamp => target as ImportStamp;
        private LODGroup[] _lodGroups = null;

        public override void OnInspectorGUI() {

            GUILayout.Space(15);
            drawAsSceneInstance(Event.current.type);
        }

        bool foldout {
            get { return _openedStampEditor.Contains(importStamp.GetInstanceID()); }
            set
            {
                if (value)
                    _openedStampEditor.Add(importStamp.GetInstanceID());
                else
                    _openedStampEditor.Remove(importStamp.GetInstanceID());
            }
        }

        private void drawAsSceneInstance(EventType eventType) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("File Path", GUILayout.Width(EditorGUIUtility.labelWidth - 5));
            EditorGUILayout.SelectableLabel(importStamp.path, GUILayout.Height(16));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Import Date", GUILayout.Width(EditorGUIUtility.labelWidth - 5));
            EditorGUILayout.SelectableLabel(new DateTime(importStamp.importTime).ToString("yyyy/MM/dd HH:mm:ss"), GUILayout.Height(16));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Import Duration", GUILayout.Width(EditorGUIUtility.labelWidth - 5));
            EditorGUILayout.SelectableLabel(new DateTime(importStamp.importDuration).ToString("HH:mm:ss.fff"), GUILayout.Height(16));
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8);
            if (importStamp.importSettings) {
                foldout = EditorGUIExtensions.DrawCategory(ImportSettingsEditor.Style, "Settings Preset", true, () => {
                    LodGroupPlacement mode = importStamp.importSettings.lodsMode;
                    if (foldout) {
                        var editor = importStamp.importSettings.GetEditor<ImportSettingsEditor>();
                        editor.drawGUI(Importer.GetSettingsTemplate(importStamp.path));
                    } else if (importStamp.importSettings.hasLODs &&
                              Importer.GetSettingsTemplate(importStamp.fullPath).hasLODs.status != ParameterAvailability.Hidden &&
                              importStamp.importSettings.treeProcess != TreeProcessType.MERGE_ALL) {
                        serializedObject.Update();
                        var serializedImportSettings = new SerializedObject(serializedObject.FindProperty("_importSettings").objectReferenceValue);
                        EditorGUILayout.PropertyField(serializedImportSettings.FindProperty("lodsMode"), new GUIContent("LODs Mode", "The LODs level tells if the LOD should be managed at assembly (Root) or at model (Leaves) level."));
                        if (importStamp.importSettings.lodsMode == LodGroupPlacement.LEAVES)
                            EditorGUILayout.PropertyField(serializedImportSettings.FindProperty("qualities"), new GUIContent("Qualities", "Qualities of LODs"));
                        serializedImportSettings.ApplyModifiedProperties();
                    }
                    LodGroupPlacement newMode = importStamp.importSettings.lodsMode;
                    if (mode != newMode) {
                        importStamp.changeLODMode(newMode);
                    }
                    if (importStamp.importSettings.hasLODs && eventType == EventType.MouseUp) {
                        if (_lodGroups == null) {
                            _lodGroups = importStamp.importSettings.lodsMode == LodGroupPlacement.LEAVES ? importStamp.gameObject.GetComponentsInChildren<LODGroup>() : new LODGroup[] { importStamp.gameObject.GetComponent<LODGroup>() };
                        }
                        foreach (var group in _lodGroups) {
                            var lods = group.GetLODs();
                            for (int i = 0; i < group.lodCount; ++i) {
                                lods[i].screenRelativeTransitionHeight = (float)importStamp.importSettings.qualities.lods[i].threshold;
                            }
                            group.SetLODs(lods);
                        }
                    }
                    GUI.enabled = true;
                }, @"The settings used to import the imported model." +
                   (importStamp.importSettings.hasLODs && importStamp.importSettings.lodsMode == LodGroupPlacement.LEAVES ? "Modify the threshold of this settings to modify the threshold of all LODGroup under this GameObject." : ""),
                   foldout);
            }
        }
    }
}
