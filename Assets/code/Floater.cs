using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    public Rigidbody rb;
    public float DepthBeforeSubmerged = 1;
    public float DisplacementAmount = 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float DisplacementMultiplier = Mathf.Clamp01(-transform.position.y / DepthBeforeSubmerged) * DisplacementAmount;
        rb.AddForce(new Vector3(0, Mathf.Abs(Physics.gravity.y) * DisplacementMultiplier, 0), ForceMode.Acceleration);
    }


}
