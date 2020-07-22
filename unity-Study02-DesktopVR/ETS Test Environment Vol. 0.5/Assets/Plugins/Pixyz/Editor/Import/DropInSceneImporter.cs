using Pixyz.Editor;
using Pixyz.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Import.Editor {

    [InitializeOnLoad]
    public static class DropInSceneImporter {

        private static bool _Dragging = false;

        static DropInSceneImporter() {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += (s) => OnSceneDragAndDrop();
#else
            SceneView.onSceneGUIDelegate += (s) => OnSceneDragAndDrop();
#endif
        }

        private static void OnSceneDragAndDrop() {

            if (!Preferences.FileDragAndDropInScene)
                return;

            Handles.BeginGUI();

            Event evt = Event.current;
            Rect frect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            Rect drop_area = new Rect(-1, -1, frect.width + 1, frect.height + 1);

            if (_Dragging) {
                GUI.Box(drop_area, "", StaticStyles.DragDropFrame);
            }

            switch (evt.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition)) {
                        _Dragging = false;
                        return;
                    }

                    var files = GetDragging3DFiles();
                    if (files.Count == 0) {
                        _Dragging = false;
                        return;
                    }

                    _Dragging = true;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag();

                        ImportWindow.Open(files[0]);

                        _Dragging = false;
                    }
                    break;
                case EventType.DragExited:
                    _Dragging = false;
                    break;
            }

            Handles.EndGUI();
        }

        private static IList<string> GetDragging3DFiles() {
            if (DragAndDrop.paths.Length == 0) {
                return new string[0];
            }
            List<string> supportedFiles = new List<string>();
            foreach (string file in DragAndDrop.paths) {
                if (Path.IsPathRooted(file) && Formats.IsFileSupported(file)) {
                    supportedFiles.Add(file);
                }
            }
            return supportedFiles;
        }
    }
}