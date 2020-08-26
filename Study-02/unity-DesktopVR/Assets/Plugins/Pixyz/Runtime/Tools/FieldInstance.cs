using Pixyz.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Pixyz.Tools {

    /// <summary>
    /// A FieldInstance is a class that wraps a FieldInfo and a reference. It can be seen as the representation on a field on one particular object instance.
    /// This enables an easy serialization / deserialization mechanism through dictionnaries.
    /// Dictionnaries are great as they allow to store multiple levels of data and structures such as structs, classes, arrays, or base types.
    /// Unity is bad at serializing multiple levels of data and doesn't handles polymorphism. Dictionnaries also fix thoses issues.
    /// </summary>
    public sealed class FieldInstance {

        public readonly FieldInfo fieldInfo;
        public readonly object reference;

        public readonly MethodInfo visibilityCheck;
        public readonly MethodInfo enablingCheck;

        public readonly string name;
        public readonly string tooltip;

        public FieldInstance(FieldInfo fieldInfo, object reference) {
            this.fieldInfo = fieldInfo;
            this.reference = reference;
        }

        public FieldInstance(FieldInfo fieldInfo, object reference, string visibilityCheck = null, string enablingCheck = null, string tooltip = null, string displayName = null) {

            this.fieldInfo = fieldInfo;
            this.reference = reference;
            this.tooltip = tooltip;
            this.name = displayName == null ? fieldInfo.Name.FancifyCamelCase() : displayName;

            if (!String.IsNullOrEmpty(visibilityCheck))
            {
                MethodInfo methodInfo = fieldInfo.DeclaringType.GetMethod(visibilityCheck, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (methodInfo.ReturnType == typeof(bool) && (methodInfo.GetParameters().Length == 0 || (methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(int))))
                    this.visibilityCheck = methodInfo;
                else
                    Debug.LogWarning("Visibility check method should be parameterless (or have juste one int parameter) and with a bool type return.");
            }
            if(!String.IsNullOrEmpty(enablingCheck))
            {
                MethodInfo methodInfoEnable = fieldInfo.DeclaringType.GetMethod(enablingCheck, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (methodInfoEnable.ReturnType == typeof(bool) && (methodInfoEnable.GetParameters().Length == 0 || (methodInfoEnable.GetParameters().Length == 1 && methodInfoEnable.GetParameters()[0].ParameterType == typeof(int))))
                    this.enablingCheck = methodInfoEnable;
                else
                    Debug.LogWarning("Enabling check method should be parameterless and with a bool type return.");
            }
        }

        public FieldInstance(FieldInfo fieldInfo, object reference, MethodInfo visibilityCheck, MethodInfo enablingCheck)
        {
            this.fieldInfo = fieldInfo;
            this.reference = reference;
            if (visibilityCheck.ReturnType == typeof(bool) && visibilityCheck.GetParameters().Length == 0)
                this.visibilityCheck = visibilityCheck;
            else
                Debug.LogWarning("Visibility check method should be parameterless and with a bool type return.");
            if(enablingCheck != null)
            {
                if (enablingCheck.ReturnType == typeof(bool))
                    this.enablingCheck = enablingCheck;
                else
                    Debug.LogWarning("Enabling check method should be parameterless and with a bool type return.");
            }
        }

        /// <summary>
        /// Returns the instance value
        /// </summary>
        public object value {
            get {
                return fieldInfo.GetValue(reference);
            }
            set {
                fieldInfo.SetValue(reference, value);
            }
        }

        /// <summary>
        /// Returns the field type
        /// </summary>
        public Type type => fieldInfo.FieldType;

        /// <summary>
        /// Fills the referenced dictionnary with serialized data (recursively if the type is complex)
        /// </summary>
        /// <param name="pairs"></param>
        public void serializeData(ref Dictionary<string, string> pairs) {
            this.Serialize(ref pairs, name, value, type);
        }

        /// <summary>
        /// Unloads the data of the given dictionnary into the object field instance
        /// </summary>
        /// <param name="pairs"></param>
        public void deserializeData(Dictionary<string, string> pairs) {
            value = this.Unserialize(ref pairs, name, type);
        }
    }
}
