using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NostrilCollision : MonoBehaviour
{

    public NoseManager nM;
    private Bounds nC;
    public float concentration = 0f;
    public List<Collider> currentColliders = new List<Collider>();

    // Start is called before the first frame update
    void Start()
    {
        nM = this.transform.parent.GetComponent<NoseManager>();
        nC = this.GetComponent<Collider>().bounds;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (String.Compare(other.tag, "Plume") == 0)
        {
            if (!currentColliders.Contains(other)) { currentColliders.Add(other); }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (String.Compare(other.tag, "Plume") == 0)
        {
            currentColliders.Remove(other);
        }
    }

    public float CheckConcentration()
    {
        concentration = 0f;
        foreach (Collider c in currentColliders)
        {
            concentration += c.GetComponent<BoundingVolume>().Concentration(nC, transform);
        }
        return concentration;
    }

    private void OnTriggerStay(Collider other)
    {
        BoundingVolume currVolume;
        if (currVolume = other.GetComponent<BoundingVolume>())
        {
            currVolume.VolumeUpdate(nM.ansysFrame);
        }
        // concentration = currVolume.Concentration(nC, transform);
        // Debug.Log(this.name + " " + concentration.ToString());
    }
}
