using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
