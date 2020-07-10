using UnityEditor;

namespace Pixyz.Editor
{
    public abstract class SingletonEditorWindow : EditorWindow
    {
        public abstract string WindowTitle { get; }
    }
}