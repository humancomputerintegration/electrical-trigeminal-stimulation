using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class settings : MonoBehaviour
{
    public bool WithETS = true; // with or without (baseline) ETS
    public static bool ETSMode;
    public bool Radius = false; //Radius mode or side mode
    public static bool RadiusMode;
    // Start is called before the first frame update
    void Start()
    {
        ETSMode = WithETS;
        RadiusMode = Radius;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
