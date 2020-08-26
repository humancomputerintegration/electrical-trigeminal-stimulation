using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class AnsysManager : MonoBehaviour
{

    public Vector3 cornerA = new Vector3(-4f, 0f, -6f); // world position
    public Vector3 cornerB = new Vector3(4f, 3.5f, 6f); // world position
    private Vector3 lowestBVHposition;
    public int divisions = 7;
    public GameObject boundingVolumePrefab;

    private Vector3 volumeDimensions;

    // Start is called before the first frame update
    void Awake()
    {
        lowestBVHposition = new Vector3(
            Mathf.Min(cornerA.x, cornerB.x),
            Mathf.Min(cornerA.y, cornerB.y),
            Mathf.Min(cornerA.z, cornerB.z)
            );
        volumeDimensions = new Vector3(
            Mathf.Abs(cornerA.x - cornerB.x) / (float)divisions,
            Mathf.Abs(cornerA.y - cornerB.y) / (float)divisions,
            Mathf.Abs(cornerA.z - cornerB.z) / (float)divisions
            );

        InitializeBoundingVolumes();
    }

    void InitializeBoundingVolumes()
    {
        string boundingVolumesDirectory = Application.streamingAssetsPath + "/AnsysBoundingVolumes";
        string[] dir = Directory.GetDirectories(boundingVolumesDirectory);
        foreach (string d in dir)
        {
            string isolatedName = d.Remove(0, boundingVolumesDirectory.Length + 1);
            char[] separators = { '_' };
            string[] indexS = isolatedName.Split(separators);
            int[] indexI = new int[3];
            for(int i = 1; i < 4; i++)
            {
                string num = Regex.Replace(indexS[i], "[^.0-9]", "");
                indexI[i-1] = Int32.Parse(num);
            }

            GameObject currVolume = Instantiate(boundingVolumePrefab);
            currVolume.transform.position = new Vector3(indexI[0] * volumeDimensions.x, indexI[1] * volumeDimensions.y, indexI[2] * volumeDimensions.z) + lowestBVHposition;
            currVolume.transform.parent = this.transform;
            currVolume.tag = "Plume";
            BoxCollider currCollider = currVolume.GetComponent<BoxCollider>();
            currCollider.size = volumeDimensions;
            currCollider.center = 0.5f * volumeDimensions;
            currVolume.name = "PVB (" + indexI[0].ToString() + "," + indexI[1].ToString() + "," + indexI[2].ToString() + ")";

            ParticleSystem.ShapeModule currPS = currVolume.GetComponent<ParticleSystem>().shape;
            currPS.position = 0.5f * volumeDimensions;
            currPS.scale = volumeDimensions;

            BoundingVolume currBV = currVolume.GetComponent<BoundingVolume>();
            currBV.volumeI = indexI[0];
            currBV.volumeJ = indexI[1];
            currBV.volumeK = indexI[2];
            currBV.InitializeVolume();

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
