using UnityEngine;
using System;

namespace Pixyz.Import {

    [Serializable]
    public sealed class TransformVariant : MonoBehaviour {

        [SerializeField]
        private VariantsManager.TransformSwitch transformSwitch;
        [SerializeField]
        private string key;

        void OnEnable() {
            if (transformSwitch == null || transform == null) return;
            transformSwitch.variantEnabled(this);
        }

        public void SetSwitch(VariantsManager.TransformSwitch ts) {
            transformSwitch = ts;
        }

        public void SetKey(string k) {
            key = k;
        }
        public string GetKey() {
            return key;
        }
    }
}