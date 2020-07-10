using Pixyz.Editor;
using Pixyz.Utils;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Import.Editor {

    [CustomPropertyDrawer(typeof(LodsGenerationSettings))]
    public class LODsSettingsDrawer : PropertyDrawer {

        const int SPLITTER_WIDTH = 12;
        const float MINIMUM_LOD_RANGE = 0.01f;

        public static readonly Color[] LOD_COLORS_FOCUS = new Color[] {
            new Color(0.38039f, 0.49020f, 0.01961f),
            new Color(0.21961f, 0.32157f, 0.45882f),
            new Color(0.16471f, 0.41961f, 0.51765f),
            new Color(0.41961f, 0.12549f, 0.01961f),
            new Color(0.30196f, 0.22745f, 0.41569f),
            new Color(0.63137f, 0.34902f, 0.00000f),
            new Color(0.35294f, 0.32157f, 0.03922f),
            new Color(0.61176f, 0.50196f, 0.01961f),
        };

        // Todo : Light theme colors are different
        public static readonly Color[] LOD_COLORS = new Color[] {
            new Color(0.23529f, 0.27451f, 0.10196f),
            new Color(0.18039f, 0.21569f, 0.26275f),
            new Color(0.15686f, 0.25098f, 0.28627f),
            new Color(0.25098f, 0.14510f, 0.10588f),
            new Color(0.20784f, 0.18039f, 0.24706f),
            new Color(0.32549f, 0.22745f, 0.09804f),
            new Color(0.22745f, 0.21569f, 0.11373f),
            new Color(0.32157f, 0.27843f, 0.10588f),
        };

        public static readonly Color CULLED_COLOR = new Color(0.31373f, 0f, 0f);
        public static readonly Color CULLED_COLOR_FOCUS = new Color(0.62745f, 0f, 0f);
        public static readonly Color FRAME_COLOR_FOCUS = new Color(0.23922f, 0.37647f, 0.56863f);

        private int selectedLodIndexPending = 0;
        private int selectedLodIndex = 0;
        private int grabbing = -1;

        private bool isLodSelected => selectedLodIndex > -1;
        private bool isLodGrabbed => grabbing > -1;

        public static Color GetLodColor(int lodNbr, bool isCulled, bool isSelected)
        {
            return isCulled ? (isSelected ? CULLED_COLOR_FOCUS : CULLED_COLOR) : (isSelected ? LOD_COLORS_FOCUS[lodNbr] : LOD_COLORS[lodNbr]);
        }

        public static GUIStyle _LodPercentTextStyle;
        public static GUIStyle LodPercentTextStyle {
            get {
                if (_LodPercentTextStyle == null) {
                    _LodPercentTextStyle = new GUIStyle();
                    _LodPercentTextStyle.alignment = TextAnchor.MiddleRight;
                    _LodPercentTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f) : new Color(.1f, .1f, .1f);
                }
                return _LodPercentTextStyle;
            }
        }

        private string[] fancyNames;
        private string[] FancyNames => fancyNames ?? (fancyNames = Enum.GetValues(typeof(LodQuality)).Cast<object>().Select(o => o.ToString().FancifyCamelCase()).ToArray());

        private float scale(float value)
        {
            return Mathf.Pow(value, 0.5f);
        }

        private float descale(float value)
        {
            return Mathf.Pow(value, 2);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (Event.current.type == EventType.Layout) {
                selectedLodIndex = selectedLodIndexPending;
            }

            var lodsArray = property.FindPropertyRelative("_lods");
            int count = lodsArray.arraySize;
            bool locked = property.FindPropertyRelative("_locked").boolValue;
            bool automatic = ImportSettingsEditor.IsDrawingForPointCloud;

            if (count > 0) {

                Rect sliderRect = EditorGUILayout.GetControlRect();
                sliderRect.y -= 2;
                sliderRect.height = 30;
                GUILayout.Space(20);

                float previousThreshold = 1f;
                float[] widths = new float[count];

                for (int i = 0; i < count; i++) {

                    bool isLast = i == count - 1;
                    float currentThreshold = (float)lodsArray.GetArrayElementAtIndex(i).FindPropertyRelative("threshold").doubleValue;
                    int quality = lodsArray.GetArrayElementAtIndex(i).FindPropertyRelative("quality").enumValueIndex;

                    widths[i] = scale(previousThreshold) - scale(currentThreshold);

                    // Draw Block
                    Rect labelRect = new Rect(
                        new Vector2(sliderRect.position.x + (1 - scale(previousThreshold)) * sliderRect.width, sliderRect.position.y),
                        new Vector2(sliderRect.width * widths[i], sliderRect.height)
                    );

                    GUIContent title = new GUIContent($" LOD {i}\n " + (automatic ? (quality == 5 ? "Culled" : "Auto") : FancyNames[quality]));
                    title.tooltip = "Right click to insert or remove an LOD";

                    EditorGUIExtensions.GUIDrawRect(labelRect, GetLodColor(i, quality == (int)LodQuality.CULLED, selectedLodIndex == i),
                        FRAME_COLOR_FOCUS, selectedLodIndex == i ? 3 : 0, title, TextAnchor.MiddleLeft);

                    // Check if click on LOD
                    EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.Link);
                    if (labelRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown)
                        {
                            if (Event.current.button == 0)
                            {
                                selectedLodIndexPending = i;
                                // Triggers change (for Repaint in Editors)
                                setDirty();
                            }
                            else if (Event.current.button == 1 && !locked)
                            {
                                selectedLodIndexPending = i;
                                GenericMenu genericMenu = new GenericMenu();

                                if (count <= (int)LodQuality.CULLED) {
                                    genericMenu.AddItem(new GUIContent("Add"), false, () => {
                                        insertLOD(lodsArray, selectedLodIndexPending);
                                        setDirty();
                                    });
                                } else {
                                    genericMenu.AddDisabledItem(new GUIContent("Add"));
                                }

                                if (count > 1) {
                                    genericMenu.AddItem(new GUIContent("Remove"), false, () => {
                                        deleteLOD(lodsArray, selectedLodIndexPending);
                                        setDirty();
                                    });
                                } else {
                                    genericMenu.AddDisabledItem(new GUIContent("Remove"));
                                }

                                genericMenu.ShowAsContext();
                            }
                        }
                    }

                    // Draw Splitter if not last
                    if (!isLast) {
                        Rect splitter = new Rect(labelRect.x + labelRect.width, labelRect.y, SPLITTER_WIDTH, labelRect.height);
                        EditorGUI.LabelField(new Rect(splitter.x - 20, splitter.y - 20, 40, 20), (Math.Round(currentThreshold * 100)) + "%", LodPercentTextStyle);
                        EditorGUIUtility.AddCursorRect(splitter, MouseCursor.ResizeHorizontal);
                        if (splitter.Contains(Event.current.mousePosition) && (Event.current.type == EventType.MouseDown && Event.current.button == 0)) {
                            if (i < (int)LodQuality.CULLED)
                                grabbing = i;
                        }
                    }

                    previousThreshold = currentThreshold;
                }

                if (Event.current.type == EventType.MouseUp)
                    grabbing = int.MinValue;

                float mouseDeltaX = 0;
                if (grabbing != int.MinValue && Event.current.type == EventType.MouseDrag) {
                    mouseDeltaX = Event.current.delta.x;
                }

                if (mouseDeltaX != 0)
                {
                    float threshold = (float)lodsArray.GetArrayElementAtIndex(grabbing).FindPropertyRelative("threshold").doubleValue;
                    float delta = -mouseDeltaX / sliderRect.width;

                    // Moves dragging LOD
                    float max = (grabbing > 0) ? (float)lodsArray.GetArrayElementAtIndex(grabbing - 1).FindPropertyRelative("threshold").doubleValue - MINIMUM_LOD_RANGE : 1 - MINIMUM_LOD_RANGE;
                    float min = (grabbing < count) ? (float)lodsArray.GetArrayElementAtIndex(grabbing + 1).FindPropertyRelative("threshold").doubleValue + MINIMUM_LOD_RANGE : MINIMUM_LOD_RANGE;
                    float newThreshold = descale(scale(threshold) + delta);
                    newThreshold = Mathf.Clamp(newThreshold, min, max);
                    lodsArray.GetArrayElementAtIndex(grabbing).FindPropertyRelative("threshold").doubleValue = newThreshold;

                    // Triggers change (for Repaint in Editors)
                    setDirty();
                }

                // Selected LOD Quality dropdown selector
                if (!locked && isLodSelected)
                {
                    int selectedLodQuality = lodsArray.GetArrayElementAtIndex(selectedLodIndex).FindPropertyRelative("quality").enumValueIndex;

                    // We allow a specific set of options.
                    // We can set a different quality as long as it preserves the order of qualities
                    string[] options = FancyNames.Slice(selectedLodIndex, FancyNames.Length - count + selectedLodIndex + 1);

                    int selectedOption = selectedLodQuality - selectedLodIndex;
                    int newOption = selectedOption;

                    if (automatic) {
                        if (selectedLodIndex == count - 1) {
                            bool wasCulled = selectedLodQuality == (int)LodQuality.CULLED;
                            EditorGUI.BeginDisabledGroup(options.Length == 1);
                            bool isCulled = EditorGUILayout.Toggle(new GUIContent("Culled", "If selected, the point cloud will be culled (not visible) for this LOD level"), wasCulled);
                            EditorGUI.EndDisabledGroup();
                            if (isCulled != wasCulled) {
                                newOption = (isCulled ? (int)LodQuality.CULLED : (int)LodQuality.POOR) - selectedLodIndex;
                            }
                        } else {
                            EditorGUILayout.HelpBox("Quality for LODs on point clouds is calculated automatically for smooth transitions.", MessageType.Info);
                        }
                    } else {
                        newOption = EditorGUILayout.Popup(new GUIContent("Selected LOD Quality", ImportSettingsTemplate.Default.quality.tooltip), selectedOption, options);
                    }

                    // If a new quality from the option was selected, we update the value
                    if (selectedOption != newOption) {
                        int newLodQuality = newOption + selectedLodIndex;
                        lodsArray.GetArrayElementAtIndex(selectedLodIndex).FindPropertyRelative("quality").enumValueIndex = newLodQuality;
                        rearrangeQualities(lodsArray, selectedLodIndex);
                    }
                }
            }

            // Ensures LOD chain is coherent
            int currentLOD = 0;
            while (currentLOD < count - 1) {
                var leftLOD = lodsArray.GetArrayElementAtIndex(currentLOD);
                var rightLOD = lodsArray.GetArrayElementAtIndex(currentLOD + 1);
                int leftLODQuality = leftLOD.FindPropertyRelative("quality").enumValueIndex;
                int rightLODQuality = rightLOD.FindPropertyRelative("quality").enumValueIndex;
                if (leftLODQuality < rightLODQuality)
                    return;
                if (leftLODQuality + 1 > (int)LodQuality.CULLED) {
                    for (int i = currentLOD + 1; i < count; ++i)
                        lodsArray.DeleteArrayElementAtIndex(currentLOD + 1);
                    leftLOD.FindPropertyRelative("threshold").doubleValue = 0;
                    return;
                } else
                    rightLOD.FindPropertyRelative("quality").enumValueIndex = leftLODQuality + 1;
                currentLOD++;
            }

            EditorGUI.EndProperty();
        }

        private void setDirty()
        {
            EditorGUIExtensions.Dirty = true;
        }

        private void deleteLOD(SerializedProperty lodsArray, int index)
        {
            lodsArray.DeleteArrayElementAtIndex(index);

            selectedLodIndexPending--;
            selectedLodIndexPending = Mathf.Clamp(selectedLodIndexPending, 0, lodsArray.arraySize - 1);

            grabbing = int.MinValue;

            rearrangeThresholds(lodsArray);
        }

        private void insertLOD(SerializedProperty lodsArray, int index)
        {
            double before = index == 0 ? 1 : lodsArray.GetArrayElementAtIndex(index - 1).FindPropertyRelative("threshold").doubleValue;
            double after = lodsArray.GetArrayElementAtIndex(index).FindPropertyRelative("threshold").doubleValue;

            lodsArray.InsertArrayElementAtIndex(index);
            lodsArray.GetArrayElementAtIndex(index).FindPropertyRelative("threshold").doubleValue = (after + before) / 2;

            selectedLodIndexPending++;

            rearrangeQualities(lodsArray, 0);
            rearrangeQualities(lodsArray, lodsArray.arraySize - 1);
            rearrangeThresholds(lodsArray);
        }

        /// <summary>
        /// Rearranges lod qualities so that it is continuous, from LOD0 with the highest quality and LODN the lowest
        /// </summary>
        /// <param name="lodsArray"></param>
        private void rearrangeQualities(SerializedProperty lodsArray, int startingIndex)
        {
            int count = lodsArray.arraySize;
            int previousQuality = 0;

            // Propagate to to LOD0
            for (int i = startingIndex; i >= 0; i--) {
                int currentQuality = lodsArray.GetArrayElementAtIndex(i).FindPropertyRelative("quality").enumValueIndex;
                if (i != startingIndex && currentQuality >= previousQuality && previousQuality > 0) {
                    currentQuality = lodsArray.GetArrayElementAtIndex(i).FindPropertyRelative("quality").enumValueIndex = previousQuality - 1;
                }
                previousQuality = currentQuality;
            }
            // Propagate to to LODN
            for (int i = startingIndex; i < count; i++) {
                int currentQuality = lodsArray.GetArrayElementAtIndex(i).FindPropertyRelative("quality").enumValueIndex;
                if (i != startingIndex && currentQuality <= previousQuality && previousQuality < FancyNames.Length - 1) {
                    currentQuality = lodsArray.GetArrayElementAtIndex(i).FindPropertyRelative("quality").enumValueIndex = previousQuality + 1;
                }
                previousQuality = currentQuality;
            }
        }

        /// <summary>
        /// Rearranges thresholds to that the order is kept and there is a minimum delta between two lods
        /// </summary>
        /// <param name="lodsArray"></param>
        private void rearrangeThresholds(SerializedProperty lodsArray)
        {
            for (int i = 0; i < lodsArray.arraySize; ++i) {
                if (i + 1 < lodsArray.arraySize) {
                    SerializedProperty leftLOD = lodsArray.GetArrayElementAtIndex(i);
                    SerializedProperty rightLOD = lodsArray.GetArrayElementAtIndex(i + 1);
                    if (leftLOD.FindPropertyRelative("threshold").doubleValue <= rightLOD.FindPropertyRelative("threshold").doubleValue + 0.027)
                        leftLOD.FindPropertyRelative("threshold").doubleValue = rightLOD.FindPropertyRelative("threshold").doubleValue + 0.05;
                }
                if (i == lodsArray.arraySize - 1) {
                    lodsArray.GetArrayElementAtIndex(i).FindPropertyRelative("threshold").doubleValue = 0;
                }
            }
        }
    }
}