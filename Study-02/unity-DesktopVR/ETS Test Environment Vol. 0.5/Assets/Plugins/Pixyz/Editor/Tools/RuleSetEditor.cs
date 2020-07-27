using Pixyz.Editor;
using Pixyz.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Tools.Editor {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(RuleSet))]
    public sealed class RuleSetEditor : UnityEditor.Editor {

        private Vector2 scrollPos;

        const int TRANSITION_SIZE = 36;
        const int HEADER_HEIGHT = 30;

        public VoidHandler changed;

        private RuleSet _rules;
        public RuleSet rules {
            get {
                if (_rules == null) {
                    _rules = target as RuleSet;
                    _rules.changed += settingsChanged;
                }
                return _rules;
            }
        }

        private ColoredTheme _style;
        public ColoredTheme style => (_style != null) ? _style : _style = new ColoredTheme(new Color(0.28f, 0.33f, 0.353f));

        public bool isValid = true;

        private void Awake() {
            replaceSource = null;
        }

        private void OnEnable() {
            replaceSource = null;
        }

        private void settingsChanged() {
            /// The Rules have changed :
            /// We first update the SerializedObject instance, which may not be up-to-date if the change comes from a different editor.
            serializedObject.Update();
            /// We ask the window to kindly repaint herself
            Repaint();
            /// Finally, we propagate the news to eventual Windows or such objects that are using the Editor
            changed?.Invoke();
        }

        public override void OnInspectorGUI() {

            EditorGUI.BeginChangeCheck();

            if (Event.current.type == EventType.MouseUp) {
                if (replaceSource != null) {
                    if (replaceDestination != null && replaceDestination.rule != null) {
                        int sourcePos = replaceSource.rule.getBlockIndex(replaceSource);
                        int destPos = replaceDestination.rule.getBlockIndex(replaceDestination);
                        replaceSource.rule.setBlock(sourcePos, replaceDestination);
                        replaceDestination.rule.setBlock(destPos, replaceSource);
                        // Reassing parent rules properly
                        var tmp = replaceSource.rule;
                        replaceSource.rule = replaceDestination.rule;
                        replaceDestination.rule = tmp;
                        GUI.changed = true;
                        replaceSource = replaceDestination = null;
                        return;
                    } else {
                        replaceSource = null;
                        Repaint();
                    }
                }
            }

            // Rules
            isValid = true;
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            drawGUI();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            // Footer
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(!isValid);
            if (GUILayout.Button("Run" + (isValid ? null : " (Please fix errors !)"), GUILayout.Height(35))) {
                rules.run();

                Undo.FlushUndoRecordObjects();
                Undo.SetCurrentGroupName(rules.name + " (Pixyz)");
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            bool rulesChanged = EditorGUI.EndChangeCheck();

            if (rulesChanged) {
                /// The rules have been changed :
                /// We mark it dirty to make sure it get serialized fully
                EditorUtility.SetDirty(target);
                /// Then we "Apply" modified properties of the serializableObject
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                /// Then we propagate the change to other potentially opened editors by triggering RuleSet's changed event.
                rules.invokeChanged();
                /// We can also force the serialization to the hard drive, but that produces a small slowdown spike.
                /// The asset is saved anyway when the project is saved.
                //AssetDatabase.Refresh();
                //AssetDatabase.SaveAssets();
            }
        }

        public void drawGUI() {

            for (int r = 0; r < rules.rulesCount; r++) {
                drawRuleGUI(rules.getRule(r));
            }

            // Add block button
            Rect rect = EditorGUILayout.BeginVertical();
            if (EditorGUIExtensions.DrawButton(new Rect(- 18 + (rect.width) / 2, rect.y + 8, TRANSITION_SIZE, TRANSITION_SIZE), StaticStyles.IconPlusMedium)) {
                rules.appendRule(new Rule());
                GUI.changed = true;
            }
            GUILayout.Space(14 + TRANSITION_SIZE);
            EditorGUILayout.EndVertical();
        }

        private void drawRuleGUI(Rule rule) {

            rule.OnAfterDeserialize();

            EditorGUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(0, 8, 6, 6) });

            Rect rect = EditorGUILayout.BeginVertical(style.categoryHeader);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(rule.name, style.categoryTitle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            Rect renameRect = new Rect(rect.x + HEADER_HEIGHT, rect.y, rect.width - HEADER_HEIGHT * 2, rect.height);
            if (EditorGUIExtensions.DrawButton(renameRect, null)) {
                PopupWindow.Show(renameRect, new RenamePopup(rule));
                GUI.changed = true;
            }

            EditorGUILayout.BeginVertical(style.categoryContent);
            EditorGUI.BeginDisabledGroup(!rule.isEnabled);
            GUILayout.Space(10);

            if (!rule.isCollapsed) {
                /// Blocks
                Rect contentRect = EditorGUILayout.BeginVertical();
                for (int b = 0; b < rule.blocksCount; b++) {
                    drawRuleBlockGUI(rule.getBlock(b));
                }
                EditorGUILayout.EndVertical();
                contentRect.width = rect.width;

                /// Add block button
                EditorGUILayout.BeginVertical();
                if (rule.blocksCount == 0 || rule.getBlock(rule.blocksCount - 1).action.outputType != null) {
                    if (EditorGUIExtensions.DrawButton(new Rect(rect.x + -5 + (contentRect.width) / 2, contentRect.y + contentRect.height + 2, TRANSITION_SIZE, TRANSITION_SIZE), StaticStyles.IconPlusMedium)) {
                        RuleBlock lastBlock = (rule.blocksCount == 0) ? null : rule.getBlock(rule.blocksCount - 1);
                        GenericMenu menu = new GenericMenu();
                        HashSet<ActionBase> availableActions = new HashSet<ActionBase>(ToolsBase.GetAvailableActions((lastBlock == null) ? null : lastBlock.action, null));
                        foreach (ActionBase action in ToolsBase.GetAllActions().OrderBy(x => x.menuPathRuleEngine)) {
                            if (availableActions.Contains(action)) {
                                menu.AddItem(new GUIContent(action.menuPathRuleEngine, action.tooltip), false, new GenericMenu.MenuFunction(() => {
                                    rule.appendBlock(new RuleBlock(action.id));
                                    GUI.changed = true;
                                }));
                            } else {
                                //menu.AddDisabledItem(new GUIContent(action.menuPathRuleEngine + " (" + GetFlowDisplayName(action.inputType) + ")", action.tooltip), false);
                            }
                        }
                        menu.ShowAsContext();
                    }
                    GUILayout.Space(14 + TRANSITION_SIZE);
                } else {
                    GUILayout.Space(10);
                }
                EditorGUILayout.EndVertical();
            }
            else {
                GUILayout.Label(rule.blocksCount + " Action" + ((rule.blocksCount > 1) ? "s" : null), style.ruleBlockTitle);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

            /// Fold Button
            if (EditorGUIExtensions.DrawButton(new Rect(rect.x + 2, rect.y + 2, 24, 24), rule.isCollapsed ? StaticStyles.IconUp : StaticStyles.IconDown)) {
                rule.isCollapsed = !rule.isCollapsed;
                GUI.changed = true;
            }

            /// Detail Button
            Rect detailRect = new Rect(rect.width - 26, rect.y + 2, 24, 24);
            if (EditorGUIExtensions.DrawButton(detailRect, StaticStyles.IconDetailHeader)) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Enabled"), rule.isEnabled, new GenericMenu.MenuFunction(() => {
                    rule.isEnabled = !rule.isEnabled;
                    GUI.changed = true;
                }));
                int cRuleIndex = rules.getRuleIndex(rule);
                if (cRuleIndex > 0)
                    menu.AddItem(new GUIContent("Move Up"), false, new GenericMenu.MenuFunction(() => {
                        var tmp = rules.getRule(cRuleIndex);
                        rules.setRule(cRuleIndex, rules.getRule(cRuleIndex - 1));
                        rules.setRule(cRuleIndex - 1, tmp);
                        GUI.changed = true;
                    }));
                if (cRuleIndex < rules.rulesCount - 1)
                    menu.AddItem(new GUIContent("Move Down"), false, new GenericMenu.MenuFunction(() => {
                        var tmp = rules.getRule(cRuleIndex);
                        rules.setRule(cRuleIndex, rules.getRule(cRuleIndex + 1));
                        rules.setRule(cRuleIndex + 1, tmp);
                        GUI.changed = true;
                    }));
                menu.AddItem(new GUIContent("Remove"), false, new GenericMenu.MenuFunction(() => {
                    rules.removeRule(rule);
                    GUI.changed = true;
                }));
                menu.ShowAsContext();
            }

            EditorGUILayout.EndVertical();
        }

        RuleBlock replaceSource;
        RuleBlock replaceDestination;
        private float replaceSourceY;

        private void drawRuleBlockGUI(RuleBlock block) {

            Rect rect = EditorGUILayout.BeginVertical(style.ruleBlock);
            GUILayout.Label(new GUIContent(block.action.displayNameRuleEngine, block.action.tooltip), style.ruleBlockTitle, GUILayout.Width(180));

            EditorGUI.BeginDisabledGroup(!block.isEnabled);

            EditorGUILayout.BeginVertical();

            block.action.onBeforeDraw();

            block.action.fieldInstances.DrawGUILayout();

            block.action.onAfterDraw();

            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();

            /// If doing a replacement operation (drag & drop)
            if (replaceSource != null) {
                float mouseY = Event.current.mousePosition.y;
                if (mouseY > rect.y && mouseY < rect.y + rect.height) {
                    if (replaceSource != block) {
                        float replaceDestinationY = rect.y + rect.height / 2;
                        bool canReorderHere = replaceSource.action.inputType == block.action.inputType && replaceSource.action.outputType == block.action.outputType;
                        GUI.Box(new Rect(rect.x - 10, (replaceSourceY < replaceDestinationY) ? replaceSourceY : replaceDestinationY, 16, Mathf.Abs(replaceDestinationY - replaceSourceY)), "", canReorderHere ? StaticStyles.DragDropReplaceValid : StaticStyles.DragDropReplaceInvalid);
                        GUI.Box(rect, "", StaticStyles.RuleBlockDragDropFrame);
                        replaceDestination = block;
                        replaceDestination = canReorderHere ? block : null;
                        Repaint();
                    } else {
                        replaceDestination = null;
                    }
                }
            }

            /// DragDrop Button
            if (EditorGUIExtensions.DrawDragButton(new Rect(rect.x + 7, rect.y + 6, 24, 24), StaticStyles.IconReorder)) {
                replaceSource = block;
                replaceSourceY = rect.y + rect.height / 2;
                Repaint();
                //return;
            }

            /// Detail Button
            if (EditorGUIExtensions.DrawButton(new Rect(rect.x + rect.width - 30, rect.y + 6, 24, 24), StaticStyles.IconDetailRuleBlock)) {
                GenericMenu menu = new GenericMenu();
                //menu.AddItem(new GUIContent("Enabled"), block.isEnabled, new GenericMenu.MenuFunction(() => { block.isEnabled = !block.isEnabled; }));
                
                if (block.action.helpersMethods.Length > 0) {
                    foreach (var method in block.action.helpersMethods) {
                        menu.AddItem(new GUIContent(method.Name.FancifyCamelCase()), false, new GenericMenu.MenuFunction(() => {
                            method.Invoke(block.action, null);
                            GUI.changed = true;
                        }));
                    }
                    menu.AddSeparator("");
                }

                int index = block.rule.getBlockIndex(block);
                RuleBlock previousBlock = (index - 1 >= 0) ? block.rule.getBlock(index - 1) : null;
                RuleBlock nextBlock = (index + 1 <= block.rule.blocksCount - 1) ? block.rule.getBlock(index + 1) : null;
                /// Replace
                IList<ActionBase> actionsForReplacement = ToolsBase.GetAvailableActions(previousBlock?.action, nextBlock?.action);
                foreach (ActionBase action in actionsForReplacement.OrderBy(x => x.menuPathRuleEngine)) {
                    if (action == block.action)
                        continue;
                    menu.AddItem(new GUIContent("Replace/" + action.menuPathRuleEngine, action.tooltip), false, new GenericMenu.MenuFunction(() => {
                        block.rule.setBlock(index, new RuleBlock(action.id));
                    }));
                }
                /// Insert Before
                IList<ActionBase> actionsForInsertBefore = ToolsBase.GetAvailableActions(previousBlock?.action, block.action);
                foreach (ActionBase action in actionsForInsertBefore.OrderBy(x => x.menuPathRuleEngine)) {
                    if (action == block.action)
                        continue;
                    menu.AddItem(new GUIContent("Insert Before/" + action.menuPathRuleEngine, action.tooltip), false, new GenericMenu.MenuFunction(() => {
                        block.rule.insertBlock(new RuleBlock(action.id), block.rule.getBlockIndex(block));
                    }));
                }
                // Insert After
                IList<ActionBase> actionsForInsertAfter = ToolsBase.GetAvailableActions(block.action, nextBlock?.action);
                foreach (ActionBase action in actionsForInsertBefore.OrderBy(x => x.menuPathRuleEngine)) {
                    if (action == block.action)
                        continue;
                    menu.AddItem(new GUIContent("Insert After/" + action.menuPathRuleEngine, action.tooltip), false, new GenericMenu.MenuFunction(() => {
                        block.rule.insertBlock(new RuleBlock(action.id), block.rule.getBlockIndex(block));
                    }));
                }
                // Remove
                menu.AddItem(new GUIContent("Remove"), false, new GenericMenu.MenuFunction(() => { block.rule.removeBlock(block); }));
                menu.ShowAsContext();
            }

            EditorGUILayout.EndVertical();

            // Transition
            if (block.action.outputType != null) {
                GUI.DrawTexture(new Rect(rect.x + 35, rect.y + rect.height - 4, TRANSITION_SIZE, TRANSITION_SIZE), (block.isLastBlockInRule)? style.iconDownFluxMid : style.iconDownFlux);
                GUI.Label(new Rect(rect.x + 70, rect.y + rect.height + (TRANSITION_SIZE - 20) / 2 - 2, TRANSITION_SIZE, 20), GetFlowDisplayName(block.action.outputType), style.downFluxTitle);
                GUILayout.Space(TRANSITION_SIZE - 8);
            }

            var errors = block.action.getErrors();
            var warnings = block.action.getWarnings();
            var infos = block.action.getInfos();
            float shift = 0;

            if (errors?.Count > 0) {
                GUI.Box(new Rect(rect.width - 55, rect.y + 7, 24, 24), ToGUIContent(errors), StaticStyles.IconErrorSmall);
                GUI.Label(new Rect(rect.width - 32, rect.y + 11, 24, 24), errors.Count.ToString(), style.ruleBlockText);
                if (block.rule.isEnabled)
                    isValid = false;
                shift -= 30;
            }

            if (warnings?.Count > 0) {
                GUI.Box(new Rect(rect.width - 55 + shift, rect.y + 7, 24, 24), ToGUIContent(warnings), StaticStyles.IconWarningSmall);
                GUI.Label(new Rect(rect.width - 32 + shift, rect.y + 11, 24, 24), warnings.Count.ToString(), style.ruleBlockText);
                shift -= 30;
            }

            if (infos?.Count > 0) {
                GUI.Box(new Rect(rect.width - 55 + shift, rect.y + 7, 24, 24), ToGUIContent(infos), StaticStyles.IconInfoSmall);
                GUI.Label(new Rect(rect.width - 32 + shift, rect.y + 11, 24, 24), infos.Count.ToString(), style.ruleBlockText);
            }

            if (replaceSource == block) {
                GUI.Box(rect, "", StaticStyles.RuleBlockDragDropFrame);
            }
        }

        public static string GetFlowDisplayName(Type type) {
            if (type == null)
                return "Start";
            string name = type.Name;
            if (type.IsIList())
                name = type.GetGenericArguments()[0].Name + "s";
            return name;
        }

        public static GUIContent ToGUIContent(IList<string> messages) {
            if (messages == null || messages.Count == 0)
            if (messages == null || messages.Count == 0)
                return new GUIContent();
            return new GUIContent("", string.Join("\n", messages));
        }
    }
}