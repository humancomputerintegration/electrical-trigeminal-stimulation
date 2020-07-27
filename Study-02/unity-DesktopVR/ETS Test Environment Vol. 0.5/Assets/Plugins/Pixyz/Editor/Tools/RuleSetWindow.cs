using UnityEngine;
using UnityEditor;
using Pixyz.Tools;
using UnityEditor.Callbacks;
using Pixyz.Editor;

namespace Pixyz.Tools.Editor {

    /// <summary>
    /// A window to show a RuleSet as a Window. Useful if some selection has to be done while the RuleSet is visible
    /// </summary>
    public class RuleSetWindow : EditorWindow {

        public RuleSet rulesSet;

        void OnEnable() {
            minSize = new Vector2(400, 400);
            maxSize = new Vector2(1000, 3000);
        }

        void OnGUI() {
            EditorGUILayout.BeginVertical(new GUIStyle { margin = new RectOffset(16, 8, 0, 0) });
            RuleSetEditor editor = rulesSet.GetEditor<RuleSetEditor>();
            editor.OnInspectorGUI();
            if (editor != rulesEditor) {
                rulesEditor = editor;
                rulesEditor.changed += rulesChanged;
            }
            EditorGUILayout.EndVertical();
        }

        private RuleSetEditor rulesEditor;

        private void rulesChanged() {
            Repaint();
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line) {
            if (Selection.activeObject is RuleSet) {
                var window = (RuleSetWindow)EditorExtensions.GetEditorWindow(typeof(RuleSetWindow), true, Selection.activeObject.name);
                window.rulesSet = Selection.activeObject as RuleSet;
                return true; // Catch open file
            }

            return false; // Let unity open the file
        }
    }

    public class RenamePopup : PopupWindowContent
    {

        IRenamable renamable;
        bool initialized = false;

        public override Vector2 GetWindowSize()
        {
            return new Vector2(180, 20);
        }

        public override void OnGUI(Rect rect)
        {
            GUI.SetNextControlName("textField");
            renamable.name = EditorGUILayout.TextField(renamable.name);
            if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape) {
                editorWindow.Close();
            }
            if (!initialized) {
                GUI.FocusControl("textField");
                initialized = true;
            }
        }

        public override void OnOpen()
        {
        }

        public override void OnClose()
        {
        }

        public RenamePopup(IRenamable renamable)
        {
            this.renamable = renamable;
        }
    }
}