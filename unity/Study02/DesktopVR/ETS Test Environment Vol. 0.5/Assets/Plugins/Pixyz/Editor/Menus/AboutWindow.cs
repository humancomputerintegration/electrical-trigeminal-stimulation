using UnityEngine;
using System;

using UnityEditor;
using Pixyz.Interface;
using Pixyz.Config;
using Pixyz.Plugin4Unity;

namespace Pixyz.Editor
{
    public class AboutWindow : SingletonEditorWindow
    {
        public override string WindowTitle => "About Pixyz Plugin for Unity";

        const int SIZE = 512;

        void OnEnable()
        {
            minSize = new Vector2(SIZE, SIZE);
            maxSize = new Vector2(SIZE, SIZE);
        }

        private void OnGUI()
        {
            Color color = EditorStyles.label.normal.textColor;
            EditorStyles.label.normal.textColor = Color.white;
            EditorStyles.miniLabel.normal.textColor = Color.white;
            GUI.DrawTexture(new Rect(0, 0, SIZE, SIZE), TextureCache.GetTexture("splashscreen"));

            GUILayout.BeginArea(new Rect(50, 160, SIZE - 50 * 2, 350));
            ShowLicenseInfos();
            GUILayout.EndArea();

            EditorGUI.LabelField(new Rect(420, 26, 84, 30), Configuration.CustomVersionTag);
            EditorGUI.LabelField(new Rect(420, 110, 84, 30), Configuration.PixyzVersion);

            GUILayout.BeginArea(new Rect(54, SIZE - 80, SIZE - 54 * 2, 100));
            EditorGUIExtensions.DrawHyperlink("Terms & Conditions", Configuration.WebsiteURL + "/general-and-products-terms-and-conditions/", centered: true);
            EditorGUILayout.LabelField("Pixyz solutions are edited by Metaverse Technologies France", new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter });
            GUILayout.EndArea();

            EditorStyles.label.normal.textColor = color;
        }

        public static void ShowLicenseInfos(bool boxes = true)
        {
            EditorGUIUtility.labelWidth = 110;

            bool hasLicense = false;

            if (NativeInterface.IsFloatingLicense()) {
                EditorGUILayout.BeginVertical("helpBox");
                EditorGUILayout.LabelField("License Type", "Floating");
                EditorGUILayout.LabelField("Server Host", Configuration.CurrentLicenseServer.serverAddress);
                EditorGUILayout.LabelField("Server Port", Configuration.CurrentLicenseServer.serverPort.ToString());
                EditorGUILayout.EndVertical();
                hasLicense = true;
            } else {
                var currentNodeLockedLicense = NativeInterface.GetCurrentLicenseInfos();
                bool hasNodeLockedLicense = currentNodeLockedLicense.endDate.year != -1;
                if (hasNodeLockedLicense) {
                    EditorStyles.label.richText = true;
                    EditorGUILayout.BeginVertical("helpBox");
                    EditorGUILayout.LabelField("Start Date", currentNodeLockedLicense.startDate.ToUnityObject().ToString("yyyy-MM-dd"));
                    EditorGUILayout.LabelField("End Date", currentNodeLockedLicense.endDate.ToEndDateRichText());
                    EditorGUILayout.LabelField("Company Name", currentNodeLockedLicense.customerCompany);
                    EditorGUILayout.LabelField("User Name", currentNodeLockedLicense.customerName);
                    EditorGUILayout.LabelField("User Email", currentNodeLockedLicense.customerEmail);
                    EditorGUILayout.EndVertical();
                    hasLicense = true;
                }
            }

            if (Configuration.CheckLicense()) {
                // It's all good !
            } else {
                // It's not good for some reason
                if (hasLicense && Configuration.CustomVersionTag == "BETA_VERSION" && !NativeInterface.IsTokenValid("Beta")) {
                    EditorGUILayout.BeginVertical("helpBox");
                    EditorGUILayout.LabelField("WARNING", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("A license with a 'Beta' token is required to run this beta version of the Pixyz Plugin for Unity", EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndVertical();
                } else {
                    EditorGUILayout.BeginVertical("helpBox");
                    EditorGUILayout.LabelField("There is no valid license installed.");
                    if (!string.IsNullOrEmpty(Configuration.GetLastError()))
                        EditorGUILayout.LabelField("Error : " + Configuration.GetLastError(), new GUIStyle(EditorStyles.miniLabel) { wordWrap = true });
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("• If you have a license, please install it using the License Manager");
                    EditorGUILayout.LabelField("• If you need a license, please visit :");
                    EditorGUIExtensions.DrawHyperlink(Configuration.WebsiteURL + "/plugin", Configuration.WebsiteURL + "/plugin/#pricing", centered: true);
                    EditorGUILayout.EndVertical();
                }
            }
        }
    }
}