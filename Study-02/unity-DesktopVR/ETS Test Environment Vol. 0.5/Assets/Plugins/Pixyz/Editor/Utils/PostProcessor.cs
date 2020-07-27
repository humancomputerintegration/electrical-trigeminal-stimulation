using Pixyz.Editor;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Pixyz.Import.Editor {

    public sealed class PostProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport {

        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            try {
                switch (report.summary.platform) {
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        if (Preferences.IncludeForRuntimeWindows) {
                            Debug.LogWarning("<b>Pixyz</b> is currently <b>included</b> in builds. If you don't use the runtime import, please uncheck 'Windows Standalone' in Include section in Pixyz Preferences.\nThis will save space in build.");
                            CopyNativeDlls(report.summary.outputPath);
                        } else {
                            Debug.LogWarning("<b>Pixyz</b> is currently <b>excluded</b> from builds. To include in build for runtime import, please check 'Windows Standalone' in Include section in Pixyz Preferences.");
                        }
                        break;
                    default:
                        break;
                }
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// Copy Pixyz native DLLS to the standalone "_Data/Plugins" folder
        /// </summary>
        /// <param name="pathToBuiltProject"></param>
        private static void CopyNativeDlls(string pathToBuiltProject)
        {
            string buildName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
            string sourcePath = Path.Combine(Preferences.PluginLocationAbsolute, @"Binaries\native");
            string pluginFolderPath = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), buildName + @"_Data\Plugins");
            CopyDir(sourcePath, pluginFolderPath);
        }

        /// <summary>
        /// Force Unity not to copy Pixyz native dlls by himself (or it will do it wrong)
        /// The copy process should be done with the function CopyNativeDlls
        /// </summary>
        /// <param name="report"></param>
        public void OnPreprocessBuild(BuildReport report) {
            PluginImporter[] pluginImporters = PluginImporter.GetAllImporters();
            string binariesFolder = Preferences.PluginLocation + "/Binaries";
            foreach (PluginImporter pluginImporter in pluginImporters) {
                if (pluginImporter.isNativePlugin && pluginImporter.assetPath.Contains(binariesFolder)) {
                    pluginImporter.SetCompatibleWithAnyPlatform(false);
                    pluginImporter.SetCompatibleWithEditor(true);
                }
            }
        }

        private static void CopyDir(string sourcePath, string destinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }
    }
}