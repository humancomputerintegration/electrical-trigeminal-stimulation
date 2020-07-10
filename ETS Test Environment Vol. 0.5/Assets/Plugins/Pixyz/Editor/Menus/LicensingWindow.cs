using UnityEngine;
using System;
using UnityEditor;
using Pixyz.Interface;
using Pixyz.Config;
using System.Linq;
using Pixyz.Plugin4Unity;
using System.Collections;
using System.Collections.Generic;

namespace Pixyz.Editor {

    /// <summary>
    /// Licensing Window.
    /// </summary>
    public class LicensingWindow : SingletonEditorWindow {

        public override string WindowTitle => "Pixyz License Manager";

        private string _username = "";
        private string _password = "";
        private int _selectedTab = 0;

        private bool _doRequest = false;
        private bool _doRelease = false;

        private Plugin4Unity.NativeInterface.WebLicenseInfo licenseToProcess;

        private string _address = null;
        private int _port = 64990;
        private bool _flexLM = false;
        private Vector2 _scrollViewPosition = new Vector2(0, 0);

        private void OnEnable() {
            minSize = new Vector2(590, 200);
            if (String.IsNullOrEmpty(_address) || _address == "127.0.0.1") {
                _address = (Configuration.CurrentLicenseServer != null) ? Configuration.CurrentLicenseServer.serverAddress : "127.0.0.1";
                _port = (Configuration.CurrentLicenseServer != null) ? Configuration.CurrentLicenseServer.serverPort : 64990;
                _flexLM = (Configuration.CurrentLicenseServer != null) ? Configuration.CurrentLicenseServer.useFlexLM : false;
            }

            _username = EditorPrefs.GetString("Pixyz_Username", "");
        }

        private void OnGUI() {

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            string[] titles = { "Current License", "Online", "Offline", "License Server", "Tokens" };
            _selectedTab = GUILayout.Toolbar(_selectedTab, titles, "LargeButton", GUI.ToolbarButtonSize.FitToContents);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUIExtensions.DrawLine(1.5f);

            GUIStyle style = new GUIStyle();
            _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition, GUILayout.MaxHeight(Screen.height - 30));
            style.margin = new RectOffset(10, 10, 10, 10);
            EditorGUILayout.BeginVertical(style);
            switch (_selectedTab) {
                case 0:
                    drawCurrentLicense();
                    break;
                case 1:
                    if (!Configuration.IsConnectedToWebServer())
                        drawLogin();
                    else
                        drawOnline();
                    break;
                case 2:
                    drawOffline();
                    break;
                case 3:
                    drawLicenseServer();
                    break;
                case 4:
                    drawTokens();
                    break;
            }
            EditorGUILayout.EndVertical();
            GUILayout.EndScrollView();

            // Outer calls
            if (_doRelease) {
                _doRelease = false;
                if (Configuration.ReleaseWebLicense(licenseToProcess))
                    EditorUtility.DisplayDialog("Release Complete", "The license release has been completed.", "Ok");
                else
                    EditorUtility.DisplayDialog("Release Failed", "An error has occured while releasing the license: " + Configuration.GetLastError(), "Ok");
                Configuration.RefreshAvailableLicenses();
            } else if (_doRequest) {
                _doRequest = false;
                if (Configuration.RequestWebLicense(licenseToProcess) && Configuration.CheckLicense())
                    EditorUtility.DisplayDialog("Installation Complete", "The license installation has been completed.", "Ok");
                else
                    EditorUtility.DisplayDialog("Installation Failed", "An error occured while installing the license: " + Configuration.GetLastError(), "Ok");
                Configuration.RefreshAvailableLicenses();
            }
        }

        private void drawCurrentLicense() {
            AboutWindow.ShowLicenseInfos();
        }

        private Vector2 scrollOnline;

        private void drawOnline() {

            var licenses = Configuration.Licenses.list.OrderByDescending(x => x.validity.ToUnityObject()).ToArray();
            var validLicenses = licenses.Where(x => x.validity.ToUnityObject() > DateTime.UtcNow).ToArray();

            // Logged As Block
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Logged as", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            bool logout = GUILayout.Button("Logout ↗", GUILayout.Height(18), GUILayout.Width(70));
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Username", Configuration.Username);

            if (logout) {
                _password = null;
                Configuration.DisconnectFromWebServer();
                return;
            }

            GUILayout.Space(12);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Licenses", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh ↻", GUILayout.Height(18), GUILayout.Width(70))) {
                Configuration.RefreshAvailableLicenses();
            }
            GUILayout.EndHorizontal();

            if (validLicenses.Length == 0) {
                EditorGUILayout.HelpBox("There are no licenses available in your account.\nTo get a trial or purchase a license, please go to the Pixyz website : ", MessageType.Warning);
                EditorGUIExtensions.DrawHyperlink(Configuration.WebsiteURL + "/login", Configuration.WebsiteURL + "/login");
            }

            bool checkLicense = Configuration.CheckLicense();
            var currentLicense = NativeInterface.GetCurrentLicenseInfos();

            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.richText = true;

            GUIStyle italic = new GUIStyle(EditorStyles.label);
            italic.fontStyle = FontStyle.Italic;

            const int COL1_WIDTH = 100;
            const int COL2_WIDTH = 125;
            const int COL3_WIDTH = 110;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("End Date", italic, GUILayout.Width(COL1_WIDTH));
            EditorGUILayout.LabelField("Remaining Days", italic, GUILayout.Width(COL2_WIDTH));
            EditorGUILayout.LabelField("Assigned", italic, GUILayout.Width(COL3_WIDTH));
            EditorGUILayout.LabelField("Current", italic, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
            GUILayout.EndHorizontal();

            scrollOnline = EditorGUILayout.BeginScrollView(scrollOnline, GUIStyle.none, GUI.skin.verticalScrollbar);

            for (int r = 0; r < licenses.Length; r++) {

                NativeInterface.WebLicenseInfo license = licenses[r];

                int remainingDays = (license.validity.ToUnityObject() - DateTime.Today).Days;

                bool canInstall;
                bool canRelease;
                bool isInstalled = false;
                string tooltip = null;
                string assignedTo = null;

                if (license.current && currentLicense.endDate.year != -1 && currentLicense.endDate.ToUnityObject() == license.validity.ToUnityObject()) {
                    // License is binded and installed on this machine. Can reinstall, can release
                    isInstalled = true;
                    canInstall = true;
                    canRelease = true;
                    assignedTo = "This Machine";
                    tooltip = "This license is currently installed on this machine.";
                } else if (license.current) {
                    // License is binded to this machine but not installed, Can install, can release
                    canInstall = true;
                    canRelease = true;
                    tooltip = "This license is binded to this machine, but it is not installed. Click on 'Install' to install the license. This step requires Administrator Privileges. Please contact the Pixyz support at support@pi.xyz if you encounter any issue.";
                    assignedTo = "This Machine";
                } else if (license.inUse == 1) {
                    // License used on another machine : Can't install, can't release
                    canInstall = false;
                    canRelease = false;
                    assignedTo = "Other Machine";
                    tooltip = "This license is used on another machine. To release the license, you need to release the license from the other machine. Please contact the Pixyz support at support@pi.xyz for any issue.";
                } else {
                    // Free License : Can install, can't release
                    canInstall = true;
                    canRelease = false;
                    assignedTo = "Free";
                }

                if (remainingDays < 0) {
                    canInstall = false;
                    canRelease = false;
                }

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(license.validity.ToUnityObject().ToString("yyyy-MM-dd"), tooltip), style, GUILayout.Width(COL1_WIDTH));
                EditorGUILayout.LabelField(new GUIContent(license.validity.GetRemainingDaysText(), tooltip), style, GUILayout.Width(COL2_WIDTH));
                EditorGUILayout.LabelField(new GUIContent(assignedTo, tooltip), style, GUILayout.Width(COL3_WIDTH));
                EditorGUILayout.LabelField(new GUIContent(isInstalled ? "✔" : "", tooltip), style, GUILayout.ExpandWidth(true), GUILayout.MinWidth(50));
                EditorGUI.BeginDisabledGroup(!canInstall);
                if (GUILayout.Button(new GUIContent(isInstalled ? "Reinstall" : "Install", tooltip), GUILayout.Height(18), GUILayout.Width(70))) {
                    _doRequest = true;
                    licenseToProcess = license;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(!canRelease);
                if (GUILayout.Button(new GUIContent("Release", tooltip), GUILayout.Height(18), GUILayout.Width(70))) {
                    if (EditorUtility.DisplayDialog("Warning", "Release (or uninstall) current license lets you install it on another computer. This action is available only once.\n\nAre you sure you want to release this license ?", "Yes", "No")) {
                        _doRelease = true;
                        licenseToProcess = license;
                    }
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                //if (!string.IsNullOrEmpty(tooltip)) {
                //    EditorGUILayout.HelpBox(tooltip, MessageType.Warning);
                //}
            }
            EditorGUILayout.EndScrollView();
        }

        private void drawOffline() {

            GUILayout.FlexibleSpace();
            GUILayout.Label("Generate an activation code and upload it on Pixyz website");
            if (GUILayout.Button("Generate Activation Code")) {
                var path = EditorUtility.SaveFilePanel(
                     "Save activation code",
                     "",
                     "Pixyz_activationCode.bin",
                     "Binary file;*.bin");

                if (Configuration.GenerateActivationCode(path))
                    EditorUtility.DisplayDialog("Generation Succeed", "The activation code has been successfully generated.", "Ok");
                else
                    EditorUtility.DisplayDialog("Generation Failed", "An error occured while generating the file: " + Configuration.GetLastError(), "Ok");
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("Install a New License");
            if (GUILayout.Button("Install License")) {
                var path = EditorUtility.OpenFilePanel(
                        "Open installation code (*.bin) or license file (*.lic)",
                        "",
                        "Install file;*.bin;*.lic");
                if (Configuration.InstallActivationCode(path))
                    EditorUtility.DisplayDialog("Installation Succeed", "The installation code has been installed.", "Ok");
                else
                    EditorUtility.DisplayDialog("Installation Failed", "An error occured while installing: " + Configuration.GetLastError(), "Ok");
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label("Generate a release code and upload it on Pixyz website");
            if (GUILayout.Button("Generate Release Code")) {
                if (EditorUtility.DisplayDialog("Warning", "Release (or uninstall) current license lets you install it on another computer. This action is available only once.\n\nAre you sure you want to release this license ?", "Yes", "No")) {
                    var path = EditorUtility.SaveFilePanel(
                     "Save release code as BIN",
                     "",
                     "Pixyz_releaseCode.bin",
                     "Binary file;*.bin");

                    if (Configuration.GenerateDeactivationCode(path))
                        EditorUtility.DisplayDialog("Generation Succeed", "The release code has been successfully generated.", "Ok");
                    else
                        EditorUtility.DisplayDialog("Generation Failed", "An error occured while generating the file: " + Configuration.GetLastError(), "Ok");
                }
            }
            GUILayout.FlexibleSpace();
        }

        private string serverError;

        private void drawLicenseServer() {

            _address = EditorGUILayout.TextField("Address", _address);
            GUILayout.Space(5);
            _port = EditorGUILayout.IntField("Port", _port);
            GUILayout.Space(5);
            _flexLM = EditorGUILayout.Toggle("Use FlexLM", _flexLM);
            GUILayout.Space(10);

            //bool enabled = (Configuration.CurrentLicenseServer != null && ((_address != Configuration.CurrentLicenseServer.serverAddress) // Or if the current license server address if different than address above
            //    || (_port != Configuration.CurrentLicenseServer.serverPort) // Or if port is different
            //    || (_flexLM != Configuration.CurrentLicenseServer.useFlexLM))); // Or if now we stop or begin using FlexLM

            EditorGUI.BeginDisabledGroup(false);
            if (GUILayout.Button("Connect")) {
                if (Configuration.ConfigureLicenseServer(_address, (ushort)_port, _flexLM)) {
                    serverError = null;
                    EditorUtility.DisplayDialog("Success", "License server has been successfuly configured", "OK");
                }
                else {
                    serverError = Configuration.GetLastError();
                }
            }
            EditorGUI.EndDisabledGroup();

            if (!string.IsNullOrEmpty(serverError)) {
                EditorGUILayout.HelpBox("Server Configuration Error : " + serverError, MessageType.Error);
            }
        }

        private static GUIStyle tokenStyleValid;
        private static GUIStyle TokenStyleValid {
            get {
                if (tokenStyleValid == null) {
                    tokenStyleValid = new GUIStyle();
                    tokenStyleValid.normal.textColor = Color.white;
                    tokenStyleValid.normal.SetHiResBackground("PixyzBlank", new Color(0.1f, 0.53f, 0.1f));
                    tokenStyleValid.margin = new RectOffset(8, 8, 8, 8);
                    tokenStyleValid.padding = new RectOffset(6, 6, 2, 2);
                    tokenStyleValid.alignment = TextAnchor.MiddleCenter;
                    tokenStyleValid.fontStyle = FontStyle.Bold;
                }
                return tokenStyleValid;
            }
        }

        private static GUIStyle tokenStyleNotValid;
        private static GUIStyle TokenStyleNotValid {
            get {
                if (tokenStyleNotValid == null) {
                    tokenStyleNotValid = new GUIStyle(TokenStyleValid);
                    tokenStyleNotValid.normal.SetHiResBackground("PixyzBlank", new Color(0.53f, 0.1f, 0.1f));
                }
                return tokenStyleNotValid;
            }
        }

        private void drawTokens() {
            EditorGUILayout.LabelField("MANDATORY", EditorStyles.boldLabel);
            drawTokens(Configuration.Tokens.Where(x => x.mandatory == true));
            EditorGUILayout.LabelField("OPTIONAL", EditorStyles.boldLabel);
            drawTokens(Configuration.Tokens.Where(x => x.mandatory == false));
        }

        private void drawTokens(IEnumerable<Token> tokens)
        {
            EditorGUILayout.BeginHorizontal();
            float stackedWidth = 0;
            float sideMargins = (TokenStyleValid.margin.left) * 2;

            foreach (Token token in tokens) {

                var content = new GUIContent(token.name, token.mandatory ? (token.valid ? "You currently have this mandatory token" : "You are currently missing this mandatory token. It is required for the plugin to work.") : (token.valid ? "You currently have this optional token" : "You are currently missing this optional token"));
                Vector2 size = TokenStyleValid.CalcSize(content);

                if (stackedWidth + size.x + sideMargins > position.width) {
                    stackedWidth = 0;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                GUILayout.Label(content, token.valid ? TokenStyleValid : TokenStyleNotValid, GUILayout.Width(size.x));
                stackedWidth += size.x + sideMargins;
            }
            EditorGUILayout.EndHorizontal();
        }

        private string loginError;

        private void drawLogin() {

            var usernameTmp = EditorGUILayout.TextField("Username", _username);

            if (_username != usernameTmp) {
                _username = usernameTmp;
                EditorPrefs.SetString("Pixyz_Username", _username);
            }

            GUILayout.Space(5);
            _password = EditorGUILayout.PasswordField("Password", _password);
            GUILayout.Space(10);

            bool keyboardShortcutPressed = Event.current.isKey &&
                 (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return);

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password));
            if (keyboardShortcutPressed || GUILayout.Button("Connect")) {
                if (Configuration.ConnectToWebServer(_username, _password)) {
                    loginError = null;
                } else {
                    loginError = Configuration.GetLastError();
                }
                Repaint();
            }
            EditorGUI.EndDisabledGroup();

            if (!string.IsNullOrEmpty(loginError)) {
                EditorGUILayout.HelpBox("Login Error : " + loginError, MessageType.Error);
            }
        }
    }
}