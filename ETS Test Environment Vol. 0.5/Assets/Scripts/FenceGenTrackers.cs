using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class FenceGenTrackers : MonoBehaviour {
    
    Vector3[] fenceCorners = new Vector3[4];
    List<GameObject> columnObjects = new List<GameObject>();
    public Transform steamRig;


    List<uint> baseStationIndices = new List<uint>();
    GameObject[] baseStations = new GameObject[2];

    public int numberColumnsPerSide = 4;

    public GameObject instantiatedObj;

    public Bounds fenceBounds;

    void Start ()
    {
        StartCoroutine(Instantiate());
    }

    IEnumerator Instantiate ()
    {
        InstantiateBaseStations();

        while ( !baseStations[0].GetComponent<SteamVR_TrackedObject>().isValid || !baseStations[1].GetComponent<SteamVR_TrackedObject>().isValid )
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("Starting 2");
        transform.position = steamRig.position;

        foreach (GameObject b in baseStations)
        {
            Debug.Log(b.transform.localPosition.ToString());
        }

        fenceCorners[0] = new Vector3(
            Mathf.Max(baseStations[0].transform.localPosition.x, baseStations[1].transform.localPosition.x),
            0,
            Mathf.Max(baseStations[0].transform.localPosition.z, baseStations[1].transform.localPosition.z)
            );

        fenceCorners[1] = new Vector3(
            Mathf.Max(baseStations[0].transform.localPosition.x, baseStations[1].transform.localPosition.x),
            0,
            Mathf.Min(baseStations[0].transform.localPosition.z, baseStations[1].transform.localPosition.z)
            );

        fenceCorners[3] = new Vector3(
            Mathf.Min(baseStations[0].transform.localPosition.x, baseStations[1].transform.localPosition.x),
            0,
            Mathf.Max(baseStations[0].transform.localPosition.z, baseStations[1].transform.localPosition.z)
            );

        fenceCorners[2] = new Vector3(
            Mathf.Min(baseStations[0].transform.localPosition.x, baseStations[1].transform.localPosition.x),
            0,
            Mathf.Min(baseStations[0].transform.localPosition.z, baseStations[1].transform.localPosition.z)
            );

        fenceBounds = new Bounds(Vector3.zero, Vector3.zero);

        for (int cornerN = 0; cornerN < 4; cornerN++)
        {
            fenceBounds.Encapsulate(fenceCorners[cornerN]);
            for (int columnI = 0; columnI < numberColumnsPerSide; columnI++) {
                GameObject newColumn = (GameObject)Instantiate(instantiatedObj);
                newColumn.transform.parent = transform;
                float t = ((float)columnI) / ((float)numberColumnsPerSide);
                newColumn.transform.localPosition = Vector3.Lerp(fenceCorners[cornerN], fenceCorners[(cornerN+1) % 4], t);
                //newColumn.transform.LookAt(transform, Vector3.up);
                columnObjects.Add(newColumn);
            }
        }
        
        fenceBounds.size = new Vector3(fenceBounds.size.x, 8f, fenceBounds.size.z);

        gameObject.GetComponent<BoxCollider>().size = fenceBounds.size;

    }


    // Thank you to Liam Ferris. https://stackoverflow.com/questions/43184610/how-to-determine-whether-a-steamvr-trackedobject-is-a-vive-controller-or-a-vive

    
    void InstantiateBaseStations() {
        var error = ETrackedPropertyError.TrackedProp_Success;
        for (uint i = 0; i < 16; i++)
        {
            var result = new System.Text.StringBuilder((int)64);
            OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);

            if (result.ToString().Contains("station"))
            {
                baseStationIndices.Add(i);
            } else if (baseStationIndices.Count == 2)
            {
                break;
            }
        }

        for (int i = 0; i < 2; i++)
        {
            baseStations[i] = new GameObject("Base station " + baseStationIndices[i].ToString());
            baseStations[i].transform.parent = steamRig.transform;
            SteamVR_TrackedObject bTrack = baseStations[i].AddComponent<SteamVR_TrackedObject>() as SteamVR_TrackedObject;
            bTrack.SetDeviceIndex((int)baseStationIndices[i]);
        }
        
    }

}