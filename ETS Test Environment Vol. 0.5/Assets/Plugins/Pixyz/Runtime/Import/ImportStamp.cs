using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pixyz.Import {

    /// <summary>
    /// This component holds all information related to the Pixyz import.
    /// </summary>
    public sealed class ImportStamp : MonoBehaviour {

        /// <summary>
        /// The path to the source file.
        /// </summary>
        public string path {
            get {
                return _path;
            }
            set {
                if (_path == value)
                    return;
                _path = value;
            }
        }

        /// <summary>
        /// The absolute path to the source file. 
        /// This is different from the path when the path is relative to the Project directory.
        /// </summary>
        public string fullPath {
            get {
                string _fullPath = _path;
                if (!String.IsNullOrEmpty(_fullPath) && _fullPath.StartsWith("Assets"))
                    _fullPath = Application.dataPath + _fullPath.Substring(6);
                return _fullPath;
            }
        }

        /// <summary>
        /// The @link Pixyz.Tools.RuleSet @endlink used to import that file.
        /// This object is an editor-only feature, hence, type needs casting for use in Editor.
        /// </summary>
        public UnityEngine.Object rules {
            get {
                return _rules;
            }
            set {
                if (_rules == value)
                    return;
                _rules = value;
            }
        }

        /// <summary>
        /// The @link Pixyz.ImportSettings @endlink used to import that file.
        /// </summary>
        public ImportSettings importSettings {
            get {
                return _importSettings;
            }
            set {
                if (_importSettings == value)
                    return;
                _importSettings = value;
                _importSettings.locked = true;
            }
        }

        /// <summary>
        /// The time (in ticks) when the file was imported.
        /// </summary>
        public long importTime => _importTime;

        /// <summary>
        /// The duration (in ticks) it took to import that file.
        /// </summary>
        public long importDuration => _importDuration;

        /// <summary>
        /// The last write time (in ticks) of that file.
        /// </summary>
        public long lastWriteTime => _lastWriteTime;

        /// <summary>
        /// The size of the file (in bytes) the last time it was imported / synchronized.
        /// </summary>
        public long lastFileSize => _lastFileSize;

        /// <summary>
        /// True if the user was warned that the file was out of sync.
        /// </summary>
        public bool wasUserWarned;

        [SerializeField]
        private string _path;

        [SerializeField]
        private long _importTime;

        [SerializeField]
        private long _importDuration;

        [SerializeField]
        private long _lastWriteTime;

        [SerializeField]
        private long _lastFileSize;

        [SerializeField]
        private ImportSettings _importSettings;

        [SerializeField]
        private UnityEngine.Object _rules;

        /// <summary>
        /// Updates the import information.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="importDuration"></param>
        public void stamp(string filePath, long importDuration) {
            _path = filePath;
            string fp = fullPath;
            _lastWriteTime = File.GetLastWriteTimeUtc(fp).Ticks;
            _lastFileSize = new FileInfo(fp).Length;
            _importTime = DateTime.Now.Ticks;
            _importDuration = importDuration;
        }

        public void markOutOfDate() {
            _lastWriteTime = 0;
        }

        /// <summary>
        /// Change LOD Mode
        ///  Root mode : regroup all LODGroup of the hierarchy in one LODGroup (added to the gameObject of the importStamp).
        ///  Leaves mode : Split the LODGroup of the importStamp gameObject and share it on the LODGroups created on the parent of the leaves of the hierarchy.
        /// </summary>
        /// <param name="mode"></param>
        public void changeLODMode(LodGroupPlacement mode)
        {
            if (mode == LodGroupPlacement.LEAVES)
            {
                LODGroup lodGroup = gameObject.GetComponent<LODGroup>();
                Dictionary<LODGroup, Dictionary<float, List<Renderer>>> finalLods = new Dictionary<LODGroup, Dictionary<float, List<Renderer>>>();
                foreach (LOD lod in lodGroup.GetLODs())
                {
                    foreach (Renderer renderer in lod.renderers)
                    {
                        LODGroup parentLODGroup = renderer.transform.parent.GetComponent<LODGroup>();
                        if (parentLODGroup == null)
                            parentLODGroup = renderer.transform.parent.gameObject.AddComponent<LODGroup>();
                        if (!finalLods.ContainsKey(parentLODGroup))
                            finalLods.Add(parentLODGroup, new Dictionary<float, List<Renderer>>());
                        if (!finalLods[parentLODGroup].ContainsKey(lod.screenRelativeTransitionHeight))
                            finalLods[parentLODGroup].Add(lod.screenRelativeTransitionHeight, new List<Renderer>());
                        finalLods[parentLODGroup][lod.screenRelativeTransitionHeight].Add(renderer);
                    }
                }
                UnityEngine.Object.DestroyImmediate(lodGroup);
                foreach (var groupPair in finalLods)
                {
                    List<LOD> lods = new List<LOD>();
                    foreach (var pair in groupPair.Value)
                    {
                        lods.Add(new LOD(pair.Key, pair.Value.ToArray()));
                    }
                    lods.Sort(delegate (LOD x, LOD y)
                    {
                        if (x.screenRelativeTransitionHeight < y.screenRelativeTransitionHeight) return 1;
                        else if (x.screenRelativeTransitionHeight == y.screenRelativeTransitionHeight) return 0;
                        else return -1;
                    });
                    groupPair.Key.SetLODs(lods.ToArray());
                }
            }
            // move LODGroup from leaves to root
            else
            {
                Dictionary<float, List<Renderer>> newLods = new Dictionary<float, List<Renderer>>();
                foreach (LODGroup lodGroup in gameObject.GetComponentsInChildren<LODGroup>())
                {
                    foreach (LOD lod in lodGroup.GetLODs())
                    {
                        if (!newLods.ContainsKey(lod.screenRelativeTransitionHeight))
                            newLods.Add(lod.screenRelativeTransitionHeight, new List<Renderer>());
                        newLods[lod.screenRelativeTransitionHeight].AddRange(lod.renderers);
                    }
                    UnityEngine.Object.DestroyImmediate(lodGroup);
                }
                LODGroup parentLODGroup = gameObject.AddComponent<LODGroup>();
                List<LOD> lods = new List<LOD>();
                foreach (KeyValuePair<float, List<Renderer>> pair in newLods)
                {
                    lods.Add(new LOD(pair.Key, pair.Value.ToArray()));
                }
                lods.Sort(delegate (LOD x, LOD y)
                {
                    if (x.screenRelativeTransitionHeight < y.screenRelativeTransitionHeight) return 1;
                    else if (x.screenRelativeTransitionHeight == y.screenRelativeTransitionHeight) return 0;
                    else return -1;
                });
                parentLODGroup.SetLODs(lods.ToArray());
            }
        }
    }
}