using Pixyz.Editor;
using Pixyz.Import;
using Pixyz.Utils;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pixyz.Tools.Editor {

    public static class FieldInstanceDrawer {

        public static void DrawGUILayout(this FieldInstance[] fieldInstances) {
            for (int a = 0; a < fieldInstances.Length; a++) {

                ///block.properties[a].type = block.action.fieldInstances[a].FieldType;
                FieldInstance fieldInstance = fieldInstances[a];
                if (fieldInstance.visibilityCheck != null && fieldInstance.visibilityCheck.GetParameters().Length == 0)
                    if (!(bool)fieldInstance.visibilityCheck.Invoke(fieldInstance.reference, new object[0]))
                        continue;
                GUILayout.Space(4);
                if (fieldInstance.enablingCheck != null && fieldInstance.enablingCheck.GetParameters().Length == 0)
                    EditorGUI.BeginDisabledGroup(!(bool)fieldInstance.enablingCheck.Invoke(fieldInstance.reference, new object[0]));
                fieldInstance.DrawGUILayout();
                if (fieldInstance.enablingCheck != null)
                    EditorGUI.EndDisabledGroup();
            }
        }

        public static void DrawGUILayout(this FieldInstance fieldInstance) {

            var name = fieldInstance.name;

            var value = fieldInstance.value;
            var type = fieldInstance.fieldInfo.FieldType;
            var tooltip = fieldInstance.tooltip;

            /// Base types
            switch (type.ToString()) {
                case "System.Boolean":
                    fieldInstance.value = EditorGUILayout.Toggle(new GUIContent(name, tooltip), (bool)value);
                    return;
                case "System.Byte":
                    fieldInstance.value = EditorGUILayout.IntField(new GUIContent(name, tooltip), (int)value);
                    return;
                case "System.Int16":
                    fieldInstance.value = EditorGUILayout.IntField(new GUIContent(name, tooltip), (short)value);
                    return;
                case "System.Int32":
                    fieldInstance.value = EditorGUILayout.IntField(new GUIContent(name, tooltip), (int)value);
                    return;
                case "System.Int64":
                    fieldInstance.value = EditorGUILayout.LongField(new GUIContent(name, tooltip), (long)value);
                    return;
                case "System.Single":
                    fieldInstance.value = EditorGUILayout.FloatField(new GUIContent(name, tooltip), (float)value);
                    return;
                case "System.Double":
                    fieldInstance.value = EditorGUILayout.DoubleField(new GUIContent(name, tooltip), (double)value);
                    return;
                case "System.String":
                    fieldInstance.value = EditorGUILayout.TextField(new GUIContent(name, tooltip), (string)value);
                    return;
                case "Pixyz.Utils.FilePath":
                    EditorGUILayout.BeginHorizontal();
                    if (!string.IsNullOrEmpty(name))
                        fieldInstance.value = (FilePath)EditorGUILayout.TextField(new GUIContent(name, tooltip), (FilePath)value);
                    else
                        fieldInstance.value = (FilePath)EditorGUILayout.TextField((FilePath)value);

                    FilterParameter filterParameter;
                    string[] filter = Formats.SupportedFormatsForFileBrowser;
                    if (fieldInstance.fieldInfo.HasAttribute(out filterParameter))
                    {
                        if (filterParameter.filter != null && filterParameter.filter.Length != 0)
                        {
                            filter = filterParameter.filter;
                        }
                    }
                    if (GUILayout.Button("...", UnityEditor.EditorStyles.miniButton, GUILayout.Width(25)))
                    {
                        fieldInstance.value = (FilePath)EditorExtensions.SelectFile(filter);
                    }
                    EditorGUILayout.EndHorizontal();
                    return;
                case "Pixyz.Utils.Range":
                    //Use GUIUtility.hotControl to detect slider movement
                    fieldInstance.value = (Range)EditorGUILayout.Slider(name, (Range)value, 0f, 100f);
                    return;
                case "System.Object":
                    if (value != null) {
                        fieldInstance.value = DrawGUILayout2(value, value.GetType());
                    }
                    return;
                case "UnityEngine.LayerMask":
                    fieldInstance.value = (LayerMask)EditorGUILayout.LayerField(new GUIContent(name, tooltip), (LayerMask)value);
                    return;
                case "UnityEngine.Color":
                    fieldInstance.value = (Color)EditorGUILayout.ColorField(new GUIContent(name, tooltip), (Color)value);
                    return;
            }

            /// More advanced types
            if (type.IsSubclassOf(typeof(UnityEngine.Object))) {
                fieldInstance.value = EditorGUILayout.ObjectField(new GUIContent(name, tooltip), (UnityEngine.Object)value, fieldInstance.type, true);
            }
            else if (type.IsEnum) {
                int indexInEnum = Array.IndexOf(Enum.GetValues(type), value);
                int newIndexInEnum = EditorGUILayout.Popup(new GUIContent(name, tooltip), indexInEnum, Enum.GetValues(type).OfType<object>().Select(o => o.ToString().FancifyCamelCase()).ToArray());
                var newEnumValue = Enum.GetValues(type).GetValue(newIndexInEnum);
                fieldInstance.value = newEnumValue;
            }
            else if (type.IsArray) {
                Type elementType = type.GetElementType();
                Array array = value as Array;
                if (array == null) {
                    array = Array.CreateInstance(type.GetElementType(), 0);
                    fieldInstance.value = array;
                }
                int size = array.Length;
                if (!string.IsNullOrEmpty(name))
                    EditorGUILayout.LabelField(name);
                Rect rect = EditorGUILayout.BeginVertical(StaticStyles.ListDetail);
                int offset = 0;
                /// Draw headers
                if (!IsBaseType(elementType) && elementType.IsValueType) {
                    var ffields = elementType.GetInstanceFields();
#if UNITY_2019_1_OR_NEWER
                    EditorGUILayout.Space(18);
#else
                    GUILayout.Space(18);
#endif
                    for (int f = 0; f < ffields.Length; f++) {
                        Rect fieldRect = new Rect(rect.x + 10 + f * (rect.width - 45) / ffields.Length, rect.y + 6, (rect.width - 45) / ffields.Length, 16);
                        EditorGUI.LabelField(fieldRect, ffields[f].Name.FancifyCamelCase());
                    }
                    offset += 18;
                }
                /// Draw value setters
                for (int i = 0; i < size; i++) {
                    var child = array.GetValue(i);
                    if (child == null) {
                        //child = Activator.CreateInstance(elementType,
                        //    BindingFlags.CreateInstance |
                        //     BindingFlags.Public |
                        //     BindingFlags.Instance |
                        //     BindingFlags.OptionalParamBinding,
                        //     null, new object[] { Type.Missing }, null);
                        child = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(elementType);
                        array.SetValue(child, i);
                        fieldInstance.value = array;
                    }
                    array.SetValue(child.DrawGUILayout2(elementType), i);
                    fieldInstance.value = array;
                    /// Draw a - button
                    if (EditorGUIExtensions.DrawButton(new Rect(rect.x + rect.width - 20, rect.y + i * (rect.height - 23 - offset) / size + 5 + offset, 14, 14), StaticStyles.IconMinusSmall)) {
                        /// Swaps sub properties from the removed element to the end, then remove the last element
                        for (int k = i; k < size - 1; k++) {
                            var tmp = array.GetValue(k);
                            array.SetValue(array.GetValue(k + 1), k);
                            array.SetValue(tmp, k + 1);
                        }
                        array = array.Resize(size - 1);
                        fieldInstance.value = array;
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUILayout.EndVertical();
                /// Draw a + button
                if (EditorGUIExtensions.DrawButton(new Rect(rect.x + rect.width - 20, rect.y + rect.height - 18, 14, 14), StaticStyles.IconPlusSmall)) {
                    array = array.Resize(size + 1);
                    fieldInstance.value = array;
                }
            }
            else if (type.IsClass || type.IsValueType) {
                /// Makes room for the layout which is actually going to be composed manually
                Rect structRect = EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(name);
                EditorGUILayout.EndHorizontal();
                int shift = (int)EditorGUIUtility.labelWidth;
                structRect.x += shift;
                structRect.width -= shift; // Additional margin to give more space
                var fields = type.GetInstanceFields();
                for (int f = 0; f < fields.Length; f++) {
                    fields[f].Name.FancifyCamelCase();
                    Rect fieldRect = new Rect(structRect.x + f * structRect.width / fields.Length, structRect.y, structRect.width / fields.Length - 5, 16);
                    object fieldValue = DrawGUI(fields[f].GetValue(value), fields[f].FieldType, fieldRect);
                    fields[f].SetValue(value, fieldValue);
                }
                /// If the type is a struct, the struct was copied, so we need to set it back.
                if (type.IsValueType) {
                    fieldInstance.value = value;
                }
            }
        }

        public static object DrawGUILayout2(this object value, Type type) {

            /// Base types
            switch (type.ToString()) {
                case "System.Boolean":
                    return EditorGUILayout.Toggle((bool)value);
                case "System.Byte":
                    return EditorGUILayout.IntField((int)value);
                case "System.Int16":
                    return EditorGUILayout.IntField((short)value);
                case "System.Int32":
                    return EditorGUILayout.IntField((int)value);
                case "System.Int64":
                    return EditorGUILayout.LongField((long)value);
                case "System.Single":
                    return EditorGUILayout.FloatField((float)value);
                case "System.Double":
                    return EditorGUILayout.DoubleField((double)value);
                case "System.String":
                    return EditorGUILayout.TextField((string)value);
                case "Pixyz.Utils.FilePath":
                    EditorGUILayout.BeginHorizontal();
                    value = (FilePath)EditorGUILayout.TextField((FilePath)value);
                    if (GUILayout.Button("...", UnityEditor.EditorStyles.miniButton, GUILayout.Width(25))) {
                        value = (FilePath)EditorExtensions.SelectFile(Formats.SupportedFormatsForFileBrowser);
                    }
                    EditorGUILayout.EndHorizontal();
                    return value;
                case "Pixyz.Utils.Range":
                    return EditorGUILayout.Slider((Range)value, 0f, 100f);
                case "UnityEngine.LayerMask":
                    return (LayerMask)EditorGUILayout.LayerField((LayerMask)value);
                case "Pixyz.Utils.DynamicEnum":
                    var de = (DynamicEnum)value;
                    de.index = EditorGUILayout.Popup(de.index, de.ToArray());
                    return de;
            }

            /// More advanced types
            if (type.IsSubclassOf(typeof(UnityEngine.Object))) {
                return EditorGUILayout.ObjectField((UnityEngine.Object)value, type, true);
            }
            else if (type.IsEnum) {
                return EditorGUILayout.Popup(Convert.ToInt32(value), Enum.GetValues(type).OfType<object>().Select(o => o.ToString().FancifyCamelCase()).ToArray());
            }
            else if (type.IsArray) {
                Type elementType = type.GetElementType();
                Array array = value as Array;
                if (array == null) {
                    array = Array.CreateInstance(type.GetElementType(), 0);
                    value = array;
                }
                int size = array.Length;
                //EditorGUILayout.LabelField(name);
                Rect rect = EditorGUILayout.BeginVertical(StaticStyles.ListDetail);
                int offset = 0;
                /// Draw headers
                if (!IsBaseType(elementType)) {
                    var ffields = elementType.GetInstanceFields();
                    EditorGUILayout.LabelField("");
                    for (int f = 0; f < ffields.Length; f++) {
                        Rect fieldRect = new Rect(rect.x + 10 + f * (rect.width - 45) / ffields.Length, rect.y + 6, (rect.width - 45) / ffields.Length, 16);
                        EditorGUI.LabelField(fieldRect, ffields[f].Name.FancifyCamelCase());
                    }
                    offset += 18;
                }
                /// Draw value setters
                for (int i = 0; i < size; i++) {
                    var child = array.GetValue(i);
                    if (child == null) {
                        //child = Activator.CreateInstance(elementType,
                        //    BindingFlags.CreateInstance |
                        //     BindingFlags.Public |
                        //     BindingFlags.Instance |
                        //     BindingFlags.OptionalParamBinding,
                        //     null, new object[] { Type.Missing }, null);
                        child = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(elementType);
                        array.SetValue(child, i);
                        value = array;
                    }
                    array.SetValue(child.DrawGUILayout2(elementType), i);
                    value = array;
                    /// Draw a - button
                    if (EditorGUIExtensions.DrawButton(new Rect(rect.x + rect.width - 20, rect.y + i * (rect.height - 23 - offset) / size + 5 + offset, 14, 14), StaticStyles.IconMinusSmall)) {
                        /// Swaps sub properties from the removed element to the end, then remove the last element
                        for (int k = i; k < size - 1; k++) {
                            var tmp = array.GetValue(k);
                            array.SetValue(array.GetValue(k + 1), k);
                            array.SetValue(tmp, k + 1);
                        }
                        return value = array = array.Resize(size - 1);
                    }
                }
                EditorGUILayout.EndVertical();
                /// Draw a + button
                if (EditorGUIExtensions.DrawButton(new Rect(rect.x + rect.width - 20, rect.y + rect.height - 18, 14, 14), StaticStyles.IconPlusSmall)) {
                    array = array.Resize(size + 1);
                    value = array;
                }
                return value;
            }
            else if (type.IsClass || type.IsValueType) {
                /// Makes to room for the layout which is actually going to be composed manually
                Rect structRect = EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("");
                EditorGUILayout.EndHorizontal();
                var fields = type.GetInstanceFields();
                for (int f = 0; f < fields.Length; f++) {
                    fields[f].Name.FancifyCamelCase();
                    Rect fieldRect = new Rect(structRect.x + f * structRect.width / fields.Length, structRect.y, structRect.width / fields.Length, 16);
                    fields[f].SetValue(value, DrawGUI(fields[f].GetValue(value), fields[f].FieldType, fieldRect));
                }
                return value;
            }

            throw new Exception($"Type '{type}' isn't supported.");
        }

        public static object DrawGUI(this object value, Type type, Rect rect) {

            /// Base types
            switch (type.ToString()) {
                case "System.Boolean":
                    return EditorGUI.Toggle(rect, (bool)value);
                case "System.Byte":
                    return EditorGUI.IntField(rect, (int)value);
                case "System.Int16":
                    return EditorGUI.IntField(rect, (short)value);
                case "System.Int32":
                    return EditorGUI.IntField(rect, (int)value);
                case "System.Int64":
                    return EditorGUI.LongField(rect, (long)value);
                case "System.Single":
                    return EditorGUI.FloatField(rect, (float)value);
                case "System.Double":
                    return EditorGUI.DoubleField(rect, (double)value);
                case "System.String":
                    return EditorGUI.TextField(rect, (string)value);
                case "Pixyz.Utils.FilePath":
                    value = (FilePath)EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width - 35, rect.height), (FilePath)value);
                    if (GUI.Button(new Rect(rect.x + rect.width - 30, rect.y, 30, rect.height), "...", UnityEditor.EditorStyles.miniButton)) {
                        value = (FilePath)EditorExtensions.SelectFile(new string[] { "All", "*" });
                    }
                    return value;
                case "Pixyz.Utils.Range":
                    return EditorGUI.Slider(rect, (Range)value, 0f, 100f);
                case "UnityEngine.LayerMask":
                    return (LayerMask)EditorGUI.LayerField(rect, (LayerMask)value);
            }

            /// More advanced types
            if (type.IsSubclassOf(typeof(UnityEngine.Object))) {
                return EditorGUI.ObjectField(rect, (UnityEngine.Object)value, type, true);
            }
            else if (type.IsEnum) {
                return EditorGUI.Popup(rect, Convert.ToInt32(value), Enum.GetValues(type).OfType<object>().Select(o => o.ToString().FancifyCamelCase()).ToArray());
            }
            else if (type.IsArray) {
                throw new NotImplementedException("Can't display a non laid out array");
            }
            else if (type.IsClass || type.IsValueType) {
                throw new NotImplementedException("Can't display a non laid out class or struct");
            }

            throw new Exception($"Type '{type}' isn't supported.");
        }

        public static bool IsBaseType(Type type) {

            switch (type.FullName) {
                case "System.Boolean":
                case "System.Byte":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Single":
                case "System.Double":
                case "System.String":
                case "System.Uri":
                case "Pixyz.Utils.FilePath":
                case "Pixyz.Utils.Range":
                case "UnityEngine.Color":
                case "UnityEngine.Vector2":
                case "UnityEngine.Vector3":
                case "UnityEngine.Vector4":
                case "UnityEngine.LayerMask":
                    return true;
            }
            return false;
        }
    }
}