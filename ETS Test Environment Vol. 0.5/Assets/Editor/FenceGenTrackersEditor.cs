using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FenceGenTrackers))]
public class FenceGenTrackersEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FenceGenTrackers gen = (FenceGenTrackers)target;
        if (GUILayout.Button("Instantiate"))
        {
            //gen.Instantiate();
        }
    }
}
