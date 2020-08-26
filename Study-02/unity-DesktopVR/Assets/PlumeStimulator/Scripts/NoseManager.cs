using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoseManager : MonoBehaviour
{

    /*
     * Basic idea:
     *      - The nose manager keeps track of the frame number AND frame rate (non the Ansys Plume).
     *      - While a nostril is in a bounding volume:
     *              + check the frame discrepancy (if nose frame > bounding volume frame). If yes, update.
     *              + call bounding volume's comparison function using the Bounds 
     */

    /*
     * PLUME VARIABLES
     */
    public int ansysFrame = 0; // nextUpdate -> plumeFrame. Made public so we can see where we're at when looking at the editor.
    public int numAnsysFrames = 300;
    public int framesPerSecond = 1;
    private float secondsPerFrame;
    public int breathsPerMinute = 18;
    public float secondsPerBreath;

    public Transform latViz;

    // Start is called before the first frame update
    void Start()
    {
        secondsPerFrame = (1.0f / (float)framesPerSecond);
        secondsPerBreath = (60.0f / (float)breathsPerMinute);
        StartCoroutine(NoseAnimate());
        StartCoroutine(NoseBreathe());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator NoseAnimate()
    {
        while (ansysFrame < numAnsysFrames)
        {
            ansysFrame++;
            yield return new WaitForSeconds(secondsPerFrame);
        }
    }

    private IEnumerator NoseBreathe()
    {
        while (true)
        {
            float left = 0f;
            float right = 0f;
            foreach (Transform nostril in transform)
            {
                if (nostril.gameObject.layer == LayerMask.NameToLayer("Debug"))
                    continue;
                NostrilCollision nC = nostril.GetComponent<NostrilCollision>();
                float conc = nC.CheckConcentration();
                if (nostril.name.Contains("left"))
                {
                    left = conc;
                } else
                {
                    right = conc;
                }
            }
            if (left < right)
            {
                latViz.localPosition = new Vector3(0.0f, 0.0f, -0.2f);
            } else if (right < left)
            {
                latViz.localPosition = new Vector3(0.0f, 0.0f, 0.2f);
            } else
            {
                latViz.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            }
            yield return new WaitForSeconds(secondsPerBreath);
        }
    }
}
