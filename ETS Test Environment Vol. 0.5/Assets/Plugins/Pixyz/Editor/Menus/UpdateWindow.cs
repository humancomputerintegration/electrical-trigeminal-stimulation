using UnityEngine;
using System;
using UnityEditor;
using Pixyz.Config;
using Pixyz.Utils;
using System.Collections;

namespace Pixyz.Editor {

    public class UpdateWindow : SingletonEditorWindow {

        public override string WindowTitle => "Pixyz Plugin Updater";

        private static bool automaticPopup = false;

        public static void AutoPopup()
        {
            Dispatcher.StartCoroutine(AutoPopupAsync());
        }

        private static IEnumerator AutoPopupAsync()
        {
            yield return Dispatcher.GoThreadPool();
            bool isNewVersionAvailable = false;
            try {
                isNewVersionAvailable = (Configuration.UpdateStatus ?? Configuration.CheckForUpdate()).newVersionAvailable;
            } catch {
                // Probably a timeout issue
                // In anycase, this is not an issue, so no error is forwarded, we just ignore the update checking
                yield break;
            }
            yield return Dispatcher.GoMainThread();

            if (isNewVersionAvailable) {
                automaticPopup = true;
                EditorExtensions.OpenWindow<UpdateWindow>();
            }
        }

        public void OnDisable() {
            automaticPopup = false;
        }

        void OnGUI() {

            Plugin4Unity.NativeInterface.checkForUpdatesReturn newVersionCheck = Configuration.UpdateStatus ?? Configuration.CheckForUpdate();

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();

                    if (newVersionCheck.newVersionAvailable)
                    {
                        EditorGUILayout.LabelField("A new version is available : " + newVersionCheck.newVersion, EditorStyles.wordWrappedLabel);
                        GUILayout.Space(20);

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Download"))
                        {
                            Application.OpenURL(newVersionCheck.newVersionLink);
                            this.Close();
                        }
                        if (GUILayout.Button("Later"))
                        {
                            this.Close();
                        }
                        GUILayout.EndHorizontal();
                        if (automaticPopup)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                Preferences.AutomaticUpdate = !EditorGUILayout.Toggle("Do not show Again", !Preferences.AutomaticUpdate);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else if (String.IsNullOrEmpty(Configuration.GetLastError()))
                    {
                        EditorGUILayout.LabelField("Your version is up to date", EditorStyles.wordWrappedLabel);
                        GUILayout.Space(20);
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Close"))
                        {
                            this.Close();
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.LabelField(Configuration.GetLastError(), EditorStyles.wordWrappedLabel);
                        GUILayout.Space(20);
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Close"))
                        {
                            this.Close();
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
    }
}