using System;
using System.Collections.Generic;

namespace Pixyz.Utils  {

    public struct Range {

        public int value;

        public static implicit operator Range(int value) {
            return new Range { value = value };
        }

        public static implicit operator int(Range value) {
            return value.value;
        }
    }
}