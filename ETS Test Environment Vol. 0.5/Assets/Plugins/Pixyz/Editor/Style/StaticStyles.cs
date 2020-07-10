using UnityEditor;
using UnityEngine;

namespace Pixyz.Editor {

    /// <summary>
    /// Various static cached styles for Pixyz editors
    /// </summary>
    public static class StaticStyles {

        private static GUIStyle _RuleBlockError;
        public static GUIStyle RuleBlockError {
            get {
                if (_RuleBlockError == null || _RuleBlockError.normal.background == null) {
                    _RuleBlockError = new GUIStyle();
                    _RuleBlockError.border = new RectOffset(42, 20, 20, 20);
                    Color tint = new Color(1, 0, 0, 0.75f);
                    _RuleBlockError.normal.SetHiResBackground("RuleBlockFrame", tint);
                }
                return _RuleBlockError;
            }
        }

        private static GUIStyle _RuleBlockDragDropFrame;
        public static GUIStyle RuleBlockDragDropFrame {
            get {
                if (_RuleBlockDragDropFrame == null || _RuleBlockDragDropFrame.normal.background == null) {
                    _RuleBlockDragDropFrame = new GUIStyle();
                    _RuleBlockDragDropFrame.border = new RectOffset(42, 20, 20, 20);
                    Color tint = new Color(1, 1, 1, 0.75f);
                    _RuleBlockDragDropFrame.normal.SetHiResBackground("RuleBlockFrame", tint);
                }
                return _RuleBlockDragDropFrame;
            }
        }

        private static GUIStyle _ListDetail;
        public static GUIStyle ListDetail {
            get {
                if (_ListDetail == null || _ListDetail.normal.background == null) {
                    _ListDetail = new GUIStyle();
                    _ListDetail.border = new RectOffset(4, 36, 4, 16);
                    _ListDetail.padding = new RectOffset(5, 27, 5, 20);
                    Color tint = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.13f) : new Color(1, 1, 1, 0.5f);
                    _ListDetail.normal.SetHiResBackground("ListDetail", tint);
                }
                return _ListDetail;
            }
        }

        private static GUIStyle _DragDropFrame;
        public static GUIStyle DragDropFrame {
            get {
                if (_DragDropFrame == null || _DragDropFrame.normal.background == null) {
                    _DragDropFrame = new GUIStyle();
                    _DragDropFrame.border = new RectOffset(14, 14, 14, 14);
                    Color tint = new Color(0.75f, 0, 0.9f, 1);
                    _DragDropFrame.normal.SetHiResBackground("Frame", tint);
                }
                return _DragDropFrame;
            }
        }

        private static GUIStyle _DragDropReplaceValid;
        public static GUIStyle DragDropReplaceValid {
            get {
                if (_DragDropReplaceValid == null || _DragDropReplaceValid.normal.background == null) {
                    _DragDropReplaceValid = new GUIStyle();
                    _DragDropReplaceValid.border = new RectOffset(0, 0, 16, 16);
                    Color tint = new Color(1f, 1f, 1f, 1f);
                    _DragDropReplaceValid.normal.SetHiResBackground("DragDropReplace", tint);
                }
                return _DragDropReplaceValid;
            }
        }
         
        private static GUIStyle _DragDropReplaceInvalid;
        public static GUIStyle DragDropReplaceInvalid {
            get {
                if (_DragDropReplaceInvalid == null || _DragDropReplaceInvalid.normal.background == null) {
                    _DragDropReplaceInvalid = new GUIStyle();
                    _DragDropReplaceInvalid.border = new RectOffset(0, 0, 16, 16);
                    Color tint = new Color(1f, 0f, 0f, 1f);
                    _DragDropReplaceInvalid.normal.SetHiResBackground("DragDropReplace", tint);
                }
                return _DragDropReplaceInvalid;
            }
        }

        public static Texture2D IconMinusSmall {
            get {
                return EditorGUIUtility.isProSkin ?
                    TextureCache.GetTexture("IconMinusSmall", new Color(.9f, .9f, .9f), false) :
                    TextureCache.GetTexture("IconMinusSmall", Color.black, false);
            }
        }

        public static Texture2D IconPlusSmall {
            get {
                return EditorGUIUtility.isProSkin ?
                    TextureCache.GetTexture("IconPlusSmall", new Color(.9f, .9f, .9f), false) :
                    TextureCache.GetTexture("IconPlusSmall", Color.black, false);
            }
        }
        
        public static Texture2D IconDetailHeader {
            get {
                return EditorGUIUtility.isProSkin ?
                    TextureCache.GetTexture("IconDetail", new Color(.9f, .9f, .9f), false) :
                    TextureCache.GetTexture("IconDetail", new Color(.9f, .9f, .9f), false);
            }
        }

        public static Texture2D IconDetailRuleBlock {
            get {
                return TextureCache.GetTexture("IconDetail", EditorGUIUtility.isProSkin ? new Color(.9f, .9f, .9f) : Color.black, false);
            }
        }

        public static Texture2D IconReorder {
            get {
                return EditorGUIUtility.isProSkin ?
                    TextureCache.GetTexture("IconReorder", new Color(.9f, .9f, .9f, 0.5f), false) :
                    TextureCache.GetTexture("IconReorder", new Color(0f, 0f, 0f, 0.5f), false);
            }
        }

        public static Texture2D IconPlusMedium {
            get {
                return EditorGUIUtility.isProSkin ?
                    TextureCache.GetTexture("IconPlusMedium", new Color(.9f, .9f, .9f), false) :
                    TextureCache.GetTexture("IconPlusMedium", Color.black, false);
            }
        }

        public static Texture2D IconUp {
            get {
                return EditorGUIUtility.isProSkin ?
                    TextureCache.GetTexture("IconUp", new Color(.9f, .9f, .9f), false) :
                    TextureCache.GetTexture("IconUp", new Color(.9f, .9f, .9f), false);
            }
        }

        public static Texture2D IconDown {
            get {
                return EditorGUIUtility.isProSkin ?
                    TextureCache.GetTexture("IconDown", new Color(.9f, .9f, .9f), false) :
                    TextureCache.GetTexture("IconDown", new Color(.9f, .9f, .9f), false);
            }
        }

        private static GUIStyle _IconErrorSmall;
        public static GUIStyle IconErrorSmall {
            get {
                if (_IconErrorSmall == null || _IconErrorSmall.normal.background == null) {
                    _IconErrorSmall = new GUIStyle();
                    _IconErrorSmall.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.9f, .9f, .9f) : Color.black;
                    _IconErrorSmall.normal.background = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;
                }
                return _IconErrorSmall;
            }
        }

        private static GUIStyle _IconWarningSmall;
        public static GUIStyle IconWarningSmall {
            get {
                if (_IconWarningSmall == null || _IconWarningSmall.normal.background == null) {
                    _IconWarningSmall = new GUIStyle();
                    _IconWarningSmall.normal.background = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
                }
                return _IconWarningSmall;
            }
        }

        private static GUIStyle _IconInfoSmall;
        public static GUIStyle IconInfoSmall {
            get {
                if (_IconInfoSmall == null || _IconInfoSmall.normal.background == null) {
                    _IconInfoSmall = new GUIStyle();
                    _IconInfoSmall.normal.background = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;
                }
                return _IconInfoSmall;
            }
        }
    }
}
