using UnityEditor;
using UnityEngine;

namespace Pixyz.Editor {

    /// <summary>
    /// A tinted instance of the Pixyz Editor Style.
    /// Useful to create menus with a given tint, that will work on both free and pro Unity versions
    /// </summary>
    public class ColoredTheme {

        public readonly Color tinit;

        public ColoredTheme(Color tint) {
            this.tinit = tint;
        }

        private GUIStyle _categoryTitle;
        public GUIStyle categoryTitle {
            get {
                if (_categoryTitle == null) {
                    _categoryTitle = new GUIStyle();
                    _categoryTitle.normal.textColor = new Color(.9f, .9f, .9f);
                    _categoryTitle.fontSize = 12;
                }
                return _categoryTitle;
            }
        }

        private GUIStyle _categoryHeader;
        public GUIStyle categoryHeader {
            get {
                if (_categoryHeader == null || _categoryHeader.normal.background == null) {
                    _categoryHeader = new GUIStyle();
                    _categoryHeader.padding = new RectOffset(0, 0, 7, 7);
                    _categoryHeader.border = new RectOffset(14, 14, 14, 14);
                    Color localTint = EditorGUIUtility.isProSkin ? new Color(tinit.r, tinit.g, tinit.b, tinit.a) : new Color(tinit.r, tinit.g, tinit.b, tinit.a);
                    _categoryHeader.normal.SetHiResBackground("CategoryHeader", localTint);
                }
                return _categoryHeader;
            }
        }

        private GUIStyle _categoryContent;
        public GUIStyle categoryContent {
            get {
                if (_categoryContent == null || _categoryContent.normal.background == null) {
                    _categoryContent = new GUIStyle();
                    _categoryContent.border = new RectOffset(10, 10, 10, 10);
                    Color localTint = EditorGUIUtility.isProSkin ?
                        new Color(tinit.r, tinit.g, tinit.b, tinit.a * 0.35f) :
                        new Color(Mathf.Pow(tinit.r, 0.75f), Mathf.Pow(tinit.g, 0.75f), Mathf.Pow(tinit.b, 0.75f), tinit.a * 0.5f);
                    _categoryContent.normal.SetHiResBackground("CategoryContentBg", localTint);
                }
                return _categoryContent;
            }
        }

        private GUIStyle _ruleBlockTitle;
        public GUIStyle ruleBlockTitle {
            get {
                if (_ruleBlockTitle == null) {
                    _ruleBlockTitle = new GUIStyle();
                    _ruleBlockTitle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f) : new Color(.1f, .1f, .1f);
                    _ruleBlockTitle.fontSize = 12;
                    _ruleBlockTitle.margin = new RectOffset(32, 10, 12, 10);
                }
                return _ruleBlockTitle;
            }
        }

        private GUIStyle _toolboxBlockTitle;
        public GUIStyle toolboxBlockTitle {
            get {
                if (_toolboxBlockTitle == null) {
                    _toolboxBlockTitle = new GUIStyle();
                    _toolboxBlockTitle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f) : new Color(.1f, .1f, .1f);
                    _toolboxBlockTitle.fontSize = 12;
                    _toolboxBlockTitle.margin = new RectOffset(12, 10, 12, 10);
                }
                return _toolboxBlockTitle;
            }
        }

        private GUIStyle _labelTitle;
        public GUIStyle labelTitle {
            get {
                if (_labelTitle == null) {
                    _labelTitle = new GUIStyle();
                    _labelTitle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f) : new Color(.1f, .1f, .1f);
                    _labelTitle.fontSize = 12;
                    _labelTitle.margin = new RectOffset(6, 10, 22, 6);
                }
                return _labelTitle;
            }
        }

        private GUIStyle _ruleBlockText;
        public GUIStyle ruleBlockText {
            get {
                if (_ruleBlockText == null) {
                    _ruleBlockText = new GUIStyle();
                    _ruleBlockText.normal.textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f) : new Color(.1f, .1f, .1f);
                }
                return _ruleBlockText;
            }
        }

        private GUIStyle _ruleBlock;
        public GUIStyle ruleBlock {
            get {
                if (_ruleBlock == null || _ruleBlock.normal.background == null) {
                    _ruleBlock = new GUIStyle();
                    _ruleBlock.padding = new RectOffset(18, 18, 10, 18);
                    _ruleBlock.border = new RectOffset(20, 20, 20, 20);
                    _ruleBlock.margin = new RectOffset(10, 10, 0, 0);
                    Color localTint = EditorGUIUtility.isProSkin ?
                        new Color(Mathf.Pow(tinit.r, 0.4f), Mathf.Pow(tinit.g, 0.4f), Mathf.Pow(tinit.b, 0.4f), tinit.a * 0.25f) :
                        new Color(Mathf.Pow(tinit.r, 0.1f), Mathf.Pow(tinit.g, 0.1f), Mathf.Pow(tinit.b, 0.1f), tinit.a * 0.75f);
                    _ruleBlock.normal.SetHiResBackground("RuleBlock", localTint);
                }
                return _ruleBlock;
            }
        }

        private GUIStyle _downFluxTitle;
        public GUIStyle downFluxTitle {
            get {
                if (_downFluxTitle == null) {
                    _downFluxTitle = new GUIStyle();
                    Color localTint = EditorGUIUtility.isProSkin ?
                        new Color(Mathf.Pow(tinit.r, 0.4f), Mathf.Pow(tinit.g, 0.4f), Mathf.Pow(tinit.b, 0.4f), tinit.a * 0.25f) :
                        new Color(Mathf.Pow(tinit.r, 0.1f), Mathf.Pow(tinit.g, 0.1f), Mathf.Pow(tinit.b, 0.1f), tinit.a * 0.75f);
                    _downFluxTitle.normal.textColor = localTint;
                    _downFluxTitle.fontSize = 12;
                    _downFluxTitle.margin = new RectOffset(32, 10, 12, 10);
                }
                return _downFluxTitle;
            }
        }

        public Texture2D iconDownFlux {
            get {
                Color localTint = EditorGUIUtility.isProSkin ?
                    new Color(Mathf.Pow(tinit.r, 0.4f), Mathf.Pow(tinit.g, 0.4f), Mathf.Pow(tinit.b, 0.4f), tinit.a * 0.25f) :
                    new Color(Mathf.Pow(tinit.r, 0.1f), Mathf.Pow(tinit.g, 0.1f), Mathf.Pow(tinit.b, 0.1f), tinit.a * 0.75f);
                return TextureCache.GetTexture("IconDownFlux", localTint, false);
            }
        }

        public Texture2D iconDownFluxMid {
            get {
                Color localTint = EditorGUIUtility.isProSkin ?
                    new Color(Mathf.Pow(tinit.r, 0.4f), Mathf.Pow(tinit.g, 0.4f), Mathf.Pow(tinit.b, 0.4f), tinit.a * 0.25f) :
                    new Color(Mathf.Pow(tinit.r, 0.1f), Mathf.Pow(tinit.g, 0.1f), Mathf.Pow(tinit.b, 0.1f), tinit.a * 0.75f);
                return TextureCache.GetTexture("IconDownFluxMid", localTint, false);
            }
        }
    }
}
