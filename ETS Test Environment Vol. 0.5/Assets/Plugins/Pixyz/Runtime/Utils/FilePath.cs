using System;
using System.Collections.Generic;

namespace Pixyz.Utils  {

    public struct FilePath {

        public FilePath(string str) {
            if (str == null)
                str = String.Empty;
            value = str;
        }

        private string value;

        static public implicit operator FilePath(string str) {
            return new FilePath(str);
        }

        static public implicit operator string(FilePath path) {
            return path.value;
        }

        public override string ToString() {
            return value;
        }
    }

    /// <summary>
    /// A DynamicEnum that uses a String values. This is used for making enum-like dropdowns with variable options in the Unity Editor.
    /// This can be used for example in Tool actions UIs for making them dynamic
    /// </summary>
    public class DynamicEnum : List<string> {

        public int index = 0;

        public void setValues(params string[] values) {
            Clear();
            AddRange(values);
        }
    }
}