using Pixyz.Editor;
using Pixyz.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Tools.Editor {

    public class ToolboxWindow : EditorWindow {

        private static ActionBase lastAction;

        const int TRANSITION_SIZE = 36;
        const int HEADER_HEIGHT = 30;

        private int actionId;

        private ActionBase _action;
        public ActionBase action {
            get {
                if (_action == null)
                    action = ToolsBase.GetRegisteredAction(actionId);
                return _action;
            }
            set {
                _action = value;
                if (_action != null)
                    actionId = _action.id;
                _hasProcessed = false; 
            }
        }

        public static void ShowToolbox(ActionBase action) {
            var window = (ToolboxWindow)EditorExtensions.GetEditorWindow(typeof(ToolboxWindow), true, "Toolbox");
            lastAction = (lastAction != null && lastAction.id == action.id) ? lastAction : action;
            window.action = lastAction;
            window.ShowUtility();
        }

        private static bool _initialized = false;
        private ColoredTheme _style;

        private static bool _IncludeChildren = true;

        void OnEnable() {
            _style = new ColoredTheme(new Color(1f, 1f, 1f, 0.5f));
            Selection.selectionChanged += ()=>{ statsBeforeProcess = null;};
        }

        private GUIStyle _marginStyle;
        public GUIStyle marginStyle {
            get {
                if (_marginStyle == null) {
                    _marginStyle = new GUIStyle();
                    _marginStyle.padding = new RectOffset(28, 28, 10, 10);
                }
                return _marginStyle;
            }
        }

        private StatsReport statsBeforeProcess;
        private StatsReport statsAfterProcess;

        void OnGUI() {

            EditorGUIUtility.labelWidth = 180;

            if (action == null) {
                Close();
                return;
            }

            titleContent = new GUIContent(action?.displayNameToolbox, action?.tooltip);

            Rect rect = EditorGUILayout.BeginVertical();

            GUILayout.Space(10);

            // DRAW BACKGROUND PIXYZ BACKGROUND
            GUI.DrawTexture(new Rect(position.width - 430, position.height - 430, 500, 500), TextureCache.GetTexture("IconPixyzWatermark"));

            EditorGUI.BeginDisabledGroup(_hasProcessed);

            Rect rect1 = EditorGUILayout.BeginVertical(_style.ruleBlock);
            {
                GUILayout.Label(new GUIContent(_hasProcessed ? "Before Process" : "Selection"), _style.toolboxBlockTitle);
                EditorGUILayout.BeginVertical();
                {
                    if (!_hasProcessed && statsBeforeProcess == null)
                        statsBeforeProcess = getStats(getSelectedGameObjects());
                    EditorGUI.BeginChangeCheck();
                    _IncludeChildren = EditorGUILayout.Toggle("Include Children", _IncludeChildren);
                    if (EditorGUI.EndChangeCheck()) {
                        statsBeforeProcess = getStats(getSelectedGameObjects());
                    }
                    GUILayout.Space(4);
                    drawStats(statsBeforeProcess);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            /// Transition
            GUI.DrawTexture(new Rect(35, rect1.y + rect1.height - 4, TRANSITION_SIZE, TRANSITION_SIZE), (false) ? _style.iconDownFluxMid : _style.iconDownFlux);
            GUI.Label(new Rect(70, rect1.y + rect1.height + (TRANSITION_SIZE - 20) / 2 - 2, TRANSITION_SIZE, 20), "GameObjects", _style.downFluxTitle);
            GUILayout.Space(TRANSITION_SIZE - 8);

            Rect rect2 = EditorGUILayout.BeginVertical(_style.ruleBlock);
            {
                GUILayout.Label(titleContent, _style.toolboxBlockTitle);
                EditorGUILayout.BeginVertical();
                {
                    action.onBeforeDraw();
                    action.fieldInstances.DrawGUILayout();
                    action.onAfterDraw();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            /// Transition
            GUI.DrawTexture(new Rect(35, rect2.y + rect2.height - 4, TRANSITION_SIZE, TRANSITION_SIZE), (false) ? _style.iconDownFluxMid : _style.iconDownFlux);
            GUI.Label(new Rect(70, rect2.y + rect2.height + (TRANSITION_SIZE - 20) / 2 - 2, TRANSITION_SIZE, 20), "GameObjects", _style.downFluxTitle);
            GUILayout.Space(TRANSITION_SIZE - 8);

            EditorGUI.EndDisabledGroup();

            Rect rect3 = EditorGUILayout.BeginVertical(_style.ruleBlock);
            {
                GUILayout.Label(new GUIContent(_hasProcessed ? "After Process" : "Process"), _style.toolboxBlockTitle);
                EditorGUILayout.BeginVertical();
                {
                    if (_hasProcessed) {
                        drawStats(statsAfterProcess, statsBeforeProcess);
                    } else {
                        var errors = action.getErrors();

                        bool enabled = true;

                        if (errors != null && errors.Count > 0) {
                            EditorGUILayout.HelpBox(String.Join("\n", errors), MessageType.Error);
                            enabled = false;
                        }

                        if (Selection.gameObjects.Length == 0) {
                            EditorGUILayout.HelpBox("At least one GameObject must be selected", MessageType.Error);
                            enabled = false;
                        }

                        EditorGUI.BeginDisabledGroup(!enabled);
                        if (GUILayout.Button("Execute")) {
                            Dispatcher.StartCoroutine(Execute());
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            if (rect.height > 20 && _lastHeight != rect.height) {
                _lastHeight = rect.height;

                Rect pos = position;
                pos.width = 360;
                pos.height = rect.height + 10;
                position = pos;
                minSize = new Vector2(pos.width, pos.height);
                maxSize = new Vector2(pos.width, pos.height);

                if (!_initialized && Event.current.type == EventType.Repaint) {
                    _initialized = true;
                    //var ms = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    //pos.x = ms.x;
                    //pos.y = ms.y;
                    EditorExtensions.CenterOnEditor(this);
                }
            }
        }

        private StatsReport getStats(IList<GameObject> gameObjects) {

            if (gameObjects == null)
                return new StatsReport();

            int triangles = 0;
            int vertices = 0;
            int renderers = 0;

            var meshes = new HashSet<Mesh>();
            var materials = new HashSet<Material>();
            for (int i = 0; i < gameObjects.Count; i++) {
                if (!gameObjects[i])
                    continue;
                MeshFilter meshFilter = gameObjects[i].GetComponent<MeshFilter>();
                if (!meshFilter)
                    continue;
                    Mesh mesh = meshFilter.sharedMesh;
                if (!mesh)
                    continue;
                meshes.Add(mesh);
                triangles += mesh.GetPolycount();
                vertices += mesh.vertices.Length;
                Renderer renderer = gameObjects[i].GetComponent<Renderer>();
                if (renderer) {
                    renderers++;
                    foreach (Material material in renderer.sharedMaterials) {
                        if (!material)
                            continue;
                        materials.Add(material);
                    }
                }
            }

            return new StatsReport(
                gameObjects.Count,
                materials.Count,
                meshes.Count,
                renderers,
                triangles,
                vertices
            );
        }

        private void drawStats(StatsReport stats, StatsReport before = null) {
            if (stats == null)
                return;
            EditorGUILayout.LabelField("GameObjects", getFormattedValue(stats.gameObjects, (before != null) ? before.gameObjects : -1));
            GUILayout.Space(4);
            EditorGUILayout.LabelField("Materials", getFormattedValue(stats.materials, (before != null) ? before.materials : -1));
            GUILayout.Space(4);
            EditorGUILayout.LabelField("Shared Meshes", getFormattedValue(stats.uniqueMeshes, (before != null) ? before.uniqueMeshes : -1));
            GUILayout.Space(4);
            EditorGUILayout.LabelField("Renderers", getFormattedValue(stats.renderers, (before != null) ? before.renderers : -1));
            GUILayout.Space(4);
            EditorGUILayout.LabelField("Triangles", getFormattedValue(stats.triangles, (before != null) ? before.triangles : -1));
            GUILayout.Space(4);
            EditorGUILayout.LabelField("Vertices", getFormattedValue(stats.vertices, (before != null) ? before.vertices : -1));
        }

        private string getFormattedValue(int current, int before) {
            string value = current.ToString("# ### ### ##0").Trim();
            if (before != -1) {
                float percent = 100.0f * (current - before) / before;
                if (percent != 0 && !float.IsInfinity(percent) && !float.IsNaN(percent))
                    value += " (" + percent.ToString("+0.##;-0.##;0") + "%)";
            }
            return value;
        }

        private IList<GameObject> getSelectedGameObjects() {
            return _IncludeChildren ? Selection.gameObjects.GetChildren(true, true) : Selection.gameObjects;
        }

        float _lastHeight;
        bool _hasProcessed = false;

        private IEnumerator Execute() {

            _hasProcessed = true;
            Repaint();

            if (Selection.gameObjects.Length == 0)
                yield break;

            yield return Dispatcher.GoMainThread();

            IList<GameObject> input = getSelectedGameObjects();

            foreach (GameObject go in input) {
                Undo.RegisterFullObjectHierarchyUndo(go, action.displayNameToolbox);
            }

            try {
                EditorUtility.DisplayProgressBar("Pixyz", $"Processing...", 0);
                Profiling.Start("ToolboxAction");
                object result = action.invoke(input);
                statsAfterProcess = getStats(((IList<GameObject>)result)?.ToArray());
                var timespan = Profiling.End("ToolboxAction");
                result = $"Pixyz Toolbox > {action.displayNameToolbox} done in {timespan.FormatNicely()}";
                if (Preferences.LogTimeWithToolbox) {
                    BaseExtensions.LogColor(Color.green, result);
                }
            } catch (Exception ex) {
                Debug.LogException(ex);
            }
            EditorUtility.ClearProgressBar();

            Undo.FlushUndoRecordObjects();
            Undo.SetCurrentGroupName(action.displayNameToolbox + " (Pixyz)");
        }
    }

    internal class StatsReport {
        public readonly int gameObjects;
        public readonly int materials;
        public readonly int uniqueMeshes;
        public readonly int renderers;
        public readonly int triangles;
        public readonly int vertices;
        public StatsReport(int gameObjects, int materials, int uniqueMeshes, int renderers, int triangles, int vertices) {
            this.gameObjects = gameObjects;
            this.materials = materials;
            this.uniqueMeshes = uniqueMeshes;
            this.renderers = renderers;
            this.triangles = triangles;
            this.vertices = vertices;
        }
        public StatsReport() { }
    }
}

