using System.Collections.Generic;
using System.Reflection;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Pixyz.Utils  {

    public static class ReflectionExtensions {

        /// <summary>
        /// Returns a field value from the property name and type using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetFieldValue<T>(this object obj, string fieldName) {
            return (T)obj.GetType().GetField(fieldName).GetValue(obj);
        }

        /// <summary>
        /// Sets a field value from the property name and type using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public static void SetFieldValue<T>(this object obj, string fieldName, T value) {
            obj.GetType().GetField(fieldName).SetValue(obj, value);
        }

        /// <summary>
        /// Returns a property value from the property name and type using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(this object obj, string propertyName) {
            return (T)obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }

        /// <summary>
        /// Sets a property value from the property name and type using reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue<T>(this object obj, string propertyName, T value) {
            obj.GetType().GetProperty(propertyName).SetValue(obj, value, null);
        }

        /// <summary>
        /// Returns all public instance field of a given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldInfo[] GetInstanceFields(this Type type) {
            return type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(x => !x.IsLiteral).ToArray();
        }

        /// <summary>
        /// Returns all derives types for a given Type in the given AppDomain
        /// </summary>
        /// <param name="appDomain"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Type> GetAllDerivedTypes<T>(this AppDomain appDomain) {
            Type baseType = typeof(T);
            var result = new List<Type>();
            Assembly[] assemblies = appDomain.GetAssemblies();
            for (int a = 0; a < assemblies.Length; a++) {
                Type[] types = assemblies[a].GetTypes();
                for (int t = 0; t < types.Length; t++) {
                    Type type = types[t];
                    if (baseType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract) {
                        result.Add(type);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns True if the given type has at least one Attribute of the given attribute type.
        /// </summary>
        /// <typeparam name="AttributeType">Attribute Type</typeparam>
        /// <param name="type">Object Type</param>
        /// <returns></returns>
        public static bool HasAttribute<AttributeType>(this Type type)
        {
            object[] attributes = type.GetCustomAttributes(typeof(AttributeType), true);
            return attributes.Length > 0;
        }

        /// <summary>
        /// Returns True if the given member has at least one Attribute of the given attribute type.
        /// </summary>
        /// <typeparam name="AttributeType">Attribute Type</typeparam>
        /// <param name="memberInfo">Member</param>
        /// <returns></returns>
        public static bool HasAttribute<AttributeType>(this MemberInfo memberInfo)
        {
            object[] attributes = memberInfo.GetCustomAttributes(typeof(AttributeType), false);
            return attributes.Length > 0;
        }

        /// <summary>
        /// Returns True if the given member has at least one Attribute of the given attribute type.
        /// </summary>
        /// <typeparam name="AttributeType">Attribute Type</typeparam>
        /// <param name="memberInfo">Member</param>
        /// <param name="attribute">Out parameter for attribute found</param>
        /// <returns></returns>
        public static bool HasAttribute<AttributeType>(this MemberInfo memberInfo, out AttributeType attribute)
        {
            object[] attributes = memberInfo.GetCustomAttributes(typeof(AttributeType), false);
            if (attributes.Length > 0) {
                attribute = (AttributeType)attributes[0];
                return true;
            }
            else {
                attribute = default(AttributeType);
                return false;
            }
        }

        /// <summary>
        /// Returns true if the given type is of the type IList.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIList(this Type type) {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>));
        }

        /// <summary>
        /// Returns an 0 size IList of the given type.
        /// This method is similar to new List&lt;T&gt; but allows to pass a type reference instead.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IList CreateListOfType(Type type) {
            Type genericListType = typeof(List<>).MakeGenericType(type);
            return (IList)Activator.CreateInstance(genericListType);
        }
    }
}