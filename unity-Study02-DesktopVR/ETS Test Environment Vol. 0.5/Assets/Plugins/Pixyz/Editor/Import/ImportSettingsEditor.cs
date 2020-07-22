using UnityEngine;
using UnityEditor;
using Pixyz.Editor;
using Pixyz.Utils;
using Pixyz.Plugin4Unity;

namespace Pixyz.Import.Editor {

    /// <summary>
    /// Editor class for ImportSettings.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ImportSettings))]
    public class ImportSettingsEditor : UnityEditor.Editor {

        [InitializeOnLoadMethod]
        private static void Register() {
            var templatePC = ImportSettingsTemplate.Default;
            templatePC.name = "Point Cloud";
            templatePC.importLines.status = ParameterAvailability.Hidden;
            templatePC.importLines.defaultValue = false;
            templatePC.importPatchBorders.status = ParameterAvailability.Hidden;
            templatePC.importPatchBorders.defaultValue = false;
            templatePC.importPoints.status = ParameterAvailability.Hidden;
            templatePC.importPoints.defaultValue = true;
            // Transforms
            templatePC.mergeFinalLevel.status = ParameterAvailability.Hidden;
            templatePC.mergeFinalLevel.defaultValue = false;
            templatePC.treeProcess.status = ParameterAvailability.Hidden;
            templatePC.treeProcess.defaultValue = TreeProcessType.FULL;
            // Geometry
            templatePC.voxelizeGridSize.status = ParameterAvailability.Available;
            templatePC.splitTo16BytesIndex.status = ParameterAvailability.Hidden;
            templatePC.splitTo16BytesIndex.defaultValue = false;
            templatePC.orient.status = ParameterAvailability.Hidden;
            templatePC.orient.defaultValue = false;
            templatePC.singularizeSymmetries.status = ParameterAvailability.Hidden;
            templatePC.singularizeSymmetries.defaultValue = false;
            templatePC.hasLODs.status = ParameterAvailability.Available;
            templatePC.hasLODs.defaultValue = false;
            templatePC.lodsMode.status = ParameterAvailability.Hidden;
            templatePC.lodsMode.defaultValue = LodGroupPlacement.LEAVES;
            templatePC.qualities.status = ParameterAvailability.Available;
            templatePC.quality.name = "Point Cloud Density";
            templatePC.quality.tooltip = "The density of the points in the point cloud. 'Maximum' will leave the point cloud unchanged. 'High' or lower will reduce the number of points in the point cloud, which can be useful for performance and memory.";
            // Rendering
            templatePC.mapUV.status = ParameterAvailability.Hidden;
            templatePC.mapUV.defaultValue = false;
            templatePC.createLightmapUV.status = ParameterAvailability.Hidden;
            templatePC.createLightmapUV.defaultValue = false;
            // Other
            templatePC.repair.status = ParameterAvailability.Hidden;
            templatePC.repair.defaultValue = false;
            Importer.AddOrSetTemplate(".e57", templatePC);
            Importer.AddOrSetTemplate(".ptx", templatePC);
            Importer.AddOrSetTemplate(".pts", templatePC);
        }

        private ImportSettings _importSettings;
        public ImportSettings importSettings {
            get {
                if (_importSettings == null) {
                    _importSettings = target as ImportSettings;
                    _importSettings.changed += settingsChanged;
                }
                return _importSettings;
            }
        }

        public VoidHandler changed;

        private void settingsChanged() {
            /// The ImportSettings have changed :
            /// We first update the SerializedObject instance, which may not be up-to-date if the change comes from a different editor.
            serializedObject.Update();
            /// We ask the window to kindly repaint herself
            Repaint();
            /// Finally, we propagate the news to eventual Windows or such objects that are using the Editor
            changed?.Invoke();
        }

        private static ColoredTheme _Style;
        public static ColoredTheme Style => (_Style != null) ? _Style : _Style = new ColoredTheme(new Color(0.33f, 0.33f, 0.33f, 0.75f));

        public override void OnInspectorGUI() {
            drawGUI(ImportSettingsTemplate.Default);
        }

        public bool isChangedInLastFrame { get; private set; }

        public static bool IsDrawingForPointCloud;

        public bool drawSetting<T>(ref T value, Parameter<T> setting, int indent = 0) {
            EditorGUI.indentLevel = 0;
            if (setting.status == ParameterAvailability.Hidden) {
                value = setting.defaultValue;
                return false;
            }
            EditorGUI.BeginDisabledGroup(setting.status == ParameterAvailability.Locked);
            value = (T)value.DrawGUIAuto<T>(new GUIContent(setting.name, setting.tooltip));
            EditorGUI.EndDisabledGroup();
            return true;
        }

        public void drawGUI(ImportSettingsTemplate template) {
            bool guiEnabled = GUI.enabled;
            GUI.enabled = guiEnabled && !importSettings.locked;

            IsDrawingForPointCloud = template.name == "Point Cloud";

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical();

            /// Coordinates Category
            if (template.loadMetadata.status != ParameterAvailability.Hidden
             || template.importPatchBorders.status != ParameterAvailability.Hidden
             || template.importPoints.status != ParameterAvailability.Hidden
             || template.importLines.status != ParameterAvailability.Hidden) {
                EditorGUIExtensions.DrawSubCategory(Style, "IMPORT", () => {
                    if (importSettings.treeProcess == TreeProcessType.FULL)
                        drawSetting(ref importSettings.loadMetadata, template.loadMetadata);
                    drawSetting(ref importSettings.importPatchBorders, template.importPatchBorders);
                    drawSetting(ref importSettings.importLines, template.importLines);
                    drawSetting(ref importSettings.importPoints, template.importPoints);
                }, "Data type to import.");
            }

            /// Hierarchy Category
            if (template.scaleFactor.status != ParameterAvailability.Hidden
             || template.isLeftHanded.status != ParameterAvailability.Hidden
             || template.isZUp.status != ParameterAvailability.Hidden
             || template.treeProcess.status != ParameterAvailability.Hidden) {
                EditorGUIExtensions.DrawSubCategory(Style, "TRANSFORMS", () => {
                    drawSetting(ref importSettings.scaleFactor, template.scaleFactor);
                    drawSetting(ref importSettings.isLeftHanded, template.isLeftHanded);
                    drawSetting(ref importSettings.isZUp, template.isZUp);
                    drawSetting(ref importSettings.mergeFinalLevel, template.mergeFinalLevel);
                    drawSetting(ref importSettings.treeProcess, template.treeProcess);
                }, "Operations affecting transformations (coordinate system, scale, hierarchy, ..).");
            }

            /// Quality Category
            if (template.splitTo16BytesIndex.status != ParameterAvailability.Hidden
             || template.orient.status != ParameterAvailability.Hidden
             || template.singularizeSymmetries.status != ParameterAvailability.Hidden
             || template.hasLODs.status != ParameterAvailability.Hidden
             || template.voxelizeGridSize.status != ParameterAvailability.Hidden
             || template.qualities.status != ParameterAvailability.Hidden) {
                EditorGUIExtensions.DrawSubCategory(Style, "GEOMETRY", () => {
                    drawSetting(ref importSettings.splitTo16BytesIndex, template.splitTo16BytesIndex);
                    drawSetting(ref importSettings.repair, template.repair);
                    drawSetting(ref importSettings.orient, template.orient);
                    drawSetting(ref importSettings.singularizeSymmetries, template.singularizeSymmetries);
                    if (IsDrawingForPointCloud) {
                        importSettings.voxelizeGridSize = EditorGUILayout.IntSlider(new GUIContent("Segmentation", "The number of voxels in one dimension to use for the segmentation. A higher value means more objects, which can help increasing performances for large point clouds (mainly for frustrum culling)."), importSettings.voxelizeGridSize, 1, 20);
                        drawSetting(ref importSettings.hasLODs, template.hasLODs);
                        if (importSettings.hasLODs) {
                            drawSetting(ref importSettings.lodsMode, template.lodsMode);
                            importSettings.lodCount = EditorGUILayout.IntSlider(new GUIContent("Number of LODs", "The number of LODs to compute. The LODs for point clouds are automatically generated to ensure that every LOD has about 50% of the point count of the previous LOD. This allows the best memory / performance tradoff and ensures a linear memory cost for LODs"), importSettings.lodCount, 2, 6);
                        } else {
                            MeshQuality meshQuality = (MeshQuality)(int)importSettings.qualities.quality.quality;
                            drawSetting(ref meshQuality, template.quality);
                            var lodSettings = new LodGenerationSettings();
                            lodSettings.quality = (LodQuality)(int)meshQuality;
                            lodSettings.threshold = importSettings.qualities.quality.threshold;
                            var qualities = importSettings.qualities;
                            qualities.quality = lodSettings;
                            importSettings.qualities = qualities;
                        }
                    } else {
                        drawSetting(ref importSettings.hasLODs, template.hasLODs);
                        if ((template.hasLODs.status == ParameterAvailability.Available) ? importSettings.hasLODs : template.hasLODs.defaultValue)
                        {
                            // Unlock GUI
                            if (importSettings.locked)
                                GUI.enabled = guiEnabled;
                            // Not pertinent if there is a single GameObject (if Merge All)
                            if (importSettings.treeProcess != TreeProcessType.MERGE_ALL)
                                drawSetting(ref importSettings.lodsMode, template.lodsMode);
                            // Draw LODs bar
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("qualities"), new GUIContent(template.qualities.name, template.qualities.tooltip));
                            // Re-lock GUI if gui should be locked
                            if (importSettings.locked)
                                GUI.enabled = guiEnabled && !importSettings.locked;
                        }
                        else
                        {
                            MeshQuality meshQuality = (MeshQuality)(int)importSettings.qualities.quality.quality;
                            drawSetting(ref meshQuality, template.quality);
                            var lodSettings = new LodGenerationSettings();
                            lodSettings.quality = (LodQuality)(int)meshQuality;
                            lodSettings.threshold = importSettings.qualities.quality.threshold;
                            var qualities = importSettings.qualities;
                            qualities.quality = lodSettings;
                            importSettings.qualities = qualities;
                        }
                    }
                }, "Options affecting the geometry (mesh structure, quality, face orientation, ...)");
            }

            /// Rendering Category
            if (template.mapUV.status != ParameterAvailability.Hidden
             || template.createLightmapUV.status != ParameterAvailability.Hidden
             || template.useMaterialsInResources.status != ParameterAvailability.Hidden
             || template.shader.status != ParameterAvailability.Hidden) {
                EditorGUIExtensions.DrawSubCategory(Style, "RENDERING", () => {
                    if (drawSetting(ref importSettings.mapUV, template.mapUV) && importSettings.mapUV) {
                        drawSetting(ref importSettings.mapUV3dSize, template.mapUV3dSize, 1);
                    }
                    if (drawSetting(ref importSettings.createLightmapUV, template.createLightmapUV) && importSettings.createLightmapUV) {
                        drawSetting(ref importSettings.lightmapResolution, template.lightmapResolution, 1);
                        drawSetting(ref importSettings.uvPadding, template.uvPadding, 1);
                        EditorGUILayout.HelpBox("This process might take a long time depending on the complexity of the imported model.", MessageType.Warning);
                    }
                    drawSetting(ref importSettings.useMaterialsInResources, template.useMaterialsInResources);
                    drawSetting(ref importSettings.shader, template.shader);
                }, "Option affeting the rendering (materials, shaders, uvs, lighting, ...)");
            }

            EditorGUILayout.EndVertical();

            isChangedInLastFrame = EditorGUI.EndChangeCheck();

            if (isChangedInLastFrame || EditorGUIExtensions.Dirty) {
                EditorGUIExtensions.Dirty = false;
                /// The import settings have been changed :
                /// We mark it dirty to make sure it get serialized fully. Not always required but safer.
                EditorUtility.SetDirty(importSettings);
                /// Then we "Apply" modified properties of the serializableObject
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                /// Then we propagate the change to other potentially opened editors by triggering ImportSetting's changed event.
                importSettings.invokeChanged();
            }
            GUI.enabled = guiEnabled;
        }
    }
}