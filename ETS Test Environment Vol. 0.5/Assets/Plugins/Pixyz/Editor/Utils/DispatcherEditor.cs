using Pixyz.Utils;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Editor {

    /// <summary>
    /// This class connects to the Editor frame update to trigger MonoContext updates when not in play mode.
    /// </summary>
    [InitializeOnLoad]
    public static class DispatcherEditor {

        static DispatcherEditor() {
            EditorApplication.update += Dispatcher.Update;
        }
    }
}
