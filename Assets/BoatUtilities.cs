using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatUtilities : MonoBehaviour
{
    // For Level management, resetting, event systems, stc

    public void ResetPositionToOrigin()
    {
        BoatManager bm = GetComponent<BoatManager>();
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        bm.Sail.transform.localRotation = Quaternion.identity;
        bm.Rudder.transform.localRotation = Quaternion.identity;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.rotation = Quaternion.identity;
        rb.position = Vector3.zero;
    }
}
