using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public GameObject[] ropePoints;

    public LineRenderer lineRenderer;

    void Start()
    {
        if (ropePoints.Length > 0)
        {
            lineRenderer.positionCount = ropePoints.Length;
            lineRenderer.useWorldSpace = true;
        }
    }

    void Update()
    {
        if(ropePoints.Length > 0)
        {
            for (int i = 0; i < ropePoints.Length; i++)
            {
                if(ropePoints[i] != null)
                {
                    ropePoints[i].GetComponent<MeshRenderer>().enabled = false;
                    lineRenderer.SetPosition(i, ropePoints[i].transform.position);
                }
            }
        }
    }
}
