using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    public Rigidbody rb;
    public float DepthBeforeSubmerged = 1;
    public float DisplacementAmount = 3;
    public int floaterCount = 4;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //add gravity at point
        rb.AddForceAtPosition(Physics.gravity / floaterCount, transform.position, ForceMode.Acceleration);
        Debug.DrawRay(transform.position, Physics.gravity / floaterCount, new Color(1, 0.5f, 0, 1));

        if(transform.position.y < 0)
        {
            float DisplacementMultiplier = Mathf.Clamp01(-transform.position.y / DepthBeforeSubmerged) * DisplacementAmount;
            rb.AddForceAtPosition(new Vector3(0, Mathf.Abs(Physics.gravity.y) * DisplacementMultiplier, 0), transform.position, ForceMode.Acceleration);
            Debug.DrawRay(transform.position, new Vector3(0, Mathf.Abs(Physics.gravity.y) * DisplacementMultiplier, 0), Color.cyan);
        }
    }


}
