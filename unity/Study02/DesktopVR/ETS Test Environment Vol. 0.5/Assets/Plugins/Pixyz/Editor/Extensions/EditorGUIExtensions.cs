using Pixyz.Utils;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Editor {

    /// <summary>
    /// Extensions methods for Unity Editor GUI
    /// </summary>
    public static class EditorGUIExtensions {

        private static GUIStyle _RectStyle;

        private static GUIStyle StaticRectStyle {
            get {
                if (_RectStyle == null)
                    _RectStyle = new GUIStyle();
                return _RectStyle;
            }
        }

        /// <summary>
        /// Returns the editor Color depending on the Pro/Free version the user has.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Color GetUnityColor(ColorType type) {
            switch (type) {
                case ColorType.Highlight:
                    return EditorColor;
                case ColorType.Active:
                    return EditorColorLight;
            }
            return Color.black;
        }

        public static readonly Color EditorColorBasic = new Color(0.76f, 0.76f, 0.76f);
        public static readonly Color EditorColorPro = new Color(0.40f, 0.40f, 0.40f);
        public static readonly Color EditorColorBasicLight = new Color(0.87f, 0.87f, 0.87f);
        public static readonly Color EditorColorProLight = new Color(0.19f, 0.19f, 0.19f);

        public static Color EditorColor => EditorGUIUtility.isProSkin ? EditorColorPro : EditorColorBasic;
        public static Color EditorColorLight => EditorGUIUtility.isProSkin ? EditorColorProLight : EditorColorBasicLight;

        public static bool DrawCategory(ColoredTheme style, string category, Action action, string tooltip) {
            return DrawCategory(style, category, false, action, tooltip, true);
        }

        public static bool DrawCategory(ColoredTheme style, string category, bool isOpenable, Action action, string tooltip, bool isOpen=true) {

            EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(0, 8, 6, 6) });

            Rect rect = EditorGUILayout.BeginVertical(style.categoryHeader);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(new GUIContent(category, tooltip), style.categoryTitle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(style.categoryContent);
            EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(10, 10, 10, 10) });

            action.Invoke();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            /// Fold Button
            if (isOpenable) {
                if (EditorGUIExtensions.DrawButton(new Rect(rect.x + 2, rect.y + 2, 24, 24), isOpen ? StaticStyles.IconDown : StaticStyles.IconUp)) {
                    isOpen = !isOpen;
                    EditorWindow.mouseOverWindow.Repaint();
                }
            }

            EditorGUILayout.EndVertical();
            return isOpen;
        }

        public static Rect DrawSubCategory(ColoredTheme style, string category, Action action, string tooltip) {
            Rect rect = EditorGUILayout.BeginVertical();
            GUILayout.Label(new GUIContent(category, tooltip), style.labelTitle);
            EditorGUILayout.BeginVertical();

            action.Invoke();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            return rect;
        }

        public static bool DrawButton(Rect rect, Texture2D texture) {
            bool isClicked = false;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (rect.Contains(Event.current.mousePosition)) {
                if (texture != null)
                    GUI.DrawTexture(rect, TextureCache.GetTexture("SkinButtonBgOnDark"));

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                    isClicked = true;
                }
            }
            if (texture != null)
                GUI.DrawTexture(rect, texture);
            return isClicked;
        }

        public static bool DrawDragButton(Rect rect, Texture2D texture) {
            bool isClicked = false;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);
            if (rect.Contains(Event.current.mousePosition)) {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                    isClicked = true;
                }
            }
            if (texture != null)
                GUI.DrawTexture(rect, texture);
            return isClicked;
        }

        public static void GUIDrawRect(Rect position, Color color, int borderThickness = 0, string title = "") {
            Color tmp = color;
            tmp.a = 1;
            GUIDrawRect(position, color, tmp, borderThickness, new GUIContent(title), TextAnchor.MiddleCenter);
        }

        public static bool dirty = false;
        public static bool Dirty {
            get {
                return dirty;
            }
            set {
                dirty = value;
                GUI.enabled = value;
            }
        }

        public static void GUIDrawRect(Rect position, Color color, Color borderColor, int borderThickness, GUIContent text, TextAnchor rectTextAnchor) {

            if (color != Color.white && color != Color.gray)
                StaticRectStyle.normal.textColor = Color.white;

            StaticRectStyle.clipping = TextClipping.Clip;
            StaticRectStyle.border = new RectOffset(-borderThickness, -borderThickness, -borderThickness, -borderThickness);
            StaticRectStyle.alignment = rectTextAnchor;
            StaticRectStyle.fontSize = 10;

            Rect innerRect = position;

            if (borderThickness > 0) {
                EditorGUI.DrawRect(position, borderColor);
                innerRect = new Rect(position.x + borderThickness, position.y + borderThickness, position.width - borderThickness * 2, position.height - borderThickness * 2);
            }

            EditorGUI.DrawRect(innerRect, color);

            Rect contentRect = new Rect(position.x + borderThickness, position.y + borderThickness, position.width - borderThickness * 2, position.height - borderThickness * 2);
            GUI.Box(contentRect, text, StaticRectStyle);
        }

        public static void SetHiResBackground(this GUIStyleState guiStateStyle, string textureName, Color tint) {
            guiStateStyle.background = TextureCache.GetTexture(textureName, tint, true);
            guiStateStyle.scaledBackgrounds = new Texture2D[] { TextureCache.GetTexture(textureName, tint) };
        }

        /// <summary>
        /// Converts a color to an int32 (bit shifting).
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static int ToInt32(this Color color) {
            return ((int)(color.r * 255) << 0) | ((int)(color.g * 255) << 8) | ((int)(color.b * 255) << 16) | ((int)(color.a * 255) << 24);
        }

        public const float BYTE_TO_FLOAT = 0x3b808080;

        /// <summary>
        /// Converts an int to a color (bit shifting).
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color ToColor(int value) {
            int red = (value >> 0) & 255;
            int green = (value >> 8) & 255;
            int blue = (value >> 16) & 255;
            int alpha = (value >> 24) & 255;
            return new Color(BYTE_TO_FLOAT * red, BYTE_TO_FLOAT * green, BYTE_TO_FLOAT * blue, BYTE_TO_FLOAT * alpha);
        }

        public static void DrawLine(float height, Color color) {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.y -= 2;
            rect.x -= 8;
            rect.width += 16;
            rect.height = height;
            EditorGUI.DrawRect(rect, color);
        }

        public static void DrawLine(float height = 1) {
            DrawLine(height, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        /// <summary>
        /// Draws an hyperlink that opens the link in the default browser.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="link"></param>
        public static void DrawHyperlink(string label, string link, bool bold = false, bool centered = false) {

            Color linkColor = EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 1) : new Color(0.30f, 0.30f, 1);

            var style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = linkColor;

            var content = new GUIContent(label);
            Vector2 size = style.CalcSize(content);

            if (centered) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }

            if (GUILayout.Button(label, style, GUILayout.Width(size.x))) {
                Application.OpenURL(link);
            }

            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            // Underlines the text
            var underlineRect = new Rect(rect);
            underlineRect.width = size.x;
            underlineRect.y += underlineRect.height - 1;
            underlineRect.height = 0.5f;
            GUIDrawRect(underlineRect, linkColor);

            if (centered) {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        public static object DrawGUIAuto<T>(this object value, GUIContent guiContent) {

            /// Base types
            switch (typeof(T).ToString()) {
                case "System.Boolean":
                    return EditorGUILayout.Toggle(guiContent, (bool)value);
                case "System.Byte":
                    return EditorGUILayout.IntField(guiContent, (int)value);
                case "System.Int16":
                    return EditorGUILayout.IntField(guiContent, (short)value);
                case "System.Int32":
                    return EditorGUILayout.IntField(guiContent, (int)value);
                case "System.Int64":
                    return EditorGUILayout.LongField(guiContent, (long)value);
                case "System.Single":
                    return EditorGUILayout.FloatField(guiContent, (float)value);
                case "System.Double":
                    return EditorGUILayout.DoubleField(guiContent, (double)value);
                case "System.String":
                    return EditorGUILayout.TextField(guiContent, (string)value);
            }

            /// More advanced types
            if (typeof(T).IsSubclassOf(typeof(UnityEngine.Object))) {
                return EditorGUILayout.ObjectField(guiContent, (UnityEngine.Object)value, typeof(T), false);
            } else if (typeof(T).IsEnum) {
                return EditorGUILayout.Popup(guiContent, Convert.ToInt32(value), Enum.GetValues(typeof(T)).OfType<object>().Select(o => o.ToString().FancifyCamelCase()).ToArray());
            } 

            throw new Exception($"Type '{typeof(T)}' isn't supported.");
        }
    }
}