using Pixyz.Editor;
using Pixyz.Tools;
using Pixyz.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityImporters = UnityEditor.Experimental.AssetImporters;

namespace Pixyz.Import.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptedImporter), true, isFallback = false)]
    public class ScriptedImporterEditor : UnityImporters.AssetImporterEditor
    {
        private ScriptedImporter mainImporter => target as ScriptedImporter;
        private bool _isSettingsOpen = true;
        private bool _isDirty = false;

        public override void OnInspectorGUI() {

            bool hasBeenImported = AssetDatabase.LoadAssetAtPath<ImportStamp>(mainImporter.assetPath) != null;

            // Settings
            _isSettingsOpen = EditorGUIExtensions.DrawCategory(ImportSettingsEditor.Style, "Settings", true, () => {
                var prevImportSettings = mainImporter.importSettings;
                mainImporter.importSettings = (ImportSettings)EditorGUILayout.ObjectField("Preset", mainImporter.importSettings, typeof(ImportSettings), false);
                if (!_isDirty && mainImporter.importSettings != prevImportSettings)
                    _isDirty = true;
                if (_isSettingsOpen) {
                    ImportSettingsEditor settingsEditor = mainImporter.importSettings.GetEditor<ImportSettingsEditor>();
                    settingsEditor?.drawGUI(Importer.GetSettingsTemplate(mainImporter.file));
                    if (!_isDirty && settingsEditor != null && settingsEditor.isChangedInLastFrame)
                        _isDirty = true;
                }
                if (mainImporter.importSettings == null) {
                    EditorStyles.helpBox.richText = true;
                    EditorGUILayout.HelpBox("No <b>Import Settings</b> are attached. Default <b>Import Settings</b> will be used.\nTo create custom <b>Import Settings</b>, click 'Pixyz > Create Import Settings', and then assign the created settings here.", MessageType.Warning);
                }
            }, "",
            _isSettingsOpen);

            // Rules
            EditorGUIExtensions.DrawCategory(ImportSettingsEditor.Style, "Post-Processing", true, () => {
                mainImporter.rules = (RuleSet)EditorGUILayout.ObjectField("Rules", mainImporter.rules, typeof(RuleSet), false);
            }, "");

            // Mandatory !!!
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool importClicked = GUILayout.Button(hasBeenImported ? "Re-Import" : "Import", GUILayout.Height(18), GUILayout.Width(100));
            if (importClicked) {
                // Assign default settings if missing
                if (mainImporter.importSettings == null)
                    mainImporter.importSettings = ImportWindow.ImportSettings;

                // If multiple objects are selected, we apply rules and settings from the main object on other selected objects
                foreach (ScriptedImporter importer in targets) {
                    if (importer != mainImporter) {
                        importer.importSettings = mainImporter.importSettings;
                        importer.rules = mainImporter.rules;
                        EditorUtility.SetDirty(importer);
                    }
                    AssetDatabase.WriteImportSettingsIfDirty(importer.file);
                    //importer.SaveAndReimport();
                }

                ApplyAndImport();

                // We refresh the selection. We postpone this operation to ensure all assets are properly reloaded
                Dispatcher.StartCoroutine(RefreshSelection(targets.Select(x => (x as ScriptedImporter).assetPath)));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        private IEnumerator RefreshSelection(IEnumerable<string> assetPaths)
        {
            yield return Dispatcher.DelayFrames(1);
            AssetDatabase.Refresh();
            yield return Dispatcher.DelayFrames(1);
            Selection.objects = assetPaths.Select(x => AssetDatabase.LoadAssetAtPath<Object>(x)).ToArray();
        }

        public override bool HasModified() {
            return _isDirty;
        }

        protected override void ResetValues() {

        }

        protected override void Apply() {
            _isDirty = false;
            base.Apply();
        }

        public override bool HasPreviewGUI() {
            return true;
        }

        public override void DrawPreview(Rect previewArea) {
            base.DrawPreview(previewArea);
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            base.OnPreviewGUI(r, background);
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }
    }
}