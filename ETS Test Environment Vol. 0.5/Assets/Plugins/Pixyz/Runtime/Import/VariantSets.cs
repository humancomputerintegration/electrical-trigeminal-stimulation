using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Pixyz.Utils;
using System.Xml;

namespace Pixyz.Import {

    [Serializable]
    public sealed class VariantSets : MonoBehaviour {

        [HideInInspector]
        public VariantsManager variantsManager;

        [SerializeField]
        List<VariantGroup> variantGroups = new List<VariantGroup>();
        public List<VariantGroup> VariantGroups { get { return variantGroups; } }

        [Serializable]
        public abstract class Set {
            [HideInInspector]
            public string type;
            [SerializeField] [HideInInspector]
            VariantsManager variantsManager;
            
            public Set(VariantsManager vm, string t) { variantsManager = vm; type = t; }
            public abstract bool select();
        }

        [Serializable]
        public class Set<T, U> : Set where T : VariantsManager.Switch<U> {
            [SerializeField] [HideInInspector]
            public T switchObject;
            public U state;

            public Set(VariantsManager vm, string t, string switch_name, string variant_name, List<T> switchList) : base(vm, t) {

                foreach (T s in switchList) {
                    if (s.name == switch_name) { switchObject = s; break; }
                }
                if (switchObject == null) throw new InvalidOperationException();

                foreach (U v in switchObject.getVariants()) {
                    string name = v.GetPropertyValue<string>("name");
                    if (name == variant_name) { state = v; break; }
                }
                if (state == null) throw new InvalidOperationException();
            }   
            
            override public bool select()
            {
                bool value = switchObject.selectVariant(state);
                return value;
            }
        }

        [Serializable]
        public class TransformSet : Set<VariantsManager.TransformSwitch, TransformVariant> {
            public TransformSet(VariantsManager vm, string t, string sn, string vn, List<VariantsManager.TransformSwitch> sl) : base(vm, t, sn, vn, sl) { }
        }
        [Serializable]
        public class MaterialSet : Set<VariantsManager.MaterialSwitch, Material> {
            public MaterialSet(VariantsManager vm, string t, string sn, string vn, List<VariantsManager.MaterialSwitch> sl) : base(vm, t, sn, vn, sl) { }
        }

        [Serializable]
        public abstract class VariantStructure {
            [HideInInspector]
            public string name;
            public VariantStructure(string n) { name = n; }
        }

        [Serializable]
        public class VariantSet : VariantStructure {
            [SerializeField]
            public List<TransformSet> transformSets;
            [SerializeField]
            public List<MaterialSet> materialSets;

            public VariantSet(string n) : base(n) { transformSets = new List<TransformSet>(); materialSets = new List<MaterialSet>(); }

            public void select() {
                if (transformSets == null) return; foreach (var set in transformSets) set.select();
                if (materialSets == null) return; foreach (var set in materialSets) set.select();
            }
        }

        [Serializable]
        public class VariantGroup : VariantStructure {
            public List<VariantSet> variantSets = new List<VariantSet>();
            public VariantGroup(string n) : base(n) { }
        }

        private T getElemWithName<T>(List<T> list, string n) where T : VariantStructure
        {
            foreach (var elem in list) {
                if (elem.name == n) { return elem; }
            }
            return null;
        }

        public void initVariantSets(string variantSetsFile)
        {
            variantsManager = GetComponent<VariantsManager>();
            if (variantsManager == null) return;

            List<string> groups = new List<string>();
            List<string> indeSets = new List<string>();

            if (!File.Exists(variantSetsFile)) return;

            // Read variant sets xml
            XmlDocument xml = new XmlDocument();
            xml.Load(variantSetsFile);

            foreach (XmlNode groupNode in xml.DocumentElement.ChildNodes) {
                if (groupNode.Name == "VariantGroup") {
                    string groupName = ((XmlElement)groupNode).GetAttribute("name");

                    // New group? If so, init
                    VariantGroup vg = getElemWithName(variantGroups, groupName);
                    if (vg == null) { vg = new VariantGroup(groupName); variantGroups.Add(vg); }

                    foreach (XmlNode setNode in groupNode.ChildNodes) {
                        if (setNode.Name != "VariantSet") continue;
                        string setName = ((XmlElement)setNode).GetAttribute("name");

                        // New set? If so, init
                        VariantSet vs = getElemWithName(vg.variantSets, setName);
                        if (vs == null) { vs = new VariantSet(setName); vg.variantSets.Add(vs); }

                        foreach (XmlNode variantNode in setNode.ChildNodes) {
                            string variantName = ((XmlElement)variantNode).GetAttribute("state");
                            string switchName = ((XmlElement)variantNode).GetAttribute("ref");
                            if (variantNode.Name == "NodeVariantRef") {
                                // Init new transform set
                                initTransformSet(vs, variantsManager, "TransformVariant", switchName, variantName);
                            } else if (variantNode.Name == "MaterialVariantRef") {
                                initMaterialSet(vs, variantsManager, "MaterialVariant", switchName, variantName);
                            }
                        }
                    }
                }
                else if (groupNode.Name == "VariantSet") {
                    string setName = ((XmlElement)groupNode).GetAttribute("name");

                    VariantGroup vg = getElemWithName(variantGroups, "");
                    if (vg == null) { vg = new VariantGroup(""); variantGroups.Add(vg); }

                    // New set? If so, init
                    VariantSet vs = getElemWithName(vg.variantSets, setName);
                    if (vs == null) { vs = new VariantSet(setName); vg.variantSets.Add(vs); }

                    foreach (XmlNode variantNode in groupNode.ChildNodes) {
                        string variantName = ((XmlElement)variantNode).GetAttribute("state");
                        string switchName = ((XmlElement)variantNode).GetAttribute("ref");

                        if (variantNode.Name == "NodeVariantRef") {
                            initTransformSet(vs, variantsManager, "TransformVariant", switchName, variantName);
                        } else if (variantNode.Name == "MaterialVariantRef") {
                            initMaterialSet(vs, variantsManager, "MaterialVariant", switchName, variantName);
                        }
                    }
                }
            }
            cleanGroups();
        }

        private void initTransformSet(VariantSet vs, VariantsManager vm, string type, string switch_name, string variant_name)
        {
            List<VariantsManager.TransformSwitch> list = vm.TransformSwitchList;
            TransformSet set = null;
            try { set = new TransformSet(vm, type, switch_name, variant_name, list); } catch { return; }
            vs.transformSets.Add(set);
        }

        private void initMaterialSet(VariantSet vs, VariantsManager vm, string type, string switch_name, string variant_name)
        { 
            List<VariantsManager.MaterialSwitch> list = vm.MaterialSwitchList;
            MaterialSet set = null;
            try { set = new MaterialSet(vm, type, switch_name, variant_name, list); }  catch { return; }
            vs.materialSets.Add(set);
        }

        private void cleanGroups()
        {
            for (int i = variantGroups.Count - 1; i >= 0; i--) {
                var variantSets = variantGroups[i].variantSets;
                for (int j = variantSets.Count - 1; j >= 0; j--) {
                    var vs = variantSets[j];
                    if (vs.transformSets.Count + vs.materialSets.Count == 0) {
                        variantSets.RemoveAt(j);
                    }
                }
                if (variantSets.Count == 0) {
                    variantGroups.RemoveAt(i);
                }
            }
        }
    }
}