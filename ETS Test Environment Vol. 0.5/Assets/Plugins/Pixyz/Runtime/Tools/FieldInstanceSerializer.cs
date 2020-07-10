using Pixyz.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Pixyz.Tools {

    public static class FieldInstanceSerializer {

        internal static UnityObjectToStringHandler GetAssetPath;
        internal static StringToStringHandler AssetPathToGUID;
        internal static TypePathToUnityObject LoadAssetAtPath;
        internal static StringToStringHandler GUIDToAssetPath;

        static FieldInstanceSerializer() {
            if (Application.isEditor) {
                GetAssetPath = (UnityObjectToStringHandler)Delegate.CreateDelegate(typeof(UnityObjectToStringHandler), Type.GetType("UnityEditor.AssetDatabase, UnityEditor").GetMethod("GetAssetPath", new[] { typeof(UnityObject) }));
                AssetPathToGUID = (StringToStringHandler)Delegate.CreateDelegate(typeof(StringToStringHandler), Type.GetType("UnityEditor.AssetDatabase, UnityEditor").GetMethod("AssetPathToGUID", new[] { typeof(string) }));
                LoadAssetAtPath = (TypePathToUnityObject)Delegate.CreateDelegate(typeof(TypePathToUnityObject), Type.GetType("UnityEditor.AssetDatabase, UnityEditor").GetMethod("LoadAssetAtPath", new[] { typeof(string), typeof(Type) }));
                GUIDToAssetPath = (StringToStringHandler)Delegate.CreateDelegate(typeof(StringToStringHandler), Type.GetType("UnityEditor.AssetDatabase, UnityEditor").GetMethod("GUIDToAssetPath", new[] { typeof(string) }));
            }
        }

        /// <summary>
        /// This should not be call called in runtime mode, so there is no issue with that.
        /// However, if the asset is a resource, we can still store the path on top of the guid
        /// so that we can unserialize it at runtime and keep track of that asset in the editor
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        private static string SerializeAsset(UnityObject asset)
        {
            if (GetAssetPath == null) {
                Debug.LogError("It is not possible to serialize an Unity Object at runtime !");
                return null;
            }
            string path = GetAssetPath(asset);
            // Transform Asset path to a Resources path
            string guid = AssetPathToGUID(path);
            if (asset != null && !path.ToLower().StartsWith("assets")) {
                Debug.LogWarning($"{asset.GetType().Name} '{asset.name}' is not serializable. Only objects present in the Asset folder can be used.");
            }
            return guid + '|' + path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializedValue"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static UnityObject UnserializeAsset(string serializedValue, Type type)
        {
            string[] split = serializedValue.Split('|');
            UnityObject asset = null;
            if (LoadAssetAtPath != null) {
                // Attempts loading asset from GUID
                asset = LoadAssetAtPath(GUIDToAssetPath(split[0]), type);
            }
            return asset;
        }

        /// <summary>
        /// Fills a dictionnary with values in string
        /// Multileveled
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void Serialize(this FieldInstance fieldInstance, ref Dictionary<string, string> properties, string path, object value, Type type)
        {
            string stringValue = null;
            switch (type.ToString()) {
                // Base types
                case "System.Boolean":
                    stringValue = ((bool)value).ToString();
                    break;
                case "System.Byte":
                    stringValue = ((byte)value).ToString();
                    break;
                case "System.Int16":
                    stringValue = ((short)value).ToString();
                    break;
                case "System.Int32":
                    stringValue = ((int)value).ToString();
                    break;
                case "System.Int64":
                    stringValue = ((long)value).ToString();
                    break;
                case "System.Single":
                    stringValue = ((float)value).ToString(CultureInfo.InvariantCulture);
                    break;
                case "System.Double":
                    stringValue = ((double)value).ToString(CultureInfo.InvariantCulture);
                    break;
                case "System.String":
                    stringValue = (string)value;
                    break;
                case "System.FilePath":
                    stringValue = (FilePath)value;
                    break;
                case "System.Object":
                    if (value != null)
                        fieldInstance.Serialize(ref properties, path, value, value.GetType());
                    return;
                case "UnityEngine.LayerMask":
                    stringValue = ((int)(LayerMask)value).ToString();
                    break;
                // More advanced types
                default:
                    if (type.IsSubclassOf(typeof(UnityObject))) {
                        stringValue = SerializeAsset((UnityObject)value);
                    }
                    else if (type.IsEnum) {
                        stringValue = ((int)value).ToString();
                    }
                    else if (type.IsArray) {
                        Array array = value as Array;
                        if (array != null) {
                            for (int i = 0; i < array.Length; i++) {
                                fieldInstance.Serialize(ref properties, path + "." + i, array.GetValue(i), type.GetElementType());
                            }
                            stringValue = array.Length.ToString();
                        }
                        else {
                            stringValue = "0";
                        }
                    }
                    else if (type.IsClass || type.IsValueType) {
                        var fields = type.GetInstanceFields().OrderBy(field => field.MetadataToken).ToArray();
                        for (int i = 0; i < fields.Length; i++) {
                            fieldInstance.Serialize(ref properties, path + "." + fields[i].Name, fields[i].GetValue(value), fields[i].FieldType);
                        }
                        stringValue = fields.Length.ToString();
                    }
                    break;
            }

            if (properties.ContainsKey(path)) {
                Debug.Log("props already exits " + path);
            }
            else {
                properties.Add(path, stringValue);
            }
        }

        /// <summary>
        /// Fills FieldInfo
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static object Unserialize(this FieldInstance fieldInstance, ref Dictionary<string, string> properties, string path, Type type)
        {
            string stringValue;
            if (!properties.TryGetValue(path, out stringValue))
                return null;

            switch (type.ToString()) {
                // Base types
                case "System.Boolean":
                    bool boolValue = false;
                    bool.TryParse(stringValue, out boolValue);
                    return boolValue;
                case "System.Byte":
                    byte byteValue = 0;
                    byte.TryParse(stringValue, out byteValue);
                    return byteValue;
                case "System.Int16":
                    short shortValue = 0;
                    short.TryParse(stringValue, out shortValue);
                    return shortValue;
                case "System.Int32":
                    int intValue = 0;
                    int.TryParse(stringValue, out intValue);
                    return intValue;
                case "System.Int64":
                    long longValue = 0;
                    long.TryParse(stringValue, out longValue);
                    return longValue;
                case "System.Single":
                    float floatValue = 0;
                    float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue);
                    return floatValue;
                case "System.Double":
                    double doubleValue = 0;
                    double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out doubleValue);
                    return doubleValue;
                case "System.String":
                    return stringValue;
                case "System.FilePath":
                    return (FilePath)stringValue;
                case "System.Object":
                    if (stringValue != null && fieldInstance.value != null)
                        return fieldInstance.Unserialize(ref properties, path, fieldInstance.value.GetType());
                    return null;
                case "UnityEngine.LayerMask":
                    int layerMaskValue = 0;
                    int.TryParse(stringValue, out layerMaskValue);
                    return (LayerMask)layerMaskValue;
                // More advanced types
                default:
                    if (type.IsSubclassOf(typeof(UnityObject))) {
                        return UnserializeAsset(stringValue, type);
                    }
                    else if (type.IsEnum) {
                        int index = 0;
                        int.TryParse(stringValue, out index);
                        var x = Enum.GetValues(type).OfType<object>().ToArray();
                        if (x.Length > index)
                            return Enum.GetValues(type).OfType<object>().ToArray()[index];
                        return null;
                    }
                    else if (type.IsArray) {
                        int index = 0;
                        int.TryParse(stringValue, out index);
                        Array array = Array.CreateInstance(type.GetElementType(), index);
                        for (int i = 0; i < index; i++) {
                            array.SetValue(fieldInstance.Unserialize(ref properties, path + "." + i, type.GetElementType()), i);
                        }
                        return array;
                    }
                    else if (type.IsClass || type.IsValueType) {
                        object obj = Activator.CreateInstance(type);
                        var fields = type.GetInstanceFields().OrderBy(field => field.MetadataToken).ToArray();
                        for (int i = 0; i < fields.Length; i++) {
                            fields[i].SetValue(obj, fieldInstance.Unserialize(ref properties, path + "." + fields[i].Name, fields[i].FieldType));
                        }
                        return obj;
                    }
                    break;
            }

            return null;
        }
    }
}