using Pixyz.Utils;
using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Editor {

    [InitializeOnLoad]
    public static class NonPersistentSerializer {

        static NonPersistentSerializer() {
            PrefabUtility.prefabInstanceUpdated += PrefabInstanceUpdated;
        }

        public static void PrefabInstanceUpdated(GameObject gameObject) {

            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            if (string.IsNullOrEmpty(path))
                return;

            var nonPersistentDependencies = gameObject.GetVolatileDependencies();
            if (nonPersistentDependencies.Length == 0)
                return;

            int itemsToShow = 6;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Pixyz has detected that following objects are not persistent.\nWould you like to make them persistent by saving them in the generated prefab ?\n\n");
            for (int i = 0; i < Mathf.Clamp(nonPersistentDependencies.Length, 0, itemsToShow); i++) {
                stringBuilder.Append("- " + nonPersistentDependencies[i] + "\n");
            }
            if (nonPersistentDependencies.Length > itemsToShow) {
                stringBuilder.Append($"and {nonPersistentDependencies.Length - itemsToShow} other objects\n");
            }
            if (!EditorUtility.DisplayDialog("Non persistent data", stringBuilder.ToString(), "Yes, save these assets", "No"))
                return;

            Dispatcher.StartCoroutine(SavePrefab(gameObject, path, nonPersistentDependencies));
        }

        private static IEnumerator SavePrefab(GameObject gameObject, string path, Object[] dependencies)
        {
            // Since Unity 2020.1, it is required to delay one frame or the prefab connection fails
            yield return Dispatcher.DelayFrames(1);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            switch (Preferences.PrefabDependenciesDestination) {
                case PrefabDependenciesDestination.InPrefab:
                    foreach (var dependency in dependencies) {
                        AssetDatabase.AddObjectToAsset(dependency, prefab);
                    }
                    break;
                case PrefabDependenciesDestination.InFolder:
                    string dir = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                    foreach (var dependency in dependencies) {
                        string depPath = dir + "/" + dependency.name;
                        dependency.CreateAsset(depPath);
                    }
                    break;
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path, InteractionMode.AutomatedAction);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}