using System;
using UnityEngine;

namespace Pixyz.Utils {

    /// <summary>
    /// Delegate to an Import ended callback.
    /// Use @link Pixyz.EditorExtensions.GetVolatileDependencies @endlink to get all runtime dependencies (Editor Only).
    /// </summary>
    /// <param name="importedObject">Imported GameObject</param>
    public delegate void GameObjectToVoidHandler(GameObject importedObject);

    /// <summary>
    /// Delegate to a progress.
    /// </summary>
    /// <param name="progress">Progress value, from 0 to 1</param>
    /// <param name="message">Progress message</param>
    public delegate void ProgressHandler(float progress, string message);

    public delegate void VoidHandler();

    public delegate UnityEngine.Object UnityObjectToUnityObjectHandler(UnityEngine.Object unityObject);

    public delegate string UnityObjectToStringHandler(UnityEngine.Object asset);

    public delegate string StringToStringHandler(string path);

    public delegate UnityEngine.Object TypePathToUnityObject(string path, Type type);
}
